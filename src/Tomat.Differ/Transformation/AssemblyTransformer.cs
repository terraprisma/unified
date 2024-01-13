using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace Tomat.Differ.Transformation;

public static class AssemblyTransformer {
    public static AssemblyContext GetAssemblyContextWithUniversalAssemblyResolverFromPath(string assemblyPath) {
        var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
        var resolver = new UniversalAssemblyResolver();
        resolver.AddSearchDirectory(assemblyDir);

        var module = ModuleDefinition.ReadModule(
            assemblyPath,
            new ReaderParameters {
                AssemblyResolver = resolver,
            }
        );

        resolver.AddEmbeddedAssembliesFrom(module);

        return new AssemblyContext(module, module.Assembly, resolver, assemblyDir);
    }

    public static void TransformAssembly(AssemblyContext context, params IAssemblyTransformer[] transformers) {
        var module = context.Module;
        var assembly = context.Assembly;
        var resolver = context.Resolver;
        var pendingWrites = new Dictionary<string, AssemblyDefinition>();
        var streams = new Dictionary<string, MemoryStream>();

        var referenceDefinitions = module.AssemblyReferences.Select(x => resolver.Resolve(x))
            .ToList();

        foreach (var refDef in referenceDefinitions) {
            if (TransformAssembly(new TransformerContext(context, refDef), transformers))
                pendingWrites.Add(refDef.MainModule.FileName, refDef);
        }

        if (TransformAssembly(new TransformerContext(context, assembly), transformers))
            pendingWrites.Add(module.FileName, assembly);

        foreach (var (fileName, assemblyReference) in pendingWrites) {
            var stream = new MemoryStream();
            assemblyReference.Write(stream);

            var path = assemblyReference.MainModule.FileName;
            if (string.IsNullOrEmpty(path))
                path = assemblyReference.Name.Name + ".dll";

            streams.Add(Path.GetFileName(assemblyReference.MainModule.FileName), stream);
        }

        foreach (var refDef in referenceDefinitions)
            refDef.Dispose();

        assembly.Dispose();

        foreach (var (fileName, stream) in streams) {
            using var fs = File.OpenWrite(Path.Combine(context.AssemblyDirectory!, fileName));
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fs);
        }
    }

    private static bool TransformAssembly(TransformerContext context, params IAssemblyTransformer[] transformers) {
        var editsMade = false;

        foreach (var transformer in transformers)
            editsMade |= transformer.TransformAssembly(context);

        return editsMade;
    }
}

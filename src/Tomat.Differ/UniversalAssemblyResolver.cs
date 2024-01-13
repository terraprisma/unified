using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace Tomat.Differ;

internal sealed class UniversalAssemblyResolver : IAssemblyResolver {
    private static readonly List<string> gac_paths = GetGacPaths();
    private readonly DefaultAssemblyResolver baseResolver = new();
    private readonly List<AssemblyDefinition> embeddedAssemblies = new();

    public AssemblyDefinition Resolve(AssemblyNameReference name) {
        return Resolve(name, new ReaderParameters());
    }

    public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
        AssemblyDefinition? asm;

        try {
            asm = baseResolver.Resolve(name, parameters);
            if (asm is not null)
                return asm;
        }
        catch {
            // ignored
        }

        asm = ResolveInternal(name);
        if (asm is not null)
            return asm;

        foreach (var embeddedAssembly in embeddedAssemblies) {
            if (embeddedAssembly.Name.Name == name.Name)
                return embeddedAssembly;
        }

        throw new AssemblyResolutionException(name);
    }

    public void AddSearchDirectory(string directory) {
        baseResolver.AddSearchDirectory(directory);
    }

    public void AddEmbeddedAssembliesFrom(ModuleDefinition module) {
        foreach (var resource in module.Resources) {
            if (resource is not EmbeddedResource embeddedResource)
                continue;

            if (!embeddedResource.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                continue;

            var assembly = AssemblyDefinition.ReadAssembly(
                embeddedResource.GetResourceStream(),
                new ReaderParameters {
                    AssemblyResolver = this,
                }
            );

            embeddedAssemblies.Add(assembly);
        }
    }

    private AssemblyDefinition? ResolveInternal(AssemblyNameReference name) {
        return GetAssemblyInGac(name);
    }

    public void Dispose() {
        foreach (var reference in embeddedAssemblies)
            reference.Dispose();

        baseResolver.Dispose();
    }

    public static AssemblyDefinition? GetAssemblyInGac(AssemblyNameReference name) {
        return GetAssemblyInNetGac(name);
    }

    private static AssemblyDefinition? GetAssemblyInNetGac(AssemblyNameReference name) {
        var caches = new[] { "GAC_MSIL", "GAC_32", "GAC_64", "GAC" };
        var prefixes = new[] { string.Empty, "v4.0_" };

        foreach (var gacPath in gac_paths)
        foreach (var cache in caches)
        foreach (var prefix in prefixes) {
            var gac = Path.Combine(gacPath, cache);
            var file = GetAssemblyFile(name, prefix, gac);
            if (Directory.Exists(gac) && File.Exists(file))
                return AssemblyDefinition.ReadAssembly(file);
        }

        return null;
    }

    private static string GetAssemblyFile(AssemblyNameReference name, string prefix, string gac) {
        var gacFolder = new StringBuilder();
        gacFolder.Append(prefix);
        gacFolder.Append(name.Version);

        if (name.PublicKeyToken is { Length: > 0 } publicKey) {
            gacFolder.Append("__");
            foreach (var keyByte in publicKey)
                gacFolder.Append(keyByte.ToString("x2"));
        }

        return Path.Combine(gac, name.Name!, gacFolder.ToString(), name.Name + ".dll");
    }

    public static List<string> GetGacPaths() {
        var paths = new List<string>(2);
        var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (winDir is null)
            return paths;

        paths.Add(Path.Combine(winDir, "assembly"));
        paths.Add(Path.Combine(winDir, "Microsoft.NET", "assembly"));
        return paths;
    }
}

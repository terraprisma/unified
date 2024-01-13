using Mono.Cecil;

namespace Tomat.Differ.Transformation;

public readonly record struct AssemblyContext(ModuleDefinition Module, AssemblyDefinition Assembly, IAssemblyResolver Resolver, string AssemblyDirectory);

public readonly record struct TransformerContext(AssemblyContext AssemblyContext, AssemblyDefinition Assembly);

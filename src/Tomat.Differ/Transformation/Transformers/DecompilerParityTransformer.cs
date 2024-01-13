using System.Linq;

namespace Tomat.Differ.Transformation.Transformers;

public sealed class DecompilerParityTransformer : IAssemblyTransformer {
    bool IAssemblyTransformer.TransformAssembly(in TransformerContext context) {
        switch (context.Assembly.Name.Name) {
            case "mscorlib":
                return Mscorlib(in context);

            case "FNA":
                return Fna(in context);

            default:
                return false;
        }
    }

    private static bool Mscorlib(in TransformerContext context) {
        var asm = context.Assembly;
        var mod = asm.MainModule;
        var editsMade = false;

        if (mod.GetType("System.MathF") is { } mathF) {
            if (mathF.Fields.FirstOrDefault(x => x.Name == "PI") is { } pi)
                editsMade |= mathF.Fields.Remove(pi);
        }

        if (mod.GetType("System.String") is { } stringType) {
            if (stringType.Methods.FirstOrDefault(x => x.Name == "Split" && x.Parameters.Count == 2 && x.Parameters[1].ParameterType.Name == "StringSplitOptions") is { } split) {
                split.Parameters[1].HasDefault = false;
                split.Parameters[1].IsOptional = false;
            }
        }

        return editsMade;
    }

    private static bool Fna(in TransformerContext context) {
        var asm = context.Assembly;
        var mod = asm.MainModule;
        var editsMade = false;

        if (mod.GetType("Microsoft.Xna.Framework.Color") is { } color) {
            var constructors = color.Methods.Where(x => x.Name == ".ctor" && x.Parameters.Count == 4)
                .ToList();

            foreach (var constructor in constructors) {
                var alpha = constructor.Parameters.FirstOrDefault(x => x.Name == "alpha");

                if (alpha is not null && alpha.Name == "alpha") {
                    alpha.Name = "a";
                    editsMade = true;
                }
            }
        }

        if (mod.GetType("Microsoft.Xna.Framework.Graphics.SpriteBatch") is { } spriteBatch) {
            var method = spriteBatch.Methods.FirstOrDefault(x => x.Name == "Begin" && x.Parameters.Count == 7 && x.Parameters.Any(y => y.ParameterType.Name == "Matrix"));

            if (method is not null && method.Parameters[6].Name != "transformMatrix") {
                method.Parameters[6].Name = "transformMatrix";
                editsMade = true;
            }
        }

        return editsMade;
    }
}

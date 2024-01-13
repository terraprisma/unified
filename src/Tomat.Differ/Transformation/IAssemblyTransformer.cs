namespace Tomat.Differ.Transformation;

public interface IAssemblyTransformer {
    bool TransformAssembly(in TransformerContext context);
}

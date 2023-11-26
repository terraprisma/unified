namespace Tomat.TerrariaDiffer;

public abstract class DiffNode {
    public string WorkspaceName { get; }

    public DiffNode[] Children { get; }

    protected DiffNode(string workspaceName, params DiffNode[] children) {
        WorkspaceName = workspaceName;
        Children = children;
    }
}

public sealed class DepotDiffNode : DiffNode {
    public string DepotName { get; }

    public string RelativePathToExecutable { get; }

    public DepotDiffNode(string depotName, string workspaceName, string relativePathToExecutable, params DiffNode[] children) : base(workspaceName, children) {
        DepotName = depotName;
        RelativePathToExecutable = relativePathToExecutable;
    }
}

public sealed class ModDiffNode : DiffNode {
    public ModDiffNode(string workspaceName, params DiffNode[] children) : base(workspaceName, children) { }
}

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Tomat.Differ;

/// <summary>
///     An abstract node.
/// </summary>
public abstract class DiffNode {
    /// <summary>
    ///     The JSON representation of a node.
    /// </summary>
    public class MetaNode {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("depot")]
        public string? Depot { get; set; }

        [JsonProperty("workspace")]
        public string? Workspace { get; set; }

        [JsonProperty("path")]
        public string? Path { get; set; }

        [JsonProperty("children")]
        public List<MetaNode>? Children { get; set; }
    }

    /// <summary>
    ///     The name of the patching workspace.
    /// </summary>
    public string WorkspaceName { get; }

    /// <summary>
    ///     Dependent children nodes.
    /// </summary>
    public DiffNode[] Children { get; }

    protected DiffNode(string workspaceName, params DiffNode[] children) {
        WorkspaceName = workspaceName;
        Children = children;
    }

    public static DiffNode FromFile(string path) {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found.", path);

        var jsonText = File.ReadAllText(path);
        var meta = JsonConvert.DeserializeObject<MetaNode>(jsonText);
        if (meta is null)
            throw new InvalidDataException("Invalid JSON.");

        return FromMetaNode(meta);
    }

    public static DiffNode FromMetaNode(MetaNode meta) {
        var children = new List<DiffNode>();

        if (meta.Children is not null) {
            foreach (var child in meta.Children)
                children.Add(FromMetaNode(child));
        }

        switch (meta.Type) {
            case "depot":
                if (meta.Depot is null)
                    throw new InvalidDataException("Missing depot name.");

                if (meta.Workspace is null)
                    throw new InvalidDataException("Missing workspace name.");

                if (meta.Path is null)
                    throw new InvalidDataException("Missing path to executable.");

                return new DepotDiffNode(meta.Depot, meta.Workspace, meta.Path, children.ToArray());

            case "mod":
                if (meta.Name is null)
                    throw new InvalidDataException("Missing mod name.");

                return new ModDiffNode(meta.Name, children.ToArray());

            default:
                throw new InvalidDataException("Invalid node type.");
        }
    }
}

/// <summary>
///     A node that uses a depot-downloaded copy of Terraria.
/// </summary>
public sealed class DepotDiffNode : DiffNode {
    /// <summary>
    ///     The name of this depot.
    /// </summary>
    public string DepotName { get; }

    /// <summary>
    ///     The path to the executable, relative to the root directory of the
    ///     depot.
    /// </summary>
    public string RelativePathToExecutable { get; }

    public DepotDiffNode(string depotName, string workspaceName, string relativePathToExecutable, params DiffNode[] children) : base(workspaceName, children) {
        DepotName = depotName;
        RelativePathToExecutable = relativePathToExecutable;
    }
}

/// <summary>
///     A node that is used for a standalone mod, meaning there is not depot to
///     draw from.
/// </summary>
public sealed class ModDiffNode : DiffNode {
    public ModDiffNode(string workspaceName, params DiffNode[] children) : base(workspaceName, children) { }
}

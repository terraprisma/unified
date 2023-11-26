using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Tomat.TerrariaDiffer;

public abstract class DiffNode {
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

    public string WorkspaceName { get; }

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

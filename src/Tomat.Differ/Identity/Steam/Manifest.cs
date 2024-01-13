namespace Tomat.Differ.Identity.Steam;

/// <summary>
///     An immutable manifest identifier for a depot.
/// </summary>
public readonly struct Manifest {
    public int DepotId { get; }

    // Consistency for us...
    public string Name { get; }

    public Manifest(int depotId, string name) {
        DepotId = depotId;
        Name = name;
    }
}

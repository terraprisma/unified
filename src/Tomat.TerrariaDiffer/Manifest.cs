namespace Tomat.TerrariaDiffer;

public struct Manifest {
    public int DepotId { get; }

    // Consistency for us...
    public string Name { get; }

    public Manifest(int depotId, string name) {
        DepotId = depotId;
        Name = name;
    }
}

using Tomat.Differ.Identity.Steam;

namespace Tomat.Differ.Identity;

/// <summary>
///     Constants for known games.
/// </summary>
public static class Games {
    public static class Terraria {
        public static readonly Game GAME = new(105600);
        public static readonly Manifest RELEASE = new(105601, "TerrariaRelease");
        public static readonly Manifest LINUX = new(105602, "TerrariaLinux");
        public static readonly Manifest MAC = new(105603, "TerrariaMac");
    }
}

namespace Tomat.Differ.Identity.Steam;

/// <summary>
///     An immutable identifier for a Steam ID, providing an app ID.
/// </summary>
public readonly struct Game {
    /// <summary>
    ///     The app ID of the Steam game.
    /// </summary>
    public int AppId { get; }

    public Game(int appId) {
        AppId = appId;
    }
}

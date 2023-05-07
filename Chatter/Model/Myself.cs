namespace Chatter.Model;

/// <summary>
///     Information about the player running this plugin.
/// </summary>
public class Myself
{
    private static string? _name;
    private static string? _homeWorld;

    /// <summary>
    ///     The player character's name.
    /// </summary>
    public static string Name
    {
        get { return _name ??= Dalamud.ClientState.LocalPlayer?.Name.TextValue ?? "Who am I?"; }
    }

    /// <summary>
    ///     The player character's home world.
    /// </summary>
    public static string HomeWorld
    {
        get { return _homeWorld ??= Dalamud.ClientState.LocalPlayer?.HomeWorld.GameData?.Name ?? "Where am I?"; }
    }

    /// <summary>
    ///     Returns my full name (name plus home world).
    /// </summary>
    public static string FullName => $"{Name}@{HomeWorld}";
}
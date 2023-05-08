using Dalamud.Game.ClientState;

namespace Chatter.Model;

/// <summary>
///     Information about the player running this plugin.
/// </summary>
public class Myself
{
    private readonly ClientState _clientState;
    private string? _homeWorld;
    private string? _name;

    public Myself(ClientState clientState)
    {
        _clientState = clientState;
    }

    /// <summary>
    ///     The player character's name.
    /// </summary>
    public string Name
    {
        get { return _name ??= _clientState.LocalPlayer?.Name.TextValue ?? "Who am I?"; }
    }

    /// <summary>
    ///     The player character's home world.
    /// </summary>
    public string HomeWorld
    {
        get { return _homeWorld ??= _clientState.LocalPlayer?.HomeWorld.GameData?.Name ?? "Where am I?"; }
    }

    /// <summary>
    ///     Returns my full name (name plus home world).
    /// </summary>
    public string FullName => $"{Name}@{HomeWorld}";
}
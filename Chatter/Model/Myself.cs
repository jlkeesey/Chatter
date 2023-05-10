using Dalamud.Game.ClientState;

namespace Chatter.Model;

/// <summary>
///     Information about the player running this plugin.
/// </summary>
public class Myself : IPlayer
{
    private readonly ClientState _clientState;
    private readonly WorldManager _worldManager;

    private World? _homeWorld;
    private string? _name;

    public Myself(ClientState clientState, WorldManager worldManager)
    {
        _clientState = clientState;
        _worldManager = worldManager;
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
    public World HomeWorld
    {
        get
        {
            return _homeWorld ??= _worldManager.GetWorld(_clientState.LocalPlayer?.HomeWorld.GameData?.Name.ToString());
        }
    }

    /// <summary>
    ///     Returns my full name (name plus home world).
    /// </summary>
    public string FullName => $"{Name}@{HomeWorld.Name}";
}
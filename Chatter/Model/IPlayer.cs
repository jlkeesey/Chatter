namespace Chatter.Model;

/// <summary>
///     An abstraction for a player's basic information.
/// </summary>
public interface IPlayer
{
    /// <summary>
    ///     The player character's name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The player character's home world.
    /// </summary>
    World HomeWorld { get; }

    /// <summary>
    ///     Returns my full name (name plus home world).
    /// </summary>
    string FullName { get; }
}
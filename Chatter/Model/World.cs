namespace Chatter.Data;

/// <summary>
///     Represents an FFXIV world. FFXIV has it's own type but it's not easily consumable by C# so
///     when a world is loaded from FFXIV it is converted to this object type.
/// </summary>
public class World
{
    /// <summary>
    ///     This world's data center.
    /// </summary>
    public readonly string DataCenter;

    /// <summary>
    ///     This world's content id.
    /// </summary>
    public readonly uint Id;

    /// <summary>
    ///     This world's name.
    /// </summary>
    public readonly string Name;

    public World(uint id, string name, string dataCenter)
    {
        Id = id;
        Name = name;
        DataCenter = dataCenter;
    }
}
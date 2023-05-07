using System.Collections.Generic;
using Chatter.Data;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="World" /> objects.
/// </summary>
public class WorldManager
{
    private static readonly Dictionary<uint, World> Worlds = new();

    /// <summary>
    ///     Retrieve the <see cref="World" /> object from the given id. The data is retrieve from FFXIV if necessary and
    ///     converted to a <see cref="World" /> object.
    /// </summary>
    /// <param name="id">The id to lookup.</param>
    /// <returns>The found <see cref="World" />. This will always return an object, even if the data cannot be found.</returns>
    public static World GetWorld(uint id)
    {
        if (Worlds.TryGetValue(id, out var world)) return world;
        // TODO Use this code to rewrite DataCenter.cs
        var w = Dalamud.GameData.Excel.GetSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(id);
        world = new World(id, w?.Name.ToString() ?? "?world?", w?.DataCenter?.Value?.Name ?? "?dc?");
        Worlds.Add(id, world);
        return world;
    }
}
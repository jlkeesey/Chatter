using System.Collections.Generic;
using System.Linq;
using Dalamud.Data;
using Lumina.Excel;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="World" /> objects.
/// </summary>
public class WorldManager
{
    private readonly DataManager _gameData;
    private readonly Dictionary<uint, World> _worldById = new();
    private readonly Dictionary<string, World> _worldByName = new();

    public WorldManager(DataManager gameData)
    {
        _gameData = gameData;
    }

    public World GetWorld(string? name)
    {
        if (name == null) return World.Null;
        if (_worldByName.TryGetValue(name, out var world)) return world;
        using var worlds = _gameData.Excel.GetSheet<Lumina.Excel.GeneratedSheets.World>()?.GetEnumerator();
        if (worlds == null) return World.Null;
        var worldRow = new EnumerableWrapper<Lumina.Excel.GeneratedSheets.World>(worlds).Where(w => w.Name == name).First();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        return worldRow == null ? World.Null : RegisterWorld(worldRow);
    }

    /// <summary>
    ///     Retrieve the <see cref="World" /> object from the given id. The data is retrieve from FFXIV if necessary and
    ///     converted to a <see cref="World" /> object.
    /// </summary>
    /// <param name="id">The id to lookup.</param>
    /// <returns>The found <see cref="World" />. This will always return an object, even if the data cannot be found.</returns>
    public World GetWorld(uint id)
    {
        if (_worldById.TryGetValue(id, out var world)) return world;
        var worldRow = _gameData.Excel.GetSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(id);
        return worldRow == null ? World.Null : RegisterWorld(worldRow);
    }

    /// <summary>
    /// Puts the world into the world caches.
    /// </summary>
    /// <param name="worldRow">The world to register.</param>
    private World RegisterWorld(Lumina.Excel.GeneratedSheets.World worldRow)
    {
        var world = new World(worldRow.RowId, worldRow.Name.ToString(), worldRow.DataCenter.Value?.Name ?? World.Null.DataCenter);
        _worldById.Add(worldRow.RowId, world);
        _worldByName.Add(world.Name, world);
        return world;
    }
}
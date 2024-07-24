// Copyright 2023 James Keesey
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="World" /> objects.
/// </summary>
public class WorldManager(IDataManager gameData)
{
    private readonly Dictionary<uint, World> _worldById = new();
    private readonly Dictionary<string, World> _worldByName = new();

    public World GetWorld(string? name)
    {
        if (name == null) return World.Null;
        if (_worldByName.TryGetValue(name, out var world)) return world;
        using var worlds = gameData.Excel.GetSheet<Lumina.Excel.GeneratedSheets.World>()?.GetEnumerator();
        if (worlds == null) return World.Null;
        var worldRow = new EnumerableWrapper<Lumina.Excel.GeneratedSheets.World>(worlds).Where(w => w.Name == name)
           .First();
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
        var worldRow = gameData.Excel.GetSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(id);
        return worldRow == null ? World.Null : RegisterWorld(worldRow);
    }

    /// <summary>
    ///     Puts the world into the world caches.
    /// </summary>
    /// <param name="worldRow">The world to register.</param>
    private World RegisterWorld(Lumina.Excel.GeneratedSheets.World worldRow)
    {
        var world = new World(worldRow.RowId,
                              worldRow.Name.ToString(),
                              worldRow.DataCenter.Value?.Name ?? World.Null.DataCenter);
        _worldById.Add(worldRow.RowId, world);
        _worldByName.Add(world.Name, world);
        return world;
    }
}

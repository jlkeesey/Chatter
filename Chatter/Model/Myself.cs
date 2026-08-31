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

namespace Chatter.Model;

/// <summary>
///     Information about the player running this plugin.
/// </summary>
/// <remarks>
///     None of the player's details are available while we're not logged in (title screen, lobby, and the moments
///     around a login or logout) but chat messages still arrive, so every lookup has to cope with the data being
///     missing. We remember the last known values so messages that arrive around a logout are still attributed
///     correctly, and we re-read them whenever they are available so switching characters is picked up.
/// </remarks>
public class Myself(IPlayerState playerState, WorldManager worldManager) : IPlayer
{
    private const string UnknownName = "Who am I?";

    private World? _homeWorld;
    private string? _name;

    /// <summary>
    ///     The player character's name.
    /// </summary>
    public string Name
    {
        get
        {
            var name = playerState.CharacterName;
            if (!string.IsNullOrEmpty(name)) _name = name;
            return _name ?? UnknownName;
        }
    }

    /// <summary>
    ///     The player character's home world.
    /// </summary>
    public World HomeWorld
    {
        get
        {
            // When we're not logged in this is a default RowRef (row id 0, no sheet) and reading Value throws.
            var homeWorld = playerState.HomeWorld;
            if (homeWorld.RowId != 0 && homeWorld.IsValid && _homeWorld?.Id != homeWorld.RowId)
                _homeWorld = worldManager.GetWorld(homeWorld.RowId);
            return _homeWorld ?? World.Null;
        }
    }

    /// <summary>
    ///     Returns my full name (name plus home world).
    /// </summary>
    public string FullName => $"{Name}@{HomeWorld.Name}";
}

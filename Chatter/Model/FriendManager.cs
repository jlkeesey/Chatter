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

using System;
using System.Collections.Generic;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="Friend" /> objects.
/// </summary>
public class FriendManager
{
    private readonly WorldManager _worldManager;

    public FriendManager(WorldManager worldManager)
    {
        _worldManager = worldManager;
    }

    /// <summary>
    ///     Returns a list of all of the current player's friends.
    /// </summary>
    /// <returns>A list of <see cref="Friend" /> objects.</returns>
    public unsafe IEnumerable<Friend> GetFriends()
    {
        List<Friend> friends = new();

        var agent = AgentFriendList.Instance();
        if (agent == null) return friends;

        var length = agent->Count;
        if (length == 0) return friends;

        for (var i = 0u; i < length; i++)
        {
            var entry = agent->GetFriend(i);
            var homeWorld = _worldManager.GetWorld(entry->HomeWorld);
            var currentWorld = _worldManager.GetWorld(entry->CurrentWorld);
            var name = MemoryHelper.ReadSeStringNullTerminated((IntPtr) entry->Name).TextValue;
            var freeCompany = MemoryHelper.ReadSeStringNullTerminated((IntPtr) entry->FCTag).TextValue;
            var isOnline = (entry->DutyStatus & 0x80) != 0;

            friends.Add(new Friend(entry->ContentId, name, freeCompany, homeWorld, currentWorld, isOnline));
        }

        return friends;
    }
}

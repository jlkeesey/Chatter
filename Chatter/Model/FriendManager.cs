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

using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="Friend" /> objects.
/// </summary>
public class FriendManager(WorldManager worldManager)
{
    /// <summary>
    ///     Returns a list of all the current player's friends.
    /// </summary>
    /// <returns>A list of <see cref="Friend" /> objects.</returns>
    public unsafe IEnumerable<Friend> GetFriends()
    {
        List<Friend> friends = new();

        var infoProxy = InfoProxyFriendList.Instance();
        if (infoProxy == null) return friends;
        var length = infoProxy->EntryCount;
        if (length == 0) return friends;

        // var agent = AgentFriendList.Instance();
        // if (agent == null) return friends;

        // var length = agent->Count;

        for (var i = 0u; i < length; i++)
        {
            var entry = infoProxy->GetEntry(i);
            var homeWorld = worldManager.GetWorld(entry->HomeWorld);
            var currentWorld = worldManager.GetWorld(entry->CurrentWorld);
            var name = SeString.Parse(entry->Name).TextValue;
            var freeCompany = SeString.Parse(entry->FCTag).TextValue;
            var isOnline = (entry->State & InfoProxyCommonList.CharacterData.OnlineStatus.Online) != 0;

            friends.Add(new Friend(entry->ContentId, name, freeCompany, homeWorld, currentWorld, isOnline));
        }

        return friends;
    }
}

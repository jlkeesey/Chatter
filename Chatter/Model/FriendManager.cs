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
using System.Runtime.InteropServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Chatter.Model;

/// <summary>
///     Utilities for manipulating <see cref="Friend" /> objects.
/// </summary>
public class FriendManager
{
    private const int InfoOffset = 0x28;
    private const int LengthOffset = 0x10;
    private const int ListOffset = 0x98;

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

        var socialFriendAgent =
            (IntPtr)
            Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.SocialFriendList);
        if (socialFriendAgent == IntPtr.Zero) return friends;

        var info = *(IntPtr*) (socialFriendAgent + InfoOffset);
        if (info == IntPtr.Zero) return friends;

        var length = *(ushort*) (info + LengthOffset);
        if (length == 0) return friends;

        var list = *(IntPtr*) (info + ListOffset);
        if (list == IntPtr.Zero) return friends;

        for (var i = 0; i < length; i++)
        {
            var entry = (FriendEntry*) (list + i * FriendEntry.Size);
            var homeWorld = _worldManager.GetWorld(entry->HomeWorld);
            var currentWorld = _worldManager.GetWorld(entry->CurrentWorld);
            friends.Add(new Friend(entry->ContentId,
                                   entry->Name,
                                   entry->FreeCompany,
                                   homeWorld,
                                   currentWorld,
                                   entry->IsOnline));
        }

        return friends;
    }

    /// <summary>
    ///     Helper struct to extract information from the FFXIV friend object.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    private unsafe struct FriendEntry
    {
        internal const int Size = 96;

        /// <summary>
        ///     The friend's content id.
        /// </summary>
        [FieldOffset(0)] public readonly ulong ContentId;

        /// <summary>
        ///     The friend's online status;
        /// </summary>
        [FieldOffset(13)] private readonly byte OnlineStatus;

        /// <summary>
        ///     The world the friend is currently on.
        /// </summary>
        [FieldOffset(22)] public readonly ushort CurrentWorld;

        /// <summary>
        ///     The friend's home world.
        /// </summary>
        [FieldOffset(24)] public readonly ushort HomeWorld;

        // /// <summary>
        // ///     The friend's current job.
        // /// </summary>
        // [FieldOffset(33)] public readonly byte Job;

        /// <summary>
        ///     The friend's raw SeString name.
        /// </summary>
        [FieldOffset(34)] private fixed byte RawName[32];

        /// <summary>
        ///     The friend's raw SeString free company tag.
        /// </summary>
        [FieldOffset(66)] private fixed byte RawFreeCompany[5];

        /// <summary>
        ///     The friend's name.
        /// </summary>
        public string Name
        {
            get
            {
                fixed (byte* ptr = RawName)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr) ptr).TextValue;
                }
            }
        }

        /// <summary>
        ///     The friend's free company tag.
        /// </summary>
        public string FreeCompany
        {
            get
            {
                fixed (byte* ptr = RawFreeCompany)
                {
                    return MemoryHelper.ReadSeStringNullTerminated((IntPtr) ptr).TextValue;
                }
            }
        }

        public bool IsOnline => OnlineStatus == 0x80;
    }
}

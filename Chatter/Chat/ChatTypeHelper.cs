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
using Dalamud.Game.Text;
using static System.String;

namespace Chatter.Chat;

/// <summary>
///     Utilities for working with the <see cref="XivChatType" /> enum.
/// </summary>
public sealed class ChatTypeHelper
{
    private readonly Dictionary<XivChatType, string> _chatCodeToShortName = new()
    {
        {XivChatType.TellOutgoing, "tellOut"}, {XivChatType.CustomEmote, "emote"},
    };

    /// <summary>
    ///     Converts the <see cref="XivChatType" /> to a string. We use the internal dictionary
    ///     <see cref="_chatCodeToShortName" /> first so we can override the defaults, or get if
    ///     the enum if not.
    /// </summary>
    /// <param name="chatType">The chat type to examine.</param>
    /// <param name="showUnknown"></param>
    /// <returns>The corresponding name or a name in the form '?45?' if none found.</returns>
    public string TypeToName(XivChatType chatType, bool showUnknown = false)
    {
        if (_chatCodeToShortName.TryGetValue(chatType, out var name)) return name;
        var slug = chatType.GetDetails()?.Slug ?? Empty;
        var defaultValue = showUnknown ? $"?{(int) chatType}?" : Empty;
        return !IsNullOrWhiteSpace(slug) ? slug : defaultValue;
    }
}

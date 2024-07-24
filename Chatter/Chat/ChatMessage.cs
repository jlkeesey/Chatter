﻿// Copyright 2023 James Keesey
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

using Dalamud.Game.Text;
using NodaTime;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Chatter.Chat;

/// <summary>
///     Represents a single chat message received.
/// </summary>
[PublicAPI]
public sealed class ChatMessage(
    XivChatType xivType,
    string typeLabel,
    int senderId,
    ChatString sender,
    ChatString body,
    ZonedDateTime when)
{
    public XivChatType ChatType { get; } = xivType;
    public int SenderId { get; } = senderId;
    public ChatString Sender { get; } = sender;
    public ChatString Body { get; } = body;
    public ZonedDateTime When { get; } = when;

    /// <summary>
    ///     Returns the string label for the chat type.
    /// </summary>
    public string TypeLabel { get; } = typeLabel;

    /// <summary>
    ///     Replaces the sender if there is a replacement defined.
    /// </summary>
    /// <param name="includeServer">
    ///     <c>true</c> if the server should be included in the user's name. If there is a replacement,
    ///     then this is ignored.
    /// </param>
    /// <param name="replacements">A dictionary of user full name to replacement text.</param>
    /// <returns>The sender to use in the log.</returns>
    public string GetLoggableSender(bool includeServer, IReadOnlyDictionary<string, string> replacements)
    {
        var cleanedSender = Sender.AsText(includeServer);
        var result = replacements.GetValueOrDefault(includeServer.ToString(), cleanedSender);
        if (string.IsNullOrWhiteSpace(result)) result = cleanedSender;
        return result;
    }

    public string GetLoggableBody(bool includeServer)
    {
        return Body.AsText(includeServer);
    }

    public override string ToString()
    {
        return $"{When}: '{Sender}' said '{Body}'";
    }
}

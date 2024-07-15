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

using Chatter.Chat;
using Chatter.Model;
using Chatter.UnitTests.Support;
using Dalamud.Game.Text;
using NodaTime;

namespace Chatter.UnitTests.Chat;

/// <summary>
///     Utilities for handling <see cref="ChatMessage" /> objects in a test environment.
/// </summary>
public static class ChatMessageUtils
{
    private static readonly ChatTypeHelper ChatTypeHelper = new();
    private static readonly IPlayer DefaultPlayer = new PlayerFake("Robert Jones");

    /// <summary>
    ///     Creates a test <see cref="ChatMessage" />. All values except for the timestamp have reasonable default but each
    ///     can be overridden as necessary.
    /// </summary>
    /// <param name="when">When the message occurred.</param>
    /// <param name="chatType">The <see cref="XivChatType" /> of this message.</param>
    /// <param name="typeLabel">
    ///     The string label to use for the chat type--this defaults to what
    ///     <see cref="ChatTypeHelper.TypeToName" /> returns.
    /// </param>
    /// <param name="senderId">The sender's unique numeric id.</param>
    /// <param name="sender">The <see cref="ChatString" /> containing the player's full name.</param>
    /// <param name="body">The <see cref="ChatString" /> containing the message body, including any player references.</param>
    /// <returns>The <see cref="ChatMessage" /> with the given values.</returns>
    public static ChatMessage CreateMessage(ZonedDateTime when,
                                            XivChatType chatType = XivChatType.Say,
                                            string? typeLabel = null,
                                            int senderId = 123,
                                            IPlayer? sender = null,
                                            string body = "This is a test")
    {
        var chatSender = ChatString.FromPlayer(sender ?? DefaultPlayer);
        var chatBody = ChatString.FromText(body);
        return new ChatMessage(chatType,
                               typeLabel ?? ChatTypeHelper.TypeToName(chatType),
                               senderId,
                               chatSender,
                               chatBody,
                               when);
    }
}

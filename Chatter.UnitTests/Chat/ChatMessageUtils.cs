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
    public static ChatMessage CreateMessage(ZonedDateTime when, XivChatType chatType = XivChatType.Say,
        string? typeLabel = null, uint senderId = 123, IPlayer? sender = null, string body = "This is a test")
    {
        var chatSender = ChatString.FromPlayer(sender ?? DefaultPlayer);
        var chatBody = ChatString.FromText(body);
        return new ChatMessage(chatType, typeLabel ?? ChatTypeHelper.TypeToName(chatType), senderId, chatSender,
            chatBody, when);
    }
}
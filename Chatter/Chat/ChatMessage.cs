using System.Collections.Generic;
using Dalamud.Game.Text;
using NodaTime;

namespace Chatter.Chat;

/// <summary>
///     Represents a single chat message received.
/// </summary>
public sealed class ChatMessage
{
    public ChatMessage(XivChatType xivType, string typeLabel, uint senderId, ChatString sender, ChatString body,
        ZonedDateTime when)
    {
        ChatType = xivType;
        TypeLabel = typeLabel;
        SenderId = senderId;
        Sender = sender;
        Body = body;
        When = when;
    }

    public XivChatType ChatType { get; }
    public uint SenderId { get; }
    public ChatString Sender { get; }
    public ChatString Body { get; }
    public ZonedDateTime When { get; }

    /// <summary>
    ///     Returns the string label for the chat type.
    /// </summary>
    public string TypeLabel { get; }


    /// <summary>
    ///     Replaces the sender if there is a replacement defined.
    /// </summary>
    /// <param name="includeServer">
    ///     <c>true</c> if the server should be included in the user's name. If there is a replacement,
    ///     then this is ignored.
    /// </param>
    /// <param name="replacements">A dictionary of user full name to replacement text.</param>
    /// <returns>The sender to use in the log.</returns>
    public string GetLoggableSender( bool includeServer, IReadOnlyDictionary<string, string> replacements)
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
}
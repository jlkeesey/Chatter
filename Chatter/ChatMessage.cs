using Dalamud.Game.Text;
using NodaTime;

namespace Chatter;

/// <summary>
///     Represents a single chat message received.
/// </summary>
public class ChatMessage
{
    public ChatMessage(XivChatType xivType, string typeLabel, uint senderId, ChatString sender, ChatString body, ZonedDateTime when)
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
}
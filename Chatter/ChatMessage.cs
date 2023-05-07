using System;
using Dalamud.Game.Text;

namespace Chatter;

/// <summary>
///     Represents a single chat message received.
/// </summary>
public class ChatMessage
{
    private string? _typeLabel;

    public ChatMessage(XivChatType xivType, uint senderId, ChatString sender, ChatString body, DateTime? when = null)
    {
        ChatType = xivType;
        SenderId = senderId;
        Sender = sender;
        Body = body;
        When = when ?? DateTime.Now;
    }

    public XivChatType ChatType { get; }
    public uint SenderId { get; }
    public ChatString Sender { get; }
    public ChatString Body { get; }
    public DateTime When { get; }

    /// <summary>
    ///     Returns the string label for the chat type.
    /// </summary>
    public string TypeLabel
    {
        get { return _typeLabel ??= ChatTypeHelper.TypeToName(ChatType); }
    }
}
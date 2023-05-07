using System.Collections.Generic;
using Dalamud.Game.Text;
using Dalamud.Utility;

namespace Chatter;

/// <summary>
///     Utilities for working with the <see cref="XivChatType" /> enum.
/// </summary>
internal static class ChatTypeHelper
{
    private static readonly Dictionary<XivChatType, string> ChatCodeToShortName = new()
    {
        {XivChatType.TellOutgoing, "tellOut"},
        {XivChatType.CustomEmote, "emote"},
    };

    /// <summary>
    ///     Converts the <see cref="XivChatType" /> to a string. We use the internal dictionary
    ///     <see cref="ChatCodeToShortName" /> first so we can override the defaults, or get if
    ///     the enum if not.
    /// </summary>
    /// <param name="chatType">The chat type to examine.</param>
    /// <returns>The corresponding name or a name in the form '?45?' if none found.</returns>
    public static string TypeToName(XivChatType chatType)
    {
        if (ChatCodeToShortName.TryGetValue(chatType, out var name)) return name;
        var slug = chatType.GetDetails()?.Slug ?? string.Empty;
        var defaultValue = global::Chatter.Chatter.Configuration.IsDebug ? $"?{(int) chatType}?" : string.Empty;
        return !slug.IsNullOrWhitespace() ? slug : defaultValue;
    }
}
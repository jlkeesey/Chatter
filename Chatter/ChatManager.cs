using System;
using System.Collections.Generic;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;

namespace Chatter;

/// <summary>
///     Handles capturing chat messages and passing them on the to chat log manager for processing.
/// </summary>
public sealed class ChatManager : IDisposable
{
    /// <summary>
    ///     Lists of all of the chat types that we support. Not all of these are currently exposed to the user.
    /// </summary>
    private static readonly List<XivChatType> AllSupportedChatTypes = new()
    {
        XivChatType.Alliance,
        XivChatType.CrossLinkShell1,
        XivChatType.CrossLinkShell2,
        XivChatType.CrossLinkShell3,
        XivChatType.CrossLinkShell4,
        XivChatType.CrossLinkShell5,
        XivChatType.CrossLinkShell6,
        XivChatType.CrossLinkShell7,
        XivChatType.CrossLinkShell8,
        XivChatType.CrossParty,
        XivChatType.CustomEmote,
        XivChatType.Echo,
        XivChatType.FreeCompany,
        XivChatType.Ls1,
        XivChatType.Ls2,
        XivChatType.Ls3,
        XivChatType.Ls4,
        XivChatType.Ls5,
        XivChatType.Ls6,
        XivChatType.Ls7,
        XivChatType.Ls8,
        XivChatType.Notice,
        XivChatType.NoviceNetwork,
        XivChatType.Party,
        XivChatType.PvPTeam,
        XivChatType.Say,
        XivChatType.Shout,
        XivChatType.StandardEmote,
        XivChatType.SystemError,
        XivChatType.SystemMessage,
        XivChatType.TellIncoming,
        XivChatType.TellOutgoing,
        XivChatType.Urgent,
        XivChatType.Yell,
    };

    private readonly Configuration _configuration;

    private readonly ChatLogManager _logManager;

    public ChatManager(Configuration configuration, ChatLogManager logManager)
    {
        _configuration = configuration;
        _logManager = logManager;
        Dalamud.Chat.ChatMessage += HandleChatMessage;
    }

    public void Dispose()
    {
        Dalamud.Chat.ChatMessage -= HandleChatMessage;
    }

    /// <summary>
    ///     Chat message handler. This is called for every chat message that passes through the system.
    /// </summary>
    /// <param name="xivType">The chat type.</param>
    /// <param name="senderId">The id of the sender.</param>
    /// <param name="seSender">
    ///     The name of the sender. The will include the world name if the world is different from the user,
    ///     but the world will not be separated from the user name.
    /// </param>
    /// <param name="seBody">
    ///     The chat message text. User names will include the world name is the world is different from the user,
    ///     but the world will not be separated from the user name.
    /// </param>
    /// <param name="isHandled">
    ///     Can be set to <c>true</c> to indicate that this handle handled the message and it should not be
    ///     passed on.
    /// </param>
    private void HandleChatMessage(
        XivChatType xivType, uint senderId, ref SeString seSender, ref SeString seBody, ref bool isHandled)
    {
        if (!AllSupportedChatTypes.Contains(xivType))
        {
            if (_configuration.IsDebug) PluginLog.Debug($"Unsupported XivChatType: {xivType}");
            return;
        }

        var body = CleanUpBody(seBody);
        var sender = CleanUpSender(seSender, body);
        var cm = new ChatMessage(xivType, senderId, sender, body);
        _logManager.LogInfo(cm);
    }

    /// <summary>
    ///     Cleans up the chat message. The world names are separated from the user names by an at sign (@).
    /// </summary>
    /// <param name="seBody">The body text to clean.</param>
    /// <returns>The cleaned message.</returns>
    private static ChatString CleanUpBody(SeString seBody)
    {
        return new ChatString(seBody);
    }


    /// <summary>
    ///     Cleans up the sender name. This removed any non-name characters and separated the world name from the user name by
    ///     an at sign (@).
    /// </summary>
    /// <remarks>
    ///     From the FFXIV help pages: names are no more than 20 characters long, have 2 parts (first and last name), each
    ///     part's length is between 2 and 15 characters long. So we can use this information to help correct the world issue
    ///     but reduce the number of false adjustments. If we try to remove the world name and the remaining name does not meet
    ///     the requirements, we know that the world name is actually part of the user's name.
    /// </remarks>
    /// <param name="seSender">The sender name.</param>
    /// <param name="message"></param>
    /// <returns>The cleaned sender name.</returns>
    private static ChatString CleanUpSender(SeString seSender, ChatString message)
    {
        var chatString = new ChatString(seSender);
        if (!chatString.HasInitialPlayer() && message.HasInitialPlayer())
            chatString = new ChatString(message.GetInitialPlayerItem(chatString.ToString(), null));

        return chatString;
    }
}
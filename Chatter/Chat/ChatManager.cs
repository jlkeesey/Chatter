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

using Chatter.Model;
using Chatter.System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

namespace Chatter.Chat;

/// <summary>
///     Handles capturing chat messages and passing them on the to chat log manager for processing.
/// </summary>
internal sealed class ChatManager : IDisposable
{
    /// <summary>
    ///     Lists of all the chat types that we support. Not all of these are currently exposed to the user.
    /// </summary>
    private static readonly List<XivChatType> AllSupportedChatTypes =
    [
        XivChatType.Alliance, XivChatType.CrossLinkShell1, XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3,
        XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6,
        XivChatType.CrossLinkShell7, XivChatType.CrossLinkShell8, XivChatType.CrossParty, XivChatType.CustomEmote,
        XivChatType.Echo, XivChatType.FreeCompany, XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3,
        XivChatType.Ls4, XivChatType.Ls5, XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.Notice,
        XivChatType.NoviceNetwork, XivChatType.Party, XivChatType.PvPTeam, XivChatType.Say, XivChatType.Shout,
        XivChatType.StandardEmote, XivChatType.SystemError, XivChatType.SystemMessage, XivChatType.TellIncoming,
        XivChatType.TellOutgoing, XivChatType.Urgent, XivChatType.Yell,
    ];

    /// <summary>
    ///     All the other chat types that we have examined and determined we should ignore.
    /// </summary>
    private static readonly List<XivChatType> IgnoredChatTypes =
    [
        (XivChatType) 72,   // Of the X parties currently recruiting, all match your search conditions.
        (XivChatType) 2091, // You use Teleport.
        (XivChatType) 2092, // You use a venture coffer.
        (XivChatType) 2105, // You spent X gil.
        (XivChatType) 2110, // You obtain XXX [from venture]
        (XivChatType) 2219, // You ready Teleport.
        (XivChatType) 2220, // You ready a venture coffer.
        (XivChatType) 2622, // You obtain 2 pots of general-purpose pastel blue dye.
        (XivChatType) 3129, // You pay XXX 2 ventures.
        (XivChatType) 8236, // Bob Smith uses an apricot.
        (XivChatType) 8750, // Bob Smith gains the effect of Well Fed.
        XivChatType.RetainerSale,
    ];

    private static readonly List<XivChatType> AlertedChatTypes = [];

    private readonly IChatGui _chatGui;
    private readonly ChatTypeHelper _chatTypeHelper = new();

    private readonly Configuration _configuration;
    private readonly IDateHelper _dateHelper;
    private readonly Myself _myself;
    private readonly ILogger _logger;
    private readonly ChatLogManager _logManager;

    /// <summary>
    ///     Manages connecting to the chat stream and converting them into a form for easier processing.
    /// </summary>
    /// <param name="configuration">The plugin configuration.</param>
    /// <param name="logger">Where to send log messages.</param>
    /// <param name="logManager">The manager that processes the formalized chat messages.</param>
    /// <param name="chatGui">The interface into the chat stream.</param>
    /// <param name="dateHelper">The manager of date/time objects.</param>
    /// <param name="myself">The current user.</param>
    public ChatManager(Configuration configuration,
                       ILogger logger,
                       ChatLogManager logManager,
                       IChatGui chatGui,
                       IDateHelper dateHelper,
                       Myself myself)
    {
        _configuration = configuration;
        _logger = logger;
        _logManager = logManager;
        _chatGui = chatGui;
        _dateHelper = dateHelper;
        _myself = myself;

        _chatGui.ChatMessage += HandleChatMessage;
    }

    public void Dispose()
    {
        _chatGui.ChatMessage -= HandleChatMessage;
    }

    /// <summary>
    ///     Chat message handler. This is called for every chat message that passes through the system.
    /// </summary>
    /// <param name="xivType">The chat type.</param>
    /// <param name="senderId">The id of the sender.</param>
    /// <param name="seSender">
    ///     The name of the sender. This will include the world name if the world is different from the user,
    ///     but the world will not be separated from the username.
    /// </param>
    /// <param name="seBody">
    ///     The chat message text. Usernames will include the world name is the world is different from the user,
    ///     but the world will not be separated from the username.
    /// </param>
    /// <param name="isHandled">
    ///     Can be set to <c>true</c> to indicate that this handle handled the message and it should not be
    ///     passed on.
    /// </param>
    private void HandleChatMessage(XivChatType xivType,
                                   int senderId,
                                   ref SeString seSender,
                                   ref SeString seBody,
                                   ref bool isHandled)
    {
        if (IgnoredChatTypes.Contains(xivType)) return;
        if (!AllSupportedChatTypes.Contains(xivType))
        {
            if (!AlertedChatTypes.Contains(xivType))
            {
                AlertedChatTypes.Add(xivType);
                if (_configuration.IsDebug) _logger.Debug($"Unsupported XivChatType: {xivType}: '{seBody.TextValue}'");
                return;
            }
        }

        var body = CleanUpBody(seBody);
        var sender = CleanUpSender(seSender, body);
        var chatTypeLabel = _chatTypeHelper.TypeToName(xivType, _configuration.IsDebug);
        var cm = new ChatMessage(xivType, chatTypeLabel, senderId, sender, body, _dateHelper.ZonedNow);
        _logManager.LogInfo(cm);
    }

    /// <summary>
    ///     Cleans up the chat message. The world names are separated from the usernames by an at sign (@).
    /// </summary>
    /// <param name="seBody">The body text to clean.</param>
    /// <returns>The cleaned message.</returns>
    private static ChatString CleanUpBody(SeString seBody)
    {
        return new ChatString(seBody);
    }

    /// <summary>
    ///     Cleans up the sender name. This removed any non-name characters and separated the world name from the username by
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
    private ChatString CleanUpSender(SeString seSender, ChatString message)
    {
        var chatString = new ChatString(seSender);
        if (!chatString.HasInitialPlayer() && message.HasInitialPlayer())
            chatString = new ChatString(message.GetInitialPlayerItem(chatString.ToString(), _myself.HomeWorld.Name));

        return chatString;
    }
}

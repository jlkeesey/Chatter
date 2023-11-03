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
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS �AS IS�
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
using System;
using System.Collections.Generic;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Manages a set of chat logs.
/// </summary>
/// <remarks>
///     <para>
///         A set of logs is created based on the current configuration. If that configuration changes, then we close the
///         chat logs and open new ones that reflect the new settings. There is one main or <c>all</c> log that contains
///         all messages and one log for each group set up in the configuration.
///     </para>
/// </remarks>
/// s
public sealed class ChatLogManager : IDisposable
{
    private readonly IChatLogGenerator _chatLogGenerator;
    private readonly Configuration _configuration;
    private readonly IDateHelper _dateHelper;
    private readonly FileHelper _fileHelper;
    private readonly LogFileInfo _logFileInfo = new();
    private readonly Dictionary<string, IChatLog> _logs = new();
    private readonly IPlayer _myself;

    public ChatLogManager(Configuration configuration,
                          IDateHelper dateHelper,
                          FileHelper fileHelper,
                          IPlayer myself,
                          IChatLogGenerator chatLogGenerator)
    {
        _configuration = configuration;
        _chatLogGenerator = chatLogGenerator;
        _dateHelper = dateHelper;
        _fileHelper = fileHelper;
        _myself = myself;

        //_logFileInfo.StartTime = dateHelper.ZonedNow;
    }

    public void Dispose()
    {
        CloseLogs();
    }

    /// <summary>
    ///     Returns the <see cref="ChatLog" /> for the given configuration. If one does not exist then a new one is created.
    /// </summary>
    /// <param name="logConfiguration">The configuration to use for this log.</param>
    /// <returns>The <see cref="ChatLog" /></returns>
    private IChatLog GetLog(ChatLogConfiguration logConfiguration)
    {
        if (!_logs.ContainsKey(logConfiguration.Name))
            _logs[logConfiguration.Name] =
                _chatLogGenerator.Create(logConfiguration, _logFileInfo, _dateHelper, _fileHelper, _myself);

        return _logs[logConfiguration.Name];
    }

    /// <summary>
    ///     Dumps the currently open configuration files to the dev log.
    /// </summary>
    /// <param name="logger">Where to send the information.</param>
    public void DumpLogs(ILogger logger)
    {
        logger.Log("Prefix        Open   Path");
        logger.Log("------------  -----  ----");
        foreach (var (_, entry) in _logs) entry.DumpLog(logger);
    }

    /// <summary>
    ///     Closes all of the open log files.
    /// </summary>
    private void CloseLogs()
    {
        foreach (var (_, entry) in _logs) entry.Close();
        _logFileInfo.StartTime = null;
    }

    /// <summary>
    ///     Sends the chat info to all of the currently defined logs. Each log decides what to do with the information.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    public void LogInfo(ChatMessage chatMessage)
    {
        _logFileInfo.StartTime ??= _dateHelper.ZonedNow; // If this is the first log message, set the start time.
        if (_logFileInfo.UpdateConfigValues(_configuration)) CloseLogs();
        var now = _dateHelper.ZonedNow.LocalDateTime;
        if (_logFileInfo.CloseCutoff < now) CloseLogs();
        foreach (var (_, configurationChatLog) in _configuration.ChatLogs)
            GetLog(configurationChatLog).LogInfo(chatMessage);
    }
}

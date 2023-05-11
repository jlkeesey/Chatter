using System;
using System.Collections.Generic;
using Chatter.Model;
using Chatter.System;
using NodaTime;
using NodaTime.Text;
using static NodaTime.Text.LocalTimePattern;
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
/// </remarks>s
public sealed class ChatLogManager : IDisposable
{
    private static readonly LocalTimePattern TimePattern = CreateWithInvariantCulture("H:mm");

    private readonly Configuration _configuration;
    private readonly IDateHelper _dateHelper;
    private readonly FileHelper _fileHelper;
    private readonly Dictionary<string, ChatLog> _logs = new();
    private readonly IPlayer _myself;
    private readonly LocalTime _whenToCloseLogs;
    private LogFileInfo _logFileInfo = new();

    public ChatLogManager(Configuration configuration, IDateHelper dateHelper, FileHelper fileHelper, IPlayer myself)
    {
        _configuration = configuration;
        _dateHelper = dateHelper;
        _fileHelper = fileHelper;
        _myself = myself;

        _logFileInfo.StartTime = dateHelper.ZonedNow;

        var result = TimePattern.Parse(configuration.WhenToCloseLogs);
        _whenToCloseLogs = result.Success ? result.Value : new LocalTime(6, 0);
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
    private ChatLog GetLog(ChatLogConfiguration logConfiguration)
    {
        UpdateConfigValues();
        if (!_logs.ContainsKey(logConfiguration.Name))
            _logs[logConfiguration.Name] = logConfiguration.Name == AllLogName
                ? new AllChatLog(logConfiguration, _logFileInfo, _dateHelper, _fileHelper)
                : new GroupChatLog(logConfiguration, _logFileInfo, _dateHelper, _fileHelper, _myself);

        return _logs[logConfiguration.Name];
    }

    /// <summary>
    ///     Returns the chat log with the given name.
    /// </summary>
    /// <param name="name">The name of the log.</param>
    /// <returns>The <see cref="IChatLog" /> or <c>null</c>.</returns>
    public IChatLog? GetLog(string name)
    {
        return _logs.TryGetValue(name, out var chatLog) ? chatLog : null;
    }

    /// <summary>
    ///     Checks if any configuration values have changed that warrant the closing and possible reopening of the
    ///     currently open log files.
    /// </summary>
    private void UpdateConfigValues()
    {
        if (_configuration.LogDirectory == _logFileInfo.Directory &&
            _configuration.LogFileNamePrefix == _logFileInfo.FileNamePrefix &&
            _configuration.LogOrder == _logFileInfo.Order) return;
        CloseLogs();
        _logFileInfo.Directory = _configuration.LogDirectory;
        _logFileInfo.FileNamePrefix = _configuration.LogFileNamePrefix;
        _logFileInfo.Order = _configuration.LogOrder;
    }

    /// <summary>
    ///     Dumps the currently open configuration files to the dev log.
    /// </summary>
    /// <param name="logger">Where to send the information.</param>
    public void DumpLogs(ILogger logger)
    {
        logger.Log("Prefix        Open   Path");
        logger.Log("------------  -----  ----");
        foreach (var (_, entry) in _logs)
            entry.DumpLog(logger);
    }

    /// <summary>
    ///     Closes all of the open log files.
    /// </summary>
    private void CloseLogs()
    {
        foreach (var (_, entry) in _logs)
            entry.Close();
        _logFileInfo.StartTime = null;
    }

    /// <summary>
    ///     Sends the chat info to all of the currently defined logs. Each log decides what to do with the information.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    public void LogInfo(ChatMessage chatMessage)
    {
        var now = _dateHelper.ZonedNow.TimeOfDay;

        if (_whenToCloseLogs < now) CloseLogs();

        foreach (var (_, configurationChatLog) in _configuration.ChatLogs)
            GetLog(configurationChatLog).LogInfo(chatMessage);
    }
}
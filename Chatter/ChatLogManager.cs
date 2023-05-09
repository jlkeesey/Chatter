using System;
using System.Collections.Generic;
using System.IO;
using Chatter.Model;
using Chatter.System;
using Dalamud.Logging;
using Dalamud.Utility;
using NodaTime;
using NodaTime.Text;
using static NodaTime.Text.LocalTimePattern;

namespace Chatter;

/// <summary>
///     Manages a set of logs.
/// </summary>
/// <remarks>
///     <para>
///         A set of logs is created based on the current configuration. If that configuration changes, then we
///         create a new set of logs that reflect the new settings. There is one main or all log that contains all messages
///         and one log for each group set up in the configuration.
///     </para>
/// </remarks>
internal sealed class ChatLogManager : IDisposable
{
    private static readonly LocalTimePattern TimePattern = CreateWithInvariantCulture("H:mm");

    private readonly Configuration _configuration;
    private readonly IDateHelper _dateHelper;
    private readonly Dictionary<string, ChatLog> _logs = new();
    private readonly FileHelper _fileHelper;
    private readonly LocalTime _whenToCloseLogs;
    private readonly Myself _myself;
    private readonly ZonedDateTimePattern _fileNamePattern;

    private string _logDirectory = string.Empty;
    private string _logFileNamePrefix = string.Empty;
    private Configuration.FileNameOrder _logOrder = Configuration.FileNameOrder.None;
    private ZonedDateTime? _logStartTime;

    public ChatLogManager(Configuration configuration, IDateHelper dateHelper, FileHelper fileHelper, Myself myself)
    {
        _configuration = configuration;
        _dateHelper = dateHelper;
        _fileHelper = fileHelper;
        _myself = myself;
        _fileNamePattern = ZonedDateTimePattern.CreateWithCurrentCulture("yyyyMMdd-HHmmss", null);
        _logStartTime = dateHelper.ZonedNow;

        var result = TimePattern.Parse(configuration.WhenToCloseLogs);
        //_whenToCloseLogs = result.Success ? result.Value : new LocalTime(6, 0);
        _whenToCloseLogs = result.Success ? result.Value : new LocalTime(6, 45);
    }

    private string FileNameLogStartTime
    {
        get
        {
            _logStartTime ??= _dateHelper.ZonedNow;
            return _fileNamePattern.Format((ZonedDateTime) _logStartTime);
        }
    }

    public void Dispose()
    {
        CloseLogs();
    }

    /// <summary>
    ///     Returns the <see cref="ChatLog" /> for the given configuration. If one does not exist then a new one is created.
    /// </summary>
    /// <param name="cfg">The configuration to use for this log.</param>
    /// <returns>The <see cref="ChatLog" /></returns>
    private ChatLog GetLog(Configuration.ChatLogConfiguration cfg)
    {
        UpdateConfigValues();
        if (!_logs.ContainsKey(cfg.Name))
            _logs[cfg.Name] = cfg.Name == Configuration.AllLogName
                ? new AllChatLog(this, cfg)
                : new GroupChatLog(this, cfg, _myself);

        return _logs[cfg.Name];
    }

    /// <summary>
    ///     Checks if any configuration values have changed that warrant the closing and possible reopening of the
    ///     currently open log files.
    /// </summary>
    private void UpdateConfigValues()
    {
        if (_configuration.LogDirectory == _logDirectory &&
            _configuration.LogFileNamePrefix == _logFileNamePrefix &&
            _configuration.LogOrder == _logOrder) return;
        CloseLogs();
        _logDirectory = _configuration.LogDirectory;
        _logFileNamePrefix = _configuration.LogFileNamePrefix;
        _logOrder = _configuration.LogOrder;
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
        _logStartTime = null;
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

    /// <summary>
    ///     Defines the handler for a single log file.
    /// </summary>
    private abstract class ChatLog : IDisposable
    {
        private static readonly Configuration.ChatLogConfiguration.ChatTypeFlag DefaultChatTypeFlag = new();
        private readonly ChatLogManager _manager;
        protected readonly Configuration.ChatLogConfiguration Config;
        private LocalDate _lastWrite = LocalDate.MinIsoValue;

        protected ChatLog(ChatLogManager chatLogManager, Configuration.ChatLogConfiguration configuration)
        {
            _manager = chatLogManager;
            Config = configuration;
        }

        /// <summary>
        ///     The name of the log file if this log is open.
        /// </summary>
        private string FileName { get; set; } = string.Empty;

        /// <summary>
        ///     The <see cref="StreamWriter" /> that we are writing to.
        /// </summary>
        private StreamWriter Log { get; set; } = StreamWriter.Null;

        /// <summary>
        ///     The default format string for this log.
        /// </summary>
        protected abstract string DefaultFormat { get; }

        /// <summary>
        ///     Returns true if this log is open.
        /// </summary>
        private bool IsOpen => Log != StreamWriter.Null;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }

        /// <summary>
        ///     Determines if the give log information should be sent to the log output by examining the configuration.
        /// </summary>
        /// <param name="chatMessage">The chat message information.</param>
        /// <param name="cleanedSender">The sender after processing based on the configuration.</param>
        /// <returns><c>true</c> if this message should be logged.</returns>
        protected virtual bool ShouldLog(ChatMessage chatMessage, string cleanedSender)
        {
            if (!Config.IsActive) return false;
            if (Config.DebugIncludeAllMessages) return true;
            return !chatMessage.TypeLabel.IsNullOrEmpty() &&
                   Config.ChatTypeFilterFlags.GetValueOrDefault(chatMessage.ChatType, DefaultChatTypeFlag).Value;
        }

        /// <summary>
        ///     Logs the chat information to the target log.
        /// </summary>
        /// <remarks>
        ///     The configuration defines whether the given message should be logged as well as what massaging of the data
        ///     is required.
        /// </remarks>
        /// <param name="chatMessage">The chat message information.</param>
        public void LogInfo(ChatMessage chatMessage)
        {
            var cleanedSender = ReplaceSender(chatMessage);
            var cleanedBody = chatMessage.Body.AsText(Config);
            if (ShouldLog(chatMessage, cleanedSender))
                WriteLog(chatMessage, cleanedSender, cleanedBody);
        }

        /// <summary>
        ///     Formats the chat message using this logs format string and sends it to the log output.
        /// </summary>
        /// <param name="chatMessage">The chat message information.</param>
        /// <param name="cleanedSender">The sender after processing based on the configuration.</param>
        /// <param name="cleanedBody">The body after processing based on the configuration.</param>
        private void WriteLog(ChatMessage chatMessage, string cleanedSender, string cleanedBody)
        {
            WriteDateSeparator();
            var bodyParts = WrapBody(cleanedBody);
            var whenString =
                chatMessage.When.ToString(
                    Config.DateTimeFormat ?? _manager._dateHelper.CultureDateTimePattern.PatternText, null);
            var format = Config.Format ?? DefaultFormat;
            var logText = FormatMessage(chatMessage, cleanedSender, format, bodyParts[0], whenString);
            WriteLine(logText);
            var indentation = Config.MessageWrapIndentation >= 0
                ? Config.MessageWrapIndentation
                : logText.IndexOf(bodyParts[0], StringComparison.Ordinal);
            var padding = "".PadLeft(indentation);
            for (var i = 1; i < bodyParts.Count; i++) WriteLine($"{padding}{bodyParts[i]}");
        }

        /// <summary>
        ///     Writes a beginning of day separator when the next log line is written and the day has changed.
        /// </summary>
        private void WriteDateSeparator()
        {
            var today = _manager._dateHelper.CurrentDate;
            var difference = today - _lastWrite;
            if (difference.Days <= 0) return;
            _lastWrite = today;
            WriteLine($"============================== {today} ==============================");
        }

        /// <summary>
        ///     Formats a single message text line using the setup format string and all of the various pieces that can
        ///     go into a message. The format will take what it wants.
        /// </summary>
        /// <param name="chatMessage">The chat message info.</param>
        /// <param name="cleanedSender">The cleaned sender, possibly replaced.</param>
        /// <param name="format">The format string.</param>
        /// <param name="body">
        ///     The first part of the body text.  If the body has been wrapped, this is the first lien, otherwise it
        ///     is all of the body.
        /// </param>
        /// <param name="whenString">The formatted timestamp.</param>
        /// <returns></returns>
        private static string FormatMessage(ChatMessage chatMessage, string cleanedSender, string format, string body,
            string whenString)
        {
            var logText = string.Format(format, chatMessage.TypeLabel, chatMessage.TypeLabel,
                chatMessage.Sender,
                cleanedSender, $"{cleanedSender} [{chatMessage.TypeLabel}]",
                body, whenString);
            return logText;
        }

        /// <summary>
        ///     Replaces the sender if there is a replacement defined.
        /// </summary>
        /// <param name="chatMessage">The chat message information.</param>
        /// <returns>The sender to use in the log.</returns>
        private string ReplaceSender(ChatMessage chatMessage)
        {
            var cleanedSender = chatMessage.Sender.AsText(Config);
            var result = Config.Users.GetValueOrDefault(chatMessage.Sender.ToString(), cleanedSender);
            if (result.IsNullOrWhitespace()) result = cleanedSender;
            return result;
        }

        /// <summary>
        ///     Returns a list of all of the wrapped lines of the body.
        /// </summary>
        /// <param name="body">The body to wrap.</param>
        /// <returns>The list of lines, there will always be at least 1 line.</returns>
        private List<string> WrapBody(string body)
        {
            if (Config.MessageWrapWidth < 1) return new List<string> {body,};
            var lines = new List<string>();
            while (body.Length > Config.MessageWrapWidth)
            {
                var index = FindBreakPoint(body, Config.MessageWrapWidth);
                var first = body[..index].Trim();
                lines.Add(first);
                body = body[index..].Trim();
            }

            if (body.Length > 0) lines.Add(body);
            return lines;
        }

        /// <summary>
        ///     Returns the next breakpoint in the body. This will be the last space character if there is one
        ///     or forced at the wrap column if necessary.
        /// </summary>
        /// <param name="body">The body text to process.</param>
        /// <param name="width">The wrap width.</param>
        /// <returns>An index into body to break.</returns>
        private static int FindBreakPoint(string body, int width)
        {
            if (body.Length < width) return body.Length - 1;
            for (var i = width - 1; i >= 0; i--)
                if (body[i] == ' ')
                    return i;

            return width - 1; // If there are no spaces we have to force a break
        }

        /// <summary>
        ///     Writes a single line to the log output. The output is flushed after every line.
        /// </summary>
        /// <param name="line">The text to output.</param>
        private void WriteLine(string line)
        {
            Open();
            Log.WriteLine(line);
            Log.Flush();
        }

        /// <summary>
        ///     Opens this log if it is not already open. A new filename may be created if configuration values have changed.
        /// </summary>
        private void Open()
        {
            if (IsOpen) return;
            var dateString = _manager.FileNameLogStartTime;
            var pattern = _manager._configuration.LogOrder == Configuration.FileNameOrder.PrefixGroupDate
                ? "{0}-{1}-{2}"
                : "{0}-{2}-{1}";
            var name = string.Format(pattern, _manager._logFileNamePrefix, Config.Name, dateString);
            FileName = _manager._fileHelper.FullFileName(_manager._logDirectory, name, FileHelper.LogFileExtension);
            _manager._fileHelper.EnsureDirectoryExists(_manager._logDirectory);
            Log = new StreamWriter(FileName, true);
        }

        /// <summary>
        ///     Closes this log if open.
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
            {
                Log.Close();
                Log = StreamWriter.Null;
                FileName = string.Empty;
            }
        }

        /// <summary>
        ///     Dumps the information about this logger to the dev log.
        /// </summary>
        /// <param name="logger">Where to send the information.</param>
        public void DumpLog(ILogger logger)
        {
            logger.Log($"{Config.Name,-12}  {IsOpen,-5}  '{FileName}'");
        }
    }

    /// <summary>
    ///     Chat log for group based logs.
    /// </summary>
    private class GroupChatLog : ChatLog
    {
        private readonly Myself _myself;

        public GroupChatLog(ChatLogManager chatLogManager, Configuration.ChatLogConfiguration configuration,
            Myself myself) : base(
            chatLogManager, configuration)
        {
            _myself = myself;
        }

        protected override string DefaultFormat => "{6,22} {4,-30} {5}";

        protected override bool ShouldLog(ChatMessage chatMessage, string cleanedSender)
        {
            if (!base.ShouldLog(chatMessage, cleanedSender)) return false;
            if (Config.IncludeAllUsers) return true;
            if (Config.Users.ContainsKey(cleanedSender)) return true;
            return Config.IncludeMe && cleanedSender == _myself.FullName;
        }
    }

    /// <summary>
    ///     Chat log for the log that record everything.
    /// </summary>
    private class AllChatLog : ChatLog
    {
        public AllChatLog(ChatLogManager chatLogManager, Configuration.ChatLogConfiguration configuration) : base(
            chatLogManager, configuration)
        {
        }

        protected override string DefaultFormat => "{2}:{0}:{5}";
    }
}
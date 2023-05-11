using System;
using System.Collections.Generic;
using System.IO;
using Chatter.System;
using Chatter.Utilities;
using Dalamud.Utility;
using NodaTime;
using JetBrains.Annotations;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Defines the handler for a single log file.
/// </summary>
public abstract class ChatLog : IChatLog, IDisposable
{
    private static readonly ChatLogConfiguration.ChatTypeFlag DefaultChatTypeFlag = new();
    private readonly IDateHelper _dateHelper;
    private readonly FileHelper _fileHelper;
    protected readonly ChatLogConfiguration LogConfiguration;
    private LocalDate _lastWrite = LocalDate.MinIsoValue;
    private LogFileInfo _logFileInfo;

    protected ChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper, FileHelper fileHelper)
    {
        LogConfiguration = configuration;
        _logFileInfo = logFileInfo;
        _dateHelper = dateHelper;
        _fileHelper = fileHelper;
    }

    /// <summary>
    ///     The <see cref="TextWriter" /> that we are writing to.
    /// </summary>
    private TextWriter Log { get; set; } = TextWriter.Null;

    /// <summary>
    ///     The default format string for this log.
    /// </summary>
    protected abstract string DefaultFormat { get; }

    /// <summary>
    ///     The name of the log file if this log is open.
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    ///     Returns true if this log is open.
    /// </summary>
    public bool IsOpen => Log != TextWriter.Null;

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
    [UsedImplicitly]
    public virtual bool ShouldLog(ChatMessage chatMessage, string cleanedSender)
    {
        if (!LogConfiguration.IsActive) return false;
        if (LogConfiguration.DebugIncludeAllMessages) return true;
        return !chatMessage.TypeLabel.IsNullOrEmpty() &&
               LogConfiguration.ChatTypeFilterFlags.GetValueOrDefault(chatMessage.ChatType, DefaultChatTypeFlag).Value;
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
        var cleanedBody = chatMessage.Body.AsText(LogConfiguration.IncludeServer);
        if (ShouldLog(chatMessage, cleanedSender))
            WriteLog(chatMessage, cleanedSender, cleanedBody);
    }

    /// <summary>
    ///     Formats the chat message using this logs format string and sends it to the log output.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <param name="cleanedSender">The sender after processing based on the configuration.</param>
    /// <param name="cleanedBody">The body after processing based on the configuration.</param>
    [UsedImplicitly]
    public void WriteLog(ChatMessage chatMessage, string cleanedSender, string cleanedBody)
    {
        WriteDateSeparator();
        var bodyParts = StringHelper.WrapBody(cleanedBody, LogConfiguration.MessageWrapWidth);
        var whenString =
            chatMessage.When.ToString(
                LogConfiguration.DateTimeFormat ?? _dateHelper.CultureDateTimePattern.PatternText, null);
        var format = LogConfiguration.Format ?? DefaultFormat;
        var logText = FormatMessage(chatMessage, cleanedSender, format, bodyParts[0], whenString);
        WriteLine(logText);
        var indentation = LogConfiguration.MessageWrapIndentation >= 0
            ? LogConfiguration.MessageWrapIndentation
            : logText.IndexOf(bodyParts[0], StringComparison.Ordinal);
        var padding = "".PadLeft(indentation);
        for (var i = 1; i < bodyParts.Count; i++) WriteLine($"{padding}{bodyParts[i]}");
    }

    /// <summary>
    ///     Writes a beginning of day separator when the next log line is written and the day has changed.
    /// </summary>
    private void WriteDateSeparator()
    {
        var today = _dateHelper.CurrentDate;
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
        var cleanedSender = chatMessage.Sender.AsText(LogConfiguration.IncludeServer);
        var result = LogConfiguration.Users.GetValueOrDefault(chatMessage.Sender.ToString(), cleanedSender);
        if (result.IsNullOrWhitespace()) result = cleanedSender;
        return result;
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
        _logFileInfo.StartTime ??= _dateHelper.ZonedNow; // If this is the first file opened, set the start time.
        var dateString = _logFileInfo.FileNameDatePattern.Format((ZonedDateTime) _logFileInfo.StartTime);

        var pattern = _logFileInfo.Order == FileNameOrder.PrefixGroupDate
            ? "{0}-{1}-{2}"
            : "{0}-{2}-{1}";
        var name = string.Format(pattern, _logFileInfo.FileNamePrefix, LogConfiguration.Name, dateString);
        FileName = _fileHelper.FullFileName(_logFileInfo.Directory, name, FileHelper.LogFileExtension);
        _fileHelper.EnsureDirectoryExists(_logFileInfo.FileNamePrefix);
        Log = _fileHelper.OpenFile(FileName, true);
    }

    /// <summary>
    ///     Closes this log if open.
    /// </summary>
    public void Close()
    {
        if (!IsOpen)
        {
            Log.Close();
            Log = TextWriter.Null;
            FileName = string.Empty;
        }
    }

    /// <summary>
    ///     Dumps the information about this logger to the dev log.
    /// </summary>
    /// <param name="logger">Where to send the information.</param>
    public void DumpLog(ILogger logger)
    {
        logger.Log($"{LogConfiguration.Name,-12}  {IsOpen,-5}  '{FileName}'");
    }
}
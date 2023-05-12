using System;
using System.Collections.Generic;
using System.IO;
using Chatter.System;
using Chatter.Utilities;
using JetBrains.Annotations;
using NodaTime;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Defines the handler for a single log file.
/// </summary>
public abstract class ChatLog : IChatLog, IDisposable
{
    private static readonly ChatLogConfiguration.ChatTypeFlag DefaultChatTypeFlag = new();

    protected readonly IDateHelper DateHelper;
    protected readonly ChatLogConfiguration LogConfiguration;
    protected readonly LogFileInfo LogFileInfo;

    protected readonly FileHelper _fileHelper;
    private LocalDate _lastWrite = LocalDate.MinIsoValue;

    protected ChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper)
    {
        LogConfiguration = configuration;
        LogFileInfo = logFileInfo;
        DateHelper = dateHelper;
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

    /// <summary>
    ///     Determines if the give log information should be sent to the log output by examining the configuration.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <returns><c>true</c> if this message should be logged.</returns>
    [UsedImplicitly]
    public virtual bool ShouldLog(ChatMessage chatMessage)
    {
        if (!LogConfiguration.IsActive) return false;
        if (LogConfiguration.DebugIncludeAllMessages) return true;
        return !string.IsNullOrWhiteSpace(chatMessage.TypeLabel) &&
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
        var loggableSender = chatMessage.GetLoggableSender(LogConfiguration.IncludeServer, LogConfiguration.Users);
        var loggableBody = chatMessage.GetLoggableBody(LogConfiguration.IncludeServer);
        if (ShouldLog(chatMessage))
            WriteLog(chatMessage, loggableSender, loggableBody);
    }

    /// <summary>
    ///     Formats the chat message using this logs format string and sends it to the log output.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <param name="sender">The sender after processing based on the configuration.</param>
    /// <param name="body">The body after processing based on the configuration.</param>
    [UsedImplicitly]
    public void WriteLog(ChatMessage chatMessage, string sender, string body)
    {
        WriteDateSeparator();
        var bodyParts = StringHelper.WrapBody(body, LogConfiguration.MessageWrapWidth);
        var whenString =
            chatMessage.When.ToString(
                LogConfiguration.DateTimeFormat ?? DateHelper.CultureDateTimePattern.PatternText, null);
        var format = LogConfiguration.Format ?? DefaultFormat;
        var logText = FormatMessage(chatMessage, sender, format, bodyParts[0], whenString);
        WriteLine(logText);
        var indentation = LogConfiguration.MessageWrapIndentation >= 0
            ? LogConfiguration.MessageWrapIndentation
            : logText.IndexOf(bodyParts[0], StringComparison.Ordinal);
        var padding = "".PadLeft(indentation);
        for (var i = 1; i < bodyParts.Count; i++) WriteLine($"{padding}{bodyParts[i]}");
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Close();
    }

    /// <summary>
    ///     Writes a beginning of day separator when the next log line is written and the day has changed.
    /// </summary>
    private void WriteDateSeparator()
    {
        var today = DateHelper.CurrentDate;
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
        var logText = string.Format(format, // pattern
            chatMessage.TypeLabel, // {0}
            chatMessage.TypeLabel, // {1}
            chatMessage.Sender, // {2}
            cleanedSender, // {3}
            $"{cleanedSender} [{chatMessage.TypeLabel}]", // {4}
            body, // {5}
            whenString // {6}
        );
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
        if (string.IsNullOrWhiteSpace(result)) result = cleanedSender;
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
        LogFileInfo.StartTime ??= DateHelper.ZonedNow; // If this is the first file opened, set the start time.
        var dateString = LogFileInfo.FileNameDatePattern.Format((ZonedDateTime) LogFileInfo.StartTime);

        var pattern = LogFileInfo.Order == FileNameOrder.PrefixGroupDate
            ? "{0}-{1}-{2}"
            : "{0}-{2}-{1}";
        var name = string.Format(pattern, LogFileInfo.FileNamePrefix, LogConfiguration.Name, dateString);
        FileName = _fileHelper.FullFileName(LogFileInfo.Directory, name, FileHelper.LogFileExtension);
        _fileHelper.EnsureDirectoryExists(LogFileInfo.FileNamePrefix);
        Log = _fileHelper.OpenFile(FileName, true);
    }
}
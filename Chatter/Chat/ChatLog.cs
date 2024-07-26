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

using Chatter.Reporting;
using Chatter.System;
using Chatter.Utilities;
using JetBrains.Annotations;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Defines the handler for a single log file.
/// </summary>
public abstract class ChatLog(
    Configuration configuration,
    ChatLogConfiguration logConfiguration,
    LogFileInfo logFileInfo,
    IDateHelper dateHelper,
    FileHelper fileHelper,
    IErrorWriter errorWriter) : IChatLog, IDisposable
{
    private static readonly ChatLogConfiguration.ChatTypeFlag DefaultChatTypeFlag = new();

    protected readonly Configuration Configuration = configuration;
    protected readonly IDateHelper DateHelper = dateHelper;
    protected readonly FileHelper FileHelper = fileHelper;
    protected readonly ChatLogConfiguration LogConfiguration = logConfiguration;
    protected readonly LogFileInfo LogFileInfo = logFileInfo;

    private LocalDate _lastWrite = LocalDate.MinIsoValue;

    private IErrorWriter ErrorWriter { get; } = errorWriter;

    /// <summary>
    ///     The <see cref="TextWriter" /> that we are writing to.
    /// </summary>
    private TextWriter Log { get; set; } = TextWriter.Null;

    /// <summary>
    ///     The default format string for this log.
    /// </summary>
    protected abstract string DefaultFormat { get; }

    private bool Failed { get; set; }

    /// <inheritdoc />
    public string FileName { get; private set; } = string.Empty;

    /// <inheritdoc />
    public bool IsOpen => Log != TextWriter.Null;

    /// <inheritdoc />
    [UsedImplicitly]
    public virtual bool ShouldLog(ChatMessage chatMessage)
    {
        if (!LogConfiguration.IsActive) return false;
        if (LogConfiguration.DebugIncludeAllMessages) return true;
        return !string.IsNullOrWhiteSpace(chatMessage.TypeLabel)
            && LogConfiguration.ChatTypeFilterFlags.GetValueOrDefault(chatMessage.ChatType, DefaultChatTypeFlag).Value;
    }

    /// <inheritdoc />
    public void LogInfo(ChatMessage chatMessage)
    {
        var loggableSender = chatMessage.GetLoggableSender(LogConfiguration.IncludeServer, LogConfiguration.Users);
        var loggableBody = chatMessage.GetLoggableBody(LogConfiguration.IncludeServer);
        if (ShouldLog(chatMessage)) WriteLog(chatMessage, loggableSender, loggableBody);
    }

    /// <inheritdoc />
    public void WriteLog(ChatMessage chatMessage, string sender, string body)
    {
        WriteDateSeparator();
        var bodyParts = StringHelper.WrapBody(body, LogConfiguration.MessageWrapWidth);
        var whenString = FormatWhen(chatMessage.When);
        var format = LogConfiguration.Format ?? DefaultFormat;
        var logText = FormatMessage(chatMessage, sender, format, bodyParts[0], whenString);
        WriteLine(logText);
        var indentation = LogConfiguration.MessageWrapIndentation >= 0
                              ? LogConfiguration.MessageWrapIndentation
                              : logText.IndexOf(bodyParts[0], StringComparison.Ordinal);
        var padding = "".PadLeft(indentation);
        for (var i = 1; i < bodyParts.Count; i++) WriteLine($"{padding}{bodyParts[i]}");
    }

    /// <inheritdoc />
    public void Close()
    {
        if (!IsOpen) return;
        Log.Close();
        Log = TextWriter.Null;
        FileName = string.Empty;
    }

    /// <inheritdoc />
    public void DumpLog(ILogger logger)
    {
        logger.Log($"{LogConfiguration.Name,-12}  {IsOpen,-5}  '{FileName}'");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Close();
    }

    protected virtual string FormatWhen(ZonedDateTime when)
    {
        return when.ToString(LogConfiguration.DateTimeFormat ?? DateHelper.CultureDateTimePattern.PatternText, null);
    }

    /// <summary>
    ///     Writes a beginning of day separator when the next log line is written and the day has changed.
    /// </summary>
    private void WriteDateSeparator()
    {
        var today = DateHelper.CurrentDate;
        var difference = Period.DaysBetween(_lastWrite, today);
        if (difference <= 0) return;
        _lastWrite = today;
        WriteLine($"============================== {today} ==============================");
    }

    /// <summary>
    ///     Formats a single message text line using the setup format string and all the various pieces that can
    ///     go into a message. The format will take what it wants.
    /// </summary>
    /// <param name="chatMessage">The chat message info.</param>
    /// <param name="cleanedSender">The cleaned sender, possibly replaced.</param>
    /// <param name="format">The format string.</param>
    /// <param name="body">
    ///     The first part of the body text.  If the body has been wrapped, this is the first lien, otherwise it
    ///     is all the body.
    /// </param>
    /// <param name="whenString">The formatted timestamp.</param>
    /// <returns></returns>
    private static string FormatMessage(ChatMessage chatMessage,
                                        string cleanedSender,
                                        string format,
                                        string body,
                                        string whenString)
    {
        var logText = string.Format(format,                                       // pattern
                                    chatMessage.TypeLabel,                        // {0}
                                    chatMessage.TypeLabel,                        // {1}
                                    chatMessage.Sender,                           // {2}
                                    cleanedSender,                                // {3}
                                    $"{cleanedSender} [{chatMessage.TypeLabel}]", // {4}
                                    body,                                         // {5}
                                    whenString                                    // {6}
                                   );
        return logText;
    }

    /// <summary>
    ///     Writes a single line to the log output. The output is flushed after every line.
    /// </summary>
    /// <param name="line">The text to output.</param>
    private void WriteLine(string line)
    {
        if (Failed) return;
        Open();
        Log.WriteLine(line);
        Log.Flush();
    }

    /// <summary>
    ///     Opens this log if it is not already open. A new filename may be created if configuration values have changed.
    /// </summary>
    private void Open()
    {
        if (Failed) return;
        if (IsOpen) return;
        /*
         * There is a logic error if we get to this point and StartDate is null but rather than
         * give up we'll just use the current time as it is only used for the file name and is
         * therefore not critical.
         */
        var startDate = LogFileInfo.StartTime ?? DateHelper.ZonedNow;

        var pathAdditional = GenerateDirectoryPrefix(startDate);
        var directory = FileHelper.Join(LogFileInfo.Directory, pathAdditional);
        var ensureCode = FileHelper.EnsureDirectoryExists(LogFileInfo.Directory);
        if (ensureCode != FileHelper.EnsureCode.Success)
        {
            ErrorWriter.PrintError($"Could not create logging directory '{LogFileInfo.Directory}' because {ensureCode}");
            Failed = true; // So we don't keep trying to create a failure.
            return;
        }

        ensureCode = FileHelper.EnsureDirectoriesExists(directory);
        if (ensureCode != FileHelper.EnsureCode.Success)
        {
            ErrorWriter.PrintError($"Could not create full logging directory '{directory}' because {ensureCode}");
            Failed = true; // So we don't keep trying to create a failure.
            return;
        }

        var dateString = LogFileInfo.FileNameDatePattern.Format(startDate);
        var pattern = LogFileInfo.Order == FileNameOrder.PrefixGroupDate ? "{0}-{1}-{2}" : "{0}-{2}-{1}";
        var name = string.Format(pattern, LogFileInfo.FileNamePrefix, LogConfiguration.Name, dateString);
        FileName = FileHelper.FullFileName(directory, name, FileHelper.LogFileExtension);
        Log = FileHelper.OpenFile(FileName, true);
    }

    private string GenerateDirectoryPrefix(ZonedDateTime date)
    {
        return Configuration.DirectoryForm switch
        {
            DirectoryFormat.None or DirectoryFormat.Unified => string.Empty,
            DirectoryFormat.Group                           => LogConfiguration.Name,
            DirectoryFormat.YearMonth                       => $"{date.Year}/{date.Month:D2}",
            DirectoryFormat.YearMonthGroup                  => $"{date.Year}/{date.Month:D2}/{LogConfiguration.Name}",
            DirectoryFormat.GroupYearMonth                  => $"{LogConfiguration.Name}/{date.Year}/{date.Month:D2}",
            _                                               => throw new ArgumentOutOfRangeException(),
        };
    }
}

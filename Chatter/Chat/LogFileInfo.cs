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

using NodaTime;
using NodaTime.Text;
using static Chatter.Configuration;
using static NodaTime.Text.LocalTimePattern;

namespace Chatter.Chat;

/// <summary>
///     Common information for all chat logs.
/// </summary>
public class LogFileInfo
{
    private static readonly LocalTimePattern TimePattern = CreateWithInvariantCulture("H:mm");
    private static readonly LocalTime DefaultTimeToClose = new(6, 0);

    // ReSharper disable once StringLiteralTypo
    private static readonly ZonedDateTimePattern DefaultFileDateTimePattern =
        ZonedDateTimePattern.CreateWithCurrentCulture("yyyyMMdd-HHmmss", null);

    private ZonedDateTime? _startTime;

    public LogFileInfo(string? directory = null,
                       string? fileNamePrefix = null,
                       FileNameOrder order = FileNameOrder.None,
                       ZonedDateTime? startTime = null,
                       ZonedDateTimePattern? fileNameDatePattern = null,
                       LocalTime? timeToClose = null)
    {
        Directory = directory ?? string.Empty;
        FileNamePrefix = fileNamePrefix ?? string.Empty;
        Order = order;
        StartTime = startTime;
        FileNameDatePattern = fileNameDatePattern ?? DefaultFileDateTimePattern;
        TimeToClose = timeToClose ?? DefaultTimeToClose;
    }

    /// <summary>
    ///     The directory to write logs to.
    /// </summary>
    public string Directory { get; private set; }

    /// <summary>
    ///     The prefix for log file names.
    /// </summary>
    public string FileNamePrefix { get; private set; }

    /// <summary>
    ///     The order of the parts in a log file name.
    /// </summary>
    public FileNameOrder Order { get; private set; }

    /// <summary>
    ///     When the logs were started. Used for roll-overs and day markers.
    /// </summary>
    public ZonedDateTime? StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            if (_startTime == null) return;
            var when = _startTime.Value;
            CloseCutoff = when.TimeOfDay < TimeToClose
                              ? when.Date.At(TimeToClose)              // Cutoff is later today
                              : when.Date.PlusDays(1).At(TimeToClose); // Cutoff is tomorrow
        }
    }

    /// <summary>
    ///     The date time format pattern for the date in a log file name. This is always fixed.
    /// </summary>
    public ZonedDateTimePattern FileNameDatePattern { get; }

    public LocalDateTime CloseCutoff { get; private set; }

    /// <summary>
    ///     The <see cref="LocalTime" /> parsing of <see cref="WhenToClose" />.
    /// </summary>
    private LocalTime TimeToClose { get; set; }

    /// <summary>
    ///     The string setting from the configuration for when logs should be closed and reopened.
    /// </summary>
    private string WhenToClose
    {
        get => TimePattern.Format(TimeToClose);
        set
        {
            var result = TimePattern.Parse(value);
            TimeToClose = result.Success ? result.Value : DefaultTimeToClose;
            /*
             * If we have started logging, set the cutoff time to today at that time. If it is before now,
             * this will force a close and reopen as expected. If it is after now, then the cutoff will happen
             * as normal.
             */
            if (StartTime != null) CloseCutoff = StartTime.Value.Date.At(TimeToClose);
        }
    }

    /// <summary>
    ///     Checks if any configuration values have changed that warrant the closing and possible reopening of the
    ///     currently open log files.
    /// </summary>
    public bool UpdateConfigValues(Configuration configuration)
    {
        if (configuration.WhenToCloseLogs != WhenToClose)
        {
            WhenToClose = configuration.WhenToCloseLogs;
            /*
             * This next line copies the parsed and formatted time back into the configuration if
             * it is different to normalize the format.
             */
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (configuration.WhenToCloseLogs != WhenToClose) configuration.WhenToCloseLogs = WhenToClose;
        }

        if (configuration.LogDirectory == Directory
         && configuration.LogFileNamePrefix == FileNamePrefix
         && configuration.LogOrder == Order)
            return false;
        Directory = configuration.LogDirectory;
        FileNamePrefix = configuration.LogFileNamePrefix;
        Order = configuration.LogOrder;
        return true;
    }
}

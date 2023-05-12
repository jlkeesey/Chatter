using NodaTime;
using NodaTime.Text;
using static NodaTime.Text.LocalTimePattern;
using static Chatter.Configuration;

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

    public LogFileInfo(string? directory = null, string? fileNamePrefix = null,
        FileNameOrder order = FileNameOrder.None, ZonedDateTime? startTime = null,
        ZonedDateTimePattern? fileNameDatePattern = null, LocalTime? timeToClose = null)
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
    public string Directory { get; set; }

    /// <summary>
    ///     The prefix for log file names.
    /// </summary>
    public string FileNamePrefix { get; set; }

    /// <summary>
    ///     The order of the parts in a log file name.
    /// </summary>
    public FileNameOrder Order { get; set; }

    /// <summary>
    ///     When the logs were started. Used for roll-overs and day markers.
    /// </summary>
    public ZonedDateTime? StartTime { get; set; }

    /// <summary>
    ///     The date time format pattern for the date in a log file name. This is always fixed.
    /// </summary>
    public ZonedDateTimePattern FileNameDatePattern { get; init; }

    /// <summary>
    ///     The <see cref="LocalTime" /> parsing of <see cref="WhenToClose" />.
    /// </summary>
    public LocalTime TimeToClose { get; private set; }

    /// <summary>
    ///     The string setting from the configuration for when logs should be closed and reopened.
    /// </summary>
    public string WhenToClose
    {
        get => TimePattern.Format(TimeToClose);
        set
        {
            var result = TimePattern.Parse(value);
            TimeToClose = result.Success ? result.Value : DefaultTimeToClose;
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
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (configuration.WhenToCloseLogs != WhenToClose)
            {
                configuration.WhenToCloseLogs = WhenToClose;
            }
        }

        if (configuration.LogDirectory == Directory &&
            configuration.LogFileNamePrefix == FileNamePrefix &&
            configuration.LogOrder == Order) return false;
        Directory = configuration.LogDirectory;
        FileNamePrefix = configuration.LogFileNamePrefix;
        Order = configuration.LogOrder;
        return true;
    }


}
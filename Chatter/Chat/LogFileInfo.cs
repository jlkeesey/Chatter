using NodaTime;
using NodaTime.Text;

namespace Chatter.Chat;

/// <summary>
///     Common information for all chat logs.
/// </summary>
public struct LogFileInfo
{
    public LogFileInfo()
    {
    }

    /// <summary>
    ///     The directory to write logs to.
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    ///     The prefix for log file names.
    /// </summary>
    public string FileNamePrefix { get; set; } = string.Empty;

    /// <summary>
    ///     The order of the parts in a log file name.
    /// </summary>
    public Configuration.FileNameOrder Order { get; set; } = Configuration.FileNameOrder.None;

    /// <summary>
    ///     When the logs were started. Used for roll-overs and day markers.
    /// </summary>
    public ZonedDateTime? StartTime { get; set; } = null;

    /// <summary>
    ///     The date time format pattern for the date in a log file name. This is always fixed.
    /// </summary>
    public ZonedDateTimePattern FileNameDatePattern { get; init; } =
        ZonedDateTimePattern.CreateWithCurrentCulture("yyyyMMdd-HHmmss", null);
}
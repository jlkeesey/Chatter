using NodaTime;
using NodaTime.Text;

namespace Chatter.System;

/// <summary>
///     The basic date management needed by this plugin.
/// </summary>
public interface IDateHelper
{
    /// <summary>
    ///     Returns a <see cref="ZonedDateTime" /> representing the moment the method was invoked.
    /// </summary>
    ZonedDateTime ZonedNow { get; }

    /// <summary>
    ///     Returns today as a <see cref="LocalDate" />.
    /// </summary>
    LocalDate CurrentDate { get; }

    /// <summary>
    ///     Returns the now as a <see cref="LocalTime" />.
    /// </summary>
    LocalTime CurrentTime { get; }

    /// <summary>
    ///     Returns the <see cref="ZonedDateTimePattern"/> representing a cultural based format.
    /// </summary>
    ZonedDateTimePattern CultureDateTimePattern { get; }

    /// <summary>
    ///     Returns the <see cref="ZonedDateTimePattern"/> for a universally sortable date.
    /// </summary>
    ZonedDateTimePattern SortableDateTimePattern { get; }
}
using System.Globalization;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace Chatter.System;

/// <summary>
///     Helper methods for manipulating dates and times. This uses NodaTime for the support.
/// </summary>
public sealed class DateHelper : IDateHelper
{
    private ZonedDateTimePattern? _cultureDateTimePattern;
    private ZonedDateTimePattern? _sortableDateTimePattern;

    private ZonedClock Clock { get; } = SystemClock.Instance.InTzdbSystemDefaultZone();

    public ZonedDateTime ZonedNow => Clock.GetCurrentZonedDateTime();

    public LocalDate CurrentDate => Clock.GetCurrentDate();
    public LocalTime CurrentTime => Clock.GetCurrentTimeOfDay();

    public ZonedDateTimePattern CultureDateTimePattern => _cultureDateTimePattern ??= CreateCultureDateTimePattern();
    public ZonedDateTimePattern SortableDateTimePattern => _sortableDateTimePattern ??= CreateSortableDateTimePattern();

    private static ZonedDateTimePattern CreateCultureDateTimePattern()
    {
        var bclDateFormat = CultureInfo.CurrentCulture.DateTimeFormat;
        var localDateTimePattern = bclDateFormat.ShortDatePattern + " " + bclDateFormat.ShortTimePattern;
        return ZonedDateTimePattern.CreateWithCurrentCulture(localDateTimePattern, DateTimeZoneProviders.Tzdb);
    }

    private static ZonedDateTimePattern CreateSortableDateTimePattern()
    {
        return ZonedDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss", DateTimeZoneProviders.Tzdb);
    }
}
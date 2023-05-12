using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatter.System;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;

namespace Chatter.UnitTests.Support;

internal class DateHelperFake : IDateHelper
{
    private readonly FakeClock _clock;
    private readonly ZonedClock _zonedClock;

    public DateHelperFake()
    {
        var initial = Instant.FromUtc(2023, 5, 9, 17, 00);
        _clock = new FakeClock(initial, Duration.FromSeconds(1));
        _zonedClock = _clock.InUtc();
    }

    public ZonedDateTime ZonedNow => _zonedClock.GetCurrentZonedDateTime();
    public LocalDate CurrentDate => _zonedClock.GetCurrentDate();
    public LocalTime CurrentTime => _zonedClock.GetCurrentTimeOfDay();
    public ZonedDateTimePattern CultureDateTimePattern =>
        ZonedDateTimePattern.CreateWithInvariantCulture("M-d-yyyy H:mm:ss", DateTimeZoneProviders.Tzdb);

    public ZonedDateTimePattern SortableDateTimePattern =>
        ZonedDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss", DateTimeZoneProviders.Tzdb);

    public Duration AutoAdvance => _clock.AutoAdvance;

    public void NextDay()
    {
        _clock.AdvanceDays(1);
    }

    public void NextHour()
    {
        _clock.AdvanceHours(1);
    }
}
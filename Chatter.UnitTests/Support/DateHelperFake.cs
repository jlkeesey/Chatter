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

    public Duration AutoAdvance => _clock.AutoAdvance;

    public ZonedDateTime ZonedNow => _zonedClock.GetCurrentZonedDateTime();
    public LocalDate CurrentDate => _zonedClock.GetCurrentDate();
    public LocalTime CurrentTime => _zonedClock.GetCurrentTimeOfDay();

    public ZonedDateTimePattern CultureDateTimePattern =>
        ZonedDateTimePattern.CreateWithInvariantCulture("M-d-yyyy H:mm:ss", DateTimeZoneProviders.Tzdb);

    public ZonedDateTimePattern SortableDateTimePattern =>
        ZonedDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss", DateTimeZoneProviders.Tzdb);

    public void NextDay()
    {
        _clock.AdvanceDays(1);
    }

    public void NextHour()
    {
        _clock.AdvanceHours(1);
    }
}

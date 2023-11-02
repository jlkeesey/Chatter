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

using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using System.Globalization;

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

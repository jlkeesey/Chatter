﻿// Copyright 2023 James Keesey
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

using JetBrains.Annotations;
using NodaTime;
using NodaTime.Text;

namespace Chatter.System;

/// <summary>
///     The basic date management needed by this plugin.
/// </summary>
[PublicAPI]
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
    ///     Returns the <see cref="ZonedDateTimePattern" /> representing a cultural based format.
    /// </summary>
    ZonedDateTimePattern CultureDateTimePattern { get; }

    /// <summary>
    ///     Returns the <see cref="ZonedDateTimePattern" /> for a universally sortable date.
    /// </summary>
    ZonedDateTimePattern SortableDateTimePattern { get; }
}

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

using System.Collections.Generic;
using static System.String;

namespace Chatter.Utilities;

public static class StringHelper
{
    /// <summary>
    ///     Returns a list of all of the wrapped lines of the body.
    /// </summary>
    /// <param name="body">The body to wrap.</param>
    /// <param name="width">The maximum width of each part of the string.</param>
    /// <returns>The list of lines, there will always be at least 1 line even if <see cref="string.Empty" />.</returns>
    public static List<string> WrapBody(string body, int width)
    {
        if (width < 1) return new List<string> { body, };
        if (body.Length < 1) return new List<string> { Empty, };
        var lines = new List<string>();
        while (body.Length > width)
        {
            var index = FindBreakPoint(body, width);
            var first = body[..index].Trim();
            lines.Add(first);
            body = body[index..].Trim();
        }

        if (body.Length > 0) lines.Add(body);
        return lines;
    }

    /// <summary>
    ///     Returns the next breakpoint in the body. This will be the last space character if there is one
    ///     or forced at the wrap column if necessary.
    /// </summary>
    /// <param name="body">The body text to process.</param>
    /// <param name="width">The wrap width.</param>
    /// <returns>An index into body to break.</returns>
    private static int FindBreakPoint(string body, int width)
    {
        if (body.Length < width) return body.Length - 1;
        for (var i = width - 1; i >= 0; i--)
            if (body[i] == ' ')
                return i;

        return width; // If there are no spaces we have to force a break
    }
}

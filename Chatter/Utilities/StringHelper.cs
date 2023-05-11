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
    /// <returns>The list of lines, there will always be at least 1 line even if <see cref="string.Empty"/>.</returns>
    public static List<string> WrapBody(string body, int width)
    {
        if (width < 1) return new List<string> {body,};
        if (body.Length < 1) return new List<string> {Empty,};
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
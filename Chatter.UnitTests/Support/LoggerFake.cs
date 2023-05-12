using System;
using System.Collections.Generic;
using System.Text;
using Chatter.System;

namespace Chatter.UnitTests.Support;

internal class LoggerFake : ILogger
{
    private readonly List<string> _lines = new();

    public void Log(string pattern, params object[] args)
    {
        _lines.Add("[L]: " + string.Format(pattern, args));
    }

    public void Debug(string pattern, params object[] args)
    {
        _lines.Add("[D]: " + string.Format(pattern, args));
    }

    public void Error(Exception? exception, string pattern, params object[] args)
    {
        _lines.Add("[E]: " + string.Format(pattern, args));
    }

    public IEnumerable<string> Lines => _lines;
}
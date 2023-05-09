using System;
using Dalamud.Logging;

namespace Chatter.System;

/// <summary>
///     Implementation for the simple logger that passes through to the <see cref="PluginLog" />.
/// </summary>
public class Logger : ILogger
{
    public void Log(string pattern, params object[] args)
    {
        PluginLog.Log(pattern, args);
    }

    public void Debug(string pattern, params object[] args)
    {
        PluginLog.Debug(pattern, args);
    }

    public void Error(Exception? exception, string pattern, params object[] args)
    {
        PluginLog.Error(exception, pattern, args);
    }
}
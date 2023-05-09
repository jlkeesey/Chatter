using System;

namespace Chatter.System;

/// <summary>
///     Simple interface for access to the plugin logger.
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Sends the formatted string to the standard log output.
    /// </summary>
    /// <param name="pattern">The message patter string.</param>
    /// <param name="args">Any replacement parameters for the pattern.</param>
    void Log(string pattern, params object[] args);

    /// <summary>
    ///     Sends the formatted string to the debug log output.
    /// </summary>
    /// <param name="pattern">The message patter string.</param>
    /// <param name="args">Any replacement parameters for the pattern.</param>
    void Debug(string pattern, params object[] args);

    /// <summary>
    ///     Sends the formatted string to the error log output.
    /// </summary>
    /// <param name="exception">The exception that caused the error.</param>
    /// <param name="pattern">The message patter string.</param>
    /// <param name="args">Any replacement parameters for the pattern.</param>
    void Error(Exception? exception, string pattern, params object[] args);
}
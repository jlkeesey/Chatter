using Chatter.System;

namespace Chatter.Chat;

/// <summary>
///     Defines the handler for a single log file.
/// </summary>
public interface IChatLog
{
    /// <summary>
    ///     <c>true</c> if this log is open i.e. there is an open file for this log.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    ///     The name of the file this log is writing to. This is only valid if <see cref="IsOpen" /> is <c>true</c>.
    ///     If <see cref="IsOpen" /> is false this may return a previous name of no name at all.
    /// </summary>
    string FileName { get; }

    /// <summary>
    ///     Dumps the information about this logger to the dev log.
    /// </summary>
    /// <param name="logger">Where to send the information.</param>
    void DumpLog(ILogger logger);

    /// <summary>
    ///     Closes this log if open.
    /// </summary>
    void Close();

    /// <summary>
    ///     Logs the chat information to the target log.
    /// </summary>
    /// <remarks>
    ///     The configuration defines whether the given message should be logged as well as what massaging of the data
    ///     is required.
    /// </remarks>
    /// <param name="chatMessage">The chat message information.</param>
    void LogInfo(ChatMessage chatMessage);

    /// <summary>
    ///     Determines if the give log information should be sent to the log output by examining the configuration.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <returns><c>true</c> if this message should be logged.</returns>
    public bool ShouldLog(ChatMessage chatMessage);

    /// <summary>
    ///     Formats the chat message using this logs format string and sends it to the log output.
    /// </summary>
    /// <param name="chatMessage">The chat message information.</param>
    /// <param name="sender">The sender after processing based on the configuration.</param>
    /// <param name="body">The body after processing based on the configuration.</param>
    public void WriteLog(ChatMessage chatMessage, string sender, string body);
}
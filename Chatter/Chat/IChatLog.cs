namespace Chatter.Chat;

/// <summary>
///     Defines a chat log's basic information.
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
}
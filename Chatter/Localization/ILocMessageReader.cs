namespace Chatter.Localization;

/// <summary>
///     A source for localization messages.
/// </summary>
public interface ILocMessageReader
{
    /// <summary>
    ///     Reads the localization messages with the given name into a string.
    /// </summary>
    /// <param name="name">The name of the resources.</param>
    /// <returns>
    ///     A <c>string</c> containing the contents of the message "file." Returns <see cref="string.Empty" /> if
    ///     the messages can not be read.
    /// </returns>
    public string Read(string name);
}
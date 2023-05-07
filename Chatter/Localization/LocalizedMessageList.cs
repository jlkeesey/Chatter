using System.Collections.Generic;

namespace Chatter.Localization;

/// <summary>
///     A collection of <see cref="LocalizedMessage" /> objects that can be retrieved by a key.
/// </summary>
internal class LocalizedMessageList
{
    private readonly IDictionary<string, LocalizedMessage> _dictionary = new Dictionary<string, LocalizedMessage>();

    public LocalizedMessageList() : this(new List<LocalizedMessage>())
    {
    }

    public LocalizedMessageList(ICollection<LocalizedMessage> messages)
    {
        Messages = messages;
    }

    /// <summary>
    ///     The list of messages. Generally this should not be used, it is present to make serialization work properly.
    /// </summary>
    public ICollection<LocalizedMessage> Messages
    {
        get => _dictionary.Values;
        set
        {
            _dictionary.Clear();
            foreach (var localizedMessage in value) _dictionary.Add(localizedMessage.Key, localizedMessage);
        }
    }

    /// <summary>
    ///     Merges another <see cref="LocalizedMessageList" /> into this one overwriting any values with the same key.
    /// </summary>
    /// <param name="other">The <see cref="LocalizedMessageList" /> to merge into this one.</param>
    public void Merge(LocalizedMessageList other)
    {
        foreach (var localizedMessage in other._dictionary.Values) _dictionary[localizedMessage.Key] = localizedMessage;
    }

    /// <summary>
    ///     Gets the value associated with the given key.
    /// </summary>
    /// <param name="key">The key to retrieve.</param>
    /// <param name="message">The associated message.</param>
    /// <returns><c>true</c> if there is a message with the given key, or <c>false</c> if there is no such message.</returns>
    public bool TryGetValue(string key, out string message)
    {
        if (_dictionary.Count == 0)
            foreach (var localizedMessage in Messages)
                _dictionary.Add(localizedMessage.Key, localizedMessage);

        if (_dictionary.TryGetValue(key, out var locMessage))
        {
            message = locMessage.Message;
            return true;
        }

        message = string.Empty;
        return false;
    }
}
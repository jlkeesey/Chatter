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
        init
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

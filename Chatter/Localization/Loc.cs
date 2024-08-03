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

using Chatter.System;
using System;
using System.Globalization;
using System.Text.Json;
using static System.String;

// ReSharper disable LocalizableElement

namespace Chatter.Localization;

/// <summary>
///     Handles basic message localization.
/// </summary>
public class Loc
{
    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
    };

    private LocalizedMessageList _messages = new();

    public Loc() : this(SystemLanguage, SystemCountry)
    {
    }

    public Loc(string language, string country)
    {
        if (IsNullOrWhiteSpace(language))
            throw new ArgumentException("Argument cannot be null, empty, or whitespace", nameof(language));
        if (IsNullOrWhiteSpace(country))
            throw new ArgumentException("Argument cannot be null, empty, or whitespace", nameof(language));
        Language = language;
        Country = country;
    }

    /// <summary>
    ///     Returns the number of messages in the message list.
    /// </summary>
    public int Count => _messages.Messages.Count;

    /// <summary>
    ///     Returns the short language tag e.g. en for English of the current environment.
    /// </summary>
    private static string SystemLanguage => CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    /// <summary>
    ///     Returns the short country tag e.g. US for the United States of the current environment.
    /// </summary>
    private static string SystemCountry => RegionInfo.CurrentRegion.TwoLetterISORegionName;

    /// <summary>
    ///     Returns the short language tag e.g. en for English.
    /// </summary>
    private string Language { get; }

    /// <summary>
    ///     Returns the short country tag e.g. US for the United States.
    /// </summary>
    private string Country { get; }

    /// <summary>
    ///     Gets the current full IETF language tag, e.g. en_US for U.S. English.
    /// </summary>
    private string LanguageTag => $"{Language}-{Country}";

    /// <summary>
    ///     Loads the localized messages for the current Culture and Region. Clears any previously loaded messages.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         There are up to 3 messages resources that are read. The first is messages which contains the fallback
    ///         for all messages (it is in en-US as that is where the author is from). The second is the language specific
    ///         resource which is based on the current CultureInfo. The name of the resource is messages-LL where LL is
    ///         the two letter language code e.g. en for English. The third is the culture specific resource which is
    ///         based on the current <see cref="CultureInfo" /> and <see cref="RegionInfo" />. The name of the resource is
    ///         messages-LL-CC where LL is the two letter language code as above and CC is the two-letter country code e.g. US
    ///         for the United States.
    ///     </para>
    /// </remarks>
    /// <param name="logger">Where to write debug and error messages.</param>
    public void Load(ILogger logger)
    {
        Load(logger, new LocMessageResourceReader());
    }

    /// <summary>
    ///     Loads the localized messages for the current Culture and Region using the given <see cref="ILocMessageReader" />.
    ///     Clears any previously loaded messages.
    /// </summary>
    /// <inheritdoc cref="Load(ILogger)" />
    /// <param name="logger">Where to write debug and error messages.</param>
    /// <param name="msgReader">Where to read the messages from.</param>
    public void Load(ILogger logger, ILocMessageReader msgReader)
    {
        _messages = LoadMessageList(logger, msgReader, Empty);
        var languageMessages = LoadMessageList(logger, msgReader, Language);
        _messages.Merge(languageMessages);

        var regionalLanguageMessages = LoadMessageList(logger, msgReader, LanguageTag);
        _messages.Merge(regionalLanguageMessages);
    }

    /// <summary>
    ///     Loads and parses a message list JSON resource into a <see cref="LocalizedMessageList" />.
    /// </summary>
    /// <param name="logger">Where to log errors.</param>
    /// <param name="msgReader">Where to read the messages from.</param>
    /// <param name="suffix">The suffix for the resource, maybe empty.</param>
    /// <returns>The loaded <see cref="LocalizedMessageList" />.</returns>
    private static LocalizedMessageList LoadMessageList(ILogger logger, ILocMessageReader msgReader, string suffix)
    {
        var resourceName = IsNullOrWhiteSpace(suffix) ? "messages" : $"messages-{suffix}";
        var result = msgReader.Read(resourceName);
        return ParseList(logger, resourceName, result);
    }

    /// <summary>
    ///     Parses a JSON string into a <see cref="LocalizedMessageList" />.
    /// </summary>
    /// <param name="logger">Where to log errors.</param>
    /// <param name="resourceName">The resource name that the JSON string was read from.</param>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>
    ///     The parsed <see cref="LocalizedMessageList" /> or an empty LocalizedMessageList if the string could not be
    ///     parsed.
    /// </returns>
    private static LocalizedMessageList ParseList(ILogger logger, string resourceName, string json)
    {
        LocalizedMessageList? lml = null;
        try
        {
            if (!IsNullOrWhiteSpace(json))
                lml = JsonSerializer.Deserialize<LocalizedMessageList>(json, SerializeOptions);
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Cannot parse JSON message resource: '{resourceName}'");
        }

        return lml ?? new LocalizedMessageList();
    }

    /// <summary>
    ///     Looks up the message by key from the language resources and returns it. Formatting any parameters is the
    ///     responsibility of the caller.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The lookup for a message works by looking the most specific set first, then the less specific, then the
    ///         fallback set. So for a system set to en_US, the search is messages-en-US.json, then messages-en.json, then
    ///         messages.json. If no message is found then a default message constructed from the key is returned.
    ///     </para>
    /// </remarks>
    /// <param name="key">The key to lookup.</param>
    /// <returns>The message string.</returns>
    public string Message(string key)
    {
        return !_messages.TryGetValue(key, out var message) ? $"??[[{key}]]??" : message;
    }
}

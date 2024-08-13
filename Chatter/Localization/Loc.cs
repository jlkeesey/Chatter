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
using System.Collections.Generic;
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

    /// <summary>
    ///     Looks up the plural message by key and returns an object that returns the correct string based on the
    ///     input values.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The lookup for a message works by looking the most specific set first, then the less specific, then the
    ///         fallback set. So for a system set to en_US, the search is messages-en-US.json, then messages-en.json, then
    ///         messages.json. If no message is found then a default message constructed from the key is returned.
    ///     </para>
    /// </remarks>
    /// <param name="key">The key to lookup.</param>
    /// <returns>The <c>IMessagePlural</c> object.</returns>
    public IPluralMessage MessagePlural(string key)
    {
        if (_messages.TryGetValue(key, out var message)) return new PluralMessage(key, message, this);
        return new PluralMessageMissing(key);
    }
}

/// <summary>
///     An object that returns the correct message pattern based on a numeric value. The numeric value
///     is compared to each pattern and if it matches, then that pattern is returned. There is always a
///     default pattern that matches all values that is returned if no other value is returned.
/// </summary>
public interface IPluralMessage
{
    /// <summary>
    ///     Returns the formatted string value based on the input values.
    /// </summary>
    /// <param name="arg0">The numeric value used to select the correct pattern.</param>
    /// <param name="args">Any additional values.</param>
    /// <returns>A formatted string.</returns>
    string Format(long arg0, params object[] args);
}

/// <summary>
///     Helper to use when there is no plural definition in the message.json file.
/// </summary>
internal class PluralMessageMissing : IPluralMessage
{
    private readonly string _key;

    internal PluralMessageMissing(string key)
    {
        _key = key;
    }

    public string Format(long arg0, params object[] args)
    {
        return $"??[[plural {arg0}: {_key}]]??";
    }
}

/// <summary>
///     Parses the input pattern string and populates values based on the parsed patterns.
/// </summary>
internal class PluralMessage : IPluralMessage
{
    private readonly SortedList<long, IMessagePattern> _patterns = [];

    internal PluralMessage(string key, string messagePattern, Loc loc)
    {
        var patterns = messagePattern.Split(",");
        foreach (var pattern in patterns)
        {
            var parts = pattern.Split("|");
            if (parts.Length != 2)
            {
                Chatter.Logger?.Error($"Invalid plural pattern for {key}: '{pattern}'");
                _patterns.Add(long.MaxValue, new DefaultMessagePattern(loc.Message(parts[^1])));
            }
            else
            {
                if (parts[0] == "*")
                {
                    _patterns.Add(long.MaxValue, new DefaultMessagePattern(loc.Message(parts[1])));
                }
                else
                {
                    if (long.TryParse(parts[0], out var result))
                    {
                        _patterns.Add(result, new ValueMessagePattern(result, loc.Message(parts[1])));
                    }
                    else
                    {
                        Chatter.Logger?.Error($"Pattern match value must be a valid integer: '{parts[0]}'");
                    }
                }
            }
        }

        if (_patterns.GetValueAtIndex(_patterns.Count - 1) is DefaultMessagePattern) return;

        var message = $"Missing default pattern for key {key}";
        Chatter.Logger?.Error(message);
        _patterns.Add(long.MaxValue, new DefaultMessagePattern(message));
    }

    public string Format(long arg0, params object[] args)
    {
        foreach (var messagePattern in _patterns)
        {
            if (messagePattern.Value.Matches(arg0)) return messagePattern.Value.Format(arg0, args);
        }

        return "??[[no pattern matched]]??";
    }
}

/// <summary>
///     Interface for a pattern piece.
/// </summary>
internal interface IMessagePattern
{
    /// <summary>
    ///     Returns <c>true</c> if the given value matches this pattern.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><c>true</c> if the given value matches this pattern.</returns>
    bool Matches(long value);

    /// <summary>
    ///     Formats this pattern using the given values. The pattern will be formatted regardless
    ///     of whether the <c>Match</c> method returns <c>true</c>.
    /// </summary>
    /// <param name="arg0">The first parameter (required).</param>
    /// <param name="args">The optional remaining parameters.</param>
    /// <returns>The formatted string.</returns>
    string Format(long arg0, params object[] args);
}

/// <summary>
///     Base for handling the formatting of a specific pattern.
/// </summary>
/// <param name="pattern">The pattern that will be formatted.</param>
internal abstract class BaseMessagePattern(string pattern) : IMessagePattern
{
    /// <inheritdoc/>
    public string Format(long arg0, params object[] args)
    {
        return string.Format(pattern, [arg0, .. args,]);
    }

    /// <inheritdoc/>
    public abstract bool Matches(long value);
}

/// <summary>
///     A pattern string that has a single, numeric match value. This will only be used if the
///     input value is exactly equal to the <c>index</c> value.
/// </summary>
/// <param name="index">The specific match value.</param>
/// <param name="pattern">The pattern string to use.</param>
internal class ValueMessagePattern(long index, string pattern) : BaseMessagePattern(pattern)
{
    /// <inheritdoc/>
    public override bool Matches(long value)
    {
        return value == index;
    }
}

/// <summary>
///     A Default pattern string used when no other value matches.
/// </summary>
/// <param name="pattern">The pattern string to use.</param>
internal class DefaultMessagePattern(string pattern) : BaseMessagePattern(pattern)
{
    /// <inheritdoc/>
    public override bool Matches(long value)
    {
        return true;
    }
}

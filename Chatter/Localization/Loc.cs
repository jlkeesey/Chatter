using System;
using System.Globalization;
using System.Text.Json;
using Chatter.System;

namespace Chatter.Localization;

/// <summary>
///     Handles basic message localization.
/// </summary>
public static class Loc
{
    private static LocalizedMessageList _messages = new();

    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private static string? _languageTag;
    private static string? _countryTag;

    /// <summary>
    ///     Returns the number of messages in the message list.
    /// </summary>
    public static int Count => _messages.Messages.Count;

    /// <summary>
    ///     Returns the short language tag e.g. en for English.
    /// </summary>
    private static string LanguageTag
    {
        get { return _languageTag ??= CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }
    }

    /// <summary>
    ///     Returns the short country tag e.g. US for United States.
    /// </summary>
    private static string CountryTag
    {
        get { return _countryTag ??= RegionInfo.CurrentRegion.TwoLetterISORegionName; }
    }

    /// <summary>
    ///     Gets the current full IETF language tag, e.g. en_US for U.S. English.
    /// </summary>
    public static string CultureTag
    {
        get => $"{LanguageTag}-{CountryTag}";
        set
        {
            var parts = value.Split('-');
            if (parts.Length <= 0) return;
            _languageTag = parts[0];
            if (parts.Length > 1) _countryTag = parts[1];
        }
    }

    /// <summary>
    ///     Loads the localized messages for the current Culture and Region. Clears any previously loaded messages.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The are up to 3 messages resources that are read. The first is messages which contains the fallback
    ///         for all messages (it is in en-US as that is where the author is from). The second is the language specific
    ///         resource which is based on the current CultureInfo. The name of the resource is messages-LL where LL is
    ///         the two letter language code e.g. en for English. The third is the culture specific resource which is
    ///         based on the current <see cref="CultureInfo" /> and <see cref="RegionInfo" />. The name of the resource is
    ///         messages-LL-CC where LL is the two letter language code as above and CC is the two letter country code e.g. US
    ///         for United States.
    ///     </para>
    /// </remarks>
    /// <param name="logger">Where to write debug and error messages.</param>
    public static void Load(ILogger logger)
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
    public static void Load(ILogger logger, ILocMessageReader msgReader)
    {
        _messages = LoadMessageList(logger, msgReader, string.Empty);
        var shortLanguage = LanguageTag;
        var languageMessages = LoadMessageList(logger, msgReader, shortLanguage);
        _messages.Merge(languageMessages);

        var regionalLanguage = CultureTag;
        var regionalLanguageMessages = LoadMessageList(logger, msgReader, regionalLanguage);
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
        var resourceName = string.IsNullOrWhiteSpace(suffix) ? "messages" : $"messages-{suffix}";
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
            lml = JsonSerializer.Deserialize<LocalizedMessageList>(json, SerializeOptions);
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Cannot parse JSON message resource: '{resourceName}'");
        }

        return lml ?? new LocalizedMessageList();
    }

    /// <summary>
    ///     Looks up the message by key from the language resources and returns it formatted with the given arguments.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The lookup for a message works by looking the most specific set first, then the less specific, then the
    ///         fallback set. So for a system set to en_US, the search is messages-en-US.json, then messages-en.json, then
    ///         messages.json. If no message is found then a default message constructed from the key is returned.
    ///     </para>
    /// </remarks>
    /// <param name="key">The key to lookup.</param>
    /// <param name="args">The optional arguments for formatting the string.</param>
    /// <returns>The formatter message string.</returns>
    public static string Message(string key, params object[] args)
    {
        if (!_messages.TryGetValue(key, out var message)) message = $"??[[{key}]]??";

        return string.Format(message, args);
    }
}
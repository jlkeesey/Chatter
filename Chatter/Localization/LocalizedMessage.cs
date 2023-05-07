using System.Text.Json.Serialization;

namespace Chatter.Localization;

/// <summary>
///     A localized (translated) message and it's metadata.
/// </summary>
public struct LocalizedMessage
{
    public LocalizedMessage(string key, string? message = null, string? description = null)
    {
        Key = key;
        Message = message ?? "??[key]??";
#if DEBUG
        Description = description ?? string.Empty;
#else
        Description = string.Empty; // We don't need the description at runtime
#endif
    }

    /// <summary>
    ///     The key used to lookup this value. This key will be the same for all localized variations of this message.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     The localized message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     A description of the purpose of this message, i.e. how it is actually used to help with the translation.
    /// </summary>
    /// <remarks>
    ///     This field is never loaded into the application, it is only used by the translators.
    /// </remarks>
    [JsonIgnore]
    public string Description { get; set; }
}
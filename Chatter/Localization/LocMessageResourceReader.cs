using Chatter.Properties;
using System.IO;
using static System.String;

namespace Chatter.Localization;

/// <summary>
///     Implementation of <see cref="ILocMessageReader" /> that reads from the assembly resources.
/// </summary>
internal class LocMessageResourceReader : ILocMessageReader
{
    /// <inheritdoc />
    public string Read(string name)
    {
        if (Resources.ResourceManager.GetObject(name) is not byte[] content) return Empty;
        using var stream = new MemoryStream(content);
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();
        return result;
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Logging;

namespace Chatter;

/// <summary>
///     Helpers for dealing with the private user area characters that FFXIV uses.
/// </summary>
/// <remarks>
///     The Unicode block from U+E000..U+F8FF is a private user area where applications can install their
///     own characters. FFXIV uses this area for special characters. As such none of these characters
///     will display properly in any situation other than inside of FFXIV.
/// </remarks>
public static class SeSpecialCharacters
{
    /// <summary>
    ///     Provides reasonable replacements for some of the special characters that FFXIV uses into
    ///     well-defined Unicode characters.
    /// </summary>
    private static readonly Dictionary<char, string> SpecialCharacterMap = new()
    {
        {'\uE03C', "\u2747"}, // Flower separator between names and worlds
        {'\uE040', "["}, // Open double arrow used to surround translatable words
        {'\uE041', "]"}, // Close double arrow used to surround translatable words
        {'\uE049', "\u25CB"}, // D-controller circle button
        {'\uE04A', "\u25FB"}, // D-controller square button
        {'\uE04B', "\u2716"}, // D-controller x button
        {'\uE04C', "\u2795"}, // D-controller triangle button
        {'\uE04D', "\u25B3"}, // D-controller + button
        {'\uE05D', "\u273F"}, // Flower
        {'\uE06D', "\u33C2"}, // AM
        {'\uE06E', "\u33D8"}, // PM
        {'\uE071', "\u24B6"}, // Letter A
        {'\uE072', "\u24B7"}, // Letter B
        {'\uE073', "\u24B8"}, // Letter C
        {'\uE074', "\u24B9"}, // Letter D
        {'\uE075', "\u24BA"}, // Letter E
        {'\uE076', "\u24BB"}, // Letter F
        {'\uE077', "\u24BC"}, // Letter G
        {'\uE078', "\u24BD"}, // Letter H
        {'\uE079', "\u24BE"}, // Letter I
        {'\uE07A', "\u24BF"}, // Letter J
        {'\uE07B', "\u24C0"}, // Letter K
        {'\uE07C', "\u24C1"}, // Letter L
        {'\uE07D', "\u24C2"}, // Letter M
        {'\uE07E', "\u24C3"}, // Letter N
        {'\uE07F', "\u24C4"}, // Letter O
        {'\uE080', "\u24C5"}, // Letter P
        {'\uE081', "\u24C6"}, // Letter Q
        {'\uE082', "\u24C7"}, // Letter R
        {'\uE083', "\u24C8"}, // Letter S
        {'\uE084', "\u24C9"}, // Letter T
        {'\uE085', "\u24CA"}, // Letter U
        {'\uE086', "\u24CB"}, // Letter V
        {'\uE087', "\u24CC"}, // Letter W
        {'\uE088', "\u24CD"}, // Letter X
        {'\uE089', "\u24CE"}, // Letter Y
        {'\uE08A', "\u24CF"}, // Letter Z
        {'\uE08F', "\u24EA"}, // Number 0
        {'\uE090', "\u2460"}, // Number 1
        {'\uE091', "\u2461"}, // Number 2
        {'\uE092', "\u2462"}, // Number 3
        {'\uE093', "\u2463"}, // Number 4
        {'\uE094', "\u2464"}, // Number 5
        {'\uE095', "\u2465"}, // Number 6
        {'\uE096', "\u2466"}, // Number 7
        {'\uE097', "\u2467"}, // Number 8
        {'\uE098', "\u2468"}, // Number 9
        {'\uE099', "\u2469"}, // Number 10
        {'\uE09A', "\u246A"}, // Number 11
        {'\uE09B', "\u246B"}, // Number 12
        {'\uE09C', "\u246C"}, // Number 13
        {'\uE09D', "\u246D"}, // Number 14
        {'\uE09E', "\u246E"}, // Number 15
        {'\uE09F', "\u246F"}, // Number 16
        {'\uE0A0', "\u2470"}, // Number 17
        {'\uE0A1', "\u2471"}, // Number 18
        {'\uE0A2', "\u2472"}, // Number 19
        {'\uE0A3', "\u2473"}, // Number 20
        {'\uE0AF', "\u2A01"}, // Plus in filled square
        {'\uE0BB', "\u02A2"}, // Filled arrow in front of links
        {'\uE0BC', "\u233D"}, // Swoop in square
        {'\uE0BD', "\u29F3"}, // Swoop in filled square
        {'\uE0BE', "\u2B07"}, // Down arrow in filled square
        {'\uE0BF', "\u274E"}, // X in filled square rotated
        {'\uE0C0', "\u272A"}, // Star in filled square
        {'\uE0C1', "\u2160"}, // Roman numeral I
        {'\uE0C2', "\u2161"}, // Roman numeral II
        {'\uE0C3', "\u2162"}, // Roman numeral III
        {'\uE0C4', "\u2163"}, // Roman numeral IV
        {'\uE0C5', "\u2164"}, // Roman numeral V
        {'\uE0C6', "\u2165"}, // Roman numeral VI
    };

    public static bool IsSpecial(char ch)
    {
        return ch is >= '\uE000' and <= '\uF8FF';
    }

    /// <summary>
    ///     Removes all special characters from the given string. Contrast this with <see cref="Replace" />
    ///     which replaces the special characters with suitable alternates.
    /// </summary>
    /// <param name="s">The string to process.</param>
    /// <returns>The processed string.</returns>
    public static string Clean(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s.Where(ch => !IsSpecial(ch))) sb.Append(ch);
        return sb.ToString();
    }

    /// <summary>
    ///     Returns the given string with all special characters replaced.
    /// </summary>
    /// <param name="s">The string to process.</param>
    /// <returns>The processed string.</returns>
    public static string Replace(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s)
            sb.Append(Replacement(ch));
        return sb.ToString();
    }

    /// <summary>
    ///     Provides a reasonable replacement for one of the FFXIV special characters. All replacements will be
    ///     well-defined Unicode characters. If there is no defined replacement or if the character is not a
    ///     special character, it is returned unchanged.
    /// </summary>
    /// <remarks>
    ///     Because the special characters are, well, special, there may not be a single character replacement that works, so
    ///     the replacement is a string.
    /// </remarks>
    /// <param name="ch">The character to replace.</param>
    /// <returns>The replacement string.</returns>
    public static string Replacement(char ch)
    {
        if (IsSpecial(ch))
        {
            if (SpecialCharacterMap.TryGetValue(ch, out var value))
                return value;
#if DEBUG
            PluginLog.Debug("Unhandled FFXIV character: (\\u{0:X4})", (int) ch);
#endif
        }

        return ch.ToString();
    }
}
﻿// Copyright 2023 James Keesey
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

using Chatter.Model;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chatter.Chat;

/// <summary>
///     A string from the chat system.
/// </summary>
/// <remarks>
///     Dalamud pass around lists of objects for string called <see cref="SeString" /> which corresponds to the stream
///     of data passed by FFXIV for stringy objects. These lists allow for non-text items to be placed into the stream
///     to add to the metadata sent e.g. icons, formatting, etc. We use <c>ChatString</c> as a container for the string
///     parts with the behaviors that we need for handle chat messages and users (both of which use the same string
///     structure).
/// </remarks>
public sealed class ChatString
{
    private readonly List<CsItem> _items = new();

    public ChatString(CsItem item)
    {
        _items.Add(item);
    }

    /// <summary>
    ///     Creates a ChatString from an <see cref="SeString" />. The <see cref="SeString" /> is parsed and converted to the
    ///     items that we need for processing chat logs.
    /// </summary>
    /// <param name="seString">The <see cref="SeString" /> to process.</param>
    public ChatString(SeString seString)
    {
        var nameState = NameState.Nothing;
        CsPlayerItem? player = null;

        var payloads = seString.Payloads;
        var count = payloads.Count;
        for (var i = 0; i < count; i++)
        {
            var payload = payloads[i];

            switch (payload)
            {
                case PlayerPayload p:
                    player = new CsPlayerItem(p.PlayerName, p.World.Value.Name.ExtractText());
                    _items.Add(player);
                    nameState = NameState.LookingForName;
                    break;
                case AutoTranslatePayload atp:
                    var atpText = atp.Text;
                    if (!string.IsNullOrWhiteSpace(atpText)) _items.Add(new CsTextItem(atpText));
                    nameState = NameState.Nothing;
                    player = null;
                    break;
                case TextPayload text:
                    var str = text.Text ?? string.Empty;
                    switch (nameState)
                    {
                        case NameState.LookingForName:
                            {
                                nameState = NameState.Nothing;
                                // EndsWith to account for special characters in front of name
                                if (str == player!.Name || str.EndsWith(player.Name))
                                {
                                    nameState = NameState.LookingForWorld;
                                    continue;
                                }

                                break;
                            }
                        case NameState.LookingForWorld:
                            {
                                nameState = NameState.Nothing;
                                if (str.StartsWith(player!.World)) str = str[player.World.Length..];
                                player = null;
                                break;
                            }
                        case NameState.Nothing:
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(str)) _items.Add(new CsTextItem(str));
                    break;
            }
        }
    }

    /// <summary>
    ///     Returns a <see cref="ChatString" /> that contains the given player.
    /// </summary>
    /// <param name="player">The <see cref="IPlayer" /> to enclose.</param>
    /// <returns>The <see cref="ChatString" />.</returns>
    public static ChatString FromPlayer(IPlayer player)
    {
        return new ChatString(new CsPlayerItem(player.Name, player.HomeWorld.Name));
    }

    /// <summary>
    ///     Returns a <see cref="ChatString" /> that contains the given string.
    /// </summary>
    /// <param name="text">The text to enclose.</param>
    /// <returns>The <see cref="ChatString" />.</returns>
    public static ChatString FromText(string text)
    {
        return new ChatString(new CsTextItem(text));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var text in _items.Select(item => item.ToString())) sb.Append(text);
        return sb.ToString();
    }

    /// <summary>
    ///     Returns <c>true</c> if the first item in the list is a <see cref="CsPlayerItem" />.
    /// </summary>
    /// <returns><c>true</c> if the first item in the list is a <see cref="CsPlayerItem" />.</returns>
    public bool HasInitialPlayer()
    {
        return _items.Count > 0 && _items[0] is CsPlayerItem;
    }

    /// <summary>
    ///     Returns the first item if it is a <see cref="CsPlayerItem" /> or a newly created <see cref="CsPlayerItem" /> from
    ///     the given information.
    /// </summary>
    /// <param name="player">The player name to use if we cannot find a player item.</param>
    /// <param name="world">The  world name to use if we cannot find a player item.</param>
    /// <returns>A <see cref="CsPlayerItem" />.</returns>
    public CsPlayerItem GetInitialPlayerItem(string player, string world)
    {
        if (_items.Count > 0 && _items[0] is CsPlayerItem cpi) return cpi;

        return new CsPlayerItem(player, world);
    }

    /// <summary>
    ///     Returns the text representation of all the items concatenated under control of the given configuration.
    /// </summary>
    /// <param name="includeServer"><c>true</c> if the server/world should be in the output.</param>
    /// <returns>The <c>string</c> value of this string.</returns>
    public string AsText(bool includeServer)
    {
        var sb = new StringBuilder();
        foreach (var text in _items.Select(item => item.AsText(includeServer))) sb.Append(text);
        return sb.ToString();
    }

    /// <summary>
    ///     The states of looking for a player's name and world in the text stream.
    /// </summary>
    private enum NameState
    {
        /// <summary>
        ///     Normal state, not doing any special processing.
        /// </summary>
        Nothing,

        /// <summary>
        ///     We've seen a <see cref="PlayerPayload" /> which means we are waiting for the textual version of the name in the
        ///     stream, so we can remove it.
        /// </summary>
        LookingForName,

        /// <summary>
        ///     We've seen the player name in the stream so wea now looking for the world name to appear if it does.
        /// </summary>
        LookingForWorld,
    }

    /// <summary>
    ///     Base class for all items that appear in a ChatString.
    /// </summary>
    public abstract class CsItem
    {
        /// <summary>
        ///     Returns the text value of this item based on the given configuration.
        /// </summary>
        /// <param name="includeServer"><c>true</c> if the server/world should be in the output.</param>
        /// <returns>The <c>string</c> value of this item.</returns>
        public abstract string AsText(bool includeServer);
    }

    /// <summary>
    ///     Represents a player's info in the stream which consists of their name and home world.
    /// </summary>
    public class CsPlayerItem(string name, string world) : CsItem
    {
        public string Name { get; } = SeSpecialCharacters.Clean(name);
        public string World { get; } = world;

        public override string ToString()
        {
            return $"{Name}@{World}";
        }

        /// <summary>
        ///     Returns the player's name with their home world optional appended. This is controlled by the configuration.
        /// </summary>
        /// <param name="includeServer"><c>true</c> if the server/world should be in the output.</param>
        /// <returns>The <c>string</c> value of this item.</returns>
        public override string AsText(bool includeServer)
        {
            return includeServer ? $"{Name}@{World}" : Name;
        }
    }

    /// <summary>
    ///     Represents a text sequence in the stream. FFXIV special characters will be handled.
    /// </summary>
    public class CsTextItem(string text) : CsItem
    {
        private string Text { get; } = SeSpecialCharacters.Replace(text);

        public override string ToString()
        {
            return Text;
        }

        /// <summary>
        ///     Returns the value of this item as text for appending into a single string.
        /// </summary>
        /// <param name="includeServer"><c>true</c> if the server/world should be in the output.</param>
        /// <returns>The <c>string</c> value of this item.</returns>
        public override string AsText(bool includeServer)
        {
            return Text;
        }
    }
}

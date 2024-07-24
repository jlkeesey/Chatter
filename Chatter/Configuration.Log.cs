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
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS �AS IS�
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
using Dalamud.Game.Text;
using System.Collections.Generic;

namespace Chatter;

/// <summary>
///     Contains all the user configuration settings.
/// </summary>
public partial class Configuration
{
    /// <summary>
    ///     The configuration for a single chat log.
    /// </summary>
    public sealed class ChatLogConfiguration
    {
        public bool IsAll => Name == AllLogName;

        /// <summary>
        ///     The include/exclude flags for each <see cref="XivChatType" />.
        /// </summary>
        public readonly Dictionary<XivChatType, ChatTypeFlag> ChatTypeFilterFlags = new();

        /// <summary>
        ///     The format string for formatting the timestamp of a chat message. If not specified, we use the
        ///     <see cref="DateHelper.CultureDateTimePattern" />.
        /// </summary>
        public string? DateTimeFormat;

        /// <summary>
        ///     When <c>true</c> all messages get written to this log including ones that normally would be filtered out. This is
        ///     different from the <see cref="IncludeAllUsers" /> in that it will include all messages from all users it will also
        ///     include all messages of all chat types.
        /// </summary>
        public bool DebugIncludeAllMessages;

        /// <summary>
        ///     The format string to use for formatting the messages.
        /// </summary>
        /// <remarks>
        ///     Every message will use this format pattern to format the message that is logged. If this is null then the default
        ///     depends on the type of chat log. Below is the list of replacement parameters that are permitted.
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Replacement</term>
        ///             <description>Description</description>
        ///         </listheader>
        ///         <item>
        ///             <term>{0}</term>
        ///             <description>The long chat type name.</description>
        ///         </item>
        ///         <item>
        ///             <term>{1}</term>
        ///             <description>The short chat type name.</description>
        ///         </item>
        ///         <item>
        ///             <term>{2}</term>
        ///             <description>
        ///                 The long sender name. This will include the world name if different from the user. This is
        ///                 without any name replacement.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>{3}</term>
        ///             <description>The short sender name. This may have the world remove and will have any replacements done.</description>
        ///         </item>
        ///         <item>
        ///             <term>{4}</term>
        ///             <description>
        ///                 The sender and chat type. The is the short sender name with the short chat type appended,
        ///                 separated by a space. Equivalent to <c>{3} {1}</c>.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>{5}</term>
        ///             <description>
        ///                 The cleaned text of the chat message. This may have the world names removed and will have any
        ///                 name replacements.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public string? Format;

        /// <summary>
        ///     If <c>true</c> the all users will be included even if they are not in the user list. This will always be
        ///     <c>true</c> for the
        ///     all user.
        /// </summary>
        public bool IncludeAllUsers;

        /// <summary>
        ///     If <c>true</c> then I am included in the log even if I'm not in the user list. This will generally be <c>true</c>
        ///     always.
        /// </summary>
        public bool IncludeMe;

        /// <summary>
        ///     If this is <c>true</c> then server names are included in the output, otherwise they are stripped from
        ///     the output, both in the name column as well as the message.
        /// </summary>
        public bool IncludeServer;

        /// <summary>
        ///     Whether this log is active and writing out to the file.
        /// </summary>
        public bool IsActive;

        /// <summary>
        ///     How many spaces to indent any message lines that wrap. Anything less than 0 means no indentation.
        /// </summary>
        public int MessageWrapIndentation;

        /// <summary>
        ///     How wide to wrap messages in characters. Anything less than or equal to 0 means no wrapping.
        /// </summary>
        public int MessageWrapWidth;

        public ChatLogConfiguration(string name,
                                    bool isActive = false,
                                    bool includeServer = false,
                                    bool includeMe = true,
                                    bool includeAllUsers = false,
                                    bool includeAllMessages = false,
                                    int wrapColumn = 0,
                                    int wrapIndent = 0,
                                    string? format = null)
        {
            Name = name;
            IsActive = isActive;
            IncludeServer = includeServer;
            IncludeMe = includeMe;
            IncludeAllUsers = includeAllUsers;
            DebugIncludeAllMessages = includeAllMessages;
            MessageWrapWidth = wrapColumn;
            MessageWrapIndentation = wrapIndent;
            Format = format;
            InitializeTypeFlags();
        }

        /// <summary>
        ///     The name of this group. Will be part of the log file name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The set of users to include.
        /// </summary>
        /// <remarks>
        ///     The key is the full name of the user to include, if the value is not <see cref="string.Empty" />, it is what the
        ///     user's
        ///     name should be renamed in the output. If this is empty, then all users are included.
        /// </remarks>
        public SortedDictionary<string, string> Users { get; } = new();

        /// <summary>
        ///     Makes sure that paired flags are kept in sync.
        /// </summary>
        public void SyncFlags()
        {
            ChatTypeFilterFlags[XivChatType.TellOutgoing] = ChatTypeFilterFlags[XivChatType.TellIncoming];
            ChatTypeFilterFlags[XivChatType.CustomEmote] = ChatTypeFilterFlags[XivChatType.StandardEmote];
        }

        /// <summary>
        ///     Initializes the enabled chat type flags.
        /// </summary>
        /// <remarks>
        ///     This has two purposes. The first is to set the default flags for this object. The second is to
        ///     set any new chat types that did not exist is previously created configuration. Serialization will
        ///     do that to you.
        /// </remarks>
        public void InitializeTypeFlags()
        {
            foreach (var type in DefaultEnabledTypes) ChatTypeFilterFlags.TryAdd(type, new ChatTypeFlag(true));
        }

        public class ChatTypeFlag
        {
            public bool Value;

            public ChatTypeFlag(bool value = false)
            {
                Value = value;
            }
        }
    }
}

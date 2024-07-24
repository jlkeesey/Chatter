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

using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Chatter.System;

namespace Chatter;

/// <summary>
///     Contains all the user configuration settings.
/// </summary>
[Serializable]
public partial class Configuration : IPluginConfiguration
{
    /// <summary>
    ///     The name of the log that saves all messages aka the all log.
    /// </summary>
    public const string AllLogName = "all";

    /// <summary>
    ///     The directory to write all logs to. This directory may not exist.
    /// </summary>
    public string LogDirectory = string.Empty;

    /// <summary>
    ///     The prefix for the logs. This will be the first part of all log file names.
    /// </summary>
    public string LogFileNamePrefix = "chatter";

    /// <summary>
    ///     The string representation of a <see cref="LocalTime" /> when a log file will be closed so a new one
    ///     can be opened. Empty or an invalid string will be treated as 06:00 local time.
    /// </summary>
    public string WhenToCloseLogs = string.Empty;

    /// <summary>
    ///     Specifies the order of the parts in a log file name.
    /// </summary>
    public enum FileNameOrder
    {
        /// <summary>
        ///     Placeholder for not set, will be interpreted the same as <see cref="FileNameOrder.PrefixGroupDate" />.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Log file names are in the form 'chatter-all-20230204-123456.log'
        /// </summary>
        PrefixGroupDate,

        /// <summary>
        ///     Log file names are in the form 'chatter-20230204-123456-all.log'
        /// </summary>
        PrefixDateGroup,
    }

    /// <summary>
    ///     Controls the order of the parts in a log file name.
    /// </summary>
    public FileNameOrder LogOrder { get; set; } = FileNameOrder.PrefixGroupDate;

    /// <summary>
    ///     The configurations for the individual chat logs.
    /// </summary>
    public SortedDictionary<string, ChatLogConfiguration> ChatLogs = new(new LogComparer());

    private class LogComparer : IComparer<string>
    {
        private readonly IComparer<string> _comparer = StringComparer.CurrentCultureIgnoreCase;

        public int Compare(string? lhs, string? rhs)
        {
            if (lhs == null)
            {
                if (rhs == null)
                    return 0;
                else
                    return -1;
            }

            if (rhs == null) return 1;

            var lhsAll = _comparer.Compare("all", lhs);
            var rhsAll = _comparer.Compare("all", rhs);
            if (lhsAll == 0)
            {
                if (rhsAll == 0)
                    return 0;
                else
                    return -1;
            }

            return rhsAll == 0 ? 1 : _comparer.Compare(lhs, rhs);
        }
    }

#if DEBUG
    public bool IsDebug = true;
#else
    public bool IsDebug = false;
#endif

    public int Version { get; set; } = 2;

    /// <summary>
    ///     Adds a log configuration.
    /// </summary>
    /// <param name="logConfiguration">The <see cref="Configuration.ChatLogConfiguration" /> to add.</param>
    public void AddLog(ChatLogConfiguration logConfiguration)
    {
        ChatLogs[logConfiguration.Name] = logConfiguration;
    }

    /// <summary>
    ///     Removes a log configuration.
    /// </summary>
    /// <param name="logConfiguration">The <see cref="Configuration.ChatLogConfiguration" /> to remove.</param>
    public void RemoveLog(ChatLogConfiguration logConfiguration)
    {
        ChatLogs.Remove(logConfiguration.Name);
    }

    /// <summary>
    ///     Removes all the logs from the configuration.
    /// </summary>
    public void RemoveAllLogs()
    {
        ChatLogs.Clear();
    }

    /// <summary>
    ///     Saves the current state of the configuration.
    /// </summary>
    public void Save()
    {
        _pluginInterface?.SavePluginConfig(this);
    }

    /// <summary>
    ///     Loads the most recently saved configuration or creates a new one.
    /// </summary>
    /// <returns>The configuration to use.</returns>
    public static Configuration Load(ILogger logger, IDalamudPluginInterface pluginInterface, FileHelper fileHelper)
    {
        // var config = new Configuration();
        // pluginInterface.SavePluginConfig(config);
        logger.Debug("@@@@ Loading config ...");
        if (pluginInterface.GetPluginConfig() is not Configuration config)
        {
            logger.Debug("@@@@ Creating new config");
            config = new Configuration();
        }

        config._pluginInterface = pluginInterface;

#if DEBUG
        config.InitializeForDebug();
#endif
        config.Initialize(fileHelper);

        config.Save();
        return config;
    }

    public void Initialize(FileHelper fileHelper)
    {
        if (string.IsNullOrWhiteSpace(LogDirectory)) LogDirectory = fileHelper.InitialLogDirectory();

        if (!ChatLogs.ContainsKey(AllLogName))
            AddLog(new ChatLogConfiguration(AllLogName, true, includeAllUsers: true));

        foreach (var (_, chatLogConfiguration) in ChatLogs) chatLogConfiguration.InitializeTypeFlags();
    }

#if DEBUG
    public void InitializeForDebug()
    {
        // ReSharper disable StringLiteralTypo
        if (!ChatLogs.ContainsKey("Frollo"))
        {
            var logConfiguration = new ChatLogConfiguration("Frollo", true)
            {
                Users =
                {
                    ["Pierre Gringoire@Zalera"] = string.Empty,
                    ["Phoebus Chateaupers@Zalera"] = "Stud Muffin",
                    ["Quasimodo Curveback@Zalera"] = "The Oppressed",
                },
            };
            AddLog(logConfiguration);
        }

        if (!ChatLogs.ContainsKey("Pimpernel"))
        {
            AddLog(new ChatLogConfiguration("Pimpernel", true, wrapColumn: 60, wrapIndent: 54, includeAllUsers: true));
        }
        // ReSharper restore StringLiteralTypo
    }
#endif

    /// <summary>
    ///     These chat type should all be enabled by default on new configurations.
    /// </summary>
    private static readonly List<XivChatType> DefaultEnabledTypes =
    [
        XivChatType.Say, XivChatType.TellOutgoing, XivChatType.TellIncoming, XivChatType.Shout, XivChatType.Party,
        XivChatType.Alliance, XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4, XivChatType.Ls5,
        XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.FreeCompany, XivChatType.NoviceNetwork,
        XivChatType.CustomEmote, XivChatType.StandardEmote, XivChatType.Yell, XivChatType.CrossParty,
        XivChatType.PvPTeam, XivChatType.CrossLinkShell1, XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3,
        XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6,
        XivChatType.CrossLinkShell7, XivChatType.CrossLinkShell8,
    ];

    [JsonIgnore] private IDalamudPluginInterface? _pluginInterface;
}

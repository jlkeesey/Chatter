using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Text;

namespace Chatter;

/// <summary>
///     Contains all of the user configuration settings.
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
    public string LogDirectory = FileHelper.InitialLogDirectory();

    /// <summary>
    ///     The prefix for the logs. This will be the first part of all log file names.
    /// </summary>
    public string LogFileNamePrefix = "chatter";

    /// <summary>
    /// Specifies the order of the parts in a log file name.
    /// </summary>
    public enum FileNameOrder
    {
        /// <summary>
        /// Placeholder for not set, will be interpreted the same as <see cref="FileNameOrder.PrefixGroupDate"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// Log file names are in the form 'chatter-all-20230204-123456.log'
        /// </summary>
        PrefixGroupDate,

        /// <summary>
        /// Log file names are in the form 'chatter-20230204-123456-all.log'
        /// </summary>
        PrefixDateGroup,
    }

    /// <summary>
    /// Controls the order of the parts in a log file name.
    /// </summary>
    public FileNameOrder LogOrder { get; set; } = FileNameOrder.PrefixGroupDate;

    /// <summary>
    ///     The configurations for the individual chat logs.
    /// </summary>
    public Dictionary<string, Configuration.ChatLogConfiguration> ChatLogs = new();

#if DEBUG
    public bool IsDebug = true;
#else
    public bool IsDebug = false;
#endif

    public int Version { get; set; } = 1;

    /// <summary>
    /// Adds a log configuration.
    /// </summary>
    /// <param name="logConfiguration">The <see cref="Configuration.ChatLogConfiguration"/> to add.</param>
    public void AddLog(Configuration.ChatLogConfiguration logConfiguration)
    {
        ChatLogs[logConfiguration.Name] = logConfiguration;
    }

    /// <summary>
    /// Removes a log configuration.
    /// </summary>
    /// <param name="logConfiguration">The <see cref="Configuration.ChatLogConfiguration"/> to remove.</param>
    public void RemoveLog(Configuration.ChatLogConfiguration logConfiguration)
    {
        ChatLogs.Remove(logConfiguration.Name);
    }

    /// <summary>
    ///     Saves the current state of the configuration.
    /// </summary>
    public void Save()
    {
        Dalamud.PluginInterface.SavePluginConfig(this);
    }

    /// <summary>
    ///     Loads the most recently saved configuration or creates a new one.
    /// </summary>
    /// <returns>The configuration to use.</returns>
    public static Configuration Load()
    {
        // var config = new Configuration();
        // Dalamud.PluginInterface.SavePluginConfig(config);
        if (Dalamud.PluginInterface.GetPluginConfig() is not Configuration config) config = new Configuration();

        if (!config.ChatLogs.ContainsKey(AllLogName))
            config.AddLog(new Configuration.ChatLogConfiguration(AllLogName, true, includeAllUsers: true, format: "{2}:{0}:{5}"));

#if DEBUG
        // ReSharper disable StringLiteralTypo
        // TODO remove before shipping
        var logConfiguration = new ChatLogConfiguration("Tifaa", true)
        {
            Users =
            {
                ["Tifaa Sidrasylan@Zalera"] = string.Empty,
                ["Aelym Sidrasylan@Zalera"] = "Stud Muffin",
                ["Fiora Greyback@Zalera"] = "The Oppressed",
            },
        };
        config.AddLog(logConfiguration);
        config.AddLog(new ChatLogConfiguration("Pups", true, wrapColumn: 60, wrapIndent: 54));
        config.AddLog(new ChatLogConfiguration("Goobtube"));
        // ReSharper restore StringLiteralTypo
#endif

        foreach (var (_, chatLogConfiguration) in config.ChatLogs) chatLogConfiguration.InitializeTypeFlags();

        config.Save();
        return config;
    }


    /// <summary>
    ///     These chat type should all be enabled by default on new configurations.
    /// </summary>
    private static readonly List<XivChatType> DefaultEnabledTypes = new()
    {
        XivChatType.Say,
        XivChatType.TellOutgoing,
        XivChatType.TellIncoming,
        XivChatType.Shout,
        XivChatType.Party,
        XivChatType.Alliance,
        XivChatType.Ls1,
        XivChatType.Ls2,
        XivChatType.Ls3,
        XivChatType.Ls4,
        XivChatType.Ls5,
        XivChatType.Ls6,
        XivChatType.Ls7,
        XivChatType.Ls8,
        XivChatType.FreeCompany,
        XivChatType.NoviceNetwork,
        XivChatType.CustomEmote,
        XivChatType.StandardEmote,
        XivChatType.Yell,
        XivChatType.CrossParty,
        XivChatType.PvPTeam,
        XivChatType.CrossLinkShell1,
        XivChatType.CrossLinkShell2,
        XivChatType.CrossLinkShell3,
        XivChatType.CrossLinkShell4,
        XivChatType.CrossLinkShell5,
        XivChatType.CrossLinkShell6,
        XivChatType.CrossLinkShell7,
        XivChatType.CrossLinkShell8,
    };
}
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Dalamud.Utility;

namespace Chatter;

/// <summary>
///     Handles the text commands for this plugin.
/// </summary>
public sealed partial class Chatter
{
    private const string CommandChatter = "/chatter";
    private const string CommandDebug = "/chatterdebug";

    private const string DebugChatDump = "chatdump";
    private const string DebugList = "list";

    private readonly Dictionary<string, CommandInfo> _commands = new();

    private readonly Dictionary<string, Func<bool>> _debugFlags = new()
    {
        {"debug", () => Chatter.Configuration.IsDebug},
    };

    /// <summary>
    ///     Registers all of the text commands with the Dalamud plugin environment.
    /// </summary>
    private void RegisterCommands()
    {
        _commands[CommandChatter] = new CommandInfo(OnChatterConfig)
        {
            HelpMessage = "Opens the Chatter configuration window.",
            ShowInHelp = true,
        };

        _commands[CommandDebug] = new CommandInfo(OnChatterDebug)
        {
            HelpMessage = "Executes debug commands",
            ShowInHelp = false,
        };

        foreach (var (command, info) in _commands) Dalamud.Commands.AddHandler(command, info);
    }

    /// <summary>
    ///     Unregisters all the text commands from the Dalamud plugin environment.
    /// </summary>
    private void UnregisterCommands()
    {
        foreach (var command in _commands.Keys) Dalamud.Commands.RemoveHandler(command);
    }

    /// <summary>
    ///     Handles the <c>/chatter</c> text command. This just toggles the configuration window.
    /// </summary>
    /// <param name="command">The text of the command (in case of aliases).</param>
    /// <param name="arguments">Any arguments to the command.</param>
    private void OnChatterConfig(string command, string arguments)
    {
        _windowManager.ToggleConfig();
    }

    // ReSharper disable once CommentTypo
    /// <summary>
    ///     Handles the <c>/chatterdebug</c> text command.
    /// </summary>
    /// <param name="command">The text of the command (in case of aliases).</param>
    /// <param name="arguments">Any arguments to the command.</param>
    private void OnChatterDebug(string command, string arguments)
    {
        if (arguments.IsNullOrEmpty())
        {
            Chatter.Configuration.IsDebug = !Chatter.Configuration.IsDebug;
            PluginLog.Debug($"Debug mode is {(Chatter.Configuration.IsDebug ? "on" : "off")}");
            PluginLog.Debug("");
            PluginLog.Debug($"Sub-commands are: {DebugChatDump}, {DebugList}");
        }
        else
        {
            var args = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var debugCommand = args[0].ToLower();
            switch (debugCommand)
            {
                case DebugChatDump:
                    ChatLogManager.DumpLogs();
                    break;
                case DebugList:
                    ListDebugFlags();
                    break;
                default:
                    PluginLog.Debug($"Debug command not recognized: '{debugCommand}'");
                    break;
            }
        }

        // <summary>
        //     Handles the list debug flags sub command.
        // </summary>
        void ListDebugFlags()
        {
            var length = _debugFlags.Keys.Select(x => x.Length).Max();
            var flagString = "Flag".PadRight(length);
            var flagUnderscores = new string('-', length);
            PluginLog.Debug($"{flagString}  on/off");
            PluginLog.Debug($"{flagUnderscores}  ------");
            foreach (var (name, func) in _debugFlags) ListDebugFlag(name, func(), length);
        }

        // <summary>
        //     Lists one debug flag.
        // </summary>
        void ListDebugFlag(string name, bool value, int length)
        {
            var onOff = value ? "on" : "off";
            PluginLog.Debug($"{name.PadRight(length)}  {onOff}");
        }
    }
}
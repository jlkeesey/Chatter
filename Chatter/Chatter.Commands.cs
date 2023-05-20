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

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;

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
    private readonly Dictionary<string, Func<bool>> _debugFlags = new();

    /// <summary>
    ///     Registers all of the text commands with the Dalamud plugin environment.
    /// </summary>
    private void RegisterCommands()
    {
        _debugFlags.TryAdd("debug", () => _configuration.IsDebug);

        _commands[CommandChatter] = new CommandInfo(OnChatterConfig)
        {
            HelpMessage = "Opens the Chatter configuration window.", ShowInHelp = true,
        };

        _commands[CommandDebug] = new CommandInfo(OnChatterDebug)
        {
            HelpMessage = "Executes debug commands", ShowInHelp = false,
        };

        foreach (var (command, info) in _commands) _commandManager.AddHandler(command, info);
    }

    /// <summary>
    ///     Unregisters all the text commands from the Dalamud plugin environment.
    /// </summary>
    private void UnregisterCommands()
    {
        foreach (var command in _commands.Keys) _commandManager.RemoveHandler(command);
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
        if (string.IsNullOrWhiteSpace(arguments))
        {
            _configuration.IsDebug = !_configuration.IsDebug;
            _logger.Log($"Debug mode is {(_configuration.IsDebug ? "on" : "off")}");
            _logger.Log("");
            _logger.Log($"Sub-commands are: {DebugChatDump}, {DebugList}");
        }
        else
        {
            var args = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var debugCommand = args[0].ToLower();
            switch (debugCommand)
            {
                case DebugChatDump:
                    _chatLogManager.DumpLogs(_logger);
                    break;
                case DebugList:
                    ListDebugFlags();
                    break;
                default:
                    _logger.Log($"Debug command not recognized: '{debugCommand}'");
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
            _logger.Log($"{flagString}  on/off");
            _logger.Log($"{flagUnderscores}  ------");
            foreach (var (name, func) in _debugFlags) ListDebugFlag(name, func(), length);
        }

        // <summary>
        //     Lists one debug flag.
        // </summary>
        void ListDebugFlag(string name, bool value, int length)
        {
            var onOff = value ? "on" : "off";
            _logger.Log($"{name.PadRight(length)}  {onOff}");
        }
    }
}

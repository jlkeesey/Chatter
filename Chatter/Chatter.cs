using System.Reflection;
using Chatter.Localization;
using Chatter.Model;
using Chatter.Properties;
using Chatter.Windows;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using ImGuiScene;

namespace Chatter;

// TODO Fix tell in vs out
// TODO When day changes (at midnight) add marker to log
// TODO auto switch log files once a day
// TODO Localize the new fields and help

public sealed partial class Chatter : IDalamudPlugin
{
    public static string Version = string.Empty;

    private readonly ChatLogManager _chatLogManager;
    private readonly ChatManager _chatManager;
    private readonly TextureWrap _chatterImage;
    private readonly CommandManager _commandManager;
    private readonly Configuration _configuration;
    private readonly JlkWindowManager _windowManager;

    public Chatter([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ChatGui chatGui,
        [RequiredVersion("1.0")] ClientState clientState,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] DataManager gameData)
    {
        _commandManager = commandManager;

        try
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            Loc.Load();

            Dalamud.Initialize(pluginInterface);

            _configuration = Configuration.Load(pluginInterface);

            var dateManager = new DateManager();
            var myself = new Myself(clientState);
            var worldManager = new WorldManager(gameData);
            var friendManager = new FriendManager(worldManager);

            _chatLogManager = new ChatLogManager(_configuration, dateManager, myself);
            _chatManager = new ChatManager(_configuration, _chatLogManager, chatGui, dateManager, myself.HomeWorld);
            _chatterImage = pluginInterface.UiBuilder.LoadImage(Resources.chatter);
            _windowManager = new JlkWindowManager(pluginInterface, _configuration, dateManager, friendManager, Name,
                _chatterImage);
            RegisterCommands();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public string Name => "Chatter";


    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _configuration?.Save(); // Should be auto-saved but let's be sure

        UnregisterCommands();
        _chatterImage?.Dispose();
        _chatLogManager?.Dispose();
        _chatManager?.Dispose();
        _windowManager?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }
}
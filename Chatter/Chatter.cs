using System.Reflection;
using Chatter.Localization;
using Chatter.Properties;
using Chatter.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiScene;

namespace Chatter;

// TODO Add timestamp
// TODO NodaTime?
// TODO Fix tell in vs out
// TODO When day changes (at midnight) add marker to log
// TODO auto switch log files once a day
// TODO Localize the new fields and help

public sealed partial class Chatter : IDalamudPlugin
{
    public static string Version = string.Empty;

    private readonly JlkWindowManager _windowManager;

    public Chatter(DalamudPluginInterface pluginInterface)
    {
        try
        {
            Dalamud.Initialize(pluginInterface);
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
            Loc.Load();

            Configuration = Configuration.Load();

            WindowSystem = new WindowSystem(Name);

            ChatLogManager = new ChatLogManager();
            ChatManager = new ChatManager(Configuration, ChatLogManager);

            ChatterImage = Dalamud.PluginInterface.UiBuilder.LoadImage(Resources.chatter);

            _windowManager = new JlkWindowManager(this);
            RegisterCommands();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

#pragma warning disable CS8618
    /// <summary>
    ///     The configuration of this plugin.
    /// </summary>
    public static Configuration Configuration { get; private set; }
#pragma warning restore CS8618

    public TextureWrap? ChatterImage { get; }
    private ChatManager ChatManager { get; }
    private ChatLogManager ChatLogManager { get; }
    public WindowSystem WindowSystem { get; private set; }
    public string Name => "Chatter";

    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Configuration?.Save(); // Should be auto-saved but let's be sure

        UnregisterCommands();
        ChatterImage?.Dispose();
        ChatLogManager?.Dispose();
        ChatManager?.Dispose();
        _windowManager?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }
}
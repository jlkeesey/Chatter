using System;
using Chatter.Localization;
using Chatter.Model;
using Chatter.System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiScene;

namespace Chatter.Windows;

/// <summary>
///     Manages the top-level windows of the plugin including binding them with the plugin system.
/// </summary>
public sealed class JlkWindowManager : IDisposable
{
    private readonly ConfigWindow _configWindow;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly WindowSystem _windowSystem;

    /// <summary>
    ///     Creates the manager, all top-level windows, and binds them where needed.
    /// </summary>
    /// <param name="pluginInterface"></param>
    /// <param name="config"></param>
    /// <param name="dateHelper"></param>
    /// <param name="friendManager"></param>
    /// <param name="nameSpace"></param>
    /// <param name="chatterImage"></param>
    /// <param name="loc"></param>
    public JlkWindowManager(DalamudPluginInterface pluginInterface, Configuration config, IDateHelper dateHelper,
        FriendManager friendManager, string nameSpace, TextureWrap chatterImage, Loc loc)
    {
        _pluginInterface = pluginInterface;
        _windowSystem = new WindowSystem(nameSpace);
        _configWindow = Add(new ConfigWindow(config, dateHelper, friendManager, chatterImage, loc));
        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
    }

    /// <summary>
    ///     Unbinds from the plugin window system.
    /// </summary>
    public void Dispose()
    {
        _pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;

        _windowSystem.RemoveAllWindows();

        _configWindow.Dispose();
    }

    /// <summary>
    ///     Toggles the visibility of the configuration window.
    /// </summary>
    public void ToggleConfig()
    {
        _configWindow.IsOpen = !_configWindow.IsOpen;
    }

    /// <summary>
    ///     Adds the given window to the plugin system window list.
    /// </summary>
    /// <typeparam name="TType">The window type.</typeparam>
    /// <param name="window">The window to add.</param>
    /// <returns>The given window.</returns>
    private TType Add<TType>(TType window) where TType : Window
    {
        _windowSystem.AddWindow(window);
        return window;
    }
}
using System;
using Dalamud.Interface.Windowing;

namespace Chatter.Windows;

/// <summary>
///     Manages the top-level windows of the plugin including binding them with the plugin system.
/// </summary>
public sealed class JlkWindowManager : IDisposable
{
    private readonly Chatter _chatter;
    private readonly global::Chatter.Windows.ConfigWindow _configWindow;

    /// <summary>
    ///     Creates the manager, all top-level windows, and binds them where needed.
    /// </summary>
    /// <param name="chatter">The plugin object.</param>
    public JlkWindowManager(Chatter chatter)
    {
        _chatter = chatter;

        _configWindow = Add(new global::Chatter.Windows.ConfigWindow(_chatter.ChatterImage));

        Dalamud.PluginInterface.UiBuilder.Draw += chatter.WindowSystem.Draw;
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
    }

    /// <summary>
    ///     Unbinds from the plugin window system.
    /// </summary>
    public void Dispose()
    {
        Dalamud.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        Dalamud.PluginInterface.UiBuilder.Draw -= _chatter.WindowSystem.Draw;

        _chatter.WindowSystem.RemoveAllWindows();

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
        _chatter.WindowSystem.AddWindow(window);
        return window;
    }
}
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

using Chatter.Localization;
using Chatter.Model;
using Chatter.System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using Chatter.Chat;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace Chatter.Windows;

/// <summary>
///     Manages the top-level windows of the plugin including binding them with the plugin system.
/// </summary>
public sealed class JlkWindowManager : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly ConfigWindow _configWindow;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly WindowSystem _windowSystem;
    private readonly SelectEventPopup _selectEventPopup;
    private readonly StopEventPopup _stopEventPopup;

    /// <summary>
    ///     Creates the manager, all top-level windows, and binds them where needed.
    /// </summary>
    /// <param name="pluginInterface"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="dateHelper"></param>
    /// <param name="friendManager"></param>
    /// <param name="chatLogManager"></param>
    /// <param name="nameSpace"></param>
    /// <param name="chatterImage"></param>
    /// <param name="loc"></param>
    public JlkWindowManager(IDalamudPluginInterface pluginInterface,
                            Configuration config,
                            ILogger logger,
                            IDateHelper dateHelper,
                            FriendManager friendManager,
                            ChatLogManager chatLogManager,
                            string nameSpace,
                            ISharedImmediateTexture chatterImage,
                            Loc loc)
    {
        _pluginInterface = pluginInterface;
        _windowSystem = new WindowSystem(nameSpace);
        _mainWindow = Add(new MainWindow(this, config, chatLogManager, loc));
        _configWindow =
            Add(new ConfigWindow(config, logger, dateHelper, friendManager, chatLogManager, chatterImage, loc));
        _selectEventPopup = Add(new SelectEventPopup(this, config, loc));
        _stopEventPopup = Add(new StopEventPopup(this, config, loc));
        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenMainUi += ToggleMain;
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
    }

    /// <summary>
    ///     Toggles the visibility of the main window.
    /// </summary>
    public void ToggleMain()
    {
        _mainWindow.IsOpen = !_mainWindow.IsOpen;
    }

    /// <summary>
    ///     Toggles the visibility of the configuration window.
    /// </summary>
    public void ToggleConfig()
    {
        _configWindow.IsOpen = !_configWindow.IsOpen;
    }

    /// <summary>
    ///     Toggles the visibility of the configuration window.
    /// </summary>
    public void ShowConfig()
    {
        _configWindow.IsOpen = true;
    }

    /// <summary>
    ///     Shows the select event popup.
    /// </summary>
    public void ShowSelectEvent(Vector2? position = null)
    {
        if (position == null)
        {
            _selectEventPopup.PositionCondition = ImGuiCond.None;
        }
        else
        {
            _selectEventPopup.PositionCondition = ImGuiCond.Appearing;
            _selectEventPopup.Position = position;
        }

        _selectEventPopup.IsOpen = true;
    }

    /// <summary>
    ///     Shows the select event popup.
    /// </summary>
    public void HideSelectEvent()
    {
        _selectEventPopup.IsOpen = false;
    }

    /// <summary>
    ///     Shows the stop event popup.
    /// </summary>
    public void ShowStopEvent(Vector2? position = null)
    {
        if (position == null)
        {
            _stopEventPopup.PositionCondition = ImGuiCond.None;
        }
        else
        {
            _stopEventPopup.PositionCondition = ImGuiCond.Appearing;
            _stopEventPopup.Position = position;
        }

        _stopEventPopup.IsOpen = true;
    }

    /// <summary>
    ///     Shows the stop event popup.
    /// </summary>
    public void HideStopEvent()
    {
        _stopEventPopup.IsOpen = false;
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

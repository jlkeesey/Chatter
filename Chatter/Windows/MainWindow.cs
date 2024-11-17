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
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using Chatter.Chat;
using Chatter.ImGuiX;
using Dalamud.Interface;

// ReSharper disable InvertIf

namespace Chatter.Windows;

/// <summary>
///     Defines the configuration editing window.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly JlkWindowManager _windowManager;
    private readonly Configuration _config;
    private readonly ChatLogManager _chatLogManager;

    private readonly Loc _loc;

    /// <summary>
    ///     Constructs the configuration editing window.
    /// </summary>
    /// <param name="windowManager"></param>
    /// <param name="config"></param>
    /// <param name="chatLogManager"></param>
    /// <param name="loc"></param>
    public MainWindow(JlkWindowManager windowManager, Configuration config, ChatLogManager chatLogManager, Loc loc) :
        base(loc.Message("Title.MainWindow"), ImGuiWindowFlags.AlwaysAutoResize)
    {
        _windowManager = windowManager;
        _config = config;
        _chatLogManager = chatLogManager;
        _loc = loc;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(20, 20), MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        //Size = new Vector2(200, 60);
        SizeCondition = ImGuiCond.Appearing;
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public override void Draw()
    {
        DrawRestart();
        ImGui.SameLine();
        DrawStartEvent();
        ImGui.SameLine();
        DrawStopEvent();
        ImGui.SameLine();
        DrawSettings();
    }

    /// <summary>
    ///     Draws and handles the restart action. This action closes all the open logs which forces them
    ///     the restart with the newest settings at the moment a new message comes through.
    /// </summary>
    private void DrawRestart()
    {
        if (DrawRestartIconButton()) _chatLogManager.HandleGeneralConfigChange();
    }

    /// <summary>
    ///     Draws the button that performs the restart action.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawRestartIconButton()
    {
        bool result;
        if (_config.ShowMinimalMainWindow)
            result = ImGuiWidgets.DrawIconButton("restart", FontAwesomeIcon.Undo, MsgButtonRestartHelp);
        else
        {
            result = ImGui.Button(MsgButtonRestart);
            if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(MsgButtonRestartHelp);
        }

        return result;
    }

    /// <summary>
    ///     Draws and handles the start event action. This event will show a popup that allows the user to
    ///     select a non-active event for starting.
    /// </summary>
    private void DrawStartEvent()
    {
        var loc = ImGui.GetCursorScreenPos();
        if (DrawStartEventButton()) _windowManager.ShowStartEvent(loc);
    }

    /// <summary>
    ///     Draws the start event button.
    /// <returns><c>true</c> if the button was pressed.</returns>
    /// </summary>
    private bool DrawStartEventButton()
    {
        bool result;
        if (_config.ShowMinimalMainWindow)
            result = ImGuiWidgets.DrawIconButton("startEvent", FontAwesomeIcon.Play, MsgButtonStartEventHelp);
        else
        {
            result = ImGui.Button(MsgButtonStartEvent);
            if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(MsgButtonStartEventHelp);
        }

        return result;
    }

    /// <summary>
    ///     Draws and handles the show/hide config action.
    /// </summary>
    private void DrawSettings()
    {
        if (DrawSettingsButton()) _windowManager.ShowConfig();
    }

    /// <summary>
    ///     Draws the show/hide config button.
    /// <returns><c>true</c> if the button was pressed.</returns>
    /// </summary>
    private bool DrawSettingsButton()
    {
        bool result;
        if (_config.ShowMinimalMainWindow)
            result = ImGuiWidgets.DrawIconButton("toggleConfig", FontAwesomeIcon.Cog, MsgButtonShowConfigHelp);
        else
        {
            result = ImGui.Button(MsgButtonShowConfig);
            if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(MsgButtonShowConfigHelp);
        }

        return result;
    }

    /// <summary>
    ///     Draws and handles the start event action. This event will show a popup that allows the user to
    ///     select a non-active event for starting.
    /// </summary>
    private void DrawStopEvent()
    {
        var loc = ImGui.GetCursorScreenPos();
        if (DrawStopEventButton()) _windowManager.ShowStopEvent(loc);
    }

    /// <summary>
    ///     Draws the start event button.
    /// <returns><c>true</c> if the button was pressed.</returns>
    /// </summary>
    private bool DrawStopEventButton()
    {
        bool result;
        if (_config.ShowMinimalMainWindow)
            result = ImGuiWidgets.DrawIconButton("stopEvent", FontAwesomeIcon.Stop, MsgButtonStopEventHelp);
        else
        {
            result = ImGui.Button(MsgButtonStopEvent);
            if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(MsgButtonStopEventHelp);
        }

        return result;
    }
}

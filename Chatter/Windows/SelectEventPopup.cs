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

using System.Collections.Generic;
using Chatter.Localization;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using System.Numerics;
using Chatter.ImGuiX;
using NodaTime;
using NodaTime.Extensions;

// ReSharper disable InvertIf

namespace Chatter.Windows;

/// <summary>
///     Defines the configuration editing window.
/// </summary>
public sealed class SelectEventPopup : Window
{
    private const string Title = "Select Event";

    private readonly JlkWindowManager _windowManager;
    private readonly Configuration _configuration;
    private readonly Loc _loc;

    private bool _hasEvents;
    private int _eventSelected;
    private List<ImGuiWidgets.ComboOption<string>> _items = [];

    /// <summary>
    ///     Constructs the start event selection popup.
    /// </summary>
    /// <param name="windowManager">The window manager.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="loc">The message localization object.</param>
    public SelectEventPopup(JlkWindowManager windowManager, Configuration config, Loc loc) : base(Title,
        ImGuiWindowFlags.AlwaysAutoResize)
    {
        _windowManager = windowManager;
        _configuration = config;
        _loc = loc;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 60), MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        Size = new Vector2(200, 60);
        SizeCondition = ImGuiCond.Appearing;
        PositionCondition = ImGuiCond.Appearing;
    }

    /// <summary>
    ///     Called when the popup is opened.
    /// </summary>
    public override void OnOpen()
    {
        base.OnOpen();
        Chatter.Logger?.Debug($"@@@@ Opening {Title}");
        _items = _configuration.ChatLogs.Select(log => log.Value)
                               .Where(log => log is {IsEvent: true, IsActive: false,})
                               .OrderBy(item => item.Name)
                               .Select(item => new ImGuiWidgets.ComboOption<string>(item.Name, item.Name))
                               .ToList();
        _hasEvents = _items.Count > 0;
        if (!_hasEvents)
        {
            _items =
            [
                new ImGuiWidgets.ComboOption<string>("(no inactive events)",
                                                     "-null-",
                                                     "There are no inactive events to start."),
            ];
        }

        _eventSelected = 0;
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public override void Draw()
    {
        ImGuiWidgets.DrawCombo("Events",
                               _items,
                               _eventSelected,
                               "Events",
                               onSelect: (ind) => { _eventSelected = ind; });

        ImGuiWidgets.VerticalSpace();
        ImGui.Separator();
        ImGuiWidgets.VerticalSpace();

        using (ImGuiWith.Disabled(!_hasEvents))
        {
            if (ImGui.Button(MsgButtonStart, new Vector2(120, 0)))
            {
                var name = _items[_eventSelected].Value;
                Chatter.Logger?.Debug($"@@@@ Starting {name}");
                var log = _configuration.ChatLogs[name];
                log.EventStartTime = SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime();
                log.EventLength = Period.FromSeconds(30);
                log.IsActive = true;
                _windowManager.HideSelectEvent();
            }
        }

        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0)))
        {
            _windowManager.HideSelectEvent();
        }
    }

    private string MsgButtonStart => _loc.Message("Button.Start");
    private string MsgButtonCancel => _loc.Message("Button.Cancel");
}

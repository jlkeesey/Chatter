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
using Dalamud.Bindings.ImGui;
using System.Linq;
using System.Numerics;
using Chatter.ImGuiX;
using Dalamud.Interface.Utility.Raii;
using NodaTime;
using NodaTime.Extensions;

// ReSharper disable InvertIf

namespace Chatter.Windows;

/// <summary>
///     Defines the configuration editing window.
/// </summary>
public sealed class StartEventPopup : Window
{
    private string MsgButtonStart => _loc.Message("Button.Start");
    private string MsgButtonCancel => _loc.Message("Button.Cancel");
    private string MsgNoInactiveEvents => _loc.Message("Combo.NoInactiveEvents");
    private string MsgNoInactiveEventsHelp => _loc.Message("Combo.NoInactiveEvents.Help");
    private string MsgEventsInactive => _loc.Message("Label.EventsInactive");
    private string MsgEventsInactiveHelp => _loc.Message("Label.EventsInactive.Help");

    private readonly JlkWindowManager _windowManager;
    private readonly Configuration _configuration;
    private readonly Loc _loc;

    private bool _hasEvents;
    private int _eventSelected;
    private List<ImGuiWidgets.ComboOption<string>> _items = [];

    private readonly PeriodEditor _periodEditor;

    /// <summary>
    ///     Constructs the start event selection popup.
    /// </summary>
    /// <param name="windowManager">The window manager.</param>
    /// <param name="config">The plugin configuration.</param>
    /// <param name="loc">The message localization object.</param>
    public StartEventPopup(JlkWindowManager windowManager, Configuration config, Loc loc) :
        base(loc.Message("Title.StartEvent"), ImGuiWindowFlags.AlwaysAutoResize)
    {
        _windowManager = windowManager;
        _configuration = config;
        _loc = loc;
        _periodEditor = new PeriodEditor(loc);

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
        _items = _configuration.ChatLogs.Select(log => log.Value)
                               .Where(log => log is {IsEvent: true, IsActive: false,})
                               .OrderBy(item => item.Name)
                               .Select(item => new ImGuiWidgets.ComboOption<string>(item.Name, item.Name))
                               .ToList();
        _eventSelected = 0;
        _hasEvents = _items.Count > 0;
        if (_hasEvents)
        {
            _periodEditor.SetPeriod(_configuration.ChatLogs[_items[_eventSelected].Value].EventLength);
        }
        else
        {
            _items = [new ImGuiWidgets.ComboOption<string>(MsgNoInactiveEvents, "-null-", MsgNoInactiveEventsHelp),];
        }
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public override void Draw()
    {
        ImGuiWidgets.DrawCombo(MsgEventsInactive,
                               _items,
                               _eventSelected,
                               MsgEventsInactiveHelp,
                               onSelect: (ind) => { _eventSelected = ind; });

        ImGuiWidgets.VerticalSpace();
        _periodEditor.DrawPeriodEditor(GetEventLength(), SetEventLength);
        ImGuiWidgets.VerticalSpace();
        ImGui.Separator();
        ImGuiWidgets.VerticalSpace();

        using (ImRaii.Disabled(!_hasEvents))
        {
            if (ImGui.Button(MsgButtonStart, new Vector2(120, 0)))
            {
                var name = _items[_eventSelected].Value;
                var log = _configuration.ChatLogs[name];
                log.EventStartTime = SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime();
                log.IsActive = true;
                _windowManager.HideStartEvent();
            }
        }

        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0)))
        {
            _windowManager.HideStartEvent();
        }
    }

    /// <summary>
    /// Returns the current event length from the selected event. If there is no matching event,
    /// <c>Period.ZERO</c> is returned.
    /// </summary>
    /// <returns>The currently selected event's event length.</returns>
    private Period GetEventLength()
    {
        var name = _items[_eventSelected].Value;
        return _configuration.ChatLogs.TryGetValue(name, out var log) ? log.EventLength : Period.Zero;
    }

    /// <summary>
    ///     Sets the currently selected event's event length.
    /// </summary>
    /// <param name="period">The new event length for the event.</param>
    private void SetEventLength(Period period)
    {
        var name = _items[_eventSelected].Value;
        if (_configuration.ChatLogs.TryGetValue(name, out var log))
        {
            log.EventLength = period;
        }
    }
}

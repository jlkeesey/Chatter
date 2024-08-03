// Copyright 2024 James Keesey
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
using Chatter.ImGuiX;
using Chatter.Localization;
using ImGuiNET;
using NodaTime;
using System.Collections.Generic;

namespace Chatter.Windows;

public class PeriodEditor
{
    private enum PeriodTypes
    {
        Days,
        Hours,
        Minutes,
    }

    private readonly Loc _loc;

    private readonly List<ImGuiWidgets.ComboOption<PeriodTypes>> _periodOptions;

    private int _periodSelected;
    private int _periodValue;

    private readonly string _label;
    private readonly string _help;

    public PeriodEditor(Loc loc, string? label = null, string? help = null)
    {
        _loc = loc;
        _label = label ?? MsgLabelPeriod;
        _help = help ?? MsgLabelPeriodHelp;
        _periodOptions =
        [
            new ImGuiWidgets.ComboOption<PeriodTypes>(MsgComboPeriodDays, PeriodTypes.Days, MsgComboPeriodDaysHelp),
            new ImGuiWidgets.ComboOption<PeriodTypes>(MsgComboPeriodHours,
                                                      PeriodTypes.Hours,
                                                      MsgComboPeriodHoursHelp),
            new ImGuiWidgets.ComboOption<PeriodTypes>(MsgComboPeriodMinutes,
                                                      PeriodTypes.Minutes,
                                                      MsgComboPeriodMinutesHelp),
        ];
    }

    /// <summary>
    ///     Should be called from and window's OnOpen.
    /// </summary>
    public void SetPeriod(Period period)
    {
        _periodSelected = 0;
        _periodValue = 1;

        if (period.Days > 0)
        {
            _periodSelected = 0;
            _periodValue = period.Days;
        }
        else if (period.Hours > 0)
        {
            _periodSelected = 1;
            _periodValue = (int) period.Hours;
        }
        else
        {
            _periodSelected = 2;
            _periodValue = (int) period.Minutes;
        }
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public void DrawPeriodEditor(Period period, Action<Period> onChange)
    {
        SetPeriod(period);
        using (ImGuiWith.ItemWidth(90))
        {
            if (ImGui.InputInt("##periodValue", ref _periodValue)) onChange(GetPeriod());
        }

        ImGui.SameLine();
        ImGuiWidgets.DrawCombo(_label,
                               _periodOptions,
                               _periodSelected,
                               _help,
                               width: 100,
                               onSelect: idx =>
                               {
                                   _periodSelected = idx;
                                   onChange(GetPeriod());
                               });
    }

    private Period GetPeriod()
    {
        return _periodSelected switch
        {
            0 => Period.FromDays(_periodValue),
            1 => Period.FromHours(_periodValue),
            _ => Period.FromMinutes(_periodValue),
        };
    }

    private string MsgLabelPeriod => _loc.Message("Label.Period");
    private string MsgLabelPeriodHelp => _loc.Message("Label.Period.Help");
    private string MsgComboPeriodDays => _loc.Message("Combo.Period.Days");
    private string MsgComboPeriodDaysHelp => _loc.Message("Combo.Period.Days.Help");
    private string MsgComboPeriodHours => _loc.Message("Combo.Period.Hours");
    private string MsgComboPeriodHoursHelp => _loc.Message("Combo.Period.Hours.Help");
    private string MsgComboPeriodMinutes => _loc.Message("Combo.Period.Minutes");
    private string MsgComboPeriodMinutesHelp => _loc.Message("Combo.Period.Minutes.Help");
}

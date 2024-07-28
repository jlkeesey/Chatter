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
using System.Collections.Generic;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace Chatter.ImGuiX;

public static class ImGuiWidgets
{
    /// <summary>
    ///     Creates a tooltip box with the given content text which will be wrapped as necessary.
    /// </summary>
    /// <param name="description">The contents of the tooltip box. If <c>null</c> or empty the tooltip box is not created.</param>
    public static void DrawTooltip(string? description)
    {
        var text = description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return;
        ImGui.BeginTooltip();
        using (ImGuiWith.TextWrapPos(ImGui.GetFontSize() * 20.0f))
        using (ImGuiWith.Color(ImGuiCol.Text, 0xff4ce5e5))
            ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }

    /// <summary>
    ///     Adds a help button that shows the given help text when hovered over.
    /// </summary>
    /// <param name="description">
    ///     The description to show. If this is <c>null</c>, empty, or all whitespace, nothing is
    ///     created.
    /// </param>
    /// <param name="sameLine"><c>true</c> if this should be on the same line as the previous item.</param>
    public static void HelpMarker(string? description, bool sameLine = true)
    {
        var text = description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return;
        if (sameLine) ImGui.SameLine();
        using (ImGuiWith.Color(ImGuiCol.Text, 0xff8c4c4c))
        using (ImGuiWith.Font(UiBuilder.IconFont))
            ImGui.TextUnformatted($"{(char) FontAwesomeIcon.QuestionCircle}");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) DrawTooltip(text);
    }

    /// <summary>
    ///     Creates an input field for a long value such that the label is not on the same line as the input field.
    /// </summary>
    /// <param name="label">The text label for this field.</param>
    /// <param name="value">The field value. This must be a ref value.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="id">The optional id for the field. This is only necessary if the label is not unique.</param>
    /// <param name="help">The optional help text displayed when hovering over the help button.</param>
    /// <param name="extra">Function to add extra parts to the end of the widget.</param>
    /// <param name="extraWidth">The width of the extra element(s).</param>
    public static void LongInputField(string label,
                                      ref string value,
                                      uint maxLength = 100,
                                      string? id = null,
                                      string? help = null,
                                      Action? extra = null,
                                      int extraWidth = 0)
    {
        ImGui.TextUnformatted(label);
        HelpMarker(help);

        ImGui.SetNextItemWidth(extraWidth == 0 ? -1 : -extraWidth);
        ImGui.InputText(id ?? label, ref value, maxLength);
        if (extra != null)
        {
            ImGui.SameLine();
            extra();
        }
    }

    /// <summary>
    ///     Draws a checkbox control with the optional help text.
    /// </summary>
    /// <param name="label">The label for the checkbox.</param>
    /// <param name="itemChecked"><c>true</c> if this check box is checked.</param>
    /// <param name="helpText">The optional help text.</param>
    /// <param name="disabled"><c>true</c> if this control should be disabled.</param>
    public static void DrawCheckbox(string label, ref bool itemChecked, string? helpText = null, bool disabled = false)
    {
        using (ImGuiWith.Disabled(disabled)) ImGui.Checkbox(label, ref itemChecked);
        HelpMarker(helpText);
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    public static bool DrawIconButton(string id, FontAwesomeIcon icon, string tooltip, bool disabled = false)
    {
        bool buttonPressed;
        using (ImGuiWith.Disabled(disabled))
        using (ImGuiWith.Font(UiBuilder.IconFont))
            buttonPressed = ImGui.Button($"{(char) icon}##{id}");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGuiWidgets.DrawTooltip(tooltip);
        return buttonPressed;
    }

    /// <summary>
    ///     Adds vertical space to the output.
    /// </summary>
    /// <param name="space">The amount of extra space to add in <c>ImGUI</c> units.</param>
    public static void VerticalSpace(float space = 3.0f)
    {
        ImGui.Dummy(new Vector2(0.0f, space));
    }

    public static void DrawTwoColumns(string id, Action left, Action right, float rightWidth = -1)
    {
        if (ImGui.BeginTable(id, 2))
        {
            ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
            if (rightWidth < 0)
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
            }
            else
            {
                ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, rightWidth);
            }


            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            left.Invoke();

            ImGui.TableSetColumnIndex(1);
            right.Invoke();

            ImGui.EndTable();
        }
    }

    public static void DrawKoFiButton(string? tooltip = null)
    {
        var message = tooltip ?? "Support me on Ko-Fi";
        using (ImGuiWith.Color(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF))
        using (ImGuiWith.Color(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFFC))
        using (ImGuiWith.Color(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF))
        {
            if (ImGuiWidgets.DrawIconButton("kofiButton", FontAwesomeIcon.Coffee, message))
                Dalamud.Utility.Util.OpenLink("https://ko-fi.com/fioragreyback");
        }
    }

    public class ComboOption<T>(string label, T value, string? help = null)
    {
        public readonly string? Help = help;
        public readonly string Label = label;
        public readonly T Value = value;
    }

    public static void DrawCombo<T>(string label,
                                    List<ComboOption<T>> options,
                                    int selected,
                                    string help,
                                    float width = 200,
                                    Action<int>? onSelect = null)
    {
        ImGui.SetNextItemWidth(200.0f);
        if (ImGui.BeginCombo(label, options[selected].Label))
        {
            for (var i = 0; i < options.Count; i++)
            {
                var isSelected = i == selected;
                if (ImGui.Selectable(options[i].Label, isSelected)) onSelect?.Invoke(i);

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGuiWidgets.DrawTooltip(options[i].Help);
                if (isSelected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        ImGuiWidgets.HelpMarker(help);
    }

    public static void ColoredText(string text, uint color)
    {
        using (ImGuiWith.Color(ImGuiCol.Text, color)) ImGui.TextUnformatted(text);
    }
}

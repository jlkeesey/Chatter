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
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using JetBrains.Annotations;

namespace Chatter.ImGuiX;

public static class ImGuiWidgets
{
    private const byte AlphaFull = 0xff;
    private const byte ByteMask = 0xff;

    /// <summary>
    ///     Deconstructs a packed RGB value into a packed ABGR value.
    /// </summary>
    /// <param name="value">The RGB value to unpack.</param>
    /// <returns>The packed ABGR value for use in ImGui.</returns>
    [UsedImplicitly]
    public static uint Rgb(uint value)
    {
        var alpha = (byte) ((value >> 24) & ByteMask);
        var red = (byte) ((value >> 16) & ByteMask);
        var green = (byte) ((value >> 8) & ByteMask);
        var blue = (byte) (value & ByteMask);
        return Argb(alpha == 0 ? AlphaFull : alpha,red, green, blue);
    }

    /// <summary>
    ///     Returns the given ARGB values the packed format that ImGui uses. For some reason, they use ABGR instead. The
    ///     alpha channel is set to 0xFF.
    /// </summary>
    /// <param name="red">The red value (0-255).</param>
    /// <param name="green">The green value (0-255).</param>
    /// <param name="blue">The blue value (0-255).</param>
    /// <returns>The packed ABGR value for use in ImGui.</returns>
    [UsedImplicitly]
    public static uint Rgb(byte red, byte green, byte blue)
    {
        return Argb(AlphaFull, red, green, blue);
    }

    /// <summary>
    ///     Deconstructs a packed ARGB value into a packed ABGR value.
    /// </summary>
    /// <param name="value">The ARGB value to unpack.</param>
    /// <returns>The packed ABGR value for use in ImGui.</returns>
    [UsedImplicitly]
    public static uint Argb(uint value)
    {
        var alpha = (byte) ((value >> 24) & ByteMask);
        var red = (byte) ((value >> 16) & ByteMask);
        var green = (byte) ((value >> 8) & ByteMask);
        var blue = (byte) (value & ByteMask);
        return Argb(alpha, red, green, blue);
    }

    /// <summary>
    ///     Returns the given ARGB values the packed format that ImGui uses. For some reason, they use ABGR instead.
    /// </summary>
    /// <param name="alpha">The alpha value (0-255).</param>
    /// <param name="red">The red value (0-255).</param>
    /// <param name="green">The green value (0-255).</param>
    /// <param name="blue">The blue value (0-255).</param>
    /// <returns>The packed ABGR value for use in ImGui.</returns>
    [UsedImplicitly]
    public static uint Argb(byte alpha, byte red, byte green, byte blue)
    {
        return (uint) ((alpha << 24) | (blue << 16) | (green << 8) | red);
    }

    /// <summary>
    ///     Creates a tooltip box with the given content text which will be wrapped as necessary.
    /// </summary>
    /// <param name="description">The contents of the tooltip box. If <c>null</c> or empty the tooltip box is not created.</param>
    public static void DrawTooltip(string? description)
    {
        var text = description?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return;
        using (ImRaii.Tooltip())
        using (ImGuiWith.TextWrapPos(ImGui.GetFontSize() * 20.0f))
        using (ImRaii.PushColor(ImGuiCol.Text, Rgb(0xe5e54c)))
            ImGui.TextUnformatted(text);
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
        using (ImRaii.PushColor(ImGuiCol.Text, Rgb(0x4c4c8c)))
        using (ImRaii.PushFont(UiBuilder.IconFont))
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
    /// <param name="allowEnter">True if the enter key is used to finish the entry.</param>
    /// <param name="filter">Filter for the characters allowed.</param>
    /// <param name="extra">Function to add extra parts to the end of the widget.</param>
    /// <param name="extraWidth">The width of the extra element(s).</param>
    public static unsafe bool LongInputField(string label,
                                             ref string value,
                                             uint maxLength = 100,
                                             string? id = null,
                                             string? help = null,
                                             bool allowEnter = false,
                                             ICharacterFilter? filter = null,
                                             Action? extra = null,
                                             int extraWidth = 0)
    {
        ImGui.TextUnformatted(label);
        HelpMarker(help);

        var flags = allowEnter ? ImGuiInputTextFlags.EnterReturnsTrue : ImGuiInputTextFlags.None;

        ImGui.SetNextItemWidth(extraWidth == 0 ? -1 : -extraWidth);
        if (filter != null)
        {
            flags |= ImGuiInputTextFlags.CallbackCharFilter;
            if (ImGui.InputText(id ?? label, ref value, maxLength, flags, filter.Filter))
            {
                return true;
            }
        }
        else
        {
            if (ImGui.InputText(id ?? label, ref value, maxLength, flags))
            {
                return true;
            }
        }

        if (extra != null)
        {
            ImGui.SameLine();
            extra();
        }

        return false;
    }

    /// <summary>
    ///     Base for character filters for input fields;
    /// </summary>
    public interface ICharacterFilter
    {
        /// <summary>
        ///     Passed to the <c>ImGui.InputText</c> to filter out any unwanted characters.
        /// </summary>
        /// <param name="data">The filter callback data.</param>
        /// <returns>0 to allow the character, 1 to ignore it.</returns>
        public unsafe int Filter(ImGuiInputTextCallbackData* data);
    }

    /// <summary>
    ///     Filters out characters that are not allowed in file names.
    /// </summary>
    public class FilenameCharactersFilter : ICharacterFilter
    {
        /// <inheritdoc/>
        public unsafe int Filter(ImGuiInputTextCallbackData* data)
        {
            var ch = Convert.ToChar(data->EventChar);
            if (char.IsLetterOrDigit(ch)) return 0;
            return ch switch
            {
                '-' or ',' or '=' or '~' or '!' or '@' or '#' or '+' => 0,
                _                                                    => 1,
            };
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
        using (ImRaii.Disabled(disabled)) ImGui.Checkbox(label, ref itemChecked);
        HelpMarker(helpText);
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    public static bool DrawIconButton(string id, FontAwesomeIcon icon, string tooltip, bool disabled = false)
    {
        bool buttonPressed;
        using (ImRaii.Disabled(disabled))
        using (ImRaii.PushFont(UiBuilder.IconFont))
            buttonPressed = ImGui.Button($"{(char) icon}##{id}");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) DrawTooltip(tooltip);
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
        using var table = ImRaii.Table(id, 2);
        if (!table) return;
        ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
        if (rightWidth < 0)
            ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthStretch);
        else
            ImGui.TableSetupColumn(string.Empty, ImGuiTableColumnFlags.WidthFixed, rightWidth);

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        left.Invoke();

        ImGui.TableSetColumnIndex(1);
        right.Invoke();
    }

    private static readonly uint KofiButtonColor = Argb(0xff, 0xff, 0x5b, 0x5e);
    private static readonly uint KofiActiveColor = Argb(0xdd, 0xff, 0x5b, 0x5e);
    private static readonly uint KofiHoveredColor = Argb(0xaa, 0xff, 0x5b, 0x5e);

    public static void DrawKoFiButton(string? tooltip = null)
    {
        var message = tooltip ?? "Support me on Ko-Fi";
        using (ImRaii.PushColor(ImGuiCol.Button, KofiButtonColor)
                     .Push(ImGuiCol.ButtonActive, KofiActiveColor)
                     .Push(ImGuiCol.ButtonHovered, KofiHoveredColor))
        {
            if (DrawIconButton("kofiButton", FontAwesomeIcon.Coffee, message))
                Util.OpenLink("https://ko-fi.com/fioragreyback");
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
        using var itemWidth = ImGuiWith.ItemWidth(width);
        using (var combo = ImRaii.Combo(label, options[selected].Label))
        {
            if (!combo) return;
            for (var i = 0; i < options.Count; i++)
            {
                var isSelected = i == selected;
                if (ImGui.Selectable(options[i].Label, isSelected)) onSelect?.Invoke(i);

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) DrawTooltip(options[i].Help);
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
        }

        HelpMarker(help);
    }

    public static void ColoredText(string text, uint color)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, color)) ImGui.TextUnformatted(text);
    }
}

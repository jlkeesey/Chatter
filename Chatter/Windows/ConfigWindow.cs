using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Chatter.Localization;
using Chatter.Model;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using static System.String;
using static Chatter.Configuration;
using static Chatter.Configuration.FileNameOrder;

// ReSharper disable InvertIf

namespace Chatter.Windows;

/// <summary>
///     Defines the configuration editing window.
/// </summary>
public sealed partial class ConfigWindow : Window, IDisposable
{
    private const string Title = "Chatter Configuration";

    /// <summary>
    ///     The list of chat types for the General section.
    /// </summary>
    private static readonly ChatTypeFlagList GeneralChatTypeFlags = new()
    {
        {XivChatType.Say, Loc.Message("ChatType.Say"), Loc.Message("ChatType.Say.Help")},
        {XivChatType.Yell, Loc.Message("ChatType.Yell"), Loc.Message("ChatType.Yell.Help")},
        {XivChatType.Shout, Loc.Message("ChatType.Shout"), Loc.Message("ChatType.Shout.Help")},
        {
            XivChatType.TellIncoming, Loc.Message("ChatType.Tell"), Loc.Message("ChatType.Tell.Help"),
            config => config.SyncFlags()
        },
        {XivChatType.Party, Loc.Message("ChatType.Party"), Loc.Message("ChatType.Party.Help")},
        {XivChatType.FreeCompany, Loc.Message("ChatType.FreeCompany"), Loc.Message("ChatType.FreeCompany.Help")},
        {XivChatType.Alliance, Loc.Message("ChatType.Alliance"), Loc.Message("ChatType.Alliance.Help")},
        {
            XivChatType.StandardEmote, Loc.Message("ChatType.Emote"), Loc.Message("ChatType.Emote.Help"),
            config => config.SyncFlags()
        },
    };

    /// <summary>
    ///     The list of chat types for the Linkshell section.
    /// </summary>
    private static readonly ChatTypeFlagList LinkShellChatTypeFlags = new()
    {
        {XivChatType.Ls1, Loc.Message("ChatType.Ls1"), Loc.Message("ChatType.Ls1.Help")},
        {XivChatType.Ls2, Loc.Message("ChatType.Ls2"), Loc.Message("ChatType.Ls2.Help")},
        {XivChatType.Ls3, Loc.Message("ChatType.Ls3"), Loc.Message("ChatType.Ls3.Help")},
        {XivChatType.Ls4, Loc.Message("ChatType.Ls4"), Loc.Message("ChatType.Ls4.Help")},
        {XivChatType.Ls5, Loc.Message("ChatType.Ls5"), Loc.Message("ChatType.Ls5.Help")},
        {XivChatType.Ls6, Loc.Message("ChatType.Ls6"), Loc.Message("ChatType.Ls6.Help")},
        {XivChatType.Ls7, Loc.Message("ChatType.Ls7"), Loc.Message("ChatType.Ls7.Help")},
        {XivChatType.Ls8, Loc.Message("ChatType.Ls8"), Loc.Message("ChatType.Ls8.Help")},
    };

    /// <summary>
    ///     The list of chat types for the Cross-World Linkshell section.
    /// </summary>
    private static readonly ChatTypeFlagList CrossWorldLinkShellChatTypeFlags = new()
    {
        {XivChatType.CrossLinkShell1, Loc.Message("ChatType.Cwls1"), Loc.Message("ChatType.Cwls1.Help")},
        {XivChatType.CrossLinkShell2, Loc.Message("ChatType.Cwls2"), Loc.Message("ChatType.Cwls2.Help")},
        {XivChatType.CrossLinkShell3, Loc.Message("ChatType.Cwls3"), Loc.Message("ChatType.Cwls3.Help")},
        {XivChatType.CrossLinkShell4, Loc.Message("ChatType.Cwls4"), Loc.Message("ChatType.Cwls4.Help")},
        {XivChatType.CrossLinkShell5, Loc.Message("ChatType.Cwls5"), Loc.Message("ChatType.Cwls5.Help")},
        {XivChatType.CrossLinkShell6, Loc.Message("ChatType.Cwls6"), Loc.Message("ChatType.Cwls6.Help")},
        {XivChatType.CrossLinkShell7, Loc.Message("ChatType.Cwls7"), Loc.Message("ChatType.Cwls7.Help")},
        {XivChatType.CrossLinkShell8, Loc.Message("ChatType.Cwls8"), Loc.Message("ChatType.Cwls8.Help")},
    };

    /// <summary>
    ///     The list of chat types for the Other section.
    /// </summary>
    private static readonly ChatTypeFlagList OtherChatTypeFlags = new()
    {
        {XivChatType.Urgent, Loc.Message("ChatType.Urgent"), Loc.Message("ChatType.Urgent.Help")},
        {XivChatType.Notice, Loc.Message("ChatType.Notice"), Loc.Message("ChatType.Notice.Help")},
        {XivChatType.NoviceNetwork, Loc.Message("ChatType.NoviceNetwork"), Loc.Message("ChatType.NoviceNetwork.Help")},
        {XivChatType.PvPTeam, Loc.Message("ChatType.PvPTeam"), Loc.Message("ChatType.PvPTeam.Help")},
        {XivChatType.Echo, Loc.Message("ChatType.Echo"), Loc.Message("ChatType.Echo.Help")},
        {XivChatType.SystemError, Loc.Message("ChatType.SystemError"), Loc.Message("ChatType.SystemError.Help")},
        {XivChatType.SystemMessage, Loc.Message("ChatType.SystemMessage"), Loc.Message("ChatType.SystemMessage.Help")},
    };

    private static int _timeStampSelected = -1;
    private readonly TextureWrap? _chatterImage;
    private readonly Configuration _configuration;

    private readonly List<ComboOption<string>> _dateOptions;
    private readonly List<ComboOption<FileNameOrder>> _fileOrderOptions;
    private bool _addUserAlreadyExists;
    private string _addUserFullName = Empty;
    private string _addUserReplacementName = Empty;
    private IEnumerable<Friend> _filteredFriends = new List<Friend>();
    private string _friendFilter = Empty;
    private IEnumerable<Friend> _friends = new List<Friend>();
    private int _logOrderSelected = -1;
    private bool _removeDialogIsOpen = true;
    private string _removeDialogUser = Empty;
    private string _removeUser = Empty;
    private string _selectedFriend = Empty;
    private string _selectedGroup = AllLogName;

    /// <summary>
    ///     Constructs the configuration editing window.
    /// </summary>
    /// <param name="chatterImage">The Chatter plugin icon.</param>
    public ConfigWindow(TextureWrap? chatterImage) : base(Title)
    {
        _chatterImage = chatterImage;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 520),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        Size = new Vector2(800, 520);
        SizeCondition = ImGuiCond.FirstUseEver;

        _configuration = Chatter.Configuration;

        _dateOptions = new List<ComboOption<string>>
        {
            new(ConfigWindow.MsgComboTimestampCultural, "G", ConfigWindow.MsgComboTimestampCulturalHelp),
            new(ConfigWindow.MsgComboTimestampSortable, "yyyy-MM-dd HH:mm:ss", ConfigWindow.MsgComboTimestampSortableHelp),
        };

        _fileOrderOptions = new List<ComboOption<FileNameOrder>>
        {
            new(ConfigWindow.MsgComboOrderGroupDate, PrefixGroupDate, ConfigWindow.MsgComboOrderGroupDateHelp),
            new(ConfigWindow.MsgComboOrderDateGroup, PrefixGroupDate, ConfigWindow.MsgComboOrderDateGroupHelp),
        };
    }

    public void Dispose()
    {
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public override void Draw()
    {
        if (_chatterImage != null)
        {
            ImGui.Image(_chatterImage.ImGuiHandle, new Vector2(64, 64));
            VerticalSpace(5.0f);
        }

        if (ImGui.BeginTabBar("tabBar", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem(ConfigWindow.MsgTabGeneral))
            {
                DrawGeneralTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(ConfigWindow.MsgTabGroups))
            {
                DrawGroupsTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    ///     Draws the general settings tab. This is where all the settings that affect the entire plugin are edited.
    /// </summary>
    private void DrawGeneralTab()
    {
        LongInputField(ConfigWindow.MsgLabelFileNamePrefix, ref _configuration.LogFileNamePrefix, 50,
            "##fileNamePrefix",
            ConfigWindow.MsgLabelFileNamePrefixHelp);

        VerticalSpace();

        LongInputField(ConfigWindow.MsgLabelSaveDirectory, ref _configuration.LogDirectory, 1024, "##saveDirectory",
            ConfigWindow.MsgLabelSaveDirectoryHelp);

        VerticalSpace();

        if (_logOrderSelected < 0)
        {
            _logOrderSelected = 0; // Default in case we don't find it
            for (var i = 0; i < _fileOrderOptions.Count; i++)
                if (_fileOrderOptions[i].Value == _configuration.LogOrder)
                {
                    _logOrderSelected = i;
                    break;
                }
        }

        ImGui.SetNextItemWidth(200.0f);
        if (ImGui.BeginCombo(ConfigWindow.MsgComboOrderLabel, _fileOrderOptions[_logOrderSelected].Label))
        {
            for (var i = 0; i < _fileOrderOptions.Count; i++)
            {
                var isSelected = i == _logOrderSelected;
                if (ImGui.Selectable(_fileOrderOptions[i].Label, isSelected))
                {
                    _logOrderSelected = i;
                    _configuration.LogOrder = _fileOrderOptions[i].Value;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    DrawTooltip(_fileOrderOptions[i].Help);
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        HelpMarker(ConfigWindow.MsgComboOrderHelp);
    }

    /// <summary>
    ///     Draws the group tab. This allows for editing a single group definition.
    /// </summary>
    private void DrawGroupsTab()
    {
        DrawGroupsList();

        ImGui.SameLine();

        DrawGroupEdit();
    }

    /// <summary>
    ///     Draws the editor for a single group's configuration.
    /// </summary>
    private void DrawGroupEdit()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        ImGui.BeginChild("groupData", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y),
            true);
        var chatLog = _configuration.ChatLogs[_selectedGroup];
        ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), chatLog.Name);
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.BeginTable("general", 2))
        {
            ImGui.TableNextColumn();
            DrawCheckbox(ConfigWindow.MsgLabelIsActive, ref chatLog.IsActive, ConfigWindow.MsgLabelIsActiveHelp,
                chatLog.Name == AllLogName);
            ImGui.TableNextColumn();
            DrawCheckbox(ConfigWindow.MsgLabelIncludeAllUsers, ref chatLog.IncludeAllUsers, ConfigWindow.MsgLabelIncludeAllUsersHelp,
                chatLog.Name == AllLogName);
            ImGui.TableNextColumn();
            DrawCheckbox(ConfigWindow.MsgLabelIncludeServerName, ref chatLog.IncludeServer, ConfigWindow.MsgLabelIncludeServerNameHelp);
            ImGui.TableNextColumn();
            DrawCheckbox(ConfigWindow.MsgLabelIncludeSelf, ref chatLog.IncludeMe, ConfigWindow.MsgLabelIncludeSelfHelp);

#if DEBUG
            ImGui.TableNextColumn();
            DrawCheckbox(MsgLabelIncludeAll, ref chatLog.DebugIncludeAllMessages, MsgLabelIncludeAllHelp);
#endif
            ImGui.EndTable();
        }

        VerticalSpace();

        ImGui.PushItemWidth(150.0f);
        ImGui.InputInt(ConfigWindow.MsgInputWrapWidthLabel, ref chatLog.MessageWrapWidth);
        HelpMarker(ConfigWindow.MsgInputWrapWidthHelp);

        ImGui.InputInt(ConfigWindow.MsgInputWrapIndentLabel, ref chatLog.MessageWrapIndentation);
        HelpMarker(ConfigWindow.MsgInputWrapIndentHelp);

        if (_timeStampSelected < 0)
        {
            _timeStampSelected = 0; // Default in case we don't find it
            for (var i = 0; i < _dateOptions.Count; i++)
                if (_dateOptions[i].Value == chatLog.Format)
                {
                    _timeStampSelected = i;
                    break;
                }
        }

        if (ImGui.BeginCombo(ConfigWindow.MsgComboTimestampLabel, _dateOptions[_timeStampSelected].Label))
        {
            for (var i = 0; i < _dateOptions.Count; i++)
            {
                var isSelected = i == _timeStampSelected;
                if (ImGui.Selectable(_dateOptions[i].Label, isSelected))
                {
                    _timeStampSelected = i;
                    chatLog.DateTimeFormat = _dateOptions[i].Value;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    DrawTooltip(_dateOptions[i].Help);
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        HelpMarker(ConfigWindow.MsgComboTimestampHelp);

        ImGui.PopItemWidth();

        VerticalSpace();

        if (ImGui.CollapsingHeader(ConfigWindow.MsgHeaderIncludedUsers))
        {
            VerticalSpace(5.0f);
            ImGui.TextWrapped(ConfigWindow.MsgDescriptionIncludedUsers);
            VerticalSpace();
            if (ImGui.Button(ConfigWindow.MsgButtonAddUser)) ImGui.OpenPopup("addUser");

            DrawAddUserPopup(chatLog);

            const ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg |
                                               ImGuiTableFlags.BordersOuter |
                                               ImGuiTableFlags.SizingFixedFit |
                                               ImGuiTableFlags.BordersV;
            var textBaseHeight = ImGui.GetTextLineHeightWithSpacing();
            var outerSize = new Vector2(0.0f, textBaseHeight * 8);
            if (ImGui.BeginTable("userTable", 3, tableFlags, outerSize))
            {
                ImGui.TableSetupScrollFreeze(0, 1); // Make top row always visible
                ImGui.TableSetupColumn(ConfigWindow.MsgColumnFullName, ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(ConfigWindow.MsgColumnReplacement, ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(Empty, ImGuiTableColumnFlags.WidthFixed, 22.0f);
                ImGui.TableHeadersRow();

                foreach (var (userFrom, userTo) in chatLog.Users)
                {
                    ImGui.PushID(userFrom);
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextColored(new Vector4(0.6f, 0.7f, 1.0f, 1.0f), userFrom);

                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.6f, 1.0f),
                        userTo.IsNullOrWhitespace() ? "-" : userTo);

                    ImGui.TableSetColumnIndex(2);
                    if (DrawUserRemoveButton(userFrom))
                    {
                        if (!chatLog.Users.ContainsKey(userFrom)) return;
                        _removeDialogUser = userFrom;
                        ImGui.OpenPopup("removeUser");
                    }

                    DrawRemoveUserDialog();
                    ImGui.PopID();
                }


                ImGui.EndTable();
                VerticalSpace();
            }
        }

        if (_removeUser != Empty)
        {
            chatLog.Users.Remove(_removeUser);
            _removeUser = Empty;
        }

        if (ImGui.CollapsingHeader(ConfigWindow.MsgHeaderIncludedChatTypes))
        {
            DrawChatTypeFlags("flagGeneral", chatLog, GeneralChatTypeFlags);
            DrawChatTypeFlags("flagLs", chatLog, LinkShellChatTypeFlags);
            DrawChatTypeFlags("flagCwls", chatLog, CrossWorldLinkShellChatTypeFlags);
            DrawChatTypeFlags("flagOther", chatLog, OtherChatTypeFlags);
        }

        ImGui.EndChild();
        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Adds vertical space to the output.
    /// </summary>
    /// <param name="space">The amount of extra space to add in <c>ImGUI</c> units.</param>
    private static void VerticalSpace(float space = 3.0f)
    {
        ImGui.Dummy(new Vector2(0.0f, space));
    }

    /// <summary>
    ///     Draws the popup to add a new user to the user list.
    /// </summary>
    /// <param name="chatLog">The chat log configuration to edit.</param>
    private void DrawAddUserPopup(ChatLogConfiguration chatLog)
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(350.0f, 100.0f), new Vector2(350.0f, 200.0f));
        if (ImGui.BeginPopup("addUser", ImGuiWindowFlags.ChildWindow))
        {
            if (_addUserAlreadyExists)
            {
                VerticalSpace();
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), ConfigWindow.MsgPlayerAlreadyInList);
                VerticalSpace();
            }

            LongInputField(ConfigWindow.MsgPlayerFullName, ref _addUserFullName, 128, "##playerFullName", ConfigWindow.MsgPlayerFullNameHelp,
                extraWidth: 30, extra: () =>
                {
                    if (DrawFindFriendButton())
                    {
                        _friendFilter = Empty;
                        _friends = _filteredFriends = FriendManager.GetFriends();
                        _selectedFriend = Empty;
                        ImGui.OpenPopup("findFriend");
                    }
                });

            VerticalSpace();
            LongInputField(ConfigWindow.MsgPlayerReplacement, ref _addUserReplacementName, 128, "##playerReplaceName",
                ConfigWindow.MsgPlayerReplacementHelp, extraWidth: 30);

            VerticalSpace();
            ImGui.Separator();
            VerticalSpace();

            if (ImGui.Button(ConfigWindow.MsgButtonAdd, new Vector2(120, 0)))
            {
                _addUserAlreadyExists = false;
                var fullName = _addUserFullName.Trim();
                if (!fullName.IsNullOrWhitespace())
                {
                    var replacement = _addUserReplacementName.Trim();
                    if (chatLog.Users.TryAdd(fullName, replacement))
                        ImGui.CloseCurrentPopup();
                    else
                        _addUserAlreadyExists = true;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button(ConfigWindow.MsgButtonCancel, new Vector2(120, 0)))
            {
                _addUserAlreadyExists = false;
                ImGui.CloseCurrentPopup();
            }

            DrawFindFriendPopup(ref _addUserFullName);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the popup that lists all the users friends for selection.
    /// </summary>
    /// <param name="targetUserFullName">Where to put the chosen friend name.</param>
    private void DrawFindFriendPopup(ref string targetUserFullName)
    {
        if (ImGui.BeginPopup("findFriend", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.InputText("##filter", ref _friendFilter, 100))
                _filteredFriends = _friends
                    .Where(f => f.Name.Contains(_friendFilter, StringComparison.OrdinalIgnoreCase))
                    .ToImmutableSortedSet();
            ImGui.SameLine();
            if (DrawClearFilterButton()) _friendFilter = Empty;

            var textBaseHeight = ImGui.GetTextLineHeightWithSpacing();
            var outerSize = new Vector2(-1.0f, textBaseHeight * 8);
            if (ImGui.BeginListBox("##friendList", outerSize))
            {
                foreach (var filteredFriend in _filteredFriends)
                    if (ImGui.Selectable(filteredFriend.FullName, filteredFriend.FullName == _selectedFriend))
                        _selectedFriend = filteredFriend.FullName;

                ImGui.EndListBox();
            }

            ImGui.Separator();
            if (ImGui.Button(ConfigWindow.MsgButtonAdd, new Vector2(120, 0)))
            {
                if (!_selectedFriend.IsNullOrWhitespace())
                    targetUserFullName = _selectedFriend;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(ConfigWindow.MsgButtonCancel, new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private static bool DrawClearFilterButton()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var buttonPressed = ImGui.Button($"{(char) FontAwesomeIcon.SquareXmark}##findFriend");
        ImGui.PopFont();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            DrawTooltip(ConfigWindow.MsgButtonClearFilterHelp);
        return buttonPressed;
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private static bool DrawFindFriendButton()
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var buttonPressed = ImGui.Button($"{(char) FontAwesomeIcon.PersonCirclePlus}##findFriend");
        ImGui.PopFont();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            DrawTooltip(ConfigWindow.MsgButtonFriendSelectorHelp);
        return buttonPressed;
    }

    /// <summary>
    ///     Draws the button that brings up the remove user dialog.
    /// </summary>
    /// <param name="user">Which user is being removed.</param>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private static bool DrawUserRemoveButton(string user)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var buttonPressed = ImGui.Button($"{(char) FontAwesomeIcon.Trash}##{user}Trash");
        ImGui.PopFont();
        return buttonPressed;
    }

    /// <summary>
    ///     Draws the remove user dialog.
    /// </summary>
    private void DrawRemoveUserDialog()
    {
        if (ImGui.BeginPopupModal("removeUser", ref _removeDialogIsOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextWrapped(Loc.Message("Text.RemoveUser", _removeDialogUser));
            ImGui.Separator();

            if (ImGui.Button(ConfigWindow.MsgButtonRemove, new Vector2(120, 0)))
            {
                _removeUser = _removeDialogUser;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(ConfigWindow.MsgButtonCancel, new Vector2(120, 0))) ImGui.CloseCurrentPopup();


            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws a check box control with the optional help text.
    /// </summary>
    /// <param name="label">The label for the checkbox.</param>
    /// <param name="itemChecked"><c>true</c> if this check box is checked.</param>
    /// <param name="helpText">The optional help text.</param>
    /// <param name="disabled"><c>true</c> if this control should be disabled.</param>
    private static void DrawCheckbox(string label, ref bool itemChecked, string? helpText = null, bool disabled = false)
    {
        if (disabled) ImGui.BeginDisabled();
        ImGui.Checkbox(label, ref itemChecked);
        if (disabled) ImGui.EndDisabled();
        HelpMarker(helpText);
    }

    /// <summary>
    ///     Draws the list of groups for selecting into the editor.
    /// </summary>
    private void DrawGroupsList()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        ImGui.BeginChild("groupsChild",
            new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, ImGui.GetContentRegionAvail().Y), true);
        if (ImGui.BeginListBox("##groups",
                new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)))
        {
            foreach (var (_, cl) in _configuration.ChatLogs)
            {
                var isSelected = _selectedGroup == cl.Name;
                if (ImGui.Selectable(cl.Name, isSelected)) _selectedGroup = cl.Name;
                if (ImGui.IsItemHovered()) DrawTooltip(cl.Name);
                if (isSelected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndListBox();
        }

        ImGui.EndChild();
        ImGui.PopStyleVar();
    }

    /// <summary>
    ///     Draws all of the chat flags in a given list.
    /// </summary>
    /// <param name="id">The id for the list of items.</param>
    /// <param name="chatLog">The chat log configuration.</param>
    /// <param name="flagList">The list of flags to draw. If a flag is in the list but not set in the config it is ignored.</param>
    private static void DrawChatTypeFlags(string id, ChatLogConfiguration chatLog,
        ChatTypeFlagList flagList)
    {
        ImGui.Spacing();
        var flags = chatLog.ChatTypeFilterFlags;
        if (ImGui.BeginTable(id, 4))
        {
            foreach (var flag in flagList)
                if (flags.TryGetValue(flag.Type, out var flagValue))
                    DrawFlag(flag, chatLog, ref flagValue.Value);

            ImGui.EndTable();
        }

        ImGui.Spacing();
    }

    /// <summary>
    ///     Draws a single chat flag item.
    /// </summary>
    /// <param name="info">The display information about this flag.</param>
    /// <param name="chatLog">The configuration this flag is from.</param>
    /// <param name="flag">The flag value location.</param>
    private static void DrawFlag(ChatTypeFlagInfo info, ChatLogConfiguration chatLog,
        ref bool flag)
    {
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(info.Label, ref flag)) info.OnChange?.Invoke(chatLog);

        HelpMarker(info.Help);
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
    private static void LongInputField(string label, ref string value, uint maxLength = 100, string? id = null,
        string? help = null, Action? extra = null, int extraWidth = 0)
    {
        ImGui.Text(label);
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
    ///     Adds a help button that shows the given help text when hovered over.
    /// </summary>
    /// <param name="description">
    ///     The description to show. If this is <c>null</c>, empty, or all whitespace, nothing is
    ///     created.
    /// </param>
    /// <param name="sameLine"><c>true</c> if this should be on the same line as the previous item.</param>
    private static void HelpMarker(string? description, bool sameLine = true)
    {
        var text = description?.Trim() ?? Empty;
        if (text.IsNullOrEmpty()) return;
        if (sameLine) ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.4f, 0.4f, 0.55f, 1.0f));
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text($"{(char) FontAwesomeIcon.QuestionCircle}");
        ImGui.PopFont();
        ImGui.PopStyleColor();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) DrawTooltip(text);
    }

    /// <summary>
    ///     Creates a tooltip box with the given content text which will be wrapped as necessary.
    /// </summary>
    /// <param name="description">The contents of the tooltip box. If <c>null</c> or empty the tooltip box is not created.</param>
    private static void DrawTooltip(string? description)
    {
        var text = description?.Trim() ?? Empty;
        if (text.IsNullOrEmpty()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 20.0f);
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.9f, 0.3f, 1.0f));
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    public class ComboOption<T>
    {
        public readonly string? Help;
        public readonly string Label;
        public readonly T Value;

        public ComboOption(string label, T value, string? help = null)
        {
            Label = label;
            Value = value;
            Help = help;
        }
    }

    /// <summary>
    ///     Defines the display information for a chat type flag.
    /// </summary>
    private class ChatTypeFlagInfo
    {
        public ChatTypeFlagInfo(XivChatType type, string label, string? help = null,
            Action<ChatLogConfiguration>? onChange = null)
        {
            Type = type;
            Label = label;
            Help = help;
            OnChange = onChange;
        }

        public XivChatType Type { get; }
        public string Label { get; }
        public string? Help { get; }
        public Action<ChatLogConfiguration>? OnChange { get; }
    }

    /// <summary>
    ///     Helper type to make creating a list of tuples easier.
    /// </summary>
    private class ChatTypeFlagList : List<ChatTypeFlagInfo>
    {
        public void Add(XivChatType type, string label, string? help = null,
            Action<ChatLogConfiguration>? onChange = null)
        {
            Add(new ChatTypeFlagInfo(type, label, help, onChange));
        }
    }
}
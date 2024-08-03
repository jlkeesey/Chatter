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

using Chatter.ImGuiX;
using Chatter.Localization;
using Chatter.Model;
using Chatter.System;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Chatter.Chat;
using static Chatter.Configuration;
using static Chatter.Configuration.FileNameOrder;
using static System.String;
using Dalamud.Interface.Textures;
using JetBrains.Annotations;
using NodaTime;
using NodaTime.Extensions;

// ReSharper disable InvertIf

namespace Chatter.Windows;

/// <summary>
///     Defines the configuration editing window.
/// </summary>
public sealed partial class ConfigWindow : Window
{
    private const string Title = "Chatter Configuration";

    /// <summary>
    ///     The list of chat types for the General section.
    /// </summary>
    private static readonly ChatTypeFlagList GeneralChatTypeFlags = new()
    {
        {XivChatType.Say, "ChatType.Say", "ChatType.Say.Help"},
        {XivChatType.Yell, "ChatType.Yell", "ChatType.Yell.Help"},
        {XivChatType.Shout, "ChatType.Shout", "ChatType.Shout.Help"},
        {XivChatType.TellIncoming, "ChatType.Tell", "ChatType.Tell.Help", config => config.SyncFlags()},
        {XivChatType.Party, "ChatType.Party", "ChatType.Party.Help"},
        {XivChatType.FreeCompany, "ChatType.FreeCompany", "ChatType.FreeCompany.Help"},
        {XivChatType.Alliance, "ChatType.Alliance", "ChatType.Alliance.Help"},
        {XivChatType.StandardEmote, "ChatType.Emote", "ChatType.Emote.Help", config => config.SyncFlags()},
    };

    /// <summary>
    ///     The list of chat types for the Linkshell section.
    /// </summary>
    private static readonly ChatTypeFlagList LinkShellChatTypeFlags = new()
    {
        {XivChatType.Ls1, "ChatType.Ls1", "ChatType.Ls1.Help"},
        {XivChatType.Ls2, "ChatType.Ls2", "ChatType.Ls2.Help"},
        {XivChatType.Ls3, "ChatType.Ls3", "ChatType.Ls3.Help"},
        {XivChatType.Ls4, "ChatType.Ls4", "ChatType.Ls4.Help"},
        {XivChatType.Ls5, "ChatType.Ls5", "ChatType.Ls5.Help"},
        {XivChatType.Ls6, "ChatType.Ls6", "ChatType.Ls6.Help"},
        {XivChatType.Ls7, "ChatType.Ls7", "ChatType.Ls7.Help"},
        {XivChatType.Ls8, "ChatType.Ls8", "ChatType.Ls8.Help"},
    };

    /// <summary>
    ///     The list of chat types for the Cross-World Linkshell section.
    /// </summary>
    private static readonly ChatTypeFlagList CrossWorldLinkShellChatTypeFlags = new()
    {
        {XivChatType.CrossLinkShell1, "ChatType.Cwls1", "ChatType.Cwls1.Help"},
        {XivChatType.CrossLinkShell2, "ChatType.Cwls2", "ChatType.Cwls2.Help"},
        {XivChatType.CrossLinkShell3, "ChatType.Cwls3", "ChatType.Cwls3.Help"},
        {XivChatType.CrossLinkShell4, "ChatType.Cwls4", "ChatType.Cwls4.Help"},
        {XivChatType.CrossLinkShell5, "ChatType.Cwls5", "ChatType.Cwls5.Help"},
        {XivChatType.CrossLinkShell6, "ChatType.Cwls6", "ChatType.Cwls6.Help"},
        {XivChatType.CrossLinkShell7, "ChatType.Cwls7", "ChatType.Cwls7.Help"},
        {XivChatType.CrossLinkShell8, "ChatType.Cwls8", "ChatType.Cwls8.Help"},
    };

    /// <summary>
    ///     The list of chat types for the Other section.
    /// </summary>
    private static readonly ChatTypeFlagList OtherChatTypeFlags = new()
    {
        {XivChatType.Urgent, "ChatType.Urgent", "ChatType.Urgent.Help"},
        {XivChatType.Notice, "ChatType.Notice", "ChatType.Notice.Help"},
        {XivChatType.NoviceNetwork, "ChatType.NoviceNetwork", "ChatType.NoviceNetwork.Help"},
        {XivChatType.PvPTeam, "ChatType.PvPTeam", "ChatType.PvPTeam.Help"},
        {XivChatType.Echo, "ChatType.Echo", "ChatType.Echo.Help"},
        {XivChatType.SystemError, "ChatType.SystemError", "ChatType.SystemError.Help"},
        {XivChatType.SystemMessage, "ChatType.SystemMessage", "ChatType.SystemMessage.Help"},
    };

    private static int _timeStampSelected;
    private readonly ISharedImmediateTexture _chatterImage;
    private readonly Configuration _configuration;
    private readonly ChatLogManager _chatLogManager;
    private readonly FriendManager _friendManager;
    private readonly Loc _loc;
    [UsedImplicitly] private readonly ILogger _logger;

    private readonly List<ImGuiWidgets.ComboOption<string>> _dateOptions;
    private readonly List<ImGuiWidgets.ComboOption<FileNameOrder>> _fileOrderOptions;
    private readonly List<ImGuiWidgets.ComboOption<DirectoryFormat>> _directoryFormOptions;
    private readonly PeriodEditor _periodEditor;
    private bool _addUserAlreadyExists;
    private bool _addGroupAlreadyExists;
    private string _addGroupName = Empty;
    private bool _addEventAlreadyExists;
    private string _addEventName = Empty;
    private string _addUserFullName = Empty;
    private string _addUserReplacementName = Empty;
    private IEnumerable<Friend> _filteredFriends = new List<Friend>();
    private string _friendFilter = Empty;
    private IEnumerable<Friend> _friends = new List<Friend>();
    private int _directoryFormSelected = -1;
    private int _logOrderSelected = -1;
    private bool _removeDialogIsOpen = true;
    private string _removeDialogUser = Empty;
    private bool _deleteGroupDialogIsOpen = true;
    private string _deleteDialogGroup = Empty;
    private string _removeUser = Empty;
    private string _deleteGroup = Empty;
    private string _selectedFriend = Empty;
    private string _selectedGroup = AllLogName;

    /// <summary>
    ///     Constructs the configuration editing window.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="dateHelper"></param>
    /// <param name="friendManager"></param>
    /// <param name="chatLogManager"></param>
    /// <param name="chatterImage">The Chatter plugin icon.</param>
    /// <param name="loc"></param>
    public ConfigWindow(Configuration config,
                        ILogger logger,
                        IDateHelper dateHelper,
                        FriendManager friendManager,
                        ChatLogManager chatLogManager,
                        ISharedImmediateTexture chatterImage,
                        Loc loc) : base(Title)
    {
        _configuration = config;
        _logger = logger;
        _friendManager = friendManager;
        _chatLogManager = chatLogManager;
        _chatterImage = chatterImage;
        _loc = loc;
        _periodEditor = new PeriodEditor(loc, MsgLabelEventLength, MsgLabelEventLengthHelp);

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 520), MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };

        Size = new Vector2(800, 520);
        SizeCondition = ImGuiCond.FirstUseEver;

        _dateOptions =
        [
            new ImGuiWidgets.ComboOption<string>(MsgComboTimestampCultural,
                                                 dateHelper.CultureDateTimePattern.PatternText,
                                                 MsgComboTimestampCulturalHelp),
            new ImGuiWidgets.ComboOption<string>(MsgComboTimestampSortable,
                                                 dateHelper.SortableDateTimePattern.PatternText,
                                                 MsgComboTimestampSortableHelp),
        ];

        _fileOrderOptions =
        [
            new ImGuiWidgets.ComboOption<FileNameOrder>(MsgComboOrderGroupDate,
                                                        PrefixGroupDate,
                                                        MsgComboOrderGroupDateHelp),
            new ImGuiWidgets.ComboOption<FileNameOrder>(MsgComboOrderDateGroup,
                                                        PrefixDateGroup,
                                                        MsgComboOrderDateGroupHelp),
        ];

        _directoryFormOptions =
        [
            new ImGuiWidgets.ComboOption<DirectoryFormat>(MsgComboDirectoryUnified,
                                                          DirectoryFormat.Unified,
                                                          MsgComboDirectoryUnifiedHelp),
            new ImGuiWidgets.ComboOption<DirectoryFormat>(MsgComboDirectoryGroup,
                                                          DirectoryFormat.Group,
                                                          MsgComboDirectoryGroupHelp),
            new ImGuiWidgets.ComboOption<DirectoryFormat>(MsgComboDirectoryYearMonth,
                                                          DirectoryFormat.YearMonth,
                                                          MsgComboDirectoryYearMonthHelp),
            new ImGuiWidgets.ComboOption<DirectoryFormat>(MsgComboDirectoryYearMonthGroup,
                                                          DirectoryFormat.YearMonthGroup,
                                                          MsgComboDirectoryYearMonthGroupHelp),
            new ImGuiWidgets.ComboOption<DirectoryFormat>(MsgComboDirectoryGroupYearMonth,
                                                          DirectoryFormat.GroupYearMonth,
                                                          MsgComboDirectoryGroupYearMonthHelp),
        ];
    }

    public override void OnOpen()
    {
        base.OnOpen();
        _logOrderSelected = 0; // Default in case we don't find it
        for (var i = 0; i < _fileOrderOptions.Count; i++)
            if (_fileOrderOptions[i].Value == _configuration.LogOrder)
            {
                _logOrderSelected = i;
                break;
            }

        _directoryFormSelected = 0; // Default in case we don't find it
        for (var i = 0; i < _directoryFormOptions.Count; i++)
            if (_directoryFormOptions[i].Value == _configuration.DirectoryForm)
            {
                _directoryFormSelected = i;
                break;
            }

        HandleSelectGroup(AllLogName);
    }

    /// <summary>
    ///     Draws this window.
    /// </summary>
    public override void Draw()
    {
        ImGuiWidgets.DrawTwoColumns("header", DrawPluginIcon, DrawRestartButton, rightWidth: 70);
        ImGuiWidgets.VerticalSpace(5);

        if (ImGui.BeginTabBar("tabBar", ImGuiTabBarFlags.None))
        {
            if (ImGui.BeginTabItem(MsgTabGeneral))
            {
                DrawGeneralTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(MsgTabGroups))
            {
                DrawGroupsTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawRestartButton()
    {
        if (ImGui.Button(MsgButtonRestart)) _chatLogManager.HandleGeneralConfigChange();
        if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(MsgButtonRestartHelp);
    }

    private void DrawPluginIcon()
    {
        ImGui.Image(_chatterImage.GetWrapOrEmpty().ImGuiHandle, new Vector2(64, 64));
    }

    /// <summary>
    ///     Draws the general settings tab. This is where all the settings that affect the entire plugin are edited.
    /// </summary>
    private void DrawGeneralTab()
    {
        ImGuiWidgets.LongInputField(MsgLabelFileNamePrefix,
                                    ref _configuration.LogFileNamePrefix,
                                    50,
                                    "##fileNamePrefix",
                                    MsgLabelFileNamePrefixHelp);

        ImGuiWidgets.VerticalSpace();
        ImGuiWidgets.LongInputField(MsgLabelSaveDirectory,
                                    ref _configuration.LogDirectory,
                                    1024,
                                    "##saveDirectory",
                                    MsgLabelSaveDirectoryHelp,
                                    extraWidth: 30,
                                    extra: () =>
                                    {
                                        if (DrawCopyButton("savePathCopy"))
                                        {
                                            ImGui.LogToClipboard();
                                            ImGui.LogText(_configuration.LogDirectory);
                                            ImGui.LogFinish();
                                            Dalamud.Utility.Util.OpenLink(_configuration.LogDirectory);
                                        }
                                    });

        ImGuiWidgets.VerticalSpace();

        ImGuiWidgets.DrawCombo(MsgComboOrderLabel,
                               _fileOrderOptions,
                               _logOrderSelected,
                               MsgComboOrderHelp,
                               onSelect: (ind) =>
                               {
                                   _logOrderSelected = ind;
                                   _configuration.LogOrder = _fileOrderOptions[ind].Value;
                               });

        ImGuiWidgets.VerticalSpace();

        ImGuiWidgets.DrawCombo(MsgComboDirectoryFormLabel,
                               _directoryFormOptions,
                               _directoryFormSelected,
                               MsgComboDirectoryFormHelp,
                               onSelect: (ind) =>
                               {
                                   _directoryFormSelected = ind;
                                   _configuration.DirectoryForm = _directoryFormOptions[ind].Value;
                               });

        ImGuiWidgets.VerticalSpace(8);

        ImGuiWidgets.DrawCheckbox(MsgShowMinimalMain, ref _configuration.ShowMinimalMainWindow, MsgShowMinimalMainHelp);

        ImGuiWidgets.VerticalSpace(8);

        ImGuiWidgets.DrawKoFiButton();
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

    private static void DrawGroupTitle(ChatLogConfiguration chatLog)
    {
        ImGuiWidgets.ColoredText(chatLog.Title, 0xff00ff00);
    }

    private void DrawGroupDelete(ChatLogConfiguration chatLog)
    {
        if (DrawDeleteGroupButton(chatLog.Name, chatLog.IsAll))
        {
            if (!_configuration.ChatLogs.ContainsKey(chatLog.Name)) return;
            _deleteDialogGroup = chatLog.Name;
            ImGui.OpenPopup(MsgTitleDelete);
        }

        DrawDeleteGroupDialog();
    }

    /// <summary>
    ///     Draws the editor for a single group's configuration.
    /// </summary>
    private void DrawGroupEdit()
    {
        using (ImGuiWith.Style(ImGuiStyleVar.ChildRounding, 5))
        {
            ImGui.BeginChild("groupData",
                             new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y),
                             true);
            var chatLog = _configuration.ChatLogs[_selectedGroup];
            ImGuiWidgets.DrawTwoColumns("groupTitleTable",
                                        rightWidth: 22,
                                        left: () => { DrawGroupTitle(chatLog); },
                                        right: () => { DrawGroupDelete(chatLog); });
            ImGui.Separator();
            if (chatLog is {IsEvent: true, IsActive: true,})
            {
                ImGui.Spacing();
                var endTime = chatLog.EventStartTime + chatLog.EventLength;
                var now = SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime();
                var diff = endTime - now;
                var timeLeft = FormatPeriod(diff);
                ImGui.TextUnformatted($"Time remaining {timeLeft}");
            }

            ImGui.Spacing();

            if (ImGui.BeginTable("general", 2))
            {
                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIsActive, ref chatLog.IsActive, MsgLabelIsActiveHelp, chatLog.IsAll);
                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIncludeAllUsers,
                                          ref chatLog.IncludeAllUsers,
                                          MsgLabelIncludeAllUsersHelp,
                                          chatLog.IsAll);
                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIsEvent, ref chatLog.IsEvent, MsgLabelIsEventHelp, chatLog.IsAll);
                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIncludeSelf, ref chatLog.IncludeMe, MsgLabelIncludeSelfHelp);

                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIncludeServerName,
                                          ref chatLog.IncludeServer,
                                          MsgLabelIncludeServerNameHelp);

#if DEBUG
                ImGui.TableNextColumn();
                ImGuiWidgets.DrawCheckbox(MsgLabelIncludeAll,
                                          ref chatLog.DebugIncludeAllMessages,
                                          MsgLabelIncludeAllHelp);
#endif
                ImGui.EndTable();
            }

            if (chatLog is {IsEvent: true,})
            {
                ImGuiWidgets.VerticalSpace();
                _periodEditor.DrawPeriodEditor(chatLog.EventLength, period => chatLog.EventLength = period);
            }

            ImGuiWidgets.VerticalSpace();

            using (ImGuiWith.ItemWidth(150))
            {
                ImGui.InputInt(MsgInputWrapWidthLabel, ref chatLog.MessageWrapWidth);
                ImGuiWidgets.HelpMarker(MsgInputWrapWidthHelp);

                ImGui.InputInt(MsgInputWrapIndentLabel, ref chatLog.MessageWrapIndentation);
                ImGuiWidgets.HelpMarker(MsgInputWrapIndentHelp);

                ImGuiWidgets.DrawCombo(MsgComboTimestampLabel,
                                       _dateOptions,
                                       _timeStampSelected,
                                       MsgComboTimestampHelp,
                                       onSelect: (ind) =>
                                       {
                                           _timeStampSelected = ind;
                                           chatLog.DateTimeFormat = _dateOptions[ind].Value;
                                       });
            }

            ImGuiWidgets.VerticalSpace();

            if (ImGui.CollapsingHeader(MsgHeaderIncludedUsers))
            {
                ImGuiWidgets.VerticalSpace(5);
                ImGui.TextUnformatted(MsgDescriptionIncludedUsers);
                ImGuiWidgets.VerticalSpace();
                if (ImGui.Button(MsgButtonAddUser)) ImGui.OpenPopup("addUser");

                DrawAddUserPopup(chatLog);

                const ImGuiTableFlags tableFlags = ImGuiTableFlags.ScrollY
                                                 | ImGuiTableFlags.RowBg
                                                 | ImGuiTableFlags.BordersOuter
                                                 | ImGuiTableFlags.SizingFixedFit
                                                 | ImGuiTableFlags.BordersV;
                var textBaseHeight = ImGui.GetTextLineHeightWithSpacing();
                var outerSize = new Vector2(0, textBaseHeight * 8);
                if (ImGui.BeginTable("userTable", 3, tableFlags, outerSize))
                {
                    ImGui.TableSetupScrollFreeze(0, 1); // Make top row always visible
                    ImGui.TableSetupColumn(MsgColumnFullName, ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn(MsgColumnReplacement, ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn(Empty, ImGuiTableColumnFlags.WidthFixed, 22);
                    ImGui.TableHeadersRow();

                    foreach (var (userFrom, userTo) in chatLog.Users)
                    {
                        using (ImGuiWith.ID(userFrom))
                        {
                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGuiWidgets.ColoredText(userFrom, 0xffFFB299);

                            ImGui.TableSetColumnIndex(1);
                            ImGuiWidgets.ColoredText(IsNullOrWhiteSpace(userTo) ? "-" : userTo, 0xff99E5B2);

                            ImGui.TableSetColumnIndex(2);
                            if (DrawRemoveButton(userFrom))
                            {
                                if (!chatLog.Users.ContainsKey(userFrom)) return;
                                _removeDialogUser = userFrom;
                                ImGui.OpenPopup(MsgTitleRemove);
                            }

                            DrawRemoveUserDialog();
                        }
                    }

                    ImGui.EndTable();
                    ImGuiWidgets.VerticalSpace();
                }
            }

            if (_removeUser != Empty)
            {
                chatLog.Users.Remove(_removeUser);
                _removeUser = Empty;
            }

            if (_deleteGroup != Empty)
            {
                HandleSelectGroup(AllLogName);
                _configuration.ChatLogs.Remove(_deleteGroup);
                _deleteGroup = Empty;
            }

            if (ImGui.CollapsingHeader(MsgHeaderIncludedChatTypes))
            {
                DrawChatTypeFlags("flagGeneral", chatLog, GeneralChatTypeFlags);
                DrawChatTypeFlags("flagLs", chatLog, LinkShellChatTypeFlags);
                DrawChatTypeFlags("flagCwls", chatLog, CrossWorldLinkShellChatTypeFlags);
                DrawChatTypeFlags("flagOther", chatLog, OtherChatTypeFlags);
            }

            ImGui.EndChild();
        }
    }

    private string FormatPeriod(Period period)
    {
        if (period.Days > 0)
        {
            return Format("{0:D} days, {1:D} hours, {2:D} minutes, and {3:D} seconds",
                          period.Days,
                          period.Hours,
                          period.Minutes,
                          period.Seconds);
        }

        if (period.Hours > 0)
        {
            return Format("{0:D} hours, {1:D} minutes, and {2:D} seconds",
                          period.Hours,
                          period.Minutes,
                          period.Seconds);
        }

        if (period.Minutes > 0)
        {
            return Format("{0:D} minutes, and {1:D} seconds", period.Minutes, period.Seconds);
        }

        return Format("{0:D} seconds", period.Seconds);
    }

    /// <summary>
    ///     Draws the popup to add a new user to the user list.
    /// </summary>
    /// <param name="chatLog">The chat log configuration to edit.</param>
    private void DrawAddUserPopup(ChatLogConfiguration chatLog)
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(350, 100), new Vector2(350, 200));
        if (ImGui.BeginPopup("addUser", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (_addUserAlreadyExists)
            {
                ImGuiWidgets.VerticalSpace();
                ImGuiWidgets.ColoredText(MsgPlayerAlreadyInList, 0xff0000FF);
                ImGuiWidgets.VerticalSpace();
            }

            ImGuiWidgets.LongInputField(MsgPlayerFullName,
                                        ref _addUserFullName,
                                        128,
                                        "##playerFullName",
                                        MsgPlayerFullNameHelp,
                                        extraWidth: 30,
                                        extra: () =>
                                        {
                                            if (DrawFindFriendButton())
                                            {
                                                _friendFilter = Empty;
                                                _friends = _filteredFriends = _friendManager.GetFriends();
                                                _selectedFriend = Empty;
                                                ImGui.OpenPopup("findFriend");
                                            }
                                        });

            ImGuiWidgets.VerticalSpace();
            ImGuiWidgets.LongInputField(MsgPlayerReplacement,
                                        ref _addUserReplacementName,
                                        128,
                                        "##playerReplaceName",
                                        MsgPlayerReplacementHelp,
                                        extraWidth: 30);

            ImGuiWidgets.VerticalSpace();
            ImGui.Separator();
            ImGuiWidgets.VerticalSpace();

            if (ImGui.Button(MsgButtonAdd, new Vector2(120, 0)))
            {
                _addUserAlreadyExists = false;
                var fullName = _addUserFullName.Trim();
                if (!IsNullOrWhiteSpace(fullName))
                {
                    var replacement = _addUserReplacementName.Trim();
                    if (chatLog.Users.TryAdd(fullName, replacement))
                        ImGui.CloseCurrentPopup();
                    else
                        _addUserAlreadyExists = true;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0)))
            {
                _addUserAlreadyExists = false;
                ImGui.CloseCurrentPopup();
            }

            DrawFindFriendPopup(ref _addUserFullName);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the popup to add a new group to the group list.
    /// </summary>
    private void DrawAddGroupPopup()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(350, 100), new Vector2(350, 200));
        if (ImGui.BeginPopup("addGroup", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (_addGroupAlreadyExists)
            {
                ImGuiWidgets.VerticalSpace();
                ImGuiWidgets.ColoredText(MsgGroupAlreadyInList, 0xff0000ff);
                ImGuiWidgets.VerticalSpace();
            }

            if (ImGuiWidgets.LongInputField(MsgGroupName,
                                            ref _addGroupName,
                                            128,
                                            "##groupName",
                                            MsgGroupNameHelp,
                                            allowEnter: true,
                                            filter: new ImGuiWidgets.FilenameCharactersFilter()))
            {
                HandleAddGroupAccept();
            }

            ImGuiWidgets.VerticalSpace();
            ImGui.Separator();
            ImGuiWidgets.VerticalSpace();

            if (ImGui.Button(MsgButtonCreate, new Vector2(120, 0))) HandleAddGroupAccept();

            ImGui.SetItemDefaultFocus();
            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0)))
            {
                _addGroupName = "";
                _addGroupAlreadyExists = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void HandleAddGroupAccept()
    {
        _addGroupAlreadyExists = false;
        var groupName = _addGroupName.Trim();
        if (!IsNullOrWhiteSpace(groupName))
        {
            if (_configuration.ChatLogs.TryAdd(groupName, new ChatLogConfiguration(groupName)))
            {
                _addGroupName = "";
                HandleSelectGroup(groupName);
                ImGui.CloseCurrentPopup();
            }
            else
                _addGroupAlreadyExists = true;
        }
    }

    /// <summary>
    ///     Draws the popup to add a new event to the group list.
    /// </summary>
    private void DrawAddEventPopup()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(350, 100), new Vector2(350, 200));
        if (ImGui.BeginPopup("addEvent", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (_addEventAlreadyExists)
            {
                ImGuiWidgets.VerticalSpace();
                ImGuiWidgets.ColoredText(MsgEventAlreadyInList, 0xff0000ff);
                ImGuiWidgets.VerticalSpace();
            }

            if (ImGuiWidgets.LongInputField(MsgEventName,
                                            ref _addEventName,
                                            128,
                                            "##eventName",
                                            MsgEventNameHelp,
                                            allowEnter: true,
                                            filter: new ImGuiWidgets.FilenameCharactersFilter()))
            {
                HandleAddEventAccept();
            }

            ImGuiWidgets.VerticalSpace();
            ImGui.Separator();
            ImGuiWidgets.VerticalSpace();

            if (ImGui.Button(MsgButtonCreate, new Vector2(120, 0))) HandleAddEventAccept();
            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0)))
            {
                _addEventName = "";
                _addEventAlreadyExists = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void HandleAddEventAccept()
    {
        _addEventAlreadyExists = false;
        var eventName = _addEventName.Trim();
        if (!IsNullOrWhiteSpace(eventName))
        {
            if (_configuration.ChatLogs.TryAdd(eventName,
                                               new ChatLogConfiguration(eventName,
                                                                        isEvent: true,
                                                                        includeAllUsers: true)))
            {
                _addEventName = "";
                HandleSelectGroup(eventName);
                ImGui.CloseCurrentPopup();
            }
            else
                _addEventAlreadyExists = true;
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
            var outerSize = new Vector2(-1, textBaseHeight * 8);
            if (ImGui.BeginListBox("##friendList", outerSize))
            {
                foreach (var filteredFriend in _filteredFriends)
                    if (ImGui.Selectable(filteredFriend.FullName, filteredFriend.FullName == _selectedFriend))
                        _selectedFriend = filteredFriend.FullName;

                ImGui.EndListBox();
            }

            ImGui.Separator();
            if (ImGui.Button(MsgButtonAdd, new Vector2(120, 0)))
            {
                if (!IsNullOrWhiteSpace(_selectedFriend)) targetUserFullName = _selectedFriend;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawClearFilterButton()
    {
        return ImGuiWidgets.DrawIconButton("clearFilter", FontAwesomeIcon.SquareXmark, MsgButtonClearFilterHelp);
    }

    /// <summary>
    ///     Draws the button that brings up the friend selection dialog.
    /// </summary>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawFindFriendButton()
    {
        return ImGuiWidgets.DrawIconButton("findFriend", FontAwesomeIcon.PersonCirclePlus, MsgButtonFriendSelectorHelp);
    }

    /// <summary>
    ///     Draws the button that brings up the remove user dialog.
    /// </summary>
    /// <param name="id">The unique id of this object being deleted.</param>
    /// <param name="disabled">True if this button should be disabled.</param>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawRemoveButton(string id, bool disabled = false)
    {
        return ImGuiWidgets.DrawIconButton($"Trash-{id}", FontAwesomeIcon.Trash, "Remove the user", disabled);
    }

    /// <summary>
    ///     Draws the button that brings up the remove user dialog.
    /// </summary>
    /// <param name="id">The unique id of this object being deleted.</param>
    /// <param name="disabled">True if this button should be disabled.</param>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawDeleteGroupButton(string id, bool disabled = false)
    {
        return ImGuiWidgets.DrawIconButton($"Trash-{id}", FontAwesomeIcon.Trash, "Delete the group", disabled);
    }

    /// <summary>
    ///     Draws a copy to clipboard icon button.
    /// </summary>
    /// <param name="id">The unique id of this object being deleted.</param>
    /// <param name="disabled">True if this button should be disabled.</param>
    /// <returns><c>true</c> if the button was pressed.</returns>
    private bool DrawCopyButton(string id, bool disabled = false)
    {
        return ImGuiWidgets.DrawIconButton($"Copy-{id}", FontAwesomeIcon.Copy, "Copy to clipboard", disabled);
    }

    /// <summary>
    ///     Draws the remove user dialog.
    /// </summary>
    private void DrawRemoveUserDialog()
    {
        if (ImGui.BeginPopupModal(MsgTitleRemove, ref _removeDialogIsOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            using (ImGuiWith.TextWrapPos(300)) ImGui.TextUnformatted(Format(MsgRemoveUser, _removeDialogUser));
            ImGui.Separator();

            if (ImGui.Button(MsgButtonRemove, new Vector2(120, 0)))
            {
                _removeUser = _removeDialogUser;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the remove group dialog.
    /// </summary>
    private void DrawDeleteGroupDialog()
    {
        if (ImGui.BeginPopupModal(MsgTitleDelete, ref _deleteGroupDialogIsOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            using (ImGuiWith.TextWrapPos(300)) ImGui.TextUnformatted(Format(MsgLabelDeleteGroup, _deleteDialogGroup));
            ImGui.Separator();

            if (ImGui.Button(MsgButtonDelete, new Vector2(120, 0)))
            {
                _deleteGroup = _deleteDialogGroup;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button(MsgButtonCancel, new Vector2(120, 0))) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    ///     Draws the list of groups for selecting into the editor.
    /// </summary>
    private void DrawGroupsList()
    {
        using (ImGuiWith.Style(ImGuiStyleVar.ChildRounding, 5))
        {
            ImGui.BeginChild("groupsChild",
                             new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, ImGui.GetContentRegionAvail().Y),
                             true);
            if (ImGui.Button(MsgButtonAddGroup)) ImGui.OpenPopup("addGroup");
            DrawAddGroupPopup();
            ImGui.SameLine();
            if (ImGui.Button(MsgButtonAddEvent)) ImGui.OpenPopup("addEvent");
            DrawAddEventPopup();

            if (ImGui.BeginListBox("##groups",
                                   new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)))
            {
                foreach (var (_, cl) in _configuration.ChatLogs)
                {
                    var isSelected = _selectedGroup == cl.Name;
                    if (ImGui.Selectable(cl.Title, isSelected)) HandleSelectGroup(cl.Name);
                    if (ImGui.IsItemHovered()) ImGuiWidgets.DrawTooltip(cl.Title);
                    if (isSelected) ImGui.SetItemDefaultFocus();
                }

                ImGui.EndListBox();
            }

            ImGui.EndChild();
        }
    }

    private void HandleSelectGroup(string name)
    {
        _selectedGroup = name;
        var chatLog = _configuration.ChatLogs[name];
        _timeStampSelected = 0; // Default in case we don't find it
        for (var i = 0; i < _dateOptions.Count; i++)
            if (_dateOptions[i].Value == chatLog.Format)
            {
                _timeStampSelected = i;
                break;
            }
    }

    /// <summary>
    ///     Draws all the chat flags in a given list.
    /// </summary>
    /// <param name="id">The id for the list of items.</param>
    /// <param name="chatLog">The chat log configuration.</param>
    /// <param name="flagList">The list of flags to draw. If a flag is in the list but not set in the config it is ignored.</param>
    private void DrawChatTypeFlags(string id, ChatLogConfiguration chatLog, ChatTypeFlagList flagList)
    {
        ImGui.Spacing();
        var flags = chatLog.ChatTypeFilterFlags;
        if (ImGui.BeginTable(id, 4))
        {
            foreach (var flag in flagList)
            {
                if (flags.TryGetValue(flag.Type, out var flagValue)) DrawFlag(flag, chatLog, ref flagValue.Value);
            }

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
    private void DrawFlag(ChatTypeFlagInfo info, ChatLogConfiguration chatLog, ref bool flag)
    {
        ImGui.TableNextColumn();
        if (ImGui.Checkbox(_loc.Message(info.Label), ref flag)) info.OnChange?.Invoke(chatLog);
        ImGuiWidgets.HelpMarker(_loc.Message(info.Help));
    }

    /// <summary>
    ///     Defines the display information for a chat type flag.
    /// </summary>
    private class ChatTypeFlagInfo(
        XivChatType type,
        string label,
        string help,
        Action<ChatLogConfiguration>? onChange = null)
    {
        public XivChatType Type { get; } = type;
        public string Label { get; } = label;
        public string Help { get; } = help;
        public Action<ChatLogConfiguration>? OnChange { get; } = onChange;
    }

    /// <summary>
    ///     Helper type to make creating a list of tuples easier.
    /// </summary>
    private class ChatTypeFlagList : List<ChatTypeFlagInfo>

    {
        public void Add(XivChatType type, string label, string help, Action<ChatLogConfiguration>? onChange = null)
        {
            Add(new ChatTypeFlagInfo(type, label, help, onChange));
        }
    }
}

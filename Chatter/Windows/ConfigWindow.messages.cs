﻿// Copyright 2023 James Keesey
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

namespace Chatter.Windows;

/// <summary>
///     These are the localized strings that are used in the configuration window.
/// </summary>
public partial class ConfigWindow
{
    private string MsgButtonAdd => _loc.Message("Button.Add");
    private string MsgButtonAddEvent => _loc.Message("Button.AddEvent");
    private string MsgButtonAddGroup => _loc.Message("Button.AddGroup");
    private string MsgButtonAddUser => _loc.Message("Button.AddUser");
    private string MsgButtonCancel => _loc.Message("Button.Cancel");
    private string MsgButtonClearFilterHelp => _loc.Message("Button.ClearFilter.Help");
    private string MsgButtonCreate => _loc.Message("Button.Create");
    private string MsgButtonDelete => _loc.Message("Button.Delete");
    private string MsgButtonFriendSelectorHelp => _loc.Message("Button.FriendSelector.Help");
    private string MsgButtonRemove => _loc.Message("Button.Remove");
    private string MsgButtonRestart => _loc.Message("Button.Restart");
    private string MsgButtonRestartHelp => _loc.Message("Button.Restart.Help");
    private string MsgColumnFullName => _loc.Message("ColumnHeader.FullName");
    private string MsgColumnReplacement => _loc.Message("ColumnHeader.Replacement");
    private string MsgComboDirectoryFormHelp => _loc.Message("Combo.Directory.Help");
    private string MsgComboDirectoryFormLabel => _loc.Message("Combo.Directory.Label");
    private string MsgComboDirectoryGroup => _loc.Message("Combo.Directory.Group");
    private string MsgComboDirectoryGroupHelp => _loc.Message("Combo.Directory.Group.Help");
    private string MsgComboDirectoryGroupYearMonth => _loc.Message("Combo.Directory.GroupYearMonth");
    private string MsgComboDirectoryGroupYearMonthHelp => _loc.Message("Combo.Directory.GroupYearMonth.Help");
    private string MsgComboDirectoryUnified => _loc.Message("Combo.Directory.Unified");
    private string MsgComboDirectoryUnifiedHelp => _loc.Message("Combo.Directory.Unified.Help");
    private string MsgComboDirectoryYearMonth => _loc.Message("Combo.Directory.YearMonth");
    private string MsgComboDirectoryYearMonthGroup => _loc.Message("Combo.Directory.YearMonthGroup");
    private string MsgComboDirectoryYearMonthGroupHelp => _loc.Message("Combo.Directory.YearMonthGroup.Help");
    private string MsgComboDirectoryYearMonthHelp => _loc.Message("Combo.Directory.YearMonth.Help");
    private string MsgComboOrderDateGroup => _loc.Message("Combo.Order.DateGroup");
    private string MsgComboOrderDateGroupHelp => _loc.Message("Combo.Order.DateGroup.Help");
    private string MsgComboOrderGroupDate => _loc.Message("Combo.Order.GroupDate");
    private string MsgComboOrderGroupDateHelp => _loc.Message("Combo.Order.GroupDate.Help");
    private string MsgComboOrderHelp => _loc.Message("Combo.Order.Help");
    private string MsgComboOrderLabel => _loc.Message("Combo.Order.Label");
    private string MsgComboTimestampCultural => _loc.Message("Combo.Timestamp.Cultural");
    private string MsgComboTimestampCulturalHelp => _loc.Message("Combo.Timestamp.Cultural.Help");
    private string MsgComboTimestampHelp => _loc.Message("Combo.Timestamp.Help");
    private string MsgComboTimestampLabel => _loc.Message("Combo.Timestamp.Label");
    private string MsgComboTimestampSortable => _loc.Message("Combo.Timestamp.Sortable");
    private string MsgComboTimestampSortableHelp => _loc.Message("Combo.Timestamp.Sortable.Help");
    private string MsgDescriptionIncludedUsers => _loc.Message("Description.IncludedUsers");
    private string MsgEventAlreadyInList => _loc.Message("Message.EventAlreadyInList");
    private string MsgEventName => _loc.Message("Label.EventName");
    private string MsgEventNameHelp => _loc.Message("Label.EventName.Help");
    private string MsgGroupAlreadyInList => _loc.Message("Message.GroupAlreadyInList");
    private string MsgGroupName => _loc.Message("Label.GroupName");
    private string MsgGroupNameHelp => _loc.Message("Label.GroupName.Help");
    private string MsgHeaderIncludedChatTypes => _loc.Message("Header.IncludedChatTypes");
    private string MsgHeaderIncludedUsers => _loc.Message("Header.IncludedUsers");
    private string MsgInputWrapIndentHelp => _loc.Message("Input.WrapIndent.Help");
    private string MsgInputWrapIndentLabel => _loc.Message("Input.WrapIndent.Label");
    private string MsgInputWrapWidthHelp => _loc.Message("Input.WrapWidth.Help");
    private string MsgInputWrapWidthLabel => _loc.Message("Input.WrapWidth.Label");
    private string MsgLabelDeleteGroup => _loc.Message("Label.Delete.Group");
    private string MsgLabelEventLength => _loc.Message("Label.EventLength");
    private string MsgLabelEventLengthHelp => _loc.Message("Label.EventLength.Help");
    private string MsgLabelFileNamePrefix => _loc.Message("Label.FileNamePrefix");
    private string MsgLabelFileNamePrefixHelp => _loc.Message("Label.FileNamePrefix.Help");
    private string MsgLabelIncludeAll => _loc.Message("Label.IncludeAll.Checkbox");
    private string MsgLabelIncludeAllHelp => _loc.Message("Label.IncludeAll.Checkbox.Help");
    private string MsgLabelIncludeAllUsers => _loc.Message("Label.IncludeAllUsers.Checkbox");
    private string MsgLabelIncludeAllUsersHelp => _loc.Message("Label.IncludeAllUsers.Checkbox.Help");
    private string MsgLabelIncludeSelf => _loc.Message("Label.IncludeSelf.Checkbox");
    private string MsgLabelIncludeSelfHelp => _loc.Message("Label.IncludeSelf.Checkbox.Help");
    private string MsgLabelIncludeServerName => _loc.Message("Label.IncludeServerName.Checkbox");
    private string MsgLabelIncludeServerNameHelp => _loc.Message("Label.IncludeServerName.Checkbox.Help");
    private string MsgLabelIsActive => _loc.Message("Label.IsActive.Checkbox");
    private string MsgLabelIsActiveHelp => _loc.Message("Label.IsActive.Checkbox.Help");
    private string MsgLabelIsEvent => _loc.Message("Label.IsEvent.Checkbox");
    private string MsgLabelIsEventHelp => _loc.Message("Label.IsEvent.Checkbox.Help");
    private string MsgLabelSaveDirectory => _loc.Message("Label.SaveDirectory");
    private string MsgLabelSaveDirectoryHelp => _loc.Message("Label.SaveDirectory.Help");
    private string MsgShowMinimalMain => _loc.Message("Label.ShowMinimalMain");
    private string MsgShowMinimalMainHelp => _loc.Message("Label.ShowMinimalMain.Help");
    private string MsgPlayerAlreadyInList => _loc.Message("Message.PlayerAlreadyInList");
    private string MsgPlayerFullName => _loc.Message("Label.PlayerFullName");
    private string MsgPlayerFullNameHelp => _loc.Message("Label.PlayerFullName.Help");
    private string MsgPlayerReplacement => _loc.Message("Label.PlayerReplacement");
    private string MsgPlayerReplacementHelp => _loc.Message("Label.PlayerReplacement.Help");
    private string MsgRemoveUser => _loc.Message("Text.RemoveUser");
    private string MsgTabGeneral => _loc.Message("Tab.General");
    private string MsgTabGroups => _loc.Message("Tab.Groups");
    private string MsgTitleDelete => _loc.Message("Title.Delete");
    private string MsgTitleRemove => _loc.Message("Title.Remove");
}

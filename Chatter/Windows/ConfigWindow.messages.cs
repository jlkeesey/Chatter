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

namespace Chatter.Windows;

/// <summary>
///     These are the localized strings that are used in the configuration window.
/// </summary>
public partial class ConfigWindow
{
    private string MsgButtonAdd => _loc.Message("Button.Add");
    private string MsgButtonAddUser => _loc.Message("Button.AddUser");
    private string MsgButtonCancel => _loc.Message("Button.Cancel");
    private string MsgButtonClearFilterHelp => _loc.Message("Button.ClearFilter.Help");
    private string MsgButtonFriendSelectorHelp => _loc.Message("Button.FriendSelector.Help");
    private string MsgButtonRemove => _loc.Message("Button.Remove");
    private string MsgColumnFullName => _loc.Message("ColumnHeader.FullName");
    private string MsgColumnReplacement => _loc.Message("ColumnHeader.Replacement");
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
    private string MsgHeaderIncludedChatTypes => _loc.Message("Header.IncludedChatTypes");
    private string MsgHeaderIncludedUsers => _loc.Message("Header.IncludedUsers");
    private string MsgInputWrapIndentHelp => _loc.Message("Input.WrapIndent.Help");
    private string MsgInputWrapIndentLabel => _loc.Message("Input.WrapIndent.Label");
    private string MsgInputWrapWidthHelp => _loc.Message("Input.WrapWidth.Help");
    private string MsgInputWrapWidthLabel => _loc.Message("Input.WrapWidth.Label");
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
    private string MsgLabelSaveDirectory => _loc.Message("Label.SaveDirectory");
    private string MsgLabelSaveDirectoryHelp => _loc.Message("Label.SaveDirectory.Help");
    private string MsgPlayerAlreadyInList => _loc.Message("Message.PlayerAlreadyInList");
    private string MsgPlayerFullName => _loc.Message("Label.PlayerFullName");
    private string MsgPlayerFullNameHelp => _loc.Message("Label.PlayerFullName.Help");
    private string MsgPlayerReplacement => _loc.Message("Label.PlayerReplacement");
    private string MsgPlayerReplacementHelp => _loc.Message("Label.PlayerReplacement.Help");
    private string MsgTabGeneral => _loc.Message("Tab.General");
    private string MsgTabGroups => _loc.Message("Tab.Groups");
}

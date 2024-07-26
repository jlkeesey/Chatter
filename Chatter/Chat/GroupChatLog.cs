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

using Chatter.Model;
using Chatter.Reporting;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Chat log for group based logs.
/// </summary>
public class GroupChatLog(
    Configuration configuration,
    ChatLogConfiguration logConfiguration,
    LogFileInfo logFileInfo,
    IDateHelper dateHelper,
    FileHelper fileHelper,
    IPlayer myself,
    IErrorWriter errorWriter)
    : ChatLog(configuration, logConfiguration, logFileInfo, dateHelper, fileHelper, errorWriter)
{
    protected override string DefaultFormat => "{6,22} {4,-30} {5}";

    /// <inheritdoc />
    public override bool ShouldLog(ChatMessage chatMessage)
    {
        if (!base.ShouldLog(chatMessage)) return false;
        if (LogConfiguration.IncludeAllUsers) return true;
        var fullSender = chatMessage.Sender.AsText(true);
        if (LogConfiguration.Users.ContainsKey(fullSender)) return true;
        return LogConfiguration.IncludeMe && fullSender == myself.FullName;
    }
}

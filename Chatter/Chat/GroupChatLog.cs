using Chatter.Model;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Chat log for group based logs.
/// </summary>
public class GroupChatLog : ChatLog
{
    private readonly IPlayer _myself;

    public GroupChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper, IPlayer myself) : base(configuration, logFileInfo, dateHelper, fileHelper)
    {
        _myself = myself;
    }

    protected override string DefaultFormat => "{6,22} {4,-30} {5}";

    public override bool ShouldLog(ChatMessage chatMessage, string cleanedSender)
    {
        if (!base.ShouldLog(chatMessage, cleanedSender)) return false;
        if (LogConfiguration.IncludeAllUsers) return true;
        if (LogConfiguration.Users.ContainsKey(cleanedSender)) return true;
        return LogConfiguration.IncludeMe && cleanedSender == _myself.FullName;
    }
}
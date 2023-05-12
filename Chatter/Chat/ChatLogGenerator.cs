using Chatter.Model;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
/// Standard chat log generator for runtime.
/// </summary>
public class ChatLogGenerator : IChatLogGenerator
{
    public IChatLog Create(ChatLogConfiguration logConfiguration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper, IPlayer myself)
    {
        return logConfiguration.Name == AllLogName
            ? new AllChatLog(logConfiguration, logFileInfo, dateHelper, fileHelper)
            : new GroupChatLog(logConfiguration, logFileInfo, dateHelper, fileHelper, myself);
    }
}
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

/// <summary>
///     Chat log for the log that record everything.
/// </summary>
public class AllChatLog : ChatLog
{
    public AllChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper) : base(configuration, logFileInfo, dateHelper, fileHelper)
    {
    }

    protected override string DefaultFormat => "{2}:{0}:{5}";
}
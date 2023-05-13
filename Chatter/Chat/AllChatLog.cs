using Chatter.System;
using NodaTime;
using NodaTime.Text;
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

    protected override string FormatWhen(ZonedDateTime when)
    {
        var instant = when.ToInstant();
        var pattern = InstantPattern.ExtendedIso;
        return pattern.Format(instant);
    }

    protected override string DefaultFormat => "{6}:{2}:{0}:{5}";
}
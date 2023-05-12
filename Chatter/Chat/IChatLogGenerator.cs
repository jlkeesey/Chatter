using Chatter.Model;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.Chat;

public interface IChatLogGenerator
{
    IChatLog Create(ChatLogConfiguration logConfiguration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileHelper fileHelper, IPlayer myself);
}
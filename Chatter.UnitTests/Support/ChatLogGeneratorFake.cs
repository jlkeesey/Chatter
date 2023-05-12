using Chatter.Chat;
using Chatter.Model;
using Chatter.System;

namespace Chatter.UnitTests.Support;

internal class ChatLogGeneratorFake : IChatLogGenerator
{
    private readonly IChatLog _chatLog;

    public ChatLogGeneratorFake(IChatLog chatLog)
    {
        _chatLog = chatLog;
    }

    public int Count { get; private set; }

    public IChatLog Create(Configuration.ChatLogConfiguration logConfiguration, LogFileInfo logFileInfo,
        IDateHelper dateHelper,
        FileHelper fileHelper, IPlayer myself)
    {
        Count++;
        return _chatLog;
    }
}
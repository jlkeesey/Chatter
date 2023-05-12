using Chatter.Chat;
using Chatter.System;
using static Chatter.Configuration;

namespace Chatter.UnitTests.Support;

internal class TestChatLog : ChatLog
{
    public TestChatLog(string name) : this(new ChatLogConfiguration(name))
    {
        LogConfiguration.InitializeTypeFlags();
        LogConfiguration.IsActive = true;
    }

    public TestChatLog(ChatLogConfiguration configuration) :
        this(configuration, new LogFileInfo(), new DateHelperFake(), new FileSystemFake())
    {
    }

    private TestChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileSystemFake fileSystem) : base(configuration, logFileInfo, dateHelper, new FileHelper(fileSystem))
    {
        FileSystem = fileSystem;
        Configuration = new Configuration();
        Configuration.Initialize(FileHelper);
        logFileInfo.UpdateConfigValues(Configuration);
    }

    public Configuration Configuration { get; }

    public FileSystemFake FileSystem { get; }

    public new DateHelperFake DateHelper => (base.DateHelper as DateHelperFake)!;

    public new ChatLogConfiguration LogConfiguration => base.LogConfiguration;

    public new LogFileInfo LogFileInfo => base.LogFileInfo;

    protected override string DefaultFormat => "{6} {4}: {5}";
}
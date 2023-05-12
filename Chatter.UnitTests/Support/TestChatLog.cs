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
        this(configuration, new LogFileInfo(), new DateHelper(), new FileSystemFake())
    {
    }

    private TestChatLog(ChatLogConfiguration configuration, LogFileInfo logFileInfo, IDateHelper dateHelper,
        FileSystemFake fileSystem) : base(configuration, logFileInfo, dateHelper, new FileHelper(fileSystem))
    {
        FileSystem = fileSystem;
        Configuration = new Configuration();
        Configuration.Initialize(_fileHelper);
        logFileInfo.UpdateConfigValues(Configuration);
    }

    public Configuration Configuration { get; }

    public FileSystemFake FileSystem { get; }

    public new IDateHelper DateHelper => base.DateHelper;

    public new ChatLogConfiguration LogConfiguration => base.LogConfiguration;

    public new LogFileInfo LogFileInfo => base.LogFileInfo;

    protected override string DefaultFormat => "{2}:  {5}";
}
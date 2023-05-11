using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatter.System;
using Chatter.UnitTests.Support;
using Dalamud.Game.Text;
using System.Reflection;
using static Chatter.Configuration;
using Chatter.Chat;

namespace Chatter.UnitTests;

[TestFixture()]
public class ChatLogManagerTests
{
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class LogInfoTests
    {
        private readonly DateHelperFake _dateHelper;
        private readonly Configuration _configuration;
        private readonly FileSystemFake _fileSystem;
        private readonly FileHelper _fileHelper;
        private readonly PlayerFake _myself;
        private readonly ChatLogManager _chatLogManager;
        private readonly PlayerFake _senderPlayer;
        private readonly ChatString _sender;
        private readonly ChatString _body;
        private readonly ChatMessage _chatMessage;

        public LogInfoTests()
        {
            _fileSystem = new FileSystemFake();
            _fileHelper = new FileHelper(_fileSystem);
            _configuration = new Configuration();
            _configuration.Initialize(_fileHelper);
            _dateHelper = new DateHelperFake();
            _myself = new PlayerFake("Bob Jones");
            _chatLogManager = new ChatLogManager(_configuration, _dateHelper, _fileHelper, _myself);
            _senderPlayer = new PlayerFake("Wolf Gold");
            _sender = ChatString.FromPlayer(_senderPlayer);
            _body = ChatString.FromText("This is the body.");
            _chatMessage = new ChatMessage(XivChatType.Say, "say", 123, _sender, _body, _dateHelper.ZonedNow);
        }

        [Test()]
        public void LogInfo_NoLogsConfigured()
        {
            _configuration.RemoveAllLogs();

            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(0, _fileSystem.Writers.Count);
        }

        [Test()]
        public void LogInfo_OneLogConfigured()
        {
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(2, _fileSystem.Writers.Count);
            Assert.True(_chatLogManager.GetLog(AllLogName).IsOpen);
            Assert.True(_chatLogManager.GetLog("Pimpernel")?.IsOpen ?? false);
        }
    }
}
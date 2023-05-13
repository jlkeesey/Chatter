using System.Collections.Generic;
using Chatter.Chat;
using Chatter.System;
using Chatter.UnitTests.Support;
using Dalamud.Game.Text;
using Moq;
using NUnit.Framework;
using static Chatter.Configuration;

namespace Chatter.UnitTests.Chat;

[TestFixture]
public class ChatLogManagerTests
{
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class LogInfoTests
    {
        private readonly ChatLogGeneratorFake _chatLogGenerator;
        private readonly ChatLogManager _chatLogManager;
        private readonly Mock<IChatLog> _chatLogMock;
        private readonly ChatMessage _chatMessage;
        private readonly Configuration _configuration;

        public LogInfoTests()
        {
            _chatLogMock = new Mock<IChatLog>(MockBehavior.Strict);
            _chatLogMock.Setup(cl => cl.LogInfo(It.IsAny<ChatMessage>()));
            var chatLog = _chatLogMock.Object!;
            _chatLogGenerator = new ChatLogGeneratorFake(chatLog);

            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);
            _configuration = new Configuration();
            _configuration.Initialize(fileHelper);
            _configuration.WhenToCloseLogs = "18:00";
            var dateHelper = new DateHelperFake();
            var myself = new PlayerFake("Bob Jones");
            _chatLogManager = new ChatLogManager(_configuration, dateHelper, fileHelper, myself, _chatLogGenerator);
            var senderPlayer = new PlayerFake("Wolf Gold");
            var sender = ChatString.FromPlayer(senderPlayer);
            var body = ChatString.FromText("This is the body.");
            _chatMessage = new ChatMessage(XivChatType.Say, "say", 123, sender, body, dateHelper.ZonedNow);
        }

        [Test]
        public void LogInfo_NoLogsConfigured()
        {
            _configuration.RemoveAllLogs();

            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(0, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Never);
        }

        [Test]
        public void LogInfo_LogsConfigured()
        {
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(3, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Exactly(3));
        }

        [Test]
        public void LogInfo_WhenToClose()
        {
            _chatLogMock.Setup(cl => cl.Close());

            _chatLogManager.LogInfo(_chatMessage);
            _configuration.WhenToCloseLogs = "17:00"; // Force close
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(3, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Exactly(6));
            _chatLogMock.Verify(cl => cl.Close(), Times.Exactly(3));
        }

        [Test]
        public void LogInfo_UpdateLogDirectory()
        {
            _chatLogMock.Setup(cl => cl.Close());

            _chatLogManager.LogInfo(_chatMessage);
            _configuration.LogDirectory = "C:\\A\\B\\C"; // Force close
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(3, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Exactly(6));
            _chatLogMock.Verify(cl => cl.Close(), Times.Exactly(3));
        }

        [Test]
        public void LogInfo_UpdateLogFileNamePrefix()
        {
            _chatLogMock.Setup(cl => cl.Close());

            _chatLogManager.LogInfo(_chatMessage);
            _configuration.LogFileNamePrefix = "talker"; // Force close
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(3, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Exactly(6));
            _chatLogMock.Verify(cl => cl.Close(), Times.Exactly(3));
        }

        [Test]
        public void LogInfo_UpdateLogOrder()
        {
            _chatLogMock.Setup(cl => cl.Close());

            _chatLogManager.LogInfo(_chatMessage);
            _configuration.LogOrder = FileNameOrder.PrefixDateGroup; // Force close
            _chatLogManager.LogInfo(_chatMessage);

            Assert.AreEqual(3, _chatLogGenerator.Count);
            _chatLogMock.Verify(cl => cl.LogInfo(It.IsAny<ChatMessage>()), Times.Exactly(6));
            _chatLogMock.Verify(cl => cl.Close(), Times.Exactly(3));
        }
    }

    public class DisposeTests
    {
        private ChatLogManager _chatLogManager = null!;
        private ChatMessage _chatMessage = null!;
        private Mock<IChatLog> _chatLogMock = null!;

        [SetUp]
        public void Setup()
        {
            _chatLogMock = new Mock<IChatLog>(MockBehavior.Strict);
            var chatLog = _chatLogMock.Object!;
            var chatLogGenerator = new ChatLogGeneratorFake(chatLog);

            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);
            var configuration = new Configuration();
            configuration.Initialize(fileHelper);
            configuration.WhenToCloseLogs = "18:00";
            var dateHelper = new DateHelperFake();
            var myself = new PlayerFake("Bob Jones");
            _chatLogManager = new ChatLogManager(configuration, dateHelper, fileHelper, myself, chatLogGenerator);
            var senderPlayer = new PlayerFake("Wolf Gold");
            var sender = ChatString.FromPlayer(senderPlayer);
            var body = ChatString.FromText("This is the body.");
            _chatMessage = new ChatMessage(XivChatType.Say, "say", 123, sender, body, dateHelper.ZonedNow);
        }

        [Test]
        public void Dispose_BeforeLogging()
        {
            _chatLogManager.Dispose();

            _chatLogMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Dispose_WithLogs()
        {
            _chatLogMock.Setup(cl => cl.Close());
            _chatLogMock.Setup(cl => cl.LogInfo(It.IsAny<ChatMessage>()));
            _chatLogManager.LogInfo(_chatMessage);

            _chatLogManager.Dispose();

            _chatLogMock.Verify(cl => cl.Close(), Times.Exactly(3));
        }
    }

    public class DumpLogsTests
    {
        private ChatLogManager _chatLogManager = null!;
        private ChatMessage _chatMessage = null!;

        [SetUp]
        public void Setup()
        {
            var chatLogMock = new Mock<IChatLog>(MockBehavior.Strict);
            chatLogMock.Setup(cl => cl.LogInfo(It.IsAny<ChatMessage>()));
            chatLogMock.Setup(cl => cl.DumpLog(It.IsAny<ILogger>()));
            var chatLog = chatLogMock.Object!;
            var chatLogGenerator = new ChatLogGeneratorFake(chatLog);

            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);
            var configuration = new Configuration();
            configuration.Initialize(fileHelper);
            configuration.WhenToCloseLogs = "18:00";
            var dateHelper = new DateHelperFake();
            var myself = new PlayerFake("Bob Jones");
            _chatLogManager = new ChatLogManager(configuration, dateHelper, fileHelper, myself, chatLogGenerator);
            var senderPlayer = new PlayerFake("Wolf Gold");
            var sender = ChatString.FromPlayer(senderPlayer);
            var body = ChatString.FromText("This is the body.");
            _chatMessage = new ChatMessage(XivChatType.Say, "say", 123, sender, body, dateHelper.ZonedNow);
        }

        [Test]
        public void DumpLogs_empty()
        {
            var expected = new List<string>
            {
                "[L]: Prefix        Open   Path",
                "[L]: ------------  -----  ----",
            };
            var logger = new LoggerFake();

            _chatLogManager.DumpLogs(logger);

            Assert.That(expected, Is.EqualTo(logger.Lines).AsCollection);
        }

        [Test]
        public void DumpLogs_WithLogs()
        {
            var expected = new List<string>
            {
                "[L]: Prefix        Open   Path",
                "[L]: ------------  -----  ----",
            };
            var logger = new LoggerFake();
            _chatLogManager.LogInfo(_chatMessage);

            _chatLogManager.DumpLogs(logger);

            Assert.That(expected, Is.EqualTo(logger.Lines).AsCollection);
        }
    }
}
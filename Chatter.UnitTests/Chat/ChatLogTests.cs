using Chatter.Chat;
using Chatter.Model;
using Chatter.UnitTests.Support;
using Dalamud.Game.Text;
using NodaTime;
using NUnit.Framework;
using System.Collections.Generic;
namespace Chatter.UnitTests.Chat;

[TestFixture]
public class ChatLogTests
{
    private static readonly ChatTypeHelper ChatTypeHelper = new();
    private static readonly IPlayer DefaultPlayer = new PlayerFake("Robert Jones");

    private static ChatMessage CreateMessage(ZonedDateTime when, XivChatType chatType = XivChatType.Say,
        string? typeLabel = null, uint senderId = 123, IPlayer? sender = null, string body = "This is a test")
    {
        var chatSender = ChatString.FromPlayer(sender ?? DefaultPlayer);
        var chatBody = ChatString.FromText(body);
        return new ChatMessage(chatType, typeLabel ?? ChatTypeHelper.TypeToName(chatType), senderId, chatSender,
            chatBody, when);
    }

    public class ShouldLogTests
    {
        [Test]
        public void ShouldLog_Defaults()
        {
            var logConfig = new Configuration.ChatLogConfiguration("test");
            var chatLog = new TestChatLog(logConfig);
            var message = CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_AllMessages()
        {
            var chatLog = new TestChatLog("test");
            chatLog.LogConfiguration.DebugIncludeAllMessages = true;
            var message = CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.True);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeMatches()
        {
            var chatLog = new TestChatLog("test");
            var message = CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.True);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeMatchesOff()
        {
            var chatLog = new TestChatLog("test");
            var message = CreateMessage(chatLog.DateHelper.ZonedNow);
            chatLog.LogConfiguration.ChatTypeFilterFlags[message.ChatType].Value = false;

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeNoLabel()
        {
            var chatLog = new TestChatLog("test");
            var message = CreateMessage(chatLog.DateHelper.ZonedNow, typeLabel: string.Empty);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }
    }

    public class WriteLogTests
    {
        [Test]
        public void WriteLog_FirstWrite()
        {
            var expected = new List<string>()
            {
                "============================== Thursday, May 11, 2023 ==============================",
                "Robert Jones@Zalera:  This is a test",
            };
            var chatLog = new TestChatLog("test");
            var message = CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(expected, Is.EqualTo(lines).AsCollection);
        }
    }
}
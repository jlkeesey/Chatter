using NUnit.Framework;
using Chatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatter.System;
using Chatter.UnitTests.Support;
using Dalamud.Game.Text;

namespace Chatter.UnitTests;

[TestFixture()]
public class ChatLogManagerTests
{
    public class LogInfoTests
    {
        [Test()]
        public void LogInfo()
        {
            var dateHelper = new DateHelperFake();
            var chatLongManager = CreateManager();
            var player = new PlayerFake("Wolf Gold");
            var sender = ChatString.FromPlayer(player);
            var body = ChatString.FromText("This is the body.");
            var chatMessage = new ChatMessage(XivChatType.Say, "say", 123, sender, body, dateHelper.ZonedNow);

            chatLongManager.LogInfo(chatMessage);

            Assert.True(true);
        }

        private static ChatLogManager CreateManager()
        {
            var configuration = new Configuration();
            var dateHelper = new DateHelperFake();
            var fileSystem = new FileSystemFake();
            var fileHelper = new FileHelper(fileSystem);
            var myself = new PlayerFake("Bob Jones");
            var chatLogManager = new ChatLogManager(configuration, dateHelper, fileHelper, myself);

            return chatLogManager;
        }
    }
}
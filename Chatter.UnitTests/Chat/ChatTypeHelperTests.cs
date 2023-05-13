using Chatter.Chat;
using Dalamud.Game.Text;
using NUnit.Framework;

namespace Chatter.UnitTests.Chat;

[TestFixture]
public class ChatTypeHelperTests
{
    public class TypeToNameTests
    {
        [Test]
        public void TypeToNameTest_BaseName()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.Say);

            Assert.AreEqual("say", actual);
        }

        [Test]
        public void TypeToNameTest_ExtName()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.TellOutgoing);

            Assert.AreEqual("tellOut", actual);
        }

        [Test]
        public void TypeToNameTest_Override()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.CustomEmote);

            Assert.AreEqual("emote", actual);
        }

        [Test]
        public void TypeToNameTest_Undefined()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.GatheringSystemMessage);

            Assert.AreEqual("", actual);
        }

        [Test]
        public void TypeToNameTest_ShowUnknown()
        {
            var chatTypeHelper = new ChatTypeHelper();
            var expected = $"?{(int)XivChatType.GatheringSystemMessage}?";

            var actual = chatTypeHelper.TypeToName(XivChatType.GatheringSystemMessage, true);

            Assert.AreEqual(expected, actual);
        }
    }
}
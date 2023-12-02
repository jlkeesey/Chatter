// Copyright 2023 James Keesey
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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

            Assert.That(actual, Is.EqualTo("say"));
        }

        [Test]
        public void TypeToNameTest_ExtName()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.TellOutgoing);

            Assert.That(actual, Is.EqualTo("tellOut"));
        }

        [Test]
        public void TypeToNameTest_Override()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.CustomEmote);

            Assert.That(actual, Is.EqualTo("emote"));
        }

        [Test]
        public void TypeToNameTest_Undefined()
        {
            var chatTypeHelper = new ChatTypeHelper();

            var actual = chatTypeHelper.TypeToName(XivChatType.GatheringSystemMessage);

            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void TypeToNameTest_ShowUnknown()
        {
            var chatTypeHelper = new ChatTypeHelper();
            var expected = $"?{(int)XivChatType.GatheringSystemMessage}?";

            var actual = chatTypeHelper.TypeToName(XivChatType.GatheringSystemMessage, true);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}

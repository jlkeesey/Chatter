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
using Dalamud.Game.Text.SeStringHandling;
using NUnit.Framework;

namespace Chatter.UnitTests.Chat;

[TestFixture]
public class ChatStringTests
{
    public class ConstructorSeStringTests
    {
        [Test]
        public void Constructor_Empty()
        {
            var builder = new SeStringBuilder();
            var seString = builder.Build();

            var chatString = new ChatString(seString);

            Assert.That(chatString.ToString().Equals(""));
        }

        [Test]
        public void Constructor_Text()
        {
            var builder = new SeStringBuilder();
            builder.AddText("This ");
            builder.AddText("is ");
            builder.AddText("some ");
            builder.AddText("text.");
            var seString = builder.Build();

            var chatString = new ChatString(seString);

            Assert.That(chatString.ToString().Equals("This is some text."));
        }

        // Can't do this test right now because PlayerPayload will try to read data from the FFXIV data sheets
        // which are not available at test time.
        // [Test]
        // public void Constructor_Player()
        // {
        //     var builder = new SeStringBuilder();
        //     builder.Add(new PlayerPayload("name", "world"));
        //     builder.AddText("is ");
        //     builder.AddText("some ");
        //     builder.AddText("test.");
        //     var seString = builder.Build();
        //
        //     var chatString = new ChatString(seString);
        //
        //     Assert.AreEqual("This is some text.", chatString.ToString());
        // }
    }

    public class HasInitialPlayerTests
    {
        [Test]
        public void HasInitialPlayerTest_NoInitialPlayer()
        {
            var cString = new ChatString(new ChatString.CsTextItem("text"));

            var result = cString.HasInitialPlayer();

            Assert.That(!result);
        }

        [Test]
        public void HasInitialPlayerTest_HasInitialPlayer()
        {
            var cString = new ChatString(new ChatString.CsPlayerItem("name", "world"));

            var result = cString.HasInitialPlayer();

            Assert.That(result);
        }
    }

    public class GetsInitialPlayerItemTests
    {
        [Test]
        public void GetsInitialPlayerItemTest_NoInitialPlayer()
        {
            var cString = new ChatString(new ChatString.CsTextItem("text"));

            var result = cString.GetInitialPlayerItem("player", "server");

            Assert.That(result.Name.Equals("player"));
            Assert.That(result.World.Equals("server"));
        }

        [Test]
        public void GetsInitialPlayerItemTest_HasInitialPlayer()
        {
            var cString = new ChatString(new ChatString.CsPlayerItem("name", "world"));

            var result = cString.GetInitialPlayerItem("player", "server");

            Assert.That(result.Name.Equals("name"));
            Assert.That(result.World.Equals("world"));
        }
    }

    public class ToStringTests
    {
        [Test]
        public void ToStringTest_Text()
        {
            var cString = new ChatString(new ChatString.CsTextItem("This is text"));

            var result = cString.ToString();

            Assert.That(result.Equals("This is text"));
        }

        [Test]
        public void ToStringTest_Player()
        {
            var cString = new ChatString(new ChatString.CsPlayerItem("name", "world"));

            var result = cString.ToString();

            Assert.That(result.Equals("name@world"));
        }
    }

    public class AsTextTests
    {
        [Test]
        public void AsTextTest_TextNoIncludeSever()
        {
            var cString = new ChatString(new ChatString.CsTextItem("This is text"));

            var result = cString.AsText(false);

            Assert.That(result.Equals("This is text"));
        }

        [Test]
        public void AsTextTest_TextIncludeSever()
        {
            var cString = new ChatString(new ChatString.CsTextItem("This is text"));

            var result = cString.AsText(true);

            Assert.That(result.Equals("This is text"));
        }

        [Test]
        public void AsTextTest_PlayerNoIncludeServer()
        {
            var cString = new ChatString(new ChatString.CsPlayerItem("name", "world"));

            var result = cString.AsText(false);

            Assert.That(result.Equals("name"));
        }

        [Test]
        public void AsTextTest_PlayerIncludeServer()
        {
            var cString = new ChatString(new ChatString.CsPlayerItem("name", "world"));

            var result = cString.AsText(true);

            Assert.That(result.Equals("name@world"));
        }
    }
}

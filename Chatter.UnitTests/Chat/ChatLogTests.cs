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

using Chatter.UnitTests.Support;
using NUnit.Framework;
using System.Collections.Generic;

namespace Chatter.UnitTests.Chat;

[TestFixture]
public class ChatLogTests
{
    [TestFixture]
    public class ShouldLogTests
    {
        [Test]
        public void ShouldLog_Defaults()
        {
            var logConfig = new Configuration.ChatLogConfiguration("test");
            var chatLog = new TestChatLog(logConfig);
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_AllMessages()
        {
            var chatLog = new TestChatLog("test");
            chatLog.LogConfiguration.DebugIncludeAllMessages = true;
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.True);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeMatches()
        {
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.True);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeMatchesOff()
        {
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            chatLog.LogConfiguration.ChatTypeFilterFlags[message.ChatType].Value = false;

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }

        [Test]
        public void ShouldLog_ChatTypeNoLabel()
        {
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow, typeLabel: string.Empty);

            var result = chatLog.ShouldLog(message);

            Assert.That(result, Is.False);
            Assert.That(chatLog.IsOpen, Is.False);
        }
    }

    [TestFixture]
    public class WriteLogTests
    {
        [Test]
        public void WriteLog_Basic()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "5-9-2023 17:00:01 Robert Jones [say]: This is a test",
                "5-9-2023 17:00:02 Robert Jones [say]: This is a test",
            };
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var message2 = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);
            chatLog.WriteLog(message2, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(lines, Is.EqualTo(expected).AsCollection);
        }

        [Test]
        public void WriteLog_ChangeDateFormat()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "2023-05-09 at 5:00 Robert Jones [say]: This is a test",
            };
            var chatLog = new TestChatLog("test");
            chatLog.LogConfiguration.DateTimeFormat = "yyyy-MM-dd 'at' h:mm";
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(expected, Is.EqualTo(lines).AsCollection);
        }

        [Test]
        public void WriteLog_ChangeMessageFormat()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "say Robert Jones@Zalera This is a test",
            };
            var chatLog = new TestChatLog("test");
            chatLog.LogConfiguration.Format = "{0} {2} {5}";
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(expected, Is.EqualTo(lines).AsCollection);
        }

        [Test]
        public void WriteLog_ReopenUsesDifferentFile()
        {
            var expected = new List<string> { "5-9-2023 17:00:02 Robert Jones [say]: This is a test", };
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var message2 = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);
            chatLog.Close();
            chatLog.LogFileInfo.StartTime = chatLog.DateHelper.ZonedNow;
            chatLog.WriteLog(message2, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            Assert.That(chatLog.FileName,
                        Is.EqualTo("C:\\Users\\Bob\\Documents\\FFXIV Chatter\\chatter-test-20230509-170004.log"));
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(lines, Is.EqualTo(expected).AsCollection);
        }

        [Test]
        public void WriteLog_ReopenUsesSameFile()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "5-9-2023 17:00:01 Robert Jones [say]: This is a test",
                "5-9-2023 17:00:02 Robert Jones [say]: This is a test",
            };
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var message2 = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);
            chatLog.Close();
            chatLog.WriteLog(message2, loggableSender, loggableBody);

            Assert.That(chatLog.IsOpen, Is.True);
            Assert.That(chatLog.FileName,
                        Is.EqualTo("C:\\Users\\Bob\\Documents\\FFXIV Chatter\\chatter-test-20230509-170000.log"));
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(expected, Is.EqualTo(lines).AsCollection);
        }

        [Test]
        public void WriteLog_LongLineNoWrap()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "5-9-2023 17:00:01 Robert Jones [say]: This is a long message so that we can test if the line wrapping code work, and works the way we expect it to.",
                "5-9-2023 17:00:02 Robert Jones [say]: This is a test",
            };
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow,
                                                         body:
                                                         "This is a long message so that we can test if the line wrapping code work, and works the way we expect it to.");
            var message2 = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);
            var loggableBody2 = message2.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);
            chatLog.WriteLog(message2, loggableSender, loggableBody2);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(lines, Is.EqualTo(expected).AsCollection);
        }

        [Test]
        public void WriteLog_WrapLines()
        {
            var expected = new List<string>
            {
                "============================== Tuesday, May 9, 2023 ==============================",
                "5-9-2023 17:00:01 Robert Jones [say]: This is a long message so that we can",
                "                                      test if the line wrapping code work,",
                "                                      and works the way we expect it to.",
                "5-9-2023 17:00:02 Robert Jones [say]: This is a test",
            };
            var chatLog = new TestChatLog("test");
            chatLog.LogConfiguration.MessageWrapWidth = 40;
            chatLog.LogConfiguration.MessageWrapIndentation = 38;
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow,
                                                         body:
                                                         "This is a long message so that we can test if the line wrapping code work, and works the way we expect it to.");
            var message2 = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);
            var loggableBody2 = message2.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);

            chatLog.WriteLog(message, loggableSender, loggableBody);
            chatLog.WriteLog(message2, loggableSender, loggableBody2);

            Assert.That(chatLog.IsOpen, Is.True);
            var lines = chatLog.FileSystem.Writers[chatLog.FileName].Lines;
            Assert.That(lines, Is.EqualTo(expected).AsCollection);
        }
    }

    public class DumpLogTests
    {
        [Test]
        public void DumpLogs_Closed()
        {
            var expected = new List<string> { "[L]: test          False  ''", };
            var logger = new LoggerFake();
            var chatLog = new TestChatLog("test");

            chatLog.DumpLog(logger);

            Assert.That(logger.Lines, Is.EqualTo(expected).AsCollection);
        }

        [Test]
        public void DumpLogs_Open()
        {
            var expected = new List<string>
            {
                "[L]: test          True   'C:\\Users\\Bob\\Documents\\FFXIV Chatter\\chatter-test-20230509-170000.log'",
            };
            var logger = new LoggerFake();
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);
            chatLog.WriteLog(message, loggableSender, loggableBody);

            chatLog.DumpLog(logger);

            Assert.That(logger.Lines, Is.EqualTo(expected).AsCollection);
        }
    }

    public class CloseTests
    {
        [Test]
        public void Close_Closed()
        {
            var chatLog = new TestChatLog("test");

            Assert.That(chatLog.FileName, Is.EqualTo(""));

            chatLog.Close();

            Assert.That(chatLog.FileName, Is.EqualTo(""));
        }

        [Test]
        public void Close_Open()
        {
            var chatLog = new TestChatLog("test");
            var message = ChatMessageUtils.CreateMessage(chatLog.DateHelper.ZonedNow);
            var loggableSender =
                message.GetLoggableSender(chatLog.LogConfiguration.IncludeServer, chatLog.LogConfiguration.Users);
            var loggableBody = message.GetLoggableBody(chatLog.LogConfiguration.IncludeServer);
            chatLog.WriteLog(message, loggableSender, loggableBody);

            Assert.That(chatLog.FileName, Is.Not.Empty);

            chatLog.Close();

            Assert.That(chatLog.FileName, Is.Empty);
        }
    }
}

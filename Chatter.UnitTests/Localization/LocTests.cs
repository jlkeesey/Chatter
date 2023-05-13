using Chatter.Localization;
using Chatter.UnitTests.Support;
using NUnit.Framework;
using static System.String;

namespace Chatter.UnitTests.Localization;

[TestFixture]
public class LocTests
{
    private const string SingleMessageJson = @"
            {
                ""messages"": [
                    { ""key"": ""ChatType.Say"",""message"": ""Say""
                    }
                ]
            }";

    private const string MessageFound = @"
            {
                ""messages"": [
                    { ""key"": ""the-key"",""message"": ""the-message""
                    }
                ]
            }";

    private const string MessageFoundReplacement = @"
            {
                ""messages"": [
                    { ""key"": ""the-key"",""message"": ""the-message here >{0}< and >{1}<""
                    }
                ]
            }";

    public class LoadTests
    {
        private const string BaseMessageJson = @"
            {
                ""messages"": [
                    { ""key"": ""base-unique"",""message"": ""base"" },
                    { ""key"": ""lang=override"",""message"": ""base"" },
                    { ""key"": ""full=override"",""message"": ""base"" },
                    { ""key"": ""both=override"",""message"": ""base"" }
                ]
            }";

        private const string LangMessageJson = @"
            {
                ""messages"": [
                    { ""key"": ""lang-unique"",""message"": ""lang"" },
                    { ""key"": ""lang=override"",""message"": ""lang"" },
                    { ""key"": ""both=override"",""message"": ""lang"" }
                ]
            }";

        private const string FullMessageJson = @"
            {
                ""messages"": [
                    { ""key"": ""full-unique"",""message"": ""full"" },
                    { ""key"": ""full=override"",""message"": ""full"" },
                    { ""key"": ""both=override"",""message"": ""full"" }
                ]
            }";

        [Test]
        public void Load_NothingToLoad()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgNullReader();
            loc.Load(logger, msgReader);

            Assert.That(loc.Count, Is.EqualTo(0));
        }

        [Test]
        public void Load_OnlyBase()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgSingleReader("messages");
            loc.Load(logger, msgReader);

            Assert.That(loc.Count, Is.EqualTo(1));
        }

        [Test]
        public void Load_OnlyLanguage()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgSingleReader("messages-en");
            loc.Load(logger, msgReader);

            Assert.That(loc.Count, Is.EqualTo(1));
        }

        [Test]
        public void Load_OnlyFullCulture()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgSingleReader("messages-en-US");
            loc.Load(logger, msgReader);

            Assert.That(loc.Count, Is.EqualTo(1));
        }

        [Test]
        public void Load_Merging()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgFullReader();
            loc.Load(logger, msgReader);

            Assert.That(loc.Count, Is.EqualTo(6));
            Assert.That(loc.Message("base-unique"), Is.EqualTo("base"));
            Assert.That(loc.Message("lang=override"), Is.EqualTo("lang"));
            Assert.That(loc.Message("full=override"), Is.EqualTo("full"));
            Assert.That(loc.Message("both=override"), Is.EqualTo("full"));
            Assert.That(loc.Message("lang-unique"), Is.EqualTo("lang"));
            Assert.That(loc.Message("full-unique"), Is.EqualTo("full"));
        }

        private class MsgFullReader : ILocMessageReader
        {
            public string Read(string name)
            {
                return name switch
                {
                    "messages" => BaseMessageJson,
                    "messages-en" => LangMessageJson,
                    "messages-en-US" => FullMessageJson,
                    _ => Empty,
                };
            }
        }
    }

    private class MsgSingleReader : ILocMessageReader
    {
        private readonly string _messages;
        private readonly string _name;

        public MsgSingleReader(string name, string? messages = null)
        {
            _name = name;
            _messages = messages ?? SingleMessageJson;
        }

        public string Read(string name)
        {
            return name == _name ? _messages : Empty;
        }
    }

    private class MsgNullReader : ILocMessageReader
    {
        public string Read(string name)
        {
            return Empty;
        }
    }

    public class MessageTests
    {
        [Test]
        public void Message_Missing()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgNullReader();
            loc.Load(logger, msgReader);

            var result = loc.Message("unknown");

            Assert.That(result, Is.EqualTo("??[[unknown]]??"));
        }

        [Test]
        public void Message_Found()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgSingleReader("messages", MessageFound);
            loc.Load(logger, msgReader);

            var result = loc.Message("the-key");

            Assert.That(result, Is.EqualTo("the-message"));
        }

        [Test]
        public void Message_FoundReplacement()
        {
            var loc = new Loc("en", "US"); // Don't assume the language/country is en-US
            var logger = new LoggerFake();
            var msgReader = new MsgSingleReader("messages", MessageFoundReplacement);
            loc.Load(logger, msgReader);

            var result = loc.Message("the-key", "one", "two");

            Assert.That(result, Is.EqualTo("the-message here >one< and >two<"));
        }
    }
}
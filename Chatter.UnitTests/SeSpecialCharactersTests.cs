using NUnit.Framework;

namespace Chatter.UnitTests;

[TestFixture]
public class SeSpecialCharactersTests
{
    public class IsSpecialTests
    {
        [Test]
        public void IsSpecialNormalCharacter()
        {
            Assert.False(SeSpecialCharacters.IsSpecial('\uDFFF'));
        }

        [Test]
        public void IsSpecialSpecialCharacterBegin()
        {
            Assert.True(SeSpecialCharacters.IsSpecial('\uE000'));
        }

        [Test]
        public void IsSpecialSpecialCharacterMiddle()
        {
            Assert.True(SeSpecialCharacters.IsSpecial('\uE031'));
        }

        [Test]
        public void IsSpecialSpecialCharacterEnd()
        {
            Assert.True(SeSpecialCharacters.IsSpecial('\uF8FF'));
        }

        [Test]
        public void IsSpecialSpecialCharacterAbove()
        {
            Assert.False(SeSpecialCharacters.IsSpecial('\uF900'));
        }
    }

    public class ReplaceTests
    {
        [Test]
        public void ReplaceNone()
        {
            const string expected = "This is a test";

            var actual = SeSpecialCharacters.Replace(expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReplaceSpecials()
        {
            const string input = "This \uE03Cis a test\uE0BB";
            const string expected = "This \u2747is a test\u02A2";

            var actual = SeSpecialCharacters.Replace(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReplaceEmpty()
        {
            const string expected = "";

            var actual = SeSpecialCharacters.Replace(expected);

            Assert.AreEqual(expected, actual);
        }
    }

    public class CleanTests
    {
        [Test]
        public void CleanNone()
        {
            const string expected = "This is a test";

            var actual = SeSpecialCharacters.Clean(expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CleanSpecials()
        {
            const string input = "This \uE081is a test\uE021";
            const string expected = "This is a test";

            var actual = SeSpecialCharacters.Clean(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CleanEmpty()
        {
            const string expected = "";

            var actual = SeSpecialCharacters.Clean(expected);

            Assert.AreEqual(expected, actual);
        }
    }
}
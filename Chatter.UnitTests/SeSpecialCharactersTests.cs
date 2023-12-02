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
            Assert.That(SeSpecialCharacters.IsSpecial('\uDFFF'), Is.False);
        }

        [Test]
        public void IsSpecialSpecialCharacterBegin()
        {
            Assert.That(SeSpecialCharacters.IsSpecial('\uE000'), Is.True);
        }

        [Test]
        public void IsSpecialSpecialCharacterMiddle()
        {
            Assert.That(SeSpecialCharacters.IsSpecial('\uE031'), Is.True);
        }

        [Test]
        public void IsSpecialSpecialCharacterEnd()
        {
            Assert.That(SeSpecialCharacters.IsSpecial('\uF8FF'), Is.True);
        }

        [Test]
        public void IsSpecialSpecialCharacterAbove()
        {
            Assert.That(SeSpecialCharacters.IsSpecial('\uF900'), Is.False);
        }
    }

    public class ReplaceTests
    {
        [Test]
        public void ReplaceNone()
        {
            var actual = SeSpecialCharacters.Replace("This is a test");

            Assert.That(actual, Is.EqualTo("This is a test"));
        }

        [Test]
        public void ReplaceSpecials()
        {
            const string input = "This \uE03Cis a test\uE0BB";

            var actual = SeSpecialCharacters.Replace(input);

            Assert.That(actual, Is.EqualTo("This \u2747is a test\u02A2"));
        }

        [Test]
        public void ReplaceEmpty()
        {
            var actual = SeSpecialCharacters.Replace("");

            Assert.That(actual, Is.Empty);
        }
    }

    public class CleanTests
    {
        [Test]
        public void CleanNone()
        {
            var actual = SeSpecialCharacters.Clean("This is a test");

            Assert.That(actual, Is.EqualTo("This is a test"));
        }

        [Test]
        public void CleanSpecials()
        {
            const string input = "This \uE081is a test\uE021";

            var actual = SeSpecialCharacters.Clean(input);

            Assert.That(actual, Is.EqualTo("This is a test"));
        }

        [Test]
        public void CleanEmpty()
        {
            var actual = SeSpecialCharacters.Clean("");

            Assert.That(actual, Is.Empty);
        }
    }
}

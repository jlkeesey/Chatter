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

using Chatter.Utilities;
using NUnit.Framework;

namespace Chatter.UnitTests.Utilities;

[TestFixture]
public class StringHelperTests
{
    public class WrapBodyTests
    {
        [Test]
        public void WrapBody_Empty()
        {
            var result = StringHelper.WrapBody(string.Empty, 50);

            Assert.That(1 == result.Count);
            Assert.That(result[0].Equals(""));
        }

        [Test]
        public void WrapBody_EqualToWidth()
        {
            var expected = "12345 67890";
            var result = StringHelper.WrapBody(expected, 11);

            Assert.That(1 == result.Count);
            Assert.That(result[0].Equals(expected));
        }

        [Test]
        public void WrapBody_OneLargerThanWidth()
        {
            var expected = "12345 67890";
            var result = StringHelper.WrapBody(expected, 10);

            Assert.That(2 == result.Count);
            Assert.That(result[0].Equals("12345"));
            Assert.That(result[1].Equals("67890"));
        }

        [Test]
        public void WrapBody_LoseExtraSpace()
        {
            var expected = "12345    67890";
            var result = StringHelper.WrapBody(expected, 10);

            Assert.That(2 == result.Count);
            Assert.That(result[0].Equals("12345"));
            Assert.That(result[1].Equals("67890"));
        }

        [Test]
        public void WrapBody_ForceBreak()
        {
            var expected = "1234567890";
            var result = StringHelper.WrapBody(expected, 5);

            Assert.That(2 == result.Count);
            Assert.That(result[0].Equals("12345"));
            Assert.That(result[1].Equals("67890"));
        }

        [Test]
        public void WrapBody_NoWidth()
        {
            var expected = "123 4567 890";
            var result = StringHelper.WrapBody(expected, 0);

            Assert.That(1 == result.Count);
            Assert.That(result[0].Equals(expected));
        }
    }
}

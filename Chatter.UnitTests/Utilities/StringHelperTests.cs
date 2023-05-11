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

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0]);
        }

        [Test]
        public void WrapBody_EqualToWidth()
        {
            var expected = "12345 67890";
            var result = StringHelper.WrapBody(expected, 11);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, result[0]);
        }

        [Test]
        public void WrapBody_OneLargerThanWidth()
        {
            var expected = "12345 67890";
            var result = StringHelper.WrapBody(expected, 10);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("12345", result[0]);
            Assert.AreEqual("67890", result[1]);
        }

        [Test]
        public void WrapBody_LoseExtraSpace()
        {
            var expected = "12345    67890";
            var result = StringHelper.WrapBody(expected, 10);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("12345", result[0]);
            Assert.AreEqual("67890", result[1]);
        }

        [Test]
        public void WrapBody_ForceBreak()
        {
            var expected = "1234567890";
            var result = StringHelper.WrapBody(expected, 5);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("12345", result[0]);
            Assert.AreEqual("67890", result[1]);
        }

        [Test]
        public void WrapBody_NoWidth()
        {
            var expected = "123 4567 890";
            var result = StringHelper.WrapBody(expected, 0);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, result[0]);
        }
    }
}
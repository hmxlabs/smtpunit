using NSubstitute;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class DummySmtpServerConnectionCollectionTests
    {
        [Test]
        public void TestAddAndCount()
        {
            var collection = new DummySmtpServerConnectionCollection();
            collection.Add(Substitute.For<IDummySmtpServerConnection>());
            Assert.That(collection.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestRemoveAndCount()
        {
            var collection = new DummySmtpServerConnectionCollection();
            var connection = Substitute.For<IDummySmtpServerConnection>();
            collection.Add(connection);
            collection.Remove(connection);
            Assert.That(collection.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestWaitForEmptyOnNewCollection()
        {
            var collection = new DummySmtpServerConnectionCollection();
            Assert.True(collection.WaitTillEmpty());
        }

        [Test]
        public void TestWaitForEmptyBlocksOnNonEmptyCollection()
        {
            var collection = new DummySmtpServerConnectionCollection();
            var connection = Substitute.For<IDummySmtpServerConnection>();
            collection.Add(connection);
            Assert.False(collection.WaitTillEmpty(250));
        }

        [Test]
        public void TestWaitForEmptyUnblocksWhenCollectionEmptied()
        {
            var collection = new DummySmtpServerConnectionCollection();
            var connection = Substitute.For<IDummySmtpServerConnection>();
            collection.Add(connection);
            collection.Remove(connection);
            Assert.True(collection.WaitTillEmpty());
        }
    }
}

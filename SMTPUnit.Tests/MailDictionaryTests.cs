using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class MailDictionaryTests
    {
        [Test]
        public void TestAddThenGet()
        {
            const string key = "somekey";
            var message = Substitute.For<IDummyMailMessage>();
            var dictionary = new MailDictionary();
            dictionary.Add(key, message);
            var retrievedMessage = dictionary[key].First();
            Assert.That(retrievedMessage, Is.SameAs(message));
        }

        [Test]
        public void TestAddThenCount()
        {
            const string key = "somekey";
            var message = Substitute.For<IDummyMailMessage>();
            var dictionary = new MailDictionary();
            dictionary.Add(key, message);
            Assert.That(dictionary.KeyCount, Is.EqualTo(1));
        }

        [Test]
        public void TestGetAlwaysReturnEmptyCollection()
        {
            var dictionary = new MailDictionary();
            var messages = dictionary["some random string"];
            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestAddThenClear()
        {
            const string key = "somekey";
            var message = Substitute.For<IDummyMailMessage>();
            var dictionary = new MailDictionary();
            dictionary.Add(key, message);
            dictionary.Clear();
            Assert.That(dictionary.KeyCount, Is.EqualTo(0));
        }
    }
}

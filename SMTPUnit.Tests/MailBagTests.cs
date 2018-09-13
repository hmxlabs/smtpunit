using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class MailBagTests
    {
        [Test]
        public void TestAddThenGetAll()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessage = mailBag.All.First();
            Assert.That(retrievedMessage, Is.SameAs(message));
        }

        [Test]
        public void TestAddWithNullSubject()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            message.Subject.ReturnsForAnyArgs((string)null);
            mailBag.Add(message);
            var retrievedMessage = mailBag.All.First();
            Assert.That(retrievedMessage, Is.SameAs(message));
        }

        [Test]
        public void TestAddThenCount()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            Assert.That(mailBag.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestAddThenEmpty()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            mailBag.Empty();
            Assert.That(mailBag.Count, Is.EqualTo(0));
            Assert.That(mailBag.All, Is.Empty);
        }

        [Test]
        public void TestAddThenGetBySubject()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessage = mailBag.GetMailsWithSubject(MailMessageData.Subject).First();
            Assert.That(mailBag.ContainsMailWithSubject(MailMessageData.Subject));
            Assert.That(retrievedMessage, Is.SameAs(message));
        }

        [Test]
        public void TestAddThenGetBySender()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessage = mailBag.GetMailsWithSender(MailMessageData.Sender).First();
            Assert.That(mailBag.ContainsMailWithSender(MailMessageData.Sender));
            Assert.That(retrievedMessage, Is.SameAs(message));
        }

        [Test]
        public void TestAddThenGetByRecipient()
        {
            var mailBag = new MailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessage = mailBag.GetMailsWithRecipient(MailMessageData.FirstRecipient).First();
            Assert.That(retrievedMessage, Is.SameAs(message));
            Assert.That(mailBag.ContainsMailWithRecipient(MailMessageData.FirstRecipient));
            retrievedMessage = mailBag.GetMailsWithRecipient(MailMessageData.SecondRecipient).First();
            Assert.That(retrievedMessage, Is.SameAs(message));
            Assert.That(mailBag.ContainsMailWithRecipient(MailMessageData.SecondRecipient));
        }

        [Test]
        public void TestGetByRecipients()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessage = mailBag.GetMailsWithRecipients(MailMessageData.Recipients).First();
            Assert.That(retrievedMessage, Is.SameAs(message));
            Assert.That(mailBag.ContainsMailWithRecipient(MailMessageData.FirstRecipient));
        }

        [Test]
        public void TestGetMailsBySubjectWithNullReturnsAllMails()
        {
            var mailBag = CreatePopulatedMailBag();
            var matchingMails = mailBag.GetMailsWithSubject(null);
            Assert.That(matchingMails.Count(), Is.EqualTo(mailBag.Count));
        }

        [Test]
        public void TestGetMailsBySenderWithNullReturnsAllMails()
        {
            var mailBag = CreatePopulatedMailBag();
            var matchingMails = mailBag.GetMailsWithSender(null);
            Assert.That(matchingMails.Count(), Is.EqualTo(mailBag.Count));
        }

        [Test]
        public void TestGetMailsByRecipientWithNullReturnsAllMails()
        {
            var mailBag = CreatePopulatedMailBag();
            var matchingMails = mailBag.GetMailsWithRecipient(null);
            Assert.That(matchingMails.Count(), Is.EqualTo(mailBag.Count));
        }

        [Test]
        public void TestGetMatchingMailWithSubjectOnly()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessages = mailBag.GetMailsWith(null, null, MailMessageData.Subject, null).ToList();
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First(), Is.SameAs(message));
        }

        [Test]
        public void TestGetMatchingMailWithSenderOnly()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessages = mailBag.GetMailsWith(MailMessageData.Sender, null, null, null).ToList();
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First(), Is.SameAs(message));
        }

        [Test]
        public void TestGetMatchingMailWithRecipientOnly()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessages = mailBag.GetMailsWith(null, MailMessageData.Recipients, null, null).ToList();
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First(), Is.SameAs(message));
        }

        [Test]
        public void TestGetMatchingMailsWithSenderSubjectAndRecipient()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessages = mailBag.GetMailsWith(MailMessageData.Sender, MailMessageData.Recipients, MailMessageData.Subject, null).ToList();
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First(), Is.SameAs(message));
        }

        [Test]
        public void TestGetMatchingMailsWithSenderSubjectRecipientAndBodyContent()
        {
            var mailBag = CreatePopulatedMailBag();
            var message = CreateTestMailMessage();
            mailBag.Add(message);
            var retrievedMessages = mailBag.GetMailsWith(MailMessageData.Sender, MailMessageData.Recipients, MailMessageData.Subject, "--- oh look").ToList();
            Assert.That(retrievedMessages.Count(), Is.EqualTo(1));
            Assert.That(retrievedMessages.First(), Is.SameAs(message));

            retrievedMessages = mailBag.GetMailsWith(MailMessageData.Sender, MailMessageData.Recipients, MailMessageData.Subject, "chocolate cake").ToList();
            Assert.That(retrievedMessages, Is.Empty);
        }

        private IDummyMailMessage CreateTestMailMessage()
        {
            var message = Substitute.For<IDummyMailMessage>();
            message.Body.ReturnsForAnyArgs(MailMessageData.Body);
            message.Recipients.ReturnsForAnyArgs(MailMessageData.Recipients);
            message.Sender.ReturnsForAnyArgs(MailMessageData.Sender);
            message.Subject.ReturnsForAnyArgs(MailMessageData.Subject);
            return message;
        }

        private IDummyMailMessage CreateTestMailMessage(int index_)
        {
            var message = Substitute.For<IDummyMailMessage>();
            message.Body.ReturnsForAnyArgs(MailMessageData.Body);
            message.Recipients.ReturnsForAnyArgs(new []{MailMessageData.AlternativeRecipients[index_]});
            message.Sender.ReturnsForAnyArgs(MailMessageData.AlternativeSenders[index_]);
            message.Subject.ReturnsForAnyArgs(MailMessageData.AlternativeSubjects[index_]);
            return message;
        }

        private MailBag CreatePopulatedMailBag()
        {
            var mailBag = new MailBag();
            for (int index = 0; index < MailMessageData.AlternativeRecipients.Length; index++)
            {
                mailBag.Add(CreateTestMailMessage(index));
            }

            return mailBag;
        }

        private static class MailMessageData
        {
            public const string Sender = "sender@address.com";
            public const string Subject = "Mail Subject";
            public const string Body = "Mail Body --- oh look nothing to see here";
            public const string FirstRecipient = "first@recipient.com";
            public const string SecondRecipient = "second@recipient.com";
            public static readonly string[] Recipients = {FirstRecipient, SecondRecipient};
            public static readonly string[] AlternativeSubjects = {"First subject", "Second subject", "Third Subject"};
            public static readonly string[] AlternativeSenders = {"sender.one@address.com", "sender.two@address.com", "sender.three@address.com"};
            public static readonly string[] AlternativeRecipients = {"recip.one@address.com", "recip.two@address.com", "recip.three@address.com"};
        }
    }
}

using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class DummySmtpServerTests
    {
        public static class TestMessage
        {
            public const string From = "JQP@bar.com";
            public const string To = "Jones@xyz.com";
            public const string AdditionalTo = "Green@foo.com";
            public const string Subject = "The Next Meeting of the Board";
            public const string Body = "\r\nBill:\r\nThe next meeting of the board of directors will be\r\non Tuesday.\r\n                        John.";
        }

        [Test]
        public void TestSendingEmailWith8BitTransferEncoding()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);
            message.BodyEncoding = Encoding.ASCII;
            message.BodyTransferEncoding = TransferEncoding.EightBit;
            TestSendingEmail(message);
        }

        [Test]
        public void TestSendingEmailWith7BitTransferEncoding()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);
            message.BodyEncoding = Encoding.ASCII;
            message.BodyTransferEncoding = TransferEncoding.SevenBit;
            TestSendingEmail(message);
        }

        [Test]
        public void TestSendingEmailWithUnknownTransferEncoding()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);
            message.BodyEncoding = Encoding.ASCII;
            message.BodyTransferEncoding = TransferEncoding.Unknown;
            TestSendingEmail(message);
        }

        [Ignore("Base64 transfer encoding is currently not supported")]
        [Test]
        public void TestSendingEmailWithBase64TransferEncoding()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);
            message.BodyEncoding = Encoding.ASCII;
            message.BodyTransferEncoding = TransferEncoding.Base64;
            TestSendingEmail(message);
        }

        [Test]
        public void TestSendingEmailWithQuotedPrintableTransferEncoding()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);
            message.BodyEncoding = Encoding.ASCII;
            message.BodyTransferEncoding = TransferEncoding.QuotedPrintable;
            TestSendingEmail(message);
        }

        [Test]
        public void TestSendingMessageWithSmtpPassworthAuthentication()
        {
            var message = new MailMessage(TestMessage.From, TestMessage.To);
            message.Body = TestMessage.Body;
            message.Subject = TestMessage.Subject;
            message.To.Add(TestMessage.AdditionalTo);

            int port = PortHelper.GetPort();
            var smtpServer = new DummySmtpServer(port);
            smtpServer.Start();
            using (var client = new SmtpClient("localhost", port))
            {
                client.Credentials = new NetworkCredential("test@user.com", "this-is-the-password");
                client.Send(message);
            }
            smtpServer.Stop();
            AssertMailBag(smtpServer.ReceivedMails, message);
        }

        private void TestSendingEmail(MailMessage message_)
        {
            int port = PortHelper.GetPort();
            var smtpServer = new DummySmtpServer(port);
            smtpServer.Start();
            using (var client = new SmtpClient("localhost", port))
            {
                client.Send(message_);
            }
            smtpServer.Stop();

            AssertMailBag(smtpServer.ReceivedMails, message_);
        }

        private void AssertMailBag(IMailBag mailBag_, MailMessage sentMessage_)
        {
            var emails = mailBag_.All.ToList();
            Assert.That(emails.Count, Is.EqualTo(1));

            var receivedMessage = emails.First();
            AssertMessage(receivedMessage, sentMessage_);
        }

        private void AssertMessage(IDummyMailMessage receivedMessage_, MailMessage sentMessage_)
        {
            
            Assert.That(receivedMessage_.Sender, Is.EqualTo(sentMessage_.From));
            Assert.That(receivedMessage_.Subject, Is.EqualTo(sentMessage_.Subject));
            Assert.That(receivedMessage_.Body, Is.Not.Null, "No message body found");
            Assert.That(receivedMessage_.Body.Contains(sentMessage_.Body), "The message body does not match"); // Need to test as contains as the .NET classes seem to add a trailing CRLF
            Assert.That(receivedMessage_.Recipients.Count(), Is.EqualTo(2));
            Assert.That(receivedMessage_.WasSentTo(sentMessage_.To[0].Address));
            Assert.That(receivedMessage_.WasSentTo(sentMessage_.To[1].Address));
        }
    }
}

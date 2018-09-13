using System.Text;
using HmxLabs.TestExt.Mocks.Net;
using NSubstitute;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class DummySmtpServerConnectionTests
    {
        [Test]
        public void TestStartingConnectionWritesWelcome()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            var readData = ReadResponseLine(netStream);
            Assert.That(readData, Is.EqualTo(SmtpServerResponses.Welcome));
        }

        [Test]
        public void TestSendingHelloWritesGreeting()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream);

            WriteRequest(netStream, SmtpServerRequests.Hello);
            var helloResponse = ReadResponseLine(netStream);
            Assert.That(helloResponse, Is.EqualTo(SmtpServerResponses.Greeting("unit-test")));
        }

        [Test]
        public void TestSendingMail()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream); // Welcome

            WriteRequest(netStream, SmtpServerRequests.Hello);
            ReadResponseLine(netStream); // Greeting to hello

            WriteRequest(netStream, SmtpServerRequests.Mail);
            var mailResponse = ReadResponseLine(netStream);
            Assert.That(mailResponse, Is.EqualTo(SmtpServerResponses.Ok));

            WriteRequest(netStream, SmtpServerRequests.Recipient);
            var recipientResponse = ReadResponseLine(netStream);
            Assert.That(recipientResponse, Is.EqualTo(SmtpServerResponses.Ok));

            WriteRequest(netStream, SmtpServerRequests.AdditionalRecipient);
            recipientResponse = ReadResponseLine(netStream);
            Assert.That(recipientResponse, Is.EqualTo(SmtpServerResponses.Ok));

            WriteRequest(netStream, SmtpServerRequests.Data);
            var dataResponse = ReadResponseLine(netStream);
            Assert.That(dataResponse, Is.EqualTo(SmtpServerResponses.StartMailInput));

            WriteRequest(netStream, SmtpServerRequests.MailBody);
            var mailBodyResponse = ReadResponseLine(netStream);
            Assert.That(mailBodyResponse, Is.EqualTo(SmtpServerResponses.Ok));
        }

        [Test]
        public void TestSendingMailBeforeHello()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream);

            WriteRequest(netStream, SmtpServerRequests.Mail);
            var mailResponse = ReadResponseLine(netStream);
            Assert.That(mailResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        [Test]
        public void TestSendingRecipientBeforeHello()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream);

            WriteRequest(netStream, SmtpServerRequests.Recipient);
            var mailResponse = ReadResponseLine(netStream);
            Assert.That(mailResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        [Test]
        public void TestSendingRecipientBeforeMail()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream); // Welcome

            WriteRequest(netStream, SmtpServerRequests.Hello);
            ReadResponseLine(netStream); // Greeting to hello

            WriteRequest(netStream, SmtpServerRequests.Recipient);
            var recipientResponse = ReadResponseLine(netStream);
            Assert.That(recipientResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        [Test]
        public void TestSendingDataBeforeHello()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream); // Welcome

            WriteRequest(netStream, SmtpServerRequests.Data);
            var dataResponse = ReadResponseLine(netStream);
            Assert.That(dataResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        [Test]
        public void TestSendingDataBeforeMail()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream); // Welcome

            WriteRequest(netStream, SmtpServerRequests.Hello);
            ReadResponseLine(netStream); // Greeting to hello

            WriteRequest(netStream, SmtpServerRequests.Data);
            var dataResponse = ReadResponseLine(netStream);
            Assert.That(dataResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        [Test]
        public void TestSendingDataBeforeRecipients()
        {
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, new MailBag(), Substitute.For<IDummySmtpServerConnectionCollection>());
            smtpConnection.Start();
            ReadResponseLine(netStream); // Welcome

            WriteRequest(netStream, SmtpServerRequests.Hello);
            ReadResponseLine(netStream); // Greeting to hello

            WriteRequest(netStream, SmtpServerRequests.Mail);
            var mailResponse = ReadResponseLine(netStream);
            Assert.That(mailResponse, Is.EqualTo(SmtpServerResponses.Ok));

            WriteRequest(netStream, SmtpServerRequests.Data);
            var dataResponse = ReadResponseLine(netStream);
            Assert.That(dataResponse, Is.EqualTo(SmtpServerResponses.BadCommandSequence));
        }

        private static void WriteRequest(MockNetworkStream stream_, string request_)
        {
            var bytes = Encoding.ASCII.GetBytes(request_);
            stream_.WriteRequest(bytes, 0, bytes.Length);
        }

        private static string ReadResponseLine(MockNetworkStream stream_)
        {
            var readBuffer = new byte[512];
            var dataBuffer = new StringBuilder();
            var receivingResponse = true;

            while (receivingResponse)
            {
                var readCount = stream_.ReadResponse(readBuffer, 0, readBuffer.Length);
                var commandDataStr = Encoding.ASCII.GetString(readBuffer, 0, readCount);
                dataBuffer.Append(commandDataStr);

                if (commandDataStr.EndsWith("\r\n"))
                    receivingResponse = false;
            }

            return dataBuffer.ToString();
        }
    }
}

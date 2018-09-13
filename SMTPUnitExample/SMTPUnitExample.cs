using System.Net.Mail;
using HmxLabs.SmtpUnit;
using NUnit.Framework;

namespace SMTPUnitExample
{
    [TestFixture]
    public class SmtpUnitExample
    {
        [Test]
        public void ExampleSmtpUnitUsage()
        {
            // Setup
            using (var mockServer = new DummySmtpServer(5000)) // Randomly picked port 5000 here, use whatever value you would normally.
            {
                mockServer.Start();

                // Act
                SendEmail();

                // Assert
                Assert.That(mockServer, Received.Mail(1));
                Assert.That(mockServer, Received.Mail(1).From("fred@flintstone.com"));
                Assert.That(mockServer, Received.Mail(1).To("wilma@flintstone.com"));
                Assert.That(mockServer, Received.Mail(1).Subject("example mail message"));
                Assert.That(mockServer, Received.Mail(1).BodyContains("lorem ipsum"));

                // Clean up
                mockServer.Stop();
            }
        }

        private void SendEmail()
        {
            // You would normally execute whatever code you expect to send emails here
            // but for the sake of example this just uses the SmtpClient to send an email
            using (var smtpClient = new SmtpClient("localhost", 5000))
            { 
                smtpClient.Send("fred@flintstone.com", "wilma@flintstone.com", "example mail message", "mail message content - lorem ipsum dolor");
            }
        }
    }
}

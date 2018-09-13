using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class DummyMailMessageTests
    {
        public static class TestData
        {
            public const string From = "JQP@bar.com";
            public static readonly string[] To = {"Jones@xyz.com", "Green@foo.com"};
            public const string Subject = "The Next Meeting of the Board";
            public const string Body = "\r\nBill:\r\nThe next meeting of the board of directors will be\r\non Tuesday.\r\n                        John.";
        }

        [Test]
        public void TestConstructionWith8BitTransferEncoding()
        {
            const string mailData = "MIME-Version: 1.0\r\nFrom: JQP@bar.com\r\nTo: Jones@xyz.com, Green@foo.com\r\nDate: 26 Apr 2016 13:11:36 +0100\r\nSubject: The Next Meeting of the Board\r\nContent-Type: text/plain; charset=us-ascii\r\nContent-Transfer-Encoding: 8bit\r\n\r\n\r\nBill:\r\nThe next meeting of the board of directors will be\r\non Tuesday.\r\n                        John.";
            var message = new DummyDummyMailMessage(TestData.From, TestData.To, mailData);
            AssertMessage(message);
        }

        [Test]
        public void TestConstructionWith7BitTransferEncoding()
        {
            const string mailData = "MIME-Version: 1.0\r\nFrom: JQP@bar.com\r\nTo: Jones@xyz.com, Green@foo.com\r\nDate: 26 Apr 2016 13:11:36 +0100\r\nSubject: The Next Meeting of the Board\r\nContent-Type: text/plain; charset=us-ascii\r\nContent-Transfer-Encoding: 7bit\r\n\r\n\r\nBill:\r\nThe next meeting of the board of directors will be\r\non Tuesday.\r\n                        John.";
            var message = new DummyDummyMailMessage(TestData.From, TestData.To, mailData);
            AssertMessage(message);
        }

        [Test]
        public void TestConstructionWithQuotedPrintableTransferEncoding()
        {
            const string mailData = "MIME-Version: 1.0\r\nFrom: JQP@bar.com\r\nTo: Jones@xyz.com, Green@foo.com\r\nDate: 5 May 2016 08:21:13 +0100\r\nSubject: The Next Meeting of the Board\r\nContent-Type: text/plain; charset=us-ascii\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\n=0D=0ABill:=0D=0AThe next meeting of the board of directors will =\r\nbe=0D=0Aon Tuesday.=0D=0A                        John.";
            var message = new DummyDummyMailMessage(TestData.From, TestData.To, mailData);
            AssertMessage(message);
        }

        [Test]
        public void TestConstructionWithSingleLineBody()
        {
            const string mailData = "MIME-Version: 1.0\r\nFrom: JQP@bar.com\r\nTo: Jones@xyz.com, Green@foo.com\r\nDate: 5 May 2016 08:21:13 +0100\r\nSubject: The Next Meeting of the Board\r\nContent-Type: text/plain; charset=us-ascii\r\nContent-Transfer-Encoding: quoted-printable\r\n\r\nHello Bill";
            var message = new DummyDummyMailMessage(TestData.From, TestData.To, mailData);
            Assert.That(message.Body, Is.EqualTo("Hello Bill"));
        }

        [Test]
        public void TestConstructionWithNoBody()
        {
            const string mailData = "MIME-Version: 1.0\r\nFrom: JQP@bar.com\r\nTo: Jones@xyz.com, Green@foo.com\r\nDate: 5 May 2016 08:21:13 +0100\r\nSubject: The Next Meeting of the Board\r\nContent-Type: text/plain; charset=us-ascii\r\nContent-Transfer-Encoding: quoted-printable\r\n";
            var message = new DummyDummyMailMessage(TestData.From, TestData.To, mailData);
            Assert.That(message.Body, Is.Null);
        }

        private void AssertMessage(DummyDummyMailMessage message_)
        {
            Assert.That(message_.Sender, Is.EqualTo(TestData.From));
            Assert.That(message_.WasSentTo(TestData.To[0]));
            Assert.That(message_.WasSentTo(TestData.To[1]));
            Assert.That(message_.Subject, Is.EqualTo(TestData.Subject));
            Assert.That(message_.Body, Is.EqualTo(TestData.Body));
        }
    }
}

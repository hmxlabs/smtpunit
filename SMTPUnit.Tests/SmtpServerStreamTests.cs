using System.IO;
using System.Text;
using HmxLabs.TestExt.Mocks.Net;
using NUnit.Framework;

namespace HmxLabs.SmtpUnit.Test
{
    [TestFixture]
    public class SmtpServerStreamTests
    {
        [Test]
        public void TestWriteWelcome()
        {
            TestWriteToStream(SmtpServerResponses.Welcome, stream_ => stream_.WriteWelcome()); // test for <CRLF> terminator
        }

        [Test]
        public void TestWriteGreeting()
        {
            const string clientId = "A-CLIENT-ID";
            TestWriteToStream(SmtpServerResponses.Greeting(clientId), stream_ => stream_.WriteHelloResponse(clientId));
        }

        [Test]
        public void TestWriteOk()
        {
            TestWriteToStream(SmtpServerResponses.Ok, stream_ => stream_.WriteOk());
        }

        [Test]
        public void TestWriteStartMailInput()
        {
            TestWriteToStream(SmtpServerResponses.StartMailInput, stream_ => stream_.WriteIntermediateOk());
        }

        [Test]
        public void TestWriteBadCommandSequence()
        {
            TestWriteToStream(SmtpServerResponses.BadCommandSequence, stream_ => stream_.WriteBadCommandSequenceError());
        }

        [Test]
        public void TestWriteGoodbye()
        {
            TestWriteToStream(SmtpServerResponses.Goodbye, stream_ => stream_.WriteGoodbye());
        }

        [Test]
        public void TestWriteUnknownCommand()
        {
            TestWriteToStream(SmtpServerResponses.UnknownCommand, stream_ => stream_.WriteUnknownCommandError());
        }

        [Test]
        public void TestReadCommandAsync()
        {
            var memStream = new MemoryStream();
            var smtpStream = new SmtpServerStream(memStream);
            using (smtpStream)
            {
                var readTask = smtpStream.ReadCommandAsync();
                const string issuedCommand = "EHLO bar.com\r\n";
                var bytes = Encoding.ASCII.GetBytes(issuedCommand);
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                readTask.Wait();
                var command = readTask.Result;
                Assert.That(command, Is.EqualTo(issuedCommand));   
            }
        }

        [Test]
        public void TestReadMailDataAsync()
        {
            var lines = new[]
                        {
                            "Date: Thu, 21 May 1998 05:33:29 -0700\r\n",
                            "From: John Q. Public <JQP@bar.com>\r\n",
                            "Subject:  The Next Meeting of the Board\r\n",
                            "To: Jones@xyz.com\r\n",
                            "\r\n",
                            "Bill:\r\n",
                            "The next meeting of the board of directors will be\r\n",
                            "on Tuesday.\r\n",
                            "\r\n",
                            ".",
                            "\r\n"
                        };

            TestReadMailData(lines);
        }

        [Test]
        public void TestReadMailDataWhenEmptyBody()
        {
            var lines = new[] {"\r\n.\r\n"};
            TestReadMailData(lines);

            lines = new[] {"\r\n", ".", "\r\n"};
            TestReadMailData(lines);
        }

        [Test]
        public void TestReadMailDataWhenTerminatorSplitAcrossPackets()
        {
            var lines = new[]
                        {
                            "Date: Thu, 21 May 1998 05:33:29 -0700\r\n",
                            "From: John Q. Public <JQP@bar.com>\r\n",
                            "Subject:  The Next Meeting of the Board\r\n",
                            "To: Jones@xyz.com\r\n",
                            "\r\n",
                            "Bill:\r\n",
                            "The next meeting of the board of directors will be\r\n",
                            "on Tuesday.\r\n",
                            "\r\n",
                            ".",
                            "\r\n"
                        };
            var netStream = new MockNetworkStream();
            var smtpStream = new SmtpServerStream(netStream);
            using (smtpStream)
            {
                var readTask = smtpStream.ReadDataAsync();

                var expectedData = new StringBuilder();
                foreach (var line in lines)
                {
                    expectedData.Append(line);
                    var bytes = Encoding.ASCII.GetBytes(line);
                    netStream.WriteRequest(bytes, 0, bytes.Length);
                }

                expectedData.Replace("\r\n.\r\n", string.Empty);
                readTask.Wait();
                var data = readTask.Result;
                Assert.That(data, Is.EqualTo(expectedData.ToString()));   
            }
        }

        private void TestReadMailData(string[] lines_)
        {
            var memStream = new MemoryStream();
            var smtpStream = new SmtpServerStream(memStream);
            using (smtpStream)
            {
                var expectedData = new StringBuilder();
                foreach (var line in lines_)
                {
                    expectedData.Append(line);
                    var bytes = Encoding.ASCII.GetBytes(line);
                    memStream.Write(bytes, 0, bytes.Length);
                }

                expectedData.Replace("\r\n.\r\n", string.Empty); // Remove the terminator. We don't expect to get this as part of the message
                memStream.Seek(0, SeekOrigin.Begin);
                var readTask = smtpStream.ReadDataAsync();
                readTask.Wait();
                var data = readTask.Result;
                Assert.That(data, Is.EqualTo(expectedData.ToString()));   
            }
        }

        private void TestWriteToStream(string expectedOutput_, SmtpStreamAction action_)
        {
            var memStream = new MemoryStream();
            var smtpStream = new SmtpServerStream(memStream);
            using (smtpStream)
            {
                action_(smtpStream);
                var buffer = new byte[100];
                memStream.Seek(0, SeekOrigin.Begin);
                var readCount = memStream.Read(buffer, 0, buffer.Length);
                var welcome = Encoding.ASCII.GetString(buffer, 0, readCount);
                Assert.That(welcome, Is.EqualTo(expectedOutput_));   
            }
        }

        private delegate void SmtpStreamAction(SmtpServerStream stream_);
    }
}

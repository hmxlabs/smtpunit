using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// A wrapper around a stream that is able to understand the SMTP protocol.
    /// 
    /// Parses the incoming commands/data and makes it available via its public API
    /// and encapsulates the functionality to respond to the SMTP client
    /// </summary>
    public class SmtpServerStream : IDisposable
    {
        /// <summary>
        /// The expected encoding to receive data in
        /// </summary>
        public static readonly Encoding WireEncoding = Encoding.ASCII;

        /// <summary>
        /// The SMTP specified line terminator character: <code>CRLF</code>
        /// </summary>
        public const string LineTerminator = "\r\n"; // <CRLF> as per SMTP specification.

        /// <summary>
        /// The SMTP specified terminator for the mail body
        /// </summary>
        public const string DataTerminator = "\r\n.\r\n";

        /// <summary>
        /// The STMP stop charactoer
        /// </summary>
        public const string Stop = ".";

        /// <summary>
        /// Creates an instance of a <code>SmtpServerStream</code> taking in the <code>Stream</code>
        /// object it should read and write to.
        /// </summary>
        /// <param name="stream_"></param>
        public SmtpServerStream(Stream stream_)
        {
            if (null == stream_)
                throw new ArgumentNullException(nameof(stream_));

            _stream = stream_;
        }

        /// <summary>
        /// Implementation of Disposable pattern.
        /// </summary>
        ~SmtpServerStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Frees resources held by this object. NOTE: This will call
        /// <code>Dispose</code> on the <code>Stream</code> object
        /// provided in the constructor
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Closes the underlying stream. This is equivalent to calling
        /// <code>Stream.Close</code> on the <code>Stream</code> object
        /// passed in the constructor
        /// </summary>
        public void Close()
        {
            _stream.Close();
        }

        /// <summary>
        /// Asynchronc reads the provided stream till it reads an SMTP command. Returns a <code>Task</code>
        /// that has been started.
        /// 
        /// See also documentation for <code>ReadCommand</code>
        /// </summary>
        /// <returns>A <code>Tast</code> for the client to process the command</returns>
        public Task<string> ReadCommandAsync()
        {
            var task = new Task<string>(ReadCommand);
            task.Start();
            return task;
        }

        /// <summary>
        /// Reads the next client command on the stream.
        /// 
        /// Under normal operation the client should send a command and end it with the <code>CRLF</code> line
        /// terminator. The client should then send no further data and wait for a response. Given
        /// this, it should be safe to keep reading till we get a CRLF and not worry about reading
        /// beyond the <code>CRLF</code>
        ///
        /// As this is only a mock SMTP server also, no attempt is made to timeout if we don't read
        /// a <code>CRLF</code> in a reasonable period of time and it is assumed the client is on the local machine
        /// and behaves correctly.
        /// </summary>
        /// <returns>A string representation of the command that was read</returns>
        public string ReadCommand()
        {
            var readBuffer = new byte[CommandReadBufferSize];
            var dataBuffer = new StringBuilder();
            var receivingCommand = true;
            
            while (receivingCommand)
            {
                var readCount = _stream.Read(readBuffer, 0, readBuffer.Length);
                var commandDataStr = WireEncoding.GetString(readBuffer, 0, readCount);
                dataBuffer.Append(commandDataStr);

                if (commandDataStr.EndsWith(LineTerminator))
                    receivingCommand = false;
            }

            return dataBuffer.ToString();
        }

        /// <summary>
        /// Read the data section in an SMTP transmission asynchronously. Returns a <code>Tast</code>
        /// that has been started.
        /// </summary>
        /// <returns></returns>
        public Task<string> ReadDataAsync()
        {
            var task = new Task<string>(ReadData);
            task.Start();
            return task;
        }

        /// <summary>
        /// Reads the data section in an SMTP transmission and returns it as string
        /// </summary>
        /// <returns>A string representation of the data section in an SMTP transmission</returns>
        public string ReadData()
        {
            var readBuffer = new byte[DataReadBufferSize];
            var dataBuffer = new StringBuilder();
            var receivingData = true;

            while (receivingData)
            {
                var readCount = _stream.Read(readBuffer, 0, readBuffer.Length);
                var dataStr = WireEncoding.GetString(readBuffer, 0, readCount);
                dataBuffer.Append(dataStr);

                if (dataStr.EndsWith(DataTerminator))
                    break;
                
                // Potentially... the <CRLF>.<CRLF> can be split across packets ... so check if the end of the databuffer now
                // contains the terminator sequence
                if (dataBuffer.Length < DataTerminator.Length)
                    continue;

                var bufferStartIndex = dataBuffer.Length - DataTerminator.Length;
                var terminatorIndex = 0;
                var terminatorReceived = true;
                for (int dataIndex = bufferStartIndex; dataIndex < dataBuffer.Length; dataIndex++)
                {
                    if (DataTerminator[terminatorIndex] != dataBuffer[dataIndex])
                    {
                        terminatorReceived = false;
                        break;
                    }
                    terminatorIndex++;
                }

                receivingData = !terminatorReceived;
            }

            return StripLeadingStopsAndTerminator(dataBuffer);
        }

        /// <summary>
        /// Writes the SMTP welcome message to the underlying stream
        /// </summary>
        public void WriteWelcome()
        {
            var str = $"{SmtpStatusCode.ServiceReady.ToCodeString()} Test Simple Mail Transfer Service Ready";
            WriteResponse(str);
        }

        /// <summary>
        /// Writes a response to the HELO or EHLO SMTP message
        /// </summary>
        /// <param name="clientId_"></param>
        public void WriteHelloResponse(string clientId_)
        {
            var str = $"{SmtpStatusCode.Ok.ToCodeString()} Mock SMTP Service greets {clientId_}";
            WriteResponse(str);
        }

        /// <summary>
        /// Writes an SMTP goodbye message to the underlying stream
        /// </summary>
        public void WriteGoodbye()
        {
            var str = $"{SmtpStatusCode.ServiceClosingTransmissionChannel.ToCodeString()} Mock SMTP Server closing transmission channel";
            WriteResponse(str);
        }

        /// <summary>
        /// Writes an SMTP bad sequence of commands error message to the underlying stream
        /// </summary>
        public void WriteBadCommandSequenceError()
        {
            var str = $"{SmtpStatusCode.BadCommandSequence.ToCodeString()} Bad sequence of commands";
            WriteResponse(str);
        }

        /// <summary>
        /// Writes an SMTP OK message to the underlying stream
        /// </summary>
        public void WriteOk()
        {
            var str = $"{SmtpStatusCode.Ok.ToCodeString()} OK";
            WriteResponse(str);
        }

        /// <summary>
        /// Writes the Start Mail Input (an intermediate OK) to the underlying stream
        /// </summary>
        public void WriteIntermediateOk()
        {
            var str = $"{SmtpStatusCode.StartMailInput.ToCodeString()} Start mail input; end with <CRLF>.<CRLF>";
            WriteResponse(str);
        }

        /// <summary>
        /// Wrties a Command Not Implemented SMTP response to the underlying stream
        /// </summary>
        public void WriteUnknownCommandError()
        {
            var str = $"{SmtpStatusCode.CommandNotImplemented.ToCodeString()} Command not implemented by Mock SMTP server";
            WriteResponse(str);
        }

        /// <summary>
        /// Resets the state of the stream to as though a new client has just connected. In the current implementation
        /// however this is a no op as there is no state contained within this object
        /// </summary>
        public void Reset()
        {
               
        }

        private void Dispose(bool disposing_)
        {
            if (!disposing_)
                return; // no unmanaged resources to dispose

            _stream.Dispose();
        }

        private void WriteResponse(string response_)
        {
            var responseWithTerminator = string.Format("{0}{1}", response_, LineTerminator);
            var bytes = WireEncoding.GetBytes(responseWithTerminator);
            _stream.Write(bytes, 0, bytes.Length);
        }

        private string StripLeadingStopsAndTerminator(StringBuilder data_)
        {
            // Any lines that start with a "." but have additional characters should have the
            // leading "." stripped from the line.
            // See: https://www.ietf.org/rfc/rfc2821.txt Section 4.5.2
            var lines = data_.ToString().Split(new [] {LineTerminator}, StringSplitOptions.None);
            var parsedData = new StringBuilder();
            
            for (int index = 0; index < lines.Length - 1; index++)  // limit is Length - 1 because the last line is just a /r/n
                                                                    // and that will get appended anyway by the last AppendLine
            {
                var line = lines[index];
                if (null != line && Stop.Length < line.Length && line.StartsWith(Stop))
                {
                    parsedData.AppendLine(line.Substring(Stop.Length));
                }
                else
                {
                    parsedData.AppendLine(line);
                }
            }

            var mailBody = parsedData.ToString();
            return mailBody.Replace(DataTerminator, string.Empty);
        }

        private readonly Stream _stream;

        private const int CommandReadBufferSize = 256;
        private const int DataReadBufferSize = 1024 * 5; // 5KB
    }
}

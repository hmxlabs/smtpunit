using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// An implementation of <code>IMailMessage</code>. See <code>IMailMessage</code> for 
    /// further details
    /// </summary>
    public class DummyDummyMailMessage : IDummyMailMessage
    {
        private static class HeaderNames
        {
            public const string Subject = "Subject";
            public const string TransferEncodingType = "Content-Transfer-Encoding";
        }

        /// <summary>
        /// Constructor taking all required information to create a subsequently immutable
        /// mail message implementation.
        /// </summary>
        /// <param name="from_">The email address of the sender</param>
        /// <param name="recipients_">A list of email addresses of the recipients</param>
        /// <param name="mailContent_">The raw data received by the mail server as the content of the email</param>
        public DummyDummyMailMessage(string from_, IEnumerable<string> recipients_, string mailContent_)
        {
            Sender = from_;
            _recipients.AddRange(recipients_);
            RawMailData = mailContent_;
            ParseMailContent(mailContent_);
        }

        /// <summary>
        /// See <code>IMailMessage.Sender</code>
        /// </summary>
        public string Sender { get; }

        /// <summary>
        /// See <code>IMailMessage.Subject</code>
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// See <code>IMailMessage.Recipients</code>
        /// </summary>
        public IEnumerable<string> Recipients => _recipients;

        /// <summary>
        /// See <code>IMailMessage.WasSentTo</code>
        /// </summary>
        /// <param name="recipient_"></param>
        /// <returns></returns>
        public bool WasSentTo(string recipient_)
        {
            return _recipients.Contains(recipient_);
        }

        /// <summary>
        /// See <code>IMailMessage.Body</code>
        /// </summary>
        public string Body { get; private set; }

        /// See <code>IMailMessage.RawMailData</code>
        public string RawMailData { get; }

        /// <summary>
        /// /// See <code>IMailMessage.ContainsHeader</code>
        /// </summary>
        /// <param name="name_"></param>
        /// <returns></returns>
        public bool ContainsHeader(string name_)
        {
            return _headers.ContainsKey(name_);
        }

        /// <summary>
        /// /// See <code>IMailMessage.GetHeader</code>
        /// </summary>
        /// <param name="name_"></param>
        /// <returns></returns>
        public string GetHeader(string name_)
        {
            return _headers[name_];
        }

        private void ParseMailContent(string mailContent_)
        {
            var lines = mailContent_.Split(new[] { SmtpServerStream.LineTerminator }, StringSplitOptions.None);
            var bodyStartIndex = -1;
            for (int index = 0; index < lines.Length; index++)
            {
                var currentLine = lines[index];
                if (-1 == bodyStartIndex)
                {
                    if (string.IsNullOrWhiteSpace(currentLine))
                        bodyStartIndex = index + 1; // +1 to account for the extra line that is added by the sender
                    else
                        ParseHeaderLine(currentLine);
                }
            }

            Body = CreateBody(lines, bodyStartIndex);
        }

        private void ParseHeaderLine(string headerLine_)
        {
            var splitIndex = headerLine_.IndexOf(':');
            if (0 > splitIndex)
                return; // Not a header row we know how to parse

            var name = headerLine_.Substring(0, splitIndex);
            var value = headerLine_.Substring(splitIndex + 2); // Convention seems to be to seperate with colon + space
            _headers[name] = value;
            if (HeaderNames.Subject.Equals(name))
                Subject = value;
        }

        private string CreateBody(string[] lines_, int startIndex_)
        {
            // Need to know the content transfer encoding type before we try and create the body.
            var encodingTypeStr = "Unknown";
            if (_headers.ContainsKey(HeaderNames.TransferEncodingType))
                encodingTypeStr = _headers[HeaderNames.TransferEncodingType];

            var transferEncoding = ParseTransferEncoding(encodingTypeStr);
            var body = ConcatenateBodyText(lines_, startIndex_);

            if (null == body)
                return null;

            if (TransferEncoding.EightBit == transferEncoding ||
                TransferEncoding.SevenBit == transferEncoding)
                return body;

            if (TransferEncoding.QuotedPrintable == transferEncoding)
                return ParseQuotedPrintable(body);

            return null;
        }

        private string ConcatenateBodyText(string[] lines_, int startIndex_)
        {
            if (null == lines_ || 0 == lines_.Length)
                return null;

            if (startIndex_ >= lines_.Length)
                return null;

            if (0 > startIndex_)
                startIndex_ = 0; // Looks like we didn't find a blank line to indicate the end of the headers... treat the whole thing as the mail body

            var body = new StringBuilder();
            for (int index = startIndex_; index < lines_.Length - 1; index++)
            {
                body.AppendLine(lines_[index]);
            }
            body.Append(lines_[lines_.Length - 1]); // Need to append the last line withouth an AppendLine to avoid an extra CRLF

            return body.ToString();
        }

        private string ParseQuotedPrintable(string input_)
        {
            return Rfc2047Parser.ParseQuotedPrintable(Encoding.ASCII, input_);
        }

        private TransferEncoding ParseTransferEncoding(string encodingStr_)
        {
            TransferEncoding encoding;
            if (Enum.TryParse(encodingStr_, out encoding))
                return encoding;

            if ("8bit".Equals(encodingStr_, StringComparison.InvariantCultureIgnoreCase))
                return TransferEncoding.EightBit;

            if ("7bit".Equals(encodingStr_, StringComparison.InvariantCultureIgnoreCase))
                return TransferEncoding.SevenBit;

            if ("quoted-printable".Equals(encodingStr_, StringComparison.InvariantCultureIgnoreCase))
                return TransferEncoding.QuotedPrintable;

            if ("Base64".Equals(encodingStr_, StringComparison.InvariantCultureIgnoreCase))
                return TransferEncoding.Base64;

            return TransferEncoding.Unknown;
        }

        private readonly List<string> _recipients = new List<string>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
    }
}

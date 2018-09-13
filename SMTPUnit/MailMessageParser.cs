using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Doesn't appear to be used anymore. Delete this?
    /// </summary>
    public class MailMessageParser
    {
        public static MailMessage CreateMailMessage(string from_, IList<string> to_, string data_)
        {
            var message = new MailMessage();
            message.From = new MailAddress(from_);

            foreach (var to in to_)
            {
                message.To.Add(new MailAddress(to));
            }

            var lines = data_.Split(new[] {SmtpServerStream.LineTerminator}, StringSplitOptions.None);
            var bodyStartIndex = -1;
            for (int index = 0; index < lines.Length; index++)
            {
                var currentLine = lines[index];
                if (-1 == bodyStartIndex && string.IsNullOrWhiteSpace(currentLine))
                    bodyStartIndex = index + 1;

                if (currentLine.StartsWith("Subject"))
                    message.Subject = ParseSubject(currentLine);
            }

            message.Body = CreateBody(lines, bodyStartIndex);
            return message;
        }

        private static string CreateBody(string[] lines_, int startIndex_)
        {
            var body = new StringBuilder();
            for (int index = startIndex_; index < lines_.Length-1; index++)
            {
                body.AppendLine(lines_[index]);
            }
            
            return body.ToString();
        }

        private static string ParseSubject(string line_)
        {
            var startIndex = line_.IndexOf(':') + 2; // Convention seems to be to seperate with colon + space
            return line_.Substring(startIndex);
        }
    }
}

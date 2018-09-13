namespace HmxLabs.SmtpUnit.Test
{
    public static class SmtpServerRequests
    {
        public const string Hello = "EHLO unit-test\r\n";
        public const string Mail = "MAIL FROM:<Smith@bar.com>\r\n";
        public const string Recipient = "RCPT TO:<Jones@foo.com>\r\n";
        public const string AdditionalRecipient = "RCPT TO:<Green@foo.com>\r\n";
        public const string Data = "DATA \r\n";
        public const string MailBody = "Blah blah blah...\r\n...etc. etc. etc.\r\n.\r\n";
    }
}

namespace HmxLabs.SmtpUnit.Test
{
    public static class SmtpServerResponses
    {
        public const string Welcome = "220 Test Simple Mail Transfer Service Ready\r\n";
        public const string Ok = "250 OK\r\n";
        public const string StartMailInput = "354 Start mail input; end with <CRLF>.<CRLF>\r\n";
        public const string BadCommandSequence = "503 Bad sequence of commands\r\n";
        public const string Goodbye = "221 Mock SMTP Server closing transmission channel\r\n";
        public const string UnknownCommand = "502 Command not implemented by Mock SMTP server\r\n";

        public static string Greeting(string clientId_)
        {
            return $"250 Mock SMTP Service greets {clientId_}\r\n";
        }
    }
}

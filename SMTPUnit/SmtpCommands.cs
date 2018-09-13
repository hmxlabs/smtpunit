using System;

namespace HmxLabs.SmtpUnit
{
    internal static class SmtpCommands
    {
        public const string Hello = "HELO";
        public const string HelloExtended = "EHLO";
        public const string Quit = "QUIT";
        public const string Reset = "RSET";
        public const string Mail = "MAIL";
        public const string Recipient = "RCPT";
        public const string Data = "DATA";

        public static bool IsHello(this string command_)
        {
            return IsCommand(Hello, command_) || IsCommand(HelloExtended, command_);
        }

        public static bool IsQuit(this string command_)
        {
            return IsCommand(Quit, command_);
        }

        public static bool IsReset(this string command_)
        {
            return IsCommand(Reset, command_);
        }

        public static bool IsRecipient(this string command_)
        {
            return IsCommand(Recipient, command_);
        }

        public static bool IsMail(this string command_)
        {
            return IsCommand(Mail, command_);
        }

        public static bool IsData(this string command_)
        {
            return IsCommand(Data, command_);
        }

        public static bool IsCommand(string definedCommand_, string toTest_)
        {
            if (null == toTest_)
                return false;

            return toTest_.StartsWith(definedCommand_, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

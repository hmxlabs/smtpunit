namespace HmxLabs.SmtpUnit.Test
{
    public class PortHelper
    {
        public static int GetPort()
        {
            lock (PortLock) { return _port++; }
        }

        private const int StartPort = 10025;
        private static readonly object PortLock = new object();
        private static int _port = StartPort;
    }
}

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Represents a single client connection to the <code>ITestSmptServer</code>
    /// </summary>
    public interface IDummySmtpServerConnection
    {
        /// <summary>
        /// Start the connection and the email sending / receiving
        /// </summary>
        void Start();

        /// <summary>
        /// Shut down this connection
        /// </summary>
        void Stop();
    }
}

using System;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Interface defining the test email server
    /// </summary>
    public interface IDummySmtpServer : IDisposable
    {
        /// <summary>
        /// The port the email server is listening on
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Start the mail server. This will cause it to start listening on the <code>Port</code>
        /// and receiving mail messages
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the mail server. This will stop listening on <code>Port</code> and dispose of any
        /// used resources
        /// </summary>
        void Stop();

        /// <summary>
        /// A <code>IMailBag</code> with all the email that has been receieved.
        /// </summary>
        IMailBag ReceivedMails { get; }
    }
}

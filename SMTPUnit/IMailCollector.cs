namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// An internally used interface, required by implementations of <code>IMockSmtpServerConnection</code> in order
    /// to be able to provide the received emails.
    /// </summary>
    public interface IMailCollector
    {
        /// <summary>
        /// Comsume the provided mail message. No implementation is defined or expected by this interface
        /// </summary>
        /// <param name="message_"></param>
        void Add(IDummyMailMessage message_);
    }
}

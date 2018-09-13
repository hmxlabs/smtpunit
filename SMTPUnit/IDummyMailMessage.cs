using System.Collections.Generic;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Encapsulation of an email message. The class is deliberately immutable to promote easier to write and debug
    /// test cases. The expectation is that implementations of the message are only ever compared against and
    /// do not need to be modified during tests
    /// </summary>
    public interface IDummyMailMessage
    {
        /// <summary>
        /// The sender (from) of the email. This field contains a complete email address
        /// such as <code>contoso@microsoft.com</code>
        /// </summary>
        string Sender { get; }
        
        /// <summary>
        /// The subject of the email
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// An enumeration of all the recipients of the email. Currently no differentiation is made
        /// between the TO, CC or BCC fields. 
        /// </summary>
        IEnumerable<string> Recipients { get; }

        /// <summary>
        /// Convenience method that returns <code>true</code> if the provided recipient email address
        /// is within the list of <code>Recipients</code>
        /// </summary>
        /// <param name="recipient_">The email address to check</param>
        /// <returns></returns>
        bool WasSentTo(string recipient_);

        /// <summary>
        /// A string representation of the email body. Note that if the email body is HTML this will contain HTML.
        /// if the email body is plain text, this will contain plain text.
        /// </summary>
        string Body { get; }
        
        /// <summary>
        /// Contains the raw data passed to the mail server (HELO command and everything afterwards) without attempting
        /// to parse it
        /// </summary>
        string RawMailData { get; }

        /// <summary>
        /// Returns <code>true</code> if the provided header name was supplied with this email
        /// </summary>
        /// <param name="name_">The name of the header</param>
        /// <returns></returns>
        bool ContainsHeader(string name_);

        /// <summary>
        /// Get the body of the header. Throws a <code>KeyNotFoundException</code> if the header
        /// is not part of this mail message. Use the <code>ContainsHeader</code> method first
        /// to check if the header is in the mail message.
        /// </summary>
        /// <param name="name_">The name of the header</param>
        /// <returns></returns>
        string GetHeader(string name_);
    }
}

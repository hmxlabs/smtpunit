using System.Collections.Generic;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// A container for mail messafges. The name bag is used deliberately in order to be
    /// non specific as to what type of container structures are used to hold the
    /// <code>IMailMessage</code>s.
    /// 
    /// Provides a number of methods to allow retrieval of the contained messages by
    /// subject, sender, recipient and body content.
    /// </summary>
    public interface IMailBag
    {
        /// <summary>
        /// Read only property specifying the number of mail messages
        /// are currently contained in this bag
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Removes all <code>IMailMessage</code>s that are currently in the bag so that it
        /// is empty.
        /// </summary>
        void Empty();

        /// <summary>
        /// Returns an enumeration of all messages in the bag
        /// </summary>
        IEnumerable<IDummyMailMessage> All { get; }

        /// <summary>
        /// Returns <code>true</code> if any messages are in this bag with the specified subject.
        /// An exact match is required to return <code>true</code> and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="subject_">The subject of the email to match against</param>
        /// <returns><code>true</code> if a match is found, else <code>false</code></returns>
        bool ContainsMailWithSubject(string subject_);

        /// <summary>
        /// Returns <code>true</code> if any messages are in this bag with the specified sender.
        /// An exact match is required to return <code>true</code> and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="sender_">The sender to match against</param>
        /// <returns><code>true</code> if a match is found, else <code>false</code></returns>
        bool ContainsMailWithSender(string sender_);

        /// <summary>
        /// Returns <code>true</code> if any messages are in this bag with the specified recipient.
        /// An exact match is required to return <code>true</code> and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="recipient_">The recipient to match against</param>
        /// <returns><code>true</code> if a match is found, else <code>false</code></returns>
        bool ContainsMailWithRecipient(string recipient_);

        /// <summary>
        /// Returns an enumeration of all emails that match the provided subject. Providing a <code>null</code>
        /// arguemnt equates to a match to any message and will return all messages in this bag and is equivalent
        /// to calling <code>All</code>
        /// 
        /// An exact match is required and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="subject_">The subject to match</param>
        /// <returns>An enumeration of <code>IMailMessages</code> which may be empty</returns>
        IEnumerable<IDummyMailMessage> GetMailsWithSubject(string subject_);

        /// <summary>
        /// Returns an enumeration of all emails that match the provided sender. Providing a <code>null</code>
        /// arguemnt equates to a match to any message and will return all messages in this bag and is equivalent
        /// to calling <code>All</code>
        /// 
        /// An exact match is required  and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="sender_">The sender to match</param>
        /// <returns>An enumeration of <code>IMailMessages</code> which may be empty</returns>
        IEnumerable<IDummyMailMessage> GetMailsWithSender(string sender_);

        /// <summary>
        /// Returns an enumeration of all emails that match the provided recipient. Providing a <code>null</code>
        /// arguemnt equates to a match to any message and will return all messages in this bag and is equivalent
        /// to calling <code>All</code>
        /// 
        /// An exact match is required  and regular expressions
        /// or other forms of pattern matching are not supported
        /// </summary>
        /// <param name="recipient_">The recipient of the email</param>
        /// <returns>An enumeration of <code>IMailMessages</code> which may be empty</returns>
        IEnumerable<IDummyMailMessage> GetMailsWithRecipient(string recipient_);

        /// <summary>
        /// Return an enumeration of all emails matching the provided parameters. A <code>null</code>
        /// parameter indicates no match is required and it will be ignored. If a parameter is provided
        /// an exact match is required and pattern matching is not supported.
        /// 
        /// The exception to this is the bodyContent parameter. This will use the <code>string.Contains</code>
        /// function to determine a match.
        /// </summary>
        /// <param name="sender_">The sender to match</param>
        /// <param name="to_">The recipient of the email</param>
        /// <param name="subject_">The subject to match</param>
        /// <param name="bodyContent_">content to search for in the mail body</param>           
        /// <returns></returns>
        IEnumerable<IDummyMailMessage> GetMailsWith(string sender_, IEnumerable<string> to_, string subject_, string bodyContent_);
    }
}

using System.Collections.Generic;
using System.Linq;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// An implementation of <code>IMailBag</code>. Please see
    /// documentation for <code>IMailBag</code> for further details.
    /// </summary>
    public class MailBag : IMailCollector, IMailBag
    {
        /// <summary>
        /// The key value used internally for the subject when its value is actually <code>null</code>
        /// </summary>
        public const string NullSubject = "[NULL]";

        /// <summary>
        /// See <code>IMailBag.Add</code>
        /// </summary>
        /// <param name="message_"></param>
        public void Add(IDummyMailMessage message_)
        {
            lock (_lock)
            {
                _all.Add(message_);
                _bySender.Add(message_.Sender, message_);
                _bySubject.Add(message_.Subject ?? NullSubject, message_);
                foreach (var recipient in message_.Recipients)
                {
                    _byRecipient.Add(recipient, message_);
                }
            }
        }

        /// <summary>
        /// See <code>IMailBag.Empty</code>
        /// </summary>
        public void Empty()
        {
            lock (_lock)
            {
                _all.Clear();
                _byRecipient.Clear();
                _bySender.Clear();
                _bySubject.Clear();
            }
        }

        /// <summary>
        /// See <code>IMailBag.Count</code>
        /// </summary>
        public int Count
        {
            get { lock(_lock) { return _all.Count; } }
        }

        /// <summary>
        /// See <code>IMailBag.All</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> All
        {
            get { lock(_lock) { return _all; } }
        }

        /// <summary>
        /// See <code>IMailBag.ContainsMailWithSubject</code>
        /// </summary>
        /// <param name="subject_"></param>
        public bool ContainsMailWithSubject(string subject_)
        {
            lock (_lock)
            {
                return _bySubject.ContainsKey(subject_);    
            }
        }

        /// <summary>
        /// See <code>IMailBag.ContainsMailWithSender</code>
        /// </summary>
        public bool ContainsMailWithSender(string sender_)
        {
            lock (_lock)
            {
                return _bySender.ContainsKey(sender_);   
            }
        }

        /// <summary>
        /// See <code>IMailBag.ContainsMailWithRecipient</code>
        /// </summary>
        public bool ContainsMailWithRecipient(string recipient_)
        {
            lock (_lock)
            {
                return _byRecipient.ContainsKey(recipient_);   
            }
        }

        /// <summary>
        /// See <code>IMailBag.GetMailsWithSubject</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> GetMailsWithSubject(string subject_)
        {
            lock (_lock)
            {
                return null == subject_ ? _all : _bySubject[subject_];
            }
        }

        /// <summary>
        /// See <code>IMailBag.GetMailsWithSender</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> GetMailsWithSender(string sender_)
        {
            lock (_lock)
            {
                return null == sender_ ? _all : _bySender[sender_];    
            }
        }

        /// <summary>
        /// See <code>IMailBag.GetMailsWithRecipient</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> GetMailsWithRecipient(string recipient_)
        {
            lock (_lock)
            {
                return null == recipient_ ? _all : _byRecipient[recipient_];
            }
        }

        /// <summary>
        /// See <code>IMailBag.GetMailsWithRecipients</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> GetMailsWithRecipients(IEnumerable<string> to_)
        {
            lock (_lock)
            {
                if (null == to_)
                    return _all;

                var toList = to_.ToList();
                if (0 == toList.Count)
                    return _all;

                if (1 == toList.Count)
                    return GetMailsWithRecipient(toList[0]);

                var messages = toList.Select(GetMailsWithRecipient).ToList();
                IEnumerable<IDummyMailMessage> matchingMessages = messages[0];
                for (int index = 1; index < messages.Count; index++)
                {
                    matchingMessages = messages[index].Intersect(matchingMessages);
                }

                return matchingMessages;
            }
        }

        /// <summary>
        /// See <code>IMailBag.GetMailsWith</code>
        /// </summary>
        public IEnumerable<IDummyMailMessage> GetMailsWith(string sender_, IEnumerable<string> to_, string subject_, string bodyContent_)
        {
            lock (_lock)
            {
                var matchingSender = GetMailsWithSender(sender_);
                var matchingSubject = GetMailsWithSubject(subject_);
                var matchingRecipient = GetMailsWithRecipients(to_);

                var matchingMails = matchingSender.Intersect(matchingSubject).Intersect(matchingRecipient);
                if (null == bodyContent_)
                    return matchingMails;

                return matchingMails.Where(matchingMail_ => null != matchingMail_.Body && matchingMail_.Body.Contains(bodyContent_));    
            }
        }

        private readonly object _lock = new object();
        private readonly List<IDummyMailMessage> _all = new List<IDummyMailMessage>();
        private readonly MailDictionary _bySender = new MailDictionary();
        private readonly MailDictionary _bySubject = new MailDictionary();
        private readonly MailDictionary _byRecipient = new MailDictionary();
    }
}

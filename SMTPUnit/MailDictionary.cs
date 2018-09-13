using System.Collections.Generic;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// A simple dictionary of string based keys to <code>IMailMessage</code>s
    /// </summary>
    public class MailDictionary
    {
        /// <summary>
        /// Add the specified mail message to this dictionary against the specified key.
        /// If the key provided already exists then override the current contents with the
        /// new dictionary.
        /// </summary>
        /// <param name="key_">The key to store the mail message against</param>
        /// <param name="message_">The mail message to store</param>
        public void Add(string key_, IDummyMailMessage message_)
        {
            List<IDummyMailMessage> messages;
            if (_data.ContainsKey(key_))
                messages = _data[key_];
            else
            {
                messages = new List<IDummyMailMessage>();
                _data.Add(key_, messages);
            }
            messages.Add(message_);
        }

        /// <summary>
        /// Indexer for accessing the mail messages
        /// </summary>
        /// <param name="key_">The string based key</param>
        /// <returns></returns>
        public IList<IDummyMailMessage> this[string key_]
        {
            get
            {
                return !_data.ContainsKey(key_) ? new List<IDummyMailMessage>() : _data[key_];
            }
        }

        /// <summary>
        /// Returns <code>true</code> if this dictionary contains the specified key
        /// </summary>
        /// <param name="key_"></param>
        /// <returns></returns>
        public bool ContainsKey(string key_)
        {
            return _data.ContainsKey(key_);
        }

        /// <summary>
        /// Empty this dictionary.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// A count of the number of keys that are stored
        /// </summary>
        public int KeyCount => _data.Count;

        private readonly Dictionary<string, List<IDummyMailMessage>> _data = new Dictionary<string, List<IDummyMailMessage>>();
    }
}

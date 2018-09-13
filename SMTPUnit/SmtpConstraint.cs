using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework.Constraints;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Implementation of an NUnit constraint in order to provide a nice fluent API
    /// to write test cases against expected emails. Examples of usage of the constraint
    /// should look something like :
    /// 
    /// Assert.That(smtpServer, Received.Mail(1).From("someone@address.com"));
    /// Assert.That(smtpServer, Received.Mail(5));
    /// Assert.That(smtpServer, Received.Mail().Subject("foo bar"));
    /// Assert.That(smtpServer, Received.Mail().To("cookie.monster@sesame.street"));
    /// 
    /// For further information see documentation on <code>NUnit</code> constraints
    /// </summary>
    public class SmtpConstraint : Constraint
    {
        /// <summary>
        /// Basic type checking to ensure we have the objects we expect
        /// </summary>
        /// <param name="actual_"></param>
        /// <returns></returns>
        public override bool Matches(object actual_)
        {
            var smtpServer = actual_ as IDummySmtpServer;
            if (null != smtpServer)
                return Matches(smtpServer.ReceivedMails);

            _errorMessage.Append("SMTP Server check failed. The provided object was of type [");
            _errorMessage.Append(actual_.GetType());
            _errorMessage.AppendLine("]. Expected an implementation of IMockSmtpServer");
            return false;
        }

        /// <summary>
        /// Implementation of the <code>Count</code> constraint path.
        /// </summary>
        /// <param name="count_">The expected count of mail messages</param>
        /// <returns></returns>
        public SmtpConstraint Count(int count_)
        {
            _count = count_;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender_"></param>
        /// <returns></returns>
        public SmtpConstraint From(string sender_)
        {
            if (null == sender_)
                throw new ArgumentNullException("sender_");

            _from = sender_;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject_"></param>
        /// <returns></returns>
        public SmtpConstraint Subject(string subject_)
        {
            _subject = subject_;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body_"></param>
        /// <returns></returns>
        public SmtpConstraint BodyContains(string body_)
        {
            if (null == body_)
                throw new ArgumentNullException("body_");

            _body = body_;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipient_"></param>
        /// <returns></returns>
        public SmtpConstraint To(string recipient_)
        {
            if (null == recipient_)
                throw new ArgumentNullException("recipient_");

            _to.Add(recipient_);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailBag_"></param>
        /// <returns></returns>
        public bool Matches(IMailBag mailBag_)
        {
            if (null == mailBag_)
            {
                _errorMessage.Append("SMTP Server check failed. The provided mailbag was null");
                return false;
            }

            GetMatchingEmails(mailBag_);

            if (_count.HasValue)
            {
                if (_count == _matchingMessages.Count)
                    return true;

                _errorMessage.AppendFormat("Expected {0} but found {1} emails ", _count, _matchingMessages.Count);
                AddMatchConditionToErrorMessage();
                return false;
            }

            if (0 != _matchingMessages.Count) 
                return true;

            _errorMessage.AppendFormat("Found no emails ");
            AddMatchConditionToErrorMessage();
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer_"></param>
        public override void WriteMessageTo(MessageWriter writer_)
        {
            writer_.WriteLine(_errorMessage.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer_"></param>
        public override void WriteDescriptionTo(MessageWriter writer_)
        {
            // No op. Should never be called
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer_"></param>
        public override void WriteActualValueTo(MessageWriter writer_)
        {
            // No op. Should never be called
        }

        private void GetMatchingEmails(IMailBag mailBag_)
        {
            _matchingMessages.AddRange(mailBag_.GetMailsWith(_from, _to, _subject, _body));
        }

        private void AddMatchConditionToErrorMessage()
        {
            if (null != _from || null != _subject || (null != _to && 0 < _to.Count) || null != _body)
                _errorMessage.Append("matching the following - ");

            if (null != _from)
                _errorMessage.AppendFormat("Sender: {0} ", _from);

            if (null != _subject)
                _errorMessage.AppendFormat("Subject: {0} ", _subject);

            if (null != _to && 0 < _to.Count)
            {
                _errorMessage.AppendFormat("To: ");
                foreach (var to in _to)
                {
                    _errorMessage.AppendFormat("{0},", to);
                }
            }
            
            if (null != _body)
                _errorMessage.AppendFormat(", Body: {0}", _body);
        }

        private readonly StringBuilder _errorMessage = new StringBuilder();
        private readonly List<string> _to = new List<string>();
        private readonly List<IDummyMailMessage> _matchingMessages = new List<IDummyMailMessage>();
        
        private string _from;
        private string _subject;
        private string _body;
        private int? _count;
    }
}
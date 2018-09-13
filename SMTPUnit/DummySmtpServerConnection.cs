using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Implementation of <code>ITestSmtpServerConnection</code>
    /// </summary>
    public class DummySmtpServerConnection : IDummySmtpServerConnection
    {
        /// <summary>
        /// Creates a new instance of a <code>TestSmtpServerConnection</code>. Requires an <code>SmtpServerStream</code>,
        /// a <code>IMailCollector</code> (usually a <code>IMailBag</code> to consume the receive emails and a
        /// <code>ITestSmtpServerConnectionCollection</code> (from the parent server).
        /// </summary>
        /// <param name="stream_"></param>
        /// <param name="mailCollector_"></param>
        /// <param name="connections_"></param>
        public DummySmtpServerConnection(SmtpServerStream stream_, IMailCollector mailCollector_, IDummySmtpServerConnectionCollection connections_)
        {
            if (null == stream_)
                throw new ArgumentNullException("stream_");

            if (null == mailCollector_)
                throw new ArgumentNullException("mailCollector_");

            if (null == connections_)
                throw new ArgumentNullException("connections_");

            _lock = new object();
            _mailCollector = mailCollector_;
            _connections = connections_;
            _stream = stream_;
            _recipientList = new List<string>();
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnection.Start</code>
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                _stream.WriteWelcome();
                _status = SmtpStatus.Connected;
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);
            }
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnection.Stop</code>
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                ProcessQuit();
            }
        }

        private void OnCommandReceived(Task<string> task_)
        {
            var command = task_.Result;
            if (command.IsHello())
            {
                ProcessHello(command);
                return;
            }

            if (command.IsQuit())
            {
                ProcessQuit();
                return;
            }

            if (command.IsReset())
            {
                ProcessReset();
                return;
            }

            if (command.IsMail())
            {
                ProcessMail(command);
                return;
            }

            if (command.IsRecipient())
            {
                ProcessRecipient(command);
                return;
            }

            if (command.IsData())
            {
                ProcessData();
                return;
            }

            ProcessUnknownCommand();
        }

        private void ProcessUnknownCommand()
        {
            lock (_lock)
            {
                _stream.WriteUnknownCommandError();
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);   
            }
        }

        private void ProcessRecipient(string command_)
        {
            if (SmtpStatus.Mail != _status &&
                SmtpStatus.Recipient != _status)
            {
                lock (_lock)
                {
                    _stream.WriteBadCommandSequenceError();
                    _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);
                }
                return;
            }

            lock (_lock)
            {
                _recipientList.Add(ParseMailAddress(command_));
                _status = SmtpStatus.Recipient;
                _stream.WriteOk();
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);
            }
        }

        private void ProcessMail(string command_)
        {
            if (SmtpStatus.Identified != _status)
            {
                lock (_lock)
                {
                    _stream.WriteBadCommandSequenceError();
                    _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);
                }
                return;
            }

            lock (_lock)
            {
                _sender = ParseMailAddress(command_);
                _status = SmtpStatus.Mail;
                _stream.WriteOk();
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);   
            }
        }

        private void ProcessData()
        {
            if (SmtpStatus.Recipient != _status)
            {
                lock (_lock)
                {
                    _stream.WriteBadCommandSequenceError();
                    _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);
                }
                return;
            }

            lock (_lock)
            {
                _status = SmtpStatus.Data;
                _stream.WriteIntermediateOk();
                _stream.ReadDataAsync().ContinueWith(OnDataReceived);
            }
        }

        private void OnDataReceived(Task<string> task_)
        {
            lock (_lock)
            {
                _data = task_.Result;
                var mailMessage = new DummyDummyMailMessage(_sender, _recipientList.ToArray(), _data);
                _mailCollector.Add(mailMessage);
                ResetState();
                _stream.WriteOk();
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);   
            }
        }

        private static string ParseMailAddress(string command_)
        {
            var start = command_.IndexOf('<') + 1;
            var end = command_.IndexOf('>');
            var sender = command_.Substring(start, (end - start));
            return sender;
        }

        private void ProcessHello(string command_)
        {
            lock (_lock)
            {
                _clientId = command_.Substring(4).Trim(); // HELO or EHLO, everything afterwards is the client id.
                _status = SmtpStatus.Identified;
                _stream.Reset();
                _stream.WriteHelloResponse(_clientId);
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);   
            }
        }

        private void ProcessReset()
        {
            lock (_lock)
            {
                _stream.Reset();
                ResetState();
                _stream.ReadCommandAsync().ContinueWith(OnCommandReceived);   
            }
        }

        private void ProcessQuit()
        {
            lock (_lock)
            {
                _stream.WriteGoodbye();
                _status = SmtpStatus.Disconnected;
                _stream.Close();
                _stream.Dispose();
                _connections.Remove(this);
            }
        }

        private void ResetState()
        {
            lock (_lock)
            {
                _status = _status >= SmtpStatus.Identified ? SmtpStatus.Identified : SmtpStatus.Connected;
                _sender = null;
                _recipientList.Clear();
            }
        }

        private readonly object _lock;
        private readonly SmtpServerStream _stream;
        private readonly List<string> _recipientList;
        private readonly IMailCollector _mailCollector;
        private readonly IDummySmtpServerConnectionCollection _connections;
        private SmtpStatus _status;
        private string _clientId;
        private string _sender;
        private string _data;
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Implementation of <code>ITestSmtpServer</code>.
    /// 
    /// See documentation for <code>ITestSmtpServer</code> for further details
    /// </summary>
    public class DummySmtpServer : IDummySmtpServer
    {
        /// <summary>
        /// The default port for the SMPT server to listen on if one is not
        /// specified. Set to 25 as per the standard for SMTP servers
        /// </summary>
        public const int DefaultSmtpPort = 25;

        /// <summary>
        /// Default constructor. Creates an instance that will listen on the default
        /// SMTP port
        /// </summary>
        public DummySmtpServer() : this(DefaultSmtpPort)
        {
        }

        /// <summary>
        /// Constructor allowing spefication of the port to listen on
        /// </summary>
        /// <param name="port_">The port to listen on</param>
        public DummySmtpServer(int port_)
        {
            if (0 >= port_)
                throw new ArgumentException("Invalid port specified");
            
            _lock = new object();
            Port = port_;
            _listener = new TcpListener(IPAddress.Any, Port);
            _mailBag = new MailBag();
            _connections = new DummySmtpServerConnectionCollection();
        }

        /// <summary>
        /// Destructor, part of the Disposable pattern
        /// </summary>
        ~DummySmtpServer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of this server and releases all resources used. If the
        /// server has not already been stopped calling this method
        /// will also stop the server
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// See <code>ITestSmtpServer.Port</code>
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// See <code>ISmtpServer.Start</code>
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                _listener.Start();
                _listener.AcceptTcpClientAsync().ContinueWith(OnTcpClientAccepted);
            }
        }

        /// <summary>
        /// See <code>ISmtpServer.Stop</code>
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _listener.Stop();
                _connections.WaitTillEmpty();
            }
        }

        /// <summary>
        /// See <code>ITestSmtpServer.ReceivedMails</code>
        /// </summary>
        public IMailBag ReceivedMails { get { return _mailBag; } }

        private void OnTcpClientAccepted(Task<TcpClient> task_)
        {
            var tcpClient = task_.Result;
            var networkStream = tcpClient.GetStream();
            var smtpStream = new SmtpServerStream(networkStream);
            var smtpConnection = new DummySmtpServerConnection(smtpStream, _mailBag, _connections);
            _connections.Add(smtpConnection);
            smtpConnection.Start();

            lock (_lock)
            {
                _listener.AcceptTcpClientAsync().ContinueWith(OnTcpClientAccepted);    
            }
        }

        private void Dispose(bool disposing_)
        {
            if (!disposing_)
                return; // No unmanaged resources to dispose

            Stop();
        }

        private readonly TcpListener _listener;
        private readonly object _lock;
        private readonly MailBag _mailBag;
        private readonly DummySmtpServerConnectionCollection _connections;
    }
}

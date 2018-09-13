using System.Collections.Generic;
using System.Threading;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Implementation of <code>ITestSmtpServerConnectionCollection</code>
    /// </summary>
    public class DummySmtpServerConnectionCollection : IDummySmtpServerConnectionCollection
    {
        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection.Add</code>
        /// </summary>
        /// <param name="connection_"></param>
        public void Add(IDummySmtpServerConnection connection_)
        {
            lock (_lock)
            {
                _connections.Add(connection_);
                _emptyEvent.Reset();
            }
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection.Remove</code>
        /// </summary>
        /// <param name="connection_"></param>
        public void Remove(IDummySmtpServerConnection connection_)
        {
            lock (_lock)
            {
                _connections.Remove(connection_);
                if (0 == _connections.Count)
                    _emptyEvent.Set();
            }
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection</code>
        /// </summary>
        /// <returns></returns>
        public bool WaitTillEmpty()
        {
            return _emptyEvent.WaitOne();
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection</code>
        /// </summary>
        /// <param name="millisecondsToWait_"></param>
        /// <returns></returns>
        public bool WaitTillEmpty(int millisecondsToWait_)
        {
            return _emptyEvent.WaitOne(millisecondsToWait_);
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection</code>
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _connections.Count;
                }
            }
            
        }

        /// <summary>
        /// See <code>ITestSmtpServerConnectionCollection.All</code>
        /// </summary>
        public IEnumerable<IDummySmtpServerConnection> All
        {
            get
            {
                lock (_lock)
                {
                    return _connections.ToArray();
                }
            }
        }

        private readonly List<IDummySmtpServerConnection> _connections = new List<IDummySmtpServerConnection>();
        private readonly object _lock = new object();
        private readonly ManualResetEvent _emptyEvent = new ManualResetEvent(true);
    }
}

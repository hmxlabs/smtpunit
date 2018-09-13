using System.Collections.Generic;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// A mutable collection of <code>ITestSmtpServerConnection</code>s.
    /// 
    /// This is used internally by the <code>ITestSmtpServer</code> to manage
    /// its connected clients
    /// </summary>
    public interface IDummySmtpServerConnectionCollection
    {
        /// <summary>
        /// Add a new connection
        /// </summary>
        /// <param name="connection_"></param>
        void Add(IDummySmtpServerConnection connection_);

        /// <summary>
        /// Remove the specified connection
        /// </summary>
        /// <param name="connection_"></param>
        void Remove(IDummySmtpServerConnection connection_);

        /// <summary>
        /// Count of the number of connections
        /// </summary>
        int Count { get; }

        /// <summary>
        /// All connections within this collection as an enumeration
        /// </summary>
        IEnumerable<IDummySmtpServerConnection> All { get; }

        /// <summary>
        /// Calling this will block till such time that there are no more
        /// connection objects contained in this collection
        /// </summary>
        /// <returns></returns>
        bool WaitTillEmpty();

        /// <summary>
        /// As per <code>WillTilEmpty</code> however will timeout after the specified period
        /// 
        /// </summary>
        /// <param name="millisecondsToWait_">The number of milliseconds to wait before timing out</param>
        /// <returns></returns>
        bool WaitTillEmpty(int millisecondsToWait_);
    }
}
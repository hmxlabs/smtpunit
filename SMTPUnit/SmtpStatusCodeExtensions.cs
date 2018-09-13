using System.Globalization;
using System.Net.Mail;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// Utility class with an extension function to convert the <code>SMTPStatusCode</code> to a string
    /// representation.
    /// </summary>
    public static class SmtpStatusCodeExtensions
    {
        /// <summary>
        /// Extension function to convert <code>SmtpStatusCode</code> to a utility function
        /// </summary>
        /// <param name="retCode_">String representation of the status code</param>
        /// <returns></returns>
        public static string ToCodeString(this SmtpStatusCode retCode_)
        {
            return ((int) retCode_).ToString(CultureInfo.InvariantCulture);
        }
    }
}

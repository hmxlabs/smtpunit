namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// A class desined to provide a more fluent API when writing tests.
    /// Provides no real functionality but makes the test code more readable
    /// </summary>
    public static class Received
    {
        /// <summary>
        /// Constructs a new NUnit constraint and populates it with an expected count
        /// </summary>
        /// <param name="count_"></param>
        /// <returns></returns>
        public static SmtpConstraint Mail(int count_)
        {
            var constraint = new SmtpConstraint();
            constraint.Count(count_);
            return constraint;
        }
    }
}

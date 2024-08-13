using System;

namespace ODrive.NET
{
    /// <summary>
    /// Represents error that during communication with OneDrive
    /// </summary>
    public class ODException : Exception
    {
        /// <summary>
        /// Creates new instance of ODException
        /// </summary>
        /// <param name="message">Exception message</param>
        public ODException(string message) : base(message) { }
    }
}

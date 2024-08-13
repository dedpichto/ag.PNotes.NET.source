using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PNContacts
{
    /// <summary>
    /// Represents custom exception
    /// </summary>
    public class PNContactsException : Exception
    {
        internal PNContactsException(Exception ex)
            : base(ex.Message, ex)
        {

        }

        /// <summary>
        /// Gets additional exception information
        /// </summary>
        public string AdditionalInfo { get; internal set; }
    }
}

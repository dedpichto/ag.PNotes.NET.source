using System;

namespace Box.NET
{
    /// <summary>
    /// Fires when file or folder is not found on Box server
    /// </summary>
    public class BoxNotFoundException : Exception
    {
        internal BoxNotFoundException(Exception ex, string message) : base(message, ex)
        {
        }
    }

    /// <summary>
    /// Fires when file or folder already exists on Box server
    /// </summary>
    public class BoxAlreadyExistsException : Exception
    {
        internal BoxAlreadyExistsException(Exception ex, string message) : base(message, ex)
        {
        }
    }
}

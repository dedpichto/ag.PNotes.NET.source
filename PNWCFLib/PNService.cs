using System;
using System.ServiceModel;

namespace PNWCFLib
{
    /// <summary>
    /// Provides data for any error event
    /// </summary>
    public class PNDataErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Creates new instance of PNDataErrorEventArgs
        /// </summary>
        /// <param name="ex">Exception occured</param>
        public PNDataErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }

        /// <summary>
        /// Exception occured
        /// </summary>
        public Exception Exception { get; private set; }
    }

    /// <summary>
    /// Provides data for note receiving event
    /// </summary>
    public class PNDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates new instance of PNDataReceivedEventArgs
        /// </summary>
        /// <param name="data">Data received (usually the string representation of note)</param>
        public PNDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        /// <summary>
        /// Data received (usually the string representation of note)
        /// </summary>
        public string Data { get; private set; }
    }

    /// <summary>
    /// Represents the method that will handle an event of data receiving
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">Event data</param>
    public delegate void PNDataReceivedEventHandler(object sender, PNDataReceivedEventArgs e);

    /// <summary>
    /// Represents the method that will handle an event of data error
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">Event data</param>
    public delegate void PNDataErrorEventHandler(object sender, PNDataErrorEventArgs e);

    /// <summary>
    /// PNotes WCF service
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class PNService : IPNService
    {
        /// <summary>
        /// Raises when note is received
        /// </summary>
        public event PNDataReceivedEventHandler PNDataReceived;

        #region IPNService Members

        /// <summary>
        /// Receives string implementation of note
        /// </summary>
        /// <param name="message">String implementation of note</param>
        /// <returns>SUCCESS if note received successfully, exception message otherwise</returns>
        public string GetNote(string message)
        {
            try
            {
                if (PNDataReceived != null)
                {
                    PNDataReceived(this, new PNDataReceivedEventArgs(message));
                }
                return "SUCCESS";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }
}
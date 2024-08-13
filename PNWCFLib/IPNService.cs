using System.ServiceModel;

namespace PNWCFLib
{
    /// <summary>
    /// Main PNotes WCF service interface
    /// </summary>
    [ServiceContract]
    public interface IPNService
    {
        /// <summary>
        /// Receives string implementation of note
        /// </summary>
        /// <param name="message">String implementation of note</param>
        /// <returns>SUCCESS if note received successfully, exception message otherwise</returns>
        [OperationContract]
        string GetNote(string message);
    }
}

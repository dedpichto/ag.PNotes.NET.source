using System.ServiceModel;
using System.ServiceModel.Channels;
using PNWCFLib;

namespace PNServer
{
    class PNClientRunner
    {
        public event PNDataReceivedEventHandler PNDataReceived;
        public event PNDataErrorEventHandler PNDataError;

        public void RunClient(string message, string address, string port)
        {
            string endPointAddr = "net.tcp://" + address + ":" + port + "/PNService";
            NetTcpBinding tcpBinding = new NetTcpBinding {TransactionFlow = false};
            tcpBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcpBinding.Security.Mode = SecurityMode.None;

            EndpointAddress endPointAddress = new EndpointAddress(endPointAddr);

            IPNService proxy = ChannelFactory<IPNService>.CreateChannel(tcpBinding, endPointAddress);

            try
            {
                string result = proxy.GetNote(message);
                if (PNDataReceived != null)
                {
                    PNDataReceived(this, new PNDataReceivedEventArgs(result));
                }
            }
            catch (EndpointNotFoundException epex)
            {
                if (PNDataError != null)
                {
                    PNDataError(this, new PNDataErrorEventArgs(epex));
                }
            }
            finally
            {
                switch (((IChannel)proxy).State)
                {
                    case CommunicationState.Opened:
                        ((IChannel)proxy).Close();
                        break;
                    case CommunicationState.Faulted:
                        ((IChannel)proxy).Abort();
                        break;
                }
            }
        }
    }
}

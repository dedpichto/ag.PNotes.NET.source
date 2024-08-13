using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PNWCFLib;
using System.ServiceModel.Description;
using System.Net;

namespace PNServer
{
    class PNHostRunner
    {
        public event PNDataReceivedEventHandler PNDataReceived;

        private ServiceHost host = null;
        private string urlMeta, urlService = "";

        public void StartHost()
        {
            try
            {
                // Returns a list of ipaddress configuration
                IPHostEntry ips = Dns.GetHostEntry(Dns.GetHostName());
                // Select the first entry. I hope it's this maschines IP
                IPAddress _ipAddress = ips.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                // Create the url that is needed to specify
                // where the service should be started
                urlService = "net.tcp://" + _ipAddress.ToString() + ":27351/PNService";

                PNWCFLib.PNService service = new PNWCFLib.PNService();
                //service.PNDataReceived += new PNWCFLib.PNDataReceivedDelegate(service_PNDataReceived);
                host = new ServiceHost(service);

                //host = new ServiceHost(typeof(PNWCFLib.PNService));
                host.Opening += new EventHandler(host_Opening);
                host.Opened += new EventHandler(host_Opened);
                host.Closing += new EventHandler(host_Closing);
                host.Closed += new EventHandler(host_Closed);

                // The binding is where we can choose what
                // transport layer we want to use. HTTP, TCP ect.
                NetTcpBinding tcpBinding = new NetTcpBinding();
                tcpBinding.TransactionFlow = false;
                tcpBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcpBinding.Security.Mode = SecurityMode.None;


                // Add a endpoint
                host.AddServiceEndpoint(typeof(PNWCFLib.IPNService), tcpBinding, urlService);
                // A channel to describe the service.
                // Used with the proxy scvutil.exe tool
                //ServiceMetadataBehavior metadataBehavior;
                //metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                //if (metadataBehavior == null)
                //{
                //    // This is how I create the proxy object
                //    // that is generated via the svcutil.exe tool
                //    metadataBehavior = new ServiceMetadataBehavior();
                //    metadataBehavior.HttpGetUrl = new Uri("http://" + _ipAddress.ToString() + ":8001/PNService");
                //    metadataBehavior.HttpGetEnabled = true;
                //    //metadataBehavior.ToString();
                //    host.Description.Behaviors.Add(metadataBehavior);
                //    urlMeta = metadataBehavior.HttpGetUrl.ToString();
                //}
                host.Open();
                (host.SingletonInstance as PNWCFLib.PNService).PNDataReceived += new PNWCFLib.PNDataReceivedEventHandler(service_PNDataReceived);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public void StopHost()
        {
            if (host != null)
            {
                host.Close();
            }
        }

        void service_PNDataReceived(object sender, PNWCFLib.PNDataReceivedEventArgs e)
        {
            Append(e.Data);
        }

        void host_Closed(object sender, EventArgs e)
        {
            Append("Service closed");
        }

        void host_Closing(object sender, EventArgs e)
        {
            Append("Service closing ... stand by");
        }

        void host_Opened(object sender, EventArgs e)
        {
            Append("Service opened.");
            Append("Service URL:\t" + urlService);
            Append("Meta URL:\t" + urlMeta + " (Not that relevant)");
            Append("Waiting for clients...");
        }

        void host_Opening(object sender, EventArgs e)
        {
            Append("Service opening ... Stand by");
        }

        private void Append(string text)
        {
            if (PNDataReceived != null)
            {
                PNDataReceived(this, new PNDataReceivedEventArgs(text));
            }
        }
    }
}

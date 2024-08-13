// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using PNWCFLib;
using System;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.ServiceModel;
using PNCommon;

namespace PNotes.NET
{
    internal class PNWCFHostRunner
    {
        internal event PNDataReceivedEventHandler PNDataReceived;
        internal event PNDataErrorEventHandler PNDataError;

        private ServiceHost _Host;
        private string _UrlService = "";

        internal void StartHosting(string port)
        {
            try
            {
                //var ips = Dns.GetHostEntry(Dns.GetHostName());
                //var ipAddress = ips.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var ipAddress =
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);

                if (ipAddress == null)  //no network adapter
                    return;

                // Create the url that is needed to specify where the service should be started
                _UrlService = "net.tcp://" + ipAddress + ":" + port + "/" + PNServerConstants.PROG_SERVICE_NAME;

                var service = new PNService();
                _Host = new ServiceHost(service);

                //// For debug purpose only
                //_Host.Opening += host_Opening;
                //_Host.Opened += host_Opened;
                //_Host.Closing += host_Closing;
                //_Host.Closed += host_Closed;

                // The binding is where we can choose what transport layer we want to use. HTTP, TCP ect.
                var tcpBinding = new NetTcpBinding { TransactionFlow = false };
                tcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcpBinding.Security.Mode = SecurityMode.None;
                tcpBinding.ReceiveTimeout = TimeSpan.FromSeconds(600);
                tcpBinding.MaxReceivedMessageSize = 80000000;
                // Add a endpoint
                _Host.AddServiceEndpoint(typeof(IPNService), tcpBinding, _UrlService);

                _Host.Open();
                if (_Host.SingletonInstance is PNService pnService)
                    pnService.PNDataReceived += service_PNDataReceived;
            }
            catch (Exception ex)
            {
                PNDataError?.Invoke(this, new PNDataErrorEventArgs(ex));
                StopHosting();
            }
        }

        internal void StopHosting()
        {
            if (_Host != null)
            {
                switch (_Host.State)
                {
                    case CommunicationState.Opened:
                        _Host.Close();
                        break;
                    case CommunicationState.Faulted:
                        _Host.Abort();
                        break;
                }
            }
        }

        private void service_PNDataReceived(object sender, PNDataReceivedEventArgs e)
        {
            if (e.Data == PNServerConstants.CHECK)
                return;
            PNDataReceived?.Invoke(this, e);
        }

        //void host_Closed(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Service closed");
        //}

        //void host_Closing(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Service closing ... stand by");
        //}

        //void host_Opened(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Service opened.");
        //    System.Diagnostics.Debug.WriteLine("Service URL:\t" + _UrlService);
        //    //System.Diagnostics.Debug.WriteLine("Meta URL:\t" + urlMeta + " (Not that relevant)");
        //    System.Diagnostics.Debug.WriteLine("Waiting for clients...");
        //}

        //void host_Opening(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Service opening ... Stand by");
        //}
    }
}

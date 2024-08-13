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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Windows.Forms;
using PNCommon;
using PNEncryption;

namespace PNotes.NET
{
    internal class PNWCFClientRunner
    {
        internal event EventHandler<NotesSendNotificationEventArgs> NotesSendNotification;
        internal event EventHandler NotesSendComplete;

        private Tuple<NetTcpBinding, EndpointAddress> createEndPoint(string ipAddress, int port, string serviceName, TimeSpan sendTimeout)
        {
            try
            {
                var endPointAddr = "net.tcp://" + ipAddress + ":" + port + "/" + serviceName;
                var tcpBinding = new NetTcpBinding { TransactionFlow = false };
                tcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
                tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                tcpBinding.Security.Mode = SecurityMode.None;
                tcpBinding.SendTimeout = sendTimeout;

                var endPointAddress = new EndpointAddress(endPointAddr);

                return Tuple.Create(tcpBinding, endPointAddress);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private bool sendNotesToServer(string message, string ipServer, int portServer, string ipReceiver, int portReceiver)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(ipReceiver);
                sb.Append(PNServerConstants.DELIM);
                sb.Append(portReceiver);
                sb.Append(PNServerConstants.END_OF_RECEVIER);
                sb.Append(message);

                var pars = createEndPoint(ipServer, portServer, PNServerConstants.NET_SERVICE_NAME, TimeSpan.FromSeconds(PNRuntimes.Instance.Settings.Network.SendTimeout));
                if (pars?.Item1 == null || pars.Item2 == null)
                    return false;

                var proxy = ChannelFactory<IPNService>.CreateChannel(pars.Item1, pars.Item2);

                try
                {
                    string result;
                    try
                    {
                        result = proxy.GetNote(sb.ToString());
                    }
                    catch (EndpointNotFoundException epnfex)
                    {
                        PNStatic.LogException(epnfex, false);
                        return false;
                    }
                    return result == PNStrings.SUCCESS;
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex, false);
                    return false;
                }
                finally
                {
                    if (proxy != null)
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
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return false;
            }
        }

        private SendResult sendNotes(string ipAddress, string message, int port)
        {
            try
            {
                var sb = new StringBuilder();
                var hostName = Environment.MachineName;
                var pos = hostName.IndexOf(".", StringComparison.Ordinal);
                if (pos > -1)
                {
                    hostName = hostName.Substring(0, pos);
                }
                sb.Append(hostName);
                sb.Append(PNStrings.END_OF_ADDRESS);
                sb.Append(message);
                sb.Append(PNStrings.END_OF_FILE);

                var pars = createEndPoint(ipAddress, port, PNServerConstants.PROG_SERVICE_NAME, TimeSpan.FromSeconds(PNRuntimes.Instance.Settings.Network.SendTimeout));
                if (pars?.Item1 == null || pars.Item2 == null)
                {
                    throw new PNSendFailedException($"Failed creating end point. IP address: {ipAddress}, port: {port}");
                }


                var proxy = ChannelFactory<IPNService>.CreateChannel(pars.Item1, pars.Item2);

                if (proxy == null)
                {
                    throw new PNSendFailedException("Failed creating proxy channel.");
                }

                try
                {
                    string result;
                    try
                    {
                        result = proxy.GetNote(sb.ToString());
                    }
                    catch (EndpointNotFoundException)
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
                        proxy = null;
                        if (PNRuntimes.Instance.Settings.Network.StoreOnServer)
                        {
                            var sent = sendNotesToServer(sb.ToString(), PNRuntimes.Instance.Settings.Network.ServerIp,
                                PNRuntimes.Instance.Settings.Network.ServerPort, ipAddress, port);
                            if (sent)
                                return SendResult.SentToServer;
                            else
                                throw new PNSendFailedException(
                                    $"Failed to store note on server. Server IP: {PNRuntimes.Instance.Settings.Network.ServerIp}, Server port: {PNRuntimes.Instance.Settings.Network.ServerPort}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                    if (result == PNStrings.SUCCESS)
                        return SendResult.SentToContact;
                    switch (((IChannel)proxy).State)
                    {
                        case CommunicationState.Opened:
                            ((IChannel)proxy).Close();
                            break;
                        case CommunicationState.Faulted:
                            ((IChannel)proxy).Abort();
                            break;
                    }
                    proxy = null;
                    if (PNRuntimes.Instance.Settings.Network.StoreOnServer)
                    {
                        var sent = sendNotesToServer(sb.ToString(), PNRuntimes.Instance.Settings.Network.ServerIp,
                            PNRuntimes.Instance.Settings.Network.ServerPort, ipAddress, port);
                        if (sent)
                            return SendResult.SentToServer;
                        else
                            throw new PNSendFailedException(
                                $"Failed to store note on server. Server IP: {PNRuntimes.Instance.Settings.Network.ServerIp}, Server port: {PNRuntimes.Instance.Settings.Network.ServerPort}");
                    }
                    else
                    {
                        throw new PNSendFailedException(result);
                    }
                }
                catch (Exception ex)
                {
                    if (proxy != null)
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
                        proxy = null;
                    }
                    if (PNRuntimes.Instance.Settings.Network.StoreOnServer)
                    {
                        var sent = sendNotesToServer(sb.ToString(), PNRuntimes.Instance.Settings.Network.ServerIp,
                            PNRuntimes.Instance.Settings.Network.ServerPort, ipAddress, port);
                        if (sent)
                            return SendResult.SentToServer;
                        else
                            throw new PNSendFailedException(
                                $"Failed to store note on server. Server IP: {PNRuntimes.Instance.Settings.Network.ServerIp}, Server port: {PNRuntimes.Instance.Settings.Network.ServerPort}");
                    }
                    else
                    {
                        PNStatic.LogException(ex, false);
                        return SendResult.Failed;
                    }
                }
                finally
                {
                    if (proxy != null)
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
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return SendResult.Failed;
            }
        }

        internal void CheckMessages(string ipAddress, int port)
        {
            IPNService proxy = null;
            try
            {
                var ipRequest =
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);
                if (ipRequest == null) return;
                var sb = new StringBuilder(PNServerConstants.REQUEST_HEADER);
                sb.Append(PNServerConstants.DELIM);
                sb.Append(ipRequest);
                sb.Append(PNServerConstants.DELIM);
                sb.Append(PNRuntimes.Instance.Settings.Network.ExchangePort);

                var pars = createEndPoint(ipAddress, port, PNServerConstants.NET_SERVICE_NAME, TimeSpan.FromSeconds(PNRuntimes.Instance.Settings.Network.SendTimeout));
                if (pars?.Item1 == null || pars.Item2 == null)
                    return;

                proxy = ChannelFactory<IPNService>.CreateChannel(pars.Item1, pars.Item2);

                proxy.GetNote(sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (proxy != null)
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

        internal string CheckServer(string ipAddress, int port, string serviceName, int timeOut)
        {
            IPNService proxy = null;
            try
            {
                var pars = createEndPoint(ipAddress, port, serviceName, TimeSpan.FromSeconds(timeOut));
                if (pars?.Item1 == null || pars.Item2 == null)
                    return "";

                proxy = ChannelFactory<IPNService>.CreateChannel(pars.Item1, pars.Item2);

                var result = proxy.GetNote(PNServerConstants.CHECK);

                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return "";
            }
            finally
            {
                if (proxy != null)
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

        internal void SendNotesToMultipleContacts(IEnumerable<PNote> notes, IEnumerable<PNContact> contacts)
        {
            var pNotes = notes as PNote[] ?? notes.ToArray();
            var pContacts = contacts as List<PNContact> ?? contacts.ToList();
            var recContacts = new List<string>();
            var recServer = new List<string>();
            try
            {
                #region Check for network adapter existance

                var ipRequest =
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                    PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);
                if (ipRequest == null)
                {
                    NotesSendNotification?.Invoke(this,
                        new NotesSendNotificationEventArgs(pNotes, pContacts.Select(c => c.Name), SendResult.NoAdapter));
                    return;
                }

                #endregion

                #region Build message string

                var sb = new StringBuilder();
                foreach (var note in pNotes)
                {
                    if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            continue;
                    string text, tempPath = "";
                    var newNote = false;
                    var nc = new NoteConverter();

                    // decrypt note file to temp file if note is encrypted
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id);
                    path += PNStrings.NOTE_EXTENSION;

                    // save note first
                    if (note.Dialog != null && note.Changed)
                    {
                        if (note.FromDB)
                        {
                            if (PNRuntimes.Instance.Settings.Network.SaveBeforeSending)
                            {
                                note.Dialog.ApplySaveNote(false);
                            }
                        }
                        else
                        {
                            path = Path.Combine(Path.GetTempPath(), note.Id);
                            path += PNStrings.NOTE_EXTENSION;
                            note.Dialog.ApplySaveByPath(path);
                            newNote = true;
                        }
                    }

                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                        PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted && !newNote)
                    {
                        var fileName = Path.GetFileName(path);
                        if (string.IsNullOrEmpty(fileName))
                            continue;
                        tempPath = Path.Combine(Path.GetTempPath(), fileName);
                        File.Copy(path, tempPath, true);
                        using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        path = tempPath;
                    }
                    // read note file content
                    using (var sr = new StreamReader(path))
                    {
                        text = sr.ReadToEnd();
                    }
                    // remove temp file
                    if (tempPath != "")
                    {
                        File.Delete(tempPath);
                    }
                    //remove temporary file created for new note
                    if (newNote)
                    {
                        File.Delete(path);
                    }
                    sb.Append(text);
                    sb.Append(PNStrings.END_OF_TEXT);
                    sb.Append(nc.ConvertToString(note));
                    sb.Append(PNStrings.END_OF_NOTE);

                    if (!newNote && note.Dialog != null && PNRuntimes.Instance.Settings.Network.HideAfterSending)
                    {
                        note.Dialog.ApplyHideNote(note);
                    }
                }
                if (sb.Length <= 0)
                {
                    NotesSendNotification?.Invoke(this,
                        new NotesSendNotificationEventArgs(pNotes, pContacts.Select(c => c.Name), SendResult.Failed));
                    return;
                }

                #endregion

                #region Define ip address to send notes to

                var addresses = new List<string>();

                for (var i = pContacts.Count - 1; i >= 0; i--)
                {
                    if (!pContacts[i].UseComputerName || !string.IsNullOrEmpty(pContacts[i].IpAddress))
                    {
                        addresses.Add(pContacts[i].IpAddress);
                    }
                    else
                    {
                        IPHostEntry ipHostInfo;
                        try
                        {
                            ipHostInfo = Dns.GetHostEntry(pContacts[i].ComputerName);
                        }
                        catch (SocketException)
                        {
                            NotesSendNotification?.Invoke(this,
                                new NotesSendNotificationEventArgs(pNotes, new[] { pContacts[i].Name },
                                    SendResult.CompNameNoteFound));
                            pContacts.RemoveAt(i);
                            continue;
                        }

                        if (ipHostInfo == null)
                        {
                            NotesSendNotification?.Invoke(this,
                                new NotesSendNotificationEventArgs(pNotes, new[] { pContacts[i].Name },
                                    SendResult.CompNameNoteFound));
                            pContacts.RemoveAt(i);
                            continue;
                        }
                        var address =
                            ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                        if (address == null)
                        {
                            NotesSendNotification?.Invoke(this,
                                new NotesSendNotificationEventArgs(pNotes, new[] { pContacts[i].Name },
                                    SendResult.CompAddressNotFound));
                            pContacts.RemoveAt(i);
                            continue;
                        }
                        addresses.Add(address.ToString());
                    }
                }
                addresses.Reverse();

                #endregion

                #region Check for recipient's computer existance

                if (!PNRuntimes.Instance.Settings.Network.StoreOnServer)
                {
                    for (var i = addresses.Count - 1; i >= 0; i--)
                    {
                        if (PNConnections.CheckContactConnection(addresses[i]) != ContactConnection.Disconnected)
                            continue;
                        if (NotesSendNotification != null)
                        {
                            NotesSendNotification(this,
                                new NotesSendNotificationEventArgs(pNotes, new[] { pContacts[i].Name },
                                    SendResult.CompNotOnNetwork));
                            addresses.RemoveAt(i);
                            pContacts.RemoveAt(i);
                        }
                    }
                }

                #endregion

                for (var i = 0; i < addresses.Count; i++)
                {
                    var result = sendNotes(addresses[i], sb.ToString(),
                        PNRuntimes.Instance.Settings.Network.ExchangePort);
                    switch (result)
                    {
                        case SendResult.SentToContact:
                            recContacts.Add(pContacts[i].Name);
                            break;
                        case SendResult.SentToServer:
                            recServer.Add(pContacts[i].Name);
                            break;
                    }
                }
                if (NotesSendNotification != null)
                {
                    if (recContacts.Any())
                        NotesSendNotification(this,
                            new NotesSendNotificationEventArgs(pNotes, recContacts, SendResult.SentToContact));
                    if (recServer.Any())
                        NotesSendNotification(this,
                            new NotesSendNotificationEventArgs(pNotes, recServer, SendResult.SentToServer));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                NotesSendNotification?.Invoke(this,
                    new NotesSendNotificationEventArgs(pNotes, pContacts.Select(c => c.Name), SendResult.Failed));
            }
            finally
            {
                NotesSendComplete?.Invoke(this, new EventArgs());
            }
        }

        internal void SendNotesToSingleContact(IEnumerable<PNote> notes, PNContact cn)
        {
            var pNotes = notes as PNote[] ?? notes.ToArray();
            try
            {
                #region Check for network adapter existance
                var ipRequest =
                            PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                            PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);
                if (ipRequest == null)
                {
                    NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.NoAdapter));
                    return;
                }
                #endregion

                #region Build message string
                var sb = new StringBuilder();
                foreach (var note in pNotes)
                {
                    if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            continue;
                    string text, tempPath = "";
                    var newNote = false;
                    var nc = new NoteConverter();

                    // decrypt note file to temp file if note is encrypted
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id);
                    path += PNStrings.NOTE_EXTENSION;

                    // save note first
                    if (note.Dialog != null && note.Changed)
                    {
                        if (note.FromDB)
                        {
                            if (PNRuntimes.Instance.Settings.Network.SaveBeforeSending)
                            {
                                note.Dialog.ApplySaveNote(false);
                            }
                        }
                        else
                        {
                            path = Path.Combine(Path.GetTempPath(), note.Id);
                            path += PNStrings.NOTE_EXTENSION;
                            note.Dialog.ApplySaveByPath(path);
                            newNote = true;
                        }
                    }

                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                        PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted && !newNote)
                    {
                        var fileName = Path.GetFileName(path);
                        if (string.IsNullOrEmpty(fileName))
                            continue;
                        tempPath = Path.Combine(Path.GetTempPath(), fileName);
                        File.Copy(path, tempPath, true);
                        using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                        {
                            pne.DecryptTextFile(tempPath);
                        }
                        path = tempPath;
                    }
                    // read note file content
                    using (var sr = new StreamReader(path))
                    {
                        text = sr.ReadToEnd();
                    }
                    // remove temp file
                    if (tempPath != "")
                    {
                        File.Delete(tempPath);
                    }
                    //remove temporary file created for new note
                    if (newNote)
                    {
                        File.Delete(path);
                    }
                    sb.Append(text);
                    sb.Append(PNStrings.END_OF_TEXT);
                    sb.Append(nc.ConvertToString(note));
                    sb.Append(PNStrings.END_OF_NOTE);
                    if (!newNote && note.Dialog != null && PNRuntimes.Instance.Settings.Network.HideAfterSending)
                    {
                        note.Dialog.ApplyHideNote(note);
                    }
                }
                if (sb.Length <= 0)
                {
                    NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.Failed));
                    return;
                }
                #endregion

                #region Define ip address to send notes to
                string ipAddress;
                if (!cn.UseComputerName || !string.IsNullOrEmpty(cn.IpAddress))
                {
                    ipAddress = cn.IpAddress;
                }
                else
                {
                    IPHostEntry ipHostInfo;
                    try
                    {
                        ipHostInfo = Dns.GetHostEntry(cn.ComputerName);
                    }
                    catch (SocketException)
                    {
                        NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.CompNameNoteFound));
                        return;
                    }

                    var address =
                        ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (address == null)
                    {
                        NotesSendNotification?.Invoke(this,
                            new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.CompAddressNotFound));
                        return;
                    }
                    ipAddress = address.ToString();
                }
                #endregion

                #region Check for recipient's computer existance
                if (!PNRuntimes.Instance.Settings.Network.StoreOnServer)
                {
                    if (PNConnections.CheckContactConnection(ipAddress) == ContactConnection.Disconnected)
                    {
                        NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.CompNotOnNetwork));
                        return;
                    }
                }
                #endregion

                var result = sendNotes(ipAddress, sb.ToString(),
                        PNRuntimes.Instance.Settings.Network.ExchangePort);
                NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, result));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                NotesSendNotification?.Invoke(this, new NotesSendNotificationEventArgs(pNotes, new[] { cn.Name }, SendResult.Failed));
            }
            finally
            {
                NotesSendComplete?.Invoke(this, new EventArgs());
            }
        }
    }
}

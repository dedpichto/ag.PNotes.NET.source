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

using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace VKontakte.NET
{
    /// <summary>
    /// Represents VKontakte authentication dialog
    /// </summary>
    public partial class DlgAuth : Form
    {
        internal event AccessTokenReceivedEventHandler AccessTokenReceived;
        /// <summary>
        /// Creates new instance of DlgAuth
        /// </summary>
        /// <param name="url">Authentication URL</param>
        public DlgAuth(string url)
        {
            InitializeComponent();
            wbAuth.Navigate(url);
        }

        private void wbAuth_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString().StartsWith("https://oauth.vk.com/blank.html#access_token="))
            {
                string queryParams = e.Url.Query;
                if (queryParams.Length > 1)
                {
                    NameValueCollection qs = Utils.ParseResponse(queryParams);
                    if (qs["access_token"] != null)
                    {
                        if (AccessTokenReceived != null)
                        {
                            AccessTokenReceived(this,
                                                new AccessTokenReceivedEventArgs(qs["access_token"],
                                                                                 qs["expires_in"] ?? ""));
                            DialogResult = DialogResult.OK;
                        }
                    }
                }
            }
        }

        private void wbAuth_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.ToString().StartsWith("https://oauth.vk.com/blank.html#access_token="))
            {
                string queryParams = e.Url.Fragment;
                if (queryParams.Length > 1)
                {
                    NameValueCollection qs = Utils.ParseResponse(queryParams);
                    if (qs["access_token"] != null)
                    {
                        if (AccessTokenReceived != null)
                        {
                            AccessTokenReceived(this,
                                                new AccessTokenReceivedEventArgs(qs["access_token"],
                                                                                 qs["expires_in"] ?? ""));
                            DialogResult = DialogResult.OK;
                        }
                    }
                }
            }
        }
    }

    internal delegate void AccessTokenReceivedEventHandler(object sender, AccessTokenReceivedEventArgs e);
    internal class AccessTokenReceivedEventArgs : EventArgs
    {
        internal string AccessToken { get; }
        internal string ExpiresIn { get; }

        internal AccessTokenReceivedEventArgs(string token, string expires)
        {
            AccessToken = token;
            ExpiresIn = expires;
        }
    }
}

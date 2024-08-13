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
using System.Windows.Navigation;

namespace LinkedIn.NET
{
    /// <summary>
    /// Interaction logic for LoginModalWindow.xaml
    /// Based on David Quail's code published here: http://developer.linkedin.com/thread/1346
    /// </summary>
    public partial class AuthorizeWindow
    {
        private readonly string _State;

        /// <summary>
        /// LinkedIn authentication code
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Creates new instance of AuthorizeWindow
        /// </summary>
        /// <param name="authLink">Authentication link</param>
        public AuthorizeWindow(string authLink)
        {
            InitializeComponent();
            NameValueCollection qs = Utils.ParseResponse(authLink);
            if (qs["state"] != null)
            {
                _State = qs["state"];
            }
            browser.Navigate(new Uri(authLink));            
        }

        private void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(Utils.CALLBACK)) {
                string queryParams = e.Uri.Query;
                if (queryParams.Length > 1)
                {
                    //Store the authentication code
                    NameValueCollection qs = Utils.ParseResponse(queryParams);
                    if (qs["state"] != null)
                    {
                        if (qs["state"] == _State)
                        {
                            if (qs["code"] != null)
                            {
                                Code = qs["code"];
                            }
                        }
                    }
                }
                Close();
            }            
        }
    }
}

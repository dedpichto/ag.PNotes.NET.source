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

#region Usings

using System;
using System.Dynamic;
using System.Windows.Forms;
using Facebook;

#endregion

namespace pnfacebook
{
    public partial class DlgAuthentication : Form
    {
        internal event EventHandler<FBAuthenticationCompleteEventArgs> FBAuthenticationComplete;

        private readonly Uri _loginUrl;
        protected readonly FacebookClient FBClient;

        internal DlgAuthentication(string appId, string extendedPermissions)
        {
            FBClient = new FacebookClient();
            _loginUrl = GenerateLoginUrl(appId, extendedPermissions);
            InitializeComponent();
        }

        private void wbAuth_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // whenever the browser navigates to a new url, try parsing the url.
            // the url may be the result of OAuth 2.0 authentication.

            if (!FBClient.TryParseOAuthCallbackUrl(e.Url, out var oauthResult)) return;
            // The url is the result of OAuth 2.0 authentication
            if (oauthResult.IsSuccess)
            {
                FBAuthenticationComplete?.Invoke(this, new FBAuthenticationCompleteEventArgs(oauthResult));
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(oauthResult.ErrorReason);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void DlgAuthentication_Load(object sender, EventArgs e)
        {
            // make sure to use AbsoluteUri.
            wbAuth.Navigate(_loginUrl.AbsoluteUri);
        }

        private Uri GenerateLoginUrl(string appId, string extendedPermissions)
        {
            // for .net 3.5
            // var parameters = new Dictionary<string,object>
            // parameters["client_id"] = appId;
            dynamic parameters = new ExpandoObject();
            parameters.client_id = appId;
            parameters.redirect_uri = "https://www.facebook.com/connect/login_success.html";

            // The requested response: an access token (token), an authorization code (code), or both (code token).
            parameters.response_type = "token";

            // list of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
            parameters.display = "popup";

            // add the 'scope' parameter only if we have extendedPermissions.
            if (!string.IsNullOrWhiteSpace(extendedPermissions))
                parameters.scope = extendedPermissions;

            // when the Form is loaded navigate to the login url.
            return FBClient.GetLoginUrl(parameters);
        }
    }

    internal class FBAuthenticationCompleteEventArgs : EventArgs
    {
        internal FBAuthenticationCompleteEventArgs(FacebookOAuthResult result)
        {
            Result = result;
        }

        internal FacebookOAuthResult Result { get; }
    }
}
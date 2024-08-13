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
using System.Drawing;
using System.Windows.Forms;
using PNCommon;
using TweetSharp;

#endregion

namespace pntwitter
{
    public partial class DlgAuthentication : Form
    {
        internal event EventHandler<TwitterServiceReceivedEventArgs> TwitterServiceReceived;

        internal DlgAuthentication(string consumerKey, string consumerSecret)
        {
            InitializeComponent();
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            wbAuth.StatusTextChanged += wbAuth_StatusTextChanged;
        }

        private int _Step;
        private readonly string _consumerKey = "";
        private readonly string _consumerSecret = "";
        private TwitterService service;
        private OAuthRequestToken requestToken;

        private void wbAuth_StatusTextChanged(object sender, EventArgs e)
        {
            sslStatus.Text = wbAuth.StatusText;
        }

        private void cmdContinue_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_Step)
                {
                    case 0:
                        // Pass your credentials to the service
                        service = new TwitterService(_consumerKey, _consumerSecret);

                        // Step 1 - Retrieve an OAuth Request Token
                        requestToken = service.GetRequestToken();

                        // Step 2 - Redirect to the OAuth Authorization URL
                        Uri uri = service.GetAuthorizationUri(requestToken);
                        wbAuth.Navigate(uri);
                        break;
                    case 2:
                        if (txtPin.Text.Trim().Length > 0)
                        {
                            // Step 3 - Exchange the Request Token for an Access Token
                            string verifier = txtPin.Text.Trim(); // <-- This is input into your application by your user
                            OAuthAccessToken access = service.GetAccessToken(requestToken, verifier);

                            // Step 4 - User authenticates using the Access Token
                            service.AuthenticateWith(access.Token, access.TokenSecret);
                            if (TwitterServiceReceived != null)
                            {
                                TwitterServiceReceived(this, new TwitterServiceReceivedEventArgs(access));
                                //return OK
                                DialogResult = DialogResult.OK;
                            }
                        }
                        else
                        {
                            MessageBox.Show(Utils.GetString("enter_pin", "Please, enter PIN CODE"));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void wbAuth_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                switch (_Step)
                {
                    case 0:
                        label1.ForeColor = SystemColors.WindowText;
                        label1.Font = new Font(label1.Font, FontStyle.Strikeout);
                        label2.Visible = true;
                        break;
                    case 1:
                        label2.ForeColor = SystemColors.WindowText;
                        label2.Font = new Font(label2.Font, FontStyle.Strikeout);
                        label3.Visible = true;
                        txtPin.Enabled = lblPin.Enabled = true;
                        break;
                    case 2:
                        label3.ForeColor = SystemColors.WindowText;
                        label3.Font = new Font(label3.Font, FontStyle.Strikeout);
                        break;
                }
                _Step++;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void DlgAuthentication_Load(object sender, EventArgs e)
        {
            try
            {
                label1.Text = Utils.GetString("label1", "1.   Press \"Continue\" button to navigate to Twitter page");
                label2.Text = Utils.GetString("label2",
                                               "2.   Fill in all required fields and authorize PNotes (pressing on \"Authorize\" button)");
                label3.Text = Utils.GetString("label3",
                                               "3.   Copy and paste PIN CODE into appropriate text box and press \"Continue\" again. You are done!");
                lblPin.Text = Utils.GetString("lblPin", "Enter PIN CODE here:");
                cmdContinue.Text = Utils.GetString("cmdContinue", "Continue");
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }

    internal class TwitterServiceReceivedEventArgs : EventArgs
    {
        internal TwitterServiceReceivedEventArgs(OAuthAccessToken tokens)
        {
            //Service = service;
            Tokens = tokens;
        }

        //internal TwitterService Service { get; private set; }
        internal OAuthAccessToken Tokens { get; private set; }
    }
}
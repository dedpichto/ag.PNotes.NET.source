using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;

namespace PNContacts
{
    internal partial class DlgAuth : Form
    {
        internal DlgAuth(string authUrl)
        {
            InitializeComponent();
            _AuthUrl = authUrl;
        }

        private readonly string _AuthUrl;

        public string AuthCode { get; private set; }

        private void DlgAuth_Load(object sender, EventArgs e)
        {
            wbAuth.Navigate(_AuthUrl);
        }

        private void wbAuth_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (!e.Url.ToString().Contains(Constants.SHOER_REDIRECT_URL)) return;
            var queryParams = e.Url.Query;
            if (queryParams.Length <= 1) return;
            //Store the authentication code
            var qs = ParseResponse(queryParams);
            if (qs["state"] == null) return;
            if (qs["state"] != Constants.AUTH_STATE) return;
            if (qs["code"] == null) return;
            AuthCode = qs["code"];
            DialogResult = DialogResult.OK;
        }

        private static NameValueCollection ParseResponse(string response)
        {
            var nvc = new NameValueCollection();
            if (response.StartsWith("?")) response = response.Substring(1);
            var arr1 = response.Split('&');
            foreach (var arr2 in arr1.Select(s => s.Split('=')).Where(arr2 => arr2.Length == 2))
            {
                nvc.Add(arr2[0].Trim(), arr2[1].Trim());
            }
            return nvc;
        }
    }
}

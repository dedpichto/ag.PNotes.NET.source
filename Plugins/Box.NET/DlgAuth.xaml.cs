using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Box.NET
{
    /// <summary>
    /// Interaction logic for DlgAuth.xaml
    /// </summary>
    public partial class DlgAuth
    {
        internal DlgAuth(string authLink)
        {
            InitializeComponent();
            var qs = Utils.ParseResponse(authLink);
            if (qs["state"] != null)
            {
                _State = qs["state"];
            }
            wbAuth.Navigate(new Uri(authLink)); 
        }

        private readonly string _State;

        internal string Code { get; private set; }

        private void wbAuth_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(Utils.CALLBACK)) return;
            var queryParams = e.Uri.Query;
            if (queryParams.Length > 1)
            {
                //Store the authentication code
                var qs = Utils.ParseResponse(queryParams);
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

        private static void setSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException(nameof(browser));

            // get an IWebBrowser2 from the document
            if (!(browser.Document is IOleServiceProvider sp)) return;
            var iidIWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
            var iidIWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

            sp.QueryService(ref iidIWebBrowserApp, ref iidIWebBrowser2, out var webBrowser);
            webBrowser?.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private void wbAuth_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            setSilent(wbAuth, true);
        }
    }
}

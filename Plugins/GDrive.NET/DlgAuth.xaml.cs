using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace GDrive.NET
{
    /// <summary>
    /// Interaction logic for DlgAuth.xaml
    /// </summary>
    public partial class DlgAuth
    {
        internal DlgAuth(string url)
        {
            InitializeComponent();
            _Url = url;
        }

        internal string Code { get; private set; }

        private readonly string _Url;

        private void wbAuth_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().StartsWith(GDStatic.REDIRECT_URL)) return;
            var queryParams = e.Uri.Query;
            if (queryParams.Length > 1)
            {
                //Store the authentication code
                var qs = ParseResponse(queryParams);
                if (qs["state"] != null)
                {
                    if (qs["state"] == GDStatic.AUTH_STATE)
                    {
                        if (qs["code"] != null)
                        {
                            Code = qs["code"];
                            DialogResult = true;
                        }
                    }
                }
            }
        }

        private void wbAuth_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            setSilent(wbAuth, true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wbAuth.Navigate(_Url);
        }

        private static void setSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            var sp = browser.Document as IOleServiceProvider;
            if (sp == null) return;
            var iidIWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
            var iidIWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

            object webBrowser;
            sp.QueryService(ref iidIWebBrowserApp, ref iidIWebBrowser2, out webBrowser);
            if (webBrowser != null)
            {
                webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
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

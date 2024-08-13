#region Usings

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Twitterizer;

#endregion

namespace pntwitter
{
    /// <summary>
    ///     Static class for holding shared variables and performing common tasks
    /// </summary>
    internal static class TWAuth
    {
        internal static OAuthTokens TwitTokens = null;
        internal static XDocument XLang = null;
        internal static string XCulture = "en-US";

        /// <summary>
        ///     Gets string from localizations file
        /// </summary>
        /// <param name="element">Element name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>String value of specified element or default value, if the element cannot be found or any error occures</returns>
        internal static string GetString(string element, string defaultValue)
        {
            try
            {
                string result = defaultValue;
                if (XLang != null)
                {
                    if (XLang.Root != null)
                    {
                        XElement xe =
                            XLang.Root.Elements("lang").FirstOrDefault(e => e.Attribute("culture").Value == XCulture);
                        if (xe != null)
                        {
                            var xElement = xe.Element(element);
                            if (xElement != null) result = xElement.Value;
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        ///     Returns current directory of plugin DLL
        /// </summary>
        internal static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        ///     Logs exception
        /// </summary>
        /// <param name="ex">Exception to log</param>
        internal static void LogException(Exception ex)
        {
            try
            {
                Type type = ex.GetType();
                using (var w = new StreamWriter(Path.Combine(AssemblyDirectory, "pntwitter.log"), true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.Append(" ");
                    sb.Append(type);
                    sb.Append(" ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append(ex.StackTrace);
                    w.WriteLine(sb.ToString());
                }
                MessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }

    /// <summary>
    ///     Handler for AccessTokenReceived event
    /// </summary>
    /// <param name="sender">Event raiser</param>
    /// <param name="e">AccessTokenReceivedEventArgs</param>
    internal delegate void AccessTokenReceivedEventHandler(object sender, AccessTokenReceivedEventArgs e);

    /// <summary>
    ///     Provides data for AccessTokenReceived event
    /// </summary>
    internal class AccessTokenReceivedEventArgs : EventArgs
    {
        private readonly string _Token = "";
        private readonly string _TokenSecret = "";

        /// <summary>
        ///     Creates new instance of AccessTokenReceivedEventArgs
        /// </summary>
        /// <param name="token">Access token</param>
        /// <param name="tokenSecret">Access token secret</param>
        internal AccessTokenReceivedEventArgs(string token, string tokenSecret)
        {
            _Token = token;
            _TokenSecret = tokenSecret;
        }

        /// <summary>
        ///     Access token
        /// </summary>
        internal string TokenSecret
        {
            get { return _TokenSecret; }
        }

        /// <summary>
        ///     Access token secret
        /// </summary>
        internal string Token
        {
            get { return _Token; }
        }
    }
}
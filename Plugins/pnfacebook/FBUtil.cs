#region Usings

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Facebook;

#endregion

namespace pnfacebook
{
    internal static class FBUtil
    {
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
                var type = ex.GetType();
                using (var w = new StreamWriter(Path.Combine(AssemblyDirectory, "pnfacebook.log"), true))
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

    internal delegate void FBAuthenticationCompleteEventHandler(object sender, FBAuthenticationCompleteEventArgs e);

    internal class FBAuthenticationCompleteEventArgs : EventArgs
    {
        private readonly FacebookOAuthResult _Result;

        internal FBAuthenticationCompleteEventArgs(FacebookOAuthResult result)
        {
            _Result = result;
        }

        internal FacebookOAuthResult Result
        {
            get { return _Result; }
        }
    }
}
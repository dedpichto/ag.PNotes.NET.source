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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PNCommon
{
    internal class Utils
    {
        internal static XDocument XLang = null;
        internal static string XCulture = "en-US";

        /// <summary>
        /// Gets string from localizations file
        /// </summary>
        /// <param name="element">Element name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>String value of specified element or default value, if the element cannot be found or any error occures</returns>
        internal static string GetString(string element, string defaultValue)
        {
            try
            {
                var result = defaultValue;
                if (XLang != null)
                {
                    if (XLang.Root != null)
                    {
                        var xe =
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
        /// Returns current directory of plugin DLL
        /// </summary>
        internal static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string productName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            return customAttributes.Length > 0 ? ((AssemblyProductAttribute)customAttributes[0]).Product : "plugin";
        }

        /// <summary>
        /// Logs exception
        /// </summary>
        /// <param name="ex">Exception to log</param>
        /// <param name="showMessage">Flag specifies whether to show message box</param>
        internal static void LogException(Exception ex, bool showMessage = false)
        {
            try
            {
                var type = ex.GetType();
                using (var w = new StreamWriter(Path.Combine(AssemblyDirectory, productName() + ".log"), true))
                {
                    var stack = new StackTrace(ex, true);
                    var frame = stack.GetFrame(stack.FrameCount - 1);

                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US")));
                    sb.AppendLine();
                    sb.Append("Type: ");
                    sb.Append(type);
                    sb.AppendLine();
                    sb.Append("Message: ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append("In: ");
                    sb.Append(frame.GetFileName());
                    sb.Append("; at: ");
                    sb.Append(frame.GetMethod().Name);
                    var line = frame.GetFileLineNumber();
                    var column = frame.GetFileColumnNumber();
                    if (line != 0 || column != 0)
                    {
                        sb.Append("; line: ");
                        sb.Append(line);
                        sb.Append("; column: ");
                        sb.Append(column);
                    }
                    else
                    {
                        sb.Append("; line: undefined; column: undefined");
                    }
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                }
                if (showMessage)
                    MessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}

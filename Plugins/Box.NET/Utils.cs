using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Box.NET
{
    internal static class Utils
    {
        internal const string CALLBACK = "https://box.net/done";

        internal static NameValueCollection ParseResponse(string response)
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

        internal static string EscapeXml(string source)
        {
            string target = source;
            target = target.Replace("&", "&amp;");
            target = target.Replace("\"", "&quot;");
            target = target.Replace("<", "&lt;");
            target = target.Replace(">", "&gt;");
            return target;
        }
    }
}

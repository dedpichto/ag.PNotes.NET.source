using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Box.NET
{
    internal static class BoxStatic
    {
        internal const string STATE = "WGOTMAF67391kwyamd";
        internal const string AUTHORIZATION_URL = "https://www.box.com/api/oauth2/authorize?response_type=code&client_id=";
        internal const string ACCESS_TOKEN_URL = "https://www.box.com/api/oauth2/token";
        internal const string GET_FILE_CONTENT_URL = "https://api.box.com/2.0/files/FILE_ID/content";
        internal const string GET_FOLDER_CONTENT_URL = "https://api.box.com/2.0/folders/FOLDER_ID/items?limit=1000&offset=0&fields=name";
        internal const string GET_FOLDER_FULL_URL = "https://api.box.com/2.0/folders/FOLDER_ID";
        internal const string GET_FILE_FULL_URL = "https://api.box.com/2.0/files/FILE_ID";
        internal const string FOLDERS_URL = "https://api.box.com/2.0/folders";
        internal const string UPLOAD_URL = "https://upload.box.com/api/2.0/files/content";
        internal const string REPLACE_URL = "https://upload.box.com/api/2.0/files/FILE_ID/content";

        private static string webResponseGetString(HttpWebRequest webRequest)
        {
            var responseData = "";

            using (var stream = webRequest.GetResponse().GetResponseStream())
            {
                if (stream != null)
                {
                    using (var responseReader = new StreamReader(stream))
                    {
                        responseData = responseReader.ReadToEnd();
                    }
                }
            }


            return responseData;
        }

        private static byte[] webResponseGetBytes(HttpWebRequest webRequest)
        {
            using (var stream = webRequest.GetResponse().GetResponseStream())
            {
                if (stream != null)
                {
                    var list = new List<byte>();
                    int count;
                    do
                    {
                        var buff = new byte[1024];
                        count = stream.Read(buff, 0, 1024);
                        list.AddRange(buff.Take(count));
                    } while (stream.CanRead && count > 0);
                    return list.ToArray();
                }
            }
            return new byte[0];
        }

        internal static byte[] makeRequestBytes(HttpWebRequest webRequest)
        {
            return webRequest != null ? webResponseGetBytes(webRequest) : new byte[0];
        }

        internal static string makeRequestString(HttpWebRequest webRequest)
        {
            return webRequest != null ? webResponseGetString(webRequest) : "";
        }

        internal static HttpWebRequest createRequest(string url, string method, string postData = null,
                                             IEnumerable<string> httpHeaders = null)
        {
            var webRequest = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
                if (httpHeaders != null)
                {
                    foreach (var h in httpHeaders)
                    {
                        webRequest.Headers.Add(h);
                    }
                }
                webRequest.Method = method;
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.PreAuthenticate = true;
                webRequest.ServicePoint.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (postData != null)
                {
                    var fileToSend = Encoding.UTF8.GetBytes(postData);
                    webRequest.ContentLength = fileToSend.Length;

                    using (Stream reqStream = webRequest.GetRequestStream())
                    {
                        reqStream.Write(fileToSend, 0, fileToSend.Length);
                    }
                }
            }
            return webRequest;
        }
    }
}

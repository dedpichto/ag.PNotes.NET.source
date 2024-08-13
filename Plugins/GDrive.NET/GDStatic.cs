using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace GDrive.NET
{
    internal class GDStatic
    {
        internal const string AUTHORIZATION_URL =
            "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id=CLIENT_ID&redirect_uri=REDIRECT_URL&scope=https://www.googleapis.com/auth/drive&state=AUTH_STATE&access_type=offline&approval_prompt=force";
        internal const string AUTH_STATE = "WURN4G4M";
        internal const string REDIRECT_URL = "http://www.pnotes.sf.net/auth.htm";// "https://gdconnect/success";
        internal const string ACCESS_TOKEN_URL = "https://accounts.google.com/o/oauth2/token";
        internal const string FILES_URL = "https://www.googleapis.com/drive/v2/files";
        internal const string UPLOAD_URL = "https://www.googleapis.com/upload/drive/v2/files";

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

        internal static HttpWebRequest createRequest(string url, string method, object postData = null,
                                             IEnumerable<string> httpHeaders = null, string contentType = "application/x-www-form-urlencoded")
        {
            var webRequest = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.ContentType = contentType;
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
                    switch (postData)
                    {
                        case string data:
                            var fileToSend = Encoding.UTF8.GetBytes(data);
                            webRequest.ContentLength = fileToSend.Length;

                            using (var reqStream = webRequest.GetRequestStream())
                            {
                                reqStream.Write(fileToSend, 0, fileToSend.Length);
                            }

                            break;
                        case byte[] bytes:
                            webRequest.ContentLength = bytes.Length;

                            using (var reqStream = webRequest.GetRequestStream())
                            {
                                reqStream.Write(bytes, 0, bytes.Length);
                            }

                            break;
                    }
                }
            }
            if (webRequest != null && webRequest.ContentLength == -1) webRequest.ContentLength = 0;
            return webRequest;
        }
    }
}

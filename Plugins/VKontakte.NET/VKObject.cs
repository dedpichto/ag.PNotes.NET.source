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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace VKontakte.NET
{
    /// <summary>
    /// Represents object for authentication, posting and getting messages via VKontakte API
    /// </summary>
    public class VKObject
    {
        private const string GET_MESSAGES_AS_XML = "https://api.vk.com/method/wall.get.xml?v=5.73&count=";
        private const string GET_MESSAGES_AS_JSON = "https://api.vk.com/method/wall.get?v=5.73&count=";
        private const string POST_AS_XML = "https://api.vk.com/method/wall.post.xml?v=5.73&message=";
        private const string AUTHORIZE_URL = "https://oauth.vk.com/authorize?client_id=";
        private const string AUTHORIZE_PARAMETERS = "&scope=wall&redirect_uri=http://oauth.vk.com/blank.html&display=page&response_type=token";

        /// <summary>
        /// Creates new instance of VKObject
        /// </summary>
        /// <param name="clientId">Client ID of calling application</param>
        public VKObject(string clientId)
        {
            _ClientId = clientId;
        }
        /// <summary>
        /// Creates new instance of VKObject
        /// </summary>
        /// <param name="clientId">Client ID of calling application</param>
        /// <param name="accessToken">Access token</param>
        public VKObject(string clientId, string accessToken)
        {
            _ClientId = clientId;
            AccessToken = accessToken;
        }

        private readonly string _ClientId;
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; private set; }
        /// <summary>
        /// Access token expiration
        /// </summary>
        public string ExpiresIn { get; private set; }
        /// <summary>
        /// Authenticates client on VK server
        /// </summary>
        /// <returns>True if authentication passed successfully, false otherwise</returns>
        public bool Authenticate()
        {
            if (string.IsNullOrEmpty(_ClientId)) throw new Exception("Client ID is not set");
            return getAccessToken();
        }
        /// <summary>
        /// Get messages from user's wall
        /// </summary>
        /// <param name="count">Count of messages to retrieve</param>
        /// <returns>Array of VKPost objects</returns>
        public VKPost[] GetMessages(int count)
        {
            var posts = new List<VKPost>();
            var url = GET_MESSAGES_AS_XML;
            url += count.ToString(CultureInfo.InvariantCulture);
            url += "&access_token=";
            url += AccessToken;
            var result = makeRequest(url, "GET");
            var xdoc = XDocument.Parse(result);
            if (xdoc.Root != null)
            {
                switch (xdoc.Root.Name.ToString())
                {
                    case "response":
                        {
                            var xc = xdoc.Root.Element("count");
                            if (xc != null && xc.Value == "0") break;
                            var list = xdoc.Root.Element("items")?.Elements("post");
                            var unixDate = new DateTime(1970, 1, 1);
                            if (list != null)
                                foreach (var x in list)
                                {
                                    var id = "";
                                    var date = DateTime.MinValue;
                                    var text = "";
                                    var xe = x.Element("id");
                                    if (xe != null)
                                        id = xe.Value.Trim();
                                    xe = x.Element("date");
                                    if (xe != null && xe.Value.Trim().Length != 0)
                                    {
                                        date = unixDate.AddSeconds(Convert.ToDouble(xe.Value.Trim()));
                                        date = date.ToLocalTime();
                                    }

                                    xe = x.Element("text");
                                    if (xe != null)
                                        text = xe.Value.Trim();
                                    posts.Add(new VKPost(id, date, text));
                                }
                        }
                        break;
                    case "error":
                        {
                            var xc = xdoc.Root.Element("error_code");
                            var xm = xdoc.Root.Element("error_msg");
                            throw new VKException(xc != null ? Convert.ToInt32(xc.Value) : 0, xm != null ? xm.Value : "");
                        }
                }
            }
            return posts.ToArray();
        }
        /// <summary>
        /// Get messages from user's wall
        /// </summary>
        /// <param name="count">Count of messages to retrieve</param>
        /// <returns>XML string with messages details</returns>
        public string GetMessagesAsXml(int count)
        {
            var url = GET_MESSAGES_AS_XML;
            url += count.ToString(CultureInfo.InvariantCulture);
            url += "&access_token=";
            url += AccessToken;
            var result = makeRequest(url, "GET");
            var xdoc = XDocument.Parse(result);
            if (xdoc.Root != null)
            {
                switch (xdoc.Root.Name.ToString())
                {
                    case "response":
                        return result;
                    case "error":
                        {
                            var xc = xdoc.Root.Element("error_code");
                            var xm = xdoc.Root.Element("error_msg");
                            throw new VKException(xc != null ? Convert.ToInt32(xc.Value) : 0, xm != null ? xm.Value : "");
                        }
                }
            }
            return "";
        }
        /// <summary>
        /// Get messages from user's wall
        /// </summary>
        /// <param name="count">Count of messages to retrieve</param>
        /// <returns>JSON string with messages details</returns>
        public string GetMessagesAsJson(int count)
        {
            var url = GET_MESSAGES_AS_JSON;
            url += count.ToString(CultureInfo.InvariantCulture);
            url += "&access_token=";
            url += AccessToken;
            var result = makeRequest(url, "GET");
            var xdoc = XDocument.Parse(result);
            if (xdoc.Root != null)
            {
                switch (xdoc.Root.Name.ToString())
                {
                    case "response":
                        return result;
                    case "error":
                        {
                            var xc = xdoc.Root.Element("error_code");
                            var xm = xdoc.Root.Element("error_msg");
                            throw new VKException(xc != null ? Convert.ToInt32(xc.Value) : 0, xm != null ? xm.Value : "");
                        }
                }
            }
            return "";
        }
        /// <summary>
        /// Posts message on users wall
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <returns>Posted message ID on success or 0</returns>
        public int PostMessage(string message)
        {
            message = Uri.EscapeDataString(message);
            var url = POST_AS_XML + message + "&access_token=" + AccessToken;
            var result = makeRequest(url, "POST");
            var xdoc = XDocument.Parse(result);
            if (xdoc.Root != null)
            {
                switch (xdoc.Root.Name.ToString())
                {
                    case "response":
                        {
                            var xe = xdoc.Root.Element("post_id");
                            if (xe != null)
                            {
                                return Convert.ToInt32(xe.Value);
                            }
                        }
                        break;
                    case "error":
                        {
                            var xc = xdoc.Root.Element("error_code");
                            var xm = xdoc.Root.Element("error_msg");
                            throw new VKException(xc != null ? Convert.ToInt32(xc.Value) : 0, xm != null ? xm.Value : "");
                        }
                }
            }
            throw new VKException(0, "No response from VK server");
        }

        private bool getAccessToken()
        {
            var url = AUTHORIZE_URL + _ClientId + AUTHORIZE_PARAMETERS;
            var dlgAuth = new DlgAuth(url);
            dlgAuth.AccessTokenReceived += dlgAuth_AccessTokenReceived;
            if (dlgAuth.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                dlgAuth.AccessTokenReceived -= dlgAuth_AccessTokenReceived;
                return false;
            }
            return true;
        }

        private void dlgAuth_AccessTokenReceived(object sender, AccessTokenReceivedEventArgs e)
        {
            if (sender is DlgAuth d)
            {
                d.AccessTokenReceived -= dlgAuth_AccessTokenReceived;
            }
            AccessToken = e.AccessToken;
            ExpiresIn = e.ExpiresIn;
        }

        /// <summary>
        /// Builds and process web request
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="method">Request method</param>
        /// <param name="postData">Post data for POST request - optional</param>
        /// <returns>The response data</returns>
        private string makeRequest(string url, string method, string postData = null)
        {
            if (WebRequest.Create(new Uri(url)) is HttpWebRequest webRequest)
            {
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

                    using (var reqStream = webRequest.GetRequestStream())
                    {
                        reqStream.Write(fileToSend, 0, fileToSend.Length);
                    }
                }
                else
                {
                    webRequest.ContentLength = 0;
                }
                return webResponseGet(webRequest);
            }
            return "";
        }

        /// <summary>
        /// Processes the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        private string webResponseGet(HttpWebRequest webRequest)
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
    }
    /// <summary>
    /// Represents VKontakte exception
    /// </summary>
    public class VKException : Exception
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int VKCode { get; }
        /// <summary>
        /// Error message
        /// </summary>
        public string VKMessage { get; }

        internal VKException(int vkcode, string vkmessage) : base(vkmessage)
        {
            VKCode = vkcode;
            VKMessage = vkmessage;
        }
    }
}

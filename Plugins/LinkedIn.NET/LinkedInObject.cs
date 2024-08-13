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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents object for authentication, posting and getting updates via LinkedIn API
    /// </summary>
    public class LinkedInObject
    {
        private const string UPDATE_STATUS_URL = "https://api.linkedin.com/v1/people/~/shares?";
        private const string GET_UPDATES_URL =
            "https://api.linkedin.com/v1/people/~/network/updates?scope=self&type=STAT&count=";
        private const string ACCESS_TOKEN_URL =
            "https://www.linkedin.com/uas/oauth2/accessToken?grant_type=authorization_code&code=";
        private const string AUTHORIZATION_URL =
            "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id=";

        #region Public propertiies
        /// <summary>
        /// Gets or sets consumer key
        /// </summary>
        public string ConsumerKey { get; set; }
        /// <summary>
        /// Gets or sets consumer secret
        /// </summary>
        public string ConsumerSecret { get; set; }
        /// <summary>
        /// Gets or sets authentication code
        /// </summary>
        public string AuthCode { get; set; }
        /// <summary>
        /// Gets or sets authentication token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Gets or sets access token expiration
        /// </summary>
        public DateTime Expiration { get; private set; }

        #endregion

        /// <summary>
        /// Creates new instance of LinkedInObject
        /// </summary>
        /// <param name="consumerKey">Consumer key</param>
        /// <param name="consumerSecret">Consumer secret</param>
        public LinkedInObject(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        /// <summary>
        /// Authorizes application on LinkedIn
        /// </summary>
        /// <returns>True if authorization was successfull</returns>
        public bool Authorize()
        {
            if (string.IsNullOrEmpty(ConsumerKey))
                throw new Exception("The consumer key is not set");
            if (string.IsNullOrEmpty(ConsumerSecret))
                throw new Exception("The consumer secret is not set");
            if (string.IsNullOrEmpty(getAuthCode()))
                throw new Exception("Failed to get authorize code");
            if (string.IsNullOrEmpty(getAccessToken()))
                throw new Exception("Failed to get access token");
            return true;
        }

        /// <summary>
        /// Updates user's status
        /// </summary>
        /// <param name="postData">Text of status</param>
        /// <returns>True on success, false otherwise</returns>
        public bool UpdateStatus(string postData)
        {
            var url = UPDATE_STATUS_URL;
            url += "oauth2_access_token=";
            url += AccessToken;
            postData = Utils.EscapeXml(postData);
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><share><comment>" + postData + "</comment><visibility><code>anyone</code></visibility></share>";
            var result = makeRequest(url, "POST", xml);
            var xdoc = XDocument.Parse(result);
            if (xdoc.Root != null)
            {
                var eKey = xdoc.Root.Element("update-key");
                var eUrl = xdoc.Root.Element("update-url");
                if (eKey != null && eKey.Value.Trim().Length > 0 && eUrl != null && eUrl.Value.Trim().Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all user's network updates
        /// </summary>
        /// <param name="count">Count of updates to retreive</param>
        /// <returns>User's network updates</returns>
        public LinkedInUpdate[] GetUpdates(int count)
        {
            var updates = new List<LinkedInUpdate>();
            var url = GET_UPDATES_URL + count.ToString(CultureInfo.InvariantCulture);
            url += "&oauth2_access_token=";
            url += AccessToken;
            var xml = makeRequest(url, "GET");
            var xdoc = XDocument.Parse(xml);
            var ac = xdoc.Root?.Attribute("total");
            if (ac != null && ac.Value != "0")
            {
                var unixDate = new DateTime(1970, 1, 1);
                var ups = xdoc.Root.Elements("update");
                foreach (var xu in ups)
                {
                    var updDate = DateTime.MinValue;
                    string updKey = "", updType = "";
                    bool isCommentable = false, isLikable = false, isLiked = false;
                    int numOfComments = 0, numOfLikes = 0;
                    string id = "", status = "", headLine = "", firstName = "", lastName = "";
                    var xe = xu.Element("timestamp");
                    if (xe != null && xe.Value.Trim().Length != 0)
                    {
                        updDate = unixDate.AddMilliseconds(Convert.ToDouble(xe.Value.Trim()));
                        updDate = updDate.ToLocalTime();
                    }
                    xe = xu.Element("update-key");
                    if (xe != null)
                    {
                        updKey = xe.Value.Trim();
                    }
                    xe = xu.Element("update-type");
                    if (xe != null)
                    {
                        updType = xe.Value.Trim();
                    }
                    xe = xu.Element("is-commentable");
                    if (xe != null)
                    {
                        isCommentable = Convert.ToBoolean(xe.Value.Trim());
                    }
                    xe = xu.Element("is-likable");
                    if (xe != null)
                    {
                        isLikable = Convert.ToBoolean(xe.Value.Trim());
                    }
                    xe = xu.Element("is-liked");
                    if (xe != null)
                    {
                        isLiked = Convert.ToBoolean(xe.Value.Trim());
                    }
                    xe = xu.Element("update-comments");
                    if (xe?.Attribute("total") != null && xe.Attribute("total")?.Value.Trim().Length > 0)
                    {
                        numOfComments = Convert.ToInt32(xe.Attribute("total")?.Value.Trim());
                    }
                    xe = xu.Element("num-likes");
                    if (xe != null && xe.Value.Trim().Length > 0)
                    {
                        numOfLikes = Convert.ToInt32(xe.Value.Trim());
                    }
                    var xc = xu.Element("update-content");
                    if (xc != null)
                    {
                        xc = xc.Element("person");
                        if (xc != null)
                        {
                            xe = xc.Element("id");
                            if (xe != null)
                            {
                                id = xe.Value.Trim();
                            }
                            xe = xc.Element("first-name");
                            if (xe != null)
                            {
                                firstName = xe.Value.Trim();
                            }
                            xe = xc.Element("last-name");
                            if (xe != null)
                            {
                                lastName = xe.Value.Trim();
                            }
                            xe = xc.Element("headline");
                            if (xe != null)
                            {
                                headLine = xe.Value.Trim();
                            }
                            xe = xc.Element("current-share");
                            var xcomment = xe?.Element("comment");
                            if (xcomment != null)
                                status = xcomment.Value.Trim();
                        }
                    }
                    var liUpdate = new LinkedInUpdate(updDate, updKey, updType, status, firstName, lastName,
                        headLine, id, isCommentable, isLikable, isLiked, numOfComments,
                        numOfLikes);
                    updates.Add(liUpdate);
                }
            }
            return updates.ToArray();
        }

        /// <summary>
        /// Authorizes the app by showing the dialog
        /// </summary>
        /// <returns>The authorization code.</returns>
        private String getAuthCode()
        {
            var aw = new AuthorizeWindow(AuthorizationLink);
            aw.ShowDialog();
            AuthCode = aw.Code;
            return AuthCode;
        }

        /// <summary>
        /// Gets access token and access token's expiration date
        /// </summary>
        /// <returns>The access token</returns>
        private string getAccessToken()
        {
            var url = ACCESS_TOKEN_URL;
            url += AuthCode;
            url += "&redirect_uri=";
            url += Utils.CALLBACK;
            url += "&client_id=";
            url += ConsumerKey;
            url += "&client_secret=";
            url += ConsumerSecret;
            Debug.WriteLine(url);
            var responseData = makeRequest(url, "POST");
            var dict =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
            if (dict["expires_in"] != null && dict["access_token"] != null)
            {
                Expiration = DateTime.Now.AddSeconds(Convert.ToDouble(dict["expires_in"]));
                AccessToken = dict["access_token"];
            }

            return AccessToken;
        }

        /// <summary>
        /// Builds and process web request
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="method">Request method</param>
        /// <param name="postData">Post date for POST request - optional</param>
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

                    using (Stream reqStream = webRequest.GetRequestStream())
                    {
                        reqStream.Write(fileToSend, 0, fileToSend.Length);
                    }
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

        private const string SCOPE = "w_share";
        private const string STATE = "RSFNHJH56747ngdfsj";

        /// <summary>
        /// Gets the link to Linked In's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid parameters.</returns>
        internal string AuthorizationLink
        {
            get
            {
                var link = AUTHORIZATION_URL + ConsumerKey +
                       "&scope=" + SCOPE + "&state=" + STATE + "&redirect_uri=" + Utils.CALLBACK;
                return link;
            }
        }
    }
}

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

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Facebook;
using PNCommon;
using PNEncryption;
using PluginsCore;

#endregion

namespace pnfacebook
{
    internal static class FBPost
    {
        private const string CONFIG_FILE = "pnfacebookconfig.xml";
        private const string LOCALIZATIONS_FILE = "pnfacebooklocalizations.xml";
        private const string EXTENDED_PERMISSIONS = "publish_actions,user_posts";// "user_about_me,read_stream,status_update";
        private const int MAX_LENGTH = 420;

        private static string _accessToken = "";
        private static string _appId = "";
        private static int _attempts;

        internal static List<PostDetails> Get(int count)
        {
            try
            {
                return !prepareObject() ? null : getMessages(count);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return null;
            }
        }

        internal static List<PostDetails> Get(int count, string culture)
        {
            Utils.XCulture = culture;
            var localPath = Path.Combine(Utils.AssemblyDirectory, LOCALIZATIONS_FILE);
            if (Utils.XLang == null && File.Exists(localPath))
            {
                Utils.XLang = XDocument.Load(localPath);
            }
            return Get(count);
        }

        /// <summary>
        ///     Posts message on plugin's network
        /// </summary>
        /// <param name="message">Message to post</param>
        /// <returns>True if message has been posted successfully, false otherwise</returns>
        internal static bool Post(string message)
        {
            try
            {
                return prepareObject() && sendMessage(message);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        internal static bool Post(string message, string culture)
        {
            Utils.XCulture = culture;
            var localPath = Path.Combine(Utils.AssemblyDirectory, LOCALIZATIONS_FILE);
            if (Utils.XLang == null && File.Exists(localPath))
            {
                Utils.XLang = XDocument.Load(localPath);
            }
            return Post(message);
        }

        private static bool prepareObject()
        {
            try
            {
                //load configuration file
                string configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    MessageBox.Show(Utils.GetString("config_file_not_found", "Configuration file not found"));
                    return false;
                }
                XDocument xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");
                //get parameters from configuration file
                var xId = xdoc.Root.Element("appId");
                var xToken = xdoc.Root.Element("accessToken");
                if (xId == null)
                {
                    MessageBox.Show(Utils.GetString("id_not_found", "Application id not found"));
                    return false;
                }
                _appId = xId.Value;
                if (xToken != null) _accessToken = xToken.Value;

                using (var pe = new PNEncryptor(_appId))
                {
                    if (_accessToken != "")
                    {
                        _accessToken = pe.DecryptString(_accessToken);
                    }
                }
                //check whether both access token and access token secret are stored from previous authentication
                if (_accessToken == "")
                {
                    //show authentication dialog
                    if (!showAuthDialog())
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private static bool showAuthDialog()
        {
            try
            {
                _accessToken = "";
                var dlgAuth = new DlgAuthentication(_appId, EXTENDED_PERMISSIONS);
                dlgAuth.FBAuthenticationComplete += dlgAuth_FBAuthenticationComplete;
                if (dlgAuth.ShowDialog() != DialogResult.OK)
                {
                    dlgAuth.FBAuthenticationComplete -= dlgAuth_FBAuthenticationComplete;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private static void dlgAuth_FBAuthenticationComplete(object sender, FBAuthenticationCompleteEventArgs e)
        {
            try
            {
                if (sender is DlgAuthentication dlg)
                {
                    dlg.FBAuthenticationComplete -= dlgAuth_FBAuthenticationComplete;
                }
                //DateTime dateExpires = e.Result.Expires;
                _accessToken = e.Result.AccessToken;
                string configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                XDocument xdoc = XDocument.Load(configPath);
                using (var pe = new PNEncryptor(_appId))
                {
                    if (xdoc.Root == null)
                        throw new Exception("Configuration file is empty");

                    XElement xElement = xdoc.Root.Element("accessToken");
                    if (xElement != null)
                        xElement.Value = pe.EncryptString(_accessToken);
                    else
                        xdoc.Root.Add(new XElement("accessToken", pe.EncryptString(_accessToken)));
                }
                xdoc.Save(configPath);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        private static List<PostDetails> getMessages(int count)
        {
            try
            {
                var unixdate = new DateTime(2000, 1, 1);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var span = (unixdate - epoch);
                var since = ((long)span.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                span = (DateTime.Now - epoch);
                var until = ((long)span.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                var posts = new List<PostDetails>();
                var facebookClient = new FacebookClient(_accessToken);
                //var parameters = new Dictionary<string, object>();
                //parameters["fields"] = "created_time,message";
                //parameters["limit"] = count.ToString(CultureInfo.InvariantCulture);
                //parameters["since"] = since;
                //parameters["until"] = until;

                //var postListData =
                //facebookClient.Get("/me/posts", parameters) as JsonObject;
                //var postListData =
                //    facebookClient.Get("fql",
                //                       new
                //                           {
                //                               q =
                //                           "SELECT time, message FROM status WHERE uid = me() AND message <> '' AND time > " +
                //                           since + " AND time <= " + until + " ORDER BY time DESC LIMIT 0," +
                //                           count.ToString(CultureInfo.InvariantCulture)
                //                           }) as JsonObject;
                var query = $"/me/feed?limit={count}&since={since}&until={until}";

                if (!(facebookClient.Get(query) is JsonObject postListData) || postListData.Count <= 0) return posts;
                if (!postListData.TryGetValue("data", out var values)) return posts;
                if (!(values is JsonArray arr)) return posts;
                foreach (var msg in arr.Select(t => t as JsonObject))
                {
                    object m = null, d = null;
                    if (msg != null && (!msg.TryGetValue("message", out m) || !msg.TryGetValue("created_time", out d)))
                        continue;
                    var message = Convert.ToString(m);
                    var date = Convert.ToDateTime(d);// new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(d)).ToLocalTime();
                    if (!string.IsNullOrEmpty(message))
                        posts.Add(new PostDetails(date, message));
                }
                return posts;
            }
            catch (FacebookOAuthException fex)
            {
                if (fex.Message.Contains("Session has expired") ||
                    fex.Message.Contains("user has changed the password") ||
                    (fex.Message.Contains("Error validating access token")
                     && fex.Message.Contains("has not authorized application")) ||
                    fex.Message.Contains(
                        "Error validating access token: The session is invalid because the user logged out"))
                {
                    if (_attempts == 0)
                    {
                        _attempts++;
                        if (!showAuthDialog())
                        {
                            Utils.LogException(fex);
                            return null;
                        }
                        return (getMessages(count));
                    }
                    Utils.LogException(fex);
                    return null;
                }
                Utils.LogException(fex);
                return null;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return null;
            }
        }

        private static bool sendMessage(string message)
        {
            try
            {
                //try to send message
                if (message.Length > MAX_LENGTH)
                {
                    MessageBox.Show(
                        Utils.GetString("message_too_long",
                                         "The message is too long and will be truncated to max available length") +
                        @" (" + MAX_LENGTH.ToString(CultureInfo.InvariantCulture) + @")");
                    message = message.Substring(0, MAX_LENGTH - 3) + "...";
                }
                var facebookClient = new FacebookClient(_accessToken);
                IDictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("message", message);
                facebookClient.Post("/me/feed", parameters);

                return true;
            }
            catch (FacebookOAuthException fex)
            {
                if (fex.Message.Contains("Session has expired") ||
                    fex.Message.Contains("user has changed the password") ||
                    (fex.Message.Contains("Error validating access token")
                     && fex.Message.Contains("has not authorized application")) ||
                    fex.Message.Contains(
                        "Error validating access token: The session is invalid because the user logged out"))
                {
                    if (_attempts == 0)
                    {
                        _attempts++;
                        if (!showAuthDialog())
                        {
                            Utils.LogException(fex);
                            return false;
                        }
                        return (sendMessage(message));
                    }
                    Utils.LogException(fex);
                    return false;
                }
                Utils.LogException(fex);
                return false;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }
    }
}
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
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using PNCommon;
using PluginsCore;
using PNEncryption;
using TweetSharp;

namespace pntwitter
{
    internal static class TWPost
    {
        private const string CONFIG_FILE = "pntwitterconfig.xml";
        private const string LOCALIZATIONS_FILE = "pntwitterlocalization.xml";
        private const int MAX_LENGTH = 140;

        private static string _accessToken = "";
        private static string _accessTokenSecret = "";
        private static string _consumerKey = "";
        private static string _consumerSecret = "";

        private static int _attempts;
        private static TwitterService _tweetService;

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
        /// Posts message on plugin's network
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

        private static bool prepareObject()
        {
            try
            {
                //load configuration file
                var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    MessageBox.Show(Utils.GetString("config_file_not_found", "Configuration file not found"));
                    return false;
                }
                var xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");
                //get parameters from configuration file
                var element = xdoc.Root.Element("consumerKey");
                if (element == null)
                {
                    MessageBox.Show(Utils.GetString("key_not_found", "Consumer key not found"));
                    return false;
                }
                _consumerKey = element.Value;
                element = xdoc.Root.Element("consumerSecret");
                if (element == null)
                {
                    MessageBox.Show(Utils.GetString("secret_not_found", "Consumer secret not found"));
                    return false;
                }
                using (var pe = new PNEncryptor(_consumerKey))
                {
                    _consumerSecret = pe.DecryptString(element.Value);
                }
                element = xdoc.Root.Element("accessToken");
                if (element != null) _accessToken = element.Value;
                element = xdoc.Root.Element("accessTokenSecret");
                if (element != null) _accessTokenSecret = element.Value;

                using (var pe = new PNEncryptor(_consumerKey))
                {
                    if (_accessToken != "")
                    {
                        _accessToken = pe.DecryptString(_accessToken);
                    }
                    if (_accessTokenSecret != "")
                    {
                        _accessTokenSecret = pe.DecryptString(_accessTokenSecret);
                    }
                }
                //check whether both access token and access token secret are stored from previous authentication
                if (_accessToken == "" || _accessTokenSecret == "")
                {
                    //show authentication dialog
                    if (showAuthDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                }
                //create twitter token
                createTokens();
                //send message
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private static List<PostDetails> getMessages(int count)
        {
            var posts = new List<PostDetails>();
            try
            {
                var opts = new ListTweetsOnHomeTimelineOptions { Count = count, ExcludeReplies = true };
                var results = _tweetService.ListTweetsOnHomeTimeline(opts);

                if (_tweetService.Response.Error == null)
                {
                    posts.AddRange(results.Select(p => new PostDetails(p.CreatedDate, p.Text)));
                    return posts;
                }
                if (_tweetService.Response.Error.Code == 89 || _tweetService.Response.Error.Code == 32)
                {
                    //Invalid or expired token or Could not authenticate you
                    if (_attempts == 0)
                    {
                        //try to recreate tokens and sen message one more time
                        _attempts++;
                        if (showAuthDialog() != DialogResult.OK)
                        {
                            return null;
                        }
                        createTokens();
                        return getMessages(count);
                    }
                }
                MessageBox.Show(_tweetService.Response.Error.Message);
                return null;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Sends message to Twitter
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>True if message has been sent successfully, false otherwise</returns>
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
                var options = new SendTweetOptions { Status = message };
                _tweetService.SendTweet(options);

                if (_tweetService.Response.Error == null)
                {
                    //message has been successfully sent
                    return true;
                }
                if (_tweetService.Response.Error.Code == 89 || _tweetService.Response.Error.Code == 32)
                {
                    //Invalid or expired token or Could not authenticate you
                    if (_attempts == 0)
                    {
                        //try to recreate tokens and sen message one more time
                        _attempts++;
                        if (showAuthDialog() != DialogResult.OK)
                        {
                            return false;
                        }
                        createTokens();
                        return sendMessage(message);
                    }
                }
                MessageBox.Show(_tweetService.Response.Error.Message);
                return false;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private static void createTokens()
        {
            try
            {
                _tweetService = new TwitterService(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        private static DialogResult showAuthDialog()
        {
            try
            {
                _accessToken = _accessTokenSecret = "";
                var dlgAuth = new DlgAuthentication(_consumerKey, _consumerSecret);
                dlgAuth.TwitterServiceReceived += dlgAuth_TwitterServiceReceived;
                var result = dlgAuth.ShowDialog();
                if (result != DialogResult.OK)
                {
                    dlgAuth.TwitterServiceReceived -= dlgAuth_TwitterServiceReceived;
                }
                return result;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return DialogResult.Cancel;
            }
        }

        private static void dlgAuth_TwitterServiceReceived(object sender, TwitterServiceReceivedEventArgs e)
        {
            if (sender is DlgAuthentication dlg)
            {
                dlg.TwitterServiceReceived -= dlgAuth_TwitterServiceReceived;
            }
            //store and save acces token and access token secret
            var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
            _accessToken = e.Tokens.Token;
            _accessTokenSecret = e.Tokens.TokenSecret;
            var xdoc = XDocument.Load(configPath);
            using (var pe = new PNEncryptor(_consumerKey))
            {
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");

                var element = xdoc.Root.Element("accessToken");
                if (element != null)
                    element.Value = pe.EncryptString(_accessToken);
                else
                    xdoc.Root.Add(new XElement("accessToken", pe.EncryptString(_accessToken)));
                element = xdoc.Root.Element("accessTokenSecret");
                if (element != null)
                    element.Value = pe.EncryptString(_accessTokenSecret);
                else
                    xdoc.Root.Add(new XElement("accessTokenSecret", pe.EncryptString(_accessTokenSecret)));
            }
            xdoc.Save(configPath);
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
    }
}

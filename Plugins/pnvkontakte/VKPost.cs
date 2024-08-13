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
using PNEncryption;
using PluginsCore;
using VKontakte.NET;
using PNCommon;

namespace pnvkontakte
{
    internal static class VKPost
    {
        private const string CONFIG_FILE = "pnvkontakteconfig.xml";
        private const string LOCALIZATIONS_FILE = "pnvkontaktelocalizations.xml";
        private const int MAX_LENGTH = 1024;
        private const int ATTEMPTS = 10;

        private static string _accessToken = "";
        private static string _clientId = "";

        internal static List<PostDetails> Get(int count)
        {
            var attempts = 0;
            var posts = new List<PostDetails>();
            if (prepareTokens())
            {
                while (true)
                {
                    try
                    {
                        var vk = new VKObject(_clientId, _accessToken);
                        var results = vk.GetMessages(count);
                        posts.AddRange(results.Select(vp => new PostDetails(vp.Date, vp.Text)));
                        break;
                    }
                    catch (VKException vex)
                    {
                        Utils.LogException(vex);
                        attempts++;
                        if (attempts > ATTEMPTS)
                        {
                            throw;
                        }
                        reCreateTokens();
                    }
                    catch (Exception ex)
                    {
                        Utils.LogException(ex);
                        return null;
                    }
                }
            }
            return posts;
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

        internal static bool Post(string message)
        {
            try
            {
                var attempts = 0;
                if (message.Length > MAX_LENGTH)
                {
                    MessageBox.Show(
                        Utils.GetString("message_too_long",
                                         "The message is too long and will be truncated to max available length") +
                        @" (" + MAX_LENGTH.ToString(CultureInfo.InvariantCulture) + @")");
                    message = message.Substring(0, MAX_LENGTH - 3) + "...";
                }
                if (prepareTokens())
                {
                    while (true)
                    {
                        try
                        {
                            var vk = new VKObject(_clientId, _accessToken);
                            if (vk.PostMessage(message) > 0)
                                return true;
                        }
                        catch (VKException vex)
                        {
                            Utils.LogException(vex);
                            attempts++;
                            if (attempts > ATTEMPTS)
                            {
                                throw;
                            }
                            reCreateTokens();
                        }
                        catch (Exception ex)
                        {
                            Utils.LogException(ex);
                            return false;
                        }
                    }
                }

                return false;
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

        private static bool prepareTokens()
        {
            var attempts = 0;
            var expiration = "";

            _accessToken = "";

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
            var element = xdoc.Root.Element("clientId");
            if (element == null)
            {
                MessageBox.Show(Utils.GetString("client_id_not_found", "Client ID not found"));
                return false;
            }
            _clientId = element.Value;
            element = xdoc.Root.Element("accessToken");
            if (element != null) _accessToken = element.Value;
            element = xdoc.Root.Element("expiration");
            if (element != null) expiration = element.Value;

            using (var pe = new PNEncryptor(_clientId))
            {
                if (_accessToken != "")
                {
                    _accessToken = pe.DecryptString(_accessToken);
                }
            }
            //check whether access token is stored from previous authentication
            if (_accessToken == "" || expiration == "" || Convert.ToDateTime(expiration) < DateTime.Now)
            {
                VKObject vk;
                while (true)
                {
                    vk = new VKObject(_clientId);
                    try
                    {
                        if (vk.Authenticate())
                            break;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Utils.LogException(ex);
                        attempts++;
                        if (attempts > ATTEMPTS)
                        {
                            throw;
                        }
                    }
                }
                //store parameters
                _accessToken = vk.AccessToken;
                using (var pe = new PNEncryptor(_clientId))
                {
                    element = xdoc.Root.Element("accessToken");
                    if (element != null)
                        element.Value = pe.EncryptString(_accessToken);
                    else
                        xdoc.Root.Add(new XElement("accessToken", pe.EncryptString(_accessToken)));
                    var expDate = DateTime.Now.AddSeconds(Convert.ToDouble(vk.ExpiresIn));
                    element = xdoc.Root.Element("expiration");
                    if (element != null)
                        element.Value = expDate.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US"));
                    else
                        xdoc.Root.Add(new XElement("expiration", expDate.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US"))));
                    xdoc.Save(configPath);
                }
            }
            return true;
        }

        private static void reCreateTokens()
        {
            try
            {
                //load configuration file
                var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    MessageBox.Show(Utils.GetString("config_file_not_found", "Configuration file not found"));
                    return;
                }
                var xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");
                var xElement = xdoc.Root.Element("accessToken");
                xElement?.Remove();
                xElement = xdoc.Root.Element("expiration");
                xElement?.Remove();
                xdoc.Save(configPath);

                prepareTokens();
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }
    }
}

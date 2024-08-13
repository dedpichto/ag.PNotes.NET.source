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
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Forms;
using PluginsCore;

namespace pntwitter
{
    /// <summary>
    /// Twitter implementation of IPostPlugin interface
    /// </summary>
    [Export(typeof(IPlugin))]
    public class PNTwitter : IPostPlugin
    {
        private IPluginsHost _Host;
        private ToolStripMenuItem _MenuPostPartial;
        private ToolStripMenuItem _MenuPostFull;
        private ToolStripMenuItem _MenuGetPartial;
        private ToolStripMenuItem _MenuGetFull;

        #region IPostPlugin Members

        /// <summary>
        /// Gets plugin name. The name should be unique among other plugins.
        /// </summary>
        public string Name => "Twitter";

        public string ProductName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return customAttributes.Length > 0 ? ((AssemblyProductAttribute)customAttributes[0]).Product : "";
            }
        }

        public string AdditionalInfo => "Uses Tweetsharp by Daniel Crenna (https://github.com/danielcrenna/tweetsharp)";

        public void Init(IPluginsHost host)
        {
            _Host = host;
            _MenuPostPartial = new ToolStripMenuItem(Name, Properties.Resources.twitter, menuClick);
            _MenuPostFull = new ToolStripMenuItem(Name, Properties.Resources.twitter, menuClick);
            _MenuGetPartial = new ToolStripMenuItem(Name, Properties.Resources.twitter, menuClick);
            _MenuGetFull = new ToolStripMenuItem(Name, Properties.Resources.twitter, menuClick);
        }

        private void menuClick(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;
            if (item.Equals(_MenuPostPartial))
            {
                RichTextBox edit = _Host.ActiveTextBox;
                if (edit != null)
                {
                    var result = TWPost.Post(edit.SelectedText, _Host.ActiveCulture);
                    PostPerformed?.Invoke(this, new PostPerformedEventArgs(result));
                }
            }
            else if (item.Equals(_MenuPostFull))
            {
                RichTextBox edit = _Host.ActiveTextBox;
                if (edit != null)
                {
                    var result = TWPost.Post(edit.Text, _Host.ActiveCulture);
                    PostPerformed?.Invoke(this, new PostPerformedEventArgs(result));
                }
            }
            else if (item.Equals(_MenuGetPartial))
            {
                var details = TWPost.Get(_Host.LimitToGet, _Host.ActiveCulture);
                GotPostsPartial?.Invoke(this, new GotPostsEventArgs(details));
            }
            else if (item.Equals(_MenuGetFull))
            {
                var details = TWPost.Get(_Host.LimitToGet, _Host.ActiveCulture);
                GotPostsFull?.Invoke(this, new GotPostsEventArgs(details));
            }
        }

        public string Author => "Andrey Gruber";

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public ToolStripMenuItem MenuPostPartial => _MenuPostPartial;

        public ToolStripMenuItem MenuPostFull => _MenuPostFull;

        public ToolStripMenuItem MenuGetPartial => _MenuGetPartial;

        public ToolStripMenuItem MenuGetFull => _MenuGetFull;

        public event EventHandler<GotPostsEventArgs> GotPostsPartial;
        public event EventHandler<GotPostsEventArgs> GotPostsFull;
        public event EventHandler<PostPerformedEventArgs> PostPerformed;

        #endregion
    }
}

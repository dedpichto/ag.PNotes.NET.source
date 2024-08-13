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

namespace pnbox
{
    [Export(typeof(IPlugin))]
    public class PNBox : ISyncEnhPlugin
    {
        private IPluginsHost _Host;
        private ToolStripMenuItem _MenuSync;

        public void Init(IPluginsHost host)
        {
            _Host = host;
            _MenuSync = new ToolStripMenuItem(Name, Properties.Resources.box, menuClick);
        }

        public string Name => "Box";

        public string Author => "Andrey Gruber";

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string ProductName
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return customAttributes.Length > 0 ? ((AssemblyProductAttribute)customAttributes[0]).Product : "";
            }
        }

        public string AdditionalInfo => "Uses Box.NET library by Andrey Gruber (http://pnotes.sourceforge.net/)";

        public SyncResult Synchronize()
        {
            var bs = new BSync();
            var result = bs.Synchronize(_Host.SyncParameters, _Host.ActiveCulture);
            SyncComplete?.Invoke(this, new SyncCompleteEventArgs(result));
            return result;
        }

        public void SynchronizeInBackground()
        {
            var bs = new BSync();
            bs.InnerSyncComplete += bs_InnerSyncComplete;
            bs.SynchronizeInBackground(_Host.SyncParameters);
        }

        public ToolStripMenuItem MenuSync => _MenuSync;

        public bool InProgress { get; set; }

        private void bs_InnerSyncComplete(object sender, SyncCompleteEventArgs e)
        {
            if (sender is BSync gds)
                gds.InnerSyncComplete -= bs_InnerSyncComplete;
            SyncCompleteInBackground?.Invoke(this, e);
        }

        private void menuClick(object sender, EventArgs e)
        {
            if (BeforeSync != null)
            {
                var be = new BeforeSyncEventArgs();
                BeforeSync(this, be);
                if (be.Cancel) return;
            }
            var bs = new BSync();
            var result = bs.Synchronize(_Host.SyncParameters, _Host.ActiveCulture);
            SyncComplete?.Invoke(this, new SyncCompleteEventArgs(result));
        }

        public event EventHandler<SyncCompleteEventArgs> SyncComplete;
        public event EventHandler<BeforeSyncEventArgs> BeforeSync;        
        public event EventHandler<SyncCompleteEventArgs> SyncCompleteInBackground;
    }
}

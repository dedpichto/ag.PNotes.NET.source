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

using PluginsCore;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Forms;

namespace pnsftp
{
    [Export(typeof(IPlugin))]
    public class PNSFTP : ISyncEnhPlugin
    {
        private IPluginsHost _Host;
        private ToolStripMenuItem _MenuSync;

        public void Init(IPluginsHost host)
        {
            _Host = host;
            _MenuSync = new ToolStripMenuItem(Name, Properties.Resources.sftp, menuClick);
        }

        public string Name => "SFTP";

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

        public string AdditionalInfo => "Uses SSH.NET library by RENCI (https://sshnet.codeplex.com/)";

        public SyncResult Synchronize()
        {
            var sftpSync = new SFTPSync();
            var result = sftpSync.Synchronize(_Host.SyncParameters, _Host.ActiveCulture);
            SyncComplete?.Invoke(this, new SyncCompleteEventArgs(result));
            return result;
        }

        public void SynchronizeInBackground()
        {
            var sftpSync = new SFTPSync();
            sftpSync.InnerSyncComplete += sftpSync_InnerSyncComplete;
            sftpSync.SynchronizeInBackground(_Host.SyncParameters);
        }

        public ToolStripMenuItem MenuSync => _MenuSync;

        public bool InProgress { get; set; }

        private void sftpSync_InnerSyncComplete(object sender, SyncCompleteEventArgs e)
        {
            if (sender is SFTPSync sftpSync)
                sftpSync.InnerSyncComplete -= sftpSync_InnerSyncComplete;
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
            var sftpSync = new SFTPSync();
            var result = sftpSync.Synchronize(_Host.SyncParameters, _Host.ActiveCulture);
            SyncComplete?.Invoke(this, new SyncCompleteEventArgs(result));
        }

        public event EventHandler<SyncCompleteEventArgs> SyncComplete;
        public event EventHandler<BeforeSyncEventArgs> BeforeSync;
        public event EventHandler<SyncCompleteEventArgs> SyncCompleteInBackground;
    }
}

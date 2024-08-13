using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginsCore;

namespace pnonedrive
{
    [Export(typeof(IPlugin))]
    public class PNOneDrive : ISyncAsyncPlugin
    {
        private IPluginsHost _Host;

        public void Init(IPluginsHost host)
        {
            _Host = host;
            MenuSync = new ToolStripMenuItem(Name, Properties.Resources.skydrive, menuClick);
        }

        public string Name => "OneDrive";

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

        public string AdditionalInfo => "Uses ODrive.NET library by Andrey Gruber (https://pnotes.sourceforge.io/)";

        [Obsolete("This method is not used since OneDrive synchronization is working as async")]
        public SyncResult Synchronize()
        {
            return SyncResult.None;
        }

        public async Task<SyncResult> SynchronizeAsync()
        {
            var odSync = new ODSync();
            var result = await odSync.Synchronize(_Host.SyncParameters);
            return result;
        }

        public void SynchronizeInBackground()
        {
            var odSync = new ODSync();
            odSync.InnerSyncComplete += ods_InnerSyncComplete;
            odSync.SynchronizeInBackground(_Host.SyncParameters);
        }

        public ToolStripMenuItem MenuSync { get; private set; }

        public bool InProgress { get; set; }

        private void ods_InnerSyncComplete(object sender, SyncCompleteEventArgs e)
        {
            if (sender is ODSync odSync)
                odSync.InnerSyncComplete -= ods_InnerSyncComplete;
            SyncCompleteInBackground?.Invoke(this, e);
        }

        private async void menuClick(object sender, EventArgs e)
        {
            if (BeforeSync != null)
            {
                var be = new BeforeSyncEventArgs();
                BeforeSync(this, be);
                if (be.Cancel) return;
            }
            var odSync = new ODSync();
            var result = await odSync.Synchronize(_Host.SyncParameters);
            SyncComplete?.Invoke(this, new SyncCompleteEventArgs(result));
        }


        public event EventHandler<SyncCompleteEventArgs> SyncComplete;
        public event EventHandler<BeforeSyncEventArgs> BeforeSync;
        public event EventHandler<SyncCompleteEventArgs> SyncCompleteInBackground;
    }
}

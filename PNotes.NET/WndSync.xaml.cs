// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSync.xaml
    /// </summary>
    public partial class WndSync
    {
        public WndSync()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSync(string folder)
            : this()
        {
            _Folder = folder;
        }

        internal WndSync(ISyncPlugin plugin)
            : this()
        {
            _Plugin = plugin;
        }

        private readonly string _Folder;
        private readonly ISyncPlugin _Plugin;
        private string _Caption;

        private void DlgSync_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (!string.IsNullOrEmpty(_Folder))
                {
                    Title += $" [{PNLang.Instance.GetCaptionText("sync_local", "Local")}]";
                    var sync = new PNSync();
                    sync.SyncComplete += sync_SyncComplete;
                    Task.Factory.StartNew(() => sync.SyncLocal(_Folder));
                }
                else if (_Plugin != null)
                {
                    _Caption = _Plugin.Name;
                    Title += $" [{_Caption}]";
                    if (_Plugin is ISyncAsyncPlugin)
                    {
                        synchronizeAsync();
                    }
                    else
                    {
                        _Plugin.SyncComplete += _Plugin_SyncComplete;
                        var t = new Thread(() => _Plugin.Synchronize());
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private async void synchronizeAsync()
        {
            try
            {
                var result = await ((ISyncAsyncPlugin)_Plugin).SynchronizeAsync();
                finishAsyncSync(result);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void FinishAsyncSyncDelegate(SyncResult result);
        private void finishAsyncSync(SyncResult result)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    FinishAsyncSyncDelegate d = finishAsyncSync;
                    Dispatcher.Invoke(d, result);
                }
                else
                {
                    Topmost = false;
                    DialogResult = true;
                    asyncSyncComplete(result);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void AsyncSyncCompleteDelegate(SyncResult result);

        private void asyncSyncComplete(SyncResult result)
        {
            try
            {
                if (!PNWindows.Instance.FormMain.Dispatcher.CheckAccess())
                {
                    AsyncSyncCompleteDelegate d = asyncSyncComplete;
                    PNWindows.Instance.FormMain.Dispatcher.Invoke(d, result);
                }
                else
                {
                    switch (result)
                    {
                        case SyncResult.None:
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("sync_complete", "Synchronization completed successfully"),
                                _Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case SyncResult.Reload:
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("sync_complete_reload",
                                    "Synchronization completed successfully. The program has to be restarted for applying all changes."),
                                _Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                            PNData.UpdateTablesAfterSync();
                            PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                            break;
                        case SyncResult.AbortVersion:
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("diff_versions",
                                    "Current version of database is different from previously synchronized version. Synchronization cannot be performed."),
                                _Caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            break;
                        case SyncResult.Error:
                            var sb =
                                new StringBuilder(PNLang.Instance.GetMessageText("sync_error_1",
                                    "An error occurred during synchronization."));
                            sb.Append(" (");
                            sb.Append(_Caption);
                            sb.Append(")");
                            sb.AppendLine();
                            sb.Append(PNLang.Instance.GetMessageText("sync_error_2",
                                    "Please, refer to log file of appropriate plugin."));
                            var balloon = new Baloon(BaloonMode.Error) { BaloonText = sb.ToString() };
                            PNWindows.Instance.FormMain.ntfPN.ShowCustomBalloon(balloon, PopupAnimation.Slide, null);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void SyncCompleteDelegate(object sender, SyncCompleteEventArgs e);
        private void _Plugin_SyncComplete(object sender, SyncCompleteEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    SyncCompleteDelegate d = _Plugin_SyncComplete;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    _Plugin.SyncComplete -= _Plugin_SyncComplete;
                    Topmost = false;
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void LocalSyncCompleteDelegate(object sender, LocalSyncCompleteEventArgs e);
        private void sync_SyncComplete(object sender, LocalSyncCompleteEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    LocalSyncCompleteDelegate d = sync_SyncComplete;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    Topmost = false;
                    var sync = (PNSync)sender;
                    sync.SyncComplete -= sync_SyncComplete;
                    switch (e.Result)
                    {
                        case LocalSyncResult.None:
                            WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Synchronization completed successfully"), PNStrings.PROG_NAME, MessageBoxButton.OK);
                            DialogResult = false;
                            break;
                        case LocalSyncResult.Reload:
                            WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Synchronization completed successfully. The program has to be restarted for applying all changes."), PNStrings.PROG_NAME, MessageBoxButton.OK);
                            PNData.UpdateTablesAfterSync();
                            DialogResult = true;
                            break;
                        case LocalSyncResult.AbortVersion:
                            WPFMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            DialogResult = false;
                            break;
                        case LocalSyncResult.Error:
                            DialogResult = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}

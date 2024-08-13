using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for UCProtection.xaml
    /// </summary>
    public partial class UcProtection : ISettingsPage
    {
        public UcProtection()
        {
            InitializeComponent();
        }

        private bool _Loaded;
        private WndSettings _ParentWindow;

        public void Init(WndSettings setings)
        {
            _ParentWindow = setings;
            _SyncComps = PNStatic.SyncComps.PNClone();
        }

        public void InitPage(bool firstTime)
        {
            initPageProtection(firstTime);
        }

        public ChangesAction DefineChangesAction()
        {
            return ChangesAction.None;
        }

        public bool SavePage()
        {
            return saveProtection();
        }

        public bool SaveCollections()
        {
            if (PNStatic.SyncComps.Inequals(_SyncComps))
            {
                PNStatic.SyncComps = _SyncComps.PNClone();
                PNData.SaveSyncComps();
            }
            return true;
        }

        public bool CanExecute()
        {
            if (PNStatic.SyncComps.Inequals(_SyncComps))
                return true;
            return false;
        }

        public void RestoreDefaultValues()
        {
        }

        public bool InDefClick { get; set; }

        public event EventHandler PromptToRestart;

        #region Protection staff
        private readonly string[] _SyncPeriods =
        {
            "Never",
            "1 min",
            "3 min",
            ">5 min",
            "10 min",
            "15 min",
            "20 min",
            "30 min",
            "45 min",
            "60 min",
            "120 min",
            "180 min",
            "240 min"
        };

        public class SSyncComp
        {
            public string CompName { get; private set; }
            public string NotesFile { get; private set; }
            public string DbFile { get; private set; }

            public SSyncComp(string compName, string nfile, string dbfile)
            {
                CompName = compName;
                NotesFile = nfile;
                DbFile = dbfile;
            }
        }

        private readonly ObservableCollection<SSyncComp> _SyncCompsList = new ObservableCollection<SSyncComp>();
        private List<PNSyncComp> _SyncComps;

        internal bool SyncCompExists(string compName)
        {
            return _SyncComps.Any(sc => sc.CompName == compName);
        }

        internal void SyncCompAdd(PNSyncComp sc)
        {
            try
            {
                _SyncComps.Add(sc);
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SyncCompReplace(PNSyncComp sc)
        {
            try
            {
                var s = _SyncComps.FirstOrDefault(scd => scd.CompName == sc.CompName);
                if (s == null) return;
                s.DataDir = sc.DataDir;
                s.DBDir = sc.DBDir;
                s.UseDataDir = sc.UseDataDir;
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyPanelLanguage()
        {
            try
            {
                var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                foreach (var c in daysChecks)
                {
                    c.Content = ci.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)Convert.ToInt32(c.Tag));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initializeComboBoxes()
        {
            try
            {
                if (!string.IsNullOrEmpty(PNStatic.Settings.GeneralSettings.Language))
                    return;
                //fill only if there is no language setting
                foreach (var s in _SyncPeriods)
                {
                    cboLocalSyncPeriod.Items.Add(s);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageProtection(bool firstTime)
        {
            try
            {
                if(firstTime)
                    initializeComboBoxes();
                chkStoreEncrypted.IsChecked = _ParentWindow.TempSettings.Protection.StoreAsEncrypted;
                chkHideTrayIcon.IsChecked = _ParentWindow.TempSettings.Protection.HideTrayIcon;
                chkBackupBeforeSaving.IsChecked = _ParentWindow.TempSettings.Protection.BackupBeforeSaving;
                chkSilentFullBackup.IsChecked = _ParentWindow.TempSettings.Protection.SilentFullBackup;
                updBackup.Value = _ParentWindow.TempSettings.Protection.BackupDeepness;
                chkDonotShowProtected.IsChecked = _ParentWindow.TempSettings.Protection.DontShowContent;
                chkIncludeBinInLocalSync.IsChecked = _ParentWindow.TempSettings.Protection.IncludeBinInSync;
                if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                {
                    cmdChangePwrd.IsEnabled =
                        cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
                    cmdCreatePwrd.IsEnabled = true;
                }
                else
                {
                    cmdChangePwrd.IsEnabled =
                        cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;
                    cmdCreatePwrd.IsEnabled = false;
                }
                if (firstTime)
                {
                    fillSyncComps();
                }
                var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                foreach (var c in daysChecks)
                {
                    c.IsChecked = _ParentWindow.TempSettings.Protection.FullBackupDays.Contains((DayOfWeek)Convert.ToInt32(c.Tag));
                }
                enableFullBackupTime();

                //if (_ParentWindow.TempSettings.Protection.FullBackupTime != DateTime.MinValue)
                //{
                dtpFullBackup.DateValue = _ParentWindow.TempSettings.Protection.FullBackupTime;
                //}
                chkPromptForPassword.IsChecked = _ParentWindow.TempSettings.Protection.PromptForPassword;
                txtDataDir.Text = _ParentWindow.TempSettings.Protection.LocalSyncFilesLocation;
                switch (_ParentWindow.TempSettings.Protection.LocalSyncPeriod)
                {
                    case -1:
                        cboLocalSyncPeriod.SelectedIndex = 0;
                        break;
                    case 1:
                        cboLocalSyncPeriod.SelectedIndex = 1;
                        break;
                    case 3:
                        cboLocalSyncPeriod.SelectedIndex = 2;
                        break;
                    case 5:
                        cboLocalSyncPeriod.SelectedIndex = 3;
                        break;
                    case 10:
                        cboLocalSyncPeriod.SelectedIndex = 4;
                        break;
                    case 15:
                        cboLocalSyncPeriod.SelectedIndex = 5;
                        break;
                    case 20:
                        cboLocalSyncPeriod.SelectedIndex = 6;
                        break;
                    case 30:
                        cboLocalSyncPeriod.SelectedIndex = 7;
                        break;
                    case 45:
                        cboLocalSyncPeriod.SelectedIndex = 8;
                        break;
                    case 60:
                        cboLocalSyncPeriod.SelectedIndex = 9;
                        break;
                    case 120:
                        cboLocalSyncPeriod.SelectedIndex = 10;
                        break;
                    case 180:
                        cboLocalSyncPeriod.SelectedIndex = 11;
                        break;
                    case 240:
                        cboLocalSyncPeriod.SelectedIndex = 12;
                        break;
                }
                if(firstTime)
                    _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveProtection()
        {
            try
            {
                if (PNStatic.SyncComps.Inequals(_SyncComps))
                {
                    PNStatic.SyncComps = _SyncComps.PNClone();
                    PNData.SaveSyncComps();
                }
                //check for pasword and encrypt/decryp notes if needed
                if (PNStatic.Settings.Protection.PasswordString == _ParentWindow.TempSettings.Protection.PasswordString)
                {
                    var passwordString = PNStatic.Settings.Protection.PasswordString;
                    if (passwordString.Length > 0)
                    {
                        // check encryption settings
                        if (!PNStatic.Settings.Protection.StoreAsEncrypted &&
                            _ParentWindow.TempSettings.Protection.StoreAsEncrypted)
                        {
                            // no encryption before and encryption after - encrypt all notes
                            PNNotesOperations.EncryptAllNotes(passwordString);
                        }
                        else if (PNStatic.Settings.Protection.StoreAsEncrypted &&
                                 !_ParentWindow.TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryption before and no encryption after - decrypt all notes
                            PNNotesOperations.DecryptAllNotes(passwordString);
                        }
                    }
                }
                else
                {
                    if (PNStatic.Settings.Protection.PasswordString.Length == 0 &&
                        _ParentWindow.TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // no password before and password ater - check encryption settings
                        if (_ParentWindow.TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryp all notes
                            PNNotesOperations.EncryptAllNotes(_ParentWindow.TempSettings.Protection.PasswordString);
                        }
                    }
                    else if (PNStatic.Settings.Protection.PasswordString.Length > 0 &&
                             _ParentWindow.TempSettings.Protection.PasswordString.Length == 0)
                    {
                        // password before and no password after
                        if (PNStatic.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes
                            PNNotesOperations.DecryptAllNotes(PNStatic.Settings.Protection.PasswordString);
                        }
                    }
                    else if (PNStatic.Settings.Protection.PasswordString.Length > 0 &&
                             _ParentWindow.TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // password has been changed
                        if (PNStatic.Settings.Protection.StoreAsEncrypted &&
                            _ParentWindow.TempSettings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNStatic.Settings.Protection.PasswordString);
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_ParentWindow.TempSettings.Protection.PasswordString);
                        }
                        else if (PNStatic.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNStatic.Settings.Protection.PasswordString);
                        }
                        else if (_ParentWindow.TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_ParentWindow.TempSettings.Protection.PasswordString);
                        }
                    }
                }

                var raiseEvent = PNStatic.Settings.Protection.DontShowContent !=
                                 _ParentWindow.TempSettings.Protection.DontShowContent;
                PNStatic.Settings.Protection = _ParentWindow.TempSettings.Protection.PNClone();
                PNData.SaveProtectionSettings();
                if (raiseEvent)
                {
                    PNStatic.FormMain.RaiseContentDisplayChangedEevent();
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private PNSyncComp getSelectedSyncComp()
        {
            try
            {
                var item = grdLocalSync.SelectedItem as SSyncComp;
                return item == null ? null : _SyncComps.FirstOrDefault(s => s.CompName == item.CompName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSyncComp()
        {
            try
            {
                var sc = getSelectedSyncComp();
                if (sc == null) return;
                var d = new WndSyncComps(_ParentWindow, sc, AddEditMode.Edit);
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSyncComps(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSyncComps(bool firstTime = true)
        {
            try
            {
                _SyncCompsList.Clear();
                cmdEditComp.IsEnabled = cmdRemoveComp.IsEnabled = false;
                foreach (var sc in _SyncComps)
                {
                    _SyncCompsList.Add(new SSyncComp(sc.CompName, sc.DataDir, sc.DBDir));
                }
                if (firstTime)
                    grdLocalSync.ItemsSource = _SyncCompsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        private void enableFullBackupTime()
        {
            try
            {
                var daysCheck = stkFullBackup.Children.OfType<CheckBox>();
                lblFullBackup.IsEnabled =
                    dtpFullBackup.IsEnabled = daysCheck.Any(c => c.IsChecked != null && c.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCreatePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if there is no password - show standard password creation dialog
                if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                {
                    var dpc = new WndPasswordCreate();
                    dpc.PasswordChanged += dpc_PasswordChanged;
                    var showDialog = dpc.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpc.PasswordChanged -= dpc_PasswordChanged;
                    }
                }
                else
                {
                    // otherwise, if password has been deleted but not applied yet, show password changing dialog using old password
                    cmdChangePwrd.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdChangePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password changing dialog
                if (PNStatic.Settings.Protection.PasswordString == _ParentWindow.TempSettings.Protection.PasswordString)
                {
                    var dpc = new WndPasswordChange();
                    dpc.PasswordChanged += dpc_PasswordChanged;
                    var showDialog = dpc.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpc.PasswordChanged -= dpc_PasswordChanged;
                    }
                }
                else
                {
                    // if new password has not been applied and old password is empty - show standard password creation dialog
                    if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                    {
                        var dpc = new WndPasswordCreate();
                        dpc.PasswordChanged += dpc_PasswordChanged;
                        var showDialog = dpc.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpc.PasswordChanged -= dpc_PasswordChanged;
                        }
                    }
                    else
                    {
                        // otherwise show standard password changing dialog using old password
                        var dpc = new WndPasswordChange();
                        dpc.PasswordChanged += dpc_PasswordChanged;
                        var showDialog = dpc.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpc.PasswordChanged -= dpc_PasswordChanged;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRemovePwrd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password removing dialog
                if (PNStatic.Settings.Protection.PasswordString == _ParentWindow.TempSettings.Protection.PasswordString)
                {
                    var dpd = new WndPasswordDelete(PasswordDlgMode.DeleteMain);
                    dpd.PasswordDeleted += dpd_PasswordDeleted;
                    var showDialog = dpd.ShowDialog();
                    if (showDialog == null || !showDialog.Value)
                    {
                        dpd.PasswordDeleted -= dpd_PasswordDeleted;
                    }
                }
                else
                {
                    // if new password has not been applied and old password is empty - just remove new password
                    if (PNStatic.Settings.Protection.PasswordString.Trim().Length == 0)
                    {
                        dpd_PasswordDeleted(this, new EventArgs());
                    }
                    else
                    {
                        // otherwise show standard password removing dialog using old password
                        var dpd = new WndPasswordDelete(PasswordDlgMode.DeleteMain);
                        dpd.PasswordDeleted += dpd_PasswordDeleted;
                        var showDialog = dpd.ShowDialog();
                        if (showDialog == null || !showDialog.Value)
                        {
                            dpd.PasswordDeleted -= dpd_PasswordDeleted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dpd_PasswordDeleted(object sender, EventArgs e)
        {
            _ParentWindow.TempSettings.Protection.PasswordString = "";
            cmdChangePwrd.IsEnabled =
                cmdRemovePwrd.IsEnabled =
                    chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
            chkStoreEncrypted.IsChecked = chkHideTrayIcon.IsChecked = false;
            cmdCreatePwrd.IsEnabled = true;

            var dlgPasswordDelete = sender as WndPasswordDelete;
            if (dlgPasswordDelete != null)
                dlgPasswordDelete.PasswordDeleted -= dpd_PasswordDeleted;
        }

        private void dpc_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            _ParentWindow.TempSettings.Protection.PasswordString = e.NewPassword;
            cmdChangePwrd.IsEnabled = cmdRemovePwrd.IsEnabled = chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;
            cmdCreatePwrd.IsEnabled = false;

            if (sender.GetType() == typeof(WndPasswordCreate))
            {
                var dlgPasswordCreate = sender as WndPasswordCreate;
                if (dlgPasswordCreate != null)
                    dlgPasswordCreate.PasswordChanged -= dpc_PasswordChanged;
            }
            else
            {
                var dlgPasswordChange = sender as WndPasswordChange;
                if (dlgPasswordChange != null)
                    dlgPasswordChange.PasswordChanged -= dpc_PasswordChanged;
            }
        }

        private void CheckProtection_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkStoreEncrypted":
                        _ParentWindow.TempSettings.Protection.StoreAsEncrypted = cb.IsChecked.Value;
                        break;
                    case "chkHideTrayIcon":
                        _ParentWindow.TempSettings.Protection.HideTrayIcon = cb.IsChecked.Value;
                        break;
                    case "chkBackupBeforeSaving":
                        _ParentWindow.TempSettings.Protection.BackupBeforeSaving = cb.IsChecked.Value;
                        break;
                    case "chkSilentFullBackup":
                        _ParentWindow.TempSettings.Protection.SilentFullBackup = cb.IsChecked.Value;
                        break;
                    case "chkDonotShowProtected":
                        _ParentWindow.TempSettings.Protection.DontShowContent = cb.IsChecked.Value;
                        break;
                    case "chkIncludeBinInLocalSync":
                        _ParentWindow.TempSettings.Protection.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkPromptForPassword":
                        _ParentWindow.TempSettings.Protection.PromptForPassword = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updBackup_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (!_Loaded) return;
            _ParentWindow.TempSettings.Protection.BackupDeepness = Convert.ToInt32(updBackup.Value);
        }

        private void chkW0_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                var dw = (DayOfWeek)Convert.ToInt32(cb.Tag);
                if (cb.IsChecked.Value)
                {
                    if (!_ParentWindow.TempSettings.Protection.FullBackupDays.Contains(dw))
                        _ParentWindow.TempSettings.Protection.FullBackupDays.Add(dw);
                }
                else
                {
                    _ParentWindow.TempSettings.Protection.FullBackupDays.RemoveAll(d => d == dw);
                }
                enableFullBackupTime();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dtpFullBackup_DateValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.Protection.FullBackupTime = dtpFullBackup.DateValue;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdLocalSync_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdEditComp.IsEnabled = cmdRemoveComp.IsEnabled = grdLocalSync.SelectedItems.Count > 0;
        }

        private void grdLocalSync_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editSyncComp();
        }

        private void cmdAddComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var d = new WndSyncComps(_ParentWindow, AddEditMode.Add);
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSyncComps(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditComp_Click(object sender, RoutedEventArgs e)
        {
            editSyncComp();
        }

        private void cmdRemoveComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("sync_comp_delete", "Delete selected synchronization target?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                var sc = getSelectedSyncComp();
                if (sc == null) return;
                _SyncComps.Remove(sc);
                fillSyncComps(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDataDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.Protection.LocalSyncFilesLocation = txtDataDir.Text.Trim();
                if (_ParentWindow.TempSettings.Protection.LocalSyncFilesLocation.Length == 0)
                    cboLocalSyncPeriod.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDataDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("choose_dir", "Choose directory")
                };
                if (txtDataDir.Text.Trim().Length > 0)
                    fbd.SelectedPath = txtDataDir.Text.Trim();
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtDataDir.Text = fbd.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLocalSyncPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                switch (cboLocalSyncPeriod.SelectedIndex)
                {
                    case 0:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = -1;
                        break;
                    case 1:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 1;
                        break;
                    case 2:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 3;
                        break;
                    case 3:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 5;
                        break;
                    case 4:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 10;
                        break;
                    case 5:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 15;
                        break;
                    case 6:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 20;
                        break;
                    case 7:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 30;
                        break;
                    case 8:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 45;
                        break;
                    case 9:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 60;
                        break;
                    case 10:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 120;
                        break;
                    case 11:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 180;
                        break;
                    case 12:
                        _ParentWindow.TempSettings.Protection.LocalSyncPeriod = 240;
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion
    }
}

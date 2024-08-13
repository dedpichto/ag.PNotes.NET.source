using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using PluginsCore;
using PNCommon;
using PNotes.NET.Annotations;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for UcNetwork.xaml
    /// </summary>
    public partial class UcNetwork : ISettingsPage
    {
        public UcNetwork()
        {
            InitializeComponent();
        }

        private bool _Loaded;
        private WndSettings _ParentWindow;

        public void Init(WndSettings setings)
        {
            _ParentWindow = setings;
            _Groups = PNStatic.ContactGroups.PNClone();
            _Contacts = PNStatic.Contacts.PNClone();
            _SmtpClients = PNStatic.SmtpProfiles.PNClone();
            _MailContacts = PNStatic.MailContacts.PNClone();
            _SocialPlugins = PNStatic.PostPlugins.PNClone();
            _SyncPlugins = PNStatic.SyncPlugins.PNClone();
            _TimerConnections.Elapsed += _TimerConnections_Elapsed;
        }

        public void InitPage(bool firstTime)
        {
            initPageNetwork(firstTime);
        }

        public ChangesAction DefineChangesAction()
        {
            if (_ParentWindow.TempSettings.Network.StoreOnServer && (ipServer.IsAnyBlank || txtServerPort.Value == 0))
            {
                PNMessageBox.Show(
                    PNLang.Instance.GetMessageText("no_server_props", "You must specify server IP address and port"),
                    PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return ChangesAction.Return;
            }
            return ChangesAction.None;
        }

        public bool SavePage()
        {
            return saveNetwork();
        }

        public bool SaveCollections()
        {
            if (PNStatic.ContactGroups.Inequals(_Groups))
            {
                PNStatic.ContactGroups = _Groups.PNClone();
                PNData.SaveContactGroups();
            }
            if (PNStatic.Contacts.Inequals(_Contacts))
            {
                PNStatic.Contacts = _Contacts.PNClone();
                PNData.SaveContacts();
            }
            if (PNStatic.SmtpProfiles.Inequals(_SmtpClients))
            {
                PNStatic.SmtpProfiles = _SmtpClients.PNClone();
                PNData.SaveSmtpClients();
            }
            if (PNStatic.MailContacts.Inequals(_MailContacts))
            {
                PNStatic.MailContacts = _MailContacts.PNClone();
                PNData.SaveMailContacts();
            }
            if (PNStatic.PostPlugins.Inequals(_SocialPlugins))
            {
                PNStatic.PostPlugins = _SocialPlugins.PNClone();
                PNData.SaveSocialPlugins();
            }
            if (PNStatic.SyncPlugins.Inequals(_SyncPlugins))
            {
                PNStatic.SyncPlugins = _SyncPlugins.PNClone();
                PNData.SaveSyncPlugins();
            }
            return true;
        }

        public bool CanExecute()
        {
            if (PNStatic.ContactGroups.Inequals(_Groups))
                return true;
            if (PNStatic.Contacts.Inequals(_Contacts))
                return true;
            if (PNStatic.SmtpProfiles.Inequals(_SmtpClients))
                return true;
            if (PNStatic.MailContacts.Inequals(_MailContacts))
                return true;
            if (PNStatic.PostPlugins.Inequals(_SocialPlugins))
                return true;
            if (PNStatic.SyncPlugins.Inequals(_SyncPlugins))
                return true;
            return false;
        }

        public void RestoreDefaultValues()
        {
            ipServer.Clear();
            //remove active smtp client
            foreach (var sm in _SmtpClients)
                sm.Active = false;
        }

        public bool InDefClick { get; set; }
        public event EventHandler PromptToRestart;

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _UseTimerConnection = false;
            _TimerConnections.Stop();
        }

        #region Network staff
        public class SContact : INotifyPropertyChanged
        {
            private ContactConnection _ConnectionStatus;
            public string Name { get; private set; }
            public string CompName { get; private set; }
            public string IpAddress { get; private set; }
            public ImageSource Icon { get; private set; }
            public int ID { get; private set; }

            public ContactConnection ConnectionStatus
            {
                get { return _ConnectionStatus; }
                set
                {
                    if (value == _ConnectionStatus) return;
                    _ConnectionStatus = value;
                    OnPropertyChanged("ConnectionStatus");
                }
            }

            public SContact(int id, string n, string cn, string ip, ImageSource ic)
            {
                ID = id;
                Name = n;
                CompName = cn;
                IpAddress = ip;
                Icon = ic;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SSmtpClient : INotifyPropertyChanged
        {
            private bool _Selected;

            public bool Selected
            {
                get { return _Selected; }
                set
                {
                    if (_Selected == value) return;
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }

            public string Name { get; private set; }
            public string DispName { get; private set; }
            public string Address { get; private set; }
            public int Port { get; private set; }
            public int Id { get; private set; }

            public SSmtpClient(bool sl, string n, string dn, string a, int p, int id)
            {
                Selected = sl;
                Name = n;
                DispName = dn;
                Address = a;
                Port = p;
                Id = id;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SMailContact
        {
            public string DispName { get; private set; }
            public string Address { get; private set; }
            public int Id { get; private set; }

            public SMailContact(string dispname, string address, int id)
            {
                DispName = dispname;
                Address = address;
                Id = id;
            }
        }
        private bool _UseTimerConnection = true;
        private bool _WorkInProgress;
        private readonly Timer _TimerConnections = new Timer(3000);
        private readonly ObservableCollection<SContact> _ContactsList = new ObservableCollection<SContact>();
        private readonly ObservableCollection<PNTreeItem> _GroupsList = new ObservableCollection<PNTreeItem>();
        private readonly ObservableCollection<SSmtpClient> _SmtpsList = new ObservableCollection<SSmtpClient>();
        private readonly ObservableCollection<SMailContact> _MailContactsList = new ObservableCollection<SMailContact>();
        private List<PNContactGroup> _Groups;
        private List<PNContact> _Contacts;
        private List<PNSmtpProfile> _SmtpClients;
        private List<PNMailContact> _MailContacts;
        private List<string> _SocialPlugins;
        private List<string> _SyncPlugins;

        internal bool IsInvalidIp()
        {
            return ipServer.IsAnyBlank || txtServerPort.Value == 0;
        }

        internal bool ContactAction(PNContact cn, AddEditMode mode)
        {
            try
            {
                if (mode == AddEditMode.Add)
                {
                    if (_Contacts.Any(c => c.Name == cn.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Contacts.Add(cn);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.ID == cn.ID);
                    if (c != null)
                    {
                        c.Name = cn.Name;
                        c.ComputerName = cn.ComputerName;
                        c.IpAddress = cn.IpAddress;
                        c.UseComputerName = cn.UseComputerName;
                        c.GroupID = cn.GroupID;
                    }
                }
                fillContacts(false);
                fillGroups(false);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal bool ContactGroupAction(PNContactGroup cg, AddEditMode mode)
        {
            try
            {
                if (mode == AddEditMode.Add)
                {
                    if (_Groups.Any(g => g.Name == cg.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Groups.Add(cg);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.ID == cg.ID);
                    if (g != null)
                    {
                        g.Name = cg.Name;
                    }
                }
                fillGroups(false);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void initPageNetwork(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    if (_ParentWindow.TempSettings.Network.EnableExchange)
                        _TimerConnections.Start();
                }
                chkIncludeBinInSync.IsChecked = _ParentWindow.TempSettings.Network.IncludeBinInSync;
                chkSyncOnStart.IsChecked = _ParentWindow.TempSettings.Network.SyncOnStart;
                chkSaveBeforeSync.IsChecked = _ParentWindow.TempSettings.Network.SaveBeforeSync;
                chkEnableExchange.IsChecked = _ParentWindow.TempSettings.Network.EnableExchange;
                chkAllowPing.IsChecked = _ParentWindow.TempSettings.Network.AllowPing;
                chkSaveBeforeSending.IsChecked = _ParentWindow.TempSettings.Network.SaveBeforeSending;
                chkNoNotifyOnArrive.IsChecked = _ParentWindow.TempSettings.Network.NoNotificationOnArrive;
                chkShowRecOnClick.IsChecked = _ParentWindow.TempSettings.Network.ShowReceivedOnClick;
                chkShowIncomingOnClick.IsChecked = _ParentWindow.TempSettings.Network.ShowIncomingOnClick;
                chkNoSoundOnArrive.IsChecked = _ParentWindow.TempSettings.Network.NoSoundOnArrive;
                chkNoNotifyOnSend.IsChecked = _ParentWindow.TempSettings.Network.NoNotificationOnSend;
                chkShowAfterReceiving.IsChecked = _ParentWindow.TempSettings.Network.ShowAfterArrive;
                chkHideAfterSending.IsChecked = _ParentWindow.TempSettings.Network.HideAfterSending;
                chkNoContInContextMenu.IsChecked = _ParentWindow.TempSettings.Network.NoContactsInContextMenu;
                chkRecOnTop.IsChecked = _ParentWindow.TempSettings.Network.ReceivedOnTop;

                cmdSyncNow.IsEnabled = lstSyncPlugins.SelectedIndex > -1 &&
                                       lstSyncPlugins.SelectedItems.OfType<PNTreeItem>()
                                           .Any(it => it.IsChecked != null && it.IsChecked.Value);

                txtExchPort.Value = _ParentWindow.TempSettings.Network.ExchangePort;

                if (firstTime)
                {
                    fillContacts();
                    fillGroups();
                    fillPlugins();
                    fillSmtpClients();
                    fillMailContacts();
                }
                for (var i = 0; i < cboPostCount.Items.Count; i++)
                {
                    if (Convert.ToInt32(cboPostCount.Items[i]) != _ParentWindow.TempSettings.Network.PostCount) continue;
                    cboPostCount.SelectedIndex = i;
                    break;
                }
                cboPostCount.IsEnabled = lstSocial.Items.Count > 0;
                chkStoreOnserver.IsChecked = _ParentWindow.TempSettings.Network.StoreOnServer;
                if (!string.IsNullOrWhiteSpace(_ParentWindow.TempSettings.Network.ServerIp))
                {
                    var arr = _ParentWindow.TempSettings.Network.ServerIp.Split('.').Select(s => Convert.ToByte(s.Trim())).ToArray();
                    ipServer.SetAddressBytes(arr);
                }
                txtServerPort.Value = _ParentWindow.TempSettings.Network.ServerPort;
                updTimeout.Value = _ParentWindow.TempSettings.Network.SendTimeout;
                if(firstTime)
                    _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveNetwork()
        {
            try
            {
                // no exchange before, exchange after
                if (!PNStatic.Settings.Network.EnableExchange && _ParentWindow.TempSettings.Network.EnableExchange)
                {
                    PNStatic.Settings.Network.ExchangePort = _ParentWindow.TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StartWCFHosting();
                }
                // exchange before, no exchange after
                else if (PNStatic.Settings.Network.EnableExchange && !_ParentWindow.TempSettings.Network.EnableExchange)
                {
                    PNStatic.Settings.Network.ExchangePort = _ParentWindow.TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StopWCFHosting();
                }
                // port number of exchange changed
                else if (PNStatic.Settings.Network.ExchangePort != _ParentWindow.TempSettings.Network.ExchangePort)
                {
                    PNStatic.Settings.Network.ExchangePort = _ParentWindow.TempSettings.Network.ExchangePort;
                    PNStatic.FormMain.StopWCFHosting();
                    PNStatic.FormMain.StartWCFHosting();
                }
                PNStatic.Settings.Network = _ParentWindow.TempSettings.Network.PNClone();
                PNData.SaveNetworkSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void removePlugin(PluginType type, int index)
        {
            try
            {
                if (PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("confirm_plugin_remove",
                            "Do you really want to remove selected plugin?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                var message =
                    PNLang.Instance.GetMessageText("confirm_plugin_remove_1",
                        "Plugin removal requires program restart. Do you want to restart the program now?") +
                    '\n' +
                    PNLang.Instance.GetMessageText("confirm_plugin_remove_2",
                        "OK - to restart now, No - to restart later, Cancel - to cancel removal");
                var result = PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.No:
                        var addRemove = false;
                        var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                        var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                        var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                        var xrem = xroot.Element(PNStrings.ELM_REMOVE);
                        if (xrem == null)
                        {
                            addRemove = true;
                            xrem = new XElement(PNStrings.ELM_REMOVE);
                        }
                        PNListBoxItem item = null;
                        switch (type)
                        {
                            case PluginType.Sync:
                                item = lstSyncPlugins.Items[index] as PNListBoxItem;
                                break;
                            case PluginType.Social:
                                item = lstSocial.Items[index] as PNListBoxItem;
                                break;
                        }
                        if (item == null) return;
                        var plugin = item.Tag as IPlugin;
                        if (plugin == null) return;
                        var pluginDir = PNPlugins.GetPluginDirectory(plugin.Name, PNPaths.Instance.PluginsDir);
                        var xdir = xrem.Elements(PNStrings.ELM_DIR).FirstOrDefault(e => e.Value == pluginDir);
                        if (xdir == null)
                        {
                            xrem.Add(new XElement(PNStrings.ELM_DIR, pluginDir));
                        }
                        if (addRemove)
                        {
                            xroot.Add(xrem);
                        }
                        if (xdoc.Root == null)
                            xdoc.Add(xroot);
                        xdoc.Save(filePreRun);
                        switch (type)
                        {
                            case PluginType.Social:
                                _SocialPlugins.RemoveAll(p => p == plugin.Name);
                                PNPlugins.Instance.SocialPlugins.RemoveAll(p => p.Name == plugin.Name);
                                PNData.SaveSocialPlugins();
                                lstSocial.Items.RemoveAt(index);
                                break;
                            case PluginType.Sync:
                                _SyncPlugins.RemoveAll(p => p == plugin.Name);
                                PNPlugins.Instance.SyncPlugins.RemoveAll(p => p.Name == plugin.Name);
                                PNData.SaveSyncPlugins();
                                lstSyncPlugins.Items.RemoveAt(index);
                                break;
                        }
                        if (result == MessageBoxResult.Yes)
                        {
                            PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkForNewPluginsVersion()
        {
            try
            {
                if (PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.PluginsChecking ||
                    PNSingleton.Instance.VersionChecking || PNSingleton.Instance.CriticalChecking ||
                    PNSingleton.Instance.ThemesDownload || PNSingleton.Instance.ThemesChecking) return;
                var updater = new PNUpdateChecker();
                updater.PluginsUpdateFound += updater_PluginsUpdateFound;
                updater.IsLatestVersion += updater_IsLatestVersion;
                updater.CheckPluginsNewVersion();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("plugins_latest_version",
                    "All plugins are up-to-date.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PluginsUpdateFound(object sender, PluginsUpdateFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.PluginsUpdateFound -= updater_PluginsUpdateFound;
                }
                var d = new WndGetPlugins(e.PluginsList) { Owner = _ParentWindow };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    if (PromptToRestart != null) PromptToRestart(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNMailContact getSelectedMailContact()
        {
            try
            {
                var item = grdMailContacts.SelectedItem as SMailContact;
                return item == null ? null : _MailContacts.FirstOrDefault(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editMailContact()
        {
            try
            {
                var item = getSelectedMailContact();
                if (item == null) return;
                var dlgMailContact = new WndMailContact(item) { Owner = _ParentWindow };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNContact getSelectedContact()
        {
            try
            {
                var item = grdContacts.SelectedItem as SContact;
                return item == null ? null : _Contacts.FirstOrDefault(c => c.ID == item.ID);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContact()
        {
            try
            {
                var cont = getSelectedContact();
                if (cont == null) return;
                var dlgContact = new WndContacts(cont, _Groups) { Owner = _ParentWindow };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgContact.ContactChanged -= dlgContact_ContactChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editSmtp()
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var smtpDlg = new WndSmtp(smtp) { Owner = _ParentWindow };
                smtpDlg.SmtpChanged += smtpDlg_SmtpChanged;
                var showDialog = smtpDlg.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    smtpDlg.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSmtpProfile getSelectedSmtp()
        {
            try
            {
                var item = grdSmtp.SelectedItem as SSmtpClient;
                return item == null ? null : _SmtpClients.FirstOrDefault(s => s.SenderAddress == item.Address);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNContactGroup getSelectedContactsGroup()
        {
            try
            {
                var item = tvwContactsGroups.SelectedItem as PNTreeItem;
                if (item == null || item.Tag == null || Convert.ToInt32(item.Tag) == -1) return null;
                return _Groups.FirstOrDefault(g => g.ID == Convert.ToInt32(item.Tag));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContactsGroup()
        {
            try
            {
                var gr = getSelectedContactsGroup();
                if (gr == null) return;
                var dlgContactGroup = new WndGroups(gr) { Owner = _ParentWindow };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void syncPlugin_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var item = sender as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as ISyncPlugin;
                if (plugin == null) return;
                if (e.State)
                {
                    _SyncPlugins.Add(plugin.Name);
                }
                else
                {
                    _SyncPlugins.Remove(plugin.Name);
                }
                cmdSyncNow.IsEnabled =
                    lstSyncPlugins.Items.OfType<PNListBoxItem>()
                        .Any(it => it.IsChecked != null && it.IsChecked.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void socIte_ListBoxItemCheckChanged(object sender, ListBoxItemCheckChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var item = sender as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as IPostPlugin;
                if (plugin == null) return;
                if (e.State)
                {
                    _SocialPlugins.Add(plugin.Name);
                }
                else
                {
                    _SocialPlugins.Remove(plugin.Name);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        private void fillGroups(bool firstTime = true)
        {
            try
            {
                _GroupsList.Clear();
                var imageC = TryFindResource("contact") as BitmapImage;
                // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                var imageG = TryFindResource("group") as BitmapImage;
                // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"));

                var itNone = new PNTreeItem(imageG, PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP),
                    -1);
                foreach (var cn in _Contacts.Where(c => c.GroupID == -1))
                {
                    itNone.Items.Add(new PNTreeItem(imageC, cn.Name, null));
                }
                _GroupsList.Add(itNone);

                foreach (var gc in _Groups)
                {
                    var id = gc.ID;
                    var it = new PNTreeItem(imageG, gc.Name, id);
                    foreach (var cn in _Contacts.Where(c => c.GroupID == id))
                    {
                        it.Items.Add(new PNTreeItem(imageC, cn.Name, null));
                    }
                    _GroupsList.Add(it);
                }

                if (firstTime)
                    tvwContactsGroups.ItemsSource = _GroupsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillMailContacts(bool firstTime = true)
        {
            try
            {
                _MailContactsList.Clear();
                cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = false;
                foreach (var mc in _MailContacts)
                    _MailContactsList.Add(new SMailContact(mc.DisplayName, mc.Address, mc.Id));
                cmdClearMailContacts.IsEnabled = _MailContactsList.Count > 0;
                if (firstTime)
                    grdMailContacts.ItemsSource = _MailContactsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSmtpClients(bool firstTime = true)
        {
            try
            {
                _SmtpsList.Clear();
                cmdEditSmtp.IsEnabled = cmdDeleteSmtp.IsEnabled = false;
                foreach (var sm in _SmtpClients)
                {
                    _SmtpsList.Add(new SSmtpClient(sm.Active, sm.HostName, sm.DisplayName, sm.SenderAddress, sm.Port,
                        sm.Id));
                }
                if (firstTime)
                    grdSmtp.ItemsSource = _SmtpsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillPlugins()
        {
            try
            {
                foreach (var p in PNPlugins.Instance.SocialPlugins)
                {
                    var socItem = new PNListBoxItem(null, p.Name, p, _SocialPlugins.Contains(p.Name));
                    socItem.ListBoxItemCheckChanged += socIte_ListBoxItemCheckChanged;
                    lstSocial.Items.Add(socItem);
                }
                foreach (var p in PNPlugins.Instance.SyncPlugins)
                {
                    var syncPlugin = new PNListBoxItem(null, p.Name, p, _SyncPlugins.Contains(p.Name));
                    syncPlugin.ListBoxItemCheckChanged += syncPlugin_ListBoxItemCheckChanged;
                    lstSyncPlugins.Items.Add(syncPlugin);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillContacts(bool firstTime = true)
        {
            try
            {
                _ContactsList.Clear();
                cmdEditContact.IsEnabled = cmdDeleteContact.IsEnabled = false;
                var image = TryFindResource("contact") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                foreach (var c in _Contacts)
                {
                    _ContactsList.Add(new SContact(c.ID, c.Name, c.ComputerName, c.IpAddress, image));
                }

                if (firstTime)
                    grdContacts.ItemsSource = _ContactsList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        private void CheckNetwork_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkIncludeBinInSync":
                        _ParentWindow.TempSettings.Network.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkSyncOnStart":
                        _ParentWindow.TempSettings.Network.SyncOnStart = cb.IsChecked.Value;
                        break;
                    case "chkSaveBeforeSync":
                        _ParentWindow.TempSettings.Network.SaveBeforeSync = cb.IsChecked.Value;
                        break;
                    case "chkEnableExchange":
                        _ParentWindow.TempSettings.Network.EnableExchange = cb.IsChecked.Value;
                        if (_ParentWindow.TempSettings.Network.EnableExchange)
                        {
                            _UseTimerConnection = true;
                            _TimerConnections.Start();
                        }
                        else
                        {
                            _UseTimerConnection = false;
                            _TimerConnections.Stop();
                        }
                        break;
                    case "chkSaveBeforeSending":
                        _ParentWindow.TempSettings.Network.SaveBeforeSending = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnArrive":
                        _ParentWindow.TempSettings.Network.NoNotificationOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkShowRecOnClick":
                        _ParentWindow.TempSettings.Network.ShowReceivedOnClick = cb.IsChecked.Value;
                        break;
                    case "chkShowIncomingOnClick":
                        _ParentWindow.TempSettings.Network.ShowIncomingOnClick = cb.IsChecked.Value;
                        break;
                    case "chkNoSoundOnArrive":
                        _ParentWindow.TempSettings.Network.NoSoundOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnSend":
                        _ParentWindow.TempSettings.Network.NoNotificationOnSend = cb.IsChecked.Value;
                        break;
                    case "chkShowAfterReceiving":
                        _ParentWindow.TempSettings.Network.ShowAfterArrive = cb.IsChecked.Value;
                        break;
                    case "chkHideAfterSending":
                        _ParentWindow.TempSettings.Network.HideAfterSending = cb.IsChecked.Value;
                        break;
                    case "chkNoContInContextMenu":
                        _ParentWindow.TempSettings.Network.NoContactsInContextMenu = cb.IsChecked.Value;
                        break;
                    case "chkAllowPing":
                        _ParentWindow.TempSettings.Network.AllowPing = cb.IsChecked.Value;
                        break;
                    case "chkRecOnTop":
                        _ParentWindow.TempSettings.Network.ReceivedOnTop = cb.IsChecked.Value;
                        break;
                    case "chkStoreOnserver":
                        _ParentWindow.TempSettings.Network.StoreOnServer = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwContactsGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                cmdEditContactGroup.IsEnabled = cmdDeleteContactGroup.IsEnabled = false;
                var item = e.NewValue as PNTreeItem;
                if (item == null || item.Tag == null || Convert.ToInt32(item.Tag) == -1) return;
                cmdEditContactGroup.IsEnabled = cmdDeleteContactGroup.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwContactsGroups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = tvwContactsGroups.GetObjectAtPoint<TreeViewItem>(e.GetPosition(tvwContactsGroups)) as PNTreeItem;
                if (item == null) return;
                editContactsGroup();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContactGroup_ContactGroupChanged(object sender, ContactGroupChangedEventArgs e)
        {
            try
            {
                var dg = sender as WndGroups;
                if (dg != null)
                {
                    dg.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Groups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Groups.Add(e.Group);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.ID == e.Group.ID);
                    if (g != null)
                    {
                        g.Name = e.Group.Name;
                    }
                }
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditContact.IsEnabled = cmdDeleteContact.IsEnabled = grdContacts.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdContacts.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdContacts)) as SContact;
                if (item == null) return;
                editContact();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgContact_ContactChanged(object sender, ContactChangedEventArgs e)
        {
            try
            {
                var dc = sender as WndContacts;
                if (dc != null)
                {
                    dc.ContactChanged -= dlgContact_ContactChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Contacts.Add(e.Contact);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.ID == e.Contact.ID);
                    if (c == null) return;
                    c.Name = e.Contact.Name;
                    c.ComputerName = e.Contact.ComputerName;
                    c.IpAddress = e.Contact.IpAddress;
                    c.UseComputerName = e.Contact.UseComputerName;
                    c.GroupID = e.Contact.GroupID;
                }
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtExchPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.Network.ExchangePort = Convert.ToInt32(txtExchPort.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSocial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var width = pnsSocPluginDetails.ActualWidth;
                lblSocPAuthor.Text = lblSocPVersion.Text = lblSocPInfo.Text = "";
                cmdRemovePostPlugin.IsEnabled = lstSocial.SelectedIndex > -1;
                if (lstSocial.SelectedIndex < 0) return;
                var item = lstSocial.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as IPostPlugin;
                if (plugin == null) return;
                lblSocPAuthor.Text = plugin.Author;
                lblSocPVersion.Text = plugin.Version;
                lblSocPInfo.Text = plugin.AdditionalInfo;
                pnsSocPluginDetails.Width = width;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckSocPlugUpdate_Click(object sender, RoutedEventArgs e)
        {
            checkForNewPluginsVersion();
        }

        private void cmdRemovePostPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                removePlugin(PluginType.Social, lstSocial.SelectedIndex);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPostCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_Loaded && cboPostCount.SelectedIndex > -1)
                {
                    _ParentWindow.TempSettings.Network.PostCount = Convert.ToInt32(cboPostCount.Items[cboPostCount.SelectedIndex]);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSyncPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var width = pnsSyncPluginDetails.ActualWidth;
                lblSyncPAuthor.Text = lblSyncPVersion.Text = lblSyncPInfo.Text = "";
                cmdRemoveSyncPlugin.IsEnabled = lstSyncPlugins.SelectedIndex > -1;
                if (lstSyncPlugins.SelectedIndex < 0) return;
                var item = lstSyncPlugins.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var plugin = item.Tag as ISyncPlugin;
                if (plugin != null)
                {
                    lblSyncPAuthor.Text = plugin.Author;
                    lblSyncPVersion.Text = plugin.Version;
                    lblSyncPInfo.Text = plugin.AdditionalInfo;
                }
                cmdSyncNow.IsEnabled =
                    lstSyncPlugins.Items.OfType<PNListBoxItem>()
                        .Any(it => it.IsChecked != null && it.IsChecked.Value);
                pnsSyncPluginDetails.Width = width;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckSyncPlugUpdate_Click(object sender, RoutedEventArgs e)
        {
            checkForNewPluginsVersion();
        }

        private void cmdRemoveSyncPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                removePlugin(PluginType.Sync, lstSyncPlugins.SelectedIndex);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdSyncNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstSyncPlugins.SelectedIndex <= -1) return;
                var item = lstSyncPlugins.Items[lstSyncPlugins.SelectedIndex] as PNListBoxItem;
                if (item == null) return;
                var plugin = PNPlugins.Instance.SyncPlugins.FirstOrDefault(p => p.Name == item.Text);
                if (plugin == null) return;
                var version = new Version(plugin.Version);
                if (version.Major >= 2 && version.Build >= 3)
                {
                    var ds = new WndSync(plugin) { Owner = _ParentWindow };
                    ds.ShowDialog();
                }
                else
                {
                    switch (plugin.Synchronize())
                    {
                        case SyncResult.None:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Syncronization completed successfully"), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case SyncResult.Reload:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Syncronization completed successfully. The program has to be restarted for applying all changes."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                            PNStatic.FormMain.ApplyAction(MainDialogAction.Restart, null);
                            break;
                        case SyncResult.AbortVersion:
                            PNMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            break;
                        case SyncResult.Error:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditSmtp.IsEnabled = cmdDeleteSmtp.IsEnabled = grdSmtp.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                editSmtp();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdMailContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = grdMailContacts.SelectedItems.Count > 0;
        }

        private void grdMailContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editMailContact();
        }

        private void smtpDlg_SmtpChanged(object sender, SmtpChangedEventArgs e)
        {
            try
            {
                if (e.Mode == AddEditMode.Add)
                {
                    if (_SmtpClients.Any(sm => sm.SenderAddress == e.Profile.SenderAddress))
                    {
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("smtp_same_address",
                                "There is already SMTP profile with the same address"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Accepted = false;
                        return;
                    }
                    e.Profile.Id = _SmtpClients.Any() ? _SmtpClients.Max(c => c.Id) + 1 : 0;
                    _SmtpClients.Add(e.Profile);
                    fillSmtpClients(false);
                }
                else
                {
                    if (
                        _SmtpClients.Any(
                            sm => sm.SenderAddress == e.Profile.SenderAddress && sm.Id != e.Profile.Id))
                    {
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("smtp_same_address",
                                "There is already SMTP profile with the same address"), PNStrings.PROG_NAME,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Accepted = false;
                        return;
                    }
                    var client = _SmtpClients.FirstOrDefault(c => c.Id == e.Profile.Id);
                    if (client == null) return;
                    client.HostName = e.Profile.HostName;
                    client.SenderAddress = e.Profile.SenderAddress;
                    client.Port = e.Profile.Port;
                    client.Password = e.Profile.Password;
                    client.DisplayName = e.Profile.DisplayName;
                    fillSmtpClients(false);
                }
                var d = sender as WndSmtp;
                if (d != null) d.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgMailContact_MailContactChanged(object sender, MailContactChangedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case AddEditMode.Add:
                        if (
                            _MailContacts.Any(
                                mc => mc.DisplayName == e.Contact.DisplayName && mc.Address == e.Contact.Address))
                        {
                            PNMessageBox.Show(
                                PNLang.Instance.GetMessageText("mail_contact_same_address",
                                    "There is already mail contact with the same name and address"), PNStrings.PROG_NAME,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            e.Accepted = false;
                            return;
                        }
                        e.Contact.Id = _MailContacts.Any() ? _MailContacts.Max(mc => mc.Id) + 1 : 0;
                        _MailContacts.Add(e.Contact);
                        fillMailContacts(false);
                        break;
                    case AddEditMode.Edit:
                        if (
                            _MailContacts.Any(
                                mc =>
                                    mc.DisplayName == e.Contact.DisplayName && mc.Address == e.Contact.Address &&
                                    mc.Id != e.Contact.Id))
                        {
                            PNMessageBox.Show(
                                PNLang.Instance.GetMessageText("mail_contact_same_address",
                                    "There is already mail contact with the same name and address"), PNStrings.PROG_NAME,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            e.Accepted = false;
                            return;
                        }
                        var contact = _MailContacts.FirstOrDefault(mc => mc.Id == e.Contact.Id);
                        if (contact == null) break;
                        contact.DisplayName = e.Contact.DisplayName;
                        contact.Address = e.Contact.Address;
                        fillMailContacts(false);
                        break;
                }
                var d = sender as WndMailContact;
                if (d == null) return;
                d.MailContactChanged -= dlgMailContact_MailContactChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtpCheck_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            var item = grdSmtp.GetObjectAtPoint<ListViewItem>(Mouse.GetPosition(grdSmtp)) as SSmtpClient;
            if (item == null) return;
            grdSmtp.SelectedItem = item;

            foreach (var smtp in _SmtpClients)
            {
                if (checkBox.IsChecked != null)
                    smtp.Active = smtp.SenderAddress == item.Address && checkBox.IsChecked.Value;
                else
                    smtp.Active = false;
            }
        }

        private void mnuImpOutlook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Outlook, _MailContacts,
                    mnuImpOutlook.Header.ToString()) { Owner = _ParentWindow };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImpGmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Gmail, _MailContacts,
                    mnuImpGmail.Header.ToString()) { Owner = _ParentWindow };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void mnuImpLotus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Lotus, _MailContacts,
                    mnuImpLotus.Header.ToString()) { Owner = _ParentWindow };
                dlgImport.ContactsImported += dlgImport_ContactsImported;
                var showDialog = dlgImport.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgImport.ContactsImported -= dlgImport_ContactsImported;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgImport_ContactsImported(object sender, ContactsImportedEventArgs e)
        {
            try
            {
                var d = sender as WndImportMailContacts;
                if (d != null) d.ContactsImported -= dlgImport_ContactsImported;
                var id = _MailContacts.Any() ? _MailContacts.Max(c => c.Id) + 1 : 0;
                foreach (var tc in e.Contacts.Where(tc => !_MailContacts.Any(c => c.DisplayName == tc.Item1 && c.Address == tc.Item2)))
                {
                    _MailContacts.Add(new PNMailContact { Id = id, DisplayName = tc.Item1, Address = tc.Item2 });
                    _MailContactsList.Add(new SMailContact(tc.Item1, tc.Item2, id));
                    id++;
                }
                cmdClearMailContacts.IsEnabled = _MailContactsList.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ctmImpContacts_Opened(object sender, RoutedEventArgs e)
        {
            //check for Office >= 2003
            mnuImpOutlook.IsEnabled = PNOffice.GetOfficeAppVersion(OfficeApp.Outlook) >= 11;
            //check for IBM Notes
            mnuImpLotus.IsEnabled = PNLotus.IsLotusInstalled();
        }


        private delegate void _TimerDelegate(object sender, ElapsedEventArgs e);
        private void _TimerConnections_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _TimerConnections.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    _TimerDelegate d = _TimerConnections_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                    catch (Exception ex)
                    {
                        PNStatic.LogException(ex);
                    }
                }
                else
                {
                    if (_WorkInProgress) return;
                    _WorkInProgress = true;
                    var bgw = new BackgroundWorker();
                    bgw.DoWork += bgw_DoWork;
                    bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
                    bgw.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (_UseTimerConnection)
                    _TimerConnections.Start();
            }
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _WorkInProgress = false;
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var sc in _ContactsList)
                    sc.ConnectionStatus = PNConnections.CheckContactConnection(sc.IpAddress);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (_Groups.Count > 0)
                {
                    newID = _Groups.Max(g => g.ID) + 1;
                }
                var dlgContactGroup = new WndGroups(newID) { Owner = _ParentWindow };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editContactsGroup();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteContactGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cg = getSelectedContactsGroup();
                if (cg == null) return;
                if (_Contacts.All(c => c.GroupID != cg.ID))
                {
                    var message = PNLang.Instance.GetMessageText("group_delete", "Delete selected group of contacts?");
                    if (
                        PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes) return;
                    _Groups.Remove(cg);
                    fillGroups(false);
                    return;
                }

                var dlg = new WndDeleteContactsGroup { Owner = _ParentWindow };
                var showDialog = dlg.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                switch (dlg.DeleteBehavior)
                {
                    case DeleteContactsGroupBehavior.DeleteAll:
                        //delete all contacts
                        _Contacts.RemoveAll(c => c.GroupID == cg.ID);
                        break;
                    case DeleteContactsGroupBehavior.Move:
                        //move all contacts to '(none)'
                        foreach (var c in _Contacts.Where(c => c.GroupID == cg.ID))
                        {
                            c.GroupID = -1;
                        }
                        break;
                }
                _Groups.Remove(cg);
                fillGroups(false);
                fillContacts(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newID = 0;
                if (_Contacts.Count > 0)
                {
                    newID = _Contacts.Max(c => c.ID) + 1;
                }
                var dlgContact = new WndContacts(newID, _Groups) { Owner = _ParentWindow };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgContact.ContactChanged -= dlgContact_ContactChanged;
                    return;
                }
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editContact();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cn = getSelectedContact();
                if (cn == null) return;
                var message = PNLang.Instance.GetMessageText("contact_delete", "Delete selected contact?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Contacts.Remove(cn);
                fillContacts(false);
                fillGroups(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddSmtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var smtpDlg = new WndSmtp(null) { Owner = _ParentWindow };
                smtpDlg.SmtpChanged += smtpDlg_SmtpChanged;
                var showDialog = smtpDlg.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    smtpDlg.SmtpChanged -= smtpDlg_SmtpChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditSmtp_Click(object sender, RoutedEventArgs e)
        {
            editSmtp();
        }

        private void cmdDeleteSmtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var profile = _SmtpClients.FirstOrDefault(sm => sm.Id == smtp.Id);
                if (profile == null) return;
                if (
                    PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("remove_smtp", "Remove selected SMTP profile?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _SmtpClients.Remove(profile);
                fillSmtpClients(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddMailContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgMailContact = new WndMailContact(null) { Owner = _ParentWindow };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditMailContact_Click(object sender, RoutedEventArgs e)
        {
            editMailContact();
        }

        private void cmdDeleteMailContact_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var contact = getSelectedMailContact();
                if (contact == null) return;
                if (
                    PNMessageBox.Show(
                        PNLang.Instance.GetMessageText("remove_mail_contact", "Remove selected mail contact?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _MailContacts.Remove(contact);
                fillMailContacts(false);
                cmdClearMailContacts.IsEnabled = grdMailContacts.Items.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdClearMailContacts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _MailContactsList.Clear();
                _MailContacts.Clear();
                cmdClearMailContacts.IsEnabled = cmdEditMailContact.IsEnabled = cmdDeleteMailContact.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdImportMailContact_Click(object sender, RoutedEventArgs e)
        {
            ctmImpContacts.IsOpen = true;
        }

        private void ipServer_FieldChanged(object sender, PNIPBox.FieldChangedEventArgs e)
        {
            try
            {
                if (!ipServer.IsAnyBlank)
                {
                    _ParentWindow.TempSettings.Network.ServerIp = ipServer.Text;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtServerPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (txtServerPort.Value > 0)
                {
                    _ParentWindow.TempSettings.Network.ServerPort = Convert.ToInt32(txtServerPort.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clientRunner = new PNWCFClientRunner();
                var result = clientRunner.CheckServer(_ParentWindow.TempSettings.Network.ServerIp, _ParentWindow.TempSettings.Network.ServerPort,
                    PNServerConstants.NET_SERVICE_NAME, _ParentWindow.TempSettings.Network.SendTimeout);
                var message = "";
                if (result == PNServerConstants.SUCCESS)
                {
                    message = PNLang.Instance.GetMessageText("server_check_success", "Test connection succeeded");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    message = PNLang.Instance.GetMessageText("server_check_failed", "Test connection failed");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdCheckForNotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clientRunner = new PNWCFClientRunner();
                Task.Factory.StartNew(
                    () => clientRunner.CheckMessages(_ParentWindow.TempSettings.Network.ServerIp, _ParentWindow.TempSettings.Network.ServerPort));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.Network.SendTimeout = Convert.ToInt32(updTimeout.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion
    }
}

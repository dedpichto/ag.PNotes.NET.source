﻿// PNotes.NET - open source desktop notes manager
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

using Microsoft.Win32;
using PluginsCore;
using PNCommon;
using PNotes.NET.Annotations;
using PNRichEdit;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using WPFStandardStyles;
using Path = System.IO.Path;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSettings.xaml
    /// </summary>
    public partial class WndSettings
    {
        public WndSettings()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private bool _Loaded;
        private bool _ChangeLanguage = true;
        private bool _SetFontEnabled = true;

        private readonly List<ScrollViewer> _Panels = new List<ScrollViewer>();
        private readonly List<RadioButton> _Radios = new List<RadioButton>();

        private PNSettings _TempSettings;

        #region Internal procedures

        internal void PanelAutohideChanged()
        {
            try
            {
                chkPanelAutoHide.IsChecked =
                    _TempSettings.Behavior.PanelAutoHide = PNRuntimes.Instance.Settings.Behavior.PanelAutoHide;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void PanelOrientationChanged()
        {
            try
            {
                cboPanelDock.SelectedIndex = (int)PNRuntimes.Instance.Settings.Behavior.NotesPanelOrientation;
                _TempSettings.Behavior.NotesPanelOrientation = PNRuntimes.Instance.Settings.Behavior.NotesPanelOrientation;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Private procedures

        private void fadePanels(ScrollViewer panelHide, ScrollViewer panelShow)
        {
            try
            {
                if (!(TryFindResource("FadeInOut") is Storyboard storyBoard)) return;
                storyBoard.Children[0].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                storyBoard.Children[1].SetValue(Storyboard.TargetNameProperty, panelHide.Name);
                storyBoard.Children[2].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                storyBoard.Children[3].SetValue(Storyboard.TargetNameProperty, panelShow.Name);
                storyBoard.Begin();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyFirstTimeLanguage()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(navBar);
                PNLang.Instance.ApplyControlLanguage(navWarnings);
                PNLang.Instance.ApplyControlLanguage(navButtons);

                foreach (var pnl in _Panels)
                {
                    applyPanelLanguage(pnl, pnl.Visibility == Visibility.Visible);
                }

                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";

                _PanelOrientations[0].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllLeft", "Left");
                _PanelOrientations[1].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllTop", "Top");
                _PanelRemoves[0].Name = PNLang.Instance.GetCaptionText("panel_remove_single", "Single click");
                _PanelRemoves[1].Name = PNLang.Instance.GetCaptionText("panel_remove_double", "Double click");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguage(bool checkName = true)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(navBar);
                PNLang.Instance.ApplyControlLanguage(navWarnings);
                PNLang.Instance.ApplyControlLanguage(navButtons);

                var pnl = _Panels.FirstOrDefault(p => p.Visibility == Visibility.Visible);
                if (pnl != null)
                {
                    applyPanelLanguage(pnl, checkName);
                }

                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";

                _PanelOrientations[0].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllLeft", "Left");
                _PanelOrientations[1].Name = PNLang.Instance.GetMenuText("main_menu", "mnuDAllTop", "Top");
                _PanelRemoves[0].Name = PNLang.Instance.GetCaptionText("panel_remove_single", "Single click");
                _PanelRemoves[1].Name = PNLang.Instance.GetCaptionText("panel_remove_double", "Double click");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyPanelLanguage(IFrameworkInputElement pnl, bool checkName = true)
        {
            try
            {
                var spnl = PNCollections.Instance.Panels.FirstOrDefault(p => p.Name == pnl.Name);
                if (spnl == null) return;
                if (spnl.Language == PNRuntimes.Instance.Settings.GeneralSettings.Language && checkName) return;
                PNLang.Instance.ApplyControlLanguage(pnl);
                if (spnl.Name == pnlGeneral.Name)
                {
                    //special cases
                    if (cboDeleteBin.Items.Count > 0)
                    {
                        var index = cboDeleteBin.SelectedIndex;
                        cboDeleteBin.Items.RemoveAt(0);
                        cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never"));
                        cboDeleteBin.SelectedIndex = index;
                    }
                    _ButtonSizes[0].Name = PNLang.Instance.GetCaptionText("b_size_normal", "Normal");
                    _ButtonSizes[1].Name = PNLang.Instance.GetCaptionText("b_size_large", "Large");
                    if (cboButtonsSize.Items.Count > 0)
                    {
                        var index = cboButtonsSize.SelectedIndex;
                        cboButtonsSize.Items.Clear();
                        foreach (var bs in _ButtonSizes)
                        {
                            cboButtonsSize.Items.Add(bs);
                        }
                        cboButtonsSize.SelectedIndex = index;
                    }
                }
                else if (pnl.Name == pnlProtection.Name)
                {
                    var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                    var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                    foreach (var c in daysChecks)
                    {
                        c.Content = ci.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)Convert.ToInt32(c.Tag));
                    }
                }
                else if (pnl.Name == pnlSchedule.Name)
                {
                    fillDaysOfWeek();
                    if (_TempSettings != null && _TempSettings.Schedule != null)
                    {
                        var current = _TempSettings.Schedule.FirstDayOfWeek;
                        var cbitem = cboDOW.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (DayOfWeek)i.Tag == current);
                        if (cbitem != null)
                            cboDOW.SelectedItem = cbitem;
                    }
                }
                spnl.Language = PNRuntimes.Instance.Settings.GeneralSettings.Language;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string getSelectedPanelText()
        {
            try
            {
                var opt = _Radios.FirstOrDefault(r => r.IsChecked != null && r.IsChecked.Value);
                if (!(opt?.Content is StackPanel st)) return "";
                foreach (var c in st.Children.OfType<TextBlock>())
                    return c.Text;
                return "";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void prepareLists()
        {
            try
            {
                //_ButtonSizes.Add(new _ButtonSize { ButtonsSize = ToolStripButtonSize.Normal, Name = "Normal" });
                //_ButtonSizes.Add(new _ButtonSize { ButtonsSize = ToolStripButtonSize.Large, Name = "Large" });

                if (PNCollections.Instance.Panels.Count == 0)
                {
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlGeneral.Name, Language = "" });
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlSchedule.Name, Language = "" });
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlAppearance.Name, Language = "" });
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlBehavior.Name, Language = "" });
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlNetwork.Name, Language = "" });
                    PNCollections.Instance.Panels.Add(new SettingsPanel { Name = pnlProtection.Name, Language = "" });
                }
                else
                {
                    foreach (var p in PNCollections.Instance.Panels)
                        p.Language = "";
                }
                _Panels.Add(pnlGeneral);
                _Panels.Add(pnlSchedule);
                _Panels.Add(pnlAppearance);
                _Panels.Add(pnlBehavior);
                _Panels.Add(pnlNetwork);
                _Panels.Add(pnlProtection);

                _Radios.Add(cmdGeneralSettings);
                _Radios.Add(cmdScheduleSettings);
                _Radios.Add(cmdAppearanceSettings);
                _Radios.Add(cmdBehaviorSettings);
                _Radios.Add(cmdNetworkSettings);
                _Radios.Add(cmdProtectionSettings);

                _Radios[PNRuntimes.Instance.Settings.Config.LastPage].IsChecked = true;

                _Panels[PNRuntimes.Instance.Settings.Config.LastPage].Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void allowToHideTrayIcon()
        {
            try
            {
                if (!_TempSettings.Protection.HideTrayIcon) return;
                var hk = PNCollections.Instance.HotKeysMain.FirstOrDefault(h => h.MenuName == "mnuLockProg");
                if (hk == null || hk.Shortcut != "") return;
                var message = PNLang.Instance.GetMessageText("hide_on_lock_warning",
                    "In order to allow the tray icon to be hidden when program is locked you have to set a hot key for \"Lock Program\" menu item");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveCollections()
        {
            try
            {
                if (PNCollections.Instance.SyncComps.Inequals(_SyncComps))
                {
                    PNCollections.Instance.SyncComps = new List<PNSyncComp>(_SyncComps.Select(sc => sc.Clone()));
                    PNData.SaveSyncComps();
                }
                if (PNCollections.Instance.ContactGroups.Inequals(_Groups))
                {
                    PNCollections.Instance.ContactGroups = new List<PNContactGroup>(_Groups.Select(g => g.Clone()));
                    PNData.SaveContactGroups();
                }
                if (PNCollections.Instance.Contacts.Inequals(_Contacts))
                {
                    PNCollections.Instance.Contacts = new List<PNContact>(_Contacts.Select(c => c.Clone()));
                    PNData.SaveContacts();
                }
                if (PNCollections.Instance.Externals.Inequals(_Externals))
                {
                    PNCollections.Instance.Externals = new List<PNExternal>(_Externals.Select(ext => ext.Clone()));
                    PNData.SaveExternals();
                }
                if (PNCollections.Instance.SearchProviders.Inequals(_SProviders))
                {
                    PNCollections.Instance.SearchProviders =
                        new List<PNSearchProvider>(_SProviders.Select(sp => sp.Clone()));
                    PNData.SaveSearchProviders();
                }
                if (PNCollections.Instance.SmtpProfiles.Inequals(_SmtpClients))
                {
                    PNCollections.Instance.SmtpProfiles = new List<PNSmtpProfile>(_SmtpClients.Select(c => c.Clone()));
                    PNData.SaveSmtpClients();
                }
                if (PNCollections.Instance.MailContacts.Inequals(_MailContacts))
                {
                    PNCollections.Instance.MailContacts = new List<PNMailContact>(_MailContacts.Select(c => c.Clone()));
                    PNData.SaveMailContacts();
                }
                if (PNCollections.Instance.Tags.Inequals(_Tags))
                {
                    if (PNNotesOperations.ResetNotesTags(_Tags))
                    {
                        PNCollections.Instance.Tags = new List<string>(_Tags);
                        PNData.SaveTags();
                    }
                }
                if (PNCollections.Instance.ActivePostPlugins.Inequals(_SocialPlugins))
                {
                    PNCollections.Instance.ActivePostPlugins = new List<string>(_SocialPlugins);
                    PNData.SaveSocialPlugins();
                }
                if (PNCollections.Instance.ActiveSyncPlugins.Inequals(_SyncPlugins))
                {
                    PNCollections.Instance.ActiveSyncPlugins =new List<string>( _SyncPlugins);
                    PNData.SaveSyncPlugins();
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void restoreDefaultValues()
        {
            try
            {
                if (
                    WPFMessageBox.Show(
                        PNLang.Instance.GetMessageText("def_warning",
                            "You are about to reset ALL program settings to their default values. Continue?"),
                        @"PNotes.NET", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                {
                    //preserve language
                    var language = PNRuntimes.Instance.Settings.GeneralSettings.Language;
                    //preserve password
                    var password = PNRuntimes.Instance.Settings.Protection.PasswordString;

                    _TempSettings.Dispose();
                    _TempSettings = new PNSettings
                    {
                        GeneralSettings = { Language = language },
                        Protection = { PasswordString = password }
                    };

                    initPageGeneral(false);
                    initPageSchedule(false);
                    initPageBehavior(false);
                    initPageNetwork(false);
                    initPageProtection(false);
                    initPageAppearance(false);

                    ipServer.Clear();
                    if (chkDefSettIncGroups.IsChecked != null && chkDefSettIncGroups.IsChecked.Value)
                    {
                        foreach (var treeItem in tvwGroups.Items.OfType<PNTreeItem>())
                            restoreAllGroupsDelaults(treeItem);
                    }
                    //remove active smtp client
                    foreach (var sm in _SmtpClients)
                        sm.Active = false;
                    chkNoSplash.IsChecked = false;
                    restoreFontUIClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool applyCanExecute()
        {
            try
            {
                if (File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH)) &&
                    (chkNoSplash.IsChecked == null || !chkNoSplash.IsChecked.Value))
                    return true;
                if (!File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH)) &&
                    chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                    return true;
                if (PNRuntimes.Instance.Settings != _TempSettings)
                    return true;
                if (_ChangedGroups.Any())
                    return true;
                if (PNCollections.Instance.SyncComps.Inequals(_SyncComps))
                    return true;
                if (PNCollections.Instance.ContactGroups.Inequals(_Groups))
                    return true;
                if (PNCollections.Instance.Contacts.Inequals(_Contacts))
                    return true;
                if (PNCollections.Instance.Externals.Inequals(_Externals))
                    return true;
                if (PNCollections.Instance.SearchProviders.Inequals(_SProviders))
                    return true;
                if (PNCollections.Instance.SmtpProfiles.Inequals(_SmtpClients))
                    return true;
                if (PNCollections.Instance.MailContacts.Inequals(_MailContacts))
                    return true;
                if (PNCollections.Instance.Tags.Inequals(_Tags))
                    return true;
                if (PNCollections.Instance.ActivePostPlugins.Inequals(_SocialPlugins))
                    return true;
                if (PNCollections.Instance.ActiveSyncPlugins.Inequals(_SyncPlugins))
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private ChangesAction applyChanges()
        {
            try
            {
                var result = ChangesAction.None;
                var changeDockSize = false;

                if (_TempSettings.Network.StoreOnServer && (ipServer.IsAnyBlank || txtServerPort.Value == 0))
                {
                    WPFMessageBox.Show(
                        PNLang.Instance.GetMessageText("no_server_props", "You must specify server IP address and port"),
                        PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return ChangesAction.None;
                }

                // hide tray icon checking
                allowToHideTrayIcon();

                var splashFile = Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH);
                if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                {
                    if (!File.Exists(splashFile))
                    {
                        using (new StreamWriter(splashFile, false))
                        {
                        }
                    }
                }
                else
                {
                    if (File.Exists(splashFile))
                    {
                        File.Delete(splashFile);
                    }
                }

                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins != _TempSettings.GeneralSettings.UseSkins)
                {
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        if (Directory.Exists(PNPaths.Instance.SkinsDir))
                        {
                            var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                            var fi = di.GetFiles("*.pnskn");
                            if (fi.Length > 0)
                            {
                                foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                                {
                                    setDefaultSkin(n, fi[0].FullName);
                                }
                            }
                        }
                    }
                }

                var changedSkins = new List<int>();
                foreach (var treeItem in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    checkAndApplyGroupChanges(treeItem, changedSkins);
                }

                //collections
                if (!saveCollections())
                {
                    return ChangesAction.None;
                }

                //settings
                if (PNRuntimes.Instance.Settings != _TempSettings)
                {
                    //general settings
                    if (PNRuntimes.Instance.Settings.GeneralSettings != _TempSettings.GeneralSettings)
                    {
                        if (!saveGeneral(ref result, ref changeDockSize)) return ChangesAction.None;
                    }
                    //schedule settings
                    if (PNRuntimes.Instance.Settings.Schedule != _TempSettings.Schedule)
                    {
                        PNRuntimes.Instance.Settings.Schedule = _TempSettings.Schedule.Clone();
                        PNData.SaveScheduleSettings();
                    }
                    //behavior
                    if (PNRuntimes.Instance.Settings.Behavior != _TempSettings.Behavior)
                    {
                        if (!saveBehavior(ref result)) return ChangesAction.None;
                    }
                    //network
                    if (PNRuntimes.Instance.Settings.Network != _TempSettings.Network)
                    {
                        if (!saveNetwork()) return ChangesAction.None;
                    }
                    //protection
                    if (PNRuntimes.Instance.Settings.Protection != _TempSettings.Protection)
                    {
                        if (!saveProtection()) return ChangesAction.None;
                    }
                    //diary
                    if (PNRuntimes.Instance.Settings.Diary != _TempSettings.Diary)
                    {
                        PNRuntimes.Instance.Settings.Diary =_TempSettings.Diary.Clone();
                        PNData.SaveDiarySettings();
                    }
                }

                if ((result & ChangesAction.SkinsReload) != ChangesAction.SkinsReload)
                {
                    if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && changedSkins.Count > 0)
                    {
                        // change skins for notes if we don't have to reload them
                        var notes = PNCollections.Instance.Notes.Where(n => n.Visible);
                        foreach (var n in notes)
                        {
                            if (n.Dialog != null && n.Skin == null)
                            {
                                PNSkinsOperations.ApplyNoteSkin(n.Dialog, n);
                            }
                        }
                    }
                    //else if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && (result & ChangesAction.ChangeDockSize) == ChangesAction.ChangeDockSize)
                    else if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && changeDockSize)
                    {
                        PNNotesOperations.ChangeDockSize();
                    }
                }
                _ChangedGroups.Clear();

                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return ChangesAction.None;
            }
        }

        private void promptToRestart()
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("confirm_restart_1",
                    "In order new settings to take effect you have to restart the program.");
                message += '\n';
                message += PNLang.Instance.GetMessageText("confirm_restart_2",
                    "Press 'Yes' to restart it now, or 'No' to restart later.");
                if (
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                        MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void reloadAllOnSkinsChanges()
        {
            try
            {
                var visible = PNCollections.Instance.Notes.Where(n => n.Visible);
                var notes = new List<PNote>();
                foreach (var n in visible)
                {
                    notes.Add(n);
                    PNNotesOperations.ShowHideSpecificNote(n, false);
                }
                foreach (var n in notes)
                {
                    PNNotesOperations.ShowHideSpecificNote(n, true);
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
                if (!string.IsNullOrEmpty(PNRuntimes.Instance.Settings.GeneralSettings.Language))
                    return;
                //fill only if there is no language setting
                foreach (var s in _DoubleSingleActions)
                {
                    cboDblAction.Items.Add(s);
                    cboSingleAction.Items.Add(s);
                }
                foreach (var s in _DefNames)
                {
                    cboDefName.Items.Add(s);
                }
                foreach (var s in _PinsUnpins)
                {
                    cboPinClick.Items.Add(s);
                }
                foreach (var s in _StartPos)
                {
                    cboNoteStartPosition.Items.Add(s);
                }
                foreach (var s in _SyncPeriods)
                {
                    cboLocalSyncPeriod.Items.Add(s);
                    cboSyncPeriod.Items.Add(s);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Event handlers

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            if (_ChangeLanguage)
            {
                applyLanguage(false);
            }
        }

        #endregion

        #region Window staff

        private void DlgSettings_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNWindows.Instance.FormSettings = this;
                prepareLists();
                initializeComboBoxes();

                var f = new PNFont();
                _SetFontEnabled = f == PNSingleton.Instance.FontUser;

                applyFirstTimeLanguage();
                //applyLanguage();

                PNWindows.Instance.FormMain.LanguageChanged += FormMain_LanguageChanged;

                _TempSettings = PNRuntimes.Instance.Settings.Clone();
                _SyncComps = new List<PNSyncComp>(PNCollections.Instance.SyncComps.Select(c => c.Clone()));

                _Groups = new List<PNContactGroup>(PNCollections.Instance.ContactGroups.Select(g => g.Clone()));
                _Contacts = new List<PNContact>(PNCollections.Instance.Contacts.Select(c => c.Clone()));

                _Externals = new List<PNExternal>(PNCollections.Instance.Externals.Select(ext => ext.Clone()));
                _SProviders =
                    new List<PNSearchProvider>(PNCollections.Instance.SearchProviders.Select(sp => sp.Clone()));
                _SmtpClients = new List<PNSmtpProfile>(PNCollections.Instance.SmtpProfiles.Select(p => p.Clone()));
                _MailContacts = new List<PNMailContact>(PNCollections.Instance.MailContacts.Select(c => c.Clone()));

                _Tags = new List<string>(PNCollections.Instance.Tags);
                _TempDocking = PNRuntimes.Instance.Docking.Clone();
                _SocialPlugins = new List<string>(PNCollections.Instance.ActivePostPlugins);
                _SyncPlugins = new List<string>(PNCollections.Instance.ActiveSyncPlugins);

                initPageGeneral(true);
                initPageSchedule(true);
                initPageAppearance(true);
                initPageBehavior(true);
                initPageNetwork(true);
                initPageProtection(true);

                _TimerConnections.Elapsed += _TimerConnections_Elapsed;
                if (_TempSettings.Network.EnableExchange)
                    _TimerConnections.Start();

                FlowDirection = PNLang.Instance.GetFlowDirection();

                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSettings_Closed(object sender, EventArgs e)
        {
            try
            {
                PNWindows.Instance.FormSettings = null;
                _UseTimerConnection = false;
                _TimerConnections.Stop();
                PNWindows.Instance.FormMain.LanguageChanged -= FormMain_LanguageChanged;
                PNData.SaveLastPage();
                foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    cleanUpGroups(n);
                }

            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgSettings_Activated(object sender, EventArgs e)
        {
            try
            {
                PNStatic.DeactivateNotesWindows();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Navigation buttons staff

        private void OptionButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Radios.Count == 0) return;
                if (!(sender is RadioButton opt)) return;
                var indexHide = PNRuntimes.Instance.Settings.Config.LastPage;
                PNRuntimes.Instance.Settings.Config.LastPage = _Radios.IndexOf(opt);
                var indexShow = PNRuntimes.Instance.Settings.Config.LastPage;
                applyPanelLanguage(_Panels[PNRuntimes.Instance.Settings.Config.LastPage]);
                Title = PNLang.Instance.GetControlText("DlgSettings", "Preferences") + @" [" + getSelectedPanelText() +
                        @"]";
                var pnlHide = _Panels[indexHide];
                var pnlShow = _Panels[indexShow];
                fadePanels(pnlHide, pnlShow);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Common buttons staff

        private void defClick()
        {
            try
            {
                _InDefSettingsClick = true;
                restoreDefaultValues();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDefSettingsClick = false;
            }
        }

        private void saveClick()
        {
            try
            {
                _ChangeLanguage = false;
                var changes = applyChanges();
                if ((changes & ChangesAction.Restart) == ChangesAction.Restart)
                {
                    promptToRestart();
                }
                if ((changes & ChangesAction.SkinsReload) == ChangesAction.SkinsReload)
                {
                    PNSingleton.Instance.InSkinReload = true;
                    reloadAllOnSkinsChanges();
                    PNSingleton.Instance.InSkinReload = false;
                }
                Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyClick()
        {
            try
            {
                var changes = applyChanges();
                if ((changes & ChangesAction.Restart) == ChangesAction.Restart)
                {
                    promptToRestart();
                }
                if ((changes & ChangesAction.SkinsReload) == ChangesAction.SkinsReload)
                {
                    PNSingleton.Instance.InSkinReload = true;
                    reloadAllOnSkinsChanges();
                    PNSingleton.Instance.InSkinReload = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region General staff
        private class SettingsLanguage
        {
            public string LangName;
            public string LangFile;
            public string Culture;

            public override string ToString()
            {
                return LangName;
            }
        }

        private class ButtonSize
        {
            public ToolStripButtonSize ButtonsSize;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public class SProvider
        {
            public string Name { get; }
            public string Query { get; }

            public SProvider(string n, string q)
            {
                Name = n;
                Query = q;
            }
        }

        public class SExternal
        {
            public string Name { get; }
            public string Prog { get; }
            public string CommLine { get; }

            public SExternal(string n, string p, string c)
            {
                Name = n;
                Prog = p;
                CommLine = c;
            }
        }
        //_ButtonSizes.Add(new _ButtonSize { ButtonsSize = ToolStripButtonSize.Normal, Name = "Normal" });
        //_ButtonSizes.Add(new _ButtonSize { ButtonsSize = ToolStripButtonSize.Large, Name = "Large" });
        private readonly List<ButtonSize> _ButtonSizes =
            new List<ButtonSize>
            {
                new ButtonSize {ButtonsSize = ToolStripButtonSize.Normal, Name = "Normal"},
                new ButtonSize {ButtonsSize = ToolStripButtonSize.Large, Name = "Large"}
            };
        private List<string> _Tags;
        private List<PNExternal> _Externals;
        private List<PNSearchProvider> _SProviders;
        private readonly ObservableCollection<SProvider> _SearchProvidersList = new ObservableCollection<SProvider>();
        private readonly ObservableCollection<SExternal> _ExternalList = new ObservableCollection<SExternal>();

        internal bool ExternalExists(string extName)
        {
            return _Externals.Any(e => e.Name == extName);
        }

        internal void ExternalAdd(PNExternal ext)
        {
            try
            {
                _Externals.Add(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ExternalReplace(PNExternal ext)
        {
            try
            {
                var e = _Externals.FirstOrDefault(ex => ex.Name == ext.Name);
                if (e == null) return;
                e.Program = ext.Program;
                e.CommandLine = ext.CommandLine;
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool SearchProviderExists(string spName)
        {
            return _SProviders.Any(s => s.Name == spName);
        }

        internal void SearchProviderAdd(PNSearchProvider sp)
        {
            try
            {
                _SProviders.Add(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SearchProviderReplace(PNSearchProvider sp)
        {
            try
            {
                var s = _SProviders.FirstOrDefault(spv => spv.Name == sp.Name);
                if (s != null)
                {
                    s.QueryString = sp.QueryString;
                }
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveGeneral(ref ChangesAction result, ref bool changeDockSize)
        {
            try
            {
                var applyAutoHeight = false;
                if (PNRuntimes.Instance.Settings.GeneralSettings.Language != _TempSettings.GeneralSettings.Language)
                {
                    PNWindows.Instance.FormMain.ApplyNewLanguage(_TempSettings.GeneralSettings.Language);
                }
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins != _TempSettings.GeneralSettings.UseSkins)
                {
                    //apply or remove skins
                    PNRuntimes.Instance.Settings.GeneralSettings.UseSkins = _TempSettings.GeneralSettings.UseSkins;
                    result |= ChangesAction.SkinsReload;
                }
                // hide toolbar
                if (PNRuntimes.Instance.Settings.GeneralSettings.HideToolbar != _TempSettings.GeneralSettings.HideToolbar &&
                    _TempSettings.GeneralSettings.HideToolbar)
                {
                    PNNotesOperations.ApplyHideToolbar();
                }
                // hide or show delete button
                if (PNRuntimes.Instance.Settings.GeneralSettings.HideDeleteButton !=
                    _TempSettings.GeneralSettings.HideDeleteButton)
                {
                    PNNotesOperations.ApplyDeleteButtonVisibility(!_TempSettings.GeneralSettings.HideDeleteButton);
                }
                // change hide to delete
                if (PNRuntimes.Instance.Settings.GeneralSettings.ChangeHideToDelete !=
                    _TempSettings.GeneralSettings.ChangeHideToDelete)
                {
                    PNNotesOperations.ApplyUseAlternative(_TempSettings.GeneralSettings.ChangeHideToDelete);
                }
                // hide or show hide button
                if (PNRuntimes.Instance.Settings.GeneralSettings.HideHideButton !=
                    _TempSettings.GeneralSettings.HideHideButton)
                {
                    PNNotesOperations.ApplyHideButtonVisibility(!_TempSettings.GeneralSettings.HideHideButton);
                }
                // scroll bars
                if (PNRuntimes.Instance.Settings.GeneralSettings.ShowScrollbar !=
                    _TempSettings.GeneralSettings.ShowScrollbar)
                {
                    if (_TempSettings.GeneralSettings.ShowScrollbar == System.Windows.Forms.RichTextBoxScrollBars.None)
                        PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                    else
                    {
                        if (!_TempSettings.GeneralSettings.AutoHeight)
                            PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                    }
                }
                // auto height
                if (PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight != _TempSettings.GeneralSettings.AutoHeight)
                {
                    // auto height after
                    if (_TempSettings.GeneralSettings.AutoHeight)
                    {
                        // scroll bars after (and may be before)
                        if (_TempSettings.GeneralSettings.ShowScrollbar !=
                            System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // remove scroll bars
                            PNNotesOperations.ApplyShowScrollBars(System.Windows.Forms.RichTextBoxScrollBars.None);
                        }
                        // apply auto height
                        applyAutoHeight = true;
                    }
                    else
                    {
                        // scroll bars after (and may be before)
                        if (_TempSettings.GeneralSettings.ShowScrollbar !=
                            System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // restore scroll bars
                            PNNotesOperations.ApplyShowScrollBars(_TempSettings.GeneralSettings.ShowScrollbar);
                        }
                    }
                }
                // buttons size
                if (PNRuntimes.Instance.Settings.GeneralSettings.ButtonsSize != _TempSettings.GeneralSettings.ButtonsSize)
                {
                    PNNotesOperations.ApplyButtonsSize(_TempSettings.GeneralSettings.ButtonsSize);
                }
                // custom fonts
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseCustomFonts !=
                    _TempSettings.GeneralSettings.UseCustomFonts)
                {
                    if (_TempSettings.GeneralSettings.UseCustomFonts)
                    {
                        PNInterop.AddCustomFonts();
                    }
                    else
                    {
                        PNInterop.RemoveCustomFonts();
                    }
                }
                // margins
                if (PNRuntimes.Instance.Settings.GeneralSettings.MarginWidth != _TempSettings.GeneralSettings.MarginWidth)
                {
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    {
                        PNNotesOperations.ApplyMarginsWidth(_TempSettings.GeneralSettings.MarginWidth);
                    }
                }

                // docked notes width and/or height
                if (PNRuntimes.Instance.Settings.GeneralSettings.DockWidth != _TempSettings.GeneralSettings.DockWidth ||
                    PNRuntimes.Instance.Settings.GeneralSettings.DockHeight != _TempSettings.GeneralSettings.DockHeight)
                {
                    changeDockSize = true;
                }

                // spell check color
                if (PNRuntimes.Instance.Settings.GeneralSettings.SpellColor != _TempSettings.GeneralSettings.SpellColor)
                {
                    if (Spellchecking.Initialized)
                    {
                        Spellchecking.ColorUnderlining = _TempSettings.GeneralSettings.SpellColor;
                        PNNotesOperations.ApplySpellColor();
                    }
                }
                // autosave
                if (PNRuntimes.Instance.Settings.GeneralSettings.Autosave != _TempSettings.GeneralSettings.Autosave)
                {
                    if (_TempSettings.GeneralSettings.Autosave)
                    {
                        PNWindows.Instance.FormMain.TimerAutosave.Interval =
                            _TempSettings.GeneralSettings.AutosavePeriod * 60000;
                        PNWindows.Instance.FormMain.TimerAutosave.Start();
                    }
                    else
                    {
                        PNWindows.Instance.FormMain.TimerAutosave.Stop();
                    }
                }
                else
                {
                    if (PNRuntimes.Instance.Settings.GeneralSettings.AutosavePeriod !=
                        _TempSettings.GeneralSettings.AutosavePeriod)
                    {
                        if (PNRuntimes.Instance.Settings.GeneralSettings.Autosave)
                        {
                            PNWindows.Instance.FormMain.TimerAutosave.Stop();
                            PNWindows.Instance.FormMain.TimerAutosave.Interval =
                                _TempSettings.GeneralSettings.AutosavePeriod * 60000;
                            PNWindows.Instance.FormMain.TimerAutosave.Start();
                        }
                    }
                }
                // clean bin
                if (PNRuntimes.Instance.Settings.GeneralSettings.RemoveFromBinPeriod !=
                    _TempSettings.GeneralSettings.RemoveFromBinPeriod)
                {
                    if (_TempSettings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // stop timer
                        PNWindows.Instance.FormMain.TimerCleanBin.Stop();
                    }
                    else if (PNRuntimes.Instance.Settings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // start timer
                        PNWindows.Instance.FormMain.TimerCleanBin.Start();
                    }
                }

                //create or delete shortcut
                var shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +
                                      PNStrings.SHORTCUT_FILE;
                if (PNRuntimes.Instance.Settings.GeneralSettings.RunOnStart != _TempSettings.GeneralSettings.RunOnStart)
                {
                    if (_TempSettings.GeneralSettings.RunOnStart)
                    {
                        //create shortcut
                        if (!File.Exists(shortcutFile))
                        {
                            using (var link = new PNShellLink())
                            {
                                link.ShortcutFile = shortcutFile;
                                link.Target = System.Windows.Forms.Application.ExecutablePath;
                                link.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                                link.IconPath = System.Windows.Forms.Application.ExecutablePath;
                                link.IconIndex = 0;
                                link.Save();
                            }
                        }
                    }
                    else
                    {
                        //delete shortcut
                        if (File.Exists(shortcutFile))
                        {
                            File.Delete(shortcutFile);
                        }
                    }
                }
                //date format
                if (PNRuntimes.Instance.Settings.GeneralSettings.DateFormat != _TempSettings.GeneralSettings.DateFormat)
                {
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongDatePattern =
                        Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern =
                            _TempSettings.GeneralSettings.DateFormat;
                }
                //time format
                if (PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat != _TempSettings.GeneralSettings.TimeFormat)
                {
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern =
                        _TempSettings.GeneralSettings.TimeFormat;
                    Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern =
                        PNStatic.StripDateTimeFormat(_TempSettings.GeneralSettings.TimeFormat, 's');
                }
                PNRuntimes.Instance.Settings.GeneralSettings = _TempSettings.GeneralSettings.Clone();
                if (applyAutoHeight)
                {
                    PNNotesOperations.ApplyAutoHeight();
                }
                PNData.SaveGeneralSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void initPageGeneral(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    addLanguages();
                    fillSearchProviders();
                    fillExternals();
                    fillTags();
                    loadRemoveFromBinPeriods();
                }
                chkHideToolbar.IsChecked = _TempSettings.GeneralSettings.HideToolbar;
                chkCustomFont.IsChecked = _TempSettings.GeneralSettings.UseCustomFonts;
                chkHideDelete.IsChecked = _TempSettings.GeneralSettings.HideDeleteButton;
                chkChangeHideToDelete.IsChecked = _TempSettings.GeneralSettings.ChangeHideToDelete;
                chkHideHide.IsChecked = _TempSettings.GeneralSettings.HideHideButton;
                cboIndent.SelectedItem = (int)_TempSettings.GeneralSettings.BulletsIndent;
                cboParIndent.SelectedItem = _TempSettings.GeneralSettings.ParagraphIndent;
                cboMargins.SelectedItem = (int)_TempSettings.GeneralSettings.MarginWidth;
                txtDTFShort.Text = _TempSettings.GeneralSettings.DateFormat;
                txtTFLong.Text = _TempSettings.GeneralSettings.TimeFormat;
                chkSaveOnExit.IsChecked = _TempSettings.GeneralSettings.SaveOnExit;
                chkConfirmDelete.IsChecked = _TempSettings.GeneralSettings.ConfirmBeforeDeletion;
                chkConfirmSave.IsChecked = _TempSettings.GeneralSettings.ConfirmSaving;
                chkSaveWithoutConfirm.IsChecked = _TempSettings.GeneralSettings.SaveWithoutConfirmOnHide;
                chkAutosave.IsChecked = _TempSettings.GeneralSettings.Autosave;
                updAutosave.Value = _TempSettings.GeneralSettings.AutosavePeriod;
                cboDeleteBin.SelectedIndex = _TempSettings.GeneralSettings.RemoveFromBinPeriod > 0
                    ? cboDeleteBin.Items.IndexOf(
                        _TempSettings.GeneralSettings.RemoveFromBinPeriod.ToString(
                            PNRuntimes.Instance.CultureInvariant))
                    : 0;
                chkWarnBeforeEmptyBin.IsChecked = _TempSettings.GeneralSettings.WarnOnAutomaticalDelete;
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
                chkRunOnStart.IsChecked = _TempSettings.GeneralSettings.RunOnStart;
                chkShowCPOnStart.IsChecked = _TempSettings.GeneralSettings.ShowCPOnStart;
                chkCheckNewVersionOnStart.IsChecked = _TempSettings.GeneralSettings.CheckNewVersionOnStart;
                chkCheckCriticalOnStart.IsChecked = _TempSettings.GeneralSettings.CheckCriticalOnStart;
                chkCheckCriticalPeriodically.IsChecked = _TempSettings.GeneralSettings.CheckCriticalPeriodically;
                chkShowPriority.IsChecked = _TempSettings.GeneralSettings.ShowPriorityOnStart;
                txtWidthSknlsDef.Value = _TempSettings.GeneralSettings.Width;
                txtHeightSknlsDef.Value = _TempSettings.GeneralSettings.Height;
                pckSpell.SelectedColor = Color.FromArgb(_TempSettings.GeneralSettings.SpellColor.A,
                    _TempSettings.GeneralSettings.SpellColor.R, _TempSettings.GeneralSettings.SpellColor.G,
                    _TempSettings.GeneralSettings.SpellColor.B);
                foreach (var bs in _ButtonSizes)
                {
                    cboButtonsSize.Items.Add(bs);
                }
                for (var i = 0; i < cboButtonsSize.Items.Count; i++)
                {
                    if (!(cboButtonsSize.Items[i] is ButtonSize bs) ||
                        bs.ButtonsSize != _TempSettings.GeneralSettings.ButtonsSize) continue;
                    cboButtonsSize.SelectedIndex = i;
                    break;
                }
                for (var i = 0; i < cboScrollBars.Items.Count; i++)
                {
                    var scb = (System.Windows.Forms.RichTextBoxScrollBars)i;
                    if (_TempSettings.GeneralSettings.ShowScrollbar != scb) continue;
                    cboScrollBars.SelectedIndex = i;
                    break;
                }
                chkAutomaticSmilies.IsChecked = _TempSettings.GeneralSettings.AutomaticSmilies;
                updSpace.Value = _TempSettings.GeneralSettings.SpacePoints;
                chkNoSplash.IsChecked = File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH));
                chkRestoreAutomatically.IsChecked = _TempSettings.GeneralSettings.RestoreAuto;
                chkAutoHeight.IsChecked = _TempSettings.GeneralSettings.AutoHeight;
                chkDeleteShortExit.IsChecked = _TempSettings.GeneralSettings.DeleteShortcutsOnExit;
                chkRestoreShortStart.IsChecked = _TempSettings.GeneralSettings.RestoreShortcutsOnStart;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadRemoveFromBinPeriods()
        {
            try
            {
                cboDeleteBin.Items.Clear();
                for (var i = 1; i <= 10; i++)
                {
                    cboDeleteBin.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }
                cboDeleteBin.Items.Add("20");
                cboDeleteBin.Items.Add("30");
                cboDeleteBin.Items.Add("60");
                cboDeleteBin.Items.Add("120");
                cboDeleteBin.Items.Add("360");
                cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never)"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addLanguages()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.LangDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.LangDir);
                var files = di.GetFiles("*.xml").OrderBy(f => f.Name);
                foreach (var fi in files)
                {
                    var xd = XDocument.Load(fi.FullName, LoadOptions.PreserveWhitespace);
                    var xAttribute = xd.Root?.Attribute("culture");
                    if (xAttribute == null) continue;
                    var ci = new CultureInfo(xAttribute.Value);
                    var name = ci.NativeName;
                    name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                    cboLanguage.Items.Add(new SettingsLanguage { LangName = name, LangFile = fi.Name, Culture = ci.Name });
                }
                for (var i = 0; i < cboLanguage.Items.Count; i++)
                {
                    if (!(cboLanguage.Items[i] is SettingsLanguage ln) ||
                        ln.Culture != PNLang.Instance.GetLanguageCulture()) continue;
                    cboLanguage.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillTags()
        {
            try
            {
                lstTags.Items.Clear();
                foreach (var t in _Tags)
                    lstTags.Items.Add(t);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillExternals(bool firstTime = true)
        {
            try
            {
                _ExternalList.Clear();
                foreach (var ext in _Externals)
                {
                    _ExternalList.Add(new SExternal(ext.Name, ext.Program, ext.CommandLine));
                }
                if (firstTime)
                    grdExternals.ItemsSource = _ExternalList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSearchProviders(bool firstTime = true)
        {
            try
            {
                _SearchProvidersList.Clear();
                foreach (var sp in _SProviders)
                {
                    _SearchProvidersList.Add(new SProvider(sp.Name, sp.QueryString));
                }
                if (firstTime)
                    grdSearchProvs.ItemsSource = _SearchProvidersList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSearchProvider getSelectedSearchProvider()
        {
            try
            {
                return grdSearchProvs.SelectedItem is SProvider sp
                    ? _SProviders.FirstOrDefault(p => p.Name == sp.Name)
                    : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNExternal getSelectedExternal()
        {
            try
            {
                return grdExternals.SelectedItem is SExternal ext
                    ? _Externals.FirstOrDefault(e => e.Name == ext.Name)
                    : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSearchProvider(PNSearchProvider sp)
        {
            try
            {
                if (sp == null) return;
                var dsp = new WndSP(this, sp) { Owner = this };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editExternal(PNExternal ext)
        {
            try
            {
                if (ext == null) return;
                var dex = new WndExternals(this, ext) { Owner = this };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLanguage.SelectedIndex <= -1) return;
                if (cboLanguage.Items[cboLanguage.SelectedIndex] is SettingsLanguage lang)
                    _TempSettings.GeneralSettings.Language = lang.LangFile;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addLangClick()
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_LANGS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setFontUIClick()
        {
            try
            {
                var fd = new WndFontChooser(PNSingleton.Instance.FontUser) { Owner = this };
                var showDialog = fd.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                PNSingleton.Instance.FontUser.FontFamily = fd.SelectedFont.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = fd.SelectedFont.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = fd.SelectedFont.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = fd.SelectedFont.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = fd.SelectedFont.FontWeight;
                PNData.SaveFontUi();
                _SetFontEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreFontUIClick()
        {
            try
            {
                var f = new PNFont();
                PNSingleton.Instance.FontUser.FontFamily = f.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = f.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = f.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = f.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = f.FontWeight;
                PNData.SaveFontUi();
                _SetFontEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckGeneral_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkRunOnStart":
                        _TempSettings.GeneralSettings.RunOnStart = cb.IsChecked.Value;
                        break;
                    case "chkShowCPOnStart":
                        _TempSettings.GeneralSettings.ShowCPOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckNewVersionOnStart":
                        _TempSettings.GeneralSettings.CheckNewVersionOnStart = cb.IsChecked.Value;
                        break;
                    case "chkHideToolbar":
                        _TempSettings.GeneralSettings.HideToolbar = cb.IsChecked.Value;
                        break;
                    case "chkCustomFont":
                        _TempSettings.GeneralSettings.UseCustomFonts = cb.IsChecked.Value;
                        break;
                    case "chkHideDelete":
                        _TempSettings.GeneralSettings.HideDeleteButton = cb.IsChecked.Value;
                        if (chkChangeHideToDelete.IsChecked != null &&
                            (!cb.IsChecked.Value && chkChangeHideToDelete.IsChecked.Value))
                        {
                            chkChangeHideToDelete.IsChecked = false;
                        }
                        break;
                    case "chkHideHide":
                        _TempSettings.GeneralSettings.HideHideButton = cb.IsChecked.Value;
                        break;
                    case "chkChangeHideToDelete":
                        _TempSettings.GeneralSettings.ChangeHideToDelete = cb.IsChecked.Value;
                        break;
                    case "chkAutosave":
                        _TempSettings.GeneralSettings.Autosave = cb.IsChecked.Value;
                        break;
                    case "chkSaveOnExit":
                        _TempSettings.GeneralSettings.SaveOnExit = cb.IsChecked.Value;
                        break;
                    case "chkConfirmSave":
                        _TempSettings.GeneralSettings.ConfirmSaving = cb.IsChecked.Value;
                        break;
                    case "chkConfirmDelete":
                        _TempSettings.GeneralSettings.ConfirmBeforeDeletion = cb.IsChecked.Value;
                        break;
                    case "chkSaveWithoutConfirm":
                        _TempSettings.GeneralSettings.SaveWithoutConfirmOnHide = cb.IsChecked.Value;
                        break;
                    case "chkWarnBeforeEmptyBin":
                        _TempSettings.GeneralSettings.WarnOnAutomaticalDelete = cb.IsChecked.Value;
                        break;
                    case "chkShowPriority":
                        _TempSettings.GeneralSettings.ShowPriorityOnStart = cb.IsChecked.Value;
                        break;
                    case "chkAutomaticSmilies":
                        _TempSettings.GeneralSettings.AutomaticSmilies = cb.IsChecked.Value;
                        break;
                    case "chkRestoreAutomatically":
                        _TempSettings.GeneralSettings.RestoreAuto = cb.IsChecked.Value;
                        break;
                    case "chkAutoHeight":
                        _TempSettings.GeneralSettings.AutoHeight = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalOnStart":
                        _TempSettings.GeneralSettings.CheckCriticalOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalPeriodically":
                        _TempSettings.GeneralSettings.CheckCriticalPeriodically = cb.IsChecked.Value;
                        break;
                    case "chkDeleteShortExit":
                        _TempSettings.GeneralSettings.DeleteShortcutsOnExit = cb.IsChecked.Value;
                        break;
                    case "chkRestoreShortStart":
                        _TempSettings.GeneralSettings.RestoreShortcutsOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCloseOnShortcut":
                        _TempSettings.GeneralSettings.CloseOnShortcut = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void newVersionClick()
        {
            try
            {
                var updater = new PNUpdateChecker();
                updater.NewVersionFound += updater_PNNewVersionFound;
                updater.IsLatestVersion += updater_PNIsLatestVersion;
                updater.CheckNewVersion(System.Windows.Forms.Application.ProductVersion);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNIsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker updater)
                {
                    updater.IsLatestVersion -= updater_PNIsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("latest_version",
                    "You are using the latest version of PNotes.NET.");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNNewVersionFound(object sender, NewVersionFoundEventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker updater)
                {
                    updater.NewVersionFound -= updater_PNNewVersionFound;
                }
                var message =
                    PNLang.Instance.GetMessageText("new_version_1",
                            "New version of PNotes.NET is available - %PLACEHOLDER1%.")
                        .Replace(PNStrings.PLACEHOLDER1, e.Version);
                message += "\n";
                message += PNLang.Instance.GetMessageText("new_version_2",
                    "Click 'OK' in order to instal new version (restart of program is required).");
                if (
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OKCancel,
                        MessageBoxImage.Information) !=
                    MessageBoxResult.OK) return;
                if (PNStatic.PrepareNewVersionCommandLine())
                {
                    PNWindows.Instance.FormMain.Close();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtWidthSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.Width = (int)txtWidthSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtHeightSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.Height = (int)txtHeightSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboButtonsSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboButtonsSize.SelectedIndex <= -1) return;
                if (cboButtonsSize.Items[cboButtonsSize.SelectedIndex] is ButtonSize bs)
                {
                    _TempSettings.GeneralSettings.ButtonsSize = bs.ButtonsSize;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboIndent.SelectedIndex > -1 &&
                    (int)cboIndent.SelectedItem != _TempSettings.GeneralSettings.BulletsIndent)
                {
                    _TempSettings.GeneralSettings.BulletsIndent = Convert.ToInt16(cboIndent.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboMargins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboMargins.SelectedIndex > -1 &&
                    (int)cboMargins.SelectedItem != _TempSettings.GeneralSettings.MarginWidth)
                {
                    _TempSettings.GeneralSettings.MarginWidth = Convert.ToInt16(cboMargins.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboParIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboParIndent.SelectedIndex > -1 &&
                    (int)cboParIndent.SelectedItem != _TempSettings.GeneralSettings.ParagraphIndent)
                {
                    _TempSettings.GeneralSettings.ParagraphIndent = (int)cboParIndent.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckSpell_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.SpellColor = System.Drawing.Color.FromArgb(pckSpell.SelectedColor.A,
                        pckSpell.SelectedColor.R, pckSpell.SelectedColor.G, pckSpell.SelectedColor.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updSpace_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.SpacePoints = Convert.ToInt32(updSpace.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updAutosave_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _TempSettings.GeneralSettings.AutosavePeriod = Convert.ToInt32(updAutosave.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDeleteBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (cboDeleteBin.SelectedIndex > -1)
                {
                    _TempSettings.GeneralSettings.RemoveFromBinPeriod = cboDeleteBin.SelectedIndex == 0
                        ? 0
                        : Convert.ToInt32(cboDeleteBin.SelectedItem);
                }
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtDTFShort.Text.Trim().Length == 0) return;
                var format = txtDTFShort.Text.Trim().Length > 1
                    ? txtDTFShort.Text.Trim().Replace("/", "'/'")
                    : "%" + txtDTFShort.Text.Trim().Replace("/", "'/'");
                _TempSettings.GeneralSettings.DateFormat = format;
                lblDTShort.Text = DateTime.Now.ToString(_TempSettings.GeneralSettings.DateFormat);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.D:
                    case Key.Y:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                        break;
                    case Key.Oem2:
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.M:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tFShortClick()
        {
            try
            {
                WPFMessageBox.Show(PNLang.Instance.GetDateFormatsText(),
                    PNLang.Instance.GetCaptionText("date_formats", "Possible date formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtTFLong.Text.Trim().Length == 0) return;
                var format = txtTFLong.Text.Trim().Length > 1 ? txtTFLong.Text.Trim() : "%" + txtTFLong.Text.Trim();
                _TempSettings.GeneralSettings.TimeFormat = format;
                lblTFLong.Text = DateTime.Now.ToString(_TempSettings.GeneralSettings.TimeFormat);
            }
            catch (FormatException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.M:
                    case Key.S:
                    case Key.F:
                    case Key.T:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                    case Key.H:
                        break;
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.Oem1:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tFLongClick()
        {
            try
            {
                WPFMessageBox.Show(PNLang.Instance.GetTimeFormatsText(),
                    PNLang.Instance.GetCaptionText("time_formats", "Possible time formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSearchProvs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdSearchProvs.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdSearchProvs)) is SProvider item)) return;
                editSearchProvider(_SProviders.FirstOrDefault(p => p.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdExternals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdExternals.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdExternals)) is SExternal item)) return;
                editExternal(_Externals.FirstOrDefault(ext => ext.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboScrollBars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.ShowScrollbar =
                    (System.Windows.Forms.RichTextBoxScrollBars)cboScrollBars.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addProvClick()
        {
            try
            {
                var dsp = new WndSP(this) { Owner = this };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editProvClick()
        {
            try
            {
                editSearchProvider(getSelectedSearchProvider());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteProvClick()
        {
            try
            {
                var sp = getSelectedSearchProvider();
                if (sp == null) return;
                var message = PNLang.Instance.GetMessageText("sp_delete", "Delete selected serach provider?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _SProviders.Remove(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addExtClick()
        {
            try
            {
                var dex = new WndExternals(this) { Owner = this };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editExtClick()
        {
            try
            {
                editExternal(getSelectedExternal());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteExtClick()
        {
            try
            {
                var ext = getSelectedExternal();
                if (ext == null) return;
                var message = PNLang.Instance.GetMessageText("ext_delete", "Delete selected external program?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Externals.Remove(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addTagClick()
        {
            try
            {
                if (_Tags.Contains(txtTag.Text.Trim()))
                {
                    var message = PNLang.Instance.GetMessageText("tag_exists", "The same tag already exists");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTag.SelectAll();
                    txtTag.Focus();
                }
                else
                {
                    lstTags.Items.Add(txtTag.Text.Trim());
                    _Tags.Add(txtTag.Text.Trim());
                    txtTag.Text = "";
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteTagClick()
        {
            try
            {
                if (lstTags.SelectedIndex < 0) return;
                var message = PNLang.Instance.GetMessageText("tag_delete", "Delete selected tag?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Tags.Remove((string)lstTags.SelectedItem);
                lstTags.Items.Remove(lstTags.SelectedItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Schedule staff

        private void initPageSchedule(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    var image = TryFindResource("loudspeaker") as BitmapImage;
                    // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                    clbSounds.Items.Add(
                        new PNListBoxItem(image, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND));
                    if (Directory.Exists(PNPaths.Instance.SoundsDir))
                    {
                        var fi = new DirectoryInfo(PNPaths.Instance.SoundsDir).GetFiles("*.wav");
                        foreach (var f in fi)
                        {
                            clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name),
                                f.FullName));
                        }
                    }
                    foreach (var s in PNCollections.Instance.Voices)
                    {
                        lstVoices.Items.Add(new PNListBoxItem(image, s, s));
                    }

                    var stopAfter = _TempSettings.Schedule.StopAfter / 1000;
                    cboStopAlert.SelectedIndex = PNStatic.StopValues.ContainsValue(stopAfter)
                        ? PNStatic.StopValues.FirstOrDefault(sv => sv.Value == stopAfter).Key
                        : 0;
                }
                chkAllowSound.IsChecked = _TempSettings.Schedule.AllowSoundAlert;
                chkVisualNotify.IsChecked = _TempSettings.Schedule.VisualNotification;
                chkTrackOverdue.IsChecked = _TempSettings.Schedule.TrackOverdue;
                chkCenterScreen.IsChecked = _TempSettings.Schedule.CenterScreen;
                var item =
                    clbSounds.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _TempSettings.Schedule.Sound);
                if (item != null)
                    clbSounds.SelectedItem = item;
                else
                {
                    clbSounds.SelectedIndex = 0;
                    _TempSettings.Schedule.Sound = PNSchedule.DEF_SOUND;
                }
                if (PNCollections.Instance.Voices.Count > 0)
                {

                    item = lstVoices.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _TempSettings.Schedule.Voice);
                    if (item != null)
                    {
                        lstVoices.SelectedItem = item;
                    }
                    else
                    {
                        lstVoices.SelectedIndex = 0;
                    }
                    txtVoiceSample.Text = "";
                }
                trkVolume.Value = _TempSettings.Schedule.VoiceVolume;
                trkSpeed.Value = _TempSettings.Schedule.VoiceSpeed;
                optDOWStandard.IsChecked = _TempSettings.Schedule.FirstDayOfWeekType == FirstDayOfWeekType.Standard;
                optDOWCustom.IsChecked = _TempSettings.Schedule.FirstDayOfWeekType != FirstDayOfWeekType.Standard;
                var current = _TempSettings.Schedule.FirstDayOfWeek;
                var cbitem = cboDOW.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (DayOfWeek)i.Tag == current);
                if (cbitem != null)
                    cboDOW.SelectedItem = cbitem;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckSchedule_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAllowSound":
                        _TempSettings.Schedule.AllowSoundAlert = cb.IsChecked.Value;
                        break;
                    case "chkVisualNotify":
                        _TempSettings.Schedule.VisualNotification = cb.IsChecked.Value;
                        break;
                    case "chkCenterScreen":
                        _TempSettings.Schedule.CenterScreen = cb.IsChecked.Value;
                        break;
                    case "chkTrackOverdue":
                        _TempSettings.Schedule.TrackOverdue = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clbSounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (clbSounds.SelectedIndex < 0) return;
                if (clbSounds.SelectedItem is PNListBoxItem item)
                    _TempSettings.Schedule.Sound = item.Text;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void defVoiceClick()
        {
            try
            {
                if (lstVoices.SelectedIndex < 0) return;
                if (!(lstVoices.Items[lstVoices.SelectedIndex] is PNListBoxItem voice)) return;
                if (_TempSettings.Schedule.Voice == voice.Text) return;
                _TempSettings.Schedule.Voice = voice.Text;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void voiceSampleClick()
        {
            try
            {
                if (!(lstVoices.Items[lstVoices.SelectedIndex] is PNListBoxItem voice)) return;
                var voiceName = voice.Text;
                PNStatic.SpeakText(txtVoiceSample.Text.Trim(), voiceName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!(sender is Slider slider)) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _TempSettings.Schedule.VoiceVolume = (int)trkVolume.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!(sender is Slider slider)) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _TempSettings.Schedule.VoiceSpeed = (int)trkSpeed.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addSoundClick()
        {
            try
            {
                var addString = true;
                var ofd = new OpenFileDialog
                {
                    Filter = @"Windows audio files|*.wav",
                    Title = PNLang.Instance.GetCaptionText("choose_sound", "Choose sound")
                };
                if (!ofd.ShowDialog(this).Value) return;
                if (!Directory.Exists(PNPaths.Instance.SoundsDir))
                    Directory.CreateDirectory(PNPaths.Instance.SoundsDir);
                if (File.Exists(PNPaths.Instance.SoundsDir + @"\" + Path.GetFileName(ofd.FileName)))
                {
                    if (
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("sound_exists",
                                "The file already exists in your 'sounds' directory. Copy anyway?"),
                            PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    addString = false;
                }
                var path2 = Path.GetFileName(ofd.FileName);
                File.Copy(ofd.FileName, Path.Combine(PNPaths.Instance.SoundsDir, path2), true);
                if (!addString) return;
                var image = TryFindResource("loudspeaker") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(ofd.FileName),
                    ofd.FileName));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeSoundClick()
        {
            try
            {
                if (WPFMessageBox.Show(PNLang.Instance.GetMessageText("sound_delete", "Delete selected sound?"),
                        PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                var index = clbSounds.SelectedIndex;
                if (!(clbSounds.SelectedItem is PNListBoxItem item)) return;
                var sound = item.Text;
                clbSounds.SelectedIndex = clbSounds.SelectedIndex - 1;
                clbSounds.Items.RemoveAt(index);
                File.Delete(Path.Combine(PNPaths.Instance.SoundsDir, sound + ".wav"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void listenSoundClick()
        {
            try
            {
                if (_TempSettings.Schedule.Sound == PNSchedule.DEF_SOUND)
                {
                    PNSound.PlayDefaultSound();
                }
                else
                {
                    PNSound.PlaySound(Path.Combine(PNPaths.Instance.SoundsDir, _TempSettings.Schedule.Sound + ".wav"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillDaysOfWeek()
        {
            try
            {
                cboDOW.Items.Clear();
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                foreach (DayOfWeek dw in Enum.GetValues(typeof(DayOfWeek)))
                {
                    cboDOW.Items.Add(new ComboBoxItem { Content = ci.DateTimeFormat.DayNames[(int)dw], Tag = dw });
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionDowChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Schedule.FirstDayOfWeekType = optDOWStandard.IsChecked != null && optDOWStandard.IsChecked.Value ? FirstDayOfWeekType.Standard : FirstDayOfWeekType.User;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDOW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (!(cboDOW.SelectedItem is ComboBoxItem item)) return;
                _TempSettings.Schedule.FirstDayOfWeek = (DayOfWeek)item.Tag;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private void CboStopAlert_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var index = cboStopAlert.SelectedIndex;
                if (index <= -1 || !PNStatic.StopValues.ContainsKey(index)) return;
                _TempSettings.Schedule.StopAfter = PNStatic.StopValues[index];
                if (_TempSettings.Schedule.StopAfter > 0)
                {
                    _TempSettings.Schedule.StopAfter *= 1000;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Appearance staff

        private PNGroup _TempDocking;
        private readonly List<int> _ChangedGroups = new List<int>();
        private bool _InTreeGroupsSelectionChanged;

        internal void GroupAddEdit(PNGroup group, int id, AddEditMode mode)
        {
            try
            {
                var treeItem = getTreeItemByGroupId(id, null);
                switch (mode)
                {
                    case AddEditMode.Add:
                        insertGroupToTree(group, treeItem);
                        break;
                    case AddEditMode.Edit:
                        if (treeItem == null) return;
                        replaceGroupOnTree(group, treeItem);
                        if (treeItem.IsSelected)
                            groupSelected(group);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void GroupDelete(int id)
        {
            try
            {
                var treeItem = getTreeItemByGroupId(id, null);
                if (treeItem == null) return;
                if (treeItem.ItemParent == null)
                    tvwGroups.Items.Remove(treeItem);
                else
                    treeItem.ItemParent.Items.Remove(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageAppearance(bool firstTime)
        {
            try
            {
                if (!PNStatic.CheckSkinsExistance()) optSkinnable.IsEnabled = false;
                if (_TempSettings.GeneralSettings.UseSkins)
                {
                    optSkinnable.IsChecked = true;
                }
                else
                {
                    optSkinless.IsChecked = true;
                }
                txtDockWidth.Value = _TempSettings.GeneralSettings.DockWidth;
                txtDockHeight.Value = _TempSettings.GeneralSettings.DockHeight;
                chkAddWeekdayName.IsChecked = _TempSettings.Diary.AddWeekday;
                chkFullWeekdayName.IsChecked = _TempSettings.Diary.FullWeekdayName;
                chkWeekdayAtTheEnd.IsChecked = _TempSettings.Diary.WeekdayAtTheEnd;
                chkNoPreviousDiary.IsChecked = _TempSettings.Diary.DoNotShowPrevious;
                chkDiaryAscOrder.IsChecked = _TempSettings.Diary.AscendingOrder;

                cboNumberOfDiaries.SelectedItem = _TempSettings.Diary.NumberOfPages;
                cboDiaryNaming.SelectedItem = _TempSettings.Diary.DateFormat;
                if (firstTime)
                {
                    //_TempGroups = new List<PNGroup>();
                    //foreach (var group in PNCollections.Instance.Groups)
                    //{
                    //    addGroupToTemp(group, null);
                    //}
                    //var gd = PNCollections.Instance.Groups.GetGroupByID(Convert.ToInt32(SpecialGroups.Diary));
                    //if (gd != null)
                    //{
                    //    addGroupToTemp(gd, null);
                    //}
                    //addGroupToTemp(_TempDocking, null);

                    //foreach (var g in _TempGroups[0].Subgroups.OrderBy(gr => gr.Name))
                    //{
                    //    addGroupToTree(g, null);
                    //}

                    foreach (var g in PNCollections.Instance.Groups[0].Subgroups.OrderBy(gr => gr.Name))
                    {
                        addGroupToTree(g, null);
                    }
                    var gd = PNCollections.Instance.Groups.GetGroupById(Convert.ToInt32(SpecialGroups.Diary));
                    if (gd != null)
                    {
                        addGroupToTree(gd, null);
                    }
                    addGroupToTree(_TempDocking, null);
                    loadSkinsList();
                    loadLogFonts();
                    lstThemes.ItemsSource = PNCollections.Instance.Themes.Keys;
                }
                ((TreeViewItem)tvwGroups.Items[0]).IsSelected = true;
                if (PNCollections.Instance.Themes.Keys.Contains(_TempSettings.Behavior.Theme))
                {
                    lstThemes.SelectedItem =
                        lstThemes.Items.OfType<string>().FirstOrDefault(s => s == _TempSettings.Behavior.Theme);
                }
                else
                {
                    lstThemes.SelectedIndex = 0;
                }
                if (firstTime)
                {
                    cboFontColor.IsDropDownOpen = true;
                    cboFontColor.IsDropDownOpen = false;
                    cboFontSize.IsDropDownOpen = true;
                    cboFontSize.IsDropDownOpen = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cleanUpGroups(PNTreeItem node)
        {
            try
            {
                var group = node.Tag as PNGroup;
                if (group != null)
                {
                    group.Dispose();
                }
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    cleanUpGroups(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreAllGroupsDelaults(PNTreeItem treeItem)
        {
            try
            {
                foreach (var ti in treeItem.Items.OfType<PNTreeItem>())
                    restoreAllGroupsDelaults(ti);
                restoreGroupDefaults(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkAndApplyGroupChanges(PNTreeItem node, List<int> changedSkins)
        {
            try
            {
                var isIconChanged = false;
                var gr = node.Tag as PNGroup;
                if (gr == null) return;
                var rg = PNCollections.Instance.Groups.GetGroupById(gr.Id);
                if (rg != null)
                {
                    if (gr != rg)
                    {
                        if (!Equals(gr.Image, rg.Image))
                            isIconChanged = true;
                        if (gr.Skin.SkinName != rg.Skin.SkinName)
                        {
                            changedSkins.Add(gr.Id);
                        }
                        gr.CopyTo(rg);
                        PNData.SaveGroupChanges(rg);
                        if (isIconChanged && PNWindows.Instance.FormCP != null)
                            PNWindows.Instance.FormCP.GroupIconChanged(rg.Id);
                    }
                    foreach (var n in node.Items.OfType<PNTreeItem>())
                    {
                        checkAndApplyGroupChanges(n, changedSkins);
                    }
                }
                else
                {
                    if (gr.Id != (int)SpecialGroups.Docking) return;
                    if (gr == PNRuntimes.Instance.Docking) return;
                    changedSkins.Add((int)SpecialGroups.Docking);
                    gr.CopyTo(PNRuntimes.Instance.Docking);
                    PNData.SaveGroupChanges(PNRuntimes.Instance.Docking);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setDefaultSkin(PNTreeItem node, string path)
        {
            try
            {
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    setDefaultSkin(n, path);
                }
                var gr = node.Tag as PNGroup;
                if (gr == null || gr.Skin.SkinName != PNSkinDetails.NO_SKIN) return;
                gr.Skin.SkinName = Path.GetFileNameWithoutExtension(path);
                PNSkinsOperations.LoadSkin(path, gr.Skin);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertGroupToTree(PNGroup group, ItemsControl parentItem)
        {
            try
            {
                var temp = @group.Clone();
                group.CopyTo(temp);

                var n = new PNTreeItem(temp.Image, temp.Name, temp);

                var inserted = false;
                if (parentItem == null) parentItem = tvwGroups;
                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    if (!(parentItem.Items[i] is PNTreeItem pni)) continue;
                    var gr = pni.Tag as PNGroup;
                    if (gr == null) continue;
                    if (string.Compare(gr.Name, group.Name, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        parentItem.Items.Insert(i, n);
                        inserted = true;
                        break;
                    }
                    if (i < parentItem.Items.Count - 1)
                    {
                        pni = parentItem.Items[i + 1] as PNTreeItem;
                        if (pni == null) continue;
                        gr = pni.Tag as PNGroup;
                        if (gr == null) continue;
                        if (gr.Id < (int)SpecialGroups.AllGroups)
                        {
                            parentItem.Items.Insert(i + 1, n);
                            inserted = true;
                            break;
                        }
                    }
                }
                if (!inserted)
                {
                    parentItem.Items.Add(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void replaceGroupOnTree(PNGroup group, PNTreeItem node)
        {
            try
            {
                var temp = @group.Clone();
                group.CopyTo(temp);
                node.Image = temp.Image;
                node.Text = temp.Name;
                node.Tag = temp;
                ItemsControl parentItem;
                if (node.ItemParent != null)
                    parentItem = node.ItemParent;
                else
                    parentItem = tvwGroups;

                var isSelected = node.IsSelected;

                parentItem.Items.Remove(node);
                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    if (!(parentItem.Items[i] is PNTreeItem ti)) continue;
                    var gr = ti.Tag as PNGroup;
                    if (gr == null) continue;
                    if (gr.Id < (int)SpecialGroups.AllGroups)
                    {
                        parentItem.Items.Insert(i, node);
                        if (isSelected)
                            node.IsSelected = true;
                        return;
                    }
                    if (string.Compare(gr.Name, node.Text, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        parentItem.Items.Insert(i, node);
                        if (isSelected)
                            node.IsSelected = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNTreeItem getTreeItemByGroupId(int groupId, ItemsControl root)
        {
            try
            {
                if (root == null) root = tvwGroups;
                PNTreeItem result = null;
                foreach (var treeItem in root.Items.OfType<PNTreeItem>())
                {
                    var group = treeItem.Tag as PNGroup;
                    if (group == null) continue;
                    if (group.Id == groupId)
                    {
                        result = treeItem;
                        break;
                    }
                    result = getTreeItemByGroupId(groupId, treeItem);
                    if (result != null)
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void restoreGroupDefaults(PNTreeItem treeItem)
        {
            try
            {
                var pnGroup = treeItem.Tag as PNGroup;
                if (pnGroup == null) return;
                var gr = pnGroup.Clone();
                pnGroup.Clear();
                pnGroup.Name = gr.Id == 0 ? PNLang.Instance.GetGroupName("general", "General") : gr.Name;
                pnGroup.Id = gr.Id;
                pnGroup.ParentId = gr.ParentId;
                var image = TryFindResource("gr") as BitmapImage;
                pnGroup.Image = image;
                pnGroup.IsDefaultImage = true;
                if (treeItem.IsSelected)
                    groupSelected(pnGroup);

                if (!Equals(treeItem.Image, pnGroup.Image))
                {
                    if (!pnGroup.IsDefaultImage)
                        treeItem.Image = pnGroup.Image;
                    else
                        treeItem.SetImageResource(pnGroup.ImageName);
                }

                compareGroups(pnGroup);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNGroup selectedGroup()
        {
            try
            {
                if (tvwGroups.SelectedItem is PNTreeItem item)
                    return item.Tag as PNGroup;
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void groupSelected(PNGroup gr)
        {
            try
            {
                pvwSkl.SetProperties(gr);
                pckBGSknls.SelectedColor = gr.Skinless.BackColor;
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == gr.Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedIndex = -1;
                }
                cboFonts.SelectedItem =
                    cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == gr.Font.lfFaceName);
                foreach (var t in from object t in cboFontColor.Items
                                  let rc = t as Rectangle
                                  where rc != null
                                  let sb = rc.Fill as SolidColorBrush
                                  where sb != null
                                  where
                                  sb.Color ==
                                  Color.FromArgb(gr.FontColor.A, gr.FontColor.R, gr.FontColor.G,
                                      gr.FontColor.B)
                                  select t)
                {
                    cboFontColor.SelectedItem = t;
                    break;
                }
                var fontSize = gr.Font.GetFontSize();
                if (cboFontSize.Items.OfType<int>().Any(i => i == fontSize))
                    cboFontSize.SelectedItem = fontSize;
                else
                    cboFontSize.SelectedIndex = 0;
                imgGroupIcon.Source = gr.Image;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSkinsList()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                var fi = di.GetFiles("*.pnskn");
                var image = TryFindResource("skins") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
                foreach (var f in fi)
                {
                    lstSkins.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name), f.FullName));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadLogFonts()
        {
            try
            {
                var list = new List<LOGFONT>();
                PNStaticFonts.Fonts.GetFontsList(list);
                var ordered = list.OrderBy(f => f.lfFaceName);
                foreach (var lf in ordered)
                {
                    cboFonts.Items.Add(lf);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addGroupToTree(PNGroup group, PNTreeItem node)
        {
            try
            {
                var temp = @group.Clone();
                group.CopyTo(temp);

                var n = new PNTreeItem(temp.Image, temp.Name, temp);
                if (node == null)
                {
                    tvwGroups.Items.Add(n);
                }
                else
                {
                    node.Items.Add(n);
                }
                foreach (var g in group.Subgroups.OrderBy(gr => gr.Name))
                {
                    addGroupToTree(g, n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optSkinnable.IsChecked != null)
                    _TempSettings.GeneralSettings.UseSkins = optSkinnable.IsChecked.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                _InTreeGroupsSelectionChanged = true;
                if (!(e.NewValue is PNTreeItem item))
                    return;
                var gr = item.Tag as PNGroup;
                if (gr == null)
                    return;
                switch (gr.Id)
                {
                    case (int)SpecialGroups.Diary:
                        pnsDiaryCust.IsEnabled = true;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = true;
                        //stkDockCust.IsEnabled = false;
                        break;
                    case (int)SpecialGroups.Docking:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = true;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = true;
                        break;
                    default:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = false;
                        break;
                }
                groupSelected(gr);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InTreeGroupsSelectionChanged = false;
            }
        }

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                gr.Skinless.BackColor = e.NewValue;
                pvwSkl.SetProperties(gr);
                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fontSknlsClick()
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                var fc = new WndFontChooser(gr.Skinless.CaptionFont, gr.Skinless.CaptionColor) { Owner = this };
                var showDialog = fc.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    gr.Skinless.CaptionFont = fc.SelectedFont;
                    gr.Skinless.CaptionColor = fc.SelectedColor;
                    pvwSkl.SetProperties(gr);
                }
                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSkins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lstSkins.SelectedIndex < 0)
                {
                    pvwSkin.EraseProperties();
                    return;
                }

                if (!(lstSkins.SelectedItem is PNListBoxItem item)) return;
                var gr = selectedGroup();
                if (gr == null) return;
                if (gr.Skin.SkinName != item.Text)
                {
                    gr.Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, gr.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, gr.Skin);
                    }
                }
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lblMoreSkins_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_SKINS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAddWeekdayName":
                        _TempSettings.Diary.AddWeekday = cb.IsChecked.Value;
                        break;
                    case "chkFullWeekdayName":
                        _TempSettings.Diary.FullWeekdayName = cb.IsChecked.Value;
                        break;
                    case "chkWeekdayAtTheEnd":
                        _TempSettings.Diary.WeekdayAtTheEnd = cb.IsChecked.Value;
                        break;
                    case "chkNoPreviousDiary":
                        _TempSettings.Diary.DoNotShowPrevious = cb.IsChecked.Value;
                        break;
                    case "chkDiaryAscOrder":
                        _TempSettings.Diary.AscendingOrder = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void compareGroups(PNGroup gr)
        {
            try
            {
                var gc = PNCollections.Instance.Groups.GetGroupById(gr.Id);
                if (gc == null) return;
                var equals = gc == gr;
                if (!equals && _ChangedGroups.All(i => i != gr.Id))
                    _ChangedGroups.Add(gr.Id);
                else if (equals && _ChangedGroups.Any(i => i == gr.Id))
                    _ChangedGroups.Remove(gr.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFonts.SelectedIndex < 0) return;
                var lf = (LOGFONT)e.AddedItems[0];
                var gr = selectedGroup();
                if (gr == null) return;

                if (!_InTreeGroupsSelectionChanged)
                {
                    var logF = new LOGFONT();
                    logF.Init();
                    logF.SetFontFace(lf.lfFaceName);
                    logF.SetFontSize((int)cboFontSize.SelectedItem);
                    gr.Font = logF;
                }

                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontColor.SelectedIndex < 0) return;
                var gr = selectedGroup();
                if (gr == null) return;
                if (!(cboFontColor.SelectedItem is Rectangle rc)) return;
                if (!(rc.Fill is SolidColorBrush sb)) return;
                gr.FontColor = System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontSize.SelectedIndex < 0) return;
                var gr = selectedGroup();
                if (gr == null) return;

                if (!_InTreeGroupsSelectionChanged)
                {
                    var logF = new LOGFONT();
                    logF.Init();
                    logF.SetFontFace(gr.Font.lfFaceName);
                    logF.SetFontSize((int)cboFontSize.SelectedItem);
                    gr.Font = logF;
                }

                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboNumberOfDiaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboNumberOfDiaries.SelectedIndex > -1)
                {
                    _TempSettings.Diary.NumberOfPages = (int)cboNumberOfDiaries.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDiaryNaming_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDiaryNaming.SelectedIndex > -1)
                {
                    _TempSettings.Diary.DateFormat = (string)cboDiaryNaming.SelectedItem;
                    lblDiaryExample.Text = DateTime.Today.ToString(_TempSettings.Diary.DateFormat.Replace("/", "'/'"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.DockWidth = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.GeneralSettings.DockHeight = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void standardViewClick()
        {
            try
            {
                if (!(tvwGroups.SelectedItem is PNTreeItem treeItem)) return;
                restoreGroupDefaults(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void changeIconClick()
        {
            try
            {
                var dlgIcons = new WndFolderIcons { Owner = this };
                dlgIcons.GroupPropertyChanged += dlgIcons_GroupPropertyChanged;
                var showDialog = dlgIcons.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgIcons.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgIcons_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is WndFolderIcons d) d.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                if (!(tvwGroups.SelectedItem is PNTreeItem treeItem)) return;
                var pnGroup = treeItem.Tag as PNGroup;
                if (pnGroup == null) return;
                imgGroupIcon.Source = (BitmapImage)e.NewStateObject;
                pnGroup.Image = (BitmapImage)e.NewStateObject;
                pnGroup.IsDefaultImage = false;
                if (!Equals(treeItem.Image, pnGroup.Image))
                {
                    if (!pnGroup.IsDefaultImage)
                        treeItem.Image = pnGroup.Image;
                    else
                        treeItem.SetImageResource(pnGroup.ImageName);
                }
                compareGroups(pnGroup);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!(lstThemes.SelectedItem is string key)) return;
                var de = PNCollections.Instance.Themes[key];
                if (de == null) return;
                imgTheme.Source = de.Item3;
                if (!_Loaded) return;
                if (lstThemes.SelectedIndex >= 0)
                {
                    _TempSettings.Behavior.Theme = (string)lstThemes.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addThemeClick()
        {
            try
            {
                if (PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.PluginsChecking ||
                    PNSingleton.Instance.VersionChecking || PNSingleton.Instance.CriticalChecking ||
                    PNSingleton.Instance.ThemesDownload || PNSingleton.Instance.ThemesChecking) return;
                var updater = new PNUpdateChecker();
                updater.ThemesUpdateFound += updater_ThemesUpdateFound;
                updater.IsLatestVersion += updaterThemes_IsLatestVersion;
                Mouse.OverrideCursor = Cursors.Wait;
                updater.CheckThemesNewVersion();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void updaterThemes_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker updater)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                Mouse.OverrideCursor = null;
                var message = PNLang.Instance.GetMessageText("themes_latest_version",
                    "All themes are up-to-date.");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (sender is PNUpdateChecker updater)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("plugins_latest_version",
                    "All plugins are up-to-date.");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_ThemesUpdateFound(object sender, ThemesUpdateFoundEventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker updater) updater.ThemesUpdateFound -= updater_ThemesUpdateFound;
                Mouse.OverrideCursor = null;
                var d = new WndGetThemes(e.ThemesList) { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    promptToRestart();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Behavior staff
        private readonly string[] _DoubleSingleActions =
        {
            "(No Action)",
            "New Note",
            "Control Panel",
            "Preferences",
            "Search In Notes",
            "Load Note",
            "New Note From Clipboard",
            "Bring All To Front",
            "Save All",
            "Show All/Hide All",
            "Search By Tags",
            "Search By Dates"
        };

        private readonly string[] _DefNames =
        {
            "First characters of note",
            "Current date/time",
            "Current date/time and first characters of note"
        };

        private readonly string[] _PinsUnpins =
        {
            "Unpin",
            "Show pinned window"
        };

        private readonly string[] _StartPos =
        {
            "Center of screen",
            "Left docked",
            "Top docked",
            "Right docked",
            "Bottom docked"
        };
        public class PanelRemove : INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    if (value == _name) return;
                    _name = value;
                    OnPropertyChanged();
                }
            }

            public PanelRemoveMode Mode { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class PanelOrientation : INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    if (value == _name) return;
                    _name = value;
                    OnPropertyChanged();
                }
            }

            public NotesPanelOrientation Orientation { get; set; }

            public override string ToString()
            {
                return Name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly ObservableCollection<PanelOrientation> _PanelOrientations =
            new ObservableCollection<PanelOrientation>
            {
                new PanelOrientation {Name = "", Orientation = NotesPanelOrientation.Left},
                new PanelOrientation {Name = "", Orientation = NotesPanelOrientation.Top}
            };

        private readonly ObservableCollection<PanelRemove> _PanelRemoves = new ObservableCollection<PanelRemove>
        {
            new PanelRemove {Name = "", Mode = PanelRemoveMode.SingleClick},
            new PanelRemove {Name = "", Mode = PanelRemoveMode.DoubleClick}
        };

        private bool _InDefSettingsClick;

        private void initPageBehavior(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    for (var i = 1; i <= 128; i++)
                    {
                        cboLengthOfContent.Items.Add(i);
                        cboLengthOfName.Items.Add(i);
                    }
                    cboPanelDock.ItemsSource = _PanelOrientations;
                    cboPanelRemove.ItemsSource = _PanelRemoves;
                }
                chkNewOnTop.IsChecked = _TempSettings.Behavior.NewNoteAlwaysOnTop;
                chkRelationalPosition.IsChecked = _TempSettings.Behavior.RelationalPositioning;
                chkHideCompleted.IsChecked = _TempSettings.Behavior.HideCompleted;
                chkShowBigIcons.IsChecked = _TempSettings.Behavior.BigIconsOnCP;
                chkDontShowList.IsChecked = _TempSettings.Behavior.DoNotShowNotesInList;
                chkKeepVisibleOnShowdesktop.IsChecked = _TempSettings.Behavior.KeepVisibleOnShowDesktop;
                chkHideFluently.IsChecked = _TempSettings.Behavior.HideFluently;
                chkPlaySoundOnHide.IsChecked = _TempSettings.Behavior.PlaySoundOnHide;
                chkShowSeparateNotes.IsChecked = _TempSettings.Behavior.ShowSeparateNotes;
                var os = Environment.OSVersion;
                var vs = os.Version;
                if (os.Platform == PlatformID.Win32NT)
                {
                    if (vs.Major < 6)
                    {
                        chkKeepVisibleOnShowdesktop.IsEnabled = false;
                    }
                }
                else
                {
                    chkKeepVisibleOnShowdesktop.IsEnabled = false;
                }
                cboDblAction.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.DoubleClickAction);
                cboSingleAction.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.SingleClickAction);
                cboDefName.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.DefaultNaming);
                cboLengthOfName.SelectedItem = _TempSettings.Behavior.DefaultNameLength;
                cboLengthOfContent.SelectedItem = _TempSettings.Behavior.ContentColumnLength;
                trkTrans.Value = 100 - Convert.ToInt32((_TempSettings.Behavior.Opacity * 100));
                chkRandBack.IsChecked = _TempSettings.Behavior.RandomBackColor;
                chkInvertText.IsChecked = _TempSettings.Behavior.InvertTextColor;
                chkRoll.IsChecked = _TempSettings.Behavior.RollOnDblClick;
                chkFitRolled.IsChecked = _TempSettings.Behavior.FitWhenRolled;
                cboPinClick.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.PinClickAction);
                cboNoteStartPosition.SelectedIndex = Convert.ToInt32(_TempSettings.Behavior.StartPosition);
                chkHideMainWindow.IsChecked = _TempSettings.Behavior.HideMainWindow;
                chkPreventResizing.IsChecked = _TempSettings.Behavior.PreventAutomaticResizing;
                chkShowPanel.IsChecked = _TempSettings.Behavior.ShowNotesPanel;
                cboPanelDock.SelectedItem =
                    _PanelOrientations.FirstOrDefault(
                        po => po.Orientation == _TempSettings.Behavior.NotesPanelOrientation);
                cboPanelRemove.SelectedItem =
                    _PanelRemoves.First(pr => pr.Mode == _TempSettings.Behavior.PanelRemoveMode);
                chkPanelAutoHide.IsChecked = _TempSettings.Behavior.PanelAutoHide;
                chkPanelSwitchOffAnimation.IsChecked = _TempSettings.Behavior.PanelSwitchOffAnimation;
                for (var i = 0; i < cboPanelDelay.Items.Count; i++)
                {
                    var dl = (double)cboPanelDelay.Items[i];
                    if (!(Math.Abs(_TempSettings.Behavior.PanelEnterDelay / 1000.0 - dl) < double.Epsilon)) continue;
                    cboPanelDelay.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveBehavior(ref ChangesAction result)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Behavior.HideMainWindow != _TempSettings.Behavior.HideMainWindow)
                {
                    result |= ChangesAction.Restart;
                }
                if (Math.Abs(PNRuntimes.Instance.Settings.Behavior.Opacity - _TempSettings.Behavior.Opacity) >
                    double.Epsilon)
                {
                    PNNotesOperations.ApplyTransparency(_TempSettings.Behavior.Opacity);
                }
                if (PNRuntimes.Instance.Settings.Behavior.BigIconsOnCP != _TempSettings.Behavior.BigIconsOnCP)
                {
                    if (PNWindows.Instance.FormCP != null)
                    {
                        PNWindows.Instance.FormCP.SetToolbarIcons(_TempSettings.Behavior.BigIconsOnCP);
                    }
                }
                if (PNRuntimes.Instance.Settings.Behavior.HideCompleted != _TempSettings.Behavior.HideCompleted)
                {
                    if (_TempSettings.Behavior.HideCompleted)
                    {
                        //hide all notes marked as complete
                        var notes = PNCollections.Instance.Notes.Where(n => n.Visible && n.Completed);
                        foreach (var n in notes)
                        {
                            n.Dialog.ApplyHideNote(n);
                        }
                    }
                }
                var dblActionChanged = PNRuntimes.Instance.Settings.Behavior.DoubleClickAction !=
                                       _TempSettings.Behavior.DoubleClickAction;

                if (PNRuntimes.Instance.Settings.Behavior.KeepVisibleOnShowDesktop !=
                    _TempSettings.Behavior.KeepVisibleOnShowDesktop)
                {
                    PNNotesOperations.ApplyKeepVisibleOnShowDesktop(_TempSettings.Behavior.KeepVisibleOnShowDesktop);
                }

                if (PNRuntimes.Instance.Settings.Behavior.Theme != _TempSettings.Behavior.Theme)
                {
                    //PNStatic.ApplyTheme(_TempSettings.Behavior.Theme);
                    result |= ChangesAction.Restart;
                }

                var panelFlag = 0;
                if (PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel != _TempSettings.Behavior.ShowNotesPanel)
                {
                    PNNotesOperations.ApplyPanelButtonVisibility(_TempSettings.Behavior.ShowNotesPanel);
                    if (PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel && !_TempSettings.Behavior.ShowNotesPanel)
                    {
                        //panel before and no panel after
                        PNWindows.Instance.FormPanel.RemoveAllThumbnails();
                        PNWindows.Instance.FormPanel.Hide();
                    }
                    else if (!PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel && _TempSettings.Behavior.ShowNotesPanel)
                    {
                        //no panel before and panel after
                        panelFlag |= 1;
                    }
                }
                if (PNRuntimes.Instance.Settings.Behavior.NotesPanelOrientation != _TempSettings.Behavior.NotesPanelOrientation)
                {
                    panelFlag |= 2;
                }
                if (PNRuntimes.Instance.Settings.Behavior.PanelAutoHide != _TempSettings.Behavior.PanelAutoHide)
                {
                    panelFlag |= 4;
                }

                //unroll all rolled notes if RollOnDoubleClick discarded
                if (PNRuntimes.Instance.Settings.Behavior.RollOnDblClick != _TempSettings.Behavior.RollOnDblClick &&
                    !_TempSettings.Behavior.RollOnDblClick)
                {
                    var notes = PNCollections.Instance.Notes.Where(n => n.Rolled);
                    foreach (var note in notes)
                    {
                        if (note.Visible)
                            note.Dialog.ApplyRollUnroll(note);
                        else
                            PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                    }
                }

                PNRuntimes.Instance.Settings.Behavior = _TempSettings.Behavior.Clone();

                if ((panelFlag & 1) == 1)
                {
                    PNWindows.Instance.FormPanel.Show();
                }
                if ((panelFlag & 1) == 1 || (panelFlag & 2) == 2 || (panelFlag & 4) == 4)
                {
                    PNWindows.Instance.FormPanel.SetPanelPlacement();
                }
                if ((panelFlag & 2) == 2)
                {
                    PNWindows.Instance.FormPanel.UpdateOrientationImageBinding();
                }
                if ((panelFlag & 4) == 4)
                {
                    PNWindows.Instance.FormPanel.UpdateAutoHideImageBinding();
                }

                if (dblActionChanged)
                {
                    PNWindows.Instance.FormMain.ApplyNewDefaultMenu();
                }

                PNData.SaveBehaviorSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void hotkeysClick()
        {
            try
            {
                var dhk = new WndHotkeys { Owner = this };
                dhk.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void menusManagementClick()
        {
            try
            {
                var dm = new WndMenusManager { Owner = this };
                dm.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckBehavior_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkNewOnTop":
                        _TempSettings.Behavior.NewNoteAlwaysOnTop = cb.IsChecked.Value;
                        break;
                    case "chkRelationalPosition":
                        _TempSettings.Behavior.RelationalPositioning = cb.IsChecked.Value;
                        break;
                    case "chkHideCompleted":
                        _TempSettings.Behavior.HideCompleted = cb.IsChecked.Value;
                        break;
                    case "chkShowBigIcons":
                        _TempSettings.Behavior.BigIconsOnCP = cb.IsChecked.Value;
                        break;
                    case "chkDontShowList":
                        _TempSettings.Behavior.DoNotShowNotesInList = cb.IsChecked.Value;
                        break;
                    case "chkKeepVisibleOnShowdesktop":
                        _TempSettings.Behavior.KeepVisibleOnShowDesktop = cb.IsChecked.Value;
                        break;
                    case "chkHideFluently":
                        _TempSettings.Behavior.HideFluently = cb.IsChecked.Value;
                        break;
                    case "chkPlaySoundOnHide":
                        _TempSettings.Behavior.PlaySoundOnHide = cb.IsChecked.Value;
                        break;
                    case "chkRandBack":
                        _TempSettings.Behavior.RandomBackColor = cb.IsChecked.Value;
                        break;
                    case "chkInvertText":
                        _TempSettings.Behavior.InvertTextColor = cb.IsChecked.Value;
                        break;
                    case "chkRoll":
                        _TempSettings.Behavior.RollOnDblClick = cb.IsChecked.Value;
                        break;
                    case "chkFitRolled":
                        _TempSettings.Behavior.FitWhenRolled = cb.IsChecked.Value;
                        break;
                    case "chkShowSeparateNotes":
                        _TempSettings.Behavior.ShowSeparateNotes = cb.IsChecked.Value;
                        break;
                    case "chkHideMainWindow":
                        _TempSettings.Behavior.HideMainWindow = cb.IsChecked.Value;
                        break;
                    case "chkPreventResizing":
                        _TempSettings.Behavior.PreventAutomaticResizing = cb.IsChecked.Value;
                        return;
                    case "chkShowPanel":
                        _TempSettings.Behavior.ShowNotesPanel = cb.IsChecked.Value;
                        break;
                    case "chkPanelAutoHide":
                        _TempSettings.Behavior.PanelAutoHide = cb.IsChecked.Value;
                        break;
                    case "chkPanelSwitchOffAnimation":
                        _TempSettings.Behavior.PanelSwitchOffAnimation = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkTrans_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!(sender is Slider slider)) return;
                slider.Value = Math.Round(e.NewValue, 0);
                lblTransPerc.Text = trkTrans.Value.ToString(PNRuntimes.Instance.CultureInvariant) + @"%";
                _TempSettings.Behavior.Opacity = (100.0 - trkTrans.Value) / 100.0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDblAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDblAction.SelectedIndex < 0) return;
                if (cboDblAction.SelectedIndex == cboSingleAction.SelectedIndex && !_InDefSettingsClick)
                {
                    var message = PNLang.Instance.GetMessageText("same_actions",
                        "You can not choose the same action for double click and single click");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cboDblAction.SelectedItem = e.RemovedItems[0];
                    return;
                }
                _TempSettings.Behavior.DoubleClickAction = (TrayMouseAction)cboDblAction.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboSingleAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboSingleAction.SelectedIndex < 0) return;
                if (cboDblAction.SelectedIndex == cboSingleAction.SelectedIndex && !_InDefSettingsClick)
                {
                    var message = PNLang.Instance.GetMessageText("same_actions",
                        "You can not choose the same action for double click and single click");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cboSingleAction.SelectedItem = e.RemovedItems[0];
                    return;
                }
                _TempSettings.Behavior.SingleClickAction = (TrayMouseAction)cboSingleAction.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDefName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDefName.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.DefaultNaming = (DefaultNaming)cboDefName.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLengthOfName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLengthOfName.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.DefaultNameLength = Convert.ToInt32(cboLengthOfName.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLengthOfContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLengthOfContent.SelectedIndex > -1)
                {
                    _TempSettings.Behavior.ContentColumnLength = Convert.ToInt32(cboLengthOfContent.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPinClick_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboPinClick.SelectedIndex != -1)
                {
                    _TempSettings.Behavior.PinClickAction = (PinClickAction)cboPinClick.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboNoteStartPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboNoteStartPosition.SelectedIndex != -1)
                {
                    _TempSettings.Behavior.StartPosition = (NoteStartPosition)cboNoteStartPosition.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelDock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.NotesPanelOrientation = (NotesPanelOrientation)cboPanelDock.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelRemove_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.PanelRemoveMode = (PanelRemoveMode)cboPanelRemove.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboPanelDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Behavior.PanelEnterDelay = Convert.ToInt32((double)cboPanelDelay.SelectedItem * 1000);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Network staff
        public class SContact : INotifyPropertyChanged
        {
            private ContactConnection _ConnectionStatus;
            public string Name { get; }
            public string CompName { get; }
            public string IpAddress { get; }
            public ImageSource Icon { get; }
            public int Id { get; }

            public ContactConnection ConnectionStatus
            {
                get => _ConnectionStatus;
                set
                {
                    if (value == _ConnectionStatus) return;
                    _ConnectionStatus = value;
                    OnPropertyChanged();
                }
            }

            public SContact(int id, string n, string cn, string ip, ImageSource ic)
            {
                Id = id;
                Name = n;
                CompName = cn;
                IpAddress = ip;
                Icon = ic;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SSmtpClient : INotifyPropertyChanged
        {
            private bool _Selected;

            public bool Selected
            {
                get => _Selected;
                set
                {
                    if (_Selected == value) return;
                    _Selected = value;
                    OnPropertyChanged();
                }
            }

            public string Name { get; }
            public string DispName { get; }
            public string Address { get; }
            public int Port { get; }
            public int Id { get; }

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
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SMailContact
        {
            public string DispName { get; }
            public string Address { get; }
            public int Id { get; }

            public SMailContact(string dispname, string address, int id)
            {
                DispName = dispname;
                Address = address;
                Id = id;
            }
        }

        private bool _WorkInProgress;
        private bool _UseTimerConnection = true;
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
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Contacts.Add(cn);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.Id == cn.Id);
                    if (c != null)
                    {
                        c.Name = cn.Name;
                        c.ComputerName = cn.ComputerName;
                        c.IpAddress = cn.IpAddress;
                        c.UseComputerName = cn.UseComputerName;
                        c.GroupId = cn.GroupId;
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
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                    _Groups.Add(cg);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.Id == cg.Id);
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
                chkIncludeBinInSync.IsChecked = _TempSettings.Network.IncludeBinInSync;
                chkSyncOnStart.IsChecked = _TempSettings.Network.SyncOnStart;
                chkSaveBeforeSync.IsChecked = _TempSettings.Network.SaveBeforeSync;
                chkEnableExchange.IsChecked = _TempSettings.Network.EnableExchange;
                chkAllowPing.IsChecked = _TempSettings.Network.AllowPing;
                chkSaveBeforeSending.IsChecked = _TempSettings.Network.SaveBeforeSending;
                chkNoNotifyOnArrive.IsChecked = _TempSettings.Network.NoNotificationOnArrive;
                chkShowRecOnClick.IsChecked = _TempSettings.Network.ShowReceivedOnClick;
                chkShowIncomingOnClick.IsChecked = _TempSettings.Network.ShowIncomingOnClick;
                chkNoSoundOnArrive.IsChecked = _TempSettings.Network.NoSoundOnArrive;
                chkNoNotifyOnSend.IsChecked = _TempSettings.Network.NoNotificationOnSend;
                chkShowAfterReceiving.IsChecked = _TempSettings.Network.ShowAfterArrive;
                chkHideAfterSending.IsChecked = _TempSettings.Network.HideAfterSending;
                chkNoContInContextMenu.IsChecked = _TempSettings.Network.NoContactsInContextMenu;
                chkRecOnTop.IsChecked = _TempSettings.Network.ReceivedOnTop;

                txtExchPort.Value = _TempSettings.Network.ExchangePort;

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
                    if (Convert.ToInt32(cboPostCount.Items[i]) != _TempSettings.Network.PostCount) continue;
                    cboPostCount.SelectedIndex = i;
                    break;
                }
                cboPostCount.IsEnabled = lstSocial.Items.Count > 0;
                chkStoreOnserver.IsChecked = _TempSettings.Network.StoreOnServer;
                if (!string.IsNullOrWhiteSpace(_TempSettings.Network.ServerIp))
                {
                    var arr = _TempSettings.Network.ServerIp.Split('.').Select(s => Convert.ToByte(s.Trim())).ToArray();
                    ipServer.SetAddressBytes(arr);
                }
                txtServerPort.Value = _TempSettings.Network.ServerPort;
                updTimeout.Value = _TempSettings.Network.SendTimeout;
                switch (_TempSettings.Network.SilentSyncPeriod)
                {
                    case -1:
                        cboSyncPeriod.SelectedIndex = 0;
                        break;
                    case 10:
                        cboSyncPeriod.SelectedIndex = 1;
                        break;
                    case 15:
                        cboSyncPeriod.SelectedIndex = 2;
                        break;
                    case 20:
                        cboSyncPeriod.SelectedIndex = 3;
                        break;
                    case 30:
                        cboSyncPeriod.SelectedIndex = 4;
                        break;
                    case 45:
                        cboSyncPeriod.SelectedIndex = 5;
                        break;
                    case 60:
                        cboSyncPeriod.SelectedIndex = 6;
                        break;
                    case 120:
                        cboSyncPeriod.SelectedIndex = 7;
                        break;
                    case 180:
                        cboSyncPeriod.SelectedIndex = 8;
                        break;
                    case 240:
                        cboSyncPeriod.SelectedIndex = 9;
                        break;
                }

                optSyncPeriod.IsChecked = _TempSettings.Network.SilentSyncType == SilentSyncType.Period;
                optSyncTime.IsChecked = _TempSettings.Network.SilentSyncType == SilentSyncType.Time;
                try
                {
                    tpSync.DateValue = Convert.ToDateTime(new DateTime()
                                .Add(_TempSettings.Network.SilentSyncTime)
                                .ToString(_TempSettings.GeneralSettings.TimeFormat));
                }
                catch (FormatException)
                {
                    tpSync.DateValue = Convert.ToDateTime(new DateTime()
                        .Add(_TempSettings.Network.SilentSyncTime)
                        .ToString(PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern));
                }

                var allItems = lstSyncPlugins.Items.OfType<PNListBoxItem>().ToArray();
                grdSyncPeriod.IsEnabled =
                    allItems.Any(it => it.IsChecked != null && it.IsChecked.Value);
                chkAutoRestart.IsChecked = _TempSettings.Network.AutomaticSyncRestart;
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
                if (!PNRuntimes.Instance.Settings.Network.EnableExchange && _TempSettings.Network.EnableExchange)
                {
                    PNRuntimes.Instance.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNWindows.Instance.FormMain.StartWCFHosting();
                }
                // exchange before, no exchange after
                else if (PNRuntimes.Instance.Settings.Network.EnableExchange && !_TempSettings.Network.EnableExchange)
                {
                    PNRuntimes.Instance.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNWindows.Instance.FormMain.StopWCFHosting();
                }
                // port number of exchange changed
                else if (PNRuntimes.Instance.Settings.Network.ExchangePort != _TempSettings.Network.ExchangePort)
                {
                    PNRuntimes.Instance.Settings.Network.ExchangePort = _TempSettings.Network.ExchangePort;
                    PNWindows.Instance.FormMain.StopWCFHosting();
                    PNWindows.Instance.FormMain.StartWCFHosting();
                }

                if (PNRuntimes.Instance.Settings.Network.SilentSyncType != _TempSettings.Network.SilentSyncType)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SwitchSyncType,
                        Tuple.Create(_TempSettings.Network.SilentSyncType, _TempSettings.Network.SilentSyncPeriod));
                }
                else if (PNRuntimes.Instance.Settings.Network.SilentSyncPeriod != _TempSettings.Network.SilentSyncPeriod)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SetSyncTimerByPeriod,
                        _TempSettings.Network.SilentSyncPeriod);
                }

                PNRuntimes.Instance.Settings.Network = _TempSettings.Network.Clone();
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
                if (WPFMessageBox.Show(
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
                var result = WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNoCancel,
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

                        if (!(item?.Tag is IPlugin plugin)) return;
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
                            PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkPluginsUpdateClick()
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

        private void updater_PluginsUpdateFound(object sender, PluginsUpdateFoundEventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker updater)
                {
                    updater.PluginsUpdateFound -= updater_PluginsUpdateFound;
                }
                var d = new WndGetPlugins(e.PluginsList) { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    promptToRestart();
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
                return !(grdMailContacts.SelectedItem is SMailContact item) ? null : _MailContacts.FirstOrDefault(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editMailContactClick()
        {
            try
            {
                var item = getSelectedMailContact();
                if (item == null) return;
                var dlgMailContact = new WndMailContact(item) { Owner = this };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
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
                return !(grdContacts.SelectedItem is SContact item) ? null : _Contacts.FirstOrDefault(c => c.Id == item.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContactClick()
        {
            try
            {
                var cont = getSelectedContact();
                if (cont == null) return;
                var dlgContact = new WndContacts(cont, _Groups) { Owner = this };
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

        private void editSmtpClick()
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var smtpDlg = new WndSmtp(smtp) { Owner = this };
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
                return !(grdSmtp.SelectedItem is SSmtpClient item) ? null : _SmtpClients.FirstOrDefault(s => s.SenderAddress == item.Address);
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
                if (!(tvwContactsGroups.SelectedItem is PNTreeItem item) || item.Tag == null || Convert.ToInt32(item.Tag) == -1) return null;
                return _Groups.FirstOrDefault(g => g.Id == Convert.ToInt32(item.Tag));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editContactsGroupClick()
        {
            try
            {
                var gr = getSelectedContactsGroup();
                if (gr == null) return;
                var dlgContactGroup = new WndGroups(gr) { Owner = this };
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
                if (!(sender is PNListBoxItem item)) return;
                if (!(item.Tag is ISyncPlugin plugin)) return;
                if (e.State)
                {
                    _SyncPlugins.Add(plugin.Name);
                }
                else
                {
                    _SyncPlugins.Remove(plugin.Name);
                }
                var allItems = lstSyncPlugins.Items.OfType<PNListBoxItem>().ToArray();
                if (!allItems.Any(it => it.IsChecked != null && it.IsChecked.Value))
                {
                    cboSyncPeriod.SelectedIndex = 0;
                }
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
                if (!(sender is PNListBoxItem item)) return;
                if (!(item.Tag is IPostPlugin plugin)) return;
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
                var imageC = TryFindResource("contact_in_list") as BitmapImage ?? TryFindResource("contact") as BitmapImage;
                // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                var imageG = TryFindResource("group_in_list") as BitmapImage ?? TryFindResource("group") as BitmapImage;
                // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"));

                var itNone = new PNTreeItem(imageG, PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP),
                    -1);
                foreach (var cn in _Contacts.Where(c => c.GroupId == -1))
                {
                    itNone.Items.Add(new PNTreeItem(imageC, cn.Name, null));
                }
                _GroupsList.Add(itNone);

                foreach (var gc in _Groups)
                {
                    var id = gc.Id;
                    var it = new PNTreeItem(imageG, gc.Name, id);
                    foreach (var cn in _Contacts.Where(c => c.GroupId == id))
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
                foreach (var mc in _MailContacts)
                    _MailContactsList.Add(new SMailContact(mc.DisplayName, mc.Address, mc.Id));
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
                var image = TryFindResource("contact_in_list") as BitmapImage ??
                            TryFindResource("contact") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"));
                foreach (var c in _Contacts)
                {
                    _ContactsList.Add(new SContact(c.Id, c.Name, c.ComputerName, c.IpAddress, image));
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
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkIncludeBinInSync":
                        _TempSettings.Network.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkSyncOnStart":
                        _TempSettings.Network.SyncOnStart = cb.IsChecked.Value;
                        break;
                    case "chkSaveBeforeSync":
                        _TempSettings.Network.SaveBeforeSync = cb.IsChecked.Value;
                        break;
                    case "chkEnableExchange":
                        _TempSettings.Network.EnableExchange = cb.IsChecked.Value;
                        if (_TempSettings.Network.EnableExchange)
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
                        _TempSettings.Network.SaveBeforeSending = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnArrive":
                        _TempSettings.Network.NoNotificationOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkShowRecOnClick":
                        _TempSettings.Network.ShowReceivedOnClick = cb.IsChecked.Value;
                        break;
                    case "chkShowIncomingOnClick":
                        _TempSettings.Network.ShowIncomingOnClick = cb.IsChecked.Value;
                        break;
                    case "chkNoSoundOnArrive":
                        _TempSettings.Network.NoSoundOnArrive = cb.IsChecked.Value;
                        break;
                    case "chkNoNotifyOnSend":
                        _TempSettings.Network.NoNotificationOnSend = cb.IsChecked.Value;
                        break;
                    case "chkShowAfterReceiving":
                        _TempSettings.Network.ShowAfterArrive = cb.IsChecked.Value;
                        break;
                    case "chkHideAfterSending":
                        _TempSettings.Network.HideAfterSending = cb.IsChecked.Value;
                        break;
                    case "chkNoContInContextMenu":
                        _TempSettings.Network.NoContactsInContextMenu = cb.IsChecked.Value;
                        break;
                    case "chkAllowPing":
                        _TempSettings.Network.AllowPing = cb.IsChecked.Value;
                        break;
                    case "chkRecOnTop":
                        _TempSettings.Network.ReceivedOnTop = cb.IsChecked.Value;
                        break;
                    case "chkStoreOnserver":
                        _TempSettings.Network.StoreOnServer = cb.IsChecked.Value;
                        break;
                    case "chkAutoRestart":
                        _TempSettings.Network.AutomaticSyncRestart = cb.IsChecked.Value;
                        break;
                }
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
                if (!(tvwContactsGroups.GetObjectAtPoint<TreeViewItem>(e.GetPosition(tvwContactsGroups)) is PNTreeItem)) return;
                editContactsGroupClick();
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
                if (sender is WndGroups dg)
                {
                    dg.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Groups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Groups.Add(e.Group);
                }
                else
                {
                    var g = _Groups.FirstOrDefault(gr => gr.Id == e.Group.Id);
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

        private void grdContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdContacts.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdContacts)) is SContact)) return;
                editContactClick();
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
                if (sender is WndContacts dc)
                {
                    dc.ContactChanged -= dlgContact_ContactChanged;
                }
                if (e.Mode == AddEditMode.Add)
                {
                    if (_Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    _Contacts.Add(e.Contact);
                }
                else
                {
                    var c = _Contacts.FirstOrDefault(con => con.Id == e.Contact.Id);
                    if (c == null) return;
                    c.Name = e.Contact.Name;
                    c.ComputerName = e.Contact.ComputerName;
                    c.IpAddress = e.Contact.IpAddress;
                    c.UseComputerName = e.Contact.UseComputerName;
                    c.GroupId = e.Contact.GroupId;
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
                _TempSettings.Network.ExchangePort = Convert.ToInt32(txtExchPort.Value);
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
                if (lstSocial.SelectedIndex < 0) return;
                if (!(lstSocial.SelectedItem is PNListBoxItem item)) return;
                if (!(item.Tag is IPostPlugin plugin)) return;
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

        private void removePostPluginClick()
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
                    _TempSettings.Network.PostCount = Convert.ToInt32(cboPostCount.Items[cboPostCount.SelectedIndex]);
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
                if (lstSyncPlugins.SelectedIndex < 0) return;
                if (!(lstSyncPlugins.SelectedItem is PNListBoxItem item)) return;
                if (item.Tag is ISyncPlugin plugin)
                {
                    lblSyncPAuthor.Text = plugin.Author;
                    lblSyncPVersion.Text = plugin.Version;
                    lblSyncPInfo.Text = plugin.AdditionalInfo;
                }
                pnsSyncPluginDetails.Width = width;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeSyncPluginClick()
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

        private void syncNowClick()
        {
            try
            {
                if (lstSyncPlugins.SelectedIndex <= -1) return;
                if (!(lstSyncPlugins.Items[lstSyncPlugins.SelectedIndex] is PNListBoxItem item)) return;
                var plugin = PNPlugins.Instance.SyncPlugins.FirstOrDefault(p => p.Name == item.Text);
                if (plugin == null) return;
                var version = new Version(plugin.Version);
                if (version.Major >= 2 && version.Build >= 3)
                {
                    var ds = new WndSync(plugin) { Owner = this };
                    ds.ShowDialog();
                }
                else
                {
                    if (plugin is ISyncAsyncPlugin asyncPlugin)
                    {
                        synchronizeAsync(asyncPlugin);
                    }
                    else
                    {
                        switch (plugin.Synchronize())
                        {
                            case SyncResult.None:
                                WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Synchronization completed successfully"), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            case SyncResult.Reload:
                                WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Synchronization completed successfully. The program has to be restarted for applying all changes."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                                PNData.UpdateTablesAfterSync();
                                PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                                break;
                            case SyncResult.AbortVersion:
                                WPFMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;
                            case SyncResult.Error:
                                var sb =
                                    new StringBuilder(PNLang.Instance.GetMessageText("sync_error_1",
                                        "An error occurred during synchronization."));
                                sb.Append(" (");
                                sb.Append(plugin.Name);
                                sb.Append(")");
                                sb.AppendLine();
                                sb.Append(PNLang.Instance.GetMessageText("sync_error_2",
                                    "Please, refer to log file of appropriate plugin."));
                                WPFMessageBox.Show(sb.ToString(), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private async void synchronizeAsync(ISyncAsyncPlugin plugin)
        {
            try
            {
                var result = await plugin.SynchronizeAsync();
                switch (result)
                {
                    case SyncResult.None:
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete", "Synchronization completed successfully"), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case SyncResult.Reload:
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("sync_complete_reload", "Synchronization completed successfully. The program has to be restarted for applying all changes."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                        PNData.UpdateTablesAfterSync();
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        break;
                    case SyncResult.AbortVersion:
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("diff_versions", "Current version of database is different from previously synchronized version. Synchronization cannot be performed."), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case SyncResult.Error:
                        var sb =
                            new StringBuilder(PNLang.Instance.GetMessageText("sync_error_1",
                                "An error occurred during synchronization."));
                        sb.Append(" (");
                        sb.Append(plugin.Name);
                        sb.Append(")");
                        sb.AppendLine();
                        sb.Append(PNLang.Instance.GetMessageText("sync_error_2",
                            "Please, refer to log file of appropriate plugin."));
                        WPFMessageBox.Show(sb.ToString(), plugin.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                }
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
                editSmtpClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdMailContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editMailContactClick();
        }

        private void smtpDlg_SmtpChanged(object sender, SmtpChangedEventArgs e)
        {
            try
            {
                if (e.Mode == AddEditMode.Add)
                {
                    if (_SmtpClients.Any(sm => sm.SenderAddress == e.Profile.SenderAddress))
                    {
                        WPFMessageBox.Show(
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
                        WPFMessageBox.Show(
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

                if (sender is WndSmtp d) d.SmtpChanged -= smtpDlg_SmtpChanged;
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
                            WPFMessageBox.Show(
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
                            WPFMessageBox.Show(
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

                if (!(sender is WndMailContact d)) return;
                d.MailContactChanged -= dlgMailContact_MailContactChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSmtpCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkBox)) return;
            if (!(grdSmtp.GetObjectAtPoint<ListViewItem>(Mouse.GetPosition(grdSmtp)) is SSmtpClient item)) return;
            grdSmtp.SelectedItem = item;

            foreach (var smtp in _SmtpClients)
            {
                if (checkBox.IsChecked != null)
                    smtp.Active = smtp.SenderAddress == item.Address && checkBox.IsChecked.Value;
                else
                    smtp.Active = false;
            }
        }

        private void impOutlookClick(string text)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Outlook, _MailContacts, text) { Owner = this };
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

        private void impGmailClick(string text)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Gmail, _MailContacts, text) { Owner = this };
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

        private void impLotusClick(string text)
        {
            try
            {
                var dlgImport = new WndImportMailContacts(ImportContacts.Lotus, _MailContacts, text) { Owner = this };
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
                if (sender is WndImportMailContacts d) d.ContactsImported -= dlgImport_ContactsImported;
                var id = _MailContacts.Any() ? _MailContacts.Max(c => c.Id) + 1 : 0;
                foreach (var tc in e.Contacts.Where(tc => !_MailContacts.Any(c => c.DisplayName == tc.Item1 && c.Address == tc.Item2)))
                {
                    _MailContacts.Add(new PNMailContact { Id = id, DisplayName = tc.Item1, Address = tc.Item2 });
                    _MailContactsList.Add(new SMailContact(tc.Item1, tc.Item2, id));
                    id++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void TimerDelegate(object sender, ElapsedEventArgs e);
        private void _TimerConnections_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _TimerConnections.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = _TimerConnections_Elapsed;
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

        private void addContactGroupClick()
        {
            try
            {
                var newId = 0;
                if (_Groups.Count > 0)
                {
                    newId = _Groups.Max(g => g.Id) + 1;
                }
                var dlgContactGroup = new WndGroups(newId) { Owner = this };
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

        private void deleteContactGroupClick()
        {
            try
            {
                var cg = getSelectedContactsGroup();
                if (cg == null) return;
                if (_Contacts.All(c => c.GroupId != cg.Id))
                {
                    var message = PNLang.Instance.GetMessageText("group_delete", "Delete selected group of contacts?");
                    if (
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes) return;
                    _Groups.Remove(cg);
                    fillGroups(false);
                    return;
                }

                var dlg = new WndDeleteContactsGroup { Owner = this };
                var showDialog = dlg.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                switch (dlg.DeleteBehavior)
                {
                    case DeleteContactsGroupBehavior.DeleteAll:
                        //delete all contacts
                        _Contacts.RemoveAll(c => c.GroupId == cg.Id);
                        break;
                    case DeleteContactsGroupBehavior.Move:
                        //move all contacts to '(none)'
                        foreach (var c in _Contacts.Where(c => c.GroupId == cg.Id))
                        {
                            c.GroupId = -1;
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

        private void addContactClick()
        {
            try
            {
                var newId = 0;
                if (_Contacts.Count > 0)
                {
                    newId = _Contacts.Max(c => c.Id) + 1;
                }
                var dlgContact = new WndContacts(newId, _Groups) { Owner = this };
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

        private void deleteContactClick()
        {
            try
            {
                var cn = getSelectedContact();
                if (cn == null) return;
                var message = PNLang.Instance.GetMessageText("contact_delete", "Delete selected contact?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
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

        private void addSmtpClick()
        {
            try
            {
                var smtpDlg = new WndSmtp(null) { Owner = this };
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

        private void deleteSmtpClick()
        {
            try
            {
                var smtp = getSelectedSmtp();
                if (smtp == null) return;
                var profile = _SmtpClients.FirstOrDefault(sm => sm.Id == smtp.Id);
                if (profile == null) return;
                if (
                    WPFMessageBox.Show(
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

        private void addMailContactClick()
        {
            try
            {
                var dlgMailContact = new WndMailContact(null) { Owner = this };
                dlgMailContact.MailContactChanged += dlgMailContact_MailContactChanged;
                var showDialog = dlgMailContact.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                    dlgMailContact.MailContactChanged -= dlgMailContact_MailContactChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteMailContactClick()
        {
            try
            {
                var contact = getSelectedMailContact();
                if (contact == null) return;
                if (
                    WPFMessageBox.Show(
                        PNLang.Instance.GetMessageText("remove_mail_contact", "Remove selected mail contact?"),
                        PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                _MailContacts.Remove(contact);
                fillMailContacts(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clearMailContactsClick()
        {
            try
            {
                _MailContactsList.Clear();
                _MailContacts.Clear();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importMailContactClick()
        {
            ctmImpContacts.IsOpen = true;
        }

        private void ipServer_FieldChanged(object sender, PNIPBox.FieldChangedEventArgs e)
        {
            try
            {
                if (!ipServer.IsAnyBlank)
                {
                    _TempSettings.Network.ServerIp = ipServer.Text;
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
                    _TempSettings.Network.ServerPort = Convert.ToInt32(txtServerPort.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private async void checkConnectionClick()
        {
            try
            {
                elpCheckConnection.Visibility = Visibility.Visible;
                var clientRunner = new PNWCFClientRunner();
                var result = await Task.Run(() => clientRunner.CheckServer(_TempSettings.Network.ServerIp,
                    _TempSettings.Network.ServerPort,
                    PNServerConstants.NET_SERVICE_NAME, _TempSettings.Network.SendTimeout));
                elpCheckConnection.Visibility = Visibility.Hidden;
                string message;
                if (result == PNServerConstants.SUCCESS)
                {
                    message = PNLang.Instance.GetMessageText("server_check_success", "Test connection succeeded");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    message = PNLang.Instance.GetMessageText("server_check_failed", "Test connection failed");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (elpCheckConnection.Visibility == Visibility.Visible)
                    elpCheckConnection.Visibility = Visibility.Hidden;
            }
        }

        private async void checkForNotesClick()
        {
            try
            {
                elpCheckConnection.Visibility = Visibility.Visible;
                var clientRunner = new PNWCFClientRunner();

                await Task.Run(() =>
                    clientRunner.CheckMessages(_TempSettings.Network.ServerIp, _TempSettings.Network.ServerPort));
                elpCheckConnection.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (elpCheckConnection.Visibility == Visibility.Visible)
                    elpCheckConnection.Visibility = Visibility.Hidden;
            }
        }

        private void updTimeout_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Network.SendTimeout = Convert.ToInt32(updTimeout.Value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionNetwork_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (!(sender is RadioButton opt)) return;
                switch (opt.Name)
                {
                    case "optSyncPeriod":
                    case "optSyncTime":
                        _TempSettings.Network.SilentSyncType =
                            optSyncPeriod.IsChecked != null && optSyncPeriod.IsChecked.Value
                                ? SilentSyncType.Period
                                : SilentSyncType.Time;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tpSync_DateValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Network.SilentSyncTime =
                    new TimeSpan(tpSync.DateValue.Hour, tpSync.DateValue.Minute, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Protection staff
        private readonly string[] _SyncPeriods =
        {
            "Never",
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
            public string CompName { get; }
            public string NotesFile { get; }
            public string DbFile { get; }

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


        private void initPageProtection(bool firstTime)
        {
            try
            {
                chkStoreEncrypted.IsChecked = _TempSettings.Protection.StoreAsEncrypted;
                chkHideTrayIcon.IsChecked = _TempSettings.Protection.HideTrayIcon;
                chkBackupBeforeSaving.IsChecked = _TempSettings.Protection.BackupBeforeSaving;
                chkSilentFullBackup.IsChecked = _TempSettings.Protection.SilentFullBackup;
                updBackup.Value = _TempSettings.Protection.BackupDeepness;
                chkDonotShowProtected.IsChecked = _TempSettings.Protection.DontShowContent;
                chkIncludeBinInLocalSync.IsChecked = _TempSettings.Protection.IncludeBinInSync;
                if (PNRuntimes.Instance.Settings.Protection.PasswordString.Trim().Length == 0)
                {
                    chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
                }
                else
                {
                    chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;
                }
                if (firstTime)
                {
                    fillSyncComps();
                }
                var daysChecks = stkFullBackup.Children.OfType<CheckBox>();
                foreach (var c in daysChecks)
                {
                    c.IsChecked = _TempSettings.Protection.FullBackupDays.Contains((DayOfWeek)Convert.ToInt32(c.Tag));
                }
                enableFullBackupTime();

                //if (_TempSettings.Protection.FullBackupTime != DateTime.MinValue)
                //{
                dtpFullBackup.DateValue = _TempSettings.Protection.FullBackupTime;
                //}
                chkPromptForPassword.IsChecked = _TempSettings.Protection.PromptForPassword;
                txtDataDir.Text = _TempSettings.Protection.LocalSyncFilesLocation;
                switch (_TempSettings.Protection.LocalSyncPeriod)
                {
                    case -1:
                        cboLocalSyncPeriod.SelectedIndex = 0;
                        break;
                    case 10:
                        cboLocalSyncPeriod.SelectedIndex = 1;
                        break;
                    case 15:
                        cboLocalSyncPeriod.SelectedIndex = 2;
                        break;
                    case 20:
                        cboLocalSyncPeriod.SelectedIndex = 3;
                        break;
                    case 30:
                        cboLocalSyncPeriod.SelectedIndex = 4;
                        break;
                    case 45:
                        cboLocalSyncPeriod.SelectedIndex = 5;
                        break;
                    case 60:
                        cboLocalSyncPeriod.SelectedIndex = 6;
                        break;
                    case 120:
                        cboLocalSyncPeriod.SelectedIndex = 7;
                        break;
                    case 180:
                        cboLocalSyncPeriod.SelectedIndex = 8;
                        break;
                    case 240:
                        cboLocalSyncPeriod.SelectedIndex = 9;
                        break;
                }
                optLocalSyncPeriod.IsChecked = _TempSettings.Protection.LocalSyncType == SilentSyncType.Period;
                optLocalSyncTime.IsChecked = _TempSettings.Protection.LocalSyncType == SilentSyncType.Time;
                try
                {
                    tpSyncLocal.DateValue = Convert.ToDateTime(new DateTime()
                                .Add(_TempSettings.Protection.LocalSyncTime)
                                .ToString(_TempSettings.GeneralSettings.TimeFormat));
                }
                catch (FormatException)
                {
                    tpSync.DateValue = Convert.ToDateTime(new DateTime()
                        .Add(_TempSettings.Network.SilentSyncTime)
                        .ToString(PNRuntimes.Instance.CultureInvariant.DateTimeFormat.ShortTimePattern));
                }
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
                //check for pasword and encrypt/decryp notes if needed
                if (PNRuntimes.Instance.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
                {
                    var passwordString = PNRuntimes.Instance.Settings.Protection.PasswordString;
                    if (passwordString.Length > 0)
                    {
                        // check encryption settings
                        if (!PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted &&
                            _TempSettings.Protection.StoreAsEncrypted)
                        {
                            // no encryption before and encryption after - encrypt all notes
                            PNNotesOperations.EncryptAllNotes(passwordString);
                        }
                        else if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted &&
                                 !_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryption before and no encryption after - decrypt all notes
                            PNNotesOperations.DecryptAllNotes(passwordString);
                        }
                    }
                }
                else
                {
                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length == 0 &&
                        _TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // no password before and password ater - check encryption settings
                        if (_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encryp all notes
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                    }
                    else if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                             _TempSettings.Protection.PasswordString.Length == 0)
                    {
                        // password before and no password after
                        if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes
                            PNNotesOperations.DecryptAllNotes(PNRuntimes.Instance.Settings.Protection.PasswordString);
                        }
                    }
                    else if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                             _TempSettings.Protection.PasswordString.Length > 0)
                    {
                        // password has been changed
                        if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted &&
                            _TempSettings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNRuntimes.Instance.Settings.Protection.PasswordString);
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                        else if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                        {
                            // decrypt all notes using old password
                            PNNotesOperations.DecryptAllNotes(
                                PNRuntimes.Instance.Settings.Protection.PasswordString);
                        }
                        else if (_TempSettings.Protection.StoreAsEncrypted)
                        {
                            // encrypt all notes using new password
                            PNNotesOperations.EncryptAllNotes(_TempSettings.Protection.PasswordString);
                        }
                    }
                }

                if (PNRuntimes.Instance.Settings.Protection.LocalSyncType != _TempSettings.Protection.LocalSyncType)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SwitchLocalSyncType,
                        Tuple.Create(_TempSettings.Protection.LocalSyncType, _TempSettings.Protection.LocalSyncPeriod));
                }
                else if (PNRuntimes.Instance.Settings.Protection.LocalSyncPeriod != _TempSettings.Protection.LocalSyncPeriod &&
                    _TempSettings.Protection.LocalSyncType == SilentSyncType.Period)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SetLocalSyncTimerByPeriod,
                        _TempSettings.Protection.LocalSyncPeriod);
                }

                var raiseEvent = PNRuntimes.Instance.Settings.Protection.DontShowContent !=
                                 _TempSettings.Protection.DontShowContent;
                PNRuntimes.Instance.Settings.Protection = _TempSettings.Protection.Clone();
                PNData.SaveProtectionSettings();
                if (raiseEvent)
                {
                    PNWindows.Instance.FormMain.RaiseContentDisplayChangedEevent();
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
                return !(grdLocalSync.SelectedItem is SSyncComp item) ? null : _SyncComps.FirstOrDefault(s => s.CompName == item.CompName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSyncCompClick()
        {
            try
            {
                var sc = getSelectedSyncComp();
                if (sc == null) return;
                var d = new WndSyncComps(this, sc, AddEditMode.Edit);
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

        private void createPwrdClick()
        {
            try
            {
                // if there is no password - show standard password creation dialog
                if (PNRuntimes.Instance.Settings.Protection.PasswordString.Trim().Length == 0)
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
                    changePwrdClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void changePwrdClick()
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password changing dialog
                if (PNRuntimes.Instance.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
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
                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Trim().Length == 0)
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

        private void removePwrdClick()
        {
            try
            {
                // if new password has been applied or old password has not been changed - show standard password removing dialog
                if (PNRuntimes.Instance.Settings.Protection.PasswordString == _TempSettings.Protection.PasswordString)
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
                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Trim().Length == 0)
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
            _TempSettings.Protection.PasswordString = "";
            chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = false;
            chkStoreEncrypted.IsChecked = chkHideTrayIcon.IsChecked = false;

            if (sender is WndPasswordDelete dlgPasswordDelete)
                dlgPasswordDelete.PasswordDeleted -= dpd_PasswordDeleted;
        }

        private void dpc_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            _TempSettings.Protection.PasswordString = e.NewPassword;
            chkStoreEncrypted.IsEnabled = chkHideTrayIcon.IsEnabled = true;

            if (sender.GetType() == typeof(WndPasswordCreate))
            {
                if (sender is WndPasswordCreate dlgPasswordCreate)
                    dlgPasswordCreate.PasswordChanged -= dpc_PasswordChanged;
            }
            else
            {
                if (sender is WndPasswordChange dlgPasswordChange)
                    dlgPasswordChange.PasswordChanged -= dpc_PasswordChanged;
            }
        }

        private void CheckProtection_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkStoreEncrypted":
                        _TempSettings.Protection.StoreAsEncrypted = cb.IsChecked.Value;
                        break;
                    case "chkHideTrayIcon":
                        _TempSettings.Protection.HideTrayIcon = cb.IsChecked.Value;
                        break;
                    case "chkBackupBeforeSaving":
                        _TempSettings.Protection.BackupBeforeSaving = cb.IsChecked.Value;
                        break;
                    case "chkSilentFullBackup":
                        _TempSettings.Protection.SilentFullBackup = cb.IsChecked.Value;
                        break;
                    case "chkDonotShowProtected":
                        _TempSettings.Protection.DontShowContent = cb.IsChecked.Value;
                        break;
                    case "chkIncludeBinInLocalSync":
                        _TempSettings.Protection.IncludeBinInSync = cb.IsChecked.Value;
                        break;
                    case "chkPromptForPassword":
                        _TempSettings.Protection.PromptForPassword = cb.IsChecked.Value;
                        break;
                    case "chkLocalAutoRestart":
                        _TempSettings.Protection.AutomaticSyncRestart = cb.IsChecked.Value;
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
            _TempSettings.Protection.BackupDeepness = Convert.ToInt32(updBackup.Value);
        }

        private void chkW0_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (!(sender is CheckBox cb) || cb.IsChecked == null) return;
                var dw = (DayOfWeek)Convert.ToInt32(cb.Tag);
                if (cb.IsChecked.Value)
                {
                    if (!_TempSettings.Protection.FullBackupDays.Contains(dw))
                        _TempSettings.Protection.FullBackupDays.Add(dw);
                }
                else
                {
                    _TempSettings.Protection.FullBackupDays.RemoveAll(d => d == dw);
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
                _TempSettings.Protection.FullBackupTime = dtpFullBackup.DateValue;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdLocalSync_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editSyncCompClick();
        }

        private void addSyncCompClick()
        {
            try
            {
                var d = new WndSyncComps(this, AddEditMode.Add);
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

        private void removeSyncCompClick()
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("sync_comp_delete", "Delete selected synchronization target?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
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
                _TempSettings.Protection.LocalSyncFilesLocation = txtDataDir.Text.Trim();
                if (_TempSettings.Protection.LocalSyncFilesLocation.Length == 0)
                {
                    cboLocalSyncPeriod.SelectedIndex = 0;
                    optLocalSyncPeriod.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dataDirClick()
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
                        _TempSettings.Protection.LocalSyncPeriod = -1;
                        break;
                    case 1:
                        _TempSettings.Protection.LocalSyncPeriod = 10;
                        break;
                    case 2:
                        _TempSettings.Protection.LocalSyncPeriod = 15;
                        break;
                    case 3:
                        _TempSettings.Protection.LocalSyncPeriod = 20;
                        break;
                    case 4:
                        _TempSettings.Protection.LocalSyncPeriod = 30;
                        break;
                    case 5:
                        _TempSettings.Protection.LocalSyncPeriod = 45;
                        break;
                    case 6:
                        _TempSettings.Protection.LocalSyncPeriod = 60;
                        break;
                    case 7:
                        _TempSettings.Protection.LocalSyncPeriod = 120;
                        break;
                    case 8:
                        _TempSettings.Protection.LocalSyncPeriod = 180;
                        break;
                    case 9:
                        _TempSettings.Protection.LocalSyncPeriod = 240;
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

        private void cboSyncPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                switch (cboSyncPeriod.SelectedIndex)
                {
                    case 0:
                        _TempSettings.Network.SilentSyncPeriod = -1;
                        break;
                    case 1:
                        _TempSettings.Network.SilentSyncPeriod = 10;
                        break;
                    case 2:
                        _TempSettings.Network.SilentSyncPeriod = 15;
                        break;
                    case 3:
                        _TempSettings.Network.SilentSyncPeriod = 20;
                        break;
                    case 4:
                        _TempSettings.Network.SilentSyncPeriod = 30;
                        break;
                    case 5:
                        _TempSettings.Network.SilentSyncPeriod = 45;
                        break;
                    case 6:
                        _TempSettings.Network.SilentSyncPeriod = 60;
                        break;
                    case 7:
                        _TempSettings.Network.SilentSyncPeriod = 120;
                        break;
                    case 8:
                        _TempSettings.Network.SilentSyncPeriod = 180;
                        break;
                    case 9:
                        _TempSettings.Network.SilentSyncPeriod = 240;
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

        private void tpSuncLocal_DateValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            try
            {
                if (!_Loaded) return;
                _TempSettings.Protection.LocalSyncTime =
                    new TimeSpan(tpSyncLocal.DateValue.Hour, tpSyncLocal.DateValue.Minute, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionProtected_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (!(sender is RadioButton opt)) return;
                switch (opt.Name)
                {
                    case "optLocalSyncPeriod":
                    case "optLocalSyncTime":
                        _TempSettings.Protection.LocalSyncType =
                            optLocalSyncPeriod.IsChecked != null && optLocalSyncPeriod.IsChecked.Value
                                ? SilentSyncType.Period
                                : SilentSyncType.Time;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Dummy:
                        switch (e.Parameter?.ToString())
                        {
                            case "cmdImportMailContact":
                                e.CanExecute = true;
                                break;
                            case "cmdAddTag":
                                e.CanExecute = txtTag.Text.Trim().Length > 0;
                                break;
                            case "cmdEditProv":
                                e.CanExecute = grdSearchProvs?.SelectedItems.Count > 0;
                                break;
                            case "cmdEditExt":
                                e.CanExecute = grdExternals?.SelectedItems.Count > 0;
                                break;
                            case "cmdEditContactGroup":
                                {
                                    e.CanExecute = (tvwContactsGroups?.SelectedItem is PNTreeItem item) &&
                                                   item.Tag != null && Convert.ToInt32(item.Tag) != -1;
                                }
                                break;
                            case "cmdEditContact":
                                e.CanExecute = grdContacts?.SelectedItems.Count > 0;
                                break;
                            case "cmdEditSmtp":
                                e.CanExecute = grdSmtp?.SelectedItems.Count > 0;
                                break;
                            case "cmdEditMailContact":
                                e.CanExecute = grdMailContacts?.SelectedItems.Count > 0;
                                break;
                            case "cmdEditComp":
                                e.CanExecute = grdLocalSync.SelectedItems.Count > 0;
                                break;
                            case "cmdDeleteProv":
                                e.CanExecute = grdSearchProvs?.SelectedItems.Count > 0;
                                break;
                            case "cmdDeleteExt":
                                e.CanExecute = grdExternals?.SelectedItems.Count > 0;
                                break;
                            case "cmdDeleteTag":
                                e.CanExecute = lstTags?.SelectedIndex >= 0;
                                break;
                            case "cmdRemoveSound":
                                e.CanExecute = clbSounds?.SelectedIndex > 0;
                                break;
                            case "cmdDeleteContactGroup":
                                {
                                    e.CanExecute = (tvwContactsGroups?.SelectedItem is PNTreeItem item) &&
                                                   item.Tag != null && Convert.ToInt32(item.Tag) != -1;
                                }
                                break;
                            case "cmdDeleteContact":
                                e.CanExecute = grdContacts?.SelectedItems.Count > 0;
                                break;
                            case "cmdDeleteSmtp":
                                e.CanExecute = grdSmtp?.SelectedItems.Count > 0;
                                break;
                            case "cmdDeleteMailContact":
                                e.CanExecute = grdMailContacts?.SelectedItems.Count > 0;
                                break;
                            case "cmdRemoveComp":
                                e.CanExecute = grdLocalSync.SelectedItems.Count > 0;
                                break;
                            case "cmdClearMailContacts":
                                e.CanExecute = _MailContactsList.Any();
                                break;
                            case "cmdListenSound":
                                e.CanExecute = clbSounds?.SelectedIndex >= 0;
                                break;
                            default:
                                e.CanExecute = true;
                                break;
                        }
                        break;
                    case CommandType.BrowseButton:
                    case CommandType.ImportGmail:
                    case CommandType.CheckPluginsUpdate:
                    case CommandType.HotkeysCP:
                    case CommandType.MenusManagementCP:
                    case CommandType.GroupIcon:
                    case CommandType.SkinlessCaptionFont:
                    case CommandType.StandardView:
                    case CommandType.Question:
                    case CommandType.CheckUpdate:
                    case CommandType.DefaultSettings:
                    case CommandType.Save:
                    case CommandType.Cancel:
                        e.CanExecute = true;
                        break;
                    case CommandType.CheckConnection:
                    case CommandType.CheckMessages:
                        e.CanExecute = elpCheckConnection.Visibility != Visibility.Visible;
                        break;
                    case CommandType.Apply:
                        e.CanExecute = applyCanExecute();
                        break;
                    case CommandType.ChangeFont:
                        e.CanExecute = _SetFontEnabled;
                        break;
                    case CommandType.RestoreFont:
                        e.CanExecute = !_SetFontEnabled;
                        break;
                    case CommandType.DefaultVoice:
                        if (lstVoices.SelectedIndex < 0) e.CanExecute = false;
                        else if (!(lstVoices.Items[lstVoices.SelectedIndex] is PNListBoxItem voice))
                            e.CanExecute = false;
                        else
                            e.CanExecute = voice.Text != _TempSettings.Schedule.Voice;
                        break;
                    case CommandType.VoiceSample:
                        e.CanExecute = txtVoiceSample.Text.Trim().Length > 0 && lstVoices.SelectedIndex >= 0;
                        break;
                    case CommandType.RemovePlugin:
                        switch (e.Parameter?.ToString())
                        {
                            case "cmdRemovePostPlugin":
                                e.CanExecute = lstSocial?.SelectedIndex > -1;
                                break;
                            case "cmdRemoveSyncPlugin":
                                e.CanExecute = lstSyncPlugins?.SelectedIndex > -1;
                                break;
                        }
                        break;
                    case CommandType.SyncNow:
                        e.CanExecute = lstSyncPlugins?.SelectedIndex > -1 &&
                                       lstSyncPlugins.Items.OfType<PNListBoxItem>().ToArray()
                                           .Any(it => it.IsChecked != null && it.IsChecked.Value);
                        break;
                    case CommandType.ImportOutlook:
                        e.CanExecute = ctmImpContacts.IsOpen && PNOffice.GetOfficeAppVersion(OfficeApp.Outlook) >= 11;
                        break;
                    case CommandType.ImportLotus:
                        e.CanExecute = ctmImpContacts.IsOpen && PNLotus.IsLotusInstalled();
                        break;
                    case CommandType.PwrdCreate:
                        e.CanExecute = string.IsNullOrEmpty(_TempSettings.Protection.PasswordString);
                        break;
                    case CommandType.PwrdChange:
                        e.CanExecute = !string.IsNullOrEmpty(_TempSettings.Protection.PasswordString);
                        break;
                    case CommandType.PwrdRemove:
                        e.CanExecute = !string.IsNullOrEmpty(_TempSettings.Protection.PasswordString);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Save:
                        saveClick();
                        break;
                    case CommandType.Cancel:
                        Close();
                        break;
                    case CommandType.Apply:
                        applyClick();
                        break;
                    case CommandType.DefaultSettings:
                        defClick();
                        break;
                    case CommandType.ChangeFont:
                        setFontUIClick();
                        break;
                    case CommandType.RestoreFont:
                        restoreFontUIClick();
                        break;
                    case CommandType.CheckUpdate:
                        newVersionClick();
                        break;
                    case CommandType.Question:
                        switch (e.Parameter.ToString())
                        {
                            case "cmdDTFShort":
                                tFShortClick();
                                break;
                            case "cmdTFLong":
                                tFLongClick();
                                break;
                        }
                        break;
                    case CommandType.Dummy:
                        switch (e.Parameter?.ToString())
                        {
                            case "cmdAddLang":
                                addLangClick();
                                break;
                            case "cmdAddProv":
                                addProvClick();
                                break;
                            case "cmdAddExt":
                                addExtClick();
                                break;
                            case "cmdAddTag":
                                addTagClick();
                                break;
                            case "cmdAddSound":
                                addSoundClick();
                                break;
                            case "cmdAddTheme":
                                addThemeClick();
                                break;
                            case "cmdAddContactGroup":
                                addContactGroupClick();
                                break;
                            case "cmdAddContact":
                                addContactClick();
                                break;
                            case "cmdAddSmtp":
                                addSmtpClick();
                                break;
                            case "cmdAddMailContact":
                                addMailContactClick();
                                break;
                            case "cmdAddComp":
                                addSyncCompClick();
                                break;
                            case "cmdEditProv":
                                editProvClick();
                                break;
                            case "cmdEditExt":
                                editExtClick();
                                break;
                            case "cmdEditContactGroup":
                                editContactsGroupClick();
                                break;
                            case "cmdEditContact":
                                editContactClick();
                                break;
                            case "cmdEditSmtp":
                                editSmtpClick();
                                break;
                            case "cmdEditMailContact":
                                editMailContactClick();
                                break;
                            case "cmdEditComp":
                                editSyncCompClick();
                                break;
                            case "cmdDeleteProv":
                                deleteProvClick();
                                break;
                            case "cmdDeleteExt":
                                deleteExtClick();
                                break;
                            case "cmdDeleteTag":
                                deleteTagClick();
                                break;
                            case "cmdRemoveSound":
                                removeSoundClick();
                                break;
                            case "cmdDeleteContactGroup":
                                deleteContactGroupClick();
                                break;
                            case "cmdDeleteContact":
                                deleteContactClick();
                                break;
                            case "cmdDeleteSmtp":
                                deleteSmtpClick();
                                break;
                            case "cmdDeleteMailContact":
                                deleteMailContactClick();
                                break;
                            case "cmdRemoveComp":
                                removeSyncCompClick();
                                break;
                            case "cmdListenSound":
                                listenSoundClick();
                                break;
                            case "cmdClearMailContacts":
                                clearMailContactsClick();
                                break;
                            case "cmdImportMailContact":
                                importMailContactClick();
                                break;
                        }
                        break;
                    case CommandType.DefaultVoice:
                        defVoiceClick();
                        break;
                    case CommandType.VoiceSample:
                        voiceSampleClick();
                        break;
                    case CommandType.StandardView:
                        standardViewClick();
                        break;
                    case CommandType.GroupIcon:
                        changeIconClick();
                        break;
                    case CommandType.SkinlessCaptionFont:
                        fontSknlsClick();
                        break;
                    case CommandType.HotkeysCP:
                        hotkeysClick();
                        break;
                    case CommandType.MenusManagementCP:
                        menusManagementClick();
                        break;
                    case CommandType.CheckConnection:
                        checkConnectionClick();
                        break;
                    case CommandType.CheckMessages:
                        checkForNotesClick();
                        break;
                    case CommandType.CheckPluginsUpdate:
                        checkPluginsUpdateClick();
                        break;
                    case CommandType.RemovePlugin:
                        switch (e.Parameter.ToString())
                        {
                            case "cmdRemovePostPlugin":
                                removePostPluginClick();
                                break;
                            case "cmdRemoveSyncPlugin":
                                removeSyncPluginClick();
                                break;
                        }
                        break;
                    case CommandType.SyncNow:
                        syncNowClick();
                        break;
                    case CommandType.ImportGmail:
                        impGmailClick(command.Text);
                        break;
                    case CommandType.ImportOutlook:
                        impOutlookClick(command.Text);
                        break;
                    case CommandType.ImportLotus:
                        impLotusClick(command.Text);
                        break;
                    case CommandType.PwrdCreate:
                        createPwrdClick();
                        break;
                    case CommandType.PwrdChange:
                        changePwrdClick();
                        break;
                    case CommandType.PwrdRemove:
                        removePwrdClick();
                        break;
                    case CommandType.BrowseButton:
                        dataDirClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }
    }
}

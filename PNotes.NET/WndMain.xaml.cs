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

using Ionic.Zip;
using Microsoft.Win32;
using PluginsCore;
using PNEncryption;
using PNRichEdit;
using PNStaticFonts;
using PNWCFLib;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WPFStandardStyles;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Control = System.Windows.Controls.Control;
using Cursors = System.Windows.Input.Cursors;
using MenuItem = System.Windows.Controls.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using RichTextBox = System.Windows.Forms.RichTextBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using Screen = System.Windows.Forms.Screen;
using SystemColors = System.Windows.SystemColors;
using TextDataFormat = System.Windows.TextDataFormat;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WndMain : IPluginsHost
    {
        internal event EventHandler<NoteBooleanChangedEventArgs> NoteBooleanChanged;
        internal event EventHandler LanguageChanged;
        internal event EventHandler HotKeysChanged;
        internal event EventHandler<NoteNameChangedEventArgs> NoteNameChanged;
        internal event EventHandler<NoteGroupChangedEventArgs> NoteGroupChanged;
        internal event EventHandler<NoteDateChangedEventArgs> NoteDateChanged;
        internal event EventHandler<NoteDockStatusChangedEventArgs> NoteDockStatusChanged;
        internal event EventHandler<NoteSendReceiveStatusChangedEventArgs> NoteSendReceiveStatusChanged;
        internal event EventHandler NoteTagsChanged;
        internal event EventHandler ContentDisplayChanged;
        internal event EventHandler<NoteDeletedCompletelyEventArgs> NoteDeletedCompletely;
        internal event EventHandler<SpellCheckingStatusChangedEventArgs> SpellCheckingStatusChanged;
        internal event EventHandler SpellCheckingDictionaryChanged;
        internal event EventHandler<NewNoteCreatedEventArgs> NewNoteCreated;
        internal event EventHandler NoteScheduleChanged;
        internal event EventHandler NotesReceived;
        internal event EventHandler<MenusOrderChangedEventArgs> MenusOrderChanged;

        private const int CRITICAL_CHECK_INTERVAL = 600; //600 seconds (10 minutes)
        private const int WM_HOTKEY = 0x0312;

        private bool _ShowHide;
        private bool _InDblClick;
        private int _Elapsed;
        private readonly Timer _TimerPin = new Timer(100);
        private readonly Timer _TimerBackup = new Timer(20000);
        private readonly Timer _TimerMonitor = new Timer(1000);
        private readonly Timer _TimerSyncPlugins = new Timer();
        private readonly Timer _TimerSyncLocal = new Timer();
        private readonly System.Windows.Forms.Timer _TmrDblClick = new System.Windows.Forms.Timer { Interval = 100 };
        private List<string> _ReceivedNotes;
        private readonly IEnumerable<PNote> _PinnedNotes = PNCollections.Instance.Notes.Where(n => n.Pinned);
        private PNWCFHostRunner _HostRunner;
        private PinClass _PinClass;
        private bool _UnsubscribedCritical;
        private int _CheckCriticalTimeElapsed;
        private HwndSource _HwndSource;

        private readonly Dictionary<string, ToolStripMenuItem> _SyncPluginsMenus =
            new Dictionary<string, ToolStripMenuItem>();

        public WndMain()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
            var dp = DependencyPropertyDescriptor.FromProperty(FlowDirectionProperty, typeof(Window));
            dp?.AddValueChanged(this, delegate
            {
                if (ctmPN != null)
                {
                    ctmPN.FlowDirection = FlowDirection;
                }
            });
        }

        #region Window procedures

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNData.SaveExitFlag(-1);

                if (PNRuntimes.Instance.Settings.Protection.PasswordString != "")
                {
                    var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                    var result = d.ShowDialog();
                    if (result == null || !result.Value)
                    {
                        PNRuntimes.Instance.HideSplash = true;
                        Close();
                    }
                }

                PNWindows.Instance.FormPanel = new WndPanel();

                //set double click timer handler
                _TmrDblClick.Tick += tmrDblClick_Tick;

                PNSingleton.Instance.FontUser.PropertyChanged += FontUser_PropertyChanged;
                //save monitors count
                PNSingleton.Instance.MonitorsCount = Screen.AllScreens.Length;

                //save screens rectangle
                PNSingleton.Instance.ScreenRect = PNStatic.AllScreensBounds();

                ApplyNewDefaultMenu();

                // get local ip address
                PNSingleton.Instance.IpAddress = PNStatic.GetLocalIPv4(NetworkInterfaceType.Wireless80211) ??
                                                 PNStatic.GetLocalIPv4(NetworkInterfaceType.Ethernet);

                //var ips = Dns.GetHostEntry(Dns.GetHostName());
                //// Select the first entry. I hope it's this maschines IP
                //var ipAddress =
                //    ips.AddressList.FirstOrDefault(
                //        ip => ip.AddressFamily == AddressFamily.InterNetwork);
                //if (ipAddress != null)
                //{
                //    _IpAddress = ipAddress.ToString();
                //}

                //check startup shortcut
                checkStartUpShortcut();

                //subscribe to system events
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

                applyLanguage();
                //load/create database
                loadDatabase();

                //#if !DEBUG
                if (PNRuntimes.Instance.Settings.GeneralSettings.CheckCriticalOnStart)
                {
                    //check for critical updates (synchronously)
                    PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("critical_check",
                        "Checking for critical updates");
                    var result = checkCriticalUpdates();
                    if (result != CriticalUpdateAction.None)
                    {
                        PNRuntimes.Instance.HideSplash = true;
                        if ((result & CriticalUpdateAction.Program) == CriticalUpdateAction.Program)
                        {
                            Close();
                            return;
                        }
                        ApplyAction(MainDialogAction.Restart, null);
                        return;
                    }
                }
                //#endif
                PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_hotkeys", "Applying hot keys");
                //apply possible hot keys
                ApplyNewHotkeys();
                //register hot keys
                registerMainHotkeys();
                // register custom fonts
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseCustomFonts)
                {
                    PNInterop.AddCustomFonts();
                }
                PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_spellchecker",
                    "Initializing spell checker");
                //init spell checker
                initSpeller();

                //prepare docking collections
                PNCollections.Instance.DockedNotes.Add(DockStatus.Left, new List<PNote>());
                PNCollections.Instance.DockedNotes.Add(DockStatus.Top, new List<PNote>());
                PNCollections.Instance.DockedNotes.Add(DockStatus.Right, new List<PNote>());
                PNCollections.Instance.DockedNotes.Add(DockStatus.Bottom, new List<PNote>());
                //init dock arrows
                initDockArrows();

                // check exit flag and autosaved notes
                if (PNRuntimes.Instance.Settings.Config.ExitFlag != 0)
                {
                    restoreAutosavedNotes();
                }
                // clear all autosaved notes
                clearAutosavedNotes();

                //execute possible synchronization
                if (PNRuntimes.Instance.Settings.Network.SyncOnStart)
                {
                    PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("sync_in_progress",
                        "Synchronization in progress...");
                    var plugins = PNPlugins.Instance.SyncPlugins.Where(p => PNCollections.Instance.ActiveSyncPlugins.Contains(p.Name));
                    foreach (var p in plugins)
                    {
                        switch (p.Synchronize())
                        {
                            case SyncResult.Reload:
                                PNData.UpdateTablesAfterSync();
                                break;
                            case SyncResult.AbortVersion:
                                WPFMessageBox.Show(
                                    PNLang.Instance.GetMessageText("diff_versions",
                                        "Current version of database is different from previously synchronized version. Synchronization cannot be performed."),
                                    p.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                break;
                        }
                    }
                }

                // define autosave timer procedures
                TimerAutosave.Elapsed += TimerAutosave_Elapsed;
                if (PNRuntimes.Instance.Settings.GeneralSettings.Autosave)
                {
                    TimerAutosave.Interval = PNRuntimes.Instance.Settings.GeneralSettings.AutosavePeriod * 60000;
                    TimerAutosave.Start();
                }

                //check skins existance
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                {
                    PNRuntimes.Instance.Settings.GeneralSettings.UseSkins = PNStatic.CheckSkinsExistance();
                }

                if (PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                {
                    PNWindows.Instance.FormPanel.Show();
                }

                PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_notes", "Loading notes");
                loadNotes();

                PNRuntimes.Instance.HideSplash = true;

                showPriorityOnStart();

                // define clean bin timer procedures
                TimerCleanBin.Elapsed += TimerCleanBin_Elapsed;
                if (PNRuntimes.Instance.Settings.GeneralSettings.RemoveFromBinPeriod > 0)
                {
                    TimerCleanBin.Start();
                }

                PNRuntimes.Instance.StartTime = DateTime.Now;

                adjustStartTimes();

                // start listening thread
                if (PNRuntimes.Instance.Settings.Network.EnableExchange)
                {
                    //StartListening();
                    StartWCFHosting();
                }

                // check overdue notes
                checkOverdueNotes();

                ntfPN.Visibility = Visibility.Visible;

                if (PNData.FirstRun)
                {
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("first_baloon_caption",
                            "Thank you for using PNotes.NET!"));
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("first_baloon_message",
                        "Right click on system tray icon to begin the work."));
                    var baloon = new Baloon(BaloonMode.FirstRun) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 15000);
                    //create welcome note
                    var note = newNoteClick();
                    note.Dialog.Topmost = true;
                    note.Dialog.Edit.SelectedRtf = PNStrings.WELCOME_NOTE_RTF;
                    note.Dialog.Height += 16;
                    note.Dialog.Edit.SelectionStart = 0;
                    note.Dialog.Topmost = false;
                }
                else if (PNSingleton.Instance.PlatformChanged)
                {
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("new_edition",
                            "You have started the new edition of PNotes.NET. All your notes and settings from previous edition have been saved as ZIP archive at:"));
                    sb.AppendLine();
                    sb.Append(System.Windows.Forms.Application.StartupPath);
                    sb.Append(@"\");
                    sb.Append(PNStrings.OLD_EDITION_ARCHIVE);
                    var baloon = new Baloon(BaloonMode.Information) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 15000);
                }
                //show Control Panel
                if (PNRuntimes.Instance.Settings.GeneralSettings.ShowCPOnStart)
                {
                    execMenuCommand(mnuCP);
                }

                //enable pin timer
                _TimerPin.Elapsed += TimerPin_Elapsed;
                _TimerPin.Start();

                //enable backup timer
                _TimerBackup.Elapsed += TimerBackup_Elapsed;
                _TimerBackup.Start();

                //enable monitor timer
                _TimerMonitor.Elapsed += _TimerMonitor_Elapsed;
                _TimerMonitor.Start();

                _TimerSyncPlugins.Elapsed += _TimerSyncPlugins_Elapsed;
                if (PNRuntimes.Instance.Settings.Network.SilentSyncType == SilentSyncType.Period && PNRuntimes.Instance.Settings.Network.SilentSyncPeriod > 0)
                {
                    _TimerSyncPlugins.Interval = PNRuntimes.Instance.Settings.Network.SilentSyncPeriod * 60000.0;
                    _TimerSyncPlugins.Start();
                }
                else if (PNRuntimes.Instance.Settings.Network.SilentSyncType == SilentSyncType.Time)
                {
                    _TimerSyncPlugins.Interval = 60000.0;
                    _TimerSyncPlugins.Start();
                }

                _TimerSyncLocal.Elapsed += _TimerSyncLocal_Elapsed;
                if (PNRuntimes.Instance.Settings.Protection.LocalSyncType == SilentSyncType.Period && PNRuntimes.Instance.Settings.Protection.LocalSyncPeriod > 0)
                {
                    _TimerSyncLocal.Interval = PNRuntimes.Instance.Settings.Protection.LocalSyncPeriod * 60000.0;
                    _TimerSyncLocal.Start();
                }
                else if (PNRuntimes.Instance.Settings.Protection.LocalSyncType == SilentSyncType.Time)
                {
                    _TimerSyncLocal.Interval = 60000.0;
                    _TimerSyncLocal.Start();
                }

                //hide possible hidden menus
                PNStatic.HideMenus(ctmPN, PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.Main).ToArray());

                //check for new version
                if (PNRuntimes.Instance.Settings.GeneralSettings.CheckNewVersionOnStart)
                {
                    checkForNewVersion();
                }

                //create dropper cursor
                PNSingleton.Instance.DropperCursor = PNStatic.CreateCursorFromResource("cursors/dropper.cur");

                var criticalLog = Path.Combine(Path.GetTempPath(), PNStrings.CRITICAL_UPDATE_LOG);
                if (File.Exists(criticalLog))
                {
                    using (var sr = new StreamReader(criticalLog))
                    {
                        while (sr.Peek() != -1)
                        {
                            PNStatic.LogThis("Critical udate has been applied for " + sr.ReadLine());
                        }
                    }
                    File.Delete(criticalLog);
                    var sb =
                        new StringBuilder(PNLang.Instance.GetMessageText("critical_applied",
                            "The program has restarted for applying critical updates."));
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("send_error_2",
                        "Please, refer to log file for details."));
                    var baloon = new Baloon(BaloonMode.CriticalUpdates) { BaloonText = sb.ToString() };
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                }
                if (PNSingleton.Instance.NoteFromShortcut != "")
                {
                    showNoteFromShortcut(PNSingleton.Instance.NoteFromShortcut);
                    PNSingleton.Instance.NoteFromShortcut = "";
                }
                restoreNotesShortcuts();
                if (PNRuntimes.Instance.Settings.Network.StoreOnServer)
                {
                    //possible check for notes on server
                    var clientRunner = new PNWCFClientRunner();
                    Task.Factory.StartNew(
                        () =>
                            clientRunner.CheckMessages(PNRuntimes.Instance.Settings.Network.ServerIp,
                                PNRuntimes.Instance.Settings.Network.ServerPort));
                }
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.IsMainWindowLoaded = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                PNSingleton.Instance.AppClosed = true;
                _HwndSource?.RemoveHook(WndProc);
                // stop timers
                TimerAutosave.Stop();
                TimerCleanBin.Stop();
                _TimerPin.Stop();
                _TimerBackup.Stop();
                _TimerMonitor.Stop();
                _TimerSyncLocal.Stop();
                _TimerSyncPlugins.Stop();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // stop listening
                StopWCFHosting();

                //unsubscribe to system events
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                //stop spell checking
                Spellchecking.HuspellStop();

                //save all notes if needed
                if (!PNSingleton.Instance.ExitWithoutSaving)
                {
                    if (PNRuntimes.Instance.Settings.GeneralSettings.SaveOnExit)
                    {
                        //close possible SaveAs dialogs in order to prevent duplicates
                        foreach (Window wd in Application.Current.Windows)
                        {
                            if (wd.Name == "DlgSaveAs")
                                wd.DialogResult = false;
                        }
                        //save notes
                        PNNotesOperations.SaveAllNotes(!PNSingleton.Instance.DoNotAskIfSave);
                    }
                }
                //save all notes shortcuts if needed
                if (PNRuntimes.Instance.Settings.GeneralSettings.DeleteShortcutsOnExit)
                {
                    PNNotesOperations.DeleteAllNotesShortcuts();
                }
                // unregister custom fonts
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseCustomFonts)
                {
                    PNInterop.RemoveCustomFonts();
                }
                //unregister hot keys
                unregisterMainHotkeys();
                //free all groups
                foreach (var g in PNCollections.Instance.Groups)
                {
                    g.Dispose();
                }
                //free all notes
                foreach (var n in PNCollections.Instance.Notes)
                {
                    n.Dispose();
                }

                //save exit flag
                PNData.SaveExitFlag(0);

#if !DEBUG
// clean registry
                PNRegistry.CleanRegRunMRU();
                PNRegistry.CleanRegMUICache();
                PNRegistry.CleanRegOpenWithList();
                PNRegistry.CleanRegOpenSaveMRU();
#endif
                //compact databases
                PNData.CompactDatabases();

                //dispose dropper cursor
                if (PNSingleton.Instance.DropperCursor != null)
                    PNSingleton.Instance.DropperCursor.Dispose();

                ntfPN.Dispose();

                //close all windows
                foreach (Window w in Application.Current.Windows)
                    w.Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = (new WindowInteropHelper(this)).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
            _HwndSource?.AddHook(WndProc);
            if (!PNRuntimes.Instance.Settings.Behavior.HideMainWindow) return;
            var exStyle = PNInterop.GetWindowLong(handle, PNInterop.GWL_EXSTYLE);
            exStyle |= PNInterop.WS_EX_TOOLWINDOW;
            PNInterop.SetWindowLong(handle, PNInterop.GWL_EXSTYLE, exStyle);
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    var id = wParam.ToInt32();
                    var hk = PNCollections.Instance.HotKeysMain.FirstOrDefault(h => h.Id == id);
                    if (hk != null)
                    {
                        if (PNCollections.Instance.HiddenMenus.Any(hm => hm.Type == MenuType.Main && hm.Name == hk.MenuName))
                            return IntPtr.Zero;

                        var menuItem = PNStatic.GetMenuByName(ctmPN, hk.MenuName);
                        if (menuItem == null)
                            return IntPtr.Zero;
                        if (!execMenuCommand(menuItem)) return IntPtr.Zero;
                        handled = true;

                        //prevent edit change on hot key
                        foreach (var d in Application.Current.Windows.OfType<WndNote>())
                            d.InHotKey = true;
                    }
                    else
                    {
                        hk = PNCollections.Instance.HotKeysNote.FirstOrDefault(h => h.Id == id);
                        if (hk != null)
                        {
                            var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                            if (d == null) return IntPtr.Zero;
                            //prevent edit change on hot key
                            d.InHotKey = true;
                            d.HotKeyClick(hk);
                            handled = true;
                        }
                        else
                        {
                            hk = PNCollections.Instance.HotKeysEdit.FirstOrDefault(h => h.Id == id);
                            if (hk != null)
                            {
                                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                                if (d == null) return IntPtr.Zero;
                                //prevent edit change on hot key
                                d.InHotKey = true;
                                d.HotKeyClick(hk);
                                handled = true;
                            }
                            else
                            {
                                hk = PNCollections.Instance.HotKeysGroups.FirstOrDefault(h => h.Id == id);
                                if (hk == null) return IntPtr.Zero;
                                if (!hk.MenuName.Contains('_')) return IntPtr.Zero;
                                var pos = hk.MenuName.IndexOf('_');
                                var groupId = int.Parse(hk.MenuName.Substring(0, pos), PNRuntimes.Instance.CultureInvariant);
                                var action = hk.MenuName.Substring(pos + 1);
                                PNNotesOperations.ShowHideSpecificGroup(groupId, action.ToUpper() == "SHOW");
                                handled = true;
                            }
                        }
                    }
                    break;
                case PNInterop.WM_ACTIVATEAPP:
                    if (wParam.ToInt32() == 0)
                    {
                        PNStatic.DeactivateNotesWindows();
                    }
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_PROG:
                    if (!execMenuCommand(mnuExit)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_SILENT_SAVE:
                    PNSingleton.Instance.DoNotAskIfSave = true;
                    if (!execMenuCommand(mnuExit)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_CLOSE_SILENT_WO_SAVE:
                    PNSingleton.Instance.ExitWithoutSaving = true;
                    if (!execMenuCommand(mnuExit)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_NOTE:
                    if (!execMenuCommand(mnuNewNote)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_NOTE_FROM_CB:
                    if (!execMenuCommand(mnuNoteFromClipboard)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_NEW_DIARY:
                    if (!execMenuCommand(mnuTodayDiary)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_RELOAD_NOTES:
                    if (!execMenuCommand(mnuReloadAll)) return IntPtr.Zero;
                    handled = true;
                    break;
                case PNInterop.WPM_START_FROM_ANOTHER_INSTANCE:
                    bool? result = true;
                    if (PNRuntimes.Instance.Settings.Protection.PasswordString != "" && PNSingleton.Instance.IsLocked)
                    {
                        var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                        result = d.ShowDialog();
                    }
                    if (result == null || !result.Value) return IntPtr.Zero;
                    actionDoubleSingle(PNRuntimes.Instance.Settings.Behavior.DoubleClickAction);
                    handled = true;
                    break;
                case PNInterop.WM_COPYDATA:
                    var m = Message.Create(hWnd, msg, wParam, lParam);
                    var cpdata =
                        (PNInterop.CopyDataStruct)m.GetLParam(typeof(PNInterop.CopyDataStruct));
                    switch ((CopyDataType)cpdata.dwData.ToInt32())
                    {
                        case CopyDataType.NewNote:
                            var note = newNoteClick();
                            if (note?.Dialog != null)
                            {
                                var arr = cpdata.lpData.Split(new[] { PNStrings.DEL_CHAR },
                                    StringSplitOptions.RemoveEmptyEntries);
                                note.Dialog.Edit.Text = arr[0];
                                if (arr.Length >= 2)
                                {
                                    note.Name = arr[1];
                                    note.Dialog.PHeader.Title = note.Dialog.Title = note.Name;
                                    note.Dialog.SaveNoteFile(note);
                                    PNNotesOperations.SaveNewNote(note);
                                    note.Dialog.ApplyTooltip();
                                    note.Changed = false;
                                }
                                if (arr.Length >= 3)
                                {
                                    note.Tags.AddRange(arr[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                    PNData.AddNewTags(note.Tags);
                                    PNNotesOperations.SaveNoteTags(note);
                                    note.RaiseTagsChangedEvent();
                                }
                            }
                            handled = true;
                            break;
                        case CopyDataType.LoadNotes:
                            if (cpdata.cbData == 0)
                            {
                                handled = true;
                                break;
                            }
                            var files = cpdata.lpData.Split(new[] { PNStrings.DEL_CHAR },
                                StringSplitOptions.RemoveEmptyEntries);
                            loadNotesFromFilesList(files);
                            handled = true;
                            break;
                        case CopyDataType.ShowNoteById:
                            if (cpdata.cbData == 0)
                            {
                                handled = true;
                                break;
                            }
                            if (PNSingleton.Instance.IsMainWindowLoaded)
                            {
                                var noteToShow = PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == cpdata.lpData);
                                if (noteToShow != null)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(noteToShow, true);
                                }
                            }
                            else
                            {
                                PNSingleton.Instance.NoteFromShortcut = cpdata.lpData;
                            }
                            handled = true;
                            break;
                        case CopyDataType.ExportNotes:
                            {
                                if (cpdata.cbData == 0)
                                {
                                    handled = true;
                                    break;
                                }
                                var arr = cpdata.lpData.Split("|");
                                if (arr.Length > 0)
                                {
                                    var fileName = arr[0];
                                    if (arr.Length == 1)
                                        exportNotes(fileName, null);
                                    else
                                    {
                                        var dates = new string[arr.Length - 1];
                                        Array.Copy(arr, 1, dates, 0, arr.Length - 1);
                                        if (!PNStatic.areDatesValidInCommandLine(dates))
                                        {
                                            handled = true;
                                            break;
                                        }
                                        exportNotes(fileName, dates);
                                    }
                                }
                                handled = true;
                                break;
                            }
                    }
                    break;
                case PNInterop.WPM_BACKUP:
                    createFullBackup(true);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion

        #region Timers

        private void tmrDblClick_Tick(object sender, EventArgs e)
        {
            try
            {
                _Elapsed += 100;
                if (_Elapsed < SystemInformation.DoubleClickTime) return;
                _TmrDblClick.Stop();
                _Elapsed = 0;
                actionDoubleSingle(PNRuntimes.Instance.Settings.Behavior.SingleClickAction);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void TimerDelegate(object sender, ElapsedEventArgs e);

        private void TimerPin_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerPin.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = TimerPin_Elapsed;
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
                    foreach (var note in _PinnedNotes)
                    {
                        _PinClass = new PinClass
                        {
                            Class = note.PinClass,
                            Pattern = PNStatic.CreatePinRegexPattern(note.PinText)
                        };
                        PNInterop.EnumWindowsProcDelegate enumProc = EnumWindowsProc;
                        PNInterop.EnumWindows(enumProc, 0);
                        if (!_PinClass.Hwnd.Equals(IntPtr.Zero))
                        {
                            var wp = new PNInterop.WindowPlacement();
                            wp.length = (uint)Marshal.SizeOf(wp);
                            PNInterop.GetWindowPlacement(_PinClass.Hwnd, ref wp);
                            if (wp.showCmd != PNInterop.SW_SHOWMINIMIZED)
                            {
                                if (!_PinClass.Hwnd.Equals(PNInterop.GetForegroundWindow())) continue;
                                // show note if it is not visible
                                if (!note.Visible)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(note, true);
                                }
                                if (note.Dialog == null) continue;
                                var topMost = note.Dialog.Topmost;
                                note.Dialog.Topmost = true;
                                note.Dialog.Topmost = topMost;
                            }
                            else
                            {
                                // hide pinned note if appropriate window is minimized and note is not in alarm state
                                if (note.Visible && note.Dialog != null && !note.Dialog.InAlarm)
                                {
                                    PNNotesOperations.ShowHideSpecificNote(note, false);
                                }
                            }
                        }
                        else
                        {
                            // hide pinned note if appropriate window was not found and note is not in alarm state
                            if (note.Visible && note.Dialog != null && !note.Dialog.InAlarm)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerPin.Start();
            }
        }

        private void TimerBackup_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerBackup.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = TimerBackup_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    var now = DateTime.Now;
                    if (PNRuntimes.Instance.Settings.Protection.FullBackupDays.Contains(now.DayOfWeek) &&
                        now.TimeOfDay >= PNRuntimes.Instance.Settings.Protection.FullBackupTime.TimeOfDay &&
                        now.Date != PNRuntimes.Instance.Settings.Protection.FullBackupDate.Date)
                    {
                        createFullBackup(true);
                        PNRuntimes.Instance.Settings.Protection.FullBackupDate = now.Date;
                        PNData.SaveProtectionSettings();
                    }
                    // check for critical updates here
                    if (PNSingleton.Instance.AppClosed) return;
                    if (_CheckCriticalTimeElapsed >= CRITICAL_CHECK_INTERVAL)
                    {
                        _CheckCriticalTimeElapsed = 0;
                        if (!PNRuntimes.Instance.Settings.GeneralSettings.CheckCriticalPeriodically) return;
                        var result = checkCriticalUpdates();
                        if ((result & CriticalUpdateAction.Program) == CriticalUpdateAction.Program)
                        {
                            Close();
                        }
                        else if ((result & CriticalUpdateAction.Plugins) == CriticalUpdateAction.Plugins)
                        {
                            ApplyAction(MainDialogAction.Restart, null);
                        }
                    }
                    else
                    {
                        _CheckCriticalTimeElapsed += (int)_TimerBackup.Interval / 1000;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerBackup.Start();
            }
        }

        private void _TimerMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerMonitor.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = _TimerMonitor_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    if (isMonitorPluggedUnplugged()) return;
                    isScreenRectangleChanged();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerMonitor.Start();
            }
        }

        private void _TimerSyncLocal_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerSyncLocal.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = _TimerSyncLocal_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    if (PNRuntimes.Instance.Settings.Protection.LocalSyncType == SilentSyncType.Time)
                    {
                        var now = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                        if (PNRuntimes.Instance.Settings.Protection.LocalSyncTime != now)
                            return;
                        else if (PNRuntimes.Instance.Settings.Config.LastLocalSync == DateTime.Today)
                            return;
                    }
                    var sync = new PNSync();
                    sync.SyncComplete += sync_SyncComplete;
                    PNStatic.LogThis("Begin local synchronization");
                    Task.Factory.StartNew(() => sync.SyncLocal(PNRuntimes.Instance.Settings.Protection.LocalSyncFilesLocation));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerSyncLocal.Start();
            }
        }

        private void sync_SyncComplete(object sender, LocalSyncCompleteEventArgs e)
        {
            try
            {
                if (sender is PNSync sync)
                {
                    sync.SyncComplete -= sync_SyncComplete;
                }

                switch (e.Result)
                {
                    case LocalSyncResult.Reload:
                        if (PNRuntimes.Instance.Settings.Protection.AutomaticSyncRestart)
                        {
                            PNData.UpdateTablesAfterSync();
                            PNStatic.LogThis("Local syncronization completed successfully. The proram is going to be restarted automatically.");
                            PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        }
                        else
                        {
                            PNStatic.LogThis("Local syncronization completed successfully. The program has to be restarted for applying all changes.");
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("sync_complete_reload",
                                    "Syncronization completed successfully. The program has to be restarted for applying all changes."),
                                "(Local)", MessageBoxButton.OK);
                            PNData.UpdateTablesAfterSync();
                        }
                        break;
                    case LocalSyncResult.AbortVersion:
                        PNStatic.LogThis(
                            "Current version of database is different from previously synchronized version. Local synchronization cannot be performed.");
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("diff_versions",
                                "Current version of database is different from previously synchronized version. Synchronization cannot be performed."),
                            "(Local)", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case LocalSyncResult.Error:
                        PNStatic.LogThis("An error occurred during local synchronization");
                        WPFMessageBox.Show("An error occurred during local synchronization", PNStrings.PROG_NAME, MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                    case LocalSyncResult.None:
                        PNStatic.LogThis("End local synchronization");
                        break;
                }
                if (PNRuntimes.Instance.Settings.Protection.LocalSyncType == SilentSyncType.Time)
                {
                    PNRuntimes.Instance.Settings.Config.LastLocalSync = DateTime.Today;
                    PNData.SaveLastSync(true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _TimerSyncPlugins_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                _TimerSyncPlugins.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = _TimerSyncPlugins_Elapsed;
                    try
                    {
                        Dispatcher.Invoke(d, sender, e);
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing when main form is disposed
                    }
                }
                else
                {
                    if (PNRuntimes.Instance.Settings.Network.SilentSyncType == SilentSyncType.Time)
                    {
                        var now = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                        if (PNRuntimes.Instance.Settings.Network.SilentSyncTime != now)
                            return;
                        else if (PNRuntimes.Instance.Settings.Config.LastPluginSync == DateTime.Today)
                            return;
                    }
                    syncInBackground();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    _TimerSyncPlugins.Start();
            }
        }

        private void TimerCleanBin_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                TimerCleanBin.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = TimerCleanBin_Elapsed;
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
                    var now = DateTime.Now;
                    var notes =
                        PNCollections.Instance.Notes.Where(
                            n =>
                                n.GroupId == (int)SpecialGroups.RecycleBin &&
                                (now - n.DateDeleted).Days >= PNRuntimes.Instance.Settings.GeneralSettings.RemoveFromBinPeriod);
                    var pNotes = notes as PNote[] ?? notes.ToArray();
                    if (!pNotes.Any()) return;
                    var proceed = true;
                    if (PNRuntimes.Instance.Settings.GeneralSettings.WarnOnAutomaticalDelete)
                    {
                        var sb = new StringBuilder();
                        sb.Append(PNLang.Instance.GetMessageText("clean_bin_1",
                            "The following notes will be permanently deleted from Recycle Bin:"));
                        foreach (var n in pNotes)
                        {
                            sb.AppendLine();
                            sb.Append(n.Name);
                        }
                        sb.AppendLine();
                        sb.Append(PNLang.Instance.GetMessageText("clean_bin_2",
                            "Choose \"Yes\" to continue or \"No\" to postpone the deletion to a future time."));
                        if (
                            WPFMessageBox.Show(sb.ToString(), PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            proceed = false;
                        }
                    }
                    if (proceed)
                    {
                        var aNotes = pNotes.ToArray();
                        for (var i = aNotes.Length - 1; i >= 0; i--)
                        {
                            PNNotesOperations.RemoveDeletedNoteFromLists(aNotes[i]);
                            var id = aNotes[i].Id;
                            var groupId = aNotes[i].GroupId;
                            PNNotesOperations.SaveNoteDeletedState(aNotes[i], false);
                            PNNotesOperations.DeleteNoteCompletely(aNotes[i], CompleteDeletionSource.AutomaticCleanBin);
                            RaiseDeletedCompletelyEvent(id, groupId);
                        }
                    }
                    else
                    {
                        foreach (var n in pNotes)
                        {
                            n.DateDeleted = now;
                            PNNotesOperations.SaveNoteDeletedDate(n);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    TimerCleanBin.Start();
            }
        }

        private void TimerAutosave_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                TimerAutosave.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    TimerDelegate d = TimerAutosave_Elapsed;
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
                    var notes = PNCollections.Instance.Notes.Where(n => n.FromDB && n.Changed && n.Visible);
                    foreach (var n in notes)
                    {
                        var destPath = Path.Combine(PNPaths.Instance.DataDir, n.Id + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                        if (File.Exists(destPath))
                        {
                            File.SetAttributes(destPath, FileAttributes.Normal);
                        }
                        PNNotesOperations.SaveNoteFile(n.Dialog.Edit, destPath);
                        File.SetAttributes(destPath, FileAttributes.Hidden);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                if (!PNSingleton.Instance.AppClosed)
                    TimerAutosave.Start();
            }
        }

        #endregion

        #region Internal properties

        internal IntPtr Handle => _HwndSource?.Handle ?? IntPtr.Zero;

        internal Timer TimerCleanBin { get; } = new Timer(1000);

        internal Timer TimerAutosave { get; } = new Timer();

        #endregion

        #region Internal procedures

        internal void RaiseContentDisplayChangedEevent()
        {
            try
            {
                ContentDisplayChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void DuplicateNote(PNote src)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(src))
                        return;
                var note = new PNote(src);
                note.Dialog = new WndNote(note, NewNoteMode.Duplication);
                PNCollections.Instance.Notes.Add(note);
                note.Visible = true;

                if (src.Visible)
                    note.NoteSize = src.Dialog.GetSize();
                else
                    note.NoteSize = new Size(PNRuntimes.Instance.Settings.GeneralSettings.Width,
                        PNRuntimes.Instance.Settings.GeneralSettings.Height);
                
                note.EditSize = note.Dialog.Edit.Size;
                note.Dialog.Show();
                // get location only after showing
                note.NoteLocation = note.Dialog.GetLocation();

                if (src.Visible)
                {
                    note.Dialog.Edit.Rtf = src.Dialog.Edit.Rtf;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, src.Id) + PNStrings.NOTE_EXTENSION;
                    PNNotesOperations.LoadNoteFile(note.Dialog.Edit, path);
                }

                NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void RaiseDeletedCompletelyEvent(string id, int groupId)
        {
            NoteDeletedCompletely?.Invoke(this, new NoteDeletedCompletelyEventArgs(id, groupId));
        }

        internal void RefreshHiddenMenus()
        {
            try
            {
                var hiddens = PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.Main).ToArray();
                PNStatic.HideMenus(ctmPN, hiddens);
                PNNotesOperations.RefreshHiddenMenus();
                if (PNWindows.Instance.FormCP != null)
                {
                    hiddens = PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.ControlPanel).ToArray();
                    PNStatic.HideMenus(PNWindows.Instance.FormCP.CPMenu, hiddens);
                    PNWindows.Instance.FormCP.HideToolbarButtons(hiddens);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void ShowSentNotificationDelegate(
            IEnumerable<string> notes, IEnumerable<string> recipients, SendResult result);
        internal void ShowSentNotification(IEnumerable<string> notes, IEnumerable<string> recipients, SendResult result)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ShowSentNotificationDelegate d = ShowSentNotification;
                    Dispatcher.Invoke(d, notes, recipients, result);
                }
                else
                {
                    var sb = new StringBuilder();
                    var mode = BaloonMode.NoteSent;
                    int? waitTimeout = null;
                    switch (result)
                    {
                        case SendResult.SentToContact:
                            sb.Append(PNLang.Instance.GetCaptionText("sent", "Notes successfully sent"));
                            waitTimeout = 10000;
                            break;
                        case SendResult.SentToServer:
                            sb.Append(PNLang.Instance.GetCaptionText("sent_to_server", "Notes sent to server due to unreachable recipients"));
                            mode = BaloonMode.Information;
                            break;
                        case SendResult.NoAdapter:
                            sb.Append(PNLang.Instance.GetMessageText("no_net_adapter", "No network adapter found"));
                            PNStatic.LogThis("Failed to send note. No network adapter found");
                            mode = BaloonMode.Error;
                            break;
                        case SendResult.CompNotOnNetwork:
                            sb.Append(PNLang.Instance.GetMessageText("contacts_disconnected",
                                "Computers of one or more contacts are not connected to network"));
                            PNStatic.LogThis("Failed to send note. Computers of one or more contacts are not connected to network");
                            mode = BaloonMode.Error;
                            break;
                        case SendResult.CompNameNoteFound:
                            sb.Append(PNLang.Instance.GetMessageText("hosts_unknown",
                                "One or more computers cannot be found on network"));
                            PNStatic.LogThis("Failed to send note. One or more computers cannot be found on network");
                            mode = BaloonMode.Error;
                            break;
                        case SendResult.CompAddressNotFound:
                            sb.Append(PNLang.Instance.GetMessageText("address_not_found",
                                "One or more of specified IP addresses cannot be found"));
                            PNStatic.LogThis("Failed to send note. One or more of specified IP addresses cannot be found");
                            mode = BaloonMode.Error;
                            break;
                        case SendResult.Failed:
                            sb.Append(PNLang.Instance.GetMessageText("send_error_1",
                                          "An error occurred during note(s) sending.") + "\n" +
                                      PNLang.Instance.GetMessageText("send_error_2",
                                          "Please, refer to log file for details."));
                            mode = BaloonMode.Error;
                            break;
                    }
                    sb.Append(": ");
                    foreach (var s in notes)
                    {
                        sb.Append(s);
                        sb.Append(";");
                    }
                    if (sb.Length > 1) sb.Length -= 1;
                    sb.AppendLine();
                    sb.Append(PNLang.Instance.GetMessageText("recipient", "Recipient(s):"));
                    sb.Append(" ");
                    foreach (var s in recipients)
                    {
                        sb.Append(s);
                        sb.Append(";");
                    }
                    if (sb.Length > 1) sb.Length -= 1;
                    var baloon =
                        new Baloon(mode)
                        {
                            BaloonText = sb.ToString()
                        };
                    if (ntfPN.CustomBalloon != null)
                        ntfPN.CloseBalloon();
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, waitTimeout);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ShowSendProgressBaloon()
        {
            try
            {
                ntfPN.ShowCustomBalloon(
                    new Baloon(BaloonMode.Sending)
                    {
                        BaloonText =
                            PNLang.Instance.GetMessageText("sending_progress", "Send operation in progress...")
                    }, PopupAnimation.Slide, null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void StartWCFHosting()
        {
            try
            {
                _HostRunner = new PNWCFHostRunner();
                _HostRunner.PNDataReceived += _HostRunner_PNDataReceived;
                _HostRunner.PNDataError += _HostRunner_PNDataError;
                Task.Factory.StartNew(
                    () =>
                        _HostRunner.StartHosting(
                            PNRuntimes.Instance.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture)));
                //var t = new Thread(() => _HostRunner.StartHosting(PNRuntimes.Instance.Settings.Network.ExchangePort.ToString(CultureInfo.InvariantCulture)));
                //t.Start();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void StopWCFHosting()
        {
            try
            {
                if (_HostRunner != null)
                {
                    _HostRunner.StopHosting();
                    _HostRunner.PNDataReceived -= _HostRunner_PNDataReceived;
                    _HostRunner.PNDataError -= _HostRunner_PNDataError;
                    _HostRunner = null;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void _HostRunner_PNDataError(object sender, PNDataErrorEventArgs e)
        {
            PNStatic.LogException(e.Exception);
        }

        private delegate void PNDataReceivedDelegate(object sender, PNDataReceivedEventArgs e);

        void _HostRunner_PNDataReceived(object sender, PNDataReceivedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    PNDataReceivedDelegate d = _HostRunner_PNDataReceived;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    receiveNote(e.Data);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewLanguage(string languageName)
        {
            try
            {
                PNLang.Instance.LoadLanguage(Path.Combine(PNPaths.Instance.LangDir, languageName));
                applyLanguage();
                applyStandardGroupsNames();

                //Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern =
                //    PNStatic.GetShortTimeFormat(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat);

                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, false);
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);

                LanguageChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewHotkeys()
        {
            try
            {
                foreach (var ti in ctmPN.Items.OfType<MenuItem>())
                {
                    applyMenuHotkey(ti);
                }

                HotKeysChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySpellStatusChange(bool status)
        {
            try
            {
                SpellCheckingStatusChanged?.Invoke(this, new SpellCheckingStatusChangedEventArgs(status));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySpellDictionaryChange()
        {
            try
            {
                SpellCheckingDictionaryChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyDoAlarm(PNote note)
        {
            try
            {
                if (PNNotesOperations.ShowHideSpecificNote(note, true) == ShowHideResult.Success)
                {
                    note.Dialog.InAlarm = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void ApplyActionDelegate(MainDialogAction action, object data);
        internal void ApplyAction(MainDialogAction action, object data)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ApplyActionDelegate d = ApplyAction;
                    try
                    {
                        Dispatcher.Invoke(d, action, data);
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
                    switch (action)
                    {
                        case MainDialogAction.Restart:
                            PNSingleton.Instance.Restart = true;
                            Close();
                            break;
                        case MainDialogAction.ReloadAll:
                            execMenuCommand(mnuReloadAll);
                            break;
                        case MainDialogAction.Help:
                            execMenuCommand(mnuHelp);
                            break;
                        case MainDialogAction.Preferences:
                            execMenuCommand(mnuPrefs);
                            break;
                        case MainDialogAction.SaveAll:
                            execMenuCommand(mnuSaveAll);
                            break;
                        case MainDialogAction.Support:
                            execMenuCommand(mnuSupport);
                            break;
                        case MainDialogAction.FullBackupCreate:
                            execMenuCommand(mnuBackupCreate);
                            break;
                        case MainDialogAction.FullBackupRestore:
                            execMenuCommand(mnuBackupRestore);
                            break;
                        case MainDialogAction.LocalSync:
                            execMenuCommand(mnuSyncLocal);
                            break;
                        case MainDialogAction.ShowAll:
                            execMenuCommand(mnuShowAll);
                            break;
                        case MainDialogAction.HideAll:
                            execMenuCommand(mnuHideAll);
                            break;
                        case MainDialogAction.BringToFront:
                            execMenuCommand(mnuAllToFront);
                            break;
                        case MainDialogAction.DockAllNone:
                            execMenuCommand(mnuDAllNone);
                            break;
                        case MainDialogAction.DockAllLeft:
                            execMenuCommand(mnuDAllLeft);
                            break;
                        case MainDialogAction.DockAllTop:
                            execMenuCommand(mnuDAllTop);
                            break;
                        case MainDialogAction.DockAllRight:
                            execMenuCommand(mnuDAllRight);
                            break;
                        case MainDialogAction.DockAllBottom:
                            execMenuCommand(mnuDAllBottom);
                            break;
                        case MainDialogAction.SearchInNotes:
                            execMenuCommand(mnuSearchInNotes);
                            break;
                        case MainDialogAction.SearchByTags:
                            execMenuCommand(mnuSearchByTags);
                            break;
                        case MainDialogAction.SearchByDates:
                            execMenuCommand(mnuSearchByDates);
                            break;
                        case MainDialogAction.NewNote:
                            execMenuCommand(mnuNewNote);
                            break;
                        case MainDialogAction.NewNoteInGroup:
                            newNoteInGroup(Convert.ToInt32(data));
                            break;
                        case MainDialogAction.NoteFromClipboard:
                            execMenuCommand(mnuNoteFromClipboard);
                            break;
                        case MainDialogAction.NoteFromClipboardInGroup:
                            newNoteFromClipboardInGroup(Convert.ToInt32(data));
                            break;
                        case MainDialogAction.LoadNotes:
                            loadNotesAsFiles(0);
                            break;
                        case MainDialogAction.LoadNotesInGroup:
                            loadNotesAsFiles(Convert.ToInt32(data));
                            break;
                        case MainDialogAction.ImportNotes:
                            execMenuCommand(mnuImportNotes);
                            break;
                        case MainDialogAction.ImportSettings:
                            execMenuCommand(mnuImportSettings);
                            break;
                        case MainDialogAction.ImportFonts:
                            execMenuCommand(mnuImportFonts);
                            break;
                        case MainDialogAction.ImportDictionaries:
                            execMenuCommand(mnuImportDictionaries);
                            break;
                        case MainDialogAction.About:
                            execMenuCommand(mnuAbout);
                            break;
                        case MainDialogAction.ExportNotes:
                            preExportNotes(Convert.ToString(data));
                            break;
                        case MainDialogAction.ConfigureReport:
                            execMenuCommand(mnuPrintFields);
                            break;
                        case MainDialogAction.SetSyncTimerByPeriod:
                            {
                                var period = (int)data;
                                _TimerSyncPlugins.Stop();
                                if (period > 0)
                                {
                                    _TimerSyncPlugins.Interval = period * 60000.0;
                                    _TimerSyncPlugins.Start();
                                }
                            }
                            break;
                        case MainDialogAction.SwitchSyncType:
                            {
                                _TimerSyncPlugins.Stop();
                                var tp = (Tuple<SilentSyncType, int>)data;
                                if (tp.Item1 == SilentSyncType.Time)
                                {
                                    _TimerSyncPlugins.Interval = 60000.0;
                                    _TimerSyncPlugins.Start();
                                }
                                else
                                {
                                    if (tp.Item2 > 0)
                                    {
                                        _TimerSyncPlugins.Interval = tp.Item2 * 60000.0;
                                        _TimerSyncPlugins.Start();
                                    }
                                }
                            }
                            break;
                        case MainDialogAction.SetLocalSyncTimerByPeriod:
                            {
                                var period = (int)data;
                                _TimerSyncLocal.Stop();
                                if (period > 0)
                                {
                                    _TimerSyncLocal.Interval = period * 60000.0;
                                    _TimerSyncLocal.Start();
                                }
                            }
                            break;
                        case MainDialogAction.SwitchLocalSyncType:
                            {
                                _TimerSyncLocal.Stop();
                                var tp = (Tuple<SilentSyncType, int>)data;
                                if (tp.Item1 == SilentSyncType.Time)
                                {
                                    _TimerSyncLocal.Interval = 60000.0;
                                    _TimerSyncLocal.Start();
                                }
                                else
                                {
                                    if (tp.Item2 > 0)
                                    {
                                        _TimerSyncLocal.Interval = tp.Item2 * 60000.0;
                                        _TimerSyncLocal.Start();
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewMenusOrder(MenusOrderChangedEventArgs e)
        {
            try
            {
                if (e.Main)
                {
                    PNMenus.RearrangeMenus(ctmPN);
                    PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);
                }

                MenusOrderChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyNewDefaultMenu()
        {
            try
            {
                if (PNSingleton.Instance.FontUser.FontFamily.FamilyTypefaces.All(tf => tf.Weight != FontWeights.Bold))
                    return;
                switch (PNRuntimes.Instance.Settings.Behavior.DoubleClickAction)
                {
                    case TrayMouseAction.BringAllToFront:
                        mnuAllToFront.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.ControlPanel:
                        mnuCP.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.LoadNote:
                        mnuLoadNote.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NewNote:
                        mnuNewNote.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NewNoteInGroup:
                        mnuNewNoteInGroup.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.NoteFromClipboard:
                        mnuNoteFromClipboard.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.Preferences:
                        mnuPrefs.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SaveAll:
                        mnuSaveAll.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchByDates:
                        mnuSearchByDates.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchByTags:
                        mnuSearchByTags.FontWeight = FontWeights.Bold;
                        break;
                    case TrayMouseAction.SearchInNotes:
                        mnuSearchInNotes.FontWeight = FontWeights.Bold;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void LoadNotesByList(List<string> listId, bool show)
        {
            try
            {
                foreach (string id in listId)
                {
                    loadSingleNote(id, show);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Private procedures

        private void restoreNotesShortcuts()
        {
            try
            {
                if (PNRuntimes.Instance.Settings.GeneralSettings.RestoreShortcutsOnStart)
                {
                    var ids = PNData.GetNotesWithShortcuts();
                    if (ids != null)
                    {
                        foreach (
                            var note in
                            ids.Select(id => PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == id)).Where(note => note != null)
                        )
                        {
                            PNNotesOperations.SaveAsShortcut(note);
                        }
                    }
                }
                //clear field
                PNData.SaveNotesWithShortcuts(null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showNoteFromShortcut(string id)
        {
            try
            {
                var noteToShow = PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == id);
                if (noteToShow != null)
                {
                    PNNotesOperations.ShowHideSpecificNote(noteToShow, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showRecentNotes(int dateDifference)
        {
            try
            {
                var notes =
                    PNCollections.Instance.Notes.Where(
                        n => n.GroupId != (int)SpecialGroups.RecycleBin &&
                             (Math.Abs(n.DateCreated.Subtract(DateTime.Now).Days) <= dateDifference ||
                              Math.Abs(n.DateSaved.Subtract(DateTime.Now).Days) <= dateDifference));
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

        private bool lockProgram(bool value)
        {
            try
            {
                var notes = PNCollections.Instance.Notes.Where(n => n.Visible);
                if (value)
                {
                    foreach (var n in notes.Where(n => n.Dialog != null))
                    {
                        n.Dialog.Hide();
                    }
                    ntfPN.ContextMenu = null;
                }
                else
                {
                    foreach (var n in notes.Where(n => n.Dialog != null))
                    {
                        n.Dialog.Show();
                    }
                    ntfPN.ContextMenu = ctmPN;
                }
                var vis = value ? Visibility.Hidden : Visibility.Visible;
                if (PNWindows.Instance.FormCP != null)
                {
                    PNWindows.Instance.FormCP.Visibility = vis;
                }
                if (PNWindows.Instance.FormSearchByDates != null)
                {
                    PNWindows.Instance.FormSearchByDates.Visibility = vis;
                }
                if (PNWindows.Instance.FormSearchByTags != null)
                {
                    PNWindows.Instance.FormSearchByTags.Visibility = vis;
                }
                if (PNWindows.Instance.FormSearchInNotes != null)
                {
                    PNWindows.Instance.FormSearchInNotes.Visibility = vis;
                }
                if (PNWindows.Instance.FormSettings != null)
                {
                    PNWindows.Instance.FormSettings.Visibility = vis;
                }
                if (PNWindows.Instance.FormPanel != null)
                {
                    PNWindows.Instance.FormPanel.Visibility = vis;
                }
                if (PNRuntimes.Instance.Settings.Protection.HideTrayIcon)
                {
                    if (value)
                    {
                        var hk = PNCollections.Instance.HotKeysMain.FirstOrDefault(h => h.MenuName == "mnuLockProg");
                        if (hk != null && hk.Shortcut == "")
                        {
                            var message = PNLang.Instance.GetMessageText("hide_on_lock_warning",
                                "In order to allow the tray icon to be hidden when program is locked you have to set a hot key for \"Lock Program\" menu item");
                            WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                            return true;
                        }
                    }
                    ntfPN.Visibility = vis;
                }
                return value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return !value;
            }
        }

        private void actionDoubleSingle(TrayMouseAction action)
        {
            try
            {
                switch (action)
                {
                    case TrayMouseAction.AllShowHide:
                        if (_ShowHide)
                        {
                            //hide all
                            execMenuCommand(mnuHideAll);
                            _ShowHide = false;
                        }
                        else
                        {
                            //show all
                            execMenuCommand(mnuShowAll);
                            _ShowHide = true;
                        }
                        break;
                    case TrayMouseAction.BringAllToFront:
                        execMenuCommand(mnuAllToFront);
                        break;
                    case TrayMouseAction.ControlPanel:
                        execMenuCommand(mnuCP);
                        break;
                    case TrayMouseAction.LoadNote:
                        execMenuCommand(mnuLoadNote);
                        break;
                    case TrayMouseAction.NewNote:
                        execMenuCommand(mnuNewNote);
                        break;
                    case TrayMouseAction.NewNoteInGroup:
                        execMenuCommand(mnuNewNoteInGroup);
                        break;
                    case TrayMouseAction.NoteFromClipboard:
                        execMenuCommand(mnuNoteFromClipboard);
                        break;
                    case TrayMouseAction.Preferences:
                        execMenuCommand(mnuPrefs);
                        break;
                    case TrayMouseAction.SaveAll:
                        execMenuCommand(mnuSaveAll);
                        break;
                    case TrayMouseAction.SearchByDates:
                        execMenuCommand(mnuSearchByDates);
                        break;
                    case TrayMouseAction.SearchByTags:
                        execMenuCommand(mnuSearchByTags);
                        break;
                    case TrayMouseAction.SearchInNotes:
                        execMenuCommand(mnuSearchInNotes);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createSyncMenu()
        {
            try
            {
                _SyncPluginsMenus.Clear();
                var plugins = PNPlugins.Instance.SyncPlugins.Where(p => PNCollections.Instance.ActiveSyncPlugins.Contains(p.Name)).ToArray();
                for (var i = mnuBackup.Items.Count - 1; i >= 0; i--)
                {
                    if (mnuBackup.Items[i] is MenuItem item)
                    {
                        if (string.IsNullOrEmpty(item.Name))
                        {
                            item.Click -= syncMenu_Click;
                            mnuBackup.Items.RemoveAt(i);
                        }
                    }

                    if (mnuBackup.Items[i] is Separator sep && string.IsNullOrEmpty(sep.Name))
                        mnuBackup.Items.RemoveAt(i);
                }
                var index = mnuBackup.Items.IndexOf(sepExport);
                foreach (var p in plugins)
                {
                    if (_SyncPluginsMenus.Keys.All(k => k != p.MenuSync.Name + p.Name))
                        _SyncPluginsMenus.Add(p.MenuSync.Name + p.Name, p.MenuSync);
                    if (mnuBackup.Items.OfType<MenuItem>().Any(mi => (string)mi.Header == p.MenuSync.Text)) continue;
                    var syncMenu = new MenuItem
                    {
                        Header = p.MenuSync.Text,
                        Icon =
                            new Image
                            {
                                Source = PNStatic.ImageFromDrawingImage(p.MenuSync.Image)
                            },
                        Tag = p.MenuSync.Name + p.Name
                    };
                    syncMenu.Click += syncMenu_Click;
                    mnuBackup.Items.Insert(++index, syncMenu);
                }
                if (plugins.Any() && index != mnuBackup.Items.IndexOf(sepExport))
                    mnuBackup.Items.Insert(++index, new Separator());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void syncMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem menu)) return;
                var tag = Convert.ToString(menu.Tag);
                if (_SyncPluginsMenus.Keys.All(k => k != tag)) return;
                var plugin = PNPlugins.Instance.SyncPlugins.FirstOrDefault(p => p.Name == tag);
                if (plugin == null) return;
                var version = new Version(plugin.Version);
                if (version.Major >= 2 && version.Build >= 3)
                {
                    var ds = new WndSync(plugin) { Owner = this };
                    ds.ShowDialog();
                }
                else
                {
                    var syncPluginsMenu = _SyncPluginsMenus[tag];
                    if (syncPluginsMenu == null) return;
                    syncPluginsMenu.PerformClick();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createTagsMenus()
        {
            try
            {
                //show tags
                foreach (
                    var ti in
                    mnuShowByTag.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuShowByTag.Items.Clear();
                foreach (var ti in PNCollections.Instance.Tags.Select(t => new MenuItem { Header = t }))
                {
                    ti.Click += menuClick;
                    mnuShowByTag.Items.Add(ti);
                }
                //hide tags
                foreach (
                    var ti in
                    mnuHideByTag.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuHideByTag.Items.Clear();
                foreach (var ti in PNCollections.Instance.Tags.Select(t => new MenuItem { Header = t }))
                {
                    ti.Click += menuClick;
                    mnuHideByTag.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createFavoritesMenu()
        {
            try
            {
                var count = mnuFavorites.Items.Count;
                if (count > 1)
                {
                    for (var i = count - 1; i > 0; i--)
                    {
                        if (mnuFavorites.Items[i] is MenuItem mi)
                            mi.Click -= menuClick;
                        mnuFavorites.Items.RemoveAt(i);
                    }
                }
                var notes =
                    PNCollections.Instance.Notes.Where(n => n.Favorite && n.GroupId != (int)SpecialGroups.RecycleBin).ToList();
                if (notes.Any())
                {
                    mnuFavorites.Items.Add(new Separator());
                    foreach (var ti in notes.Select(note => new MenuItem { Header = note.Name, Tag = note.Id }))
                    {
                        ti.Click += menuClick;
                        mnuFavorites.Items.Add(ti);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addNoteToShowHideMenu(MenuItem mi, PNote note)
        {
            try
            {
                var ti = new MenuItem { Header = note.Name, Tag = note.Id };
                ti.Click += menuClick;
                mi.Items.Add(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addGroupToShowHideMenu(MenuItem mi, PNGroup group, bool show)
        {
            try
            {
                var ti = new MenuItem { Header = group.Name, Tag = group.Id };
                if (PNRuntimes.Instance.Settings.Behavior.ShowSeparateNotes)
                {
                    var notes = PNCollections.Instance.Notes.Where(n => n.GroupId == group.Id);
                    var pNotes = notes as PNote[] ?? notes.ToArray();
                    if (!pNotes.Any())
                    {
                        ti.IsEnabled = false;
                    }
                    else
                    {
                        var tt = new MenuItem();
                        if (show)
                        {
                            tt.Header = PNLang.Instance.GetMenuText("main_menu", "mnuShowAll", "Show All");
                            tt.Tag = group.Id + "_show";
                        }
                        else
                        {
                            tt.Header = PNLang.Instance.GetMenuText("main_menu", "mnuHideAll", "Hide All");
                            tt.Tag = group.Id + "_hide";
                        }
                        tt.Click += menuClick;
                        ti.Items.Add(tt);
                        ti.Items.Add(new Separator());

                        foreach (PNote n in pNotes)
                        {
                            addNoteToShowHideMenu(ti, n);
                        }
                    }
                }
                else
                {
                    ti.Click += menuClick;
                }
                mi.Items.Insert(2, ti);

                foreach (PNGroup g in group.Subgroups)
                {
                    addGroupToShowHideMenu(mi, g, show);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createShowHideMenus()
        {
            try
            {
                var count = mnuShowGroups.Items.Count;
                if (count > 3)
                {
                    for (var i = count - 2; i > 1; i--)
                    {
                        if (mnuShowGroups.Items[i] is MenuItem ti)
                        {
                            if (ti.Items.Count > 0)
                            {
                                for (int j = ti.Items.Count - 1; j >= 0; j--)
                                {
                                    if (ti.Items[j].GetType() == typeof(MenuItem))
                                    {
                                        ((MenuItem)ti.Items[j]).Click -= menuClick;
                                    }
                                    ti.Items.RemoveAt(j);
                                }
                            }
                            else
                            {
                                ti.Click -= menuClick;
                            }
                        }
                        mnuShowGroups.Items.RemoveAt(i);
                    }
                }
                mnuShowGroups.Items.Insert(2, new Separator());
                foreach (var g in PNCollections.Instance.Groups[0].Subgroups)
                {
                    addGroupToShowHideMenu(mnuShowGroups, g, true);
                }

                count = mnuHideGroups.Items.Count;
                if (count > 3)
                {
                    for (var i = count - 2; i > 1; i--)
                    {
                        if (mnuHideGroups.Items[i] is MenuItem ti)
                        {
                            if (ti.Items.Count > 0)
                            {
                                for (var j = ti.Items.Count - 1; j >= 0; j--)
                                {
                                    if (ti.Items[j].GetType() == typeof(MenuItem))
                                    {
                                        ((MenuItem)ti.Items[j]).Click -= menuClick;
                                    }
                                    ti.Items.RemoveAt(j);
                                }
                            }
                            else
                            {
                                ti.Click -= menuClick;
                            }
                        }
                        mnuHideGroups.Items.RemoveAt(i);
                    }
                }
                mnuHideGroups.Items.Insert(2, new Separator());
                foreach (var g in PNCollections.Instance.Groups[0].Subgroups)
                {
                    addGroupToShowHideMenu(mnuHideGroups, g, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createRunMenu()
        {
            try
            {
                foreach (var ti in mnuRun.Items.OfType<MenuItem>())
                {
                    ti.Click -= menuClick;
                }
                mnuRun.Items.Clear();
                foreach (
                    var ti in
                    PNCollections.Instance.Externals.Select(
                        ext => new MenuItem { Header = ext.Name, Tag = ext.Program + "|" + ext.CommandLine }))
                {
                    ti.Click += menuClick;
                    mnuRun.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDiaryMenu()
        {
            try
            {
                for (var i = mnuDiary.Items.Count - 1; i > 0; i--)
                {
                    ((MenuItem)mnuDiary.Items[i]).Click -= menuClick;
                    mnuDiary.Items.RemoveAt(i);
                }
                var diaries =
                    PNCollections.Instance.Notes.Where(n => n.GroupId == (int)SpecialGroups.Diary && n.DateCreated < DateTime.Today);
                var pNotes = diaries.ToList().OrderByDescending(n => n.DateCreated);
                foreach (var ti in pNotes.Select(n => new MenuItem { Header = n.Name, Tag = n.Id }))
                {
                    ti.Click += menuClick;
                    mnuDiary.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void menuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                if (!(ti.Parent is MenuItem parent)) return;
                switch (parent.Name)
                {
                    case "mnuRun":
                        var args = ((string)ti.Tag).Split('|');
                        PNStatic.RunExternalProgram(args[0], args[1]);
                        break;
                    case "mnuShowGroups":
                        {
                            var id = (int)ti.Tag;
                            PNNotesOperations.ShowHideSpecificGroup(id, true);
                            break;
                        }
                    case "mnuHideGroups":
                        {
                            var id = (int)ti.Tag;
                            PNNotesOperations.ShowHideSpecificGroup(id, false);
                            break;
                        }
                    case "mnuFavorites":
                        {
                            var id = (string)ti.Tag;
                            var note = PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == id);
                            if (note != null)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    case "mnuShowByTag":
                        {
                            var notes =
                                PNCollections.Instance.Notes.Where(
                                    n =>
                                        n.Tags.Contains(ti.Header.ToString()) &&
                                        n.GroupId != (int)SpecialGroups.RecycleBin);
                            foreach (var note in notes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    case "mnuHideByTag":
                        {
                            var notes =
                                PNCollections.Instance.Notes.Where(
                                    n =>
                                        n.Tags.Contains(ti.Header.ToString()) &&
                                        n.GroupId != (int)SpecialGroups.RecycleBin);
                            foreach (var note in notes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, false);
                            }
                            break;
                        }
                    case "mnuDiary":
                        {
                            var id = (string)ti.Tag;
                            var note = PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == id);
                            if (note != null)
                            {
                                PNNotesOperations.ShowHideSpecificNote(note, true);
                            }
                            break;
                        }
                    default:
                        if (parent.Parent is MenuItem totalParent)
                        {
                            var tag = (string)ti.Tag;

                            switch (totalParent.Name)
                            {
                                case "mnuShowGroups":
                                    {
                                        var pos = tag.IndexOf("_show", StringComparison.Ordinal);
                                        if (pos >= 0)
                                        {
                                            var id = Convert.ToInt32(tag.Substring(0, pos));
                                            PNNotesOperations.ShowHideSpecificGroup(id, true);
                                        }
                                        else
                                        {
                                            var note = PNCollections.Instance.Notes.Note(tag);
                                            if (note != null)
                                            {
                                                PNNotesOperations.ShowHideSpecificNote(note, true);
                                            }
                                        }
                                    }
                                    break;
                                case "mnuHideGroups":
                                    {
                                        var pos = tag.IndexOf("_hide", StringComparison.Ordinal);
                                        if (pos >= 0)
                                        {
                                            var id = Convert.ToInt32(tag.Substring(0, pos));
                                            PNNotesOperations.ShowHideSpecificGroup(id, false);
                                        }
                                        else
                                        {
                                            var note = PNCollections.Instance.Notes.Note(tag);
                                            if (note != null)
                                            {
                                                PNNotesOperations.ShowHideSpecificNote(note, false);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void baloon_BaloonLinkClicked(object sender, BaloonClickedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case BaloonMode.NewVersion:
                        if (PNStatic.PrepareNewVersionCommandLine())
                        {
                            Close();
                        }
                        break;
                    case BaloonMode.NoteReceived:
                        if (PNRuntimes.Instance.Settings.Network.ShowReceivedOnClick)
                        {
                            foreach (var id in _ReceivedNotes)
                            {
                                PNNotesOperations.ShowHideSpecificNote(PNCollections.Instance.Notes.Note(id), true);
                            }
                        }
                        if (PNRuntimes.Instance.Settings.Network.ShowIncomingOnClick)
                        {
                            execMenuCommand(mnuCP);
                            PNWindows.Instance.FormCP.SelectSpecificGroup((int)SpecialGroups.Incoming);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNote newNoteClick()
        {
            try
            {
                var note = new PNote();
                note.Dialog = new WndNote(note, NewNoteMode.None);
                note.Dialog.Show();
                note.Visible = true;

                applyDefaultFont(note);

                //note size and location for setting other than NoteStartPosition.Center are set at loading
                if (PNRuntimes.Instance.Settings.Behavior.StartPosition == NoteStartPosition.Center)
                {
                    note.NoteSize = note.Dialog.GetSize();
                    note.NoteLocation = note.Dialog.GetLocation();
                }
                note.EditSize = note.Dialog.Edit.Size;

                PNCollections.Instance.Notes.Add(note);
                NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                subscribeToNoteEvents(note);
                return note;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void checkForNewVersion()
        {
            try
            {
                var updater = new PNUpdateChecker();
                updater.NewVersionFound += updater_PNNewVersionFound;
                updater.CheckNewVersion(System.Windows.Forms.Application.ProductVersion);
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
                var text = PNLang.Instance.GetMessageText("new_version_1",
                        "New version of PNotes.NET is available - %PLACEHOLDER1%.")
                    .Replace(PNStrings.PLACEHOLDER1, e.Version);
                var link = PNLang.Instance.GetMessageText("new_version_3",
                    "Click here in order to instal new version (restart of program is required).");
                var baloon = new Baloon(BaloonMode.NewVersion) { BaloonText = text, BaloonLink = link };
                baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            try
            {
                var sbClass = new StringBuilder(1024);
                PNInterop.GetClassName(hwnd, sbClass, sbClass.Capacity);
                if (sbClass.ToString() != _PinClass.Class) return true;
                if (!PNInterop.IsWindowVisible(hwnd)) return true;
                var count = PNInterop.GetWindowTextLength(hwnd);
                if (count <= 0) return true;
                var sb = new StringBuilder(count + 1);
                PNInterop.GetWindowText(hwnd, sb, count + 1);
                if (!Regex.IsMatch(sb.ToString(), _PinClass.Pattern)) return true;
                _PinClass.Hwnd = hwnd;
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void createFullBackup(bool silent)
        {
            try
            {
                var sfd = new SaveFileDialog();
                string fileBackup;
                if (silent)
                {
                    fileBackup = Path.Combine(PNPaths.Instance.BackupDir,
                        DateTime.Now.ToString("yyyyMMddHHmmss") + PNStrings.FULL_BACK_EXTENSION);
                }
                else
                {
                    sfd.Title = PNLang.Instance.GetCaptionText("full_backup", "Create full backup copy");
                    var sb = new StringBuilder();
                    sb.Append(PNLang.Instance.GetCaptionText("full_backup_filter", "PNotes full backup files"));
                    sb.Append(" (");
                    sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                    sb.Append(")|");
                    sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                    sfd.Filter = sb.ToString();
                    if (!sfd.ShowDialog(this).Value)
                    {
                        return;
                    }
                    fileBackup = sfd.FileName;
                }
                if (!PNStatic.CreateFullBackup(fileBackup)) return;
                if (silent) return;
                var message = PNLang.Instance.GetMessageText("full_backup_complete",
                    "Full backup operation completed successfully");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreFromFullBackup()
        {
            var hash = "";

            try
            {
                var ofd = new OpenFileDialog
                {
                    Title = PNLang.Instance.GetCaptionText("resture_full_backup", "Restore from full backup")
                };
                var sb = new StringBuilder();
                sb.Append(PNLang.Instance.GetCaptionText("full_backup_filter", "PNotes full backup files"));
                sb.Append(" (");
                sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                sb.Append(")|");
                sb.Append("*" + PNStrings.FULL_BACK_EXTENSION);
                ofd.Filter = sb.ToString();
                ofd.Multiselect = false;
                if (!ofd.ShowDialog(this).Value)
                {
                    return;
                }
                var packageFile = ofd.FileName;
                var message = PNLang.Instance.GetMessageText("full_restore_warning",
                    "ATTENTION! All existing notes will ber removed and replaced by notes from backup copy. Continue?");
                if (
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) ==
                    MessageBoxResult.No)
                {
                    return;
                }
                // store existing files for possible crash
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
                Directory.CreateDirectory(PNPaths.Instance.TempDir);
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                string path;
                foreach (var f in files)
                {
                    path = Path.Combine(PNPaths.Instance.TempDir, f.Name);
                    File.Move(f.FullName, path);
                }
                var fi = new FileInfo(PNPaths.Instance.DBPath);
                path = Path.Combine(PNPaths.Instance.TempDir, fi.Name);
                File.Move(fi.FullName, path);

                // start reading package
                using (var package = Package.Open(packageFile, FileMode.Open, FileAccess.Read))
                {
                    // build parameters file URI
                    var uriData = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative),
                        new Uri("data.xml", UriKind.Relative));
                    // get parameters file
                    var documentPart = package.GetPart(uriData);
                    // load XDocument
                    var xdoc = XDocument.Load(documentPart.GetStream(), LoadOptions.None);
                    var xhash = xdoc.Root?.Element("hash");
                    if (xhash != null)
                    {
                        hash = xhash.Value;
                    }
                    foreach (var prs in package.GetRelationships())
                    {
                        var noteExtracted = false;
                        var uriDocumentTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative),
                            prs.TargetUri);
                        documentPart = package.GetPart(uriDocumentTarget);
                        if (prs.RelationshipType.EndsWith(PNStrings.NOTE_EXTENSION))
                        {
                            path = Path.Combine(PNPaths.Instance.DataDir, prs.RelationshipType);
                            noteExtracted = true;
                        }
                        else
                        {
                            path = Path.Combine(PNPaths.Instance.DataDir, prs.RelationshipType);
                        }
                        extractPackagePart(documentPart.GetStream(), path);
                        if (!noteExtracted) continue;
                        // check whether note has been added to backup as encrypted
                        if (hash != "")
                        {
                            // decrypt note
                            using (var pne = new PNEncryptor(hash))
                            {
                                pne.DecryptTextFile(path);
                            }
                        }
                        // check whether note should be encrypted
                        if (!PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted) continue;
                        // encrypt note
                        using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                        {
                            pne.EncryptTextFile(path);
                        }
                    }
                }
                //reloadNotes();
                message = PNLang.Instance.GetMessageText("full_restore_complete",
                    "Restoration from full backup copy completed successfully.");
                message += "\n";
                message += PNLang.Instance.GetMessageText("restart_required",
                    "Program restart is required.");
                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                ApplyAction(MainDialogAction.Restart, null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    getFilesBack(PNPaths.Instance.TempDir);
                }
            }
            finally
            {
                if (Directory.Exists(PNPaths.Instance.TempDir))
                {
                    Directory.Delete(PNPaths.Instance.TempDir, true);
                }
            }
        }

        private void extractPackagePart(Stream source, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                PNStatic.CopyStream(source, fileStream);
            }
        }

        private void getFilesBack(string tempFolder)
        {
            try
            {
                var di = new DirectoryInfo(tempFolder);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                foreach (var f in files)
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, f.Name);
                    File.Copy(f.FullName, path, true);
                }
                var dbTemp = Path.Combine(tempFolder, PNStrings.DB_FILE);
                File.Copy(dbTemp, PNPaths.Instance.DBPath, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }

        private void showHelp()
        {
            try
            {
                var fileChm = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.CHM_FILE);
                var filePdf = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.PDF_FILE);
                if (File.Exists(fileChm))
                {
                    Process.Start(fileChm);
                }
                else if (File.Exists(filePdf))
                {
                    Process.Start(filePdf);
                }
                else
                {
                    var d = new WndHelpChooser { Owner = this };
                    d.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void reloadNotes()
        {
            try
            {
                foreach (var note in PNCollections.Instance.Notes.Where(note => note.Visible))
                {
                    note.Dialog.Close();
                }
                PNCollections.Instance.Notes.Clear();
                var controlPanel = false;
                if (PNWindows.Instance.FormCP != null)
                {
                    controlPanel = true;
                    PNWindows.Instance.FormCP.Close();
                }
                loadNotes();
                if (controlPanel)
                {
                    execMenuCommand(mnuCP);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool isMonitorPluggedUnplugged()
        {
            try
            {
                if (PNSingleton.Instance.MonitorsCount == Screen.AllScreens.Length) return false;
                var currentMonitorsCount = PNSingleton.Instance.MonitorsCount;
                PNSingleton.Instance.MonitorsCount = Screen.AllScreens.Length;
                if (Screen.AllScreens.Length >= currentMonitorsCount) return false;
                PNNotesOperations.RelocateAllNotesOnScreenPlug();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void isScreenRectangleChanged()
        {
            try
            {
                var currentRect = PNStatic.AllScreensBounds();
                if (PNSingleton.Instance.ScreenRect == currentRect) return;
                PNSingleton.Instance.ScreenRect = currentRect;
                PNNotesOperations.RedockNotes();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void adjustStartTimes()
        {
            try
            {
                var notes =
                    PNCollections.Instance.Notes.Where(
                        n => n.Schedule.Type == ScheduleType.After || n.Schedule.Type == ScheduleType.RepeatEvery);
                foreach (
                    var n in
                    notes.Where(
                        n =>
                            n.Schedule.StartFrom == ScheduleStart.ProgramStart))
                {
                    n.Schedule.LastRun = PNRuntimes.Instance.StartTime;
                    PNNotesOperations.SaveNoteSchedule(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showPriorityOnStart()
        {
            try
            {
                if (!PNRuntimes.Instance.Settings.GeneralSettings.ShowPriorityOnStart) return;
                var notes = PNCollections.Instance.Notes.Where(n => n.Priority && !n.Visible);
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

        private void loadSingleNote(string id, bool show)
        {
            try
            {
                var groups = new List<int>();
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    var sqlQuery = "SELECT * FROM NOTES WHERE ID = '" + id + "'";
                    using (var t = oData.FillDataTable(sqlQuery))
                    {
                        if (t.Rows.Count <= 0) return;
                        var note = new PNote();
                        if (!PNNotesOperations.LoadNoteProperties(note, t.Rows[0]))
                        {
                            return;
                        }
                        //load custom settings
                        sqlQuery = "SELECT * FROM CUSTOM_NOTES_SETTINGS WHERE NOTE_ID = '" + note.Id + "'";
                        using (var tc = oData.FillDataTable(sqlQuery))
                        {
                            if (tc.Rows.Count > 0)
                            {
                                PNNotesOperations.LoadNoteCustomProperties(note, tc.Rows[0]);
                            }
                        }
                        //load tags
                        PNNotesOperations.LoadNoteTags(note);
                        //load linked notes
                        PNNotesOperations.LoadLinkedNotes(note);
                        //create new note window
                        if (note.Visible && show)
                        {
                            var showNote = true;
                            var pnGroup = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                            if (pnGroup != null && !groups.Contains(pnGroup.Id))
                            {
                                showNote = PNNotesOperations.LoginToGroup(pnGroup, ref groups);
                            }
                            if (showNote)
                            {
                                if (note.PasswordString.Trim().Length > 0)
                                {
                                    showNote &= PNNotesOperations.LoginToNote(note);
                                }
                                if (showNote)
                                {
                                    note.Dialog = new WndNote(note, note.Id, NewNoteMode.Identificator);
                                    note.Dialog.Show();
                                }
                                else
                                {
                                    note.Visible = false;
                                }
                            }
                        }
                        PNCollections.Instance.Notes.Add(note);
                        NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                        subscribeToNoteEvents(note);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                foreach (var fi in files)
                {
                    loadSingleNote(Path.GetFileNameWithoutExtension(fi.Name), true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void restoreAutosavedNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                if (files.Length <= 0) return;
                var result = MessageBoxResult.Yes;
                if (!PNRuntimes.Instance.Settings.GeneralSettings.RestoreAuto)
                    result =
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("load_autosaved",
                                "Program did not finish correctly last time. Would you like to load autosaved notes?"),
                            PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
                foreach (var f in files)
                {
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                    var id = Path.GetFileNameWithoutExtension(f.Name);
                    var path = Path.Combine(PNPaths.Instance.DataDir, id + PNStrings.NOTE_EXTENSION);
                    File.Copy(f.FullName, path, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private static void clearAutosavedNotes()
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                var files = di.GetFiles("*" + PNStrings.NOTE_AUTO_BACK_EXTENSION);
                foreach (var f in files)
                {
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                    File.Delete(f.FullName);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initDockArrows()
        {
            try
            {
                PNWindows.Instance.DockArrows.Add(DockArrow.LeftUp, new WndArrow(DockArrow.LeftUp));
                PNWindows.Instance.DockArrows.Add(DockArrow.LeftDown, new WndArrow(DockArrow.LeftDown));
                PNWindows.Instance.DockArrows.Add(DockArrow.TopLeft, new WndArrow(DockArrow.TopLeft));
                PNWindows.Instance.DockArrows.Add(DockArrow.TopRight, new WndArrow(DockArrow.TopRight));
                PNWindows.Instance.DockArrows.Add(DockArrow.RightUp, new WndArrow(DockArrow.RightUp));
                PNWindows.Instance.DockArrows.Add(DockArrow.RightDown, new WndArrow(DockArrow.RightDown));
                PNWindows.Instance.DockArrows.Add(DockArrow.BottomLeft, new WndArrow(DockArrow.BottomLeft));
                PNWindows.Instance.DockArrows.Add(DockArrow.BottomRight, new WndArrow(DockArrow.BottomRight));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initSpeller()
        {
            try
            {
                if (PNRuntimes.Instance.Settings.GeneralSettings.SpellDict == "") return;
                var fileDict = Path.Combine(PNPaths.Instance.DictDir, PNRuntimes.Instance.Settings.GeneralSettings.SpellDict);
                var newName = Path.ChangeExtension(PNRuntimes.Instance.Settings.GeneralSettings.SpellDict, ".aff");
                if (newName == null) return;
                var fileAff = Path.Combine(PNPaths.Instance.DictDir, newName);
                Spellchecking.HunspellInit(fileDict, fileAff);
                if (Spellchecking.Initialized)
                {
                    Spellchecking.ColorUnderlining = PNRuntimes.Instance.Settings.GeneralSettings.SpellColor;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void registerMainHotkeys()
        {
            try
            {
                var keys = PNCollections.Instance.HotKeysMain.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHk(_HwndSource.Handle, hk);
                }
                keys = PNCollections.Instance.HotKeysGroups.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHk(_HwndSource.Handle, hk);
                }
                keys = PNCollections.Instance.HotKeysNote.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHk(_HwndSource.Handle, hk);
                }
                keys = PNCollections.Instance.HotKeysEdit.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.RegisterHk(_HwndSource.Handle, hk);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void unregisterMainHotkeys()
        {
            try
            {
                var keys = PNCollections.Instance.HotKeysMain.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHk(_HwndSource.Handle, hk.Id);
                }
                keys = PNCollections.Instance.HotKeysGroups.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHk(_HwndSource.Handle, hk.Id);
                }
                keys = PNCollections.Instance.HotKeysNote.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHk(_HwndSource.Handle, hk.Id);
                }
                keys = PNCollections.Instance.HotKeysEdit.Where(h => h.Vk != 0);
                foreach (var hk in keys)
                {
                    HotkeysStatic.UnregisterHk(_HwndSource.Handle, hk.Id);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMenuHotkey(MenuItem mi)
        {
            try
            {
                var hk = PNCollections.Instance.HotKeysMain.FirstOrDefault(h => h.MenuName == mi.Name);
                if (hk != null)
                {
                    mi.InputGestureText = hk.Shortcut;
                }
                foreach (var ti in mi.Items.OfType<MenuItem>())
                {
                    applyMenuHotkey(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareHotkeysTable(SQLiteDataObject oData)
        {
            var d = new WndNote();
            try
            {
                var id = PNData.HK_START;
                var o = oData.GetScalar("SELECT MAX(ID) FROM HOT_KEYS");
                if (o != null && !DBNull.Value.Equals(o))
                {
                    id = (int)(long)o + 1;
                }
                startPreparingMenuHotkeys(HotkeyType.Main, oData, ctmPN, ref id);
                startPreparingMenuHotkeys(HotkeyType.Note, oData, d.NoteMenu, ref id);
                startPreparingMenuHotkeys(HotkeyType.Edit, oData, d.EditMenu, ref id);
                startPreparingGroupsHotkeys(HotkeyType.Group, oData, ref id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                d.Close();
            }
        }

        private void startPreparingGroupsHotkeys(HotkeyType type, SQLiteDataObject oData, ref int id)
        {
            try
            {
                //delete possible duplicates first
                var sqlQuery = "SELECT MAX(GROUP_ID) FROM GROUPS";
                var obj = oData.GetScalar(sqlQuery);
                if (obj == null || PNData.IsDBNull(obj))
                    return;
                var maxId = Convert.ToInt32(obj);
                var deleteList = new List<string>();
                sqlQuery = "SELECT MENU_NAME, ID FROM HOT_KEYS WHERE HK_TYPE = 3";
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var arr = Convert.ToString(r["MENU_NAME"]).Split('_');
                        if (Convert.ToInt32(arr[0]) <= maxId) continue;
                        var sb = new StringBuilder("DELETE FROM HOT_KEYS WHERE MENU_NAME = '");
                        sb.Append(r["MENU_NAME"]);
                        sb.Append("' AND ID = ");
                        sb.Append(r["ID"]);
                        deleteList.Add(sb.ToString());
                    }
                }
                if (deleteList.Count > 0)
                {
                    PNData.ExecuteTransactionForList(deleteList, oData.ConnectionString);
                }

                sqlQuery = "SELECT MENU_NAME FROM HOT_KEYS WHERE HK_TYPE = " +
                           ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    var names = (from DataRow r in t.Rows select (string)r[0]).ToList();
                    var group = PNCollections.Instance.Groups.GetGroupById((int)SpecialGroups.AllGroups);
                    if (group != null)
                    {
                        foreach (var g in group.Subgroups)
                        {
                            prepareSingleGroupHotKey(g, oData, names, HotkeyType.Group, ref id);
                        }
                    }
                }
                loadSpecificHotKeys(type, oData);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void startPreparingMenuHotkeys(HotkeyType type, SQLiteDataObject oData, ContextMenu ctm, ref int id)
        {
            try
            {
                var sqlQuery = "SELECT MENU_NAME FROM HOT_KEYS WHERE HK_TYPE = " +
                               ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    var names = (from DataRow r in t.Rows select (string)r[0]).ToList();
                    foreach (var ti in ctm.Items.OfType<MenuItem>())
                    {
                        prepareSingleMenuHotKey(ti, oData, names, type, ref id);
                    }
                }
                loadSpecificHotKeys(type, oData);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSpecificHotKeys(HotkeyType type, SQLiteDataObject oData)
        {
            try
            {
                var sqlQuery = "SELECT MENU_NAME, ID, MODIFIERS, VK, SHORTCUT FROM HOT_KEYS WHERE HK_TYPE = " +
                               ((int)type).ToString(CultureInfo.InvariantCulture);
                using (var t = oData.FillDataTable(sqlQuery))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        switch (type)
                        {
                            case HotkeyType.Main:
                                PNCollections.Instance.HotKeysMain.Add(new PNHotKey
                                {
                                    MenuName = (string)r["MENU_NAME"],
                                    Id = (int)r["ID"],
                                    Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"],
                                    Vk = (uint)(int)r["VK"],
                                    Shortcut = (string)r["SHORTCUT"],
                                    Type = type
                                });
                                break;
                            case HotkeyType.Note:
                                PNCollections.Instance.HotKeysNote.Add(new PNHotKey
                                {
                                    MenuName = (string)r["MENU_NAME"],
                                    Id = (int)r["ID"],
                                    Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"],
                                    Vk = (uint)(int)r["VK"],
                                    Shortcut = (string)r["SHORTCUT"],
                                    Type = type
                                });
                                break;
                            case HotkeyType.Edit:
                                PNCollections.Instance.HotKeysEdit.Add(new PNHotKey
                                {
                                    MenuName = (string)r["MENU_NAME"],
                                    Id = (int)r["ID"],
                                    Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"],
                                    Vk = (uint)(int)r["VK"],
                                    Shortcut = (string)r["SHORTCUT"],
                                    Type = type
                                });
                                break;
                            case HotkeyType.Group:
                                PNCollections.Instance.HotKeysGroups.Add(new PNHotKey
                                {
                                    MenuName = (string)r["MENU_NAME"],
                                    Id = (int)r["ID"],
                                    Modifiers = (HotkeyModifiers)(int)r["MODIFIERS"],
                                    Vk = (uint)(int)r["VK"],
                                    Shortcut = (string)r["SHORTCUT"],
                                    Type = type
                                });
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

        private void prepareSingleGroupHotKey(PNGroup group, SQLiteDataObject oData, List<string> names, HotkeyType type,
            ref int id)
        {
            try
            {
                foreach (var g in group.Subgroups)
                {
                    prepareSingleGroupHotKey(g, oData, names, HotkeyType.Group, ref id);
                }
                var prefix = group.Id + "_show";
                if (names.All(n => n != prefix))
                {
                    var sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" +
                                   ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + prefix + "'," +
                                   id.ToString(CultureInfo.InvariantCulture) + ",'')";
                    oData.Execute(sqlQuery);
                    id++;
                }
                prefix = group.Id + "_hide";
                if (names.All(n => n != prefix))
                {
                    string sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" +
                                      ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + prefix + "'," +
                                      id.ToString(CultureInfo.InvariantCulture) + ",'')";
                    oData.Execute(sqlQuery);
                    id++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareSingleMenuHotKey(MenuItem mi, SQLiteDataObject oData, List<string> names, HotkeyType type,
            ref int id)
        {
            try
            {
                foreach (var ti in mi.Items.OfType<MenuItem>())
                {
                    prepareSingleMenuHotKey(ti, oData, names, type, ref id);
                }
                if (names.All(s => s != mi.Name))
                {
                    string sqlQuery = "INSERT INTO HOT_KEYS (HK_TYPE, MENU_NAME, ID, SHORTCUT) VALUES(" +
                                      ((int)type).ToString(CultureInfo.InvariantCulture) + ",'" + mi.Name + "'," +
                                      id.ToString(CultureInfo.InvariantCulture) + ",'" + mi.InputGestureText + "')";
                    oData.Execute(sqlQuery);
                    id++;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private CriticalUpdateAction checkCriticalUpdates()
        {
            try
            {
                _UnsubscribedCritical = false;
                var critical = new PNUpdateChecker();
                critical.CriticalUpdatesFound += critical_CriticalUpdatesFound;
                var result = critical.CheckCriticalUpdates(System.Windows.Forms.Application.ProductVersion);
                if (!_UnsubscribedCritical)
                {
                    critical.CriticalUpdatesFound -= critical_CriticalUpdatesFound;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return CriticalUpdateAction.None;
            }
        }

        void critical_CriticalUpdatesFound(object sender, CriticalUpdatesFoundEventArgs e)
        {
            try
            {
                if (sender is PNUpdateChecker pnu) pnu.CriticalUpdatesFound -= critical_CriticalUpdatesFound;
                _UnsubscribedCritical = true;
                var commandLinePrepared = !string.IsNullOrWhiteSpace(e.ProgramFileName) &&
                                          prepareCriticalVersionUpdateCommandLine(e.ProgramFileName);
                var pluginsXmlPrepared = e.Plugins != null && e.Plugins.Any() &&
                                         preparePreRunCriticalPluginsXml(e.Plugins);
                e.Accepted = commandLinePrepared | pluginsXmlPrepared;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool preparePreRunCriticalPluginsXml(IEnumerable<CriticalPluginUpdate> plugins)
        {
            try
            {
                var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                var addCopy = false;
                var xcopies = xroot.Element(PNStrings.ELM_COPY_PLUGS);
                if (xcopies == null)
                {
                    addCopy = true;
                    xcopies = new XElement(PNStrings.ELM_COPY_PLUGS);
                }
                else
                {
                    xcopies.RemoveAll();
                }
                var tempDir = Path.GetTempPath();
                using (var wc = new WebClient())
                {
                    foreach (var pl in plugins)
                    {
                        var caption = PNLang.Instance.GetCaptionText("downloading", "Downloading:") + @" " +
                                      pl.ProductName;
                        PNRuntimes.Instance.SpTextProvider.SplashText = caption;
                        var tempFile = Path.Combine(tempDir, pl.FileName);
                        if (File.Exists(tempFile)) File.Delete(tempFile);
                        wc.DownloadFile(new Uri(PNStrings.URL_DOWNLOAD_DIR + pl.FileName), tempFile);
                        caption = PNLang.Instance.GetCaptionText("extracting", "Extracting:") + @" " + pl.ProductName;
                        PNRuntimes.Instance.SpTextProvider.SplashText = caption;
                        using (var zip = new ZipFile(tempFile))
                        {
                            zip.ExtractAll(tempDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                        File.Delete(tempFile);

                        var fromPath = Path.Combine(Path.GetTempPath(), pl.ProductName);
                        var name = pl.ProductName;
                        var xc = new XElement(PNStrings.ELM_COPY);
                        xc.Add(new XAttribute(PNStrings.ATT_NAME, name));
                        xc.Add(new XAttribute(PNStrings.ATT_FROM, fromPath));
                        xc.Add(new XAttribute(PNStrings.ATT_TO,
                            Path.Combine(PNPaths.Instance.PluginsDir, pl.ProductName)));
                        xc.Add(new XAttribute(PNStrings.ATT_DEL_DIR, "false"));
                        xc.Add(new XAttribute(PNStrings.ATT_IS_CRITICAL, "true"));
                        xcopies.Add(xc);
                    }
                }
                if (addCopy)
                {
                    xroot.Add(xcopies);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool prepareCriticalVersionUpdateCommandLine(string updateZip)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("0 ");
                sb.Append("\"");
                sb.Append(PNLang.Instance.GetCaptionText("update_progress", "Update in progress..."));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("downloading", "Downloading:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("extracting", "Extracting:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("copying", "Coping:"));
                sb.Append("\" \"");
                sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.ExecutablePath);
                sb.Append("\" \"");
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.StartupPath);
                sb.Append("\" \"");
                sb.Append(updateZip);
                sb.Append("\"");

                PNSingleton.Instance.UpdaterCommandLine = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void loadDatabase()
        {
            try
            {
                if(File.Exists(PNPaths.Instance.DBPath))
                {
                    PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_database",
                        "Loading database");
                }

                if (PNData.ConnectionString != "")
                {
                    using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                    {
                        //notes
                        var sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [NOTES] ([ID] TEXT PRIMARY KEY NOT NULL, [NAME] TEXT NOT NULL, [GROUP_ID] INT NOT NULL, [PREV_GROUP_ID] INT, [OPACITY] REAL, [VISIBLE] BOOLEAN, [FAVORITE] BOOLEAN, [PROTECTED] BOOLEAN, [COMPLETED] BOOLEAN, [PRIORITY] BOOLEAN, [PASSWORD_STRING] TEXT, [PINNED] BOOLEAN, [TOPMOST] BOOLEAN, [ROLLED] BOOLEAN, [DOCK_STATUS] INT, [DOCK_ORDER] INT, [SEND_RECEIVE_STATUS] INT, [DATE_CREATED] TEXT, [DATE_SAVED] TEXT, [DATE_SENT] TEXT, [DATE_RECEIVED] TEXT, [DATE_DELETED] TEXT, [SIZE] TEXT, [LOCATION] TEXT, [EDIT_SIZE] TEXT, [REL_X] REAL, [REL_Y] REAL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [SENT_TO] TEXT, [RECEIVED_FROM] TEXT, [PIN_CLASS] TEXT, [PIN_TEXT] TEXT, [SCRAMBLED] BOOLEAN DEFAULT (0), [THUMBNAIL] BOOLEAN DEFAULT (0), [RECEIVED_IP] TEXT)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'SCRAMBLED' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [SCRAMBLED] BOOLEAN DEFAULT (0)";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET SCRAMBLED = 0";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'THUMBNAIL' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [THUMBNAIL] BOOLEAN DEFAULT (0)";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET THUMBNAIL = 0";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'RECEIVED_IP' AND TABLE_NAME = 'NOTES'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES ADD COLUMN [RECEIVED_IP] TEXT";
                                oData.Execute(sqlQuery);
                                sqlQuery = "UPDATE NOTES SET THUMBNAIL = 0";
                                oData.Execute(sqlQuery);
                            }
                        }
                        if (!PNSingleton.Instance.PlatformChanged)
                        {
                            //custom notes settings
                            sqlQuery =
                                "CREATE TABLE IF NOT EXISTS [CUSTOM_NOTES_SETTINGS] ([NOTE_ID] TEXT NOT NULL, [BACK_COLOR] TEXT, [CAPTION_FONT_COLOR] TEXT, [CAPTION_FONT] TEXT, [SKIN_NAME] TEXT, [CUSTOM_OPACITY] BOOLEAN, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                            oData.Execute(sqlQuery);
                        }
                        else
                        {
                            if (oData.TableExists("CUSTOM_NOTES_SETTINGS"))
                            {
                                PNData.NormalizeCustomNotesTable(oData);
                            }
                            else
                            {
                                sqlQuery =
                                    "CREATE TABLE [CUSTOM_NOTES_SETTINGS] ([NOTE_ID] TEXT NOT NULL, [BACK_COLOR] TEXT, [CAPTION_FONT_COLOR] TEXT, [CAPTION_FONT] TEXT, [SKIN_NAME] TEXT, [CUSTOM_OPACITY] BOOLEAN, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                                oData.Execute(sqlQuery);
                            }
                        }
                        //notes tags
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [NOTES_TAGS] ([NOTE_ID] TEXT NOT NULL, [TAG] TEXT NOT NULL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                        oData.Execute(sqlQuery);
                        //notes schedule
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [NOTES_SCHEDULE] ([NOTE_ID] TEXT NOT NULL, [SCHEDULE_TYPE] INT, [ALARM_DATE] TEXT, [START_DATE] TEXT, [LAST_RUN] TEXT, [SOUND] TEXT, [STOP_AFTER] INT, [TRACK] BOOLEAN, [REPEAT_COUNT] INT, [SOUND_IN_LOOP] BOOLEAN, [USE_TTS] BOOLEAN, [START_FROM] INT, [MONTH_DAY] TEXT, [ALARM_AFTER] TEXT, [WEEKDAYS] TEXT, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [PROG_TO_RUN] TEXT, [CLOSE_ON_NOTIFICATION] BOOLEAN)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.GetSchema("Columns"))
                        {
                            var rows = t.Select("COLUMN_NAME = 'PROG_TO_RUN' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [PROG_TO_RUN] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'CLOSE_ON_NOTIFICATION' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [CLOSE_ON_NOTIFICATION] BOOLEAN";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'MULTI_ALERTS' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [MULTI_ALERTS] TEXT";
                                oData.Execute(sqlQuery);
                            }
                            rows = t.Select("COLUMN_NAME = 'TIME_ZONE' AND TABLE_NAME = 'NOTES_SCHEDULE'");
                            if (rows.Length == 0)
                            {
                                sqlQuery = "ALTER TABLE NOTES_SCHEDULE ADD COLUMN [TIME_ZONE] TEXT";
                                oData.Execute(sqlQuery);
                            }
                        }
                        //remove possible trash schedules (may remain after synchronization)
                        sqlQuery = "DELETE FROM NOTES_SCHEDULE WHERE SCHEDULE_TYPE = 0";
                        oData.Execute(sqlQuery);
                        //linked notes
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [LINKED_NOTES] ([NOTE_ID] TEXT NOT NULL, [LINK_ID] TEXT NOT NULL, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )))";
                        oData.Execute(sqlQuery);
                        //groups
                        if (!oData.TableExists("GROUPS"))
                        {
                            sqlQuery =
                                "CREATE TABLE [GROUPS] ([GROUP_ID] INT PRIMARY KEY NOT NULL UNIQUE, [PARENT_ID] INT, [GROUP_NAME] TEXT NOT NULL, [ICON] TEXT, [BACK_COLOR] TEXT NOT NULL, [CAPTION_FONT_COLOR] TEXT NOT NULL, [CAPTION_FONT] TEXT NOT NULL, [SKIN_NAME] TEXT NOT NULL, [PASSWORD_STRING] TEXT, [FONT] TEXT, [FONT_COLOR] TEXT, [UPD_DATE] TEXT DEFAULT(strftime ( '%Y%m%d%H%M%S' , 'now' , 'localtime' )), [IS_DEFAULT_IMAGE] BOOLEAN)";
                            oData.Execute(sqlQuery);
                            var sb = new StringBuilder();
                            sb.Append(prepareGroupInsert((int)SpecialGroups.AllGroups, (int)SpecialGroups.DummyGroup,
                                "All groups", "gr_all.png"));
                            sb.Append(prepareGroupInsert(0, (int)SpecialGroups.AllGroups, "General", "gr.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.RecycleBin, (int)SpecialGroups.DummyGroup,
                                "Recycle Bin", "gr_bin.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Diary, (int)SpecialGroups.DummyGroup,
                                "Diary", "gr_diary.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.SearchResults,
                                (int)SpecialGroups.DummyGroup, "Search Results", "gr_search.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Backup, (int)SpecialGroups.DummyGroup,
                                "Backup", "gr_back.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Favorites, (int)SpecialGroups.DummyGroup,
                                "Favorites", "gr_fav.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Incoming, (int)SpecialGroups.DummyGroup,
                                "Incoming", "gr_inc.png"));
                            sb.Append(prepareGroupInsert((int)SpecialGroups.Docking, (int)SpecialGroups.DummyGroup,
                                "Docking", "dockall_tree.png"));

                            sqlQuery = sb.ToString();
                            oData.Execute(sqlQuery);
                        }
                        else
                        {
                            using (var t = oData.GetSchema("Columns"))
                            {
                                var rows = t.Select("COLUMN_NAME = 'IS_DEFAULT_IMAGE' AND TABLE_NAME = 'GROUPS'");
                                if (rows.Length == 0)
                                {
                                    sqlQuery = "ALTER TABLE GROUPS ADD COLUMN [IS_DEFAULT_IMAGE] BOOLEAN";
                                    oData.Execute(sqlQuery);
                                    sqlQuery = "UPDATE GROUPS SET IS_DEFAULT_IMAGE = 1";
                                    oData.Execute(sqlQuery);
                                }
                            }
                            if (PNSingleton.Instance.PlatformChanged)
                            {
                                PNData.NormalizeGroupsTable(oData, false);
                                upgradeGroups(oData);
                            }
                        }
                        var groupsWithoutParent = new List<PNGroup>();

                        sqlQuery =
                            "SELECT GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, FONT, FONT_COLOR, IS_DEFAULT_IMAGE FROM GROUPS ORDER BY GROUP_ID ASC";
                        using (var t = oData.FillDataTable(sqlQuery))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                if ((int)r["GROUP_ID"] < 0)
                                {
                                    if ((int)r["GROUP_ID"] != (int)SpecialGroups.Docking)
                                    {
                                        var group = new PNGroup();
                                        fillGroup(group, r);
                                        PNCollections.Instance.Groups.Insert(0, group);
                                    }
                                    else
                                    {
                                        fillGroup(PNRuntimes.Instance.Docking, r);
                                    }
                                }
                                else
                                {
                                    var gr = new PNGroup();
                                    fillGroup(gr, r);

                                    var parentExists =
                                        PNCollections.Instance.Groups.Select(grp => grp.GetGroupById(gr.ParentId))
                                            .Any(pg => pg != null);
                                    if (!parentExists)
                                    {
                                        groupsWithoutParent.Add(gr);
                                        continue;
                                    }

                                    foreach (
                                        var parent in
                                        PNCollections.Instance.Groups.Select(g => g.GetGroupById(gr.ParentId))
                                            .Where(parent => parent != null))
                                    {
                                        parent.Subgroups.Add(gr);
                                        break;
                                    }
                                }
                            }
                        }
                        while (groupsWithoutParent.Count > 0)
                        {
                            for (var i = groupsWithoutParent.Count - 1; i >= 0; i--)
                            {
                                if (
                                    PNCollections.Instance.Groups.Select(grp => grp.GetGroupById(groupsWithoutParent[i].ParentId))
                                        .All(pg => pg == null))
                                    continue;
                                var i1 = i;
                                foreach (
                                    var parent in
                                    PNCollections.Instance.Groups.Select(g => g.GetGroupById(groupsWithoutParent[i1].ParentId))
                                        .Where(parent => parent != null))
                                {
                                    parent.Subgroups.Add(groupsWithoutParent[i]);
                                    groupsWithoutParent.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        applyStandardGroupsNames();

                        //sync comps
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [SYNC_COMPS] ([COMP_NAME] TEXT NOT NULL, [DATA_DIR] TEXT NOT NULL, [DB_DIR] TEXT NOT NULL, [USE_DATA_DIR] TEXT NOT NULL)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SYNC_COMPS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.SyncComps.Add(new PNSyncComp
                                {
                                    CompName = (string)r["COMP_NAME"],
                                    DataDir = (string)r["DATA_DIR"],
                                    DBDir = (string)r["DB_DIR"],
                                    UseDataDir = bool.Parse((string)r["USE_DATA_DIR"])
                                });
                            }
                        }
                        //moved to PNData
                        ////contact groups
                        //sqlQuery =
                        //    "CREATE TABLE IF NOT EXISTS [CONTACT_GROUPS] ([ID] INT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [GROUP_NAME] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE)";
                        //oData.Execute(sqlQuery);
                        //using (var t = oData.FillDataTable("SELECT * FROM CONTACT_GROUPS"))
                        //{
                        //    foreach (DataRow r in t.Rows)
                        //    {
                        //        PNCollections.Instance.ContactGroups.Add(new PNContactGroup
                        //        {
                        //            Id = (int)r["ID"],
                        //            Name = (string)r["GROUP_NAME"]
                        //        });
                        //    }
                        //}
                        //moved to PNData
                        ////contacts
                        //sqlQuery =
                        //    "CREATE TABLE IF NOT EXISTS [CONTACTS] ([ID] INT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [GROUP_ID] INT NOT NULL, [CONTACT_NAME] TEXT NOT NULL UNIQUE ON CONFLICT REPLACE, [COMP_NAME] TEXT NOT NULL, [IP_ADDRESS] TEXT NOT NULL, [USE_COMP_NAME] BOOLEAN NOT NULL)";
                        //oData.Execute(sqlQuery);
                        //using (
                        //    var t =
                        //        oData.FillDataTable(
                        //            "SELECT ID, GROUP_ID, CONTACT_NAME, COMP_NAME, IP_ADDRESS, USE_COMP_NAME FROM CONTACTS")
                        //)
                        //{
                        //    foreach (var cont in from DataRow r in t.Rows
                        //                         select new PNContact
                        //                         {
                        //                             Name = (string)r["CONTACT_NAME"],
                        //                             ComputerName = (string)r["COMP_NAME"],
                        //                             IpAddress = (string)r["IP_ADDRESS"],
                        //                             GroupId = (int)r["GROUP_ID"],
                        //                             UseComputerName = (bool)r["USE_COMP_NAME"],
                        //                             Id = (int)r["ID"]
                        //                         })
                        //    {
                        //        PNCollections.Instance.Contacts.Add(cont);
                        //    }
                        //    Task.Factory.StartNew(updateContactsWithoutIp);
                        //}
                        //externals
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [EXTERNALS] ([EXT_NAME] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [PROGRAM] TEXT NOT NULL, [COMMAND_LINE] TEXT NOT NULL)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT EXT_NAME, PROGRAM, COMMAND_LINE FROM EXTERNALS "))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.Externals.Add(new PNExternal
                                {
                                    Name = (string)r["EXT_NAME"],
                                    Program = (string)r["PROGRAM"],
                                    CommandLine = (string)r["COMMAND_LINE"]
                                });
                            }
                        }
                        //search providers
                        if (!oData.TableExists("SEARCH_PROVIDERS"))
                        {
                            sqlQuery =
                                "CREATE TABLE [SEARCH_PROVIDERS] ([SP_NAME] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE, [SP_QUERY] TEXT NOT NULL)";
                            oData.Execute(sqlQuery);
                            //insert two default provider for the first time
                            sqlQuery =
                                "INSERT INTO SEARCH_PROVIDERS VALUES('Google', 'http://www.google.com/search?q=')";
                            oData.Execute(sqlQuery);
                            sqlQuery =
                                "INSERT INTO SEARCH_PROVIDERS VALUES('Yahoo', 'http://search.yahoo.com/search?p=')";
                            oData.Execute(sqlQuery);
                        }
                        using (var t = oData.FillDataTable("SELECT * FROM SEARCH_PROVIDERS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.SearchProviders.Add(new PNSearchProvider
                                {
                                    Name = (string)r["SP_NAME"],
                                    QueryString = (string)r["SP_QUERY"]
                                });
                            }
                        }
                        //tags
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [TAGS] ([TAG] TEXT PRIMARY KEY ON CONFLICT REPLACE NOT NULL UNIQUE ON CONFLICT REPLACE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM TAGS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.Tags.Add((string)r["TAG"]);
                            }
                        }
                        //plugins
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [POST_PLUGINS] ([PLUGIN] TEXT PRIMARY KEY NOT NULL UNIQUE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM POST_PLUGINS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.ActivePostPlugins.Add((string)r["PLUGIN"]);
                            }
                        }
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [SYNC_PLUGINS] ([PLUGIN] TEXT PRIMARY KEY NOT NULL UNIQUE)";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SYNC_PLUGINS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.ActiveSyncPlugins.Add((string)r["PLUGIN"]);
                            }
                        }
                        //hotkeys
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [HOT_KEYS] ([HK_TYPE] INT NOT NULL, [MENU_NAME] TEXT NOT NULL, [ID] INT NOT NULL DEFAULT(0), [MODIFIERS] INT NOT NULL DEFAULT(0), [VK] INT NOT NULL DEFAULT(0), [SHORTCUT] TEXT NOT NULL DEFAULT(''), PRIMARY KEY ([HK_TYPE], [MENU_NAME]))";
                        oData.Execute(sqlQuery);
                        prepareHotkeysTable(oData);
                        //find/replace
                        if (!oData.TableExists("FIND_REPLACE"))
                        {
                            sqlQuery = "CREATE TABLE [FIND_REPLACE] ([FIND] TEXT, [REPLACE] TEXT); ";
                            sqlQuery += "INSERT INTO FIND_REPLACE VALUES(NULL, NULL)";
                        }
                        oData.Execute(sqlQuery);
                        //remove possible program version table from previous versions
                        if (oData.TableExists("PROGRAM_VERSION"))
                        {
                            sqlQuery = "DROP TABLE [PROGRAM_VERSION]; ";
                            oData.Execute(sqlQuery);
                        }
                        //hidden menus
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [HIDDEN_MENUS] ([MENU_NAME] TEXT NOT NULL, [MENU_TYPE] INT NOT NULL, PRIMARY KEY ([MENU_NAME], [MENU_TYPE]))";
                        oData.Execute(sqlQuery);
                        PNData.LoadHiddenMenus();
                        //menus order
                        if (!oData.TableExists("MENUS_ORDER"))
                        {
                            sqlQuery =
                                "CREATE TABLE [MENUS_ORDER] ([CONTEXT_NAME] TEXT NOT NULL, [MENU_NAME] TEXT NOT NULL, [ORDER_ORIGINAL] INT NOT NULL, [ORDER_NEW] INT NOT NULL, [PARENT_NAME] TEXT, PRIMARY KEY ( [CONTEXT_NAME], [MENU_NAME]));";
                            oData.Execute(sqlQuery);
                            createMenusOrder(oData, true);
                        }
                        else
                        {
                            createMenusOrder(oData, false);
                        }
                        //SMTP profiles
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [SMTP_PROFILES] ([ID] INT UNIQUE ON CONFLICT REPLACE, [ACTIVE] BOOLEAN, [HOST_NAME] TEXT, [DISPLAY_NAME] TEXT, [SENDER_ADDRESS] TEXT PRIMARY KEY UNIQUE, [PASSWORD] TEXT, [PORT] INT )";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM SMTP_PROFILES"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.SmtpProfiles.Add(new PNSmtpProfile
                                {
                                    Active = Convert.ToBoolean(r["ACTIVE"]),
                                    Id = Convert.ToInt32(r["ID"]),
                                    HostName = Convert.ToString(r["HOST_NAME"]),
                                    SenderAddress = Convert.ToString(r["SENDER_ADDRESS"]),
                                    Password = Convert.ToString(r["PASSWORD"]),
                                    Port = Convert.ToInt32(r["PORT"]),
                                    DisplayName = Convert.ToString(r["DISPLAY_NAME"])
                                });
                            }
                        }
                        //mail contacts
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS [MAIL_CONTACTS] ([ID] INT UNIQUE ON CONFLICT REPLACE, [DISPLAY_NAME] TEXT, [ADDRESS] TEXT )";
                        oData.Execute(sqlQuery);
                        using (var t = oData.FillDataTable("SELECT * FROM MAIL_CONTACTS"))
                        {
                            foreach (DataRow r in t.Rows)
                            {
                                PNCollections.Instance.MailContacts.Add(new PNMailContact
                                {
                                    DisplayName = Convert.ToString(r["DISPLAY_NAME"]),
                                    Address = Convert.ToString(r["ADDRESS"]),
                                    Id = Convert.ToInt32(r["ID"]),
                                });
                            }
                        }
                        //services
                        sqlQuery =
                            "CREATE TABLE IF NOT EXISTS SERVICES ( APP_NAME TEXT PRIMARY KEY, CLIENT_ID TEXT, CLIENT_SECRET TEXT, ACCESS_TOKEN TEXT, REFRESH_TOKEN TEXT, TOKEN_EXPIRY TEXT )";
                        oData.Execute(sqlQuery);
                        prepareServices(oData);
                        //triggers
                        sqlQuery = PNStrings.CREATE_TRIGGERS;
                        oData.Execute(sqlQuery);

                        //load plugins
                        PNPlugins.Instance.LoadPlugins(this, PNPaths.Instance.PluginsDir);
                        //rename SkyDrive into OneDrive
                        var pln = PNPlugins.Instance.SyncPlugins.FirstOrDefault(p => p.Name.ToUpper() == "ONEDRIVE");
                        if (pln != null)
                        {
                            sqlQuery = "UPDATE SYNC_PLUGINS SET PLUGIN = 'OneDrive' WHERE PLUGIN = 'SkyDrive'";
                            oData.Execute(sqlQuery);
                            if (PNCollections.Instance.ActiveSyncPlugins.Any(p => p == "SkyDrive"))
                            {
                                PNCollections.Instance.ActiveSyncPlugins.RemoveAll(p => p == "SkyDrive");
                                PNCollections.Instance.ActiveSyncPlugins.Add("OneDrive");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        //private void updateContactsWithoutIp()
        //{
        //    //check whether ip address appears for each contact
        //    try
        //    {
        //        foreach (
        //            var cn in
        //            PNCollections.Instance.Contacts.Where(c => c.UseComputerName && string.IsNullOrEmpty(c.IpAddress))
        //                .Where(PNStatic.SetContactIpAddress))
        //        {
        //            PNData.UpdateContact(cn);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PNStatic.LogException(ex);
        //    }
        //}

        private void upgradeGroupIcon(PNGroup gr, int groupId, string iconString)
        {
            try
            {
                var sb = new StringBuilder("UPDATE GROUPS SET ICON = ");
                string groupIcon;
                switch (groupId)
                {
                    case (int)SpecialGroups.AllGroups:
                        groupIcon = "gr_all.png";
                        break;
                    case 0:
                        groupIcon = "gr.png";
                        break;
                    case (int)SpecialGroups.Diary:
                        groupIcon = "gr_diary.png";
                        break;
                    case (int)SpecialGroups.SearchResults:
                        groupIcon = "gr_search.png";
                        break;
                    case (int)SpecialGroups.Backup:
                        groupIcon = "gr_back.png";
                        break;
                    case (int)SpecialGroups.Favorites:
                        groupIcon = "gr_fav.png";
                        break;
                    case (int)SpecialGroups.Incoming:
                        groupIcon = "gr_inc.png";
                        break;
                    case (int)SpecialGroups.Docking:
                        groupIcon = "dockall_tree.png";
                        break;
                    case (int)SpecialGroups.RecycleBin:
                        groupIcon = "gr_bin.png";
                        break;
                    default:
                        if (iconString.StartsWith("resource."))
                        {
                            groupIcon = iconString.Substring("resource.".Length);
                            groupIcon += ".png";
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
                sb.Append("'");
                sb.Append(groupIcon);
                sb.Append("', IS_DEFAULT_IMAGE = 1");
                sb.Append(" WHERE GROUP_ID = ");
                sb.Append(groupId);
                PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                gr.Image = TryFindResource(Path.GetFileNameWithoutExtension(groupIcon)) as BitmapImage;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void upgradeGroups(SQLiteDataObject oData)
        {
            try
            {
                var list = new List<string>();
                using (
                    var t =
                        oData.FillDataTable(
                            "SELECT GROUP_ID, ICON FROM GROUPS"))
                {
                    foreach (DataRow r in t.Rows)
                    {
                        var sb = new StringBuilder("UPDATE GROUPS SET ");
                        var isDefImage = 1;
                        switch (Convert.ToInt32(r["GROUP_ID"]))
                        {
                            case (int)SpecialGroups.AllGroups:
                                sb.Append(" ICON = 'gr_all.png', ");
                                break;
                            case 0:
                                sb.Append(" ICON = 'gr.png', ");
                                break;
                            case (int)SpecialGroups.Diary:
                                sb.Append(" ICON = 'gr_diary.png', ");
                                break;
                            case (int)SpecialGroups.SearchResults:
                                sb.Append(" ICON = 'gr_search.png', ");
                                break;
                            case (int)SpecialGroups.Backup:
                                sb.Append(" ICON = 'gr_back.png', ");
                                break;
                            case (int)SpecialGroups.Favorites:
                                sb.Append(" ICON = 'gr_fav.png', ");
                                break;
                            case (int)SpecialGroups.Incoming:
                                sb.Append(" ICON = 'gr_inc.png', ");
                                break;
                            case (int)SpecialGroups.Docking:
                                sb.Append(" ICON = 'dockall_tree.png', ");
                                break;
                            case (int)SpecialGroups.RecycleBin:
                                sb.Append(" ICON = 'gr_bin.png', ");
                                break;
                            default:
                                var iconString = Convert.ToString(r["ICON"]);
                                if (iconString.StartsWith("resource."))
                                {
                                    var imageName = iconString.Substring("resource.".Length);
                                    imageName = imageName + ".png";
                                    sb.Append(" ICON = '");
                                    sb.Append(imageName);
                                    sb.Append("', ");
                                }
                                else
                                {
                                    isDefImage = 0;
                                }
                                break;
                        }
                        sb.Append(" IS_DEFAULT_IMAGE = ");
                        sb.Append(isDefImage);
                        sb.Append(" WHERE GROUP_ID = ");
                        sb.Append(r["GROUP_ID"]);
                        list.Add(sb.ToString());
                    }
                }
                PNData.ExecuteTransactionForList(list, oData.ConnectionString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareServices(SQLiteDataObject oData)
        {
            try
            {
                //google data
                var obj = oData.GetScalar("SELECT COUNT(APP_NAME) FROM SERVICES WHERE APP_NAME = 'PNContactsLoader'");
                if (obj == null || PNData.IsDBNull(obj) || Convert.ToInt32(obj) == 0)
                {
                    oData.Execute(
                        "INSERT INTO SERVICES (APP_NAME, CLIENT_ID, CLIENT_SECRET) VALUES('PNContactsLoader', '924809411382-kn585uq0fptek2kduvm6jj85cpdgjj5v.apps.googleusercontent.com','rY40+ZKA34bL+1wExbD3gxSEZTO8DE/iPmzHN/+qOUU=')");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string buildUpdateMenuString(SQLiteDataObject oData, string contextName, string menuName, int index)
        {
            try
            {
                var sb = new StringBuilder("SELECT ORDER_ORIGINAL FROM MENUS_ORDER WHERE CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME = '");
                sb.Append(menuName);
                sb.Append("'");
                var obj = oData.GetScalar(sb.ToString());
                if (obj != null && !PNData.IsDBNull(obj))
                {
                    if (Convert.ToInt32(obj) == index)
                        return "";
                }
                sb = new StringBuilder("UPDATE MENUS_ORDER SET ORDER_ORIGINAL = ");
                sb.Append(index);
                sb.Append(" WHERE CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME = '");
                sb.Append(menuName);
                sb.Append("'");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void newMenusOrder(SQLiteDataObject oData, string contextName, Control item, Control parent,
            int index, IEnumerable<string> names, List<string> updateList)
        {
            try
            {
                var tmi = item as MenuItem;
                if (tmi == null)
                {
                    if (!(item is Separator)) return;
                }

                var enumerable = names as string[] ?? names.ToArray();
                if (enumerable.All(n => n != item.Name))
                    insertMenusOrder(oData, contextName, item, parent, index);
                else
                    updateList.Add(buildUpdateMenuString(oData, contextName, item.Name, index));

                if (tmi == null) return;
                foreach (var ti in tmi.Items.OfType<Control>())
                    newMenusOrder(oData, contextName, ti, tmi, tmi.Items.IndexOf(ti), enumerable, updateList);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertMenusOrder(SQLiteDataObject oData, string contextName, Control item,
            object parent, int index)
        {
            try
            {
                if (!(item is MenuItem))
                {
                    if (!(item is Separator)) return;
                }

                var sb =
                    new StringBuilder(
                        "INSERT INTO MENUS_ORDER (CONTEXT_NAME, MENU_NAME, ORDER_ORIGINAL, ORDER_NEW, PARENT_NAME) VALUES('");
                sb.Append(contextName);
                sb.Append("', '");
                sb.Append(item.Name);
                sb.Append("', ");
                sb.Append(index);
                sb.Append(", ");
                sb.Append(index);
                sb.Append(", ");

                if (parent == null)
                {
                    sb.Append("'')");
                }
                else
                {
                    sb.Append("'");
                    if (parent is Control prItem)
                        sb.Append(prItem.Name);
                    else if (parent is string)
                        sb.Append(parent);
                    else
                        return;
                    sb.Append("')");
                }
                oData.Execute(sb.ToString());

                sb =
                    new StringBuilder("UPDATE MENUS_ORDER SET ORDER_NEW = ORDER_NEW + 1 WHERE ORDER_NEW >= ");
                sb.Append(index);
                sb.Append(" AND CONTEXT_NAME = '");
                sb.Append(contextName);
                sb.Append("' AND MENU_NAME <> '");
                sb.Append(item.Name);
                sb.Append("' AND PARENT_NAME = ");
                if (parent == null)
                {
                    sb.Append("''");
                }
                else
                {
                    sb.Append("'");
                    if (parent is Control prItem)
                        sb.Append(prItem.Name);
                    else sb.Append(parent);
                    sb.Append("'");
                }
                oData.Execute(sb.ToString());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addMenusOrder(SQLiteDataObject oData, string contextName, Control item, object parent, int index)
        {
            try
            {
                insertMenusOrder(oData, contextName, item, parent, index);

                if (!(item is MenuItem tmi)) return;
                foreach (var ti in tmi.Items.OfType<Control>())
                    addMenusOrder(oData, contextName, ti, tmi, tmi.Items.IndexOf(ti));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createMenusOrder(SQLiteDataObject oData, bool firstTime)
        {
            var dn = new WndNote();
            var dcp = new WndCP(false);
            try
            {
                var ctmNote = dn.NoteMenu;
                var ctmEdit = dn.EditMenu;
                var ctmList = dcp.CPMenu;

                if (firstTime)
                {
                    PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("load_menus",
                        "Preparing menus order table");
                    foreach (var ti in ctmPN.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmPN.Name, ti, null, ctmPN.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmNote.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmNote.Name, ti, null, ctmNote.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmEdit.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmEdit.Name, ti, null, ctmEdit.Items.IndexOf(ti));
                    }
                    foreach (var ti in ctmList.Items.OfType<Control>())
                    {
                        addMenusOrder(oData, ctmList.Name, ti, null, ctmList.Items.IndexOf(ti));
                    }
                }
                else
                {
                    var updateList = new List<string>();
                    const string SQL = "SELECT MENU_NAME FROM MENUS_ORDER WHERE CONTEXT_NAME = '%CONTEXT_NAME%'";
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmPN.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmPN.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmPN.Name, ti, null, ctmPN.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmNote.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmNote.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmNote.Name, ti, null, ctmNote.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmEdit.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmEdit.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmEdit.Name, ti, null, ctmEdit.Items.IndexOf(ti), names, updateList);
                        }
                    }
                    using (var t = oData.FillDataTable(SQL.Replace("%CONTEXT_NAME%", ctmList.Name)))
                    {
                        var names = t.AsEnumerable().Select(r => Convert.ToString(r["MENU_NAME"]));
                        foreach (var ti in ctmList.Items.OfType<Control>())
                        {
                            newMenusOrder(oData, ctmList.Name, ti, null, ctmList.Items.IndexOf(ti), names, updateList);
                        }
                    }

                    updateList.RemoveAll(string.IsNullOrWhiteSpace);
                    if (updateList.Count > 0)
                    {
                        PNRuntimes.Instance.SpTextProvider.SplashText = PNLang.Instance.GetMessageText("update_menus_order",
                            "Updating menus indexes");
                        foreach (var s in updateList)
                        {
                            oData.Execute(s);
                        }
                    }
                }
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, false);
                PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, false);
                PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, false);
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, false);
                PNMenus.PrepareDefaultMenuStrip(ctmPN, MenuType.Main, true);
                PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, true);
                PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, true);
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, true);
                PNMenus.CheckAndApplyNewMenusOrder(ctmPN);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                dn.Close();
                dcp.Close();
            }
        }

        private void applyStandardGroupsNames()
        {
            try
            {
                //change groups names
                var values = Enum.GetValues(typeof(SpecialGroups));
                foreach (var e in values)
                {
                    var name = Enum.GetName(typeof(SpecialGroups), e);
                    var g = PNCollections.Instance.Groups.GetGroupById((int)e);
                    if (g != null)
                    {
                        g.Name = PNLang.Instance.GetGroupName(name, g.Name);
                    }
                }
                var gg = PNCollections.Instance.Groups.GetGroupById(0);
                if (gg != null)
                {
                    gg.Name = PNLang.Instance.GetGroupName("General", gg.Name);
                }
                PNRuntimes.Instance.Docking.Name = PNLang.Instance.GetGroupName("Docking", PNRuntimes.Instance.Docking.Name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillGroup(PNGroup gr, DataRow r)
        {
            try
            {
                var c = new ColorConverter();
                var wfc = new WPFFontConverter();
                var lfc = new LogFontConverter();
                var dcc = new System.Drawing.ColorConverter();

                gr.Id = (int)r["GROUP_ID"];
                gr.ParentId = (int)r["PARENT_ID"];
                gr.Name = (string)r["GROUP_NAME"];
                gr.PasswordString = (string)r["PASSWORD_STRING"];
                gr.IsDefaultImage = (bool)r["IS_DEFAULT_IMAGE"];
                if (!PNData.IsDBNull(r["ICON"]))
                {
                    var base64String = (string)r["ICON"];
                    if (!base64String.EndsWith(PNStrings.PNG_EXT))
                    {
                        try
                        {
                            var buffer = Convert.FromBase64String(base64String);
                            if (gr.Id.In((int)SpecialGroups.AllGroups, 0, (int)SpecialGroups.Diary,
                                    (int)SpecialGroups.Backup, (int)SpecialGroups.SearchResults,
                                    (int)SpecialGroups.Favorites, (int)SpecialGroups.Incoming,
                                    (int)SpecialGroups.Docking,
                                    (int)SpecialGroups.RecycleBin) || base64String.StartsWith("resource."))
                            {
                                //possible image data stored as string when data directory just copied into new edition folder
                                upgradeGroupIcon(gr, gr.Id, (string)r["ICON"]);
                            }
                            else
                            {
                                using (var ms = new MemoryStream(buffer))
                                {
                                    ms.Position = 0;
                                    gr.Image = new BitmapImage();
                                    gr.Image.BeginInit();
                                    gr.Image.CacheOption = BitmapCacheOption.OnLoad;
                                    gr.Image.StreamSource = ms;
                                    gr.Image.EndInit();
                                }
                                if (gr.IsDefaultImage)
                                {
                                    gr.IsDefaultImage = false;
                                    var sb = new StringBuilder("UPDATE GROUPS SET IS_DEFAULT_IMAGE = 0");
                                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                                }
                            }
                        }
                        catch (FormatException)
                        {
                            //possible exception when data directory just copied into new edition folder
                            upgradeGroupIcon(gr, gr.Id, (string)r["ICON"]);
                        }
                    }
                    else
                    {
                        if (gr.Id != (int)SpecialGroups.Docking)
                            gr.Image = TryFindResource(Path.GetFileNameWithoutExtension(base64String)) as BitmapImage;
                        else
                        {
                            var image = TryFindResource("dockall_tree") as BitmapImage;
                            gr.Image = image ?? TryFindResource(Path.GetFileNameWithoutExtension(base64String)) as BitmapImage;
                        }
                        // new BitmapImage(new Uri(base64String));
                    }
                }

                try
                {
                    var clr = c.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, (string)r["BACK_COLOR"]);
                    if (clr != null)
                        gr.Skinless.BackColor = (Color)clr;
                }
                catch (FormatException)
                {
                    //possible FormatException after synchronization with old database
                    var clr = dcc.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, (string)r["BACK_COLOR"]);
                    if (clr != null)
                    {
                        var drawingColor = (System.Drawing.Color)clr;
                        gr.Skinless.BackColor = Color.FromArgb(drawingColor.A, drawingColor.R,
                            drawingColor.G, drawingColor.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET BACK_COLOR = '");
                        sb.Append(c.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, gr.Skinless.BackColor));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(gr.Id);
                        PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                    }
                }

                try
                {
                    var clr = c.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, (string)r["CAPTION_FONT_COLOR"]);
                    if (clr != null)
                        gr.Skinless.CaptionColor = (Color)clr;
                }
                catch (FormatException)
                {
                    //possible FormatException after synchronization with old database
                    var clr = dcc.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, (string)r["CAPTION_FONT_COLOR"]);
                    if (clr != null)
                    {
                        var drawingColor = (System.Drawing.Color)clr;
                        gr.Skinless.CaptionColor = Color.FromArgb(drawingColor.A, drawingColor.R,
                            drawingColor.G, drawingColor.B);
                        var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT_COLOR = '");
                        sb.Append(c.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, gr.Skinless.CaptionColor));
                        sb.Append("' WHERE GROUP_ID = ");
                        sb.Append(gr.Id);
                        PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                    }
                }

                var fontString = (string)r["CAPTION_FONT"];
                //try
                //{
                var fonts = new InstalledFontCollection();
                var arr = fontString.Split(',');
                if (fontString.Any(ch => ch == '^'))
                {
                    //old format font string
                    var lf = lfc.ConvertFromString(fontString);
                    gr.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                    sb.Append(wfc.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, gr.Skinless.CaptionFont));
                    sb.Append("' WHERE GROUP_ID = ");
                    sb.Append(gr.Id);
                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                }
                else if (fonts.Families.Any(ff => ff.Name == arr[0]))
                {
                    //normal font string
                    gr.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString(fontString);
                }
                else
                {
                    //possible not existing font name
                    arr[0] = PNStrings.DEF_CAPTION_FONT;
                    fontString = string.Join(",", arr);
                    gr.Skinless.CaptionFont = (PNFont)wfc.ConvertFromString(fontString);
                    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                    sb.Append(fontString);
                    sb.Append("' WHERE GROUP_ID = ");
                    sb.Append(gr.Id);
                    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                }
                //}
                //catch (IndexOutOfRangeException)
                //{
                //    //possible IndexOutOfRangeException after synchronization with old database
                //    var lf = lfc.ConvertFromString(fontString);
                //    gr.Skinless.CaptionFont = PNStatic.FromLogFont(lf);
                //    var sb = new StringBuilder("UPDATE GROUPS SET CAPTION_FONT = '");
                //    sb.Append(wfc.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, gr.Skinless.CaptionFont));
                //    sb.Append("' WHERE GROUP_ID = ");
                //    sb.Append(gr.ID);
                //    PNData.ExecuteTransactionForStringBuilder(sb, PNData.ConnectionString);
                //}

                var skinName = (string)r["SKIN_NAME"];
                if (skinName != PNSkinDetails.NO_SKIN)
                {
                    gr.Skin.SkinName = skinName;
                    //load skin
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, gr.Skin.SkinName) + ".pnskn";
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, gr.Skin);
                    }
                }
                if (!PNData.IsDBNull(r["FONT"]))
                {
                    gr.Font = lfc.ConvertFromString((string)r["FONT"]);
                }
                if (!PNData.IsDBNull(r["FONT_COLOR"]))
                {
                    var clr = dcc.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, (string)r["FONT_COLOR"]);
                    if (clr != null)
                        gr.FontColor = (System.Drawing.Color)clr;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string prepareGroupInsert(int id, int parentId, string name, string imageName)
        {
            try
            {
                var c = new ColorConverter();
                var lfc = new WPFFontConverter();
                var sb = new StringBuilder();
                sb.Append(
                    "INSERT INTO GROUPS (GROUP_ID, PARENT_ID, GROUP_NAME, ICON, BACK_COLOR, CAPTION_FONT_COLOR, FONT_COLOR, CAPTION_FONT, SKIN_NAME, PASSWORD_STRING, IS_DEFAULT_IMAGE) VALUES(");
                sb.Append(id);
                sb.Append(",");
                sb.Append(parentId);
                sb.Append(",'");
                sb.Append(name.Replace("'", "''"));
                sb.Append("','");
                //sb.Append(PNStrings.RESOURCE_PREFIX);
                sb.Append(imageName);
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, PNSkinlessDetails.DefColor));
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, SystemColors.ControlTextColor));
                sb.Append("','");
                sb.Append(c.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, SystemColors.ControlTextColor));
                sb.Append("','");
                var f = new PNFont { FontWeight = FontWeights.Bold };
                sb.Append(lfc.ConvertToString(f));
                sb.Append("','");
                sb.Append(PNSkinDetails.NO_SKIN);
                sb.Append("','',1");
                sb.Append("); ");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void applyLanguage()
        {
            try
            {
                applyMainMenuLanguage();
                var xspell = PNLang.Instance.GetLangElement("spellchecking");
                if (xspell != null)
                {
                    Spellchecking.SetLocalization(xspell);
                }
                ntfPN.ToolTipText = PNLang.Instance.GetMiscText("tray_tooltip",
                    "PNotes - your virtual desktop notes organizer");

                //change schedule descriptions
                PNLang.Instance.ChangeScheduleDescriptions();
                //change commands language
                PNLang.Instance.ApplyCommandsLanguage();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMainMenuLanguage()
        {
            try
            {
                foreach (var ti in ctmPN.Items.OfType<MenuItem>())
                {
                    PNLang.Instance.ApplyMenuItemLanguage(ti, "main_menu");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importDictionaries()
        {
            var oldWin = new Form();
            try
            {
                var found = false;
                var fbd = new FolderBrowserDialog
                {
                    Description =
                        PNLang.Instance.GetCaptionText("import_dicts_caption", "PNotes dictionaries directory")
                };
                if (fbd.ShowDialog(oldWin) != System.Windows.Forms.DialogResult.OK) return;
                var files =
                    Directory.EnumerateFiles(fbd.SelectedPath, "*.*")
                        .Where(f => f.EndsWith(".aff", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".dic", StringComparison.InvariantCultureIgnoreCase));
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (string.IsNullOrEmpty(name)) continue;
                    var destPath = Path.Combine(PNPaths.Instance.DictDir, name);
                    if (File.Exists(destPath)) continue;
                    File.Copy(f, destPath);
                    found = true;
                }
                if (found)
                {
                    var message = PNLang.Instance.GetMessageText("dicts_found",
                        "Applying new dictionaries requires program restart. Do you want to restart now?");
                    if (
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                    {
                        ApplyAction(MainDialogAction.Restart, null);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                oldWin.Close();
            }
        }

        private void importFonts()
        {
            var oldWin = new Form();
            try
            {
                var fbd = new FolderBrowserDialog
                {
                    Description = PNLang.Instance.GetCaptionText("import_fonts_caption", "PNotes fonts directory")
                };
                if (fbd.ShowDialog(oldWin) != System.Windows.Forms.DialogResult.OK) return;
                var files =
                    Directory.EnumerateFiles(fbd.SelectedPath, "*.*")
                        .Where(f => f.EndsWith(".fon", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".fnt", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".ttf", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".ttc", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".fot", StringComparison.InvariantCultureIgnoreCase) ||
                                    f.EndsWith(".otf", StringComparison.InvariantCultureIgnoreCase));
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (string.IsNullOrEmpty(name)) continue;
                    var destPath = Path.Combine(PNPaths.Instance.FontsDir, name);
                    if (File.Exists(destPath)) continue;
                    File.Copy(f, destPath);
                    if (PNInterop.AddFontResourceEx(destPath, PNInterop.FR_PRIVATE, 0))
                    {
                        PNCollections.Instance.CustomFonts.Add(destPath);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                oldWin.Close();
            }
        }

        private void loadNotesAsFiles(int groupId)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = @"PNotes.NET notes|*" + PNStrings.NOTE_EXTENSION,
                    Multiselect = true,
                    Title = PNLang.Instance.GetCaptionText("load_notes", "Load notes")
                };
                if (ofd.ShowDialog(this).Value)
                {
                    loadNotesFromFilesList(ofd.FileNames, groupId);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotesFromFilesList(IEnumerable<string> files, int groupId = 0)
        {
            try
            {
                var encrypted = PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                                PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted;
                if (encrypted)
                {
                    var message = PNLang.Instance.GetMessageText("encrypt_warning_1",
                        "You are about to load notes, while the prograam is using encryption.");
                    message += "\n";
                    message += PNLang.Instance.GetMessageText("encrypt_warning_2",
                        "If these notes have been encrypted before it may cuase them to become unreadable. Continue?");
                    if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                }
                using (var oData = new SQLiteDataObject(PNData.ConnectionString))
                {
                    foreach (var fileName in files)
                    {
                        var id = Path.GetFileNameWithoutExtension(fileName);
                        var sqlQuery = "SELECT ID FROM NOTES WHERE ID = '" + id + "'";
                        var o = oData.GetScalar(sqlQuery);
                        if (o != null && !PNData.IsDBNull(o))
                        {
                            var message = PNLang.Instance.GetMessageText("id_exists",
                                "The note with the same ID already exists");
                            message += " (" + id + ")";
                            WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                        }
                        else
                        {
                            if (!isFileReadable(fileName))
                            {
                                var message = PNLang.Instance.GetMessageText("invalid_file_format",
                                    "Invalid file format. Perhaps it has been encrypted before.");
                                WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                                continue;
                            }
                            var note = groupId > 0 ? new PNote(groupId) : new PNote();
                            note.Id = id;
                            note.Visible = true;
                            note.Dialog = new WndNote(note, fileName, NewNoteMode.File);
                            note.Dialog.Show();
                            note.NoteSize = note.Dialog.GetSize();
                            note.NoteLocation = note.Dialog.GetLocation();
                            note.EditSize = note.Dialog.Edit.Size;
                            PNCollections.Instance.Notes.Add(note);

                            subscribeToNoteEvents(note);

                            PNNotesOperations.SaveNewNote(note);

                            if (id == null) continue;
                            var newPath = Path.Combine(PNPaths.Instance.DataDir, id) + PNStrings.NOTE_EXTENSION;
                            if (!File.Exists(newPath))
                                File.Copy(fileName, newPath, true);

                            if (!encrypted) continue;
                            using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                            {
                                pne.EncryptTextFile(newPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool isFileReadable(string path)
        {
            try
            {
                var re = new RichTextBox();
                re.LoadFile(path, RichTextBoxStreamType.RichText);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
                return false;
            }
        }

        private void newNoteFromClipboardInGroup(int groupId)
        {
            try
            {
                var note = new PNote(groupId);
                note.Dialog = new WndNote(note, NewNoteMode.Clipboard);
                note.Dialog.Show();
                note.Visible = true;

                applyDefaultFont(note);

                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNCollections.Instance.Notes.Add(note);
                NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void newNoteInGroup(int groupId)
        {
            try
            {
                var note = new PNote(groupId);
                note.Dialog = groupId != (int)SpecialGroups.Diary
                    ? new WndNote(note, NewNoteMode.None)
                    : new WndNote(note, NewNoteMode.Diary);
                note.Dialog.Show();
                note.Visible = true;

                applyDefaultFont(note);

                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNCollections.Instance.Notes.Add(note);
                NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkOverdueNotes()
        {
            try
            {
                if (!PNRuntimes.Instance.Settings.Schedule.TrackOverdue) return;
                if (PNSingleton.Instance.InOverdueChecking) return;
                PNSingleton.Instance.InOverdueChecking = true;
                var now = DateTime.Now;
                var list = new List<PNote>();
                var days = 0;

                var notes = PNCollections.Instance.Notes.Where(n => n.Schedule.Type != ScheduleType.None && n.Schedule.Track);
                foreach (var n in notes)
                {
                    DateTime start, alarmDate, startDate;
                    long seconds;
                    var changeZone = !n.Schedule.TimeZone.Equals(TimeZoneInfo.Local);

                    switch (n.Schedule.Type)
                    {
                        case ScheduleType.Once:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            if (now > alarmDate)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.EveryDay:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            if ((n.Schedule.LastRun == DateTime.MinValue || n.Schedule.LastRun.Date <= now.Date.AddDays(-1))
                                && now.IsTimeMore(alarmDate))
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.After:
                            start = n.Schedule.StartFrom == ScheduleStart.ExactTime
                                ? (changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.StartDate)
                                : PNRuntimes.Instance.StartTime;
                            PNStatic.NormalizeStartDate(ref start);
                            seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                            if (seconds > n.Schedule.AlarmAfter.TotalSeconds)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.RepeatEvery:
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                start = n.Schedule.StartFrom == ScheduleStart.ExactTime
                                    ? (changeZone
                                        ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                            n.Schedule.TimeZone)
                                        : n.Schedule.StartDate)
                                    : PNRuntimes.Instance.StartTime;
                            }
                            else
                            {
                                start = n.Schedule.LastRun;
                            }
                            PNStatic.NormalizeStartDate(ref start);
                            seconds = (now - start).Ticks / TimeSpan.TicksPerSecond;
                            if (seconds > n.Schedule.AlarmAfter.TotalSeconds)
                            {
                                list.Add(n);
                            }
                            break;
                        case ScheduleType.Weekly:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local, n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            var dayMin = n.Schedule.Weekdays.Min();
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                //check whether more than week is over
                                var startDw = n.Schedule.StartDate.DayOfWeek;
                                var firstAlarm = n.Schedule.StartDate.AddDays(dayMin - startDw) +
                                                 alarmDate.TimeOfDay;
                                if (Math.Abs((now - firstAlarm).TotalDays) > 7)
                                {
                                    list.Add(n);
                                }
                            }
                            else
                            {
                                if (Math.Abs((n.Schedule.LastRun - now).TotalDays) > 7)
                                {
                                    //more than week is over
                                    list.Add(n);
                                }
                                else
                                {
                                    var lastDw = n.Schedule.LastRun.DayOfWeek;
                                    if (n.Schedule.Weekdays.Any(dw => dw > lastDw && dw < now.DayOfWeek))
                                    {
                                        //there were selected weekdays when alarm was not fired
                                        list.Add(n);
                                    }
                                    else
                                    {
                                        if (n.Schedule.Weekdays.Any(dw => dw == now.DayOfWeek))
                                        {
                                            if (n.Schedule.LastRun.Date != now.Date && now.IsTimeMore(alarmDate))
                                            {
                                                //today is alarm weekday, but time is over
                                                list.Add(n);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case ScheduleType.MonthlyDayOfWeek:
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                alarmDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.AlarmDate;
                                startDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : n.Schedule.StartDate;
                                if (now.DayOfWeek == n.Schedule.MonthDay.WeekDay)
                                {
                                    var isLast = false;
                                    var ordinal = PNStatic.WeekdayOrdinal(now, n.Schedule.MonthDay.WeekDay, ref isLast);
                                    if (n.Schedule.MonthDay.OrdinalNumber == DayOrdinal.Last)
                                    {
                                        if (isLast)
                                        {
                                            if (now.IsTimeMore(alarmDate))
                                            {
                                                list.Add(n);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((int)n.Schedule.MonthDay.OrdinalNumber == ordinal)
                                        {
                                            if (now.IsTimeMore(alarmDate))
                                            {
                                                list.Add(n);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (weekdayOccured(startDate, now.AddDays(-1),
                                        n.Schedule.MonthDay.WeekDay, n.Schedule.MonthDay.OrdinalNumber))
                                    {
                                        list.Add(n);
                                    }
                                }
                            }
                            else
                            {
                                if (weekdayOccured(n.Schedule.LastRun.AddDays(1), now, n.Schedule.MonthDay.WeekDay,
                                    n.Schedule.MonthDay.OrdinalNumber))
                                {
                                    list.Add(n);
                                }
                            }
                            break;
                        case ScheduleType.MonthlyExact:
                            alarmDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.AlarmDate, TimeZoneInfo.Local,
                                    n.Schedule.TimeZone)
                                : n.Schedule.AlarmDate;
                            startDate = changeZone
                                ? TimeZoneInfo.ConvertTime(n.Schedule.StartDate, TimeZoneInfo.Local,
                                    n.Schedule.TimeZone)
                                : n.Schedule.StartDate;
                            // if schedule has not been triggered yet
                            if (n.Schedule.LastRun == DateTime.MinValue)
                            {
                                // if now is exactly the day
                                if (now.Day == alarmDate.Day)
                                {
                                    if (now.IsTimeMore(alarmDate))
                                    {
                                        list.Add(n);
                                    }
                                }
                                else
                                {
                                    // check for day occurence
                                    if (dayOcurred(startDate, now.AddDays(-1), alarmDate.Day))
                                    {
                                        list.Add(n);
                                    }
                                }
                            }
                            else
                            {
                                // check for day occurence
                                if (dayOcurred(n.Schedule.LastRun.AddDays(1), now, alarmDate.Day))
                                {
                                    list.Add(n);
                                }
                            }
                            break;
                        case ScheduleType.MultipleAlerts:
                            foreach (var ma in n.Schedule.MultiAlerts.Where(a => !a.Raised))
                            {
                                alarmDate = changeZone
                                    ? TimeZoneInfo.ConvertTime(ma.Date, TimeZoneInfo.Local,
                                        n.Schedule.TimeZone)
                                    : ma.Date;
                                if (now <= alarmDate) continue;
                                list.Add(n);
                                break;
                            }
                            break;
                    }
                }
                if (list.Count <= 0) return;
                var dov = new WndOverdue(list);
                dov.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                PNSingleton.Instance.InOverdueChecking = false;
            }
        }

        private bool dayOcurred(DateTime start, DateTime end, int day)
        {
            try
            {
                var dateCurrent = start;
                while (dateCurrent < end)
                {
                    if (dateCurrent.Day == day)
                    {
                        return true;
                    }
                    dateCurrent = dateCurrent.AddDays(1);
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool weekdayOccured(DateTime start, DateTime end, DayOfWeek dof, DayOrdinal ordinal)
        {
            try
            {
                var dateCurrent = start;
                while (dateCurrent < end)
                {
                    if (dateCurrent.DayOfWeek == dof)
                    {
                        DayOrdinal ordTemp;
                        var isLast = false;
                        var ordCurrent = PNStatic.WeekdayOrdinal(dateCurrent, dateCurrent.DayOfWeek, ref isLast);
                        if (isLast)
                        {
                            ordTemp = DayOrdinal.Last;
                        }
                        else
                        {
                            ordTemp = (DayOrdinal)ordCurrent;
                        }
                        if (ordTemp == ordinal)
                        {
                            return true;
                        }
                    }
                    dateCurrent = dateCurrent.AddDays(1);
                }
                return false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void checkStartUpShortcut()
        {
            try
            {
                var shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +
                                   PNStrings.SHORTCUT_FILE;
                if (PNRuntimes.Instance.Settings.GeneralSettings.RunOnStart && !File.Exists(shortcutFile))
                {
                    // create shortcut
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
                else if (!PNRuntimes.Instance.Settings.GeneralSettings.RunOnStart && File.Exists(shortcutFile))
                {
                    // delete shortcut
                    File.Delete(shortcutFile);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLogOn()
        {
            try
            {
                var notes = PNCollections.Instance.Notes.Where(n => n.Visible && n.Dialog != null);
                foreach (var note in notes)
                {
                    note.Dialog.Hide();
                    note.Dialog.Show();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        private void runProcessInAnotherThread(string fileName)
        {
            try
            {
                var ptr = new IntPtr();
                var sucessfullyDisabledWow64Redirect = false;

                // Disable x64 directory virtualization if we're on x64,
                // otherwise keyboard launch will fail.
                if (Environment.Is64BitOperatingSystem)
                {
                    sucessfullyDisabledWow64Redirect = Wow64DisableWow64FsRedirection(ref ptr);
                }

                // osk.exe is in windows/system folder. So we can directky call it without path
                using (var osk = new Process())
                {
                    osk.StartInfo.FileName = fileName; //"osk.exe";
                    osk.StartInfo.UseShellExecute = !Environment.Is64BitOperatingSystem;
                    osk.Start();
                }

                // Re-enable directory virtualisation if it was disabled.
                if (!Environment.Is64BitOperatingSystem) return;
                if (sucessfullyDisabledWow64Redirect)
                    Wow64RevertWow64FsRedirection(ptr);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void receiveNote(string data)
        {
            try
            {
                var temp = data.Split(PNStrings.END_OF_ADDRESS);
                var receivedFrom = PNCollections.Instance.Contacts.ContactNameByComputerName(temp[0]);
                var addresses = Dns.GetHostAddresses(temp[0]);
                // because we are on intranet, sender's ip which is equal to ourself ip is most probably ip of our computer
                var recIp = (addresses.Any(ip => ip.Equals(PNSingleton.Instance.IpAddress)))
                    ? PNSingleton.Instance.IpAddress
                    : addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                var sb = new StringBuilder();
                _ReceivedNotes = new List<string>();

                var rawData = temp[1].Split(PNStrings.END_OF_NOTE);
                //rawData[rawData.Length - 1] = rawData[rawData.Length - 1].Substring(0, rawData[rawData.Length - 1].IndexOf(PNStrings.END_OF_FILE));
                for (var i = 0; i < rawData.Length - 1; i++)
                {
                    temp = rawData[i].Split(PNStrings.END_OF_TEXT);
                    var nc = new NoteConverter();
                    var note = (PNote)nc.ConvertFromString(temp[1]);
                    if (note == null) continue;
                    note.Id = DateTime.Now.ToString("yyMMddHHmmssfff");
                    //note.NoteLocation = new Point(0, 0);
                    note.GroupId = (int)SpecialGroups.Incoming;
                    note.PrevGroupId = note.GroupId;
                    note.SentReceived = SendReceiveStatus.Received;
                    note.DateReceived = DateTime.Now;
                    note.ReceivedFrom = receivedFrom;
                    note.ReceivedIp = recIp?.ToString() ?? "";
                    note.NoteLocation =
                        new Point((SystemParameters.WorkArea.Width - note.NoteSize.Width) / 2,
                            (SystemParameters.WorkArea.Height - note.NoteSize.Height) / 2);
                    //new Point(
                    //    (Screen.GetWorkingArea(new System.Drawing.Point((int)Left,
                    //         (int)Top)).Width - note.NoteSize.Width) / 2,
                    //    (Screen.GetWorkingArea(new System.Drawing.Point((int)Left,
                    //         (int)Top)).Height - note.NoteSize.Height) / 2);

                    if (PNRuntimes.Instance.Settings.Network.ReceivedOnTop)
                    {
                        note.Topmost = true;
                    }

                    _ReceivedNotes.Add(note.Id);
                    sb.Append(note.Name);
                    sb.Append(";");
                    //sb.AppendLine();

                    if (!PNRuntimes.Instance.Settings.Network.ShowAfterArrive)
                    {
                        note.Visible = false;
                    }

                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id) + PNStrings.NOTE_EXTENSION;
                    using (var sw = new StreamWriter(path, false))
                    {
                        sw.Write(temp[0]);
                    }
                    if (PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0 &&
                        PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                    {
                        using (var pne = new PNEncryptor(PNRuntimes.Instance.Settings.Protection.PasswordString))
                        {
                            pne.EncryptTextFile(path);
                        }
                    }
                    if (note.Visible)
                    {
                        note.Dialog = new WndNote(note, note.Id, NewNoteMode.Identificator);
                        note.Dialog.Show();
                    }
                    PNCollections.Instance.Notes.Add(note);
                    NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                    subscribeToNoteEvents(note);

                    // save received note
                    PNNotesOperations.SaveNewNote(note);
                    PNNotesOperations.SaveNoteTags(note);
                    if (note.Schedule != null)
                    {
                        PNNotesOperations.SaveNoteSchedule(note);
                    }
                    note.Changed = false;
                }
                if (!PNRuntimes.Instance.Settings.Network.NoSoundOnArrive)
                {
                    PNSound.PlayMailSound();
                }

                if (!PNRuntimes.Instance.Settings.Network.NoNotificationOnArrive)
                {
                    var sbb = new StringBuilder(PNLang.Instance.GetCaptionText("received", "New notes received"));
                    sbb.Append(": ");
                    sbb.Append(sb);
                    if (sbb.Length > 1) sbb.Length -= 1;
                    sbb.AppendLine();
                    sbb.Append(PNLang.Instance.GetMessageText("sender", "Sender:"));
                    sbb.Append(" ");
                    sbb.Append(receivedFrom);
                    var baloon = new Baloon(BaloonMode.NoteReceived);
                    if (PNRuntimes.Instance.Settings.Network.ShowReceivedOnClick || PNRuntimes.Instance.Settings.Network.ShowIncomingOnClick)
                    {
                        baloon.BaloonLink = sbb.ToString();
                    }
                    else
                    {
                        baloon.BaloonText = sbb.ToString();
                    }
                    baloon.BaloonLinkClicked += baloon_BaloonLinkClicked;
                    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 10000);
                }

                NotesReceived?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region System events

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionLogon:
                    case SessionSwitchReason.RemoteConnect:
                    case SessionSwitchReason.ConsoleConnect:
                        checkOverdueNotes();
                        break;
                    case SessionSwitchReason.RemoteDisconnect:
                        applyLogOn();
                        break;
                    case SessionSwitchReason.SessionUnlock:
                        checkOverdueNotes();
                        applyLogOn();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case PowerModes.Resume:
                        checkOverdueNotes();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Event handlers

        private void subscribeToNoteEvents(PNote note)
        {
            note.NoteBooleanChanged += note_NoteBooleanChanged;
            note.NoteNameChanged += note_NoteNameChanged;
            note.NoteGroupChanged += note_NoteGroupChanged;
            note.NoteDateChanged += note_NoteDateChanged;
            note.NoteDockStatusChanged += note_NoteDockStatusChanged;
            note.NoteSendReceiveStatusChanged += note_NoteSendReceiveStatusChanged;
            note.NoteTagsChanged += note_NoteTagsChanged;
            //note.NoteDeletedCompletely += note_NoteDeletedCompletely;
            note.NoteScheduleChanged += note_NoteScheduleChanged;
        }

        void FontUser_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FontFamily")
            {
                ApplyNewDefaultMenu();
            }
        }

        void note_NoteScheduleChanged(object sender, EventArgs e)
        {
            NoteScheduleChanged?.Invoke(sender, e);
        }

        //void note_NoteDeletedCompletely(object sender, NoteDeletedCompletelyEventArgs e)
        //{
        //    if (NoteDeletedCompletely != null)
        //    {
        //        NoteDeletedCompletely(sender, e);
        //    }
        //}

        void note_NoteTagsChanged(object sender, EventArgs e)
        {
            NoteTagsChanged?.Invoke(sender, e);
        }

        void note_NoteSendReceiveStatusChanged(object sender, NoteSendReceiveStatusChangedEventArgs e)
        {
            NoteSendReceiveStatusChanged?.Invoke(sender, e);
        }

        void note_NoteDockStatusChanged(object sender, NoteDockStatusChangedEventArgs e)
        {
            NoteDockStatusChanged?.Invoke(sender, e);
        }

        private void note_NoteDateChanged(object sender, NoteDateChangedEventArgs e)
        {
            NoteDateChanged?.Invoke(sender, e);
        }

        private void note_NoteGroupChanged(object sender, NoteGroupChangedEventArgs e)
        {
            NoteGroupChanged?.Invoke(sender, e);
        }

        private void note_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
        {
            NoteBooleanChanged?.Invoke(sender, e);
        }

        private void note_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
        {
            NoteNameChanged?.Invoke(sender, e);
        }

        #endregion

        #region Private event handlers

        private void dlgNewInGroup_NoteGroupChanged(object sender, NoteGroupChangedEventArgs e)
        {
            if (sender is WndNewInGroup dlgNewInGroup)
                dlgNewInGroup.NoteGroupChanged -= dlgNewInGroup_NoteGroupChanged;
            newNoteInGroup(e.NewGroup);
        }

        #endregion

        #region Menu clicks

        private void exitClick()
        {
            Close();
        }

        private void toPanelAllClick()
        {
            try
            {
                PNNotesOperations.ApplyThumbnails();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fromPanelAllClick()
        {
            try
            {
                if (PNWindows.Instance.FormPanel == null) return;
                PNWindows.Instance.FormPanel.RemoveAllThumbnails();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void onScreenKbrdClick()
        {
            try
            {
                var oskWindow = PNInterop.FindWindow("OSKMainClass", null);
                if (!oskWindow.Equals(IntPtr.Zero))
                    PNInterop.SendMessage(oskWindow, PNInterop.WM_CLOSE, 0, 0);
                else
                {
                    Task.Factory.StartNew(() => runProcessInAnotherThread("osk.exe"));
                    //var t = new Thread(() => runProcessInAnotherThread("osk.exe"));
                    //t.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void magnifierClick()
        {
            try
            {
                var magWindow = PNInterop.FindWindow("MagUIClass", null);
                if (!magWindow.Equals(IntPtr.Zero))
                    PNInterop.SendMessage(magWindow, PNInterop.WM_CLOSE, 0, 0);
                else
                {
                    Task.Factory.StartNew(() => runProcessInAnotherThread("magnify.exe"));
                    //var t = new Thread(() => runProcessInAnotherThread("magnify.exe"));
                    //t.Start();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void newNoteInGroupClick()
        {
            try
            {
                var dlgNewInGroup = new WndNewInGroup();
                dlgNewInGroup.NoteGroupChanged += dlgNewInGroup_NoteGroupChanged;
                var showDialog = dlgNewInGroup.ShowDialog();
                if (!showDialog.HasValue || !showDialog.Value)
                {
                    dlgNewInGroup.NoteGroupChanged -= dlgNewInGroup_NoteGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNoteClick()
        {
            loadNotesAsFiles(0);
        }

        private void noteFromClipboardClick()
        {
            try
            {
                var note = new PNote();
                note.Dialog = new WndNote(note, NewNoteMode.Clipboard);
                note.Dialog.Show();
                note.Visible = true;

                applyDefaultFont(note);

                note.NoteSize = note.Dialog.GetSize();
                note.NoteLocation = note.Dialog.GetLocation();
                note.EditSize = note.Dialog.Edit.Size;

                PNCollections.Instance.Notes.Add(note);
                NewNoteCreated?.Invoke(this, new NewNoteCreatedEventArgs(note));
                subscribeToNoteEvents(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void todayDiaryClick()
        {
            PNNotesOperations.CreateOrShowTodayDiary();
        }

        private void prefsClick()
        {
            try
            {
                if (PNWindows.Instance.FormSettings == null)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Mouse.OverrideCursor = Cursors.Wait;
                    PNWindows.Instance.FormSettings = new WndSettings();
                    PNWindows.Instance.FormSettings.Show();
                }
                else
                {
                    if (PNWindows.Instance.FormSettings.WindowState == WindowState.Minimized)
                        PNWindows.Instance.FormSettings.WindowState = WindowState.Normal;
                }
                PNWindows.Instance.FormSettings.BringToFront();
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

        private void controlPanelClick()
        {
            try
            {
                if (PNWindows.Instance.FormCP == null)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Mouse.OverrideCursor = Cursors.Wait;
                    PNWindows.Instance.FormCP = new WndCP();
                    PNWindows.Instance.FormCP.Show();
                }
                else
                {
                    if (PNWindows.Instance.FormCP.WindowState == WindowState.Minimized)
                    {
                        if (Math.Abs(PNRuntimes.Instance.Settings.Config.CPLocation.X - (-1)) < double.Epsilon &&
                            Math.Abs(PNRuntimes.Instance.Settings.Config.CPLocation.Y - (-1)) < double.Epsilon &&
                            Math.Abs(PNRuntimes.Instance.Settings.Config.CPSize.Width - (-1)) < double.Epsilon &&
                            Math.Abs(PNRuntimes.Instance.Settings.Config.CPSize.Height - (-1)) < double.Epsilon)
                        {
                            PNWindows.Instance.FormCP.WindowState = WindowState.Maximized;
                        }
                        else
                        {
                            PNWindows.Instance.FormCP.WindowState = WindowState.Normal;
                        }
                    }
                }
                PNWindows.Instance.FormCP.BringToFront();
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

        private void hotkeysClick()
        {
            try
            {
                if (PNWindows.Instance.FormHotkeys == null)
                {
                    var d = new WndHotkeys { Owner = this };
                    d.ShowDialog();
                }
                else
                {
                    PNWindows.Instance.FormHotkeys.Activate();
                }
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
                if (PNWindows.Instance.FormMenus == null)
                {
                    var d = new WndMenusManager { Owner = this };
                    d.ShowDialog();
                }
                else
                {
                    PNWindows.Instance.FormMenus.Activate();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showAllClick()
        {
            PNNotesOperations.ShowHideAllGroups(true);
        }

        private void showIncomingClick()
        {
            PNNotesOperations.ShowHideSpecificGroup((int)SpecialGroups.Incoming, true);
        }

        private void hideAllClick()
        {
            PNNotesOperations.ShowHideAllGroups(false);
        }

        private void hideIncomingClick()
        {
            PNNotesOperations.ShowHideSpecificGroup((int)SpecialGroups.Incoming, false);
        }

        private void allToFrontClick()
        {
            try
            {
                var notes = PNCollections.Instance.Notes.Where(n => n.Visible);
                foreach (var note in notes)
                {
                    note.Dialog.SendWindowToForeground();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveAllClick()
        {
            PNNotesOperations.SaveAllNotes(false);
        }

        private void backupCreateClick()
        {
            createFullBackup(PNRuntimes.Instance.Settings.Protection.SilentFullBackup);
        }

        private void backupRestoreClick()
        {
            restoreFromFullBackup();
        }

        private void syncLocalClick()
        {
            try
            {
                var dls = new WndLocalSync { Owner = this };
                var showDialog = dls.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    ApplyAction(MainDialogAction.Restart, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importNotesClick()
        {
            try
            {
                var d = new WndImportNotes { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                if (PNWindows.Instance.FormCP == null) return;
                PNWindows.Instance.FormCP.Close();
                execMenuCommand(mnuCP);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importSettingClick()
        {
            try
            {
                var d = new WndImportSettings { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                if (PNWindows.Instance.FormSettings == null) return;
                PNWindows.Instance.FormSettings.Close();
                execMenuCommand(mnuPrefs);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void importFontsClick()
        {
            importFonts();
        }

        private void importDictionariesClick()
        {
            importDictionaries();
        }

        private void reloadAllClick()
        {
            reloadNotes();
        }

        private void dockAllNoneClick()
        {
            PNNotesOperations.ApplyDocking(DockStatus.None);
        }

        private void dockAllLeftClick()
        {
            PNNotesOperations.ApplyDocking(DockStatus.Left);
        }

        private void dockAllTopClick()
        {
            PNNotesOperations.ApplyDocking(DockStatus.Top);
        }

        private void dockAllRightClick()
        {
            PNNotesOperations.ApplyDocking(DockStatus.Right);
        }

        private void dockAllBottomClick()
        {
            PNNotesOperations.ApplyDocking(DockStatus.Bottom);
        }

        private void sOnHighPriorityClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Priority, true);
        }

        private void sOnProtectionClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Protection, true);
        }

        private void sOnCompleteClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Complete, true);
        }

        private void sOnRollClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Roll, true);
        }

        private void sOnOnTopClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Topmost, true);
        }

        private void sOffHighPriorityClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Priority, false);
        }

        private void sOffProtectionClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Protection, false);
        }

        private void sOffCompleteClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Complete, false);
        }

        private void sOffRollClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Roll, false);
        }

        private void sOffOnTopClick()
        {
            PNNotesOperations.ApplySwitch(NoteBooleanTypes.Topmost, false);
        }

        private void searchInNotesClick()
        {
            try
            {
                if (PNWindows.Instance.FormSearchInNotes == null)
                {
                    PNWindows.Instance.FormSearchInNotes = new WndSearchInNotes();
                    PNWindows.Instance.FormSearchInNotes.Show();
                }
                PNWindows.Instance.FormSearchInNotes.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void searchByTagsClick()
        {
            try
            {
                if (PNWindows.Instance.FormSearchByTags == null)
                {
                    PNWindows.Instance.FormSearchByTags = new WndSearchByTags();
                    PNWindows.Instance.FormSearchByTags.Show();
                }
                PNWindows.Instance.FormSearchByTags.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void searchByDatesClick()
        {
            try
            {
                if (PNWindows.Instance.FormSearchByDates == null)
                {
                    PNWindows.Instance.FormSearchByDates = new WndSearchByDates();
                    PNWindows.Instance.FormSearchByDates.Show();
                }
                PNWindows.Instance.FormSearchByDates.BringToFront();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showAllFavoritesClick()
        {
            try
            {
                var notes = PNCollections.Instance.Notes.Where(n => n.Favorite && n.GroupId != (int)SpecialGroups.RecycleBin);
                foreach (var note in notes)
                {
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lockProgClick()
        {
            PNSingleton.Instance.IsLocked = lockProgram(!PNSingleton.Instance.IsLocked);
        }

        private void helpClick()
        {
            showHelp();
        }

        private void aboutClick()
        {
            var d = new WndAbout { Owner = this };
            d.ShowDialog();
        }

        private void supportClick()
        {
            PNStatic.LoadPage(PNStrings.URL_PAYPAL);
        }

        private void homepageClick()
        {
            PNStatic.LoadPage(PNStrings.URL_MAIN);
        }

        #endregion

        public RichTextBox ActiveTextBox
        {
            get
            {
                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                return d?.Edit;
            }
        }

        public string ActiveCulture => PNLang.Instance.GetLanguageCulture();

        public string ActiveNoteName
        {
            get
            {
                var d = Application.Current.Windows.OfType<WndNote>().FirstOrDefault(w => w.Active);
                if (d == null) return "";
                var note = PNCollections.Instance.Notes.Note(d.Handle);
                return note != null ? note.Name : "";
            }
        }

        public int LimitToGet => PNRuntimes.Instance.Settings.Network.PostCount;

        public Dictionary<string, string> SyncParameters
        {
            get
            {
                var parameters = new Dictionary<string, string>
                {
                    {"createTriggers", PNStrings.CREATE_TRIGGERS},
                    {"dropTriggers", PNStrings.DROP_TRIGGERS},
                    {"dataPath", PNPaths.Instance.DataDir},
                    {"dbPath", PNPaths.Instance.DBPath},
                    {"includeDeleted", PNRuntimes.Instance.Settings.Network.IncludeBinInSync.ToString()},
                    {"noteExt", PNStrings.NOTE_EXTENSION}
                };
                return parameters;
            }
        }

        private void ntfPN_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? result = true;
                if (PNRuntimes.Instance.Settings.Protection.PasswordString != "" && PNSingleton.Instance.IsLocked)
                {
                    var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                    result = d.ShowDialog();
                }
                if (result == null || !result.Value)
                {
                    return;
                }
                _InDblClick = true;
                _TmrDblClick.Stop();
                _Elapsed = 0;
                actionDoubleSingle(PNRuntimes.Instance.Settings.Behavior.DoubleClickAction);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_InDblClick)
                    _InDblClick = false;
                else
                    _TmrDblClick.Start();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Protection.PasswordString == "" || !PNSingleton.Instance.IsLocked) return;
                var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                var result = d.ShowDialog();
                if (result != null && result.Value)
                {
                    PNSingleton.Instance.IsLocked = lockProgram(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ntfPN_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            try
            {
                createDiaryMenu();
                createRunMenu();
                createShowHideMenus();
                createFavoritesMenu();
                createTagsMenus();
                createSyncMenu();

                PNMenus.SetEnabledByHiddenMenus(ctmPN);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void exportMenuClick(string parameter)
        {
            try
            {
                var filter = "";
                switch (parameter)
                {
                    case "mnuExpPdf":
                        filter = PNLang.Instance.GetCaptionText("pdf_filter", "PDF files (*.pdf)|*.pdf");
                        break;
                    case "mnuExpTif":
                        filter = PNLang.Instance.GetCaptionText("tif_filter", "TIF images (*.tif)|*.tif");
                        break;
                    case "mnuExpDoc":
                        filter = PNLang.Instance.GetCaptionText("doc_filter", "Word documents (*.doc)|*.doc");
                        break;
                    case "mnuExpRtf":
                        filter = PNLang.Instance.GetCaptionText("rtf_filter", "RTF documents (*.rtf)|*.rtf");
                        break;
                    case "mnuExpTxt":
                        filter = PNLang.Instance.GetCaptionText("save_as_filter", "Text files (*.txt)|*.txt");
                        break;
                }
                preExportNotes(filter);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void preExportNotes(string filter)
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = filter,
                    InitialDirectory = PNPaths.Instance.BackupDir,
                    Title = PNLang.Instance.GetMenuText("main_menu", "mnuExportAll", "Export All Notes To")
                };
                if (!sfd.ShowDialog(this).Value) return;
                Mouse.OverrideCursor = Cursors.Wait;
                exportNotes(sfd.FileName, PNRuntimes.Instance.Settings.Config.ReportDates.Split("|"));
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

        private void applyDefaultFont(PNote note)
        {
            try
            {
                var noteGroup = PNCollections.Instance.Groups.GetGroupById(note.GroupId) ?? PNCollections.Instance.Groups.GetGroupById(0);
                note.Dialog.Edit.SetFontByFont(noteGroup.Font);
                note.Dialog.Edit.SelectionColor = noteGroup.FontColor;
                //try to apply needed font to entire RichTextBox
                //otherwise size of bullets/numbering is not applied
                try
                {
                    using (var f = System.Drawing.Font.FromLogFont(noteGroup.Font))
                    {
                        note.Dialog.Edit.SelectAll();
                        note.Dialog.Edit.Font = f;
                        note.Dialog.Edit.Select(0, 0);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void syncInBackground()
        {
            try
            {
                if (PNSingleton.Instance.AppClosed) return;
                var plugins = PNPlugins.Instance.SyncPlugins.Where(p => PNCollections.Instance.ActiveSyncPlugins.Contains(p.Name))
                    .OfType<ISyncEnhPlugin>();
                foreach (var syncP in plugins.Where(p => !p.InProgress))
                {
                    syncP.InProgress = true;
                    syncP.SyncCompleteInBackground += syncP_SyncCompleteInBackground;
                    PNStatic.LogThis($"Begin synchronization with {syncP.Name}");
                    syncP.SynchronizeInBackground();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void SyncCompleteInBackgroundDelegate(object sender, SyncCompleteEventArgs e);
        private void syncP_SyncCompleteInBackground(object sender, SyncCompleteEventArgs e)
        {
            if (PNSingleton.Instance.AppClosed) return;
            if (!Dispatcher.CheckAccess())
            {
                SyncCompleteInBackgroundDelegate d = syncP_SyncCompleteInBackground;
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
                if (!(sender is ISyncEnhPlugin syncP)) return;
                syncP.SyncCompleteInBackground -= syncP_SyncCompleteInBackground;
                syncP.InProgress = false;

                if (PNRuntimes.Instance.Settings.Network.SilentSyncType == SilentSyncType.Time)
                {
                    PNRuntimes.Instance.Settings.Config.LastPluginSync = DateTime.Today;
                    PNData.SaveLastSync(false);
                }

                switch (e.Result)
                {
                    case SyncResult.Reload:
                        if (PNRuntimes.Instance.Settings.Network.AutomaticSyncRestart)
                        {
                            PNData.UpdateTablesAfterSync();
                            PNStatic.LogThis(
                                $"Synchronization with {syncP.Name} completed successfully. The program is going to be restarted automatically.");
                            PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Restart, null);
                        }
                        else
                        {
                            PNStatic.LogThis($"Synchronization with {syncP.Name} completed successfully. The program has to be restarted for applying all changes.");
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("sync_complete_reload",
                                    "Synchronization completed successfully. The program has to be restarted for applying all changes."),
                                syncP.Name, MessageBoxButton.OK);
                            PNData.UpdateTablesAfterSync();
                        }
                        break;
                    case SyncResult.AbortVersion:
                        PNStatic.LogThis($"Current version of database is different from previously synchronized version. Synchronization with {syncP.Name} cannot be performed.");
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("diff_versions",
                                "Current version of database is different from previously synchronized version. Synchronization cannot be performed."),
                            syncP.Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    case SyncResult.Error:
                        PNStatic.LogThis($"An error occurred during synchronization with {syncP.Name}.");
                        var sb =
                            new StringBuilder(PNLang.Instance.GetMessageText("sync_error_1",
                                "An error occurred during synchronization."));
                        sb.Append(" (");
                        sb.Append(syncP.Name);
                        sb.Append(")");
                        sb.AppendLine();
                        sb.Append(PNLang.Instance.GetMessageText("sync_error_2",
                            "Please, refer to log file of appropriate plugin."));
                        WPFMessageBox.Show(sb.ToString(), PNStrings.PROG_NAME, MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                    case SyncResult.None:
                        PNStatic.LogThis($"End synchronization with {syncP.Name}.");
                        break;
                }
            }
        }

        private void exportNotes(string fileName, string[] dates)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Protection.PasswordString != "")
                {
                    var d = new WndPasswordDelete(PasswordDlgMode.LoginMain);
                    var result = d.ShowDialog();
                    if (result == null || !result.Value)
                    {
                        return;
                    }
                }
                var extension = Path.GetExtension(fileName);
                if (extension == null) return;
                extension = extension.ToUpper();
                if (!extension.In(".PDF", ".TIF", ".DOC", ".RTF", ".TXT"))
                {
                    //    var message = PNLang.Instance.GetMessageText("export_in_progress",
                    //        "Export notes is in progress. The destination file is:");
                    //    message += "\n";
                    //    message += fileName;
                    //    var baloon = new Baloon(BaloonMode.Export) {BaloonText = message};
                    //    ntfPN.ShowCustomBalloon(baloon, PopupAnimation.Slide, 15000);
                    //}
                    //else
                    //{
                    PNStatic.LogThis("Invalid file specified for notes report");
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_exp_comm_line",
                        "Invalid file specified for notes report"));
                    return;
                }
                var dExp = new WndExport(fileName, extension, dates);
                dExp.ShowDialog();
                //switch (extension)
                //{
                //    case ".PDF":
                //        PNExport.ExportNotes(ReportType.Pdf, fileName, dates);
                //        break;
                //    case ".TIF":
                //        PNExport.ExportNotes(ReportType.Tif, fileName, dates);
                //        break;
                //    case ".DOC":
                //        PNExport.ExportNotes(ReportType.Doc, fileName, dates);
                //        break;
                //    case ".RTF":
                //        PNExport.ExportNotes(ReportType.Rtf, fileName, dates);
                //        break;
                //    case ".TXT":
                //        PNExport.ExportNotes(ReportType.Txt, fileName, dates);
                //        break;
                //}
                //dExp.Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void printFieldsClick()
        {
            try
            {
                if (PNWindows.Instance.FormConfigReport == null)
                {
                    PNWindows.Instance.FormConfigReport = new WndConfigureReport();
                    PNWindows.Instance.FormConfigReport.ShowDialog();
                }
                else
                {
                    PNWindows.Instance.FormConfigReport.Focus();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.NewNote:
                    case CommandType.NewNoteInGroup:
                    case CommandType.LoadNote:
                    case CommandType.Diary:
                    case CommandType.Today:
                    case CommandType.Preferences:
                    case CommandType.ControlPanel:
                    case CommandType.HotkeysCP:
                    case CommandType.MenusManagementCP:
                    case CommandType.ShowHide:
                    case CommandType.ShowGroups:
                    case CommandType.HideGroups:
                    case CommandType.ShowAllOnCp:
                    case CommandType.ShowIncoming:
                    case CommandType.HideAllOnCp:
                    case CommandType.HideIncoming:
                    case CommandType.LastModifiedNotes:
                    case CommandType.LastToday:
                    case CommandType.Last1:
                    case CommandType.Last2:
                    case CommandType.Last3:
                    case CommandType.Last4:
                    case CommandType.Last5:
                    case CommandType.Last6:
                    case CommandType.Last7:
                    case CommandType.AllToFront:
                    case CommandType.SaveAll:
                    case CommandType.BackupSync:
                    case CommandType.BackupCreate:
                    case CommandType.BackupRestore:
                    case CommandType.SyncLocal:
                    case CommandType.ImportNotes:
                    case CommandType.ImportSettings:
                    case CommandType.ImportFonts:
                    case CommandType.ImportDictionaries:
                    case CommandType.ExpAll:
                    case CommandType.ExpPdf:
                    case CommandType.ExpRtf:
                    case CommandType.ExpTif:
                    case CommandType.ExpTxt:
                    case CommandType.ExpDoc:
                    case CommandType.PrintFields:
                    case CommandType.ReloadAll:
                    case CommandType.DockAll:
                    case CommandType.DockAllNone:
                    case CommandType.DockAllLeft:
                    case CommandType.DockAllTop:
                    case CommandType.DockAllRight:
                    case CommandType.DockAllBottom:
                    case CommandType.SwitchOnAll:
                    case CommandType.SwitchOnHighPriority:
                    case CommandType.SwitchOnProtection:
                    case CommandType.SwitchOnComplete:
                    case CommandType.SwitchOnRoll:
                    case CommandType.SwitchOnOnTop:
                    case CommandType.SwitchOffAll:
                    case CommandType.SwitchOffHighPriority:
                    case CommandType.SwitchOffProtection:
                    case CommandType.SwitchOffComplete:
                    case CommandType.SwitchOffRoll:
                    case CommandType.SwitchOffOnTop:
                    case CommandType.SpecialOptions:
                    case CommandType.OnScreenKeyboard:
                    case CommandType.Magnifier:
                    case CommandType.SearchInMain:
                    case CommandType.SearchInNotes:
                    case CommandType.SearchByTags:
                    case CommandType.SearchByDates:
                    case CommandType.FavoritesMain:
                    case CommandType.Help:
                    case CommandType.About:
                    case CommandType.Homepage:
                    case CommandType.Support:
                    case CommandType.Exit:
                        e.CanExecute = true;
                        break;
                    case CommandType.NoteFromClipboard:
                        e.CanExecute = Clipboard.ContainsText(TextDataFormat.Text) ||
                                      Clipboard.ContainsText(TextDataFormat.UnicodeText) ||
                                      Clipboard.ContainsText(TextDataFormat.Rtf) ||
                                      Clipboard.ContainsText(TextDataFormat.Xaml) ||
                                      Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue);
                        break;
                    case CommandType.TagsShowBy:
                    case CommandType.TagsHideBy:
                        e.CanExecute = PNCollections.Instance.Tags.Any();
                        break;
                    case CommandType.Panel:
                    case CommandType.ToPanelAll:
                    case CommandType.FromPanelAll:
                        e.CanExecute = PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel;
                        break;
                    case CommandType.ShowAllFavorites:
                        e.CanExecute = PNCollections.Instance.Notes.Any(n => n.Favorite && n.GroupId != (int)SpecialGroups.RecycleBin);
                        break;
                    case CommandType.LockProgram:
                        e.CanExecute = PNRuntimes.Instance.Settings.Protection.PasswordString.Trim().Length > 0;
                        break;
                    case CommandType.Run:
                        e.CanExecute = PNCollections.Instance.Externals.Count > 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.NewNote:
                        newNoteClick();
                        break;
                    case CommandType.NewNoteInGroup:
                        newNoteInGroupClick();
                        break;
                    case CommandType.LoadNote:
                        loadNoteClick();
                        break;
                    case CommandType.NoteFromClipboard:
                        noteFromClipboardClick();
                        break;
                    case CommandType.Today:
                        todayDiaryClick();
                        break;
                    case CommandType.Preferences:
                        prefsClick();
                        break;
                    case CommandType.ControlPanel:
                        controlPanelClick();
                        break;
                    case CommandType.HotkeysCP:
                        hotkeysClick();
                        break;
                    case CommandType.MenusManagementCP:
                        menusManagementClick();
                        break;
                    case CommandType.ToPanelAll:
                        toPanelAllClick();
                        break;
                    case CommandType.FromPanelAll:
                        fromPanelAllClick();
                        break;
                    case CommandType.ShowAllOnCp:
                        showAllClick();
                        break;
                    case CommandType.ShowIncoming:
                        showIncomingClick();
                        break;
                    case CommandType.HideAllOnCp:
                        hideAllClick();
                        break;
                    case CommandType.HideIncoming:
                        hideIncomingClick();
                        break;
                    case CommandType.LastToday:
                        showRecentNotes(0);
                        break;
                    case CommandType.Last1:
                        showRecentNotes(1);
                        break;
                    case CommandType.Last2:
                        showRecentNotes(2);
                        break;
                    case CommandType.Last3:
                        showRecentNotes(3);
                        break;
                    case CommandType.Last4:
                        showRecentNotes(4);
                        break;
                    case CommandType.Last5:
                        showRecentNotes(5);
                        break;
                    case CommandType.Last6:
                        showRecentNotes(6);
                        break;
                    case CommandType.Last7:
                        showRecentNotes(7);
                        break;
                    case CommandType.AllToFront:
                        allToFrontClick();
                        break;
                    case CommandType.SaveAll:
                        saveAllClick();
                        break;
                    case CommandType.BackupCreate:
                        backupCreateClick();
                        break;
                    case CommandType.BackupRestore:
                        backupRestoreClick();
                        break;
                    case CommandType.SyncLocal:
                        syncLocalClick();
                        break;
                    case CommandType.ImportNotes:
                        importNotesClick();
                        break;
                    case CommandType.ImportSettings:
                        importSettingClick();
                        break;
                    case CommandType.ImportFonts:
                        importFontsClick();
                        break;
                    case CommandType.ImportDictionaries:
                        importDictionariesClick();
                        break;
                    case CommandType.ExpPdf:
                    case CommandType.ExpRtf:
                    case CommandType.ExpTif:
                    case CommandType.ExpTxt:
                    case CommandType.ExpDoc:
                        exportMenuClick(e.Parameter.ToString());
                        break;
                    case CommandType.PrintFields:
                        printFieldsClick();
                        break;
                    case CommandType.ReloadAll:
                        reloadAllClick();
                        break;
                    case CommandType.DockAllNone:
                        dockAllNoneClick();
                        break;
                    case CommandType.DockAllLeft:
                        dockAllLeftClick();
                        break;
                    case CommandType.DockAllTop:
                        dockAllTopClick();
                        break;
                    case CommandType.DockAllRight:
                        dockAllRightClick();
                        break;
                    case CommandType.DockAllBottom:
                        dockAllBottomClick();
                        break;
                    case CommandType.SwitchOnHighPriority:
                        sOnHighPriorityClick();
                        break;
                    case CommandType.SwitchOnProtection:
                        sOnProtectionClick();
                        break;
                    case CommandType.SwitchOnComplete:
                        sOnCompleteClick();
                        break;
                    case CommandType.SwitchOnRoll:
                        sOnRollClick();
                        break;
                    case CommandType.SwitchOnOnTop:
                        sOnOnTopClick();
                        break;
                    case CommandType.SwitchOffHighPriority:
                        sOffHighPriorityClick();
                        break;
                    case CommandType.SwitchOffProtection:
                        sOffProtectionClick();
                        break;
                    case CommandType.SwitchOffComplete:
                        sOffCompleteClick();
                        break;
                    case CommandType.SwitchOffRoll:
                        sOffRollClick();
                        break;
                    case CommandType.SwitchOffOnTop:
                        sOffOnTopClick();
                        break;
                    case CommandType.OnScreenKeyboard:
                        onScreenKbrdClick();
                        break;
                    case CommandType.Magnifier:
                        magnifierClick();
                        break;
                    case CommandType.SearchInNotes:
                        searchInNotesClick();
                        break;
                    case CommandType.SearchByTags:
                        searchByTagsClick();
                        break;
                    case CommandType.SearchByDates:
                        searchByDatesClick();
                        break;
                    case CommandType.ShowAllFavorites:
                        showAllFavoritesClick();
                        break;
                    case CommandType.LockProgram:
                        lockProgClick();
                        break;
                    case CommandType.Help:
                        helpClick();
                        break;
                    case CommandType.About:
                        aboutClick();
                        break;
                    case CommandType.Homepage:
                        homepageClick();
                        break;
                    case CommandType.Support:
                        supportClick();
                        break;
                    case CommandType.Exit:
                        exitClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool execMenuCommand(MenuItem menuItem)
        {
            try
            {
                if (!(menuItem.Command is PNRoutedUICommand command)) return false;
                if (!command.CanExecute(null, menuItem)) return false;
                command.Execute(menuItem.CommandParameter, menuItem);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }
    }
}

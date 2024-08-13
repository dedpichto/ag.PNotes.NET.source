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

using PNRichEdit;
using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Security;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WPFStandardStyles;
using Application = System.Windows.Forms.Application;
using Brush = System.Drawing.Brush;
using Clipboard = System.Windows.Clipboard;
using Color = System.Windows.Media.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using FlowDirection = System.Windows.FlowDirection;
using Fonts = PNStaticFonts.Fonts;
using Image = System.Drawing.Image;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using SystemColors = System.Drawing.SystemColors;
using TextDataFormat = System.Windows.TextDataFormat;
using Timer = System.Timers.Timer;

namespace PNotes.NET
{
    /// <summary>
    ///     Interaction logic for WndNote.xaml
    /// </summary>
    public partial class WndNote
    {
        private const int MIN_HEIGHT = 72;
        private readonly DayOfWeekStruct[] _DaysOfWeek = new DayOfWeekStruct[7];

        private readonly string _InputString = "";
        private readonly NewNoteMode _Mode = NewNoteMode.None;
        private readonly PNote _Note;

        private readonly Dictionary<string, ToolStripMenuItem> _PostPluginsMenus =
            new Dictionary<string, ToolStripMenuItem>();

        private readonly Timer _StopAlarmTimer = new Timer();
        private readonly Timer _TimerAlarm = new Timer(250);
        private DependencyPropertyDescriptor _BackgroundDescriptor;
        private bool _Closing;
        private DropCase _DropCase;
        private EditControl _EditControl;
        private HwndSource _HwndSource;
        private bool _InAlarm;
        private bool _InDock;
        private bool _InDrop;
        private bool _InResize;

        private bool _InRoll;

        //private WndAlarm _DlgAlarm;
        private bool _Loaded;
        private bool _DoNotCheckCanExecute;
        private PinClass _PinClass;
        private PNSkinDetails _RuntimeSkin = new PNSkinDetails();
        private SaveAsNoteNameSetEventArgs _SaveArgs;
        private bool _SizeChangedFirstTime;
        private SpeechSynthesizer _SpeechSynthesizer;

        private enum DropCase
        {
            None,
            Object,
            Content,
            Link
        }

        #region Comparers

        private class AscStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }

        private class DescStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.Compare(y, x, StringComparison.Ordinal);
            }
        }

        #endregion

        #region Constructors

        public WndNote()
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            applyMenusLanguage();
            if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }

        internal WndNote(PNote note, string id, NewNoteMode mode)
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            _Note = note;
            _InputString = id;
            _Mode = mode;
            if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }

        internal WndNote(PNote note, NewNoteMode mode)
        {
            NoteVisual = new DrawingBrush();
            InDockProcess = false;
            InitializeComponent();
            initializaFields();
            _Note = note;
            _Mode = mode;
            if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
            {
                PHeader = SkinlessHeader;
                PFooter = SkinlessFooter;
                PToolbar = SkinlessToolbar;
            }
            else
            {
                PHeader = SkinnableHeader;
                PFooter = SkinnableFooter;
                PToolbar = SkinnableToolbar;
            }
        }

        #endregion

        #region Public properties

        public static DependencyProperty IsRolledProperty = DependencyProperty.Register("IsRolled", typeof(bool),
            typeof(WndNote), new FrameworkPropertyMetadata(false));

        public bool IsRolled
        {
            get => (bool)GetValue(IsRolledProperty);
            private set => SetValue(IsRolledProperty, value);
        }

        #endregion

        #region Internal procedures

        internal void SetThumbnail()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null) return;

                if (note.DockStatus != DockStatus.None) UndockNote(note);

                PNStatic.DoEvents();
                Application.DoEvents();

                NoteVisual = takeSnapshot();

                runThumbnailAnimation(note);

                PNWindows.Instance.FormPanel.Thumbnails.Add(new ThumbnailButton
                {
                    ThumbnailBrush = NoteVisual,
                    Id = note.Id,
                    ThumbnailName = note.Name
                });
                if (!note.Thumbnail)
                {
                    note.Thumbnail = true;
                    PNNotesOperations.SaveNoteThumbnail(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void HideToolbar()
        {
            PToolbar.Visibility = Visibility.Collapsed;
        }

        internal void DockNote(PNote note, DockStatus status, bool fromLoad)
        {
            try
            {
                _InDock = true;
                if (!fromLoad)
                {
                    if (note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                        PNWindows.Instance.FormPanel.RemoveThumbnail(note);

                    var prevStatus = note.DockStatus;
                    note.DockStatus = status;
                    if (PNCollections.Instance.DockedNotes[status].Count > 0)
                        note.DockOrder = PNCollections.Instance.DockedNotes[status].Max(n => n.DockOrder) + 1;
                    else
                        note.DockOrder = 0;
                    PNNotesOperations.ShiftPreviousDock(note, prevStatus, false);
                }

                var wa = PNStatic.AllScreensBounds();

                var multiplier = fromLoad
                    ? note.DockOrder
                    : PNCollections.Instance.DockedNotes[status].Count(n => n.DockOrder < note.DockOrder);
                int w, h;

                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                {
                    w = PNRuntimes.Instance.Settings.GeneralSettings.DockWidth;
                    h = PNRuntimes.Instance.Settings.GeneralSettings.DockHeight;
                    PHeader.SetPNFont(PNRuntimes.Instance.Docking.Skinless.CaptionFont);
                    Foreground = new SolidColorBrush(PNRuntimes.Instance.Docking.Skinless.CaptionColor);
                    Background = new SolidColorBrush(PNRuntimes.Instance.Docking.Skinless.BackColor);
                    this.SetSize(new Size(w, h));
                }
                else
                {
                    w = PNRuntimes.Instance.Docking.Skin.BitmapSkin.Width;
                    h = PNRuntimes.Instance.Docking.Skin.BitmapSkin.Height;
                    Hide();
                    SetRuntimeSkin(PNRuntimes.Instance.Docking.Skin);
                    Show();
                }

                if (TryFindResource("DockStoryboard") is Storyboard stb)
                {
                    if (!(stb.Children[0] is DoubleAnimation anim1) ||
                        !(stb.Children[1] is DoubleAnimation anim2)) return;
                    switch (note.DockStatus)
                    {
                        case DockStatus.Left:
                            anim1.To = wa.Left;
                            anim2.To = wa.Top + multiplier * h;
                            break;
                        case DockStatus.Top:
                            anim1.To = wa.Left + multiplier * w;
                            anim2.To = wa.Top;
                            break;
                        case DockStatus.Right:
                            anim1.To = wa.Right - w;
                            anim2.To = wa.Top + multiplier * h;
                            break;
                        case DockStatus.Bottom:
                            anim1.To = wa.Left + multiplier * w;
                            anim2.To = wa.Bottom - h;
                            break;
                    }

                    stb.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                }
                else
                {
                    switch (note.DockStatus)
                    {
                        case DockStatus.Left:
                            this.SetLocation(new Point(wa.Left, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Top:
                            this.SetLocation(new Point(wa.Left + multiplier * w, wa.Top));
                            break;
                        case DockStatus.Right:
                            this.SetLocation(new Point(wa.Right - w, wa.Top + multiplier * h));
                            break;
                        case DockStatus.Bottom:
                            this.SetLocation(new Point(wa.Left + multiplier * w, wa.Bottom - h));
                            break;
                    }
                }

                Topmost = true;
                if (!fromLoad && note.FromDB) PNNotesOperations.SaveNoteDockStatus(note);
                PNCollections.Instance.DockedNotes[status].Add(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDock = false;
            }
        }

        internal void ApplyBackColor(Color color)
        {
            try
            {
                Background = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyCaptionFont(PNFont font)
        {
            try
            {
                PHeader.SetPNFont(font);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyCaptionFontColor(Color color)
        {
            try
            {
                Foreground = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplySentReceivedStatus(bool visibility)
        {
            PFooter.SetMarkButtonVisibility(MarkType.Mail, visibility);
            if (visibility) SetSendReceiveTooltip();
        }

        internal void ApplyAppearanceAdjustment(NoteAppearanceAdjustedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                {
                    note.Opacity = e.Opacity;
                    note.CustomOpacity = e.CustomOpacity;
                    Opacity = note.Opacity;
                    PNNotesOperations.SaveNoteOpacity(note);

                    if (e.CustomSkinless)
                    {
                        note.Skinless = (PNSkinlessDetails)e.Skinless.Clone();
                        if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                        {
                            PHeader.SetPNFont(note.Skinless.CaptionFont);
                            Foreground = new SolidColorBrush(note.Skinless.CaptionColor);
                            Background = new SolidColorBrush(note.Skinless.BackColor);
                        }
                    }
                    else
                    {
                        note.Skinless = null;
                        var group = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                        if (group != null)
                            if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                            {
                                PHeader.SetPNFont(group.Skinless.CaptionFont);
                                Foreground = new SolidColorBrush(group.Skinless.CaptionColor);
                                Background = new SolidColorBrush(group.Skinless.BackColor);
                            }
                    }

                    PNNotesOperations.SaveNoteSkinless(note);

                    if (e.CustomSkin)
                    {
                        note.Skin = e.Skin.Clone();
                        if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                            PNSkinsOperations.ApplyNoteSkin(this, note);
                    }
                    else
                    {
                        note.Skin = null;
                        var group = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                        if (group != null)
                            if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                                PNSkinsOperations.ApplyNoteSkin(this, note);
                    }

                    PNNotesOperations.SaveNoteSkin(note);

                    if (!e.CustomOpacity && !e.CustomSkin && !e.CustomSkinless)
                        PNNotesOperations.RemoveCustomNotesSettings(note.Id);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyScramble()
        {
            scrambleClick();
        }

        internal void ApplyRollUnroll(PNote note)
        {
            try
            {
                //_InRoll = true;
                if (!note.Rolled)
                {
                    rollUnrollNote(note, true);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                }
                else
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }

            //finally
            //{
            //    _InRoll = false;
            //}
        }

        private delegate bool ApplySaveNoteDelegate(bool rename);

        internal bool ApplySaveNote(bool rename)
        {
            if (!Dispatcher.CheckAccess())
            {
                ApplySaveNoteDelegate d = ApplySaveNote;
                try
                {
                    Dispatcher.Invoke(d, rename);
                }
                catch (ObjectDisposedException)
                {
                    // do nothing when main form is disposed
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                }

                return false;
            }
            return saveClick(rename);
        }

        internal void ApplySave(PNote note, bool showQuestion)
        {
            try
            {
                if (note.Changed)
                {
                    var results = MessageBoxResult.Yes;
                    if (showQuestion && PNRuntimes.Instance.Settings.GeneralSettings.ConfirmSaving)
                    {
                        var message = PNLang.Instance.GetMessageText("save_note_1", "Note has been changed:");
                        message += "\n<" + note.Name + ">\n";
                        message += PNLang.Instance.GetMessageText("save_note_2", "Do you want to save it?");
                        results = WPFMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    }

                    if (results == MessageBoxResult.Yes) saveClick(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SendWindowToForeground()
        {
            PNInterop.BringWindowToTop(_HwndSource.Handle);
        }

        private delegate void ApplySaveByPathDelegate(string path);

        internal void ApplySaveByPath(string path)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ApplySaveByPathDelegate d = ApplySaveByPath;
                    try
                    {
                        Dispatcher.Invoke(d, path);
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
                    Edit.SaveFile(path, RichTextBoxStreamType.RichText);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void ApplyHideNoteDelegate(PNote note);

        internal void ApplyHideNote(PNote note)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ApplyHideNoteDelegate d = ApplyHideNote;
                    try
                    {
                        Dispatcher.Invoke(d, note);
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
                    if (note.Changed)
                    {
                        var results = MessageBoxResult.Yes;
                        if (!PNRuntimes.Instance.Settings.GeneralSettings.SaveWithoutConfirmOnHide)
                        {
                            var message = PNLang.Instance.GetMessageText("save_note_1", "Note has been changed:");
                            message += "\n<" + note.Name + ">\n";
                            message += PNLang.Instance.GetMessageText("save_note_2", "Do you want to save it?");
                            results = WPFMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                        }

                        if (results == MessageBoxResult.Yes)
                            saveClick(false);
                        else
                            note.Changed = false;
                    }

                    if (note.DockStatus != DockStatus.None && !PNSingleton.Instance.InSkinReload)
                        PNNotesOperations.PreUndockNote(note);

                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Visible, false, null);

                    if (PNRuntimes.Instance.Settings.Behavior.PlaySoundOnHide)
                        if (!PNSingleton.Instance.InSkinReload)
                            playSoundOnHide();
                    if (PNRuntimes.Instance.Settings.Behavior.HideFluently)
                    {
                        if (!PNSingleton.Instance.InSkinReload)
                        {
                            if (!(TryFindResource("FadeAway") is Storyboard sb))
                            {
                                note.Dialog = null;
                                Close();
                            }
                            else
                            {
                                sb.Begin();
                            }
                        }
                        else
                        {
                            note.Dialog = null;
                            Close();
                        }
                    }
                    else
                    {
                        note.Dialog = null;
                        Close();
                    }

                    if (note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                        PNWindows.Instance.FormPanel.RemoveThumbnail(note, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyPrintNote()
        {
            printClick();
        }

        internal void ApplyShowScrollBars(RichTextBoxScrollBars value)
        {
            Edit.ScrollBars = value;
            Edit.WordWrap = value != RichTextBoxScrollBars.Horizontal &&
                            value != RichTextBoxScrollBars.Both;
        }

        internal void ApplyHideButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Hide,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplyPanelButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Panel,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplySwitch(NoteBooleanTypes sw)
        {
            try
            {
                var menuName = "";
                switch (sw)
                {
                    case NoteBooleanTypes.Complete:
                        menuName = "mnuMarkAsComplete";
                        break;
                    case NoteBooleanTypes.Priority:
                        menuName = "mnuToggleHighPriority";
                        break;
                    case NoteBooleanTypes.Protection:
                        menuName = "mnuToggleProtectionMode";
                        break;
                    case NoteBooleanTypes.Roll:
                        menuName = "mnuRollUnroll";
                        break;
                    case NoteBooleanTypes.Topmost:
                        menuName = "mnuOnTop";
                        break;
                }

                if (menuName == "") return;

                var menuItem = PNStatic.GetMenuByName(ctmNote, menuName);

                if (menuItem == null) return;
                if (!(menuItem.Command is PNRoutedUICommand command)) return;
                if (command.CanExecute(null, menuItem))
                {
                    command.Execute(null, menuItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ApplyDeleteButtonVisibility(bool visibility)
        {
            PHeader.SetButtonVisibility(HeaderButtonType.Delete,
                visibility ? Visibility.Visible : Visibility.Collapsed);
        }

        internal void ApplyUseAlternative(bool value)
        {
            PHeader.SetAlternative(value);
        }

        internal void ApplyAutoHeight()
        {
            resizeOnAutoheight();
        }

        internal void ApplyMarginsWidth(short marginSize)
        {
            Edit.SetMargins(marginSize);
        }

        internal void UndockNote(PNote note)
        {
            try
            {
                _InDock = true;
                PNNotesOperations.PreUndockNote(note);

                this.SetSize(PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight
                    ? new Size(note.NoteSize.Width, note.AutoHeight)
                    : note.NoteSize);
                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                {
                    if (note.Skinless != null)
                    {
                        PHeader.SetPNFont(note.Skinless.CaptionFont);
                        Foreground = new SolidColorBrush(note.Skinless.CaptionColor);
                        Background = new SolidColorBrush(note.Skinless.BackColor);
                    }
                    else
                    {
                        var group = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                        if (group != null)
                        {
                            PHeader.SetPNFont(group.Skinless.CaptionFont);
                            Foreground = new SolidColorBrush(group.Skinless.CaptionColor);
                            Background = new SolidColorBrush(group.Skinless.BackColor);
                        }
                    }

                    if (note.Rolled)
                    {
                        rollUnrollNote(note, true);
                        PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                    }
                }
                else
                {
                    PNSkinsOperations.ApplyNoteSkin(this, note);
                }

                if (TryFindResource("DockStoryboard") is Storyboard stb)
                {
                    if (!(stb.Children[0] is DoubleAnimation anim1) || !(stb.Children[1] is DoubleAnimation anim2)) return;
                    anim1.To = note.NoteLocation.X;
                    anim2.To = note.NoteLocation.Y;
                    stb.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                }
                else
                {
                    this.SetLocation(note.NoteLocation);
                }

                Topmost = note.Topmost;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InDock = false;
            }
        }

        private delegate bool SaveNoteFileDelegate(PNote note);
        internal bool SaveNoteFile(PNote note)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    SaveNoteFileDelegate d = SaveNoteFile;
                    try
                    {
                        Dispatcher.Invoke(d, note);
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
                    PFooter.SetMarkButtonVisibility(MarkType.Change, false);
                    //mnuSave.IsEnabled = false;
                    Edit.Modified = false;
                    //first save back copy
                    if (PNRuntimes.Instance.Settings.Protection.BackupBeforeSaving) saveBackCopy(note);
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id);
                    path += PNStrings.NOTE_EXTENSION;
                    PNNotesOperations.SaveNoteFile(Edit, path);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        internal void SetSendReceiveTooltip()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                string text;
                switch (note.SentReceived)
                {
                    case SendReceiveStatus.Received:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Received_From", "Received From");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.ReceivedFrom;
                        text += " ";
                        text += note.DateReceived.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateReceived.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                    case SendReceiveStatus.Sent:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Sent_To", "Sent To");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.SentTo;
                        text += " ";
                        text += note.DateSent.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateSent.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                    case SendReceiveStatus.Both:
                        text = PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Sent_To", "Sent To");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.SentTo;
                        text += " ";
                        text += note.DateSent.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateSent.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat);
                        text += Environment.NewLine;
                        text += PNLang.Instance.GetGridColumnCaption("grdNotes_Note_Received_From", "Received From");
                        text += ":";
                        text += Environment.NewLine;
                        text += note.ReceivedFrom;
                        text += " ";
                        text += note.DateReceived.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                        text += " ";
                        text += note.DateReceived.ToString(PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat);
                        PFooter.SetMarkButtonTooltip(MarkType.Mail, text);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private delegate void ApplyTooltipDelegate();

        internal void ApplyTooltip()
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ApplyTooltipDelegate d = ApplyTooltip;
                    Dispatcher.Invoke(d);
                }
                else
                {
                    var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                    var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                    if (note == null) return;
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                        PHeader.ToolTip = note.Name +
                                          (note.DateSaved != DateTime.MinValue
                                              ? " - " +
                                                note.DateSaved.ToString(
                                                    PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci)
                                              : "");
                    else
                        PHeader.ToolTip = note.Name +
                                          (note.DateSaved != DateTime.MinValue
                                              ? " - " +
                                                note.DateSaved.ToString(
                                                    PNRuntimes.Instance.Settings.GeneralSettings.DateFormat, ci)
                                              : "");
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SetFooterButtonSize(ToolStripButtonSize bs)
        {
            try
            {
                PFooter.SetButtonSize(bs);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void DeactivateWindow()
        {
            try
            {
                if (_Closing) return;

                Active = false;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                Opacity = note.CustomOpacity ? note.Opacity : PNRuntimes.Instance.Settings.Behavior.Opacity;

                if (PNRuntimes.Instance.Settings.GeneralSettings.HideToolbar) return;
                PToolbar.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void HotKeyClick(PNHotKey hk)
        {
            try
            {
                MenuItem menuItem = null;
                switch (hk.Type)
                {
                    case HotkeyType.Note:
                        menuItem = PNStatic.GetMenuByName(ctmNote, hk.MenuName);
                        break;
                    case HotkeyType.Edit:
                        menuItem = PNStatic.GetMenuByName(ctmEdit, hk.MenuName);
                        break;
                }

                if (menuItem == null) return;
                if (!(menuItem.Command is PNRoutedUICommand command)) return;
                if (command.CanExecute(null, menuItem))
                {
                    command.Execute(menuItem.CommandParameter, menuItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Internal properties

        internal bool InHotKey { get; set; }

        internal DrawingBrush NoteVisual { get; set; }

        internal void SetRuntimeSkin(PNSkinDetails value, bool fromLoad = false)
        {
            try
            {
                _RuntimeSkin = value;

                this.SetSize(_RuntimeSkin.BitmapPattern.Size.Width, _RuntimeSkin.BitmapPattern.Size.Height);
                SkinImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapSkin, _RuntimeSkin.MaskColor);

                EditTargetSkinnable.Width = _RuntimeSkin.PositionEdit.Width;
                EditTargetSkinnable.Height = _RuntimeSkin.PositionEdit.Height;
                Canvas.SetLeft(EditTargetSkinnable, _RuntimeSkin.PositionEdit.X);
                Canvas.SetTop(EditTargetSkinnable, _RuntimeSkin.PositionEdit.Y);
                _EditControl.WinForm.BackgroundImage = editBackgroundImage();
                //header
                Canvas.SetLeft(SkinnableHeader, _RuntimeSkin.PositionDelHide.X);
                Canvas.SetTop(SkinnableHeader, _RuntimeSkin.PositionDelHide.Y);
                SkinnableHeader.InitialLeft = _RuntimeSkin.PositionDelHide.X;
                SkinnableHeader.HideImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapDelHide,
                    _RuntimeSkin.MaskColor, 0, 0, _RuntimeSkin.BitmapDelHide.Width / 2,
                    _RuntimeSkin.BitmapDelHide.Height);
                SkinnableHeader.DeleteImage.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapDelHide,
                    _RuntimeSkin.MaskColor, _RuntimeSkin.BitmapDelHide.Width / 2, 0,
                    _RuntimeSkin.BitmapDelHide.Width / 2,
                    _RuntimeSkin.BitmapDelHide.Height);
                SkinnableHeader.Width = _RuntimeSkin.PositionDelHide.Width;
                SkinnableHeader.Height = _RuntimeSkin.PositionDelHide.Height;
                //toolbar
                var width = _RuntimeSkin.BitmapCommands.Width / 12;
                var x = 0;
                Canvas.SetLeft(SkinnableToolbar, _RuntimeSkin.PositionToolbar.X);
                Canvas.SetTop(SkinnableToolbar, _RuntimeSkin.PositionToolbar.Y);
                SkinnableToolbar.FontFamilyButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontSizeButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontColorButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontBoldButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontItalicButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontUnderlineButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.FontStrikethroughButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.HighlightButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.LeftButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.CenterButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.RightButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                x += width;
                SkinnableToolbar.BulletsButton.Source = PNStatic.ImageFromDrawingImage(
                    _RuntimeSkin.BitmapCommands, _RuntimeSkin.MaskColor, x, 0, width,
                    _RuntimeSkin.BitmapCommands.Height);
                SkinnableToolbar.Width = _RuntimeSkin.PositionToolbar.Width;
                SkinnableToolbar.Height = _RuntimeSkin.PositionToolbar.Height;

                //footer
                x = 0;
                width = _RuntimeSkin.BitmapMarks.Width / _RuntimeSkin.MarksCount;
                Canvas.SetLeft(SkinnableFooter, _RuntimeSkin.PositionMarks.X);
                Canvas.SetTop(SkinnableFooter, _RuntimeSkin.PositionMarks.Y);
                SkinnableFooter.ScheduleButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.ChangeButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.ProtectedButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PriorityButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.CompleteButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PasswordButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.PinButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.MailButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                x += width;
                SkinnableFooter.EncryptedButton.Source = PNStatic.ImageFromDrawingImage(_RuntimeSkin.BitmapMarks,
                    _RuntimeSkin.MaskColor, x, 0, width, _RuntimeSkin.BitmapMarks.Height);
                SkinnableFooter.Width = _RuntimeSkin.PositionMarks.Width;
                SkinnableFooter.Height = _RuntimeSkin.PositionMarks.Height;

                BorderThickness = new Thickness(0);
                pnlkSkinless.Visibility = Visibility.Hidden;
                cnvSkin.Visibility = Visibility.Visible;
                Background = new SolidColorBrush(Colors.Transparent);
                ResizeMode = ResizeMode.NoResize;
                lnSizeEast.Visibility =
                    lnSizeNorth.Visibility =
                        lnSizeSouth.Visibility =
                            lnSizeWest.Visibility =
                                rectSizeNorthEast.Visibility =
                                    rectSizeNorthWest.Visibility =
                                        rectSizeSouthWest.Visibility =
                                            gripSize.Visibility = Visibility.Hidden;
                if (!fromLoad) updateThumbnail();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool InDockProcess { get; set; }

        internal Guid Handle { get; private set; }

        internal bool IsDisposed => _HwndSource.IsDisposed;

        internal bool InAlarm
        {
            get => _InAlarm;
            set
            {
                var prevAlarm = _InAlarm;
                _InAlarm = value;
                if (_InAlarm && !prevAlarm)
                    startAlarm();
                else
                    stopAlarm();
            }
        }

        internal bool Active { get; private set; }

        internal PNRichEditBox Edit { get; private set; }

        internal ContextMenu EditMenu => ctmEdit;

        internal ContextMenu NoteMenu => ctmNote;

        internal IHeader PHeader { get; set; }

        internal IFooter PFooter { get; set; }

        internal IToolbar PToolbar { get; set; }

        #endregion

        #region Window procedures

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins ||
                    !PNRuntimes.Instance.Settings.Behavior.RollOnDblClick)
                {
                    Edit.Focus();
                    Application.DoEvents();
                }
                else
                {
                    activateWindow();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || PNStatic.FindParent<SkinlessHeader>(e.OriginalSource) == null)
                return;
            if (PNRuntimes.Instance.Settings.Behavior.RollOnDblClick)
            {
                e.Handled = true;
                rollUnrollClick();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_InAlarm) InAlarm = false;

            var resizeMode = ResizeMode;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null || note.DockStatus != DockStatus.None) return;
                if (PNRuntimes.Instance.Settings.Behavior.PreventAutomaticResizing) ResizeMode = ResizeMode.NoResize;

                e.Handled = true;

                PNInterop.DragWindow(_HwndSource.Handle);

                PNNotesOperations.SaveNoteLocation(note, this.GetLocation());

                if (PNRuntimes.Instance.Settings.Behavior.PreventAutomaticResizing) ResizeMode = resizeMode;

                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins ||
                    !PNRuntimes.Instance.Settings.Behavior.RollOnDblClick)
                    Edit.Focus();
                else
                    activateWindow();
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                activateWindow();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var invertTextColor = false;
                var dockFromLoad = true;
                prepareControls();
                createDaysOfWeekArray();

                _StopAlarmTimer.Elapsed += m_StopAlarmTimer_Elapsed;
                _TimerAlarm.Elapsed += _TimerAlarm_Elapsed;
                //hide possible hidden menus
                PNStatic.HideMenus(ctmNote,
                    PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.Note).ToArray());
                PNStatic.HideMenus(ctmEdit,
                    PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.Edit).ToArray());

                //check buttons size
                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins &&
                    PToolbar.GetButtonSize() != PNRuntimes.Instance.Settings.GeneralSettings.ButtonsSize)
                    SetFooterButtonSize(PNRuntimes.Instance.Settings.GeneralSettings.ButtonsSize);

                if (PNRuntimes.Instance.Settings.GeneralSettings.HideToolbar)
                    PToolbar.Visibility = Visibility.Collapsed;

                Opacity = !_Note.CustomOpacity ? PNRuntimes.Instance.Settings.Behavior.Opacity : _Note.Opacity;

                var noteGroup = PNCollections.Instance.Groups.GetGroupById(_Note.GroupId) ??
                                PNCollections.Instance.Groups.GetGroupById(0);

                Edit.ForeColor = System.Drawing.Color.FromArgb(noteGroup.FontColor.R, noteGroup.FontColor.G,
                    noteGroup.FontColor.B);
                
                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                {
                    if (_Note.Skinless == null)
                    {
                        PHeader.SetPNFont(noteGroup.Skinless.CaptionFont);
                        if (!PNRuntimes.Instance.Settings.Behavior.RandomBackColor || _Note.FromDB)
                        {
                            Foreground =
                                new SolidColorBrush(noteGroup.Skinless.CaptionColor);
                            Background =
                                new SolidColorBrush(noteGroup.Skinless.BackColor);
                        }
                        else
                        {
                            var clr = randomColor();
                            Background = new SolidColorBrush(clr);
                            _Note.Skinless =
                                (PNSkinlessDetails)PNCollections.Instance.Groups.GetGroupById(_Note.GroupId).Skinless
                                    .Clone();
                            _Note.Skinless.BackColor = clr;
                            if (PNRuntimes.Instance.Settings.Behavior.InvertTextColor)
                            {
                                clr = invertColor(((SolidColorBrush)Background).Color);
                                Foreground = new SolidColorBrush(clr);
                                _Note.Skinless.CaptionColor = clr;
                                Edit.ForeColor = System.Drawing.Color.FromArgb(clr.R, clr.G, clr.B);
                                invertTextColor = true;
                            }
                        }
                    }
                    else
                    {
                        PHeader.SetPNFont(_Note.Skinless.CaptionFont);
                        Foreground = new SolidColorBrush(_Note.Skinless.CaptionColor);
                        Background = new SolidColorBrush(_Note.Skinless.BackColor);
                    }

                    if (!PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight)
                        if (_Note.NoteSize == Size.Empty)
                            this.SetSize(PNRuntimes.Instance.Settings.GeneralSettings.Width,
                                PNRuntimes.Instance.Settings.GeneralSettings.Height);
                        else
                            this.SetSize(_Note.NoteSize);
                    else
                        Width = _Note.NoteSize == Size.Empty
                            ? PNRuntimes.Instance.Settings.GeneralSettings.Width
                            : _Note.NoteSize.Width;
                }
                else
                {
                    PNSkinsOperations.ApplyNoteSkin(this, _Note, true);
                }

                //set note location
                if (!_Note.NoteLocation.IsEmpty())
                {
                    //if (!m_Note.Thumbnail)
                    _Note.PlaceOnScreen();
                }
                else if (!_Note.FromDB &&
                         PNRuntimes.Instance.Settings.Behavior.StartPosition != NoteStartPosition.Center &&
                         _Note.NoteLocation.IsEmpty())
                {
                    switch (PNRuntimes.Instance.Settings.Behavior.StartPosition)
                    {
                        case NoteStartPosition.Left:
                            _Note.DockStatus = DockStatus.Left;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Top:
                            _Note.DockStatus = DockStatus.Top;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Right:
                            _Note.DockStatus = DockStatus.Right;
                            dockFromLoad = false;
                            break;
                        case NoteStartPosition.Bottom:
                            _Note.DockStatus = DockStatus.Bottom;
                            dockFromLoad = false;
                            break;
                    }

                    //store initial center position for possible future undocking
                    var wr = Screen.FromHandle(_HwndSource.Handle).WorkingArea;
                    _Note.NoteLocation = new Point((wr.Width - PNSettings.DEF_WIDTH) / 2.0,
                        (wr.Height - PNSettings.DEF_HEIGHT) / 2.0);
                    _Note.NoteSize = new Size(PNSettings.DEF_WIDTH, PNSettings.DEF_HEIGHT);
                }

                if (!_Note.FromDB)
                {
                    if (_InputString == "")
                    {
                        //_Edit.SetFontByFont(noteGroup.Font);
                        //_Edit.SelectionColor = noteGroup.FontColor;
                        if (_Mode == NewNoteMode.Clipboard) Edit.Paste();
                    }
                    else
                    {
                        switch (_Mode)
                        {
                            case NewNoteMode.Identificator:
                                var path = Path.Combine(PNPaths.Instance.DataDir, _InputString) +
                                           PNStrings.NOTE_EXTENSION;
                                if (File.Exists(path))
                                    Edit.LoadFile(path, RichTextBoxStreamType.RichText);
                                break;
                            case NewNoteMode.File:
                                if (File.Exists(_InputString))
                                    Edit.LoadFile(_InputString, RichTextBoxStreamType.RichText);
                                break;
                            case NewNoteMode.Clipboard:
                                break;
                        }
                    }

                    if (invertTextColor)
                    {
                        var brush = (SolidColorBrush)Foreground;
                        Edit.SelectionColor = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G,
                            brush.Color.B);
                    }

                    _Note.Topmost = PNRuntimes.Instance.Settings.Behavior.NewNoteAlwaysOnTop;

                    // set note's font accordingly to group settings
                    Edit.SetFontByFont(noteGroup.Font);
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, _InputString) + PNStrings.NOTE_EXTENSION;
                    if (File.Exists(path))
                        PNNotesOperations.LoadNoteFile(Edit, path);
                }

                if (PNRuntimes.Instance.Settings.GeneralSettings.HideDeleteButton)
                    PHeader.SetButtonVisibility(HeaderButtonType.Delete, Visibility.Collapsed);
                if (PNRuntimes.Instance.Settings.GeneralSettings.HideHideButton)
                    PHeader.SetButtonVisibility(HeaderButtonType.Hide, Visibility.Collapsed);
                PHeader.SetButtonVisibility(HeaderButtonType.Panel,
                    PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel ? Visibility.Visible : Visibility.Collapsed);

                if (PNRuntimes.Instance.Settings.GeneralSettings.HideDeleteButton &&
                    PNRuntimes.Instance.Settings.GeneralSettings.ChangeHideToDelete)
                    PHeader.SetAlternative(true);

                PFooter.SetMarkButtonVisibility(MarkType.Schedule, _Note.Schedule.Type != ScheduleType.None);
                PFooter.SetMarkButtonVisibility(MarkType.Priority, _Note.Priority);
                PFooter.SetMarkButtonVisibility(MarkType.Protection, _Note.Protected);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, _Note.Pinned);
                PFooter.SetMarkButtonVisibility(MarkType.Password, _Note.PasswordString.Length > 0);
                PFooter.SetMarkButtonVisibility(MarkType.Complete, _Note.Completed);
                PFooter.SetMarkButtonVisibility(MarkType.Mail,
                    (_Note.SentReceived & SendReceiveStatus.Received) == SendReceiveStatus.Received ||
                    (_Note.SentReceived & SendReceiveStatus.Sent) == SendReceiveStatus.Sent);
                PFooter.SetMarkButtonVisibility(MarkType.Encrypted, _Note.Scrambled);
                applyNoteLanguage();

                if (!PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight &&
                    PNRuntimes.Instance.Settings.GeneralSettings.ShowScrollbar != RichTextBoxScrollBars.None)
                    Edit.ScrollBars = PNRuntimes.Instance.Settings.GeneralSettings.ShowScrollbar;
                Edit.WordWrap = Edit.ScrollBars != RichTextBoxScrollBars.Horizontal &&
                                Edit.ScrollBars != RichTextBoxScrollBars.Both;

                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    Edit.SetMargins(PNRuntimes.Instance.Settings.GeneralSettings.MarginWidth);
                Edit.ReadOnly = _Note.Protected || _Note.Scrambled;
                Edit.CheckSpellingAutomatically = PNRuntimes.Instance.Settings.GeneralSettings.SpellMode == 1;
                Edit.Modified = false;

                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins &&
                    PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight) resizeOnAutoheight();

                if (_Note.Rolled && !PNRuntimes.Instance.Settings.GeneralSettings.UseSkins) rollUnrollNote(_Note, true);

                PNInterop.BringWindowToTop(_HwndSource.Handle);
                _Loaded = true;

                if (_Mode == NewNoteMode.Duplication || _Mode == NewNoteMode.Clipboard) setChangedSign(_Note);

                if (_Note.DockStatus != DockStatus.None) DockNote(_Note, _Note.DockStatus, dockFromLoad);

                if (_Note.Schedule.Type != ScheduleType.None && !_Note.Timer.Enabled) _Note.Timer.Start();

                setScheduleTooltip(_Note);

                PNMenus.CheckAndApplyNewMenusOrder(ctmNote);
                PNMenus.CheckAndApplyNewMenusOrder(ctmEdit);

                PNWindows.Instance.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.HotKeysChanged += FormMain_HotKeysChanged;
                PNWindows.Instance.FormMain.SpellCheckingStatusChanged += FormMain_SpellCheckingStatusChanged;
                PNWindows.Instance.FormMain.SpellCheckingDictionaryChanged += FormMain_SpellCheckingDictionaryChanged;
                PNWindows.Instance.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                PNWindows.Instance.FormMain.MenusOrderChanged += FormMain_MenusOrderChanged;

                if (_Note.DockStatus == DockStatus.None) Topmost = _Note.Topmost;
                if (_Note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel) SetThumbnail();
                if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Edit.Focus();
            }
        }

        private void TopAnimationThumbnail_Completed(object sender, EventArgs e)
        {
            BeginAnimation(TopProperty, null);
            Top = -(SystemParameters.WorkArea.Height + 1);
        }

        private void LeftAnimationThumbnail_Completed(object sender, EventArgs e)
        {
            BeginAnimation(LeftProperty, null);
            Left = -(SystemParameters.WorkArea.Width + 1);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                stopSpeak();
                removeControls();
                PNWindows.Instance.FormMain.LanguageChanged -= FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.HotKeysChanged -= FormMain_HotKeysChanged;
                PNWindows.Instance.FormMain.SpellCheckingStatusChanged -= FormMain_SpellCheckingStatusChanged;
                PNWindows.Instance.FormMain.SpellCheckingDictionaryChanged -= FormMain_SpellCheckingDictionaryChanged;
                PNWindows.Instance.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
                PNWindows.Instance.FormMain.MenusOrderChanged -= FormMain_MenusOrderChanged;
                _RuntimeSkin.Dispose();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
            if (PNRuntimes.Instance.Settings.Behavior.DoNotShowNotesInList)
            {
                var exStyle = PNInterop.GetWindowLong(handle, PNInterop.GWL_EXSTYLE);
                exStyle |= PNInterop.WS_EX_TOOLWINDOW;
                PNInterop.SetWindowLong(handle, PNInterop.GWL_EXSTYLE, exStyle);
            }

            if (PNRuntimes.Instance.Settings.Behavior.KeepVisibleOnShowDesktop) PNStatic.ToggleAeroPeek(handle, true);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _Closing = true;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null || note.DockStatus == DockStatus.None) return;
                var wa = PNStatic.AllScreensBounds();
                switch (note.DockStatus)
                {
                    case DockStatus.Left:
                        if (Top + Height >= wa.Bottom
                            &&
                            PNCollections.Instance.DockedNotes[note.DockStatus].Any(
                                n => n.Dialog.Top + n.Dialog.Height > wa.Bottom))
                            PNWindows.Instance.DockArrows[DockArrow.LeftDown].Show();
                        else if (Top <= wa.Top &&
                                 PNCollections.Instance.DockedNotes[note.DockStatus].Any(n => n.Dialog.Top < wa.Top))
                            PNWindows.Instance.DockArrows[DockArrow.LeftUp].Show();
                        break;
                    case DockStatus.Top:
                        if (Left + Width >= wa.Right
                            &&
                            PNCollections.Instance.DockedNotes[note.DockStatus].Any(
                                n => n.Dialog.Left + n.Dialog.Width > wa.Right))
                            PNWindows.Instance.DockArrows[DockArrow.TopRight].Show();
                        else if (Left <= wa.Left &&
                                 PNCollections.Instance.DockedNotes[note.DockStatus].Any(n => n.Dialog.Left < wa.Left))
                            PNWindows.Instance.DockArrows[DockArrow.TopLeft].Show();
                        break;
                    case DockStatus.Right:
                        if (Top + Height >= wa.Bottom
                            &&
                            PNCollections.Instance.DockedNotes[note.DockStatus].Any(
                                n => n.Dialog.Top + n.Dialog.Height > wa.Bottom))
                            PNWindows.Instance.DockArrows[DockArrow.RightDown].Show();
                        else if (Top <= wa.Top &&
                                 PNCollections.Instance.DockedNotes[note.DockStatus].Any(n => n.Dialog.Top < wa.Top))
                            PNWindows.Instance.DockArrows[DockArrow.RightUp].Show();
                        break;
                    case DockStatus.Bottom:
                        if (Left + Width >= wa.Right
                            &&
                            PNCollections.Instance.DockedNotes[note.DockStatus].Any(
                                n => n.Dialog.Left + n.Dialog.Width > wa.Right))
                            PNWindows.Instance.DockArrows[DockArrow.BottomRight].Show();
                        else if (Left <= wa.Left &&
                                 PNCollections.Instance.DockedNotes[note.DockStatus].Any(n => n.Dialog.Left < wa.Left))
                            PNWindows.Instance.DockArrows[DockArrow.BottomLeft].Show();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null || note.DockStatus == DockStatus.None) return;
                var r = new Rect(this.GetLocation(), this.GetSize());
                var pw = PointToScreen(e.GetPosition(this));
                var factors = PNStatic.GetScalingFactors();
                pw.X /= factors.Item1;
                pw.Y /= factors.Item2;
                if (r.Contains(pw)) return;
                switch (note.DockStatus)
                {
                    case DockStatus.Left:
                        if (PNWindows.Instance.DockArrows[DockArrow.LeftUp].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.LeftUp].Hide();
                        if (PNWindows.Instance.DockArrows[DockArrow.LeftDown].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.LeftDown].Hide();
                        break;
                    case DockStatus.Top:
                        if (PNWindows.Instance.DockArrows[DockArrow.TopLeft].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.TopLeft].Hide();
                        if (PNWindows.Instance.DockArrows[DockArrow.TopRight].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.TopRight].Hide();
                        break;
                    case DockStatus.Right:
                        if (PNWindows.Instance.DockArrows[DockArrow.RightUp].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.RightUp].Hide();
                        if (PNWindows.Instance.DockArrows[DockArrow.RightDown].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.RightDown].Hide();
                        break;
                    case DockStatus.Bottom:
                        if (PNWindows.Instance.DockArrows[DockArrow.BottomLeft].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.BottomLeft].Hide();
                        if (PNWindows.Instance.DockArrows[DockArrow.BottomRight].IsVisible)
                            PNWindows.Instance.DockArrows[DockArrow.BottomRight].Hide();
                        break;
                }
            }
            catch (InvalidOperationException)
            {
                //This Visual is not connected to a PresentationSource - when window becomes hidden - do nothing
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Size procedures

        private void OnSizeSouth(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Down);
            e.Handled = true;
        }

        private void OnSizeNorth(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(PNBorderDragDirection.Up);
            e.Handled = true;
        }

        private void OnSizeEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.Right
                : PNBorderDragDirection.Left);
            e.Handled = true;
        }

        private void OnSizeWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.Left
                : PNBorderDragDirection.Right);
            e.Handled = true;
        }

        private void OnSizeNorthWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.WestNorth
                : PNBorderDragDirection.EastNorth);
            e.Handled = true;
        }

        private void OnSizeNorthEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.EastNorth
                : PNBorderDragDirection.WestNorth);
            e.Handled = true;
        }

        private void OnSizeSouthEast(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.EastSouth
                : PNBorderDragDirection.WestSouth);
            e.Handled = true;
        }

        private void OnSizeSouthWest(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            resizeWindow(FlowDirection == FlowDirection.LeftToRight
                ? PNBorderDragDirection.WestSouth
                : PNBorderDragDirection.EastSouth);
            e.Handled = true;
        }

        private void resizeWindow(PNBorderDragDirection direction)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null || note.Rolled || note.DockStatus != DockStatus.None) return;
                if (PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight &&
                    direction != PNBorderDragDirection.Left && direction != PNBorderDragDirection.Right) return;
                _InResize = true;
                PNInterop.ResizeWindowByBorder(direction, _HwndSource.Handle);
                _InResize = false;
                if (PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight) resizeOnAutoheight();
                Edit.Refresh();
                PNNotesOperations.SaveNoteSize(note, this.GetSize(), Edit.Size);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InResize = false;
            }
        }

        #endregion

        #region Timers

        private delegate void ElapsedTimerDelegate(object sender, ElapsedEventArgs e);

        private void m_StopAlarmTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _StopAlarmTimer.Stop();
                if (!Dispatcher.CheckAccess())
                {
                    ElapsedTimerDelegate d = m_StopAlarmTimer_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    if (_InAlarm) InAlarm = false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _TimerAlarm_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    ElapsedTimerDelegate d = _TimerAlarm_Elapsed;
                    Dispatcher.Invoke(d, sender, e);
                }
                else
                {
                    PopAlarm.IsOpen = !PopAlarm.IsOpen;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Event handlers

        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins) return;
                if (!(Background is SolidColorBrush brush)) return;
                var clr = System.Drawing.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G,
                    brush.Color.B);
                _EditControl.WinForm.BackColor = clr;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteScheduleChanged(object sender, EventArgs e)
        {
            try
            {
                var nt = PNCollections.Instance.Notes.Note(Handle);
                if (!(sender is PNote note) || nt == null || note.Id != nt.Id) return;
                PFooter.SetMarkButtonVisibility(MarkType.Schedule, note.Schedule.Type != ScheduleType.None);
                setScheduleTooltip(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_HotKeysChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (var ti in ctmNote.Items.OfType<MenuItem>()) applyNoteHotkeys(ti, HotkeyType.Note);

                foreach (var ti in ctmEdit.Items.OfType<MenuItem>()) applyNoteHotkeys(ti, HotkeyType.Edit);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            applyNoteLanguage();
            PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, false);
            PNMenus.PrepareDefaultMenuStrip(ctmNote, MenuType.Note, true);
            PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, false);
            PNMenus.PrepareDefaultMenuStrip(ctmEdit, MenuType.Edit, true);
        }

        private void FormMain_SpellCheckingStatusChanged(object sender, SpellCheckingStatusChangedEventArgs e)
        {
            Edit.CheckSpellingAutomatically = e.Status;
            Edit.Invalidate();
            UpdateLayout();
        }

        private void FormMain_SpellCheckingDictionaryChanged(object sender, EventArgs e)
        {
            Edit.Invalidate();
            UpdateLayout();
        }

        private void FormMain_MenusOrderChanged(object sender, MenusOrderChangedEventArgs e)
        {
            if (e.Note)
                rearrangeMenu(ctmNote, MenuType.Note);
            if (e.Edit)
                rearrangeMenu(ctmEdit, MenuType.Edit);
        }

        #endregion

        #region Private procedures

        private void activateWindow()
        {
            try
            {
                if (_Closing) return;
                Active = true;
                Opacity = 1.0;
                if (!PNRuntimes.Instance.Settings.GeneralSettings.HideToolbar) PToolbar.Visibility = Visibility.Visible;

                if (_InAlarm) InAlarm = false;

                PNStatic.DeactivateNotesWindows(Handle);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private DrawingBrush takeSnapshot()
        {
            var dwb = new DrawingBrush();
            try
            {
                var dg = new DrawingGroup();
                dg.Children.Add(new GeometryDrawing
                {
                    Brush = new VisualBrush { Visual = this },
                    Geometry = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight))
                });
                var factors = PNStatic.GetScalingFactors();
                using (var bmp = new Bitmap(Edit.Width, Edit.Height))
                {
                    var pt = PointFromScreen(new Point(_EditControl.WinForm.Left, _EditControl.WinForm.Top));
                    Edit.PrintToBitmap(bmp);
                    //_Edit.DrawToBitmap(bmp, new System.Drawing.Rectangle((int)pt.X, (int)pt.Y, _Edit.Width, _Edit.Height));

                    var bsource = PNStatic.ImageFromDrawingImage(bmp);
                    dg.Children.Add(new GeometryDrawing
                    {
                        Brush = new ImageBrush(bsource),
                        Geometry = new RectangleGeometry(new Rect(pt.X, pt.Y, (int)(Edit.Width / factors.Item1),
                            (int)(Edit.Height / factors.Item2)))
                    });
                    dwb.Drawing = dg;
                }

                return dwb;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return dwb;
            }
        }

        private void startAlarm()
        {
            try
            {
                if (_StopAlarmTimer.Enabled) _StopAlarmTimer.Stop();

                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;

                if (note.DockStatus != DockStatus.None) UndockNote(note);

                if (note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                    PNWindows.Instance.FormPanel.RemoveThumbnail(note);

                if (note.Rolled)
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }

                if (PNRuntimes.Instance.Settings.Schedule.CenterScreen)
                {
                    var wa = SystemParameters
                        .WorkArea; // System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point((int)Left, (int)Top));
                    this.SetLocation(new Point(wa.Left + (wa.Width - Width) / 2, wa.Top + (wa.Height - Height) / 2));
                }

                if (PNRuntimes.Instance.Settings.Schedule.VisualNotification) _TimerAlarm.Start();

                if (PNRuntimes.Instance.Settings.Schedule.AllowSoundAlert)
                {
                    if (!note.Schedule.UseTts)
                    {
                        PNSound.PlayAlarmSound(note.Schedule.Sound, note.Schedule.SoundInLoop);
                    }
                    else
                    {
                        var text = PNStatic.GetNoteText(note);
                        if (text.Trim().Length <= 0) return;
                        _SpeechSynthesizer = new SpeechSynthesizer();
                        PNStatic.SpeakTextAsync(_SpeechSynthesizer, text, note.Schedule.Sound);
                    }
                }

                if (!string.IsNullOrEmpty(note.Schedule.ProgramToRunOnAlert))
                {
                    var ext = PNCollections.Instance.Externals.FirstOrDefault(p =>
                        p.Name == note.Schedule.ProgramToRunOnAlert);
                    if (ext != null)
                        PNStatic.RunExternalProgram(ext.Program, ext.CommandLine);
                }

                if (note.Schedule.StopAfter > 0)
                {
                    _StopAlarmTimer.Interval = note.Schedule.StopAfter;
                    _StopAlarmTimer.Start();
                }
                else if (PNRuntimes.Instance.Settings.Schedule.StopAfter > 0)
                {
                    _StopAlarmTimer.Interval = PNRuntimes.Instance.Settings.Schedule.StopAfter;
                    _StopAlarmTimer.Start();
                }

                switch (note.Schedule.Type)
                {
                    case ScheduleType.Once:
                    case ScheduleType.After:
                        note.Schedule = new PNNoteSchedule();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void stopAlarm()
        {
            try
            {
                _TimerAlarm.Stop();
                PNSound.StopSound();
                stopSpeak();
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (note.Schedule.CloseOnNotification)
                    if (note.Schedule.Type != ScheduleType.None && note.Schedule.Type != ScheduleType.Once &&
                        note.Schedule.Type != ScheduleType.After)
                        ApplyHideNote(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void stopSpeak()
        {
            try
            {
                if (_SpeechSynthesizer == null) return;
                _SpeechSynthesizer.SpeakAsyncCancelAll();
                _SpeechSynthesizer.Dispose();
                _SpeechSynthesizer = null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void StoryboardClose_Completed(object sender, EventArgs e)
        {
            var note = PNCollections.Instance.Notes.Note(Handle);
            if (note != null)
                note.Dialog = null;
            Close();
            if (note == null) return;
            if (note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                PNWindows.Instance.FormPanel.RemoveThumbnail(note);
        }

        private void initializaFields()
        {
            try
            {
                Handle = Guid.NewGuid();
                //add handler for background changed event
                _BackgroundDescriptor =
                    DependencyPropertyDescriptor.FromProperty(BackgroundProperty, typeof(Window));
                _BackgroundDescriptor.AddValueChanged(this, OnBackgroundChanged);

                initializeEdit();
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
                PToolbar.LoadLogFonts();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveBackCopy(PNote note)
        {
            try
            {
                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                var fis = di.GetFiles(note.Id + "*" + PNStrings.NOTE_BACK_EXTENSION);
                var lenght = fis.Length;
                if (lenght == 0)
                {
                    //just save the first back copy
                    var src = Path.Combine(PNPaths.Instance.DataDir, note.Id) + PNStrings.NOTE_EXTENSION;
                    var dest = Path.Combine(PNPaths.Instance.BackupDir, note.Id) + "_1" +
                               PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src)) File.Copy(src, dest, true);
                }
                else if (lenght < PNRuntimes.Instance.Settings.Protection.BackupDeepness)
                {
                    //shift all copies backward and save current note as first
                    var list = new Dictionary<int, string>();
                    foreach (var f in fis)
                    {
                        var name = Path.GetFileNameWithoutExtension(f.Name);
                        var ind = name.IndexOf("_", StringComparison.Ordinal);
                        var number = Convert.ToInt32(name.Substring(ind + 1));
                        list.Add(number, f.FullName);
                    }

                    var ordList = list.OrderByDescending(op => op.Key);

                    var pfx = lenght + 1;
                    string dest;
                    foreach (var op in ordList)
                    {
                        dest = Path.Combine(PNPaths.Instance.BackupDir, note.Id) + "_" +
                               pfx.ToString(PNRuntimes.Instance.CultureInvariant) +
                               PNStrings.NOTE_BACK_EXTENSION;
                        File.Move(op.Value, dest);
                        pfx--;
                    }

                    var src = Path.Combine(PNPaths.Instance.DataDir, note.Id) + PNStrings.NOTE_EXTENSION;
                    dest = Path.Combine(PNPaths.Instance.BackupDir, note.Id) + "_1" + PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src)) File.Copy(src, dest, true);
                }
                else
                {
                    //remove last copy, shift all copies backward and save current note as first
                    var list = new Dictionary<int, string>();
                    foreach (var f in fis)
                    {
                        var name = Path.GetFileNameWithoutExtension(f.Name);
                        var ind = name.IndexOf("_", StringComparison.Ordinal);
                        var number = Convert.ToInt32(name.Substring(ind + 1));
                        list.Add(number, f.FullName);
                    }

                    //get max index
                    var max = list.Max(op => op.Key);
                    //delete file
                    File.Delete(list[max]);
                    //remove pair from list
                    list.Remove(max);
                    var ordList = list.OrderByDescending(op => op.Key);

                    var pfx = PNRuntimes.Instance.Settings.Protection.BackupDeepness;
                    string dest;
                    foreach (var op in ordList)
                    {
                        dest = Path.Combine(PNPaths.Instance.BackupDir, note.Id) + "_" +
                               pfx.ToString(PNRuntimes.Instance.CultureInvariant) +
                               PNStrings.NOTE_BACK_EXTENSION;
                        File.Move(op.Value, dest);
                        pfx--;
                    }

                    var src = Path.Combine(PNPaths.Instance.DataDir, note.Id) + PNStrings.NOTE_EXTENSION;
                    dest = Path.Combine(PNPaths.Instance.BackupDir, note.Id) + "_1" + PNStrings.NOTE_BACK_EXTENSION;
                    if (File.Exists(src)) File.Copy(src, dest, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveClick(bool rename)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return false;
                if (!note.FromDB)
                {
                    if (note.GroupId != (int)SpecialGroups.Diary)
                    {
                        //prompt to save
                        string name = "", text;
                        switch (PNRuntimes.Instance.Settings.Behavior.DefaultNaming)
                        {
                            case DefaultNaming.FirstCharacters:
                                text = Edit.Text.Trim().Length > 0
                                    ? Edit.Text
                                    : PNLang.Instance.GetNoteString("def_caption", "Untitled");
                                text = text.Replace('\n', ' ').Replace('\r', ' ');
                                name = text.Length < PNRuntimes.Instance.Settings.Behavior.DefaultNameLength
                                    ? text
                                    : text.Substring(0, PNRuntimes.Instance.Settings.Behavior.DefaultNameLength);
                                break;
                            case DefaultNaming.DateTime:
                                name = DateTime.Now.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                                break;
                            case DefaultNaming.DateTimeAndFirstCharacters:
                                name = DateTime.Now.ToString(PNRuntimes.Instance.Settings.GeneralSettings.DateFormat);
                                name += " ";
                                text = Edit.Text.Trim().Length > 0
                                    ? Edit.Text
                                    : PNLang.Instance.GetNoteString("def_caption", "Untitled");
                                text = text.Replace('\n', ' ').Replace('\r', ' ');
                                if (text.Length < PNRuntimes.Instance.Settings.Behavior.DefaultNameLength)
                                    name += text;
                                else
                                    name += text.Substring(0, PNRuntimes.Instance.Settings.Behavior.DefaultNameLength);
                                break;
                        }

                        var dlgSaveAs = new WndSaveAs(name, note.GroupId) { Owner = this };
                        dlgSaveAs.SaveAsNoteNameSet += dlgSaveAs_SaveAsNoteNameSet;
                        var showDialog = dlgSaveAs.ShowDialog();
                        if (showDialog == null || showDialog.Value == false)
                        {
                            dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
                            return false;
                        }
                    }
                    else
                    {
                        _SaveArgs = new SaveAsNoteNameSetEventArgs(note.Name, note.GroupId);
                    }
                }
                else
                {
                    if (rename)
                    {
                        var dlgSaveAs = new WndSaveAs(note.Name, note.GroupId) { Owner = this };
                        dlgSaveAs.SaveAsNoteNameSet += dlgSaveAs_SaveAsNoteNameSet;
                        var showDialog = dlgSaveAs.ShowDialog();
                        if (showDialog == null || showDialog.Value == false)
                        {
                            dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
                            return false;
                        }
                    }
                    else
                    {
                        _SaveArgs = new SaveAsNoteNameSetEventArgs(note.Name, note.GroupId);
                    }
                }

                if (!SaveNoteFile(note)) return false;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Change, false, _SaveArgs);
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
            finally
            {
                ApplyTooltip();
            }
        }

        private void dlgSaveAs_SaveAsNoteNameSet(object sender, SaveAsNoteNameSetEventArgs e)
        {
            var note = PNCollections.Instance.Notes.Note(Handle);
            if (sender is WndSaveAs dlgSaveAs) dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
            PHeader.Title = Title = e.Name;
            _SaveArgs = new SaveAsNoteNameSetEventArgs(e.Name, e.GroupId);
            if (note == null || note.GroupId == e.GroupId) return;
            var pnGroup = PNCollections.Instance.Groups.GetGroupById(e.GroupId);
            if (pnGroup != null) PNNotesOperations.ChangeNoteLookOnGroupChange(this, pnGroup);
        }

        private void resizeOnAutoheight()
        {
            try
            {
                edit_ContentsResized(Edit,
                    new ContentsResizedEventArgs(new Rectangle(0, 0, Edit.Width,
                        getEditContentHeight() + (int)PFooter.ActualHeight)));
                scrollToFirstVisibleLine();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private int getEditContentHeight()
        {
            try
            {
                var fontHeight = Edit.GetFontSize();

                if (Edit.TextLength == 0) return fontHeight + 16; //_Edit.Height;

                var pos1 = Edit.GetPositionFromCharIndex(0);
                var pos2 = Edit.GetPositionFromCharIndex(Edit.TextLength - 1);

                if (pos2.Y < pos1.Y) return fontHeight + 16; //_Edit.Height;

                var selStart = Edit.SelectionStart;
                var selLength = Edit.SelectionLength;
                Edit.SelectionStart = Edit.TextLength - 1;
                Edit.SelectionLength = 0;
                //var fontHeight = _Edit.GetFontSize();
                Edit.SelectionStart = selStart;
                Edit.SelectionLength = selLength;

                return pos2.Y - pos1.Y + fontHeight + 16;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return Edit.Height;
            }
        }

        private void scrollToFirstVisibleLine()
        {
            try
            {
                var selStart = Edit.SelectionStart;
                var selLength = Edit.SelectionLength;
                Edit.SelectionStart = 0;
                Edit.SelectionLength = 0;
                Edit.ScrollToCaret();
                Edit.SelectionStart = selStart;
                Edit.SelectionLength = selLength;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private TextureBrush skinTextureBrush(int width, int height)
        {
            try
            {
                var pos = Edit.GetPositionFromCharIndex(Edit.SelectionStart);
                pos.X += _RuntimeSkin.PositionEdit.X;
                pos.Y += _RuntimeSkin.PositionEdit.Y;
                using (var bmp = new Bitmap(width, height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(_RuntimeSkin.BitmapSkin, new Rectangle(0, 0, width, height),
                            new Rectangle(pos.X, pos.Y, width, height), GraphicsUnit.Pixel);
                    }

                    return new TextureBrush(bmp);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private Image editBackgroundImage()
        {
            try
            {
                var factors = PNStatic.GetScalingFactors();
                var bmp = new Bitmap((int)(_RuntimeSkin.BitmapForEdit.Width * factors.Item1),
                    (int)(_RuntimeSkin.BitmapForEdit.Height * factors.Item2));
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(_RuntimeSkin.BitmapForEdit,
                        new Rectangle(0, 0, (int)(_RuntimeSkin.BitmapForEdit.Width * factors.Item1),
                            _RuntimeSkin.BitmapForEdit.Height * 125 / 100),
                        new Rectangle((int)(_RuntimeSkin.PositionEdit.X * factors.Item1),
                            (int)(_RuntimeSkin.PositionEdit.Y * factors.Item2),
                            (int)(_RuntimeSkin.BitmapForEdit.Width * factors.Item1),
                            (int)(_RuntimeSkin.BitmapForEdit.Height * factors.Item2)),
                        GraphicsUnit.Pixel);
                }

                return bmp;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void insertImage(Image image, bool isTransparent = false)
        {
            try
            {
                InHotKey = false;
                using (var bmp = new Bitmap(image.Width, image.Height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var clr = ((SolidColorBrush)Background).Color;
                        var backColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                        Brush br;
                        if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                            br = new SolidBrush(backColor);
                        else
                            br = skinTextureBrush(image.Width, image.Height);
                        try
                        {
                            g.FillRectangle(br, 0, 0, bmp.Width, bmp.Height);
                            if (!isTransparent)
                                using (var b = new Bitmap(image))
                                {
                                    if (br is SolidBrush brush)
                                        b.MakeTransparent(brush.Color);
                                    g.DrawImage(b, 0, 0);
                                }
                            else
                                g.DrawImage(image, 0, 0);
                        }
                        finally
                        {
                            br?.Dispose();
                        }
                    }

                    Edit.InsertImage(bmp);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void rearrangeMenu(ContextMenu ctm, MenuType type)
        {
            try
            {
                PNMenus.RearrangeMenus(ctm);
                PNMenus.PrepareDefaultMenuStrip(ctm, type, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyNoteHotkeys(MenuItem ti, HotkeyType type)
        {
            try
            {
                foreach (var t in ti.Items.OfType<MenuItem>()) applyNoteHotkeys(t, type);
                PNHotKey hk;
                switch (type)
                {
                    case HotkeyType.Note:
                        hk = PNCollections.Instance.HotKeysNote.FirstOrDefault(h => h.MenuName == ti.Name);
                        if (hk != null && ti.InputGestureText == "") ti.InputGestureText = hk.Shortcut;
                        break;
                    case HotkeyType.Edit:
                        hk = PNCollections.Instance.HotKeysEdit.FirstOrDefault(h => h.MenuName == ti.Name);
                        if (hk != null && ti.InputGestureText == "") ti.InputGestureText = hk.Shortcut;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void playSoundOnHide()
        {
            try
            {
                var stream = System.Windows.Application.GetResourceStream(new Uri("sounds/dice.wav", UriKind.Relative));
                if (stream == null) return;
                var player = new SoundPlayer(stream.Stream);
                player.Play();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updateThumbnail()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                if (!note.Thumbnail) return;

                var index = PNWindows.Instance.FormPanel.RemoveThumbnail(note, false);

                PNStatic.DoEvents();
                Application.DoEvents();

                NoteVisual = takeSnapshot();

                runThumbnailAnimation(note);

                PNWindows.Instance.FormPanel.Thumbnails.Insert(index,
                    new ThumbnailButton { ThumbnailBrush = NoteVisual, Id = note.Id, ThumbnailName = note.Name });
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        private void runThumbnailAnimation(PNote note)
        {
            try
            {
                if (TryFindResource("ThumbnailStoryboard") is Storyboard stb)
                {
                    if (!(stb.Children[0] is DoubleAnimation dbaLeft) || !(stb.Children[1] is DoubleAnimation dbaTop))
                    {
                        this.SetLocation(-(SystemParameters.WorkArea.Width + 1),
                            -(SystemParameters.WorkArea.Height + 1));
                    }
                    else
                    {
                        switch (PNRuntimes.Instance.Settings.Behavior.NotesPanelOrientation)
                        {
                            case NotesPanelOrientation.Left:
                                dbaLeft.To = SystemParameters.WorkArea.Left - (note.NoteLocation.X + 1);
                                dbaTop.To = (SystemParameters.WorkArea.Height - note.NoteSize.Height) / 2;
                                break;
                            case NotesPanelOrientation.Top:
                                dbaLeft.To = (SystemParameters.WorkArea.Width - note.NoteSize.Width) / 2;
                                dbaTop.To = SystemParameters.WorkArea.Top - (note.NoteLocation.Y + 1);
                                break;
                        }

                        BeginStoryboard(stb);
                    }
                }
                else
                {
                    this.SetLocation(-(SystemParameters.WorkArea.Width + 1), -(SystemParameters.WorkArea.Height + 1));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setScheduleTooltip(PNote note)
        {
            try
            {
                PFooter.SetMarkButtonTooltip(MarkType.Schedule,
                    note.Schedule.Type != ScheduleType.None
                        ? PNLang.Instance.GetNoteScheduleDescription(note.Schedule, _DaysOfWeek)
                        : "");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setChangedSign(PNote note)
        {
            try
            {
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Change, true, null);
                //mnuSave.IsEnabled = true;
                if (!PFooter.IsMarkButtonVisible(MarkType.Change))
                    PFooter.SetMarkButtonVisibility(MarkType.Change, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void rollUnrollNote(PNote note, bool roll)
        {
            try
            {
                _InRoll = true;
                if (note.DockStatus != DockStatus.None) return;
                if (roll)
                {
                    _EditControl.WinForm.Visible = false;
                    Height = MinHeight;
                    if (PNRuntimes.Instance.Settings.Behavior.FitWhenRolled) Width = MinWidth;
                    IsRolled = true;
                }
                else
                {
                    this.SetSize(PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight
                        ? new Size(note.NoteSize.Width, note.AutoHeight)
                        : note.NoteSize);
                    IsRolled = false;
                    _EditControl.WinForm.Visible = true;
                    var pt = this.GetLocation();
                    var totalWidth =
                        PNStatic.AllScreensBounds()
                            .Width; // System.Windows.Forms.Screen.AllScreens.Sum(sc => sc.WorkingArea.Width);
                    var diffX = Left + note.NoteSize.Width - totalWidth;
                    var diffY = Top + note.NoteSize.Height - SystemParameters.WorkArea.Height;
                    //System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width,
                    //    (int)Height)).Height;
                    if (diffX <= 0 && diffY <= 0) return;
                    if (diffX > 0) pt.X -= diffX;
                    if (diffY > 0) pt.Y -= diffY;
                    this.SetLocation(pt);
                    PNNotesOperations.SaveNoteLocation(note, this.GetLocation());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InRoll = false;
            }
        }

        private Color randomColor()
        {
            var color = PNCollections.Instance.Groups.GetGroupById(0).Skinless.BackColor;
            try
            {
                var rand = new Random();
                var bytes = new byte[3];
                rand.NextBytes(bytes);
                color = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                return color;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return color;
            }
        }

        private Color invertColor(Color srcColor)
        {
            try
            {
                return Color.FromArgb(255, (byte)~srcColor.R, (byte)~srcColor.G, (byte)~srcColor.B);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return srcColor;
            }
        }

        private void createDaysOfWeekArray()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var values = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
                for (var i = 0; i < values.Length; i++)
                    _DaysOfWeek[i] = new DayOfWeekStruct { DayOfW = values[i], Name = ci.DateTimeFormat.DayNames[i] };
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initializeEdit()
        {
            try
            {
                _EditControl = PNRuntimes.Instance.Settings.GeneralSettings.UseSkins
                    ? new EditControl(EditTargetSkinnable)
                    : new EditControl(EditTargetSkinless);
                Edit = _EditControl.EditBox;
                Edit.HideSelection = false;
                Edit.RightToLeft = PNLang.Instance.GetFlowDirection() == FlowDirection.LeftToRight
                    ? RightToLeft.No
                    : RightToLeft.Yes;
                Edit.MouseUp += edit_MouseUp;
                Edit.ContentsResized += edit_ContentsResized;
                Edit.DragDrop += edit_DragDrop;
                Edit.KeyDown += edit_KeyDown;
                Edit.KeyPress += edit_KeyPress;
                Edit.LinkClicked += edit_LinkClicked;
                Edit.PNREActivatedByMouse += edit_PNREActivatedByMouse;
                Edit.TextChanged += edit_TextChanged;
                Edit.GotFocus += edit_GotFocus;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void closeContextMenus()
        {
            try
            {
                if (ctmBullets.IsOpen) ctmBullets.IsOpen = false;
                if (ctmFontColor.IsOpen) ctmFontColor.IsOpen = false;
                if (ctmFontHighlight.IsOpen) ctmFontHighlight.IsOpen = false;
                if (ctmFontSize.IsOpen) ctmFontSize.IsOpen = false;
                if (ctmDrop.IsOpen) ctmDrop.IsOpen = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyNoteLanguage()
        {
            try
            {
                PToolbar.SetButtonTooltip(FormatType.FontFamily,
                    PNLang.Instance.GetNoteString("cmdFontFamily", "Font family"));

                PToolbar.SetButtonTooltip(FormatType.FontSize,
                    PNLang.Instance.GetNoteString("cmdFontSize", "Font size"));
                PToolbar.SetButtonTooltip(FormatType.FontColor,
                    PNLang.Instance.GetNoteString("cmdFontColor", "Font color"));
                PToolbar.SetButtonTooltip(FormatType.FontBold,
                    PNLang.Instance.GetNoteString("cmdBold", "Bold") + " (Ctrl+B)");
                PToolbar.SetButtonTooltip(FormatType.FontItalic,
                    PNLang.Instance.GetNoteString("cmdItalic", "Italic") + " (Ctrl+I)");
                PToolbar.SetButtonTooltip(FormatType.FontUnderline,
                    PNLang.Instance.GetNoteString("cmdUnderline", "Underline") + " (Ctrl+U)");
                PToolbar.SetButtonTooltip(FormatType.FontStrikethrough,
                    PNLang.Instance.GetNoteString("cmdStrikethrough", "Strikethrough") + " (Ctrl+K)");
                PToolbar.SetButtonTooltip(FormatType.Highlight,
                    PNLang.Instance.GetNoteString("cmdHighlight", "Highlight"));
                PToolbar.SetButtonTooltip(FormatType.Left,
                    PNLang.Instance.GetNoteString("cmdLeft", "Left") + " (Ctrl+L)");
                PToolbar.SetButtonTooltip(FormatType.Center,
                    PNLang.Instance.GetNoteString("cmdCenter", "Center") + " (Ctrl+E)");
                PToolbar.SetButtonTooltip(FormatType.Right,
                    PNLang.Instance.GetNoteString("cmdRight", "Right") + " (Ctrl+R)");
                PToolbar.SetButtonTooltip(FormatType.Bullets,
                    PNLang.Instance.GetNoteString("cmdBullets", "Bullets"));
                PHeader.SetButtonTooltip(HeaderButtonType.Hide, PNLang.Instance.GetNoteString("cmdHide", "Hide"));
                PHeader.SetButtonTooltip(HeaderButtonType.Delete, PNLang.Instance.GetNoteString("cmdDelete", "Delete"));
                PHeader.SetButtonTooltip(HeaderButtonType.Panel,
                    PNLang.Instance.GetNoteString("cmdPanel", "Put In Panel"));

                if (!_Note.FromDB)
                    if (_Mode == NewNoteMode.Duplication || _Mode == NewNoteMode.Diary)
                        PHeader.Title = Title = _Note.Name;
                    else
                        PHeader.Title = Title = PNLang.Instance.GetNoteString("def_caption", "Untitled");
                else
                    PHeader.Title = Title = _Note.Name;
                SetSendReceiveTooltip();
                PFooter.SetMarkButtonTooltip(MarkType.Password,
                    PNLang.Instance.GetControlText("cmdRemovePwrd", "Remove password"));
                applyMenusLanguage();
                ApplyTooltip();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyMenusLanguage()
        {
            try
            {
                foreach (var mi in ctmNote.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "note_menu");
                foreach (var mi in ctmEdit.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmBullets.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmFontColor.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmFontHighlight.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "edit_menu");
                foreach (var mi in ctmDrop.Items.OfType<MenuItem>())
                    PNLang.Instance.ApplyMenuItemLanguage(mi, "insert_menu");
                foreach (var ti in ctmNote.Items.OfType<MenuItem>())
                    applyNoteHotkeys(ti, HotkeyType.Note);
                foreach (var ti in ctmEdit.Items.OfType<MenuItem>())
                    applyNoteHotkeys(ti, HotkeyType.Edit);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void prepareControls()
        {
            try
            {
                PToolbar.FontComboDropDownClosed += cboFonts_DropDownClosed;
                _EditControl.EditControlSizeChanged += _EditControl_EditControlSizeChanged;
                ctmEdit.DataContext =
                    ctmNote.DataContext =
                        ctmBullets.DataContext =
                            ctmFontSize.DataContext =
                                ctmFontHighlight.DataContext =
                                    ctmFontColor.DataContext = ctmDrop.DataContext = PNSingleton.Instance.FontUser;

                loadLogFonts();

                applyMenusLanguage();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeControls()
        {
            try
            {
                _BackgroundDescriptor.RemoveValueChanged(this, OnBackgroundChanged);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkFontSize()
        {
            try
            {
                var fontSize = Edit.GetFontSize();
                foreach (var ti in ctmFontSize.Items.OfType<MenuItem>())
                {
                    ti.IsEnabled = !Edit.ReadOnly;
                    ti.IsChecked = ti.CommandParameter.ToString() == fontSize.ToString(PNRuntimes.Instance.CultureInvariant);
                }

                foreach (var ti in mnuFontSize.Items.OfType<MenuItem>())
                {
                    ti.IsEnabled = !Edit.ReadOnly;
                    ti.IsChecked = ti.CommandParameter.ToString() == fontSize.ToString(PNRuntimes.Instance.CultureInvariant);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkHighlight()
        {
            try
            {
                //var found = false;
                //var clr = Edit.SelectionBackColor;
                //var backColor = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                foreach (var mi in ctmFontHighlight.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    //mi.IsChecked = false;
                    //if (found || ((SolidColorBrush)mi.Background).Color != backColor) continue;
                    //mi.IsChecked = true;
                    //found = true;
                }
                //if (!found)
                //    ((MenuItem)ctmFontColor.Items[0]).IsChecked = true;
                //found = false;
                foreach (var mi in mnuHighlight.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    //mi.IsChecked = false;
                    //if (found || ((SolidColorBrush)mi.Background).Color != backColor) continue;
                    //mi.IsChecked = true;
                    //found = true;
                }
                //if (!found)
                //    ((MenuItem)mnuFontColor.Items[0]).IsChecked = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkBullets()
        {
            try
            {
                var bstyle = Edit.CurrentBulletStyle();
                var menuName = "";
                switch (bstyle)
                {
                    case 0:
                        menuName = "mnuNoBullets";
                        break;
                    case 1:
                        menuName = "mnuBullets";
                        break;
                    case 2:
                        menuName = "mnuArabicPoint";
                        break;
                    case 3:
                        menuName = "mnuArabicParts";
                        break;
                    case 4:
                        menuName = "mnuSmallLettersPoint";
                        break;
                    case 5:
                        menuName = "mnuSmallLettersPart";
                        break;
                    case 6:
                        menuName = "mnuBigLettersPoint";
                        break;
                    case 7:
                        menuName = "mnuBigLettersParts";
                        break;
                    case 8:
                        menuName = "mnuLatinSmall";
                        break;
                    case 9:
                        menuName = "mnuLatinBig";
                        break;
                }

                foreach (var mi in ctmBullets.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    mi.IsChecked = mi.CommandParameter.ToString() == menuName;
                }

                foreach (var mi in mnuBulletsNumbering.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    mi.IsChecked = mi.CommandParameter.ToString() == menuName;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkFontColor()
        {
            try
            {
                var found = false;
                var clr = Edit.SelectionColor;
                var fontColor = Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                foreach (var mi in ctmFontColor.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    mi.IsChecked = false;
                    if (found || ((SolidColorBrush)mi.Background).Color != fontColor) continue;
                    mi.IsChecked = true;
                    found = true;
                }

                if (!found)
                    ((MenuItem)ctmFontColor.Items[0]).IsChecked = true;
                found = false;
                foreach (var mi in mnuFontColor.Items.OfType<MenuItem>())
                {
                    mi.IsEnabled = !Edit.ReadOnly;
                    mi.IsChecked = false;
                    if (found || ((SolidColorBrush)mi.Background).Color != fontColor) continue;
                    mi.IsChecked = true;
                    found = true;
                }

                if (!found)
                    ((MenuItem)mnuFontColor.Items[0]).IsChecked = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Header and footer events

        private void PHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    var now = TimeSpan.FromTicks(DateTime.Now.Ticks);
            //    var diff = now - _ClickTime;
            //    _ClickTime = now;

            //    if (diff.TotalMilliseconds > SystemInformation.DoubleClickTime) return;

            //    e.Handled = true;
            //    if (PNRuntimes.Instance.Settings.Behavior.RollOnDblClick)
            //    {
            //        rollUnrollClick();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PNStatic.LogException(ex);
            //}
        }

        private void PHeader_HideDeleteButtonClicked(object sender, HeaderButtonClickedEventArgs e)
        {
            try
            {
                PNote note;
                switch (e.ButtonType)
                {
                    case HeaderButtonType.Delete:
                        note = PNCollections.Instance.Notes.Note(Handle);
                        if (note != null)
                        {
                            var type = PNNotesOperations.DeletionWarning(HotkeysStatic.LeftShiftDown(), 1,
                                note);
                            if (type != NoteDeleteType.None)
                            {
                                _DoNotCheckCanExecute = true;
                                if (PNNotesOperations.DeleteNote(type, note))
                                {
                                    if (!PNRuntimes.Instance.Settings.Behavior.HideFluently ||
                                        !(TryFindResource("FadeAway") is Storyboard sb))
                                    {
                                        note.Dialog = null;
                                        Close();
                                        if (note.Thumbnail && PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel)
                                            PNWindows.Instance.FormPanel.RemoveThumbnail(note);
                                    }
                                    else
                                    {
                                        sb.Begin();
                                    }
                                }
                            }
                        }

                        break;
                    case HeaderButtonType.Hide:
                        if (PNRuntimes.Instance.Settings.GeneralSettings.ChangeHideToDelete)
                        {
                            PHeader_HideDeleteButtonClicked(PHeader,
                                new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
                        }
                        else
                        {
                            note = PNCollections.Instance.Notes.Note(Handle);
                            if (note != null) ApplyHideNote(note);
                        }

                        break;
                    case HeaderButtonType.Panel:
                        SetThumbnail();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void PFooter_MarkButtonClicked(object sender, MarkButtonClickedEventArgs e)
        {
            try
            {
                switch (e.Buttontype)
                {
                    case MarkType.Schedule:
                        adjustScheduleClick();
                        break;
                    case MarkType.Change:
                        saveClick(false);
                        break;
                    case MarkType.Complete:
                        markAsCompleteClick();
                        break;
                    case MarkType.Encrypted:
                        scrambleClick();
                        break;
                    case MarkType.Mail:
                        break;
                    case MarkType.Password:
                        removePasswordClick();
                        break;
                    case MarkType.Pin:
                        if (PNRuntimes.Instance.Settings.Behavior.PinClickAction == PinClickAction.Toggle)
                        {
                            unpinClick();
                        }
                        else
                        {
                            var note = PNCollections.Instance.Notes.Note(Handle);
                            if (note != null)
                            {
                                _PinClass = new PinClass
                                {
                                    Class = note.PinClass,
                                    Pattern = PNStatic.CreatePinRegexPattern(note.PinText)
                                };
                                PNInterop.EnumWindowsProcDelegate enumProc = EnumWindowsProc;
                                PNInterop.EnumWindows(enumProc, 0);
                                if (!_PinClass.Hwnd.Equals(IntPtr.Zero)) PNInterop.BringWindowToTop(_PinClass.Hwnd);
                            }
                        }

                        break;
                    case MarkType.Priority:
                        toggleHighPriorityClick();
                        break;
                    case MarkType.Protection:
                        toggleProtectionModeClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void PFooter_FormatButtonClicked(object sender, FormatButtonClickedEventArgs e)
        {
            try
            {
                switch (e.Buttontype)
                {
                    case FormatType.Bullets:
                        if (Edit.ReadOnly) break;
                        checkBullets();
                        ctmBullets.IsOpen = true;
                        break;
                    case FormatType.FontSize:
                        if (Edit.ReadOnly) break;
                        checkFontSize();
                        ctmFontSize.IsOpen = true;
                        break;
                    case FormatType.FontColor:
                        if (Edit.ReadOnly) break;
                        checkFontColor();
                        ctmFontColor.IsOpen = true;
                        break;
                    case FormatType.Highlight:
                        if (Edit.ReadOnly) break;
                        checkHighlight();
                        ctmFontHighlight.IsOpen = true;
                        break;
                    case FormatType.FontFamily:
                        if (Edit.ReadOnly) break;
                        showFontComboBox();
                        break;
                    case FormatType.FontBold:
                        if (Edit.ReadOnly) break;
                        Edit.SetFontDecoration(REFDecorationMask.CFM_BOLD, REFDecorationStyle.CFE_BOLD);
                        break;
                    case FormatType.FontItalic:
                        if (Edit.ReadOnly) break;
                        Edit.SetFontDecoration(REFDecorationMask.CFM_ITALIC, REFDecorationStyle.CFE_ITALIC);
                        break;
                    case FormatType.FontUnderline:
                        if (Edit.ReadOnly) break;
                        Edit.SetFontDecoration(REFDecorationMask.CFM_UNDERLINE, REFDecorationStyle.CFE_UNDERLINE);
                        break;
                    case FormatType.FontStrikethrough:
                        if (Edit.ReadOnly) break;
                        Edit.SetFontDecoration(REFDecorationMask.CFM_STRIKEOUT, REFDecorationStyle.CFE_STRIKEOUT);
                        break;
                    case FormatType.Left:
                        if (Edit.ReadOnly) break;
                        Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                        break;
                    case FormatType.Right:
                        if (Edit.ReadOnly) break;
                        Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Right;
                        break;
                    case FormatType.Center:
                        if (Edit.ReadOnly) break;
                        Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void showFontComboBox()
        {
            try
            {
                var fontString = Edit.GetFontName();
                PToolbar.ShowFontsComboBox(fontString);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is ComboBox cbo)) return;
                if (!(cbo.SelectedItem is LOGFONT)) return;
                var lf = (LOGFONT)cbo.SelectedItem;
                if (Edit.GetFontName() == lf.lfFaceName) return;
                Edit.SetFontByName(lf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Edit events

        private void edit_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (InHotKey)
                {
                    InHotKey = false;
                    return;
                }
                if (!_Loaded && _Mode != NewNoteMode.Clipboard && _Mode != NewNoteMode.Duplication) return;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null && !note.Changed) setChangedSign(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_PNREActivatedByMouse(object sender, PNREActivatedByMouseEventArgs e)
        {
            try
            {
                if (_InAlarm) InAlarm = false;
                if (!Active)
                {
                    var index = Edit.GetCharIndexFromPosition(e.Position);
                    var text = Edit.Text;
                    if (index == text.Length - 1)
                    {
                        var width = Edit.GetTextWidth(index, index + 1);
                        if (width > 0)
                        {
                            var point = Edit.GetPositionFromCharIndex(text.Length - 1);
                            Edit.Select(e.Position.X > point.X + width / 4 ? text.Length : index, 0);
                        }
                        else
                        {
                            Edit.Select(index, 0);
                        }
                    }
                    else
                    {
                        Edit.Select(index, 0);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //do nothing - this may occur when user clicks on rich edit while note becomes hidden
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                var link = e.LinkText;
                if (e.LinkText.StartsWith("file:"))
                {
                    link = link.Remove(link.IndexOf("file:", StringComparison.Ordinal), "file:".Length);
                    if (!link.StartsWith(PNStrings.LINK_PFX))
                    {
                        try
                        {
                            var path = Path.GetFullPath(link);
                            if (!File.Exists(path) && !Directory.Exists(path))
                            {
                                WPFMessageBox.Show(
                                    PNLang.Instance.GetMessageText("file_not_exist",
                                        "The link is pointing to not existing file (directory)"),
                                    PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        catch (ArgumentException aex)
                        {
                            PNStatic.LogException(aex, false);
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        catch (SecurityException sex)
                        {
                            PNStatic.LogException(sex, false);
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("sec_ex_link",
                                    "You have no permissions to work with file"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        catch (NotSupportedException nsex)
                        {
                            PNStatic.LogException(nsex, false);
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        catch (PathTooLongException ptlex)
                        {
                            PNStatic.LogException(ptlex, false);
                            WPFMessageBox.Show(
                                PNLang.Instance.GetMessageText("inv_file_link", "Invalid link to file system object"),
                                PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        link = link.Substring(PNStrings.LINK_PFX.Length).Trim();
                        var arr = link.Split('|');
                        var note = PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == arr[1]);
                        if (note == null)
                        {
                            var message = PNLang.Instance.GetMessageText("link_not_exists",
                                "The linked note does not exists. Perhaps it has been moved to Recycle Bin or deleted completely.");
                            WPFMessageBox.Show(message, PNStrings.PROG_NAME);
                            return;
                        }

                        PNNotesOperations.ShowHideSpecificNote(note, true);
                        return;
                    }
                }
                else if (e.LinkText.StartsWith("mailto:"))
                {
                    var recipients = link.Remove(link.IndexOf("mailto:", StringComparison.Ordinal), "mailto:".Length);
                    var note = PNCollections.Instance.Notes.Note(Handle);
                    if (note == null) return;
                    PNNotesOperations.SendNoteAsText(note, recipients, link);
                    return;
                }

                PNStatic.LoadPage(link);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                //prevent RichEdit built-in Ctrl+I shortcut from being applied (it inserts TAB instead of applying italic style)
                var mod = HotkeysStatic.GetModifiers();
                var key = HotkeysStatic.GetKey();
                if (mod == HotkeyModifiers.ModControl && key == HotkeysStatic.VK_I)
                {
                    e.Handled = true;
                    return;
                }

                if (!PNRuntimes.Instance.Settings.GeneralSettings.AutomaticSmilies || Edit.SelectionStart <= 0) return;
                BitmapImage image = null;
                switch (e.KeyChar)
                {
                    case ')':
                        {
                            var c =
                                Edit.Text[Edit.SelectionStart - 1];
                            if (c == ':')
                                image = TryFindResource(
                                        "happy") as
                                    BitmapImage; //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "happy.png"));
                        }
                        break;
                    case '(':
                        {
                            var c =
                                Edit.Text[Edit.SelectionStart - 1];
                            if (c == ':')
                                image = TryFindResource(
                                        "sad") as
                                    BitmapImage; //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "sad.png"));
                        }
                        break;
                }

                if (image != null)
                {
                    Edit.SelectionStart--;
                    Edit.SelectionLength = 1;
                    insertImage(PNStatic.ImageToDrawingImage(image));
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                InHotKey = false;
                closeContextMenus();
                var mod = HotkeysStatic.GetModifiers();
                var key = HotkeysStatic.GetKey();
                if (mod == HotkeyModifiers.ModNone && key >= HotkeysStatic.VK_F1 && key <= HotkeysStatic.VK_F24 ||
                    mod != HotkeyModifiers.ModNone && key != 0)
                    if (key == HotkeysStatic.VK_F3)
                    {
                        if (!(mnuFindNext.Command is PNRoutedUICommand command)) return;
                        if (command.CanExecute(null, mnuFindNext))
                        {
                            command.Execute(null, mnuFindNext);
                            e.Handled = true;
                            return;
                        }
                    }


                if (mod == HotkeyModifiers.ModControl)
                    switch (key)
                    {
                        case HotkeysStatic.VK_C:
                            var note = PNCollections.Instance.Notes.Note(Handle);
                            if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                            {
                                if (note == null) return;
                                if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                Edit.Copy();
                            }

                            return;
                        case HotkeysStatic.VK_S:
                            {
                                if (mnuSave.Command is PNRoutedUICommand command)
                                {
                                    if (command.CanExecute(null, mnuSave))
                                    {
                                        command.Execute(null, mnuSave);
                                    }
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_P:
                            {
                                if (mnuPrint.Command is PNRoutedUICommand command)
                                {
                                    if (command.CanExecute(null, mnuPrint))
                                    {
                                        command.Execute(null, mnuPrint);
                                    }
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_F:
                            {
                                if (!(mnuFind.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuFind))
                                {
                                    command.Execute(null, mnuFind);
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_H:
                            {
                                if (!(mnuReplace.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuReplace))
                                {
                                    command.Execute(null, mnuReplace);
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_B:
                            {
                                if (!(mnuBold.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuBold))
                                {
                                    command.Execute(null, mnuBold);
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_I:
                            {
                                if (!(mnuItalic.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuItalic))
                                {
                                    command.Execute(null, mnuItalic);
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_U:
                            {
                                if (!(mnuUnderline.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuUnderline))
                                {
                                    command.Execute(null, mnuUnderline);
                                }
                            }
                            e.Handled = true;
                            return;
                        case HotkeysStatic.VK_K:
                            {
                                if (!(mnuStrikethrough.Command is PNRoutedUICommand command)) return;
                                if (command.CanExecute(null, mnuStrikethrough))
                                {
                                    command.Execute(null, mnuStrikethrough);
                                }
                            }
                            e.Handled = true;
                            return;
                    }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent("FileDrop")) return;
                var files = (string[])e.Data.GetData("FileDrop");
                if (files.Length <= 0) return;
                if (files.Length > 1)
                {
                    var message = PNLang.Instance.GetMessageText("many_files_dropped",
                        "Only one file may be dropped onto note");
                    WPFMessageBox.Show(this, message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Effect = DragDropEffects.None;
                    return;
                }

                _InDrop = true;
                _DropCase = DropCase.None;
                ctmDrop.IsOpen = true;
                while (_InDrop) Application.DoEvents();
                switch (_DropCase)
                {
                    case DropCase.Content:
                        if (Path.GetExtension(files[0])
                            .In(".bmp", ".png", ".gif", ".jpg", ".jpeg", ".ico", ".emf", ".wmf"))
                            using (var image = Image.FromFile(files[0]))
                            {
                                insertImage(image);
                            }
                        else
                            using (var sr = new StreamReader(files[0]))
                            {
                                InHotKey = false;
                                Edit.SelectedText = sr.ReadToEnd();
                            }

                        e.Effect = DragDropEffects.None;
                        break;
                    case DropCase.Object:
                        //just insert object
                        InHotKey = false;
                        break;
                    case DropCase.Link:
                        InHotKey = false;
                        var link = files[0];
                        if (link.Contains(' '))
                            link = " <file:" + link + "> ";
                        else
                            link = " file:" + link + " ";
                        Edit.SelectedText = link;
                        e.Effect = DragDropEffects.None;
                        break;
                    default:
                        e.Effect = DragDropEffects.None;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                e.Effect = DragDropEffects.None;
            }
        }

        private void edit_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null || e == null || Edit == null) return;

                if (Edit.Height == e.NewRectangle.Height)
                {
                    note.AutoHeight = Height;
                    return;
                }

                if (!_SizeChangedFirstTime) return;

                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins ||
                    !PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight ||
                    _InResize || _InRoll || _InDock || note.Rolled) return;

                var offset = e.NewRectangle.Height - Edit.Height;
                var tempHeight = Height;
                if (tempHeight + offset >= MIN_HEIGHT)
                    tempHeight += offset;
                else
                    tempHeight = MIN_HEIGHT;
                if (note.DockStatus == DockStatus.None)
                    Height = tempHeight;
                note.AutoHeight = tempHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void edit_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            ctmEdit.IsOpen = true;
        }

        private void edit_GotFocus(object sender, EventArgs e)
        {
            try
            {
                activateWindow();
                PNInterop.BringWindowToTop(_EditControl.WinForm.Handle);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _EditControl_EditControlSizeChanged(object sender, EditControlSizeChangedEventArgs e)
        {
            try
            {
                if (_SizeChangedFirstTime) return;
                _SizeChangedFirstTime = true;
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null || e == null || Edit == null) return;

                if (!PNRuntimes.Instance.Settings.GeneralSettings.AutoHeight ||
                    PNRuntimes.Instance.Settings.GeneralSettings.UseSkins) return;
                //var offset = e.NewRectangle.Height - edit.Height;
                //if (tempHeight + offset >= MIN_HEIGHT)
                //{
                //    tempHeight += offset;
                //}
                //else
                //{
                //    tempHeight = MIN_HEIGHT;
                //}
                double tempHeight = getEditContentHeight() + (int)PFooter.ActualHeight + (int)PHeader.ActualHeight;
                if (note.DockStatus == DockStatus.None && !note.Rolled)
                    Height = tempHeight;
                note.AutoHeight = tempHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Note menu clicks

        private void ctmEdit_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;

                PNMenus.SetEnabledByHiddenMenus(ctmEdit);

                checkFontSize();
                checkFontColor();
                checkBullets();
                checkHighlight();

                if (Directory.Exists(PNPaths.Instance.DictDir))
                    createDictMenu();
                _PostPluginsMenus.Clear();
                if (PNCollections.Instance.ActivePostPlugins.Count > 0)
                {
                    createPostPluginsMenu(mnuInsertPost);
                }

                if (Edit.SelectionLength > 0)
                {
                    createSearchProvidersMenu();
                    if (PNCollections.Instance.ActivePostPlugins.Count > 0)
                    {
                        createPostPluginsMenu(mnuPostOn);
                    }
                }

                var spaces = Edit.GetParagraphSpacing();
                if (spaces[REParaSpace.Before])
                {
                    mnuAddSpaceBefore.Visibility = Visibility.Collapsed;
                    mnuRemoveSpaceBefore.Visibility = Visibility.Visible;
                }
                else
                {
                    mnuAddSpaceBefore.Visibility = Visibility.Visible;
                    mnuRemoveSpaceBefore.Visibility = Visibility.Collapsed;
                }

                if (spaces[REParaSpace.After])
                {
                    mnuRemoveSpaceAfter.Visibility = Visibility.Visible;
                    mnuAddSpaceAfter.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mnuRemoveSpaceAfter.Visibility = Visibility.Collapsed;
                    mnuAddSpaceAfter.Visibility = Visibility.Visible;
                }

                mnuSpace10.IsChecked = mnuSpace15.IsChecked = mnuSpace20.IsChecked = mnuSpace30.IsChecked = false;
                var lineSpacing = Edit.GetLineSpacing();
                switch (lineSpacing)
                {
                    case RELineSpacing.Single:
                        mnuSpace10.IsChecked = true;
                        break;
                    case RELineSpacing.OneAndHalf:
                        mnuSpace15.IsChecked = true;
                        break;
                    case RELineSpacing.Double:
                        mnuSpace20.IsChecked = true;
                        break;
                    case RELineSpacing.Triple:
                        mnuSpace30.IsChecked = true;
                        break;
                }

                var dec = Edit.GetFontDecoration();
                mnuBold.IsChecked = (dec & REFDecorationStyle.CFE_BOLD) == REFDecorationStyle.CFE_BOLD;
                mnuItalic.IsChecked = (dec & REFDecorationStyle.CFE_ITALIC) == REFDecorationStyle.CFE_ITALIC;
                mnuUnderline.IsChecked = (dec & REFDecorationStyle.CFE_UNDERLINE) == REFDecorationStyle.CFE_UNDERLINE;
                mnuStrikethrough.IsChecked =
                    (dec & REFDecorationStyle.CFE_STRIKEOUT) == REFDecorationStyle.CFE_STRIKEOUT;
                mnuSubscript.IsChecked = (dec & REFDecorationStyle.CFE_SUBSCRIPT) == REFDecorationStyle.CFE_SUBSCRIPT;
                mnuSuperscript.IsChecked = (dec & REFDecorationStyle.CFE_SUPERSCRIPT) ==
                                           REFDecorationStyle.CFE_SUPERSCRIPT;
                mnuAlignLeft.IsChecked = Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Left;
                mnuAlignCenter.IsChecked = Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Center;
                mnuAlignRight.IsChecked = Edit.SelectionAlignment == System.Windows.Forms.HorizontalAlignment.Right;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createSearchProvidersMenu()
        {
            try
            {
                var count = mnuSearchWeb.Items.Count;
                if (count > 0)
                    for (var i = count - 1; i >= 0; i--)
                    {
                        if (mnuSearchWeb.Items[i] is MenuItem mi) mi.Click -= menu_Click;
                        mnuSearchWeb.Items.RemoveAt(i);
                    }

                foreach (
                    var ti in
                    PNCollections.Instance.SearchProviders.Select(psp =>
                        new MenuItem { Header = psp.Name, Tag = psp.QueryString })
                )
                {
                    ti.Click += menu_Click;
                    mnuSearchWeb.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        //private void mnuSave_Click(object sender, RoutedEventArgs e)
        //{
        //    saveClick(false);
        //}

        //private void mnuRename_Click(object sender, RoutedEventArgs e)
        //{
        //    saveClick(true);
        //}

        private void saveAsTextClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null) PNNotesOperations.SaveNoteAsTextFile(note, this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreFromBackupClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null) PNNotesOperations.LoadBackupCopy(note, this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void duplicateNoteClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null) PNWindows.Instance.FormMain.DuplicateNote(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveAsShortcutClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.SaveAsShortcut(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void printClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                Edit.Print(note.Name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void adjustAppearanceClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var dlgAdjustAppearance = new WndAdjustAppearance(note) { Owner = this };
                dlgAdjustAppearance.NoteAppearanceAdjusted += dlgAdjustAppearance_NoteAppearanceAdjusted;
                var result = dlgAdjustAppearance.ShowDialog();
                if (!result.HasValue || !result.Value)
                    dlgAdjustAppearance.NoteAppearanceAdjusted -= dlgAdjustAppearance_NoteAppearanceAdjusted;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgAdjustAppearance_NoteAppearanceAdjusted(object sender, NoteAppearanceAdjustedEventArgs e)
        {
            try
            {
                if (sender is WndAdjustAppearance dlgAdjustAppearance)
                    dlgAdjustAppearance.NoteAppearanceAdjusted -= dlgAdjustAppearance_NoteAppearanceAdjusted;
                ApplyAppearanceAdjustment(e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void adjustScheduleClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle) ?? _Note;
                if (note == null) return;
                PNNotesOperations.AdjustNoteSchedule(note, this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void hideNoteClick()
        {
            PHeader_HideDeleteButtonClicked(PHeader, new HeaderButtonClickedEventArgs(HeaderButtonType.Hide));
        }

        private void deleteNoteClick()
        {
            PHeader_HideDeleteButtonClicked(PHeader, new HeaderButtonClickedEventArgs(HeaderButtonType.Delete));
        }

        private void dockNoneClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                    if (note.DockStatus != DockStatus.None)
                        UndockNote(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dockLefClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                    if (note.DockStatus != DockStatus.Left)
                        DockNote(note, DockStatus.Left, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dockTopClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                    if (note.DockStatus != DockStatus.Top)
                        DockNote(note, DockStatus.Top, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dockRightClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                    if (note.DockStatus != DockStatus.Right)
                        DockNote(note, DockStatus.Right, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dockBottomClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                    if (note.DockStatus != DockStatus.Bottom)
                        DockNote(note, DockStatus.Bottom, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sendAsTextClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                PNNotesOperations.SendNoteAsText(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sendAsAttachment_Click()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                if (!File.Exists(path)) return;
                var files = new List<string> { path };
                PNNotesOperations.SendNotesAsAttachments(files);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sendZipClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                    if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                        return;
                var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                if (!File.Exists(path)) return;
                var files = new List<string> { path };
                var dzip = new WndArchName(files) { Owner = this };
                dzip.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toPanelClick()
        {
            SetThumbnail();
        }

        private void addContactClick()
        {
            try
            {
                var newId = 0;
                if (PNCollections.Instance.Contacts.Count > 0)
                    newId = PNCollections.Instance.Contacts.Max(c => c.Id) + 1;
                var dlgContact = new WndContacts(newId, PNCollections.Instance.ContactGroups) { Owner = this };
                dlgContact.ContactChanged += dlgContact_ContactChanged;
                var showDialog = dlgContact.ShowDialog();
                if (showDialog != null && !showDialog.Value) dlgContact.ContactChanged -= dlgContact_ContactChanged;
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
                if (sender is WndContacts dc) dc.ContactChanged -= dlgContact_ContactChanged;
                if (e.Mode == AddEditMode.Add)
                {
                    if (PNCollections.Instance.Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }

                    PNCollections.Instance.Contacts.Add(e.Contact);
                }
                else
                {
                    var c = PNCollections.Instance.Contacts.FirstOrDefault(con => con.Id == e.Contact.Id);
                    if (c != null)
                    {
                        c.Name = e.Contact.Name;
                        c.ComputerName = e.Contact.ComputerName;
                        c.IpAddress = e.Contact.IpAddress;
                        c.UseComputerName = e.Contact.UseComputerName;
                        c.GroupId = e.Contact.GroupId;
                    }
                }

                if (PNWindows.Instance.FormSettings != null)
                    PNWindows.Instance.FormSettings.ContactAction(e.Contact, e.Mode);
                PNData.SaveContacts();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addContactsGroupClick()
        {
            try
            {
                var newId = 0;
                if (PNCollections.Instance.ContactGroups.Count > 0)
                    newId = PNCollections.Instance.ContactGroups.Max(g => g.Id) + 1;
                var dlgContactGroup = new WndGroups(newId) { Owner = this };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
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
                if (sender is WndGroups dg) dg.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                if (e.Mode == AddEditMode.Add)
                {
                    if (PNCollections.Instance.ContactGroups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }

                    PNCollections.Instance.ContactGroups.Add(e.Group);
                }
                else
                {
                    var g = PNCollections.Instance.ContactGroups.FirstOrDefault(gr => gr.Id == e.Group.Id);
                    if (g != null) g.Name = e.Group.Name;
                }

                if (PNWindows.Instance.FormSettings != null)
                    PNWindows.Instance.FormSettings.ContactGroupAction(e.Group, e.Mode);
                PNData.SaveContactGroups();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void selectContactClick()
        {
            try
            {
                var dlgSelectC = new WndSelectContacts { Owner = this };
                dlgSelectC.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectC.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgSelectC.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgSelectCOrG_ContactsSelected(object sender, ContactsSelectedEventArgs e)
        {
            try
            {
                if (sender is WndSelectContacts d)
                {
                    d.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
                else
                {
                    if (sender is WndSelectGroups gd) gd.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }

                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (e.Contacts.All(c => c == null)) return;
                var clientRunner = new PNWCFClientRunner();
                clientRunner.NotesSendNotification += clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete += clientRunner_NotesSendComplete;
                Task.Factory.StartNew(
                    () => clientRunner.SendNotesToMultipleContacts(new[] { note }, e.Contacts.Where(c => c != null)));
                PNWindows.Instance.FormMain.ShowSendProgressBaloon();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void selectContactsGroupClick()
        {
            try
            {
                var dlgSelectG = new WndSelectGroups { Owner = this };
                dlgSelectG.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectG.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgSelectG.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void replyClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var cn = (PNCollections.Instance.Contacts.FirstOrDefault(c => c.Name == note.ReceivedFrom) ??
                          PNCollections.Instance.Contacts.FirstOrDefault(c => c.ComputerName == note.ReceivedFrom)) ??
                         PNCollections.Instance.Contacts.FirstOrDefault(c => c.IpAddress == note.ReceivedIp);
                if (cn == null) return;

                var clientRunner = new PNWCFClientRunner();
                clientRunner.NotesSendNotification += clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete += clientRunner_NotesSendComplete;
                Task.Factory.StartNew(() => clientRunner.SendNotesToSingleContact(new List<PNote> { note }, cn));
                PNWindows.Instance.FormMain.ShowSendProgressBaloon();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clientRunner_NotesSendComplete(object sender, EventArgs e)
        {
            try
            {
                if (sender is PNWCFClientRunner clientRunner)
                {
                    clientRunner.NotesSendNotification -= clientRunner_NotesSendNotification;
                    clientRunner.NotesSendComplete -= clientRunner_NotesSendComplete;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clientRunner_NotesSendNotification(object sender, NotesSendNotificationEventArgs e)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Network.NoNotificationOnSend)
                    return;
                PNWindows.Instance.FormMain.ShowSentNotification(e.Notes.Select(n => n.Name), e.SentTo, e.Result);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void exportOutlookNoteClick()
        {
            try
            {
                PNOffice.ExportToOutlookNote(Edit.Text);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tagsClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var d = new WndExchangeLists(note.Id, ExchangeLists.Tags) { Owner = this };
                d.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void manageLinksClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var d = new WndExchangeLists(note.Id, ExchangeLists.LinkedNotes) { Owner = this };
                d.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addToFavoritesClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null && !note.Favorite)
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, true, null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeFromFavoritesClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null && note.Favorite)
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, false, null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void onTopClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Topmost;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Topmost, value, null);
                Topmost = value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toggleHighPriorityClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Priority;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Priority, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Priority, value);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toggleProtectionModeClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Protected;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Protection, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Protection, value);

                Edit.ReadOnly = value || note.Scrambled;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setPasswordClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                {
                    var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                    var pwrdCrweate = new WndPasswordCreate(text)
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    pwrdCrweate.PasswordChanged += pwrdCrweate_PasswordChanged;
                    var showDialog = pwrdCrweate.ShowDialog();
                    if (showDialog != null && !showDialog.Value)
                        pwrdCrweate.PasswordChanged -= pwrdCrweate_PasswordChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pwrdCrweate_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            try
            {
                if (sender is WndPasswordCreate pwrdCrweate) pwrdCrweate.PasswordChanged -= pwrdCrweate_PasswordChanged;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, true, e.NewPassword);
                PFooter.SetMarkButtonVisibility(MarkType.Password, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removePasswordClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.DeleteNote, text,
                    note.PasswordString)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                pwrdDelete.PasswordDeleted += pwrdDelete_PasswordDeleted;
                var showDialog = pwrdDelete.ShowDialog();
                if (showDialog != null && !showDialog.Value) pwrdDelete.PasswordDeleted -= pwrdDelete_PasswordDeleted;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pwrdDelete_PasswordDeleted(object sender, EventArgs e)
        {
            try
            {
                if (sender is WndPasswordDelete pwrdDelete) pwrdDelete.PasswordDeleted -= pwrdDelete_PasswordDeleted;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, false, null);
                PFooter.SetMarkButtonVisibility(MarkType.Password, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void markAsCompleteClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var value = !note.Completed;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Complete, value, null);
                PFooter.SetMarkButtonVisibility(MarkType.Complete, value);
                if (PNRuntimes.Instance.Settings.Behavior.HideCompleted) ApplyHideNote(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void rollUnrollClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null || note.DockStatus != DockStatus.None) return;
                if (!note.Rolled)
                {
                    rollUnrollNote(note, true);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, true, null);
                }
                else
                {
                    rollUnrollNote(note, false);
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Roll, false, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pinClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var dlgPin = new WndPin(note.Name) { Owner = this };
                dlgPin.PinnedWindowChanged += dlgPin_PinnedWindowChanged;
                var showDialog = dlgPin.ShowDialog();
                if (showDialog != null && !showDialog.Value) dlgPin.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgPin_PinnedWindowChanged(object sender, PinnedWindowChangedEventArgs e)
        {
            try
            {
                if (sender is WndPin d) d.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                note.PinText = e.PinText;
                note.PinClass = e.PinClass;
                note.Pinned = true;
                PNNotesOperations.SaveNotePin(note);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void unpinClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                note.PinText = "";
                note.PinClass = "";
                note.Pinned = false;
                PNNotesOperations.SaveNotePin(note);
                PFooter.SetMarkButtonVisibility(MarkType.Pin, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void scrambleClick()
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (!PNNotesOperations.ApplyScramble(note, this))
                    return;
                var value = !note.Scrambled;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Scrambled, value, null);
                Edit.ReadOnly = value || note.Protected;
                PFooter.SetMarkButtonVisibility(MarkType.Encrypted, note.Scrambled);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ctmDrop_Closed(object sender, RoutedEventArgs e)
        {
            _InDrop = false;
        }

        private void ctmNote_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                createLinksMenu(note);
                clearContactsMenu();
                if (PNRuntimes.Instance.Settings.Network.EnableExchange &&
                    !PNRuntimes.Instance.Settings.Network.NoContactsInContextMenu) createContactsMenu();

                PNMenus.SetEnabledByHiddenMenus(ctmNote);

                if (PNRuntimes.Instance.Settings.Network.EnableExchange)
                {
                    if ((note.SentReceived & SendReceiveStatus.Received) ==
                        SendReceiveStatus.Received)
                    {
                        mnuReply.Header = PNLang.Instance.GetMenuText("note_menu", "mnuReply", "Reply");
                        mnuReply.Header += " (" + note.ReceivedFrom + ")";
                    }
                    else
                    {
                        mnuReply.Header = PNLang.Instance.GetMenuText("note_menu", "mnuReply", "Reply");
                    }
                }

                mnuAddToFavorites.Visibility = !note.Favorite &&
                                               !PNCollections.Instance.HiddenMenus.Any(
                                                   hm => hm.Type == MenuType.Note && hm.Name == mnuAddToFavorites.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                mnuRemoveFromFavorites.Visibility = note.Favorite &&
                                                    !PNCollections.Instance.HiddenMenus.Any(
                                                        hm =>
                                                            hm.Type == MenuType.Note &&
                                                            hm.Name == mnuRemoveFromFavorites.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuSetPassword.Visibility = !(note.PasswordString.Length > 0) &&
                                            !PNCollections.Instance.HiddenMenus.Any(
                                                hm => hm.Type == MenuType.Note && hm.Name == mnuSetPassword.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuRemovePassword.Visibility = note.PasswordString.Length > 0 &&
                                               !PNCollections.Instance.HiddenMenus.Any(
                                                   hm => hm.Type == MenuType.Note && hm.Name == mnuRemovePassword.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuPin.Visibility = !note.Pinned &&
                                    !PNCollections.Instance.HiddenMenus.Any(
                                        hm => hm.Type == MenuType.Note && hm.Name == mnuPin.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuUnpin.Visibility = note.Pinned &&
                                      !PNCollections.Instance.HiddenMenus.Any(
                                          hm => hm.Type == MenuType.Note && hm.Name == mnuUnpin.Name)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                mnuOnTop.IsChecked = note.Topmost;
                mnuToggleHighPriority.IsChecked = note.Priority;
                mnuToggleProtectionMode.IsChecked = note.Protected;
                mnuMarkAsComplete.IsChecked = note.Completed;
                if (PNCollections.Instance.ActivePostPlugins.Count > 0)
                {
                    _PostPluginsMenus.Clear();
                    if (Edit.TextLength > 0)
                    {
                        createPostPluginsMenu(mnuPostNote);
                    }
                    createPostPluginsMenu(mnuReplacePost);
                }

                if (!note.Scrambled)
                {
                    mnuScramble.Header = PNLang.Instance.GetMenuText("note_menu", "mnuScramble", "Encrypt text");
                    mnuScramble.Icon = new System.Windows.Controls.Image
                    {
                        Source =
                            TryFindResource(
                                    "scramble") as
                                BitmapImage //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "scramble.png"))
                    };
                }
                else
                {
                    mnuScramble.Header = PNLang.Instance.GetMenuText("note_menu", "mnuUnscramble", "Decrypt text");
                    mnuScramble.Icon = new System.Windows.Controls.Image
                    {
                        Source =
                            TryFindResource(
                                    "unscramble") as
                                BitmapImage //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "unscramble.png"))
                    };
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createPostPluginsMenu(MenuItem menu)
        {
            try
            {
                var count = menu.Items.Count;
                if (count > 0)
                    for (var i = count - 1; i >= 0; i--)
                    {
                        if (menu.Items[i] is MenuItem mi) mi.Click -= menu_Click;
                        menu.Items.RemoveAt(i);
                    }

                foreach (var p in PNPlugins.Instance.SocialPlugins)
                {
                    if (!PNCollections.Instance.ActivePostPlugins.Contains(p.Name)) continue;
                    MenuItem mi = null;
                    switch (menu.Name)
                    {
                        case "mnuPostOn":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuPostPartial);
                            mi = new MenuItem
                            {
                                Header = p.MenuPostPartial.Text,
                                Icon =
                                    new System.Windows.Controls.Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuPostPartial.Image)
                                    },
                                Tag = p.Name + menu.Name
                            };
                            break;
                        case "mnuPostNote":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuPostFull);
                            mi = new MenuItem
                            {
                                Header = p.MenuPostFull.Text,
                                Icon =
                                    new System.Windows.Controls.Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuPostFull.Image)
                                    },
                                Tag = p.Name + menu.Name
                            };
                            break;
                        case "mnuReplacePost":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuGetFull);
                            mi = new MenuItem
                            {
                                Header = p.MenuGetFull.Text,
                                Icon =
                                    new System.Windows.Controls.Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuGetFull.Image)
                                    },
                                Tag = p.Name + menu.Name,
                                IsEnabled = !Edit.ReadOnly
                            };
                            break;
                        case "mnuInsertPost":
                            _PostPluginsMenus.Add(p.Name + menu.Name, p.MenuGetPartial);
                            mi = new MenuItem
                            {
                                Header = p.MenuGetPartial.Text,
                                Icon =
                                    new System.Windows.Controls.Image
                                    {
                                        Source = PNStatic.ImageFromDrawingImage(p.MenuGetPartial.Image)
                                    },
                                Tag = p.Name + menu.Name,
                                IsEnabled = !Edit.ReadOnly
                            };
                            mi.IsEnabled = !Edit.ReadOnly;
                            break;
                    }

                    if (mi == null) continue;
                    mi.Click += postPluginMenu_Click;
                    menu.Items.Add(mi);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void postPluginMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem menu)) return;
                var tag = Convert.ToString(menu.Tag);
                if (_PostPluginsMenus.Keys.All(k => k != tag)) return;
                var pmenu = _PostPluginsMenus[tag];
                if (pmenu == null) return;
                pmenu.PerformClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createLinksMenu(PNote note)
        {
            try
            {
                var count = mnuLinked.Items.Count;
                if (count > 1)
                    for (var i = count - 1; i > 0; i--)
                    {
                        if (mnuLinked.Items[i] is MenuItem menu) menu.Click -= menu_Click;
                        mnuLinked.Items.RemoveAt(i);
                    }

                if (note.LinkedNotes.Count > 0)
                {
                    mnuLinked.Items.Add(new Separator());
                    foreach (var l in note.LinkedNotes)
                    {
                        var n = PNCollections.Instance.Notes.Note(l);
                        if (n != null)
                        {
                            var ti = new MenuItem { Header = n.Name, Tag = l };
                            ti.Click += menu_Click;
                            mnuLinked.Items.Add(ti);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clearContactsMenu()
        {
            try
            {
                var count = mnuSendNetwork.Items.Count;
                if (count <= 5) return;
                for (var i = count - 3; i > 2; i--)
                {
                    var menu = mnuSendNetwork.Items[i] as MenuItem;
                    if (menu != null && menu.Items.Count == 0)
                    {
                        menu.Click -= contactMenu_Click;
                    }
                    else
                    {
                        if (menu != null)
                            foreach (var mi in menu.Items.OfType<MenuItem>())
                                mi.Click -= contactMenu_Click;
                    }

                    mnuSendNetwork.Items.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createContactsMenu()
        {
            try
            {
                var index = 3;
                // all contacts from group (None)
                var contactsGroup = PNCollections.Instance.Contacts.Where(c => c.GroupId == -1);
                foreach (var mi in contactsGroup.Select(c => new MenuItem
                {
                    Header = c.Name,
                    Tag = c.Id,
                    Icon = new System.Windows.Controls.Image
                    {
                        Source = TryFindResource("contact") as BitmapImage
                    }
                }))
                {
                    mi.Click += contactMenu_Click;
                    mnuSendNetwork.Items.Insert(index++, mi);
                }

                // get all other groups
                var groups = PNCollections.Instance.ContactGroups.Where(g => g.Id != -1);
                foreach (var pgroup in groups)
                {
                    var mgi = new MenuItem
                    {
                        Header = pgroup.Name,
                        IsEnabled = false,
                        Icon = new System.Windows.Controls.Image
                        {
                            Source =
                                TryFindResource(
                                        "group") as
                                    BitmapImage //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"))
                        }
                    };
                    mnuSendNetwork.Items.Insert(index++, mgi);
                    var pg = pgroup;
                    contactsGroup = PNCollections.Instance.Contacts.Where(c => c.GroupId == pg.Id);
                    foreach (var mi in contactsGroup.Select(c => new MenuItem
                    {
                        Header = c.Name,
                        Tag = c.Id,
                        Icon = new System.Windows.Controls.Image
                        {
                            Source =
                                TryFindResource(
                                        "contact") as
                                    BitmapImage //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"))
                        }
                    }))
                    {
                        mi.Click += contactMenu_Click;
                        mgi.Items.Add(mi);
                        mgi.IsEnabled = true;
                    }
                }

                if (index > 3) mnuSendNetwork.Items.Insert(index, new Separator());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void contactMenu_Click(object sender, EventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (!(sender is MenuItem mi)) return;
                var cn = PNCollections.Instance.Contacts.FirstOrDefault(c => c.Id == (int)mi.Tag);
                if (cn == null) return;

                var clientRunner = new PNWCFClientRunner();
                clientRunner.NotesSendNotification += clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete += clientRunner_NotesSendComplete;
                Task.Factory.StartNew(() => clientRunner.SendNotesToSingleContact(new List<PNote> { note }, cn));
                PNWindows.Instance.FormMain.ShowSendProgressBaloon();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void menu_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                if (!(ti.Parent is MenuItem parent)) return;
                switch (parent.Name)
                {
                    case "mnuLinked":
                        var id = (string)ti.Tag;
                        var note = PNCollections.Instance.Notes.Note(id);
                        if (note != null) PNNotesOperations.ShowHideSpecificNote(note, true);
                        break;
                    case "mnuSearchWeb":
                        var query = (string)ti.Tag;
                        query += Edit.SelectedText;
                        PNStatic.LoadPage(query);
                        break;
                }
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

        #endregion

        #region Edit menu clicks

        private void undoClick()
        {
            InHotKey = false;
            if (Edit.CanUndo) Edit.Undo();
        }

        private void redoClick()
        {
            InHotKey = false;
            if (Edit.CanRedo) Edit.Redo();
        }

        private void cutClick()
        {
            InHotKey = false;
            Edit.Cut();
        }

        private void copyClick()
        {
            Edit.Copy();
        }

        private void pasteClick()
        {
            InHotKey = false;
            Edit.Paste();
        }

        private void copyPlainClick()
        {
            Clipboard.SetText(Edit.SelectedText);
        }

        private void pastePlainClick()
        {
            InHotKey = false;
            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            Edit.SelectedText = text;
        }


        private void toUpperClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                if (Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.Object) ||
                    Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                        return;
                }

                var start = Edit.SelectionStart;
                var length = Edit.SelectionLength;
                Edit.SelectedText = Edit.SelectedText.ToUpper();
                Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toLowerClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                if (Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.Object) ||
                    Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        MessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                        return;
                }

                var start = Edit.SelectionStart;
                var length = Edit.SelectionLength;
                Edit.SelectedText = Edit.SelectedText.ToLower();
                Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void capSentClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var index = 0;
                char[] delimiters = { '.' };
                var start = Edit.SelectionStart;
                var length = Edit.SelectionLength;
                var position = start;
                var selectedText = Edit.SelectedText;
                var tokens = selectedText.Split(delimiters);
                if (start > 0 && !delimiters.Contains(Edit.Text[start - 1]))
                {
                    index = 1;
                    position += tokens[0].Length;
                }

                for (; index < tokens.Length; index++)
                {
                    position = Edit.Text.IndexOf(tokens[index], position, StringComparison.Ordinal);
                    if (string.IsNullOrWhiteSpace(tokens[index]))
                    {
                        position += tokens[index].Length;
                        continue;
                    }

                    if (tokens[index].Length > 1)
                    {
                        var firstIndex = 0;
                        for (var i = 0; i < tokens[index].Length; i++)
                        {
                            if (!char.IsWhiteSpace(tokens[index][i]))
                                break;
                            firstIndex++;
                        }

                        tokens[index] = tokens[index].Substring(0, firstIndex) +
                                        tokens[index].Substring(firstIndex, 1).ToUpper() +
                                        tokens[index].Substring(firstIndex + 1);
                    }
                    else
                    {
                        tokens[index] = tokens[index].ToUpper();
                    }

                    Edit.Select(position, tokens[index].Length);
                    Edit.SelectedText = tokens[index];
                    position += tokens[index].Length;
                }

                Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void capWordClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var index = 0;
                var delimiters = Spellchecking.PNRE_DELIMITERS.ToCharArray();
                var start = Edit.SelectionStart;
                var length = Edit.SelectionLength;
                var position = start;
                var selectedText = Edit.SelectedText;
                var tokens = selectedText.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (start > 0 && !delimiters.Contains(Edit.Text[start - 1]))
                {
                    index = 1;
                    position += tokens[0].Length;
                }

                for (; index < tokens.Length; index++)
                {
                    position = Edit.Text.IndexOf(tokens[index], position, StringComparison.Ordinal);
                    if (tokens[index].Length > 1)
                        tokens[index] = tokens[index].Substring(0, 1).ToUpper() + tokens[index].Substring(1);
                    else
                        tokens[index] = tokens[index].ToUpper();
                    Edit.Select(position, tokens[index].Length);
                    Edit.SelectedText = tokens[index];
                    position += tokens[index].Length;
                }

                Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toggleCaseClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                if (Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.Object) ||
                    Edit.SelectionType.HasFlag(RichTextBoxSelectionTypes.MultiObject))
                {
                    var message = PNLang.Instance.GetMessageText("selection_objects_warning",
                        "Selected text contains one or more non-text objects (pictures etc). If you continue, these objects will be deleted. Continue anyway?");
                    if (
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) ==
                        MessageBoxResult.No)
                        return;
                }

                var start = Edit.SelectionStart;
                var length = Edit.SelectionLength;
                var arr = Edit.SelectedText.ToCharArray();
                for (var i = 0; i < arr.Length; i++)
                    if (char.IsLower(arr[i]))
                        arr[i] = char.ToUpper(arr[i]);
                    else
                        arr[i] = char.ToLower(arr[i]);
                Edit.SelectedText = new string(arr);
                Edit.Select(start, length);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fontClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontFamily));
        }

        private void boldClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontBold));
        }

        private void italicClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontItalic));
        }

        private void underlineClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontUnderline));
        }

        private void strikethroughClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.FontStrikethrough));
        }

        private void alignLeftClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Left));
        }

        private void alignCenterClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Center));
        }

        private void alignRightClick()
        {
            InHotKey = false;
            PFooter_FormatButtonClicked(PFooter, new FormatButtonClickedEventArgs(FormatType.Right));
        }

        private void space10Click()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetLineSpacing(RELineSpacing.Single);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void space15Click()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetLineSpacing(RELineSpacing.OneAndHalf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void space20Click()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetLineSpacing(RELineSpacing.Double);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void space30Click()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetLineSpacing(RELineSpacing.Triple);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addSpaceBeforeClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetParagraphSpacing(REParaSpace.Before, PNRuntimes.Instance.Settings.GeneralSettings.SpacePoints);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeSpaceBeforeClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetParagraphSpacing(REParaSpace.Before, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeSpaceAfterClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetParagraphSpacing(REParaSpace.After, 0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addSpaceAfterClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetParagraphSpacing(REParaSpace.After, PNRuntimes.Instance.Settings.GeneralSettings.SpacePoints);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void subscriptClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetFontDecoration(REFDecorationMask.CFM_SUBSCRIPT, REFDecorationStyle.CFE_SUBSCRIPT);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void superscriptClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetFontDecoration(REFDecorationMask.CFM_SUPERSCRIPT, REFDecorationStyle.CFE_SUPERSCRIPT);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clearFormatClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.RemoveHighlightColor();
                Edit.SelectionColor = SystemColors.WindowText;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note != null)
                {
                    var gr = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                    if (gr != null) Edit.SetFontByFont(gr.Font);
                }

                Edit.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
                Edit.SetFontDecoration(
                    REFDecorationMask.CFM_UNDERLINE | REFDecorationMask.CFM_BOLD | REFDecorationMask.CFM_ITALIC |
                    REFDecorationMask.CFM_STRIKEOUT | REFDecorationMask.CFM_SUBSCRIPT |
                    REFDecorationMask.CFM_SUPERSCRIPT, REFDecorationStyle.CFE_NONE);
                Edit.ClearBullets();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void increaseIndentClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetIndent(PNRuntimes.Instance.Settings.GeneralSettings.ParagraphIndent);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void decreaseIndentClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                Edit.SetIndent(-PNRuntimes.Instance.Settings.GeneralSettings.ParagraphIndent);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertPictureClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var filter = PNLang.Instance.GetCaptionText("image_filter", "Image files");
                filter +=
                    " (*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*.emf;*.wmf)|*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*.emf;*.wmf";
                var ofd = new OpenFileDialog { Filter = filter };
                if (!ofd.ShowDialog(this).Value) return;
                using (var image = Image.FromFile(ofd.FileName))
                {
                    insertImage(image);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertSmileyClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var smilies = new WndSmilies { Owner = this };
                smilies.SmilieSelected += smilies_ImageSelected;
                var showDialog = smilies.ShowDialog();
                if (showDialog != null && !showDialog.Value) smilies.SmilieSelected -= smilies_ImageSelected;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void smilies_ImageSelected(object sender, SmilieSelectedEventArgs e)
        {
            try
            {
                if (sender is WndSmilies smilies) smilies.SmilieSelected -= smilies_ImageSelected;
                insertImage(e.Image);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertDateTimeClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var now = DateTime.Now;
                var text =
                    now.ToString(
                        PNRuntimes.Instance.Settings.GeneralSettings.DateFormat + " " +
                        PNRuntimes.Instance.Settings.GeneralSettings.TimeFormat, ci);
                Edit.SelectedText = text;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertTableClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var dlgTable = new WndAddTable { Owner = this };
                dlgTable.TableReady += dlgTable_TableReady;
                var showDialog = dlgTable.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgTable.TableReady -= dlgTable_TableReady;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgTable_TableReady(object sender, CustomRtfReadyEventArgs e)
        {
            try
            {
                if (sender is WndAddTable d) d.TableReady -= dlgTable_TableReady;
                InHotKey = false;
                Edit.SelectedRtf = e.CustomRtf;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertLinkToNoteClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                var dlink = new WndLinkToNote(note.Id) { Owner = this };
                dlink.LinkReady += dlink_LinkReady;
                var showDialog = dlink.ShowDialog();
                if (showDialog != null && !showDialog.Value) dlink.LinkReady -= dlink_LinkReady;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlink_LinkReady(object sender, CustomRtfReadyEventArgs e)
        {
            try
            {
                if (sender is WndLinkToNote d) d.LinkReady -= dlink_LinkReady;
                InHotKey = false;
                if (Edit.SelectionStart == 0 || char.IsWhiteSpace(Edit.Text[Edit.SelectionStart - 1]))
                    Edit.SelectedText = e.CustomRtf;
                else
                    Edit.SelectedText = " " + e.CustomRtf;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertSpecialSymbolClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var dsps = new WndSpecialSymbols { Owner = this };
                dsps.SpecialSymbolSelected += dsps_SpecialSymbolSelected;
                var showDialog = dsps.ShowDialog();
                if (showDialog != null && !showDialog.Value) dsps.SpecialSymbolSelected -= dsps_SpecialSymbolSelected;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dsps_SpecialSymbolSelected(object sender, SpecialSymbolSelectedEventArgs e)
        {
            try
            {
                if (sender is WndSpecialSymbols d) d.SpecialSymbolSelected -= dsps_SpecialSymbolSelected;
                InHotKey = false;
                if (!Fonts.IsUnicodeCharAvailable(e.Symbol[0], Edit.GetFontName())) setDefFont();
                Edit.SelectedText = e.Symbol;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setDefFont()
        {
            try
            {
                InHotKey = false;
                var lf = new LOGFONT();
                lf.Init();
                lf.lfFaceName = PNStrings.DEFAULT_FONT_NAME;
                lf.SetFontSize(Edit.GetFontSize());
                Edit.SetFontByName(lf);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertDrawingClick()
        {
            try
            {
                if (Edit.ReadOnly) return;
                InHotKey = false;
                var dlgCanvas = new WndCanvas(this,
                    PNRuntimes.Instance.Settings.GeneralSettings.UseSkins
                        ? Colors.White
                        : ((SolidColorBrush)Background).Color);
                dlgCanvas.CanvasSaved += dlgCanvas_CanvasSaved;
                var showDialog = dlgCanvas.ShowDialog();
                if (showDialog != null && !showDialog.Value)
                    dlgCanvas.CanvasSaved -= dlgCanvas_CanvasSaved;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgCanvas_CanvasSaved(object sender, CanvasSavedEventArgs e)
        {
            try
            {
                if (sender is WndCanvas d) d.CanvasSaved -= dlgCanvas_CanvasSaved;
                if (e.Image != null)
                    insertImage(e.Image, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkSpellNowClick()
        {
            try
            {
                if (Spellchecking.CheckRESpelling(Edit, PHeader.Title))
                    WPFMessageBox.Show(PNLang.Instance.GetSpellText("msgComplete", "Spell checking complete"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkSpellAutoClick()
        {
            try
            {
                PNRuntimes.Instance.Settings.GeneralSettings.SpellMode = mnuCheckSpellAuto.IsChecked ? 1 : 0;
                PNData.SaveSpellSettings();
                PNWindows.Instance.FormMain.ApplySpellStatusChange(mnuCheckSpellAuto.IsChecked);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void downloadDictClick()
        {
            try
            {
                var updater = new PNUpdateChecker();
                IEnumerable<DictData> list;

                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    list = updater.GetListDictionaries();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                if (list != null)
                {
                    var d = new WndGetDicts(list) { Owner = PNWindows.Instance.FormMain };
                    d.ShowDialog();
                }
                else
                {
                    if (
                        WPFMessageBox.Show(
                            PNLang.Instance.GetMessageText("dict_connection_problem",
                                "The dictionaries download server is unavailable. Open dictionaries page in browser?"),
                            PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                        PNStatic.LoadPage(PNStrings.URL_DICTIONARIES);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findClick()
        {
            try
            {
                var dlg = new WndSearchReplace(SearchReplace.Search, Edit) { Owner = this };
                dlg.Show();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findNextClick()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PNRuntimes.Instance.FindString)) return;
                int position;
                switch (PNRuntimes.Instance.SearchMode)
                {
                    case SearchMode.Normal:
                        position = Edit.SelectionStart == Edit.TextLength
                            ? PNStatic.FindEditString(Edit, 0)
                            : PNStatic.FindEditString(Edit, Edit.SelectionStart + 1);
                        if (position == -1) PNStatic.FindEditString(Edit, 0);
                        break;
                    case SearchMode.RegularExp:
                        var reverse = (PNRuntimes.Instance.FindOptions & RichTextBoxFinds.Reverse) ==
                                      RichTextBoxFinds.Reverse;
                        position = Edit.SelectionStart == Edit.TextLength
                            ? PNStatic.FindEditStringByRegExp(Edit, 0, reverse)
                            : PNStatic.FindEditStringByRegExp(Edit, Edit.SelectionStart + 1, reverse);
                        if (position == -1)
                            if (reverse)
                                PNStatic.FindEditStringByRegExp(Edit, Edit.TextLength - 1, true);
                            else
                                PNStatic.FindEditStringByRegExp(Edit, 0, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void replaceClick()
        {
            try
            {
                InHotKey = false;
                var dlg = new WndSearchReplace(SearchReplace.Replace, Edit) { Owner = this };
                dlg.Show();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void selectAllClick()
        {
            Edit.SelectAll();
        }

        private void sortAscendingClick()
        {
            try
            {
                InHotKey = false;
                sortEdit(SortOrder.Ascending);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sortDescendingClick()
        {
            try
            {
                InHotKey = false;
                sortEdit(SortOrder.Descending);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDictMenu()
        {
            try
            {
                mnuCheckSpellAuto.IsChecked = PNRuntimes.Instance.Settings.GeneralSettings.SpellMode == 1;

                var count = mnuSpell.Items.Count;
                for (var i = count - 1; i > 4; i--)
                {
                    if (mnuSpell.Items[i] is MenuItem ti) ti.Click -= dictMenu_Click;
                    mnuSpell.Items.RemoveAt(i);
                }

                var files = new DirectoryInfo(PNPaths.Instance.DictDir).GetFiles("*.dic");
                if (files.Length > 0)
                    mnuSpell.Items.Add(new Separator());
                var orderedFiles = files.OrderBy(f => f.Name);
                foreach (var fi in orderedFiles)
                {
                    if (!File.Exists(Path.ChangeExtension(fi.FullName, "aff"))) continue;
                    var text = Path.GetFileNameWithoutExtension(fi.Name);
                    try
                    {
                        if (PNRuntimes.Instance.Dictionaries.Root != null)
                        {
                            var xe = PNRuntimes.Instance.Dictionaries.Root.Element(text);
                            if (xe != null) text = xe.Value;
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    var mi = new MenuItem { Header = text, Tag = fi.Name };
                    mi.Click += dictMenu_Click;
                    if (PNRuntimes.Instance.Settings.GeneralSettings.SpellDict == fi.Name) mi.IsChecked = true;
                    mnuSpell.Items.Add(mi);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dictMenu_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is MenuItem mi)) return;
                if (!mi.IsChecked)
                {
                    PNRuntimes.Instance.Settings.GeneralSettings.SpellDict = (string)mi.Tag;
                    Spellchecking.HuspellStop();
                    var fileDict = Path.Combine(PNPaths.Instance.DictDir,
                        PNRuntimes.Instance.Settings.GeneralSettings.SpellDict);
                    var newName = Path.ChangeExtension(PNRuntimes.Instance.Settings.GeneralSettings.SpellDict,
                        ".aff");
                    if (!string.IsNullOrEmpty(newName))
                    {
                        var fileAff = Path.Combine(PNPaths.Instance.DictDir,
                            newName);
                        Spellchecking.HunspellInit(fileDict, fileAff);
                    }
                }
                else
                {
                    PNRuntimes.Instance.Settings.GeneralSettings.SpellDict = "";
                    PNRuntimes.Instance.Settings.GeneralSettings.SpellMode = 0;
                    Spellchecking.HuspellStop();
                }

                PNData.SaveSpellSettings();
                PNWindows.Instance.FormMain.ApplySpellDictionaryChange();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void sortEdit(SortOrder order)
        {
            try
            {
                if (order == SortOrder.None) return;
                InHotKey = false;
                var hiddenEdit = new PNRichEditBox();
                var isTable = Edit.Rtf.Contains("\\trowd\\trgaph");
                var sortedLines = order == SortOrder.Ascending
                    ? new SortedList<string, Tuple<int, int, bool>>(new AscStringComparer())
                    : new SortedList<string, Tuple<int, int, bool>>(new DescStringComparer());
                int start = 0, index = 0;
                var tableLine = "";
                var tableStart = 0;
                var addTable = false;
                foreach (var line in Edit.Lines)
                {
                    if (isTable)
                    {
                        Edit.Select(start, line.Length);
                        var tbl = Edit.SelectedRtf.Contains("\\trowd\\trgaph");
                        if (line.Trim().Length == 0 && tbl && !line.Contains('\t'))
                        {
                            start += line.Length + 1;
                            continue;
                        }

                        if (tbl)
                        {
                            tableLine += line;
                            if (tableStart == 0) tableStart = start;
                            addTable = true;
                        }
                        else
                        {
                            if (addTable)
                            {
                                sortedLines.Add(tableLine + index++, Tuple.Create(tableStart, tableLine.Length, true));
                                tableLine = "";
                                tableStart = 0;
                                addTable = false;
                            }

                            sortedLines.Add(line + index++, Tuple.Create(start, line.Length, false));
                        }
                    }
                    else
                    {
                        sortedLines.Add(line + index++, Tuple.Create(start, line.Length, false));
                    }

                    start += line.Length + 1;
                }

                index = 1;
                foreach (var sl in sortedLines)
                {
                    if (sl.Value.Item2 > 0)
                    {
                        Edit.Select(sl.Value.Item1, sl.Value.Item2);
                        Edit.Copy();
                        hiddenEdit.Paste();
                    }

                    if (index < sortedLines.Count && !sl.Value.Item3)
                        hiddenEdit.AppendText("\r\n");
                    index++;
                }

                hiddenEdit.SelectAll();
                hiddenEdit.Copy();
                Edit.SelectAll();
                Edit.Paste();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        private void NoteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.SaveAsText:
                    case CommandType.Print:
                    case CommandType.AdjustSchedule:
                    case CommandType.HideNote:
                    case CommandType.DeleteNote:
                    case CommandType.DockNote:
                    case CommandType.DockAllNone:
                    case CommandType.DockAllLeft:
                    case CommandType.DockAllTop:
                    case CommandType.DockAllRight:
                    case CommandType.DockAllBottom:
                    case CommandType.SendAsText:
                    case CommandType.SendAsAttachment:
                    case CommandType.SendAsZip:
                    case CommandType.ContactAdd:
                    case CommandType.ContactGroupAdd:
                    case CommandType.ContactGroupSelect:
                    case CommandType.ContactSelect:
                    case CommandType.Tags:
                    case CommandType.LinkedNotes:
                    case CommandType.LinkedManagement:
                    case CommandType.HighPriority:
                    case CommandType.ProtectionMode:
                    case CommandType.SetNotePassword:
                    case CommandType.RemoveNotePassword:
                    case CommandType.MarkAsComplete:
                    case CommandType.Pin:
                    case CommandType.Unpin:
                    case CommandType.Scramble:
                        e.CanExecute = true;
                        break;
                    case CommandType.SaveNote:
                        e.CanExecute = note.Changed;
                        break;
                    case CommandType.SaveAs:
                        e.CanExecute = note.FromDB && note.GroupId != (int)SpecialGroups.Diary;
                        break;
                    case CommandType.RestoreFromBackup:
                        e.CanExecute = !Edit.ReadOnly;
                        break;
                    case CommandType.DuplicateNote:
                        e.CanExecute = note.GroupId != (int)SpecialGroups.Diary;
                        break;
                    case CommandType.SaveAsShortcut:
                        e.CanExecute = note.FromDB;
                        break;
                    case CommandType.AdjustAppearance:
                        e.CanExecute = note.DockStatus == DockStatus.None;
                        break;
                    case CommandType.ToPanel:
                        e.CanExecute = PNRuntimes.Instance.Settings.Behavior.ShowNotesPanel;
                        break;
                    case CommandType.SendNetwork:
                        e.CanExecute = PNRuntimes.Instance.Settings.Network.EnableExchange;
                        break;
                    case CommandType.Reply:
                        e.CanExecute = PNRuntimes.Instance.Settings.Network.EnableExchange &&
                                       ((note.SentReceived & SendReceiveStatus.Received) == SendReceiveStatus.Received);
                        break;
                    case CommandType.ToPost:
                        e.CanExecute = PNCollections.Instance.ActivePostPlugins.Count > 0 && Edit.TextLength > 0;
                        break;
                    case CommandType.FromPost:
                        e.CanExecute = PNCollections.Instance.ActivePostPlugins.Count > 0 && !Edit.ReadOnly;
                        break;
                    case CommandType.ExportToOffice:
                        e.CanExecute = PNOffice.GetOfficeAppVersion(OfficeApp.Outlook).In(11, 12);
                        break;
                    case CommandType.ExportToOutlookNote:
                        e.CanExecute = PNOffice.GetOfficeAppVersion(OfficeApp.Outlook).In(11, 12);
                        break;
                    case CommandType.AddFavorites:
                        e.CanExecute = note.GroupId != (int)SpecialGroups.Diary;
                        break;
                    case CommandType.RemoveFavorites:
                        e.CanExecute = note.GroupId != (int)SpecialGroups.Diary;
                        break;
                    case CommandType.OnTop:
                        e.CanExecute = note.DockStatus == DockStatus.None;
                        break;
                    case CommandType.RollUnroll:
                        e.CanExecute = note.DockStatus == DockStatus.None &&
                                       !PNRuntimes.Instance.Settings.GeneralSettings.UseSkins;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void NoteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(Handle);
                if (note == null) return;
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.SaveNote:
                        saveClick(false);
                        break;
                    case CommandType.SaveAs:
                        saveClick(true);
                        break;
                    case CommandType.SaveAsText:
                        saveAsTextClick();
                        break;
                    case CommandType.RestoreFromBackup:
                        restoreFromBackupClick();
                        break;
                    case CommandType.DuplicateNote:
                        duplicateNoteClick();
                        break;
                    case CommandType.Print:
                        printClick();
                        break;
                    case CommandType.SaveAsShortcut:
                        saveAsShortcutClick();
                        break;
                    case CommandType.AdjustAppearance:
                        adjustAppearanceClick();
                        break;
                    case CommandType.AdjustSchedule:
                        adjustScheduleClick();
                        break;
                    case CommandType.HideNote:
                        hideNoteClick();
                        break;
                    case CommandType.DeleteNote:
                        deleteNoteClick();
                        break;
                    case CommandType.ToPanel:
                        toPanelClick();
                        break;
                    case CommandType.DockAllNone:
                        dockNoneClick();
                        break;
                    case CommandType.DockAllLeft:
                        dockLefClick();
                        break;
                    case CommandType.DockAllTop:
                        dockTopClick();
                        break;
                    case CommandType.DockAllRight:
                        dockRightClick();
                        break;
                    case CommandType.DockAllBottom:
                        dockBottomClick();
                        break;
                    case CommandType.SendAsText:
                        sendAsTextClick();
                        break;
                    case CommandType.SendAsAttachment:
                        sendAsAttachment_Click();
                        break;
                    case CommandType.SendAsZip:
                        sendZipClick();
                        break;
                    case CommandType.ContactAdd:
                        addContactClick();
                        break;
                    case CommandType.ContactGroupAdd:
                        addContactsGroupClick();
                        break;
                    case CommandType.ContactSelect:
                        selectContactClick();
                        break;
                    case CommandType.ContactGroupSelect:
                        selectContactsGroupClick();
                        break;
                    case CommandType.Reply:
                        replyClick();
                        break;
                    case CommandType.ExportToOutlookNote:
                        exportOutlookNoteClick();
                        break;
                    case CommandType.Tags:
                        tagsClick();
                        break;
                    case CommandType.LinkedManagement:
                        manageLinksClick();
                        break;
                    case CommandType.AddFavorites:
                        addToFavoritesClick();
                        break;
                    case CommandType.RemoveFavorites:
                        removeFromFavoritesClick();
                        break;
                    case CommandType.OnTop:
                        onTopClick();
                        break;
                    case CommandType.HighPriority:
                        toggleHighPriorityClick();
                        break;
                    case CommandType.ProtectionMode:
                        toggleProtectionModeClick();
                        break;
                    case CommandType.SetNotePassword:
                        setPasswordClick();
                        break;
                    case CommandType.RemoveNotePassword:
                        removePasswordClick();
                        break;
                    case CommandType.MarkAsComplete:
                        markAsCompleteClick();
                        break;
                    case CommandType.RollUnroll:
                        rollUnrollClick();
                        break;
                    case CommandType.Pin:
                        pinClick();
                        break;
                    case CommandType.Unpin:
                        unpinClick();
                        break;
                    case CommandType.Scramble:
                        scrambleClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void EditCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (_DoNotCheckCanExecute || !(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Find:
                    case CommandType.FindNext:
                    case CommandType.DownloadDict:
                    case CommandType.InsertLink:
                    case CommandType.InsertSpecialSymbol:
                    case CommandType.InsertDrawing:
                    case CommandType.InsertTable:
                    case CommandType.InsertSmilie:
                    case CommandType.InsertDateTime:
                    case CommandType.InsertPicture:
                    case CommandType.AddSpaceAfter:
                    case CommandType.AddSpaceBefore:
                    case CommandType.RemoveSpaceAfter:
                    case CommandType.RemoveSpaceBefore:
                    case CommandType.LineSpacing:
                    case CommandType.Bullets:
                    case CommandType.Highlight:
                    case CommandType.FontColor:
                    case CommandType.FontSize:
                    case CommandType.Font:
                    case CommandType.NoBulletsMenu:
                    case CommandType.SimpleBulletsMenu:
                    case CommandType.BulletsMenu:
                    case CommandType.FontSizeMenu:
                    case CommandType.AutomaticColor:
                    case CommandType.NoHighlight:
                    case CommandType.ColorMenu:
                        e.CanExecute = true;
                        break;
                    case CommandType.SortDesc:
                    case CommandType.SortAsc:
                        e.CanExecute = Edit != null && Edit.Lines.Length > 1 && !Edit.ReadOnly;
                        break;
                    case CommandType.SelectAll:
                        e.CanExecute = Edit != null && Edit.TextLength > 0;
                        break;
                    case CommandType.ReplaceFromPost:
                        e.CanExecute = Edit != null && PNCollections.Instance.ActivePostPlugins.Count > 0 &&
                                       !Edit.ReadOnly;
                        break;
                    case CommandType.PostSelectedtOn:
                        e.CanExecute = Edit != null && Edit.SelectionLength > 0 &&
                                       PNCollections.Instance.ActivePostPlugins.Count > 0;
                        break;
                    case CommandType.SearchWeb:
                        e.CanExecute = Edit != null && Edit.SelectionLength > 0;
                        break;
                    case CommandType.Replace:
                        e.CanExecute = Edit != null && !Edit.ReadOnly;
                        break;
                    case CommandType.Spell:
                        e.CanExecute = Directory.Exists(PNPaths.Instance.DictDir);
                        break;
                    case CommandType.CheckSpellNow:
                        e.CanExecute = Edit != null && Spellchecking.Initialized &&
                                       !string.IsNullOrWhiteSpace(
                                           PNRuntimes.Instance.Settings.GeneralSettings.SpellDict) && !Edit.ReadOnly;
                        break;
                    case CommandType.CheckSpellAuto:
                        e.CanExecute = Spellchecking.Initialized &&
                                       !string.IsNullOrWhiteSpace(
                                           PNRuntimes.Instance.Settings.GeneralSettings.SpellDict);
                        break;
                    case CommandType.Insert:
                    case CommandType.IncreaseIndent:
                    case CommandType.DecreaseIndent:
                    case CommandType.Superscript:
                    case CommandType.Subscript:
                    case CommandType.Space10:
                    case CommandType.Space15:
                    case CommandType.Space20:
                    case CommandType.Space30:
                    case CommandType.AlignCenter:
                    case CommandType.AlignLeft:
                    case CommandType.AlignRight:
                    case CommandType.Strikethrough:
                    case CommandType.Underline:
                    case CommandType.Italic:
                    case CommandType.Bold:
                    case CommandType.Format:
                        e.CanExecute = Edit != null && !Edit.ReadOnly;
                        break;
                    case CommandType.ClearFormat:
                        e.CanExecute = Edit != null && Edit.SelectionLength > 0 && !Edit.ReadOnly;
                        break;
                    case CommandType.ToggleCase:
                    case CommandType.ToLower:
                    case CommandType.ToUpper:
                    case CommandType.CapitalSentence:
                    case CommandType.CapitalWord:
                        e.CanExecute = Edit != null && Edit.SelectedText.Length > 0 && !Edit.ReadOnly;
                        break;
                    case CommandType.PastePlain:
                        e.CanExecute = Edit != null && (Edit.CanPaste(DataFormats.GetFormat(DataFormats.Html)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.OemText))
                                                        || Edit.CanPaste(DataFormats.GetFormat(DataFormats.Rtf)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.StringFormat))
                                                        || Edit.CanPaste(DataFormats.GetFormat(DataFormats.Text)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.UnicodeText))
                                       ) && !Edit.ReadOnly;
                        break;
                    case CommandType.CopyPlain:
                        e.CanExecute = Edit != null && Edit.SelectedText.Length > 0;
                        break;
                    case CommandType.Paste:
                        e.CanExecute = Edit != null && (Edit.CanPaste(DataFormats.GetFormat(DataFormats.Bitmap)) ||
                                                        Edit.CanPaste(
                                                            DataFormats.GetFormat(DataFormats.EnhancedMetafile))
                                                        || Edit.CanPaste(DataFormats.GetFormat(DataFormats.Html)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.MetafilePict))
                                                        || Edit.CanPaste(DataFormats.GetFormat(DataFormats.OemText)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.Rtf))
                                                        || Edit.CanPaste(
                                                            DataFormats.GetFormat(DataFormats.StringFormat)) ||
                                                        Edit.CanPaste(DataFormats.GetFormat(DataFormats.Text))
                                                        || Edit.CanPaste(
                                                            DataFormats.GetFormat(DataFormats.UnicodeText))) &&
                                       !Edit.ReadOnly;
                        break;
                    case CommandType.Copy:
                        e.CanExecute = Edit != null && Edit.SelectedText.Length > 0;
                        break;
                    case CommandType.Cut:
                        e.CanExecute = Edit != null && Edit.SelectedText.Length > 0 && !Edit.ReadOnly;
                        break;
                    case CommandType.Redo:
                        e.CanExecute = Edit != null && Edit.CanRedo && !Edit.ReadOnly;
                        break;
                    case CommandType.Undo:
                        e.CanExecute = Edit != null && Edit.CanUndo && !Edit.ReadOnly;
                        break;

                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void EditCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.SortDesc:
                        sortDescendingClick();
                        break;
                    case CommandType.SortAsc:
                        sortAscendingClick();
                        break;
                    case CommandType.SelectAll:
                        selectAllClick();
                        break;
                    case CommandType.Find:
                        findClick();
                        break;
                    case CommandType.FindNext:
                        findNextClick();
                        break;
                    case CommandType.Replace:
                        replaceClick();
                        break;
                    case CommandType.CheckSpellNow:
                        checkSpellNowClick();
                        break;
                    case CommandType.CheckSpellAuto:
                        checkSpellAutoClick();
                        break;
                    case CommandType.DownloadDict:
                        downloadDictClick();
                        break;
                    case CommandType.InsertLink:
                        insertLinkToNoteClick();
                        break;
                    case CommandType.InsertSpecialSymbol:
                        insertSpecialSymbolClick();
                        break;
                    case CommandType.InsertDrawing:
                        insertDrawingClick();
                        break;
                    case CommandType.InsertTable:
                        insertTableClick();
                        break;
                    case CommandType.InsertSmilie:
                        insertSmileyClick();
                        break;
                    case CommandType.InsertDateTime:
                        insertDateTimeClick();
                        break;
                    case CommandType.InsertPicture:
                        insertPictureClick();
                        break;
                    case CommandType.IncreaseIndent:
                        increaseIndentClick();
                        break;
                    case CommandType.DecreaseIndent:
                        decreaseIndentClick();
                        break;
                    case CommandType.ClearFormat:
                        clearFormatClick();
                        break;
                    case CommandType.Superscript:
                        superscriptClick();
                        break;
                    case CommandType.Subscript:
                        subscriptClick();
                        break;
                    case CommandType.AddSpaceAfter:
                        addSpaceAfterClick();
                        break;
                    case CommandType.AddSpaceBefore:
                        addSpaceBeforeClick();
                        break;
                    case CommandType.RemoveSpaceAfter:
                        removeSpaceAfterClick();
                        break;
                    case CommandType.RemoveSpaceBefore:
                        removeSpaceBeforeClick();
                        break;
                    case CommandType.Space10:
                        space10Click();
                        break;
                    case CommandType.Space15:
                        space15Click();
                        break;
                    case CommandType.Space20:
                        space20Click();
                        break;
                    case CommandType.Space30:
                        space30Click();
                        break;
                    case CommandType.AlignRight:
                        alignRightClick();
                        break;
                    case CommandType.AlignLeft:
                        alignLeftClick();
                        break;
                    case CommandType.AlignCenter:
                        alignCenterClick();
                        break;
                    case CommandType.Strikethrough:
                        strikethroughClick();
                        break;
                    case CommandType.Underline:
                        underlineClick();
                        break;
                    case CommandType.Italic:
                        italicClick();
                        break;
                    case CommandType.Bold:
                        boldClick();
                        break;
                    case CommandType.Font:
                        fontClick();
                        break;
                    case CommandType.ToggleCase:
                        toggleCaseClick();
                        break;
                    case CommandType.CapitalWord:
                        capWordClick();
                        break;
                    case CommandType.CapitalSentence:
                        capSentClick();
                        break;
                    case CommandType.ToLower:
                        toLowerClick();
                        break;
                    case CommandType.ToUpper:
                        toUpperClick();
                        break;
                    case CommandType.PastePlain:
                        pastePlainClick();
                        break;
                    case CommandType.CopyPlain:
                        copyPlainClick();
                        break;
                    case CommandType.Paste:
                        pasteClick();
                        break;
                    case CommandType.Copy:
                        copyClick();
                        break;
                    case CommandType.Cut:
                        cutClick();
                        break;
                    case CommandType.Redo:
                        redoClick();
                        break;
                    case CommandType.Undo:
                        undoClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BulletsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.NoBulletsMenu:
                        Edit.ClearBullets();
                        break;
                    case CommandType.SimpleBulletsMenu:
                        Edit.SetBullets(RENumbering.PFN_BULLET, 0,
                            PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                        break;
                    default:
                        switch (e.Parameter?.ToString())
                        {
                            case "mnuArabicPoint":
                                Edit.SetBullets(RENumbering.PFN_ARABIC, RENumberingStyle.PFNS_PERIOD,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuArabicParts":
                                Edit.SetBullets(RENumbering.PFN_ARABIC, RENumberingStyle.PFNS_PAREN,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuSmallLettersPoint":
                                Edit.SetBullets(RENumbering.PFN_LCLETTER, RENumberingStyle.PFNS_PERIOD,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuSmallLettersPart":
                                Edit.SetBullets(RENumbering.PFN_LCLETTER, RENumberingStyle.PFNS_PAREN,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuBigLettersPoint":
                                Edit.SetBullets(RENumbering.PFN_UCLETTER, RENumberingStyle.PFNS_PERIOD,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuBigLettersParts":
                                Edit.SetBullets(RENumbering.PFN_UCLETTER, RENumberingStyle.PFNS_PAREN,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuLatinSmall":
                                Edit.SetBullets(RENumbering.PFN_LCROMAN, RENumberingStyle.PFNS_PERIOD,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                            case "mnuLatinBig":
                                Edit.SetBullets(RENumbering.PFN_UCROMAN, RENumberingStyle.PFNS_PERIOD,
                                    PNRuntimes.Instance.Settings.GeneralSettings.BulletsIndent);
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FontSizeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var fontSize = Convert.ToInt32(e.Parameter);
                Edit.SetFontSize(fontSize);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ColorCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                var parent = ((e.Source as MenuItem)?.Parent as ItemsControl)?.Name;
                if (string.IsNullOrEmpty(parent)) return;
                if (parent.In("mnuFontColor", "ctmFontColor"))
                {
                    Edit.SelectionColor = command.Type == CommandType.AutomaticColor
                        ? SystemColors.WindowText
                        : System.Drawing.Color.FromName(e.Parameter.ToString());
                }
                else
                {
                    if (command.Type == CommandType.NoHighlight)
                        Edit.RemoveHighlightColor();
                    else
                        Edit.SelectionBackColor = System.Drawing.Color.FromName(e.Parameter.ToString());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DropCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DropCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.DropInsertContent:
                        _DropCase = DropCase.Content;
                        break;
                    case CommandType.DropInsertObject:
                        _DropCase = DropCase.Object;
                        break;
                    case CommandType.DropInsertLink:
                        _DropCase = DropCase.Link;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}
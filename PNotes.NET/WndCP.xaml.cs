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

using PNotes.NET.Annotations;
using PNRichEdit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndCP.xaml
    /// </summary>
    public partial class WndCP : INotifyPropertyChanged
    {
        private enum NewGroupTarget
        {
            None,
            TopLevel,
            SubGroup
        }

        [Flags]
        private enum VisibleHidden
        {
            None = 0,
            Visible = 1,
            Hidden = 2
        }

        private enum FavoriteStatus
        {
            No,
            Yes,
            Mix
        }

        private enum DragSource
        {
            None,
            ListGroup,
            ListIncoming,
            Tree
        }

        private sealed class CPBackNote : INotifyPropertyChanged
        {
            private string _name;
            private string _original;
            private DateTime _created;

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

            public string Original
            {
                private get => _original;
                set
                {
                    if (value == _original) return;
                    _original = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Created
            {
                private get => _created;
                set
                {
                    if (value.Equals(_created)) return;
                    _created = value;
                    OnPropertyChanged();
                }
            }

            public string Id { get; set; }

            public string FileName { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class CPNote : INotifyPropertyChanged
        {
            private NoteState _state;
            private string _name;
            private string _id;
            private bool _priority;
            private bool _completed;
            private bool _protected;
            private PasswordProtectionMode _password;
            private bool _pinned;
            private bool _favorites;
            private SendReceiveStatus _sentReceived;
            private bool _encrypted;
            private string _group;
            private string _prevGroup;
            private DateTime _created;
            private DateTime _saved;
            private DateTime _deleted;
            private string _scheduleType;
            private string _tags;
            private string _content;
            private string _sentTo;
            private DateTime _sentAt;
            private string _receivedFrom;
            private DateTime _receivedAt;

            public NoteState State
            {
                private get { return _state; }
                set
                {
                    if (value == _state) return;
                    _state = value;
                    OnPropertyChanged();
                }
            }

            public string Name
            {
                private get { return _name; }
                set
                {
                    if (value == _name) return;
                    _name = value;
                    OnPropertyChanged();
                }
            }

            public string Id
            {
                get => _id;
                set
                {
                    if (value == _id) return;
                    _id = value;
                    OnPropertyChanged();
                }
            }

            public bool Priority
            {
                private get { return _priority; }
                set
                {
                    if (value.Equals(_priority)) return;
                    _priority = value;
                    OnPropertyChanged();
                }
            }

            public bool Completed
            {
                private get { return _completed; }
                set
                {
                    if (value.Equals(_completed)) return;
                    _completed = value;
                    OnPropertyChanged();
                }
            }

            public bool Protected
            {
                private get { return _protected; }
                set
                {
                    if (value.Equals(_protected)) return;
                    _protected = value;
                    OnPropertyChanged();
                }
            }

            public PasswordProtectionMode Password
            {
                private get { return _password; }
                set
                {
                    if (value == _password) return;
                    _password = value;
                    OnPropertyChanged();
                }
            }

            public bool Pinned
            {
                private get { return _pinned; }
                set
                {
                    if (value.Equals(_pinned)) return;
                    _pinned = value;
                    OnPropertyChanged();
                }
            }

            public bool Favorites
            {
                get => _favorites;
                set
                {
                    if (value.Equals(_favorites)) return;
                    _favorites = value;
                    OnPropertyChanged();
                }
            }

            public SendReceiveStatus SentReceived
            {
                private get { return _sentReceived; }
                set
                {
                    if (value == _sentReceived) return;
                    _sentReceived = value;
                    OnPropertyChanged();
                }
            }

            public bool Encrypted
            {
                private get { return _encrypted; }
                set
                {
                    if (value.Equals(_encrypted)) return;
                    _encrypted = value;
                    OnPropertyChanged();
                }
            }

            public string Group
            {
                private get { return _group; }
                set
                {
                    if (value == _group) return;
                    _group = value;
                    OnPropertyChanged();
                }
            }

            public string PrevGroup
            {
                private get { return _prevGroup; }
                set
                {
                    if (value == _prevGroup) return;
                    _prevGroup = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Created
            {
                private get { return _created; }
                set
                {
                    if (value.Equals(_created)) return;
                    _created = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Saved
            {
                private get { return _saved; }
                set
                {
                    if (value.Equals(_saved)) return;
                    _saved = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Deleted
            {
                private get { return _deleted; }
                set
                {
                    if (value.Equals(_deleted)) return;
                    _deleted = value;
                    OnPropertyChanged();
                }
            }

            public string ScheduleType
            {
                private get { return _scheduleType; }
                set
                {
                    if (value == _scheduleType) return;
                    _scheduleType = value;
                    OnPropertyChanged();
                }
            }

            public string Tags
            {
                private get { return _tags; }
                set
                {
                    if (value == _tags) return;
                    _tags = value;
                    OnPropertyChanged();
                }
            }

            public string Content
            {
                private get { return _content; }
                set
                {
                    if (value == _content) return;
                    _content = value;
                    OnPropertyChanged();
                }
            }

            public string SentTo
            {
                private get { return _sentTo; }
                set
                {
                    if (value == _sentTo) return;
                    _sentTo = value;
                    OnPropertyChanged();
                }
            }

            public DateTime SentAt
            {
                private get { return _sentAt; }
                set
                {
                    if (value.Equals(_sentAt)) return;
                    _sentAt = value;
                    OnPropertyChanged();
                }
            }

            public string ReceivedFrom
            {
                private get { return _receivedFrom; }
                set
                {
                    if (value == _receivedFrom) return;
                    _receivedFrom = value;
                    OnPropertyChanged();
                }
            }

            public DateTime ReceivedAt
            {
                private get { return _receivedAt; }
                set
                {
                    if (value.Equals(_receivedAt)) return;
                    _receivedAt = value;
                    OnPropertyChanged();
                }
            }

            public int IdGroup { get; set; }

            public int IdPrevGroup { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public WndCP()
        {
            InitializeComponent();
            initializeEdit();
            DataContext = PNSingleton.Instance.FontUser;
            ctmGroups.DataContext = PNSingleton.Instance.FontUser;
            var dp = DependencyPropertyDescriptor.FromProperty(FlowDirectionProperty, typeof(Window));
            dp?.AddValueChanged(this, delegate
            {
                if (ctmGroups != null)
                {
                    ctmGroups.FlowDirection = FlowDirection;
                }
            });
        }

        internal WndCP(bool allowSave)
            : this()
        {
            _AllowSettingsSave = allowSave;
        }

        private PNRichEditBox _EditPreview;
        private EditControl _EditControl;
        private HwndSource _HwndSource;
        private bool _Loaded;
        private readonly bool _AllowSettingsSave = true;
        private bool _AllowColumnsSave;
        private bool _IncBinInSearch;

        private ContextMenu _NotesColsMenu, _BackColsMenu;

        private PNote _TempNote;

        private DragSource _DragSource = DragSource.None;
        private string _CapTotal = "", _CapCount = "", _CapGroup = "";
        private PNTreeItem _LastTreeItem, _HighLightedItem, _RightSelectedItem;
        private int _LastGroup = -1, _SelectedGroup = -1234;
        private NewGroupTarget _NewGroupTarget = NewGroupTarget.None;
        private readonly DayOfWeekStruct[] _DaysOfWeek = new DayOfWeekStruct[7];

        private readonly List<ColumnProps> _NotesGridCols = new List<ColumnProps>();
        private readonly List<GrdSort> _NotesGridSorts = new List<GrdSort>();
        private readonly List<ColumnProps> _BackGridCols = new List<ColumnProps>();
        private readonly List<GrdSort> _BackGridSorts = new List<GrdSort>();

        private readonly GridLength[] _OuterColsWidth = new GridLength[2];

        private GridLength _RowHeight = new GridLength(1, GridUnitType.Star);
        private GridLength _ColumnWidth = new GridLength(1, GridUnitType.Star);

        private readonly List<string> _SearchResults = new List<string>();

        private readonly ObservableCollection<PNTreeItem> _PGroups = new ObservableCollection<PNTreeItem>();
        private readonly ObservableCollection<CPNote> _Notes = new ObservableCollection<CPNote>();
        private readonly ObservableCollection<CPBackNote> _BackNotes = new ObservableCollection<CPBackNote>();

        public bool InLongOperation
        {
            get => _inLongOperation;
            set
            {
                if (_inLongOperation == value) return;
                _inLongOperation = value;
                OnPropertyChanged();
            }
        }

        internal void DiaryRestored()
        {
            try
            {
                foreach (
                    var dummy in
                    tvwGroups.Items.OfType<PNTreeItem>()
                        .Where(item => changeNodeText(item, (int)SpecialGroups.RecycleBin)))
                {
                    break;
                }
                changeSpecialNodeText((int)SpecialGroups.Favorites);
                changeSpecialNodeText((int)SpecialGroups.Backup);
                changeSpecialNodeText((int)SpecialGroups.SearchResults);
                updateStatusBar();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void SaveCPProperties()
        {
            try
            {
                if (!_AllowSettingsSave) return;
                //save Control Panel properties
                PNRuntimes.Instance.Settings.Config.CPLastGroup = _LastGroup;
                switch (WindowState)
                {
                    case WindowState.Normal:
                        PNRuntimes.Instance.Settings.Config.CPLocation = this.GetLocation();
                        PNRuntimes.Instance.Settings.Config.CPSize = this.GetSize();
                        break;
                    case WindowState.Maximized:
                        PNRuntimes.Instance.Settings.Config.CPLocation = new Point(-1, -1);
                        PNRuntimes.Instance.Settings.Config.CPSize = new Size(double.MaxValue, double.MaxValue);
                        break;
                        //case WindowState.Minimized:
                        //    return;
                }
                PNData.SaveCPProperties();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal IntPtr Handle => _HwndSource.Handle;

        internal ContextMenu CPMenu => ctmList;

        internal void SetToolbarIcons(bool bigIcons)
        {
            try
            {
                foreach (var bt in tbrCp.Children.OfType<ButtonBase>())
                {
                    if (!(bt.Content is Image img)) continue;
                    if (img.Source != null)
                    {
                        var arr = img.Source.ToString().Split(@"/");

                        var file = Path.GetFileNameWithoutExtension(arr[arr.Length - 1]);

                        if (bigIcons)
                            img.SetResourceReference(Image.SourceProperty, "big_" + file);
                        else
                            img.SetResourceReference(Image.SourceProperty, file);
                    }
                    Utils.SetIsBigIcon(bt, bigIcons);
                }
                foreach (var bt in tbrGroups.Children.OfType<ButtonBase>())
                {
                    if (!(bt.Content is Image img)) continue;
                    if (img.Source != null)
                    {
                        var arr = img.Source.ToString().Split(@"/");

                        var file = Path.GetFileNameWithoutExtension(arr[arr.Length - 1]);

                        if (bigIcons)
                            img.SetResourceReference(Image.SourceProperty, "big_" + file);
                        else
                            img.SetResourceReference(Image.SourceProperty, file);
                    }
                    Utils.SetIsBigIcon(bt, bigIcons);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void HideToolbarButtons(PNHiddenMenu[] hiddens)
        {
            try
            {
                Separator sep = null;
                var visibleCount = 0;
                foreach (var item in tbrCp.Children)
                {
                    if (item is ToolbarButton || item is ToolbarToggleButton || item is DropDownButton)
                    {
                        var btn = item as ButtonBase;
                        if (hiddens.All(hm => hm.Name != btn.Name.Replace("cmd", "mnu")))
                        {
                            btn.Visibility = Visibility.Visible;
                            visibleCount++;
                        }
                        else
                        {
                            btn.Visibility = Visibility.Collapsed;
                        }
                    }
                    else if (item is Separator)
                    {
                        sep = item as Separator;
                        sep.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
                        visibleCount = 0;
                    }
                }
                if (sep != null)
                {
                    sep.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SelectSpecificGroup(int groupId)
        {
            try
            {
                foreach (var pnTreeItem in from node in tvwGroups.Items.OfType<PNTreeItem>()
                                           let pnGroup = node.Tag as PNGroup
                                           where pnGroup != null && pnGroup.Id == groupId
                                           select node)
                {
                    pnTreeItem.IsSelected = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void GroupIconChanged(int groupId)
        {
            try
            {
                var pnGroup = PNCollections.Instance.Groups.GetGroupById(groupId);
                if (pnGroup == null) return;
                var pnTreeItem = getTreeItemByGroupId(groupId, null);
                if (pnTreeItem == null) return;
                if (Equals(pnTreeItem.Image, pnGroup.Image)) return;
                if (!pnGroup.IsDefaultImage)
                    pnTreeItem.Image = pnGroup.Image;
                else
                    pnTreeItem.SetImageResource(pnGroup.ImageName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #region Private procedures

        private void setSelectedGroup(int groupId)
        {
            try
            {
                if (groupId.In(Enum.GetValues(typeof(SpecialGroups)).Cast<int>().ToArray()))
                {
                    foreach (var it in tvwGroups.Items.OfType<PNTreeItem>())
                    {
                        var gr = it.Tag as PNGroup;
                        if (gr == null) continue;
                        if (gr.Id == groupId)
                        {
                            it.IsSelected = true;
                            return;
                        }
                    }
                }
                else
                {
                    setSelectedSubGroup(tvwGroups.Items[0] as PNTreeItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool setSelectedSubGroup(PNTreeItem item)
        {
            try
            {
                var gr = item.Tag as PNGroup;
                if (gr != null && gr.Id == PNRuntimes.Instance.Settings.Config.CPLastGroup)
                {
                    item.IsSelected = true;
                    return true;
                }
                return item.Items.OfType<PNTreeItem>().Any(setSelectedSubGroup);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void hideGroupsPane()
        {
            try
            {
                _OuterColsWidth[0] = OuterGrid.ColumnDefinitions[0].Width;
                OuterGrid.ColumnDefinitions[0].Width = new GridLength(0);
                _OuterColsWidth[1] = OuterGrid.ColumnDefinitions[1].Width;
                OuterGrid.ColumnDefinitions[1].Width = new GridLength(0);
                InnerGrid.Margin = new Thickness(0);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void hidePreview()
        {
            try
            {
                if (!PNRuntimes.Instance.Settings.Config.CPPvwRight)
                {
                    SplitVert.Visibility = Visibility.Collapsed;
                    InnerGrid.ColumnDefinitions[2].Width = new GridLength(0);
                    _EditControl.WinForm.Visible = false;
                    SplitHorz.Visibility = Visibility.Collapsed;
                    _RowHeight = InnerGrid.RowDefinitions[2].Height;
                    InnerGrid.RowDefinitions[2].Height = new GridLength(0);
                }
                else
                {
                    SplitHorz.Visibility = Visibility.Collapsed;
                    InnerGrid.RowDefinitions[2].Height = new GridLength(0);
                    _EditControl.WinForm.Visible = false;
                    SplitVert.Visibility = Visibility.Collapsed;
                    _ColumnWidth = InnerGrid.ColumnDefinitions[2].Width;
                    InnerGrid.ColumnDefinitions[2].Width = new GridLength(0);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setPreviewRight()
        {
            try
            {
                _RowHeight = InnerGrid.RowDefinitions[2].Height;

                Grid.SetRowSpan(grdBackup, 3);
                Grid.SetColumnSpan(grdBackup, 1);
                Grid.SetRowSpan(grdNotes, 3);
                Grid.SetColumnSpan(grdNotes, 1);

                SplitVert.Visibility = Visibility.Visible;
                SplitHorz.Visibility = Visibility.Collapsed;

                Grid.SetRow(BorderHost, 0);
                Grid.SetColumn(BorderHost, 2);
                Grid.SetRowSpan(BorderHost, 3);
                Grid.SetColumnSpan(BorderHost, 1);

                InnerGrid.ColumnDefinitions[2].Width = _ColumnWidth;
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
                _EditControl = new EditControl(brdHost);
                _EditPreview = _EditControl.EditBox;
                _EditPreview.ReadOnly = true;
                _EditPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
                if (PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor)
                {
                    _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.A,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.R, PNRuntimes.Instance.Settings.Config.CPPvwColor.G,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.B);
                }
                else
                {
                    var clr = PNSkinlessDetails.DefColor;
                    _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguageExceptMenus()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                PNLang.Instance.ApplyColumnsVisibilityMenuLanguage(_NotesColsMenu, grdNotes.Name);
                PNLang.Instance.ApplyColumnsVisibilityMenuLanguage(_BackColsMenu, grdBackup.Name);
                _CapTotal = PNLang.Instance.GetCaptionText("cp_total", "Total notes:");
                _CapCount = PNLang.Instance.GetCaptionText("cp_count", "Notes in group:");
                _CapGroup = PNLang.Instance.GetCaptionText("cp_group", "Group:");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void saveNoteAs(PNote note)
        {
            try
            {
                _TempNote = note;
                var dlgSaveAs = new WndSaveAs(note.Name, note.GroupId) { Owner = this };
                dlgSaveAs.SaveAsNoteNameSet += dlgSaveAs_SaveAsNoteNameSet;
                var showDialog = dlgSaveAs.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _TempNote = null;
            }
        }

        private void dlgSaveAs_SaveAsNoteNameSet(object sender, SaveAsNoteNameSetEventArgs e)
        {
            if (sender is WndSaveAs dlgSaveAs) dlgSaveAs.SaveAsNoteNameSet -= dlgSaveAs_SaveAsNoteNameSet;
            if (_TempNote == null) return;
            PNNotesOperations.ApplyBooleanChange(_TempNote, NoteBooleanTypes.Change, false,
                new SaveAsNoteNameSetEventArgs(e.Name, e.GroupId));
        }

        private void deleteNote(PNote note, NoteDeleteType type)
        {
            try
            {
                WndNote dlg = null;
                if (note.Visible)
                {
                    dlg = note.Dialog;
                }
                if (note.Favorite)
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, !note.Favorite, null);
                if (!PNNotesOperations.DeleteNote(type, note)) return;

                dlg?.Close();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private FavoriteStatus getFavoriteStatus()
        {
            try
            {
                var countYes = grdNotes.SelectedItems.OfType<CPNote>().Count(n => n.Favorites);
                var countNo = grdNotes.SelectedItems.OfType<CPNote>().Count(n => !n.Favorites);
                return ((countYes > 0 && countNo > 0) || (countYes == 0 && countNo == 0))
                    ? FavoriteStatus.Mix
                    : (countYes > 0 ? FavoriteStatus.Yes : FavoriteStatus.No);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return FavoriteStatus.Mix;
            }
        }

        private void clearPreview()
        {
            try
            {
                if (PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor)
                {
                    _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.A,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.R, PNRuntimes.Instance.Settings.Config.CPPvwColor.G,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.B);
                }
                else
                {
                    var clr = PNSkinlessDetails.DefColor;
                    _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
                }
                _EditPreview.Text = "";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotePreview(PNote note)
        {
            try
            {
                if (note == null) return;
                var gr = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                if ((note.PasswordString.Trim().Length > 0 || (gr != null && gr.PasswordString.Trim().Length > 0)) &&
                    PNRuntimes.Instance.Settings.Protection.DontShowContent)
                {
                    _EditPreview.Text = new string('*', PNRuntimes.Instance.Settings.Behavior.ContentColumnLength);
                }
                else
                {
                    if (note.FromDB)
                    {
                        var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                        if (!File.Exists(path)) return;
                        PNNotesOperations.LoadNoteFile(_EditPreview, path);
                    }
                    else
                    {
                        if (note.Dialog?.Edit != null)
                        {
                            _EditPreview.Rtf = note.Dialog.Edit.Rtf;
                        }
                    }

                    if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins) return;
                    if (PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor)
                    {
                        _EditControl.WinForm.BackColor =
                            System.Drawing.Color.FromArgb(PNRuntimes.Instance.Settings.Config.CPPvwColor.A,
                                PNRuntimes.Instance.Settings.Config.CPPvwColor.R, PNRuntimes.Instance.Settings.Config.CPPvwColor.G,
                                PNRuntimes.Instance.Settings.Config.CPPvwColor.B);
                    }
                    else
                    {
                        _EditControl.WinForm.BackColor = note.DrawingColor();
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadBackupPreview(string name)
        {
            try
            {
                clearPreview();
                var fileName = name + PNStrings.NOTE_BACK_EXTENSION;
                var path = Path.Combine(PNPaths.Instance.BackupDir, fileName);
                if (File.Exists(path))
                {
                    PNNotesOperations.LoadNoteFile(_EditPreview, path);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadGroups(PNGroup group, PNTreeItem local, bool byUser = false)
        {
            try
            {
                int count;
                switch (group.Id)
                {
                    case (int)SpecialGroups.AllGroups:
                        count = PNCollections.Instance.Notes.Count(nt => nt.GroupId != (int)SpecialGroups.RecycleBin);
                        break;
                    case (int)SpecialGroups.Favorites:
                        count = PNCollections.Instance.Notes.Count(nt => nt.Favorite && nt.GroupId != (int)SpecialGroups.RecycleBin);
                        break;
                    case (int)SpecialGroups.Backup:
                        var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                        var fis = di.GetFiles("*" + PNStrings.NOTE_BACK_EXTENSION);
                        count =
                            fis.Count(
                                f =>
                                    PNCollections.Instance.Notes.Any(
                                        n =>
                                            n.Id ==
                                            Path.GetFileNameWithoutExtension(f.Name)
                                                .Substring(0, Path.GetFileNameWithoutExtension(f.Name).IndexOf('_'))));
                        break;
                    default:
                        count = PNCollections.Instance.Notes.Count(nt => nt.GroupId == group.Id);
                        break;
                }

                PNTreeItem pnTreeItem;
                if (group.IsDefaultImage)
                {
                    pnTreeItem = group.PasswordString == ""
                        ? new PNTreeItem(group.ImageName,
                            group.Name + " (" + count + ")", group)
                        { IsExpanded = true }
                        : new PNTreeItem(group.ImageName,
                            group.Name + " (" + count + ") ***", group)
                        { IsExpanded = true };
                }
                else
                {
                    pnTreeItem = group.PasswordString == ""
                        ? new PNTreeItem(group.Image, group.Name + " (" + count + ")", group) { IsExpanded = true }
                        : new PNTreeItem(group.Image, group.Name + " (" + count + ") ***", group) { IsExpanded = true };
                }

                foreach (var chg in group.Subgroups.OrderBy(g => g.Name))
                {
                    loadGroups(chg, pnTreeItem);
                }

                if (local == null)
                {
                    _PGroups.Add(pnTreeItem);
                }
                else
                {
                    if (!byUser)
                    {
                        local.Items.Add(pnTreeItem);
                    }
                    else
                    {
                        insertGroup(group, local, pnTreeItem);
                    }
                }
                if (group.Id == PNRuntimes.Instance.Settings.Config.CPLastGroup)
                    pnTreeItem.IsSelected = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertGroup(PNGroup group, PNTreeItem parentItem, PNTreeItem insertedItem)
        {
            try
            {
                var inserted = false;
                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    if (!(parentItem.Items[i] is PNTreeItem pni)) continue;
                    var gr = pni.Tag as PNGroup;
                    if (gr == null) continue;
                    if (string.Compare(gr.Name, group.Name, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        parentItem.Items.Insert(i, insertedItem);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted)
                {
                    parentItem.Items.Add(insertedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private string getNoteContent(PNote note)
        {
            try
            {
                var content = "";
                var rtb = new System.Windows.Forms.RichTextBox();
                var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                if (File.Exists(path))
                {
                    PNNotesOperations.LoadNoteFile(rtb, path);
                    content = rtb.Text;
                    if (content.Length > PNRuntimes.Instance.Settings.Behavior.ContentColumnLength)
                    {
                        content = content.Substring(0, PNRuntimes.Instance.Settings.Behavior.ContentColumnLength);
                    }
                }
                content = content.Replace('\n', ' ');
                if (PNRuntimes.Instance.Settings.Protection.DontShowContent)
                {
                    var groupId = note.GroupId == (int)SpecialGroups.RecycleBin ? note.PrevGroupId : note.GroupId;
                    var group = PNCollections.Instance.Groups.GetGroupById(groupId);
                    if ((group != null && group.PasswordString != "") || note.PasswordString != "")
                    {
                        content = new string('*', PNRuntimes.Instance.Settings.Behavior.ContentColumnLength);
                    }
                }
                return content;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return "";
            }
        }

        private void deleteSelectedNotes(string[] ids)
        {
            try
            {
                InLongOperation = true;
                if (ids.Length == 0) return;
                var temp = PNCollections.Instance.Notes.Note(ids[0]);
                var type = PNNotesOperations.DeletionWarning(HotkeysStatic.LeftShiftDown(), ids.Length, temp);
                if (type == NoteDeleteType.None) return;
                Mouse.OverrideCursor = Cursors.Wait;
                foreach (var id in ids)
                {
                    var note = PNCollections.Instance.Notes.Note(id);
                    if (note == null) continue;
                    deleteNote(note, type);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                InLongOperation = false;
                Mouse.OverrideCursor = null;
            }
        }

        private void changeNotesGroup(PNGroup group, IEnumerable<string> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    var note = PNCollections.Instance.Notes.Note(id);
                    if (note == null) continue;
                    var oldGroup = note.GroupId;
                    note.GroupId = group.Id;
                    PNNotesOperations.SaveGroupChange(note);
                    if (note.Visible)
                    {
                        PNNotesOperations.ChangeNoteLookOnGroupChange(note.Dialog, group);
                    }
                    foreach (var node in tvwGroups.Items.OfType<PNTreeItem>())
                    {
                        if (changeNodeText(node, oldGroup))
                        {
                            break;
                        }
                    }
                    foreach (var node in tvwGroups.Items.OfType<PNTreeItem>())
                    {
                        if (changeNodeText(node, group.Id))
                        {
                            break;
                        }
                    }
                }
                updateStatusBar();
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
                applyAppearanceAdjustment(e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyAppearanceAdjustment(NoteAppearanceAdjustedEventArgs e)
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var isDialog = note.Visible && note.Dialog != null;
                note.Opacity = e.Opacity;
                note.CustomOpacity = e.CustomOpacity;
                if (isDialog)
                {
                    note.Dialog.Opacity = note.Opacity;
                }
                PNNotesOperations.SaveNoteOpacity(note);

                if (e.CustomSkinless)
                {
                    note.Skinless = (PNSkinlessDetails)e.Skinless.Clone();
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && isDialog)
                    {
                        note.Dialog.PHeader.SetPNFont(note.Skinless.CaptionFont);
                        note.Dialog.Foreground = new SolidColorBrush(note.Skinless.CaptionColor);
                        note.Dialog.Background = new SolidColorBrush(note.Skinless.BackColor);
                    }
                }
                else
                {
                    note.Skinless = null;
                    var group = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                    if (group != null)
                    {
                        if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && isDialog)
                        {
                            note.Dialog.PHeader.SetPNFont(group.Skinless.CaptionFont);
                            note.Dialog.Foreground = new SolidColorBrush(group.Skinless.CaptionColor);
                            note.Dialog.Background = new SolidColorBrush(group.Skinless.BackColor);
                        }
                    }
                }
                PNNotesOperations.SaveNoteSkinless(note);

                if (e.CustomSkin)
                {
                    note.Skin = e.Skin.Clone();
                    if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && isDialog)
                    {
                        PNSkinsOperations.ApplyNoteSkin(note.Dialog, note);
                    }
                }
                else
                {
                    note.Skin = null;
                    var group = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                    if (group != null)
                    {
                        if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins && isDialog)
                        {
                            PNSkinsOperations.ApplyNoteSkin(note.Dialog, note);
                        }
                    }
                }
                PNNotesOperations.SaveNoteSkin(note);
                loadNotePreview(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDaysOfWeekArray()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var values = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
                for (var i = 0; i < values.Length; i++)
                {
                    _DaysOfWeek[i] = new DayOfWeekStruct { DayOfW = values[i], Name = ci.DateTimeFormat.DayNames[i] };
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setColumnsVisibility(int groupId)
        {
            try
            {
                _AllowColumnsSave = false;
                switch (groupId)
                {
                    case (int)SpecialGroups.AllGroups:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted");
                        break;
                    case (int)SpecialGroups.RecycleBin:
                        toggleGroupsContextMenus(false);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Favorites", "Note_Group");
                        disableNotesColsContextItems("Note_Name", "Note_Favorites");
                        break;
                    case (int)SpecialGroups.Diary:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_Favorites",
                            "Note_Group", "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted", "Note_Favorites", "Note_Group",
                            "Note_PrevGroup");
                        break;
                    case (int)SpecialGroups.SearchResults:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted");
                        break;
                    case (int)SpecialGroups.Backup:
                        hideGridCols(grdBackup.View as GridView, _BackColsMenu,
                            _BackGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        break;
                    case (int)SpecialGroups.Favorites:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_Favorites",
                            "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted", "Note_Favorites");
                        break;
                    case (int)SpecialGroups.Incoming:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_Group",
                            "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted", "Note_Group", "Note_PrevGroup");
                        break;
                    default:
                        toggleGroupsContextMenus(true);
                        hideGridCols(grdNotes.View as GridView, _NotesColsMenu,
                            _NotesGridCols.Where(c => c.Visibility != Visibility.Visible).Select(c => c.Name));
                        hideDefaultGridCols(grdNotes.View as GridView, _NotesColsMenu, "Note_Deleted", "Note_Group",
                            "Note_PrevGroup");
                        disableNotesColsContextItems("Note_Name", "Note_Deleted", "Note_Group", "Note_PrevGroup");
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _AllowColumnsSave = true;
            }
        }

        private void recursivelyShowHideGroup(PNGroup group, bool show)
        {
            try
            {
                foreach (var g in group.Subgroups)
                {
                    recursivelyShowHideGroup(g, show);
                }
                PNNotesOperations.ShowHideSpecificGroup(group.Id, show);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void hideGridCols(GridView gridView, ContextMenu ctm, IEnumerable<string> hiddens)
        {
            try
            {
                var menuItems = ctm.Items.OfType<MenuItem>().ToArray();
                foreach (var col in gridView.Columns.OfType<FixedWidthColumn>())
                {
                    col.Visibility = Visibility.Visible;
                    var mi = menuItems.FirstOrDefault(m => (string)m.Tag == WPFGridViewHelper.GetColumnName(col));
                    if (mi != null)
                    {
                        mi.IsChecked = true;
                    }
                }
                var hiddenCols =
                    gridView.Columns.OfType<FixedWidthColumn>()
                        .Where(c => hiddens.Contains(WPFGridViewHelper.GetColumnName(c)));
                foreach (var col in hiddenCols)
                {
                    col.Visibility = Visibility.Collapsed;
                    var mi = menuItems.FirstOrDefault(m => (string)m.Tag == WPFGridViewHelper.GetColumnName(col));
                    if (mi != null)
                    {
                        mi.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void hideDefaultGridCols(GridView gridView, ContextMenu ctm, params string[] keys)
        {
            try
            {
                var menuItems = ctm.Items.OfType<MenuItem>().ToArray();
                foreach (
                    var col in
                    gridView.Columns.OfType<FixedWidthColumn>()
                        .Where(c => keys.Contains(WPFGridViewHelper.GetColumnName(c))))
                {
                    col.Visibility = Visibility.Collapsed;
                    var mi = menuItems.FirstOrDefault(m => (string)m.Tag == WPFGridViewHelper.GetColumnName(col));
                    if (mi != null)
                    {
                        mi.IsChecked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void disableNotesColsContextItems(params string[] disabled)
        {
            try
            {
                foreach (var mi in _NotesColsMenu.Items.OfType<MenuItem>())
                    mi.IsEnabled = true;
                var items =
                    _NotesColsMenu.Items.OfType<MenuItem>().Where(mi => disabled.Contains((string)mi.Tag));
                foreach (var mi in items)
                    mi.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void toggleGroupsContextMenus(bool value)
        {
            try
            {
                var menuItems = _NotesColsMenu.Items.OfType<MenuItem>().ToArray();
                var mi = menuItems.FirstOrDefault(m => (string)m.Tag == "Note_Group");
                if (mi != null)
                {
                    mi.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
                mi = menuItems.FirstOrDefault(m => (string)m.Tag == "Note_PrevGroup");
                if (mi != null)
                {
                    mi.Visibility = !value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addCPNote(PNote note)
        {
            try
            {
                var grCurrent = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                var grPrev = PNCollections.Instance.Groups.GetGroupById(note.PrevGroupId);
                var cpNote = new CPNote
                {
                    Name = note.Name,
                    Id = note.Id,
                    Priority = note.Priority,
                    Completed = note.Completed,
                    Protected = note.Protected,
                    Group = grCurrent != null ? grCurrent.Name : "",
                    PrevGroup = grPrev != null ? grPrev.Name : "",
                    Password =
                        note.PasswordString != ""
                            ? PasswordProtectionMode.Note
                            : (grCurrent != null && grCurrent.PasswordString != ""
                                ? PasswordProtectionMode.Group
                                : PasswordProtectionMode.None),
                    Pinned = note.Pinned,
                    Favorites = note.Favorite,
                    SentReceived = note.SentReceived,
                    Created = note.DateCreated,
                    Saved = note.DateSaved,
                    Deleted = note.DateDeleted,
                    Tags = note.Tags.ToCommaSeparatedString(),
                    Content = getNoteContent(note),
                    SentTo = note.SentTo,
                    SentAt = note.DateSent,
                    ReceivedFrom = note.ReceivedFrom,
                    ReceivedAt = note.DateReceived,
                    Encrypted = note.Scrambled,
                    ScheduleType = PNLang.Instance.GetNoteScheduleDescription(note.Schedule, _DaysOfWeek),
                    IdGroup = note.GroupId,
                    IdPrevGroup = note.PrevGroupId,
                    State = getNoteState(note)
                };
                _Notes.Add(cpNote);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private NoteState getNoteState(PNote note)
        {
            try
            {
                if (!note.FromDB)
                {
                    if (note.Changed && (note.Schedule != null && note.Schedule.Type != ScheduleType.None))
                        return NoteState.NewChangedScheduled;
                    if (note.Changed)
                        return NoteState.NewChanged;
                    if (note.Schedule != null && note.Schedule.Type != ScheduleType.None)
                        return NoteState.NewScheduled;
                    return NoteState.New;
                }
                if (note.Visible)
                {
                    if (note.Changed && (note.Schedule != null && note.Schedule.Type != ScheduleType.None))
                        return NoteState.OrdinalVisibleChangedScheduled;
                    if (note.Changed)
                        return NoteState.OrdinalVisibleChanged;
                    if (note.Schedule != null && note.Schedule.Type != ScheduleType.None)
                        return NoteState.OrdinalVisibleScheduled;
                    return NoteState.OrdinalVisible;
                }
                if (note.Changed && (note.Schedule != null && note.Schedule.Type != ScheduleType.None))
                    return NoteState.OrdinalHiddenChangedScheduled;
                if (note.Changed)
                    return NoteState.OrdinalHiddenChanged;
                if (note.Schedule != null && note.Schedule.Type != ScheduleType.None)
                    return NoteState.OrdinalHiddenScheduled;
                return NoteState.OrdinalHidden;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return NoteState.OrdinalHidden;
            }
        }

        private void loadNotes()
        {
            try
            {
                foreach (var n in PNCollections.Instance.Notes)
                {
                    addCPNote(n);
                }
                grdNotes.ItemsSource = _Notes;
                var view = (CollectionView)CollectionViewSource.GetDefaultView(grdNotes.ItemsSource);
                view.Filter = notesFilter;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadBackUpNotes()
        {
            try
            {
                _BackNotes.Clear();
                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                var fis = di.GetFiles("*" + PNStrings.NOTE_BACK_EXTENSION);
                foreach (var fi in fis)
                {
                    var backName = Path.GetFileNameWithoutExtension(fi.Name);
                    var id = backName.Substring(0, backName.IndexOf('_'));
                    var note = PNCollections.Instance.Notes.Note(id);
                    if (note != null)
                    {
                        _BackNotes.Add(new CPBackNote
                        {
                            Id = note.Id,
                            Created = fi.LastWriteTime,
                            Name = backName,
                            Original = note.Name,
                            FileName = fi.FullName
                        });
                    }
                }
                grdBackup.ItemsSource = _BackNotes;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNote getSelectedNote()
        {
            try
            {
                return !(grdNotes.SelectedItem is CPNote item) ? null : PNCollections.Instance.Notes.FirstOrDefault(n => n.Id == item.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private List<PNote> getSelectedNotes()
        {
            try
            {
                return
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(cpn => PNCollections.Instance.Notes.Note(cpn.Id))
                        .Where(note => note != null).ToList();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNGroup getSelectedGroup()
        {
            try
            {
                if (!(tvwGroups.SelectedItem is PNTreeItem item)) return null;
                return item.Tag as PNGroup;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
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

        private bool notesFilter(object obj)
        {
            try
            {
                if (!(obj is CPNote note)) return false;
                var gr = getSelectedGroup();
                if (gr == null) return false;
                switch (gr.Id)
                {
                    case (int)SpecialGroups.AllGroups:
                        return note.IdGroup != (int)SpecialGroups.RecycleBin;
                    case (int)SpecialGroups.Favorites:
                        return note.Favorites && note.IdGroup != (int)SpecialGroups.RecycleBin;
                    case (int)SpecialGroups.SearchResults:
                        return _SearchResults.Any(s => s == note.Id);
                    default:
                        return note.IdGroup == gr.Id;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private VisibleHidden checkGroupVisibility()
        {
            try
            {
                var vh = VisibleHidden.None;
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(n => PNCollections.Instance.Notes.FirstOrDefault(nt => nt.Id == n.Id))
                        .Where(note => note != null))
                {
                    if (note.Visible)
                        vh |= VisibleHidden.Visible;
                    else
                        vh |= VisibleHidden.Hidden;
                }
                return vh;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return VisibleHidden.None;
            }
        }

        private VisibleHidden checkBranchVisibility(PNGroup png)
        {
            try
            {
                var vh = VisibleHidden.None;
                var notes = PNCollections.Instance.Notes.Where(n => n.GroupId == png.Id).ToArray();
                if (notes.Any(n => !n.Visible))
                {
                    vh |= VisibleHidden.Hidden;
                }
                if (notes.Any(n => n.Visible))
                {
                    vh |= VisibleHidden.Visible;
                }
                return png.Subgroups.Aggregate(vh, (current, g) => current | checkBranchVisibility(g));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return VisibleHidden.None;
            }
        }

        private void prepareGridColumns(bool forceRearrange = false)
        {
            try
            {
                _NotesGridCols.Clear();
                _NotesGridSorts.Clear();
                _BackGridCols.Clear();
                _BackGridSorts.Clear();

                PNData.LoadGridColumns(grdNotes.View as GridView, _NotesGridCols, _NotesGridSorts, "grdNotes");
                PNData.LoadGridColumns(grdBackup.View as GridView, _BackGridCols, _BackGridSorts, "grdBackup");

                foreach (var mi in _NotesColsMenu.Items.OfType<MenuItem>())
                {
                    var gc = _NotesGridCols.FirstOrDefault(c => c.Name == mi.Tag.ToString());
                    if (gc == null) continue;
                    mi.IsChecked = gc.Visibility == Visibility.Visible;
                }
                foreach (var mi in _BackColsMenu.Items.OfType<MenuItem>())
                {
                    var gc = _BackGridCols.FirstOrDefault(c => c.Name == mi.Tag.ToString());
                    if (gc == null) continue;
                    mi.IsChecked = gc.Visibility == Visibility.Visible;
                }

                if (grdNotes.View is GridView gridView)
                {
                    arrangeGridColumns(gridView, NotesColumns_CollectionChanged, forceRearrange);
                }
                gridView = grdBackup.View as GridView;
                if (gridView != null)
                {
                    arrangeGridColumns(gridView, BackColumns_CollectionChanged, forceRearrange);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updateNotesAfterGroupPasswordChange(PNGroup group)
        {
            try
            {
                var notes = PNCollections.Instance.Notes.Where(n => n.GroupId == group.Id).ToArray();
                foreach (var note in notes)
                {
                    var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                    if (cpn == null) continue;
                    cpn.Password = note.PasswordString != ""
                        ? PasswordProtectionMode.Note
                        : (group.PasswordString != ""
                            ? PasswordProtectionMode.Group
                            : PasswordProtectionMode.None);
                    cpn.Content = getNoteContent(note);
                    var selectedNote = getSelectedNote();
                    if (selectedNote != null && selectedNote.Id == cpn.Id)
                    {
                        loadNotePreview(selectedNote);
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void arrangeGridColumns(GridView gridView, NotifyCollectionChangedEventHandler handler,
            bool forceRearrange)
        {
            try
            {
                gridView.Columns.CollectionChanged -= handler;
                gridView.Columns.CollectionChanged += handler;
                if (gridView.Columns.OfType<FixedWidthColumn>().All(c => c.OriginalIndex == c.DisplayIndex) &&
                    !forceRearrange) return;
                var savedCols =
                    gridView.Columns.OfType<FixedWidthColumn>().OrderBy(c => c.DisplayIndex).ToArray();
                gridView.Columns.Clear();
                foreach (var col in savedCols)
                {
                    gridView.Columns.Add(col);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkCustomColorMenu(ItemsControl parentItem)
        {
            try
            {
                if (parentItem.Items[0] is MenuItem mnuUseCustColor)
                {
                    mnuUseCustColor.IsChecked = PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkIgnoreCaseMenu(ItemsControl parentItem)
        {
            try
            {
                if (parentItem.Items[8] is MenuItem mnuIgnoreCase)
                {
                    mnuIgnoreCase.IsChecked = PNRuntimes.Instance.Settings.Config.CPIgnoreCase;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDiaryMenu(ItemsControl parentItem)
        {
            try
            {
                for (var i = parentItem.Items.Count - 1; i > 0; i--)
                {
                    ((MenuItem)parentItem.Items[i]).Click -= Diary_MenuClick;
                    parentItem.Items.RemoveAt(i);
                }
                var diaries =
                    PNCollections.Instance.Notes.Where(n => n.GroupId == (int)SpecialGroups.Diary && n.DateCreated < DateTime.Today);
                var pNotes = diaries.ToList().OrderByDescending(n => n.DateCreated);
                foreach (var ti in pNotes.Select(n => new MenuItem { Header = n.Name, Tag = n.Id }))
                {
                    ti.Click += Diary_MenuClick;
                    parentItem.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkIncBinInSearch(ItemsControl parentItem)
        {
            try
            {
                foreach (var it in parentItem.Items.OfType<MenuItem>())
                {
                    if (it.Name == "mnuIncBinInQSearch")
                    {
                        it.IsChecked = _IncBinInSearch;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createShowByTagsMenu(ItemsControl parentItem)
        {
            try
            {
                for (var i = parentItem.Items.Count - 1; i > 0; i--)
                {
                    ((MenuItem)parentItem.Items[i]).Click -= ShowByTag_MenuClick;
                    parentItem.Items.RemoveAt(i);
                }
                foreach (var t in PNCollections.Instance.Tags)
                {
                    var ti = new MenuItem { Header = t };
                    ti.Click += ShowByTag_MenuClick;
                    parentItem.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createHideByTagsMenu(ItemsControl parentItem)
        {
            try
            {
                for (var i = parentItem.Items.Count - 1; i > 0; i--)
                {
                    ((MenuItem)parentItem.Items[i]).Click -= HideByTag_MenuClick;
                    parentItem.Items.RemoveAt(i);
                }
                foreach (var t in PNCollections.Instance.Tags)
                {
                    var ti = new MenuItem { Header = t };
                    ti.Click += HideByTag_MenuClick;
                    parentItem.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createExternalsMenu(ItemsControl parentMenu)
        {
            try
            {
                foreach (var ti in parentMenu.Items.OfType<MenuItem>())
                {
                    ti.Click -= Run_MenuClick;
                }
                parentMenu.Items.Clear();
                foreach (
                    var ti in
                    PNCollections.Instance.Externals.Select(
                        ext => new MenuItem { Header = ext.Name, Tag = ext.Program + "|" + ext.CommandLine }))
                {
                    ti.Click += Run_MenuClick;
                    parentMenu.Items.Add(ti);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setSwitches(ItemsControl parentMenu)
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var menus = parentMenu.Items.OfType<MenuItem>().ToArray();
                var item = menus.FirstOrDefault(m => m.Name == "mnuOnTop");
                if (item != null)
                {
                    item.IsChecked = note.Topmost;
                }
                item = menus.FirstOrDefault(m => m.Name == "mnuToggleHighPriority");
                if (item != null)
                {
                    item.IsChecked = note.Priority;
                }
                item = menus.FirstOrDefault(m => m.Name == "mnuToggleProtectionMode");
                if (item != null)
                {
                    item.IsChecked = note.Protected;
                }
                item = menus.FirstOrDefault(m => m.Name == "mnuMarkAsComplete");
                if (item != null)
                {
                    item.IsChecked = note.Completed;
                }
                var it1 = menus.FirstOrDefault(m => m.Name == "mnuSetPassword");
                var it2 = menus.FirstOrDefault(m => m.Name == "mnuRemovePassword");
                if (it1 != null && it2 != null)
                {
                    if (note.PasswordString.Length > 0)
                    {
                        it1.Visibility = Visibility.Collapsed;
                        it2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        it1.Visibility = Visibility.Visible;
                        it2.Visibility = Visibility.Collapsed;
                    }
                }
                it1 = menus.FirstOrDefault(m => m.Name == "mnuPin");
                it2 = menus.FirstOrDefault(m => m.Name == "mnuUnpin");
                if (it1 != null && it2 != null)
                {
                    if (note.Pinned)
                    {
                        it1.Visibility = Visibility.Collapsed;
                        it2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        it1.Visibility = Visibility.Visible;
                        it2.Visibility = Visibility.Collapsed;
                    }
                }
                it1 = menus.FirstOrDefault(m => m.Name == "mnuScramble");
                it2 = menus.FirstOrDefault(m => m.Name == "mnuUnscramble");
                if (it1 != null && it2 != null)
                {
                    if (note.Scrambled)
                    {
                        it1.Visibility = Visibility.Collapsed;
                        it2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        it1.Visibility = Visibility.Visible;
                        it2.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteGroup(PNGroup group, List<int> groupsIds, List<string> notesIds)
        {
            try
            {
                groupsIds.Add(group.Id);
                //collect all notes belong to png
                notesIds.AddRange(PNCollections.Instance.Notes.Where(n => n.GroupId == group.Id).Select(n => n.Id));
                for (var i = group.Subgroups.Count - 1; i >= 0; i--)
                {
                    deleteGroup(group.Subgroups[i], groupsIds, notesIds);
                    PNCollections.Instance.Groups.RemoveAll(g => g.Id == group.Subgroups[i].Id);
                    group.Subgroups.RemoveAll(g => g.Id == group.Subgroups[i].Id);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void deleteGroupOnDrop(PNGroup pnGroup, PNTreeItem itemToDelete = null)
        {
            try
            {
                var message = PNLang.Instance.GetMessageText("delete_group_warning",
                    "Are you sure you want to delete this group - " + PNStrings.PLACEHOLDER1 +
                    " - and all included subgroups and notes?");
                message = message.Replace(PNStrings.PLACEHOLDER1, pnGroup.Name);
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                var groupsIds = new List<int>();
                var notesIds = new List<string>();
                deleteGroup(pnGroup, groupsIds, notesIds);
                var parent = PNCollections.Instance.Groups.GetGroupById(pnGroup.ParentId);
                if (parent != null)
                {
                    parent.Subgroups.RemoveAll(g => g.Id == pnGroup.Id);
                }
                PNCollections.Instance.Groups.RemoveAll(g => g.Id == pnGroup.Id);

                if (groupsIds.Count > 0)
                {
                    PNData.DeleteGroups(groupsIds);
                }
                foreach (var id in notesIds)
                {
                    var note = PNCollections.Instance.Notes.Note(id);
                    deleteNote(note, note.FromDB ? NoteDeleteType.Bin : NoteDeleteType.Complete);
                }

                if (itemToDelete == null) return;
                if (itemToDelete.ItemParent == null)
                {
                    _PGroups.Remove(itemToDelete);
                }
                else
                {
                    itemToDelete.ItemParent.Items.Remove(itemToDelete);
                }
                if (PNWindows.Instance.FormSettings != null)
                {
                    PNWindows.Instance.FormSettings.GroupDelete(pnGroup.Id);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Run_MenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                var args = ((string)ti.Tag).Split('|');
                PNStatic.RunExternalProgram(args[0], args[1]);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Diary_MenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                var id = (string)ti.Tag;
                var note = PNCollections.Instance.Notes.Note(id);
                if (note == null) return;
                PNNotesOperations.ShowHideSpecificNote(note, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ShowByTag_MenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                var notes =
                    PNCollections.Instance.Notes.Where(
                        n => n.Tags.Contains(ti.Header.ToString()) && n.GroupId != (int)SpecialGroups.RecycleBin);
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

        private void HideByTag_MenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is MenuItem ti)) return;
                var notes =
                    PNCollections.Instance.Notes.Where(
                        n => n.Tags.Contains(ti.Header.ToString()) && n.GroupId != (int)SpecialGroups.RecycleBin);
                foreach (var note in notes)
                {
                    PNNotesOperations.ShowHideSpecificNote(note, false);
                }
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
                    if (PNCollections.Instance.Contacts.Any(c => c.Name == e.Contact.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("contact_exists",
                            "Contact with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
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
                {
                    PNWindows.Instance.FormSettings.ContactAction(e.Contact, e.Mode);
                }
                PNData.SaveContacts();
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
                    if (PNCollections.Instance.ContactGroups.Any(g => g.Name == e.Group.Name))
                    {
                        var message = PNLang.Instance.GetMessageText("group_exists",
                            "Contacts group with this name already exists");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                        e.Accepted = false;
                        return;
                    }
                    PNCollections.Instance.ContactGroups.Add(e.Group);
                }
                else
                {
                    var g = PNCollections.Instance.ContactGroups.FirstOrDefault(gr => gr.Id == e.Group.Id);
                    if (g != null)
                    {
                        g.Name = e.Group.Name;
                    }
                }
                if (PNWindows.Instance.FormSettings != null)
                {
                    PNWindows.Instance.FormSettings.ContactGroupAction(e.Group, e.Mode);
                }
                PNData.SaveContactGroups();
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
                    if (sender is WndSelectGroups gd)
                    {
                        gd.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                    }
                }
                var notesToSend = getSelectedNotes();
                if (notesToSend == null) return;
                if (e.Contacts.All(c => c == null)) return;

                var clientRunner = new PNWCFClientRunner();
                clientRunner.NotesSendNotification += clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete += clientRunner_NotesSendComplete;
                Task.Factory.StartNew(
                    () => clientRunner.SendNotesToMultipleContacts(notesToSend, e.Contacts.Where(c => c != null)));
                PNWindows.Instance.FormMain.ShowSendProgressBaloon();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clientRunner_NotesSendComplete(object sender, EventArgs e)
        {
            if (sender is PNWCFClientRunner clientRunner)
            {
                clientRunner.NotesSendNotification -= clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete -= clientRunner_NotesSendComplete;
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

        private void notePasswordSet(object sender, PasswordChangedEventArgs e)
        {
            try
            {
                if (sender is WndPasswordCreate pwrdCrweate) pwrdCrweate.PasswordChanged -= notePasswordSet;
                var note = getSelectedNote();
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, true, e.NewPassword);
                if (note.Visible)
                {
                    note.Dialog?.PFooter.SetMarkButtonVisibility(MarkType.Password, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void notePasswordRemoved(object sender, EventArgs e)
        {
            try
            {
                if (sender is WndPasswordDelete pwrdDelete) pwrdDelete.PasswordDeleted -= notePasswordRemoved;
                var note = getSelectedNote();
                if (note == null) return;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Password, false, null);
                if (note.Visible)
                {
                    note.Dialog?.PFooter.SetMarkButtonVisibility(MarkType.Password, false);
                }
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
                if (sender is WndPin d)
                {
                    d.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
                }
                var note = getSelectedNote();
                if (note != null)
                {
                    note.PinText = e.PinText;
                    note.PinClass = e.PinClass;
                    note.Pinned = true;
                    PNNotesOperations.SaveNotePin(note);
                    note.Dialog?.PFooter.SetMarkButtonVisibility(MarkType.Pin, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dpc_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            try
            {
                var password = e.NewPassword;
                if (sender.GetType() == typeof(WndPasswordCreate))
                {
                    if (sender is WndPasswordCreate dlgPasswordCreate)
                        dlgPasswordCreate.PasswordChanged -= dpc_PasswordChanged;
                    PNRuntimes.Instance.Settings.Protection.PasswordString = password;
                }
                else
                {
                    if (sender is WndPasswordChange dlgPasswordChange)
                        dlgPasswordChange.PasswordChanged -= dpc_PasswordChanged;
                    if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                    {
                        PNNotesOperations.DecryptAllNotes(PNRuntimes.Instance.Settings.Protection.PasswordString);
                    }
                    PNRuntimes.Instance.Settings.Protection.PasswordString = password;
                    if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                    {
                        PNNotesOperations.EncryptAllNotes(PNRuntimes.Instance.Settings.Protection.PasswordString);
                    }
                }
                PNData.SavePassword();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dpd_PasswordDeleted(object sender, EventArgs e)
        {
            try
            {
                if (sender is WndPasswordDelete dlgPasswordDelete)
                    dlgPasswordDelete.PasswordDeleted -= dpd_PasswordDeleted;
                if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                {
                    PNNotesOperations.DecryptAllNotes(PNRuntimes.Instance.Settings.Protection.PasswordString);
                }
                PNRuntimes.Instance.Settings.Protection.PasswordString = "";
                PNData.SavePassword();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Dialog boxes events procedures

        private void pwrdCreate_PasswordChanged(object sender, PasswordChangedEventArgs e)
        {
            try
            {
                if (sender is WndPasswordCreate pwrdCreate) pwrdCreate.PasswordChanged -= pwrdCreate_PasswordChanged;
                var group = PNCollections.Instance.Groups.GetGroupById(_SelectedGroup);
                if (group != null)
                {
                    group.PasswordString = e.NewPassword;
                    PNData.SaveGroupPassword(group);
                    changeNodeText(_LastTreeItem, group.Id);
                    updateNotesAfterGroupPasswordChange(group);
                }
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
                var group = PNCollections.Instance.Groups.GetGroupById(_SelectedGroup);
                if (group != null)
                {
                    group.PasswordString = "";
                    PNData.SaveGroupPassword(group);
                    changeNodeText(_LastTreeItem, group.Id);
                    updateNotesAfterGroupPasswordChange(group);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgGroup_GroupChanged(object sender, GroupChangedEventArgs e)
        {
            try
            {
                var id = -1;
                if (sender is WndNewGroup dlgNewGroup) dlgNewGroup.GroupChanged -= dlgGroup_GroupChanged;
                PNGroup pnGroup = null, groupToSettingsScreen = null;
                switch (_NewGroupTarget)
                {
                    case NewGroupTarget.SubGroup:
                        pnGroup = e.TreeItem.Tag as PNGroup ?? getSelectedGroup();
                        break;
                    case NewGroupTarget.TopLevel:
                        if (tvwGroups.Items[0] is PNTreeItem item)
                            pnGroup = item.Tag as PNGroup;
                        break;
                }
                if (pnGroup != null)
                {
                    var pnTreeItem = e.TreeItem;
                    if (e.Mode == AddEditMode.Add)
                    {
                        var gr = (PNGroup)e.Group.Clone();
                        gr.ParentId = pnGroup.Id;
                        gr.Id = PNData.GetNewGroupId();
                        pnGroup.Subgroups.Add(gr);
                        PNData.InsertNewGroup(gr);
                        if (_NewGroupTarget == NewGroupTarget.SubGroup)
                        {
                            loadGroups(gr, pnTreeItem, true);
                            if (pnTreeItem != null) pnTreeItem.IsExpanded = true;
                        }
                        else
                        {
                            if (tvwGroups.Items[0] is PNTreeItem item)
                            {
                                loadGroups(gr, item, true);
                                item.IsExpanded = true;
                            }
                        }
                        id = gr.ParentId;
                        groupToSettingsScreen = gr;
                    }
                    else
                    {
                        e.Group.CopyTo(pnGroup);
                        pnGroup.Name = e.Group.Name;
                        changeNodeText(pnTreeItem, pnGroup.Id);
                        if (!Equals(pnTreeItem.Image, pnGroup.Image))
                        {
                            if (!pnGroup.IsDefaultImage)
                                pnTreeItem.Image = pnGroup.Image;
                            else
                                pnTreeItem.SetImageResource(pnGroup.ImageName);
                        }
                        replaceGroupOnTree(pnTreeItem);
                        updateStatusBar();
                        PNData.SaveGroupChanges(pnGroup);
                        var selectedNote = getSelectedNote();
                        if (selectedNote != null)
                            loadNotePreview(selectedNote);
                        id = pnGroup.Id;
                        groupToSettingsScreen = pnGroup;
                    }
                }
                _NewGroupTarget = NewGroupTarget.None;
                if (PNWindows.Instance.FormSettings != null)
                {
                    PNWindows.Instance.FormSettings.GroupAddEdit(groupToSettingsScreen, id, e.Mode);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void replaceGroupOnTree(PNTreeItem node)
        {
            try
            {
                ItemsControl parentItem;
                if (node.ItemParent != null)
                    parentItem = node.ItemParent;
                else
                    parentItem = (TreeViewItem)tvwGroups.Items[0];

                var isSelected = node.IsSelected;

                parentItem.Items.Remove(node);

                if (parentItem.Items.Count == 0)
                {
                    //just put the item back
                    parentItem.Items.Add(node);
                    if (isSelected)
                        node.IsSelected = true;
                    return;
                }

                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    if (!(parentItem.Items[i] is PNTreeItem ti)) continue;
                    var gr = ti.Tag as PNGroup;
                    if (gr == null) continue;
                    //search for right alphabetical place
                    if (string.Compare(gr.Name, node.Text, StringComparison.OrdinalIgnoreCase) <= 0) continue;
                    parentItem.Items.Insert(i, node);
                    if (isSelected)
                        node.IsSelected = true;
                    return;
                }

                //just add the node to the end of parent's children
                parentItem.Items.Add(node);
                if (isSelected)
                    node.IsSelected = true;
                //((TreeViewItem)tvwGroups.Items[0]).Items.Add(node);
                //if (isSelected)
                //    node.IsSelected = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Grid view columns events handlers

        private void NotesColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (e.Action != NotifyCollectionChangedAction.Move) return;
                if (!(sender is GridViewColumnCollection cols)) return;
                PNData.SaveGridColumnsOrder(cols, "grdNotes");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BackColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (e.Action != NotifyCollectionChangedAction.Move) return;
                if (!(sender is GridViewColumnCollection cols)) return;
                PNData.SaveGridColumnsOrder(cols, "grdBackup");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Window procedures

        private void DlgCP_SourceInitialized(object sender, EventArgs e)
        {
            var handle = (new WindowInteropHelper(this)).Handle;
            _HwndSource = HwndSource.FromHwnd(handle);
        }

        private void DlgCP_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNWindows.Instance.FormCP = this;

                //hide possible hidden menus
                var hiddens = PNCollections.Instance.HiddenMenus.Where(hm => hm.Type == MenuType.ControlPanel).ToArray();
                PNStatic.HideMenus(ctmList, hiddens);
                HideToolbarButtons(hiddens);

                _NotesColsMenu = (ContextMenu)grdNotes.TryFindResource("ListNotesHeaderContextMenu") ??
                                 new ContextMenu();
                _BackColsMenu = (ContextMenu)grdBackup.TryFindResource("ListBackHeaderContextMenu") ??
                                new ContextMenu();

                applyLanguageExceptMenus();
                createDaysOfWeekArray();

                PNWindows.Instance.FormMain.NoteBooleanChanged += FormMain_NoteBooleanChanged;
                PNWindows.Instance.FormMain.NoteDateChanged += FormMain_NoteDateChanged;
                PNWindows.Instance.FormMain.NoteGroupChanged += FormMain_NoteGroupChanged;
                PNWindows.Instance.FormMain.NoteNameChanged += FormMain_NoteNameChanged;
                PNWindows.Instance.FormMain.NoteSendReceiveStatusChanged += FormMain_NoteSendReceiveStatusChanged;
                PNWindows.Instance.FormMain.NoteTagsChanged += FormMain_NoteTagsChanged;
                PNWindows.Instance.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.NewNoteCreated += FormMain_NewNoteCreated;
                PNWindows.Instance.FormMain.NoteDeletedCompletely += FormMain_NoteDeletedCompletely;
                PNWindows.Instance.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                PNWindows.Instance.FormMain.NotesReceived += FormMain_NotesReceived;
                PNWindows.Instance.FormMain.MenusOrderChanged += FormMain_MenusOrderChanged;
                PNWindows.Instance.FormMain.ContentDisplayChanged += FormMain_ContentDisplayChanged;
                if (PNRuntimes.Instance.Settings.Behavior.BigIconsOnCP)
                    SetToolbarIcons(true);

                prepareGridColumns();

                loadNotes();

                var grAll = PNCollections.Instance.Groups.GetGroupById((int)SpecialGroups.AllGroups);
                loadGroups(grAll, null);
                foreach (
                    var gr in PNCollections.Instance.Groups.Where(g => g.Id != (int)SpecialGroups.AllGroups).OrderBy(g => g.Name))
                {
                    loadGroups(gr, null);
                }
                //foreach (var gr in PNCollections.Instance.Groups)
                //{
                //    loadGroups(gr, null);
                //}
                tvwGroups.ItemsSource = _PGroups;

                setSelectedGroup(PNRuntimes.Instance.Settings.Config.CPLastGroup);

                if (PNRuntimes.Instance.Settings.Config.CPSize != Size.Empty)
                {
                    try
                    {
                        this.SetSize(PNRuntimes.Instance.Settings.Config.CPSize);
                    }
                    catch (ArgumentException)
                    {
                        PNRuntimes.Instance.Settings.Config.CPSize = new Size(100, 600);
                        this.SetSize(PNRuntimes.Instance.Settings.Config.CPSize);
                    }
                }
                else
                {
                    WindowState = WindowState.Maximized;
                }
                if (WindowState != WindowState.Maximized)
                {
                    if (PNRuntimes.Instance.Settings.Config.CPLocation != default(Point))
                    {
                        if (Math.Abs(PNRuntimes.Instance.Settings.Config.CPLocation.X - (-1)) > double.Epsilon)
                        {
                            try
                            {
                                var wa = PNStatic.AllScreensBounds();
                                if (!wa.Contains(PNRuntimes.Instance.Settings.Config.CPLocation))
                                    PNRuntimes.Instance.Settings.Config.CPLocation = new Point((wa.Width - Width) / 2,
                                        (wa.Height - Height) / 2);
                                else
                                    this.SetLocation(PNRuntimes.Instance.Settings.Config.CPLocation);
                            }
                            catch (ArgumentException)
                            {
                                PNRuntimes.Instance.Settings.Config.CPLocation = new Point(0, 0);
                                this.SetLocation(PNRuntimes.Instance.Settings.Config.CPLocation);
                            }
                        }
                    }
                }

                if (PNRuntimes.Instance.Settings.Config.CPPvwRight)
                {
                    cmdPreviewRight.IsChecked = mnuPreviewRight.IsChecked = true;
                    setPreviewRight();
                }
                if (!PNRuntimes.Instance.Settings.Config.CPPvwShow)
                {
                    cmdPreview.IsChecked = mnuPreview.IsChecked = false;
                    hidePreview();
                }
                if (!PNRuntimes.Instance.Settings.Config.CPGroupsShow)
                {
                    cmdShowGroups.IsChecked = mnuShowGroups.IsChecked = false;
                    hideGroupsPane();
                }

                _AllowColumnsSave = true;

                FlowDirection = PNLang.Instance.GetFlowDirection();
                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgCP_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                SaveCPProperties();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgCP_Activated(object sender, EventArgs e)
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

        private void DlgCP_Closed(object sender, EventArgs e)
        {
            try
            {
                SaveCPProperties();
                //var gridView = grdNotes.View as GridView;
                //if (gridView != null)
                //{
                PNData.SaveGridColumnsWidth(_NotesGridCols, "grdNotes");
                //}
                //gridView = grdBackup.View as GridView;
                //if (gridView != null)
                //{
                PNData.SaveGridColumnsWidth(_BackGridCols, "grdBackup");
                //}
                PNWindows.Instance.FormCP = null;
                PNWindows.Instance.FormMain.NoteBooleanChanged -= FormMain_NoteBooleanChanged;
                PNWindows.Instance.FormMain.NoteDateChanged -= FormMain_NoteDateChanged;
                PNWindows.Instance.FormMain.NoteGroupChanged -= FormMain_NoteGroupChanged;
                PNWindows.Instance.FormMain.NoteNameChanged -= FormMain_NoteNameChanged;
                PNWindows.Instance.FormMain.NoteSendReceiveStatusChanged -= FormMain_NoteSendReceiveStatusChanged;
                PNWindows.Instance.FormMain.NoteTagsChanged -= FormMain_NoteTagsChanged;
                PNWindows.Instance.FormMain.LanguageChanged -= FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.NewNoteCreated -= FormMain_NewNoteCreated;
                PNWindows.Instance.FormMain.NoteDeletedCompletely -= FormMain_NoteDeletedCompletely;
                PNWindows.Instance.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
                PNWindows.Instance.FormMain.NotesReceived -= FormMain_NotesReceived;
                PNWindows.Instance.FormMain.MenusOrderChanged -= FormMain_MenusOrderChanged;
                PNWindows.Instance.FormMain.ContentDisplayChanged -= FormMain_ContentDisplayChanged;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Main form events handlers

        void FormMain_ContentDisplayChanged(object sender, EventArgs e)
        {
            try
            {
                var selectedNote = getSelectedNote();
                if (selectedNote != null)
                {
                    loadNotePreview(selectedNote);
                }
                var group = getSelectedGroup();
                if (group == null) return;
                var notes = PNCollections.Instance.Notes.Where(n => n.GroupId == group.Id);
                foreach (var note in notes)
                {
                    var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                    if (cpn == null) continue;
                    cpn.Content = getNoteContent(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_MenusOrderChanged(object sender, MenusOrderChangedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NotesReceived(object sender, EventArgs e)
        {
            try
            {
                changeSpecialNodeText((int)SpecialGroups.Incoming);
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
                if (!(sender is PNote note)) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                cpn.ScheduleType = PNLang.Instance.GetNoteScheduleDescription(note.Schedule, _DaysOfWeek);
                cpn.State = getNoteState(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteDeletedCompletely(object sender, NoteDeletedCompletelyEventArgs e)
        {
            try
            {
                var cpn = _Notes.FirstOrDefault(n => n.Id == e.Id);
                if (cpn == null) return;
                _Notes.Remove(cpn);
                foreach (var item in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    if (changeNodeText(item, e.GroupId)) break;
                }
                changeSpecialNodeText((int)SpecialGroups.Favorites);
                changeSpecialNodeText((int)SpecialGroups.Backup);
                changeSpecialNodeText((int)SpecialGroups.SearchResults);
                //changeSpecialNodeText((int)SpecialGroups.Diary);
                updateStatusBar();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NewNoteCreated(object sender, NewNoteCreatedEventArgs e)
        {
            try
            {
                addCPNote(e.Note);
                if (e.Note.GroupId != (int)SpecialGroups.Diary)
                    changeNodeText(tvwGroups.Items[0] as PNTreeItem, e.Note.GroupId);
                else
                    changeSpecialNodeText((int)SpecialGroups.Diary);
                updateStatusBar();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteTagsChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                cpn.Tags = note.Tags.ToCommaSeparatedString();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteSendReceiveStatusChanged(object sender, NoteSendReceiveStatusChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                cpn.SentReceived = note.SentReceived;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                cpn.Name = note.Name;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteGroupChanged(object sender, NoteGroupChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var group = getSelectedGroup();
                if (group == null) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                var grCurrent = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                var grPrev = PNCollections.Instance.Groups.GetGroupById(note.PrevGroupId);

                cpn.IdGroup = note.GroupId;
                cpn.IdPrevGroup = note.PrevGroupId;
                cpn.Group = grCurrent != null ? grCurrent.Name : "";
                cpn.PrevGroup = grPrev != null ? grPrev.Name : "";

                foreach (var item in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    if (changeNodeText(item, e.NewGroup)) break;
                }
                foreach (var item in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    if (changeNodeText(item, e.OldGroup)) break;
                }
                updateStatusBar();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteDateChanged(object sender, NoteDateChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var group = getSelectedGroup();
                if (group == null) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                switch (e.Type)
                {
                    case NoteDateType.Creation:
                        {
                            cpn.Created = e.NewDate;
                            var selectedNote = getSelectedNote();
                            if (selectedNote != null && selectedNote.Id == cpn.Id)
                            {
                                loadNotePreview(selectedNote);
                            }
                        }
                        break;
                    case NoteDateType.Deletion:
                        cpn.Deleted = e.NewDate;
                        var view = (CollectionView)CollectionViewSource.GetDefaultView(grdNotes.ItemsSource);
                        view.Refresh();
                        break;
                    case NoteDateType.Receiving:
                        cpn.ReceivedAt = e.NewDate;
                        cpn.ReceivedFrom = note.ReceivedFrom;
                        cpn.SentReceived = note.SentReceived;
                        changeSpecialNodeText((int)SpecialGroups.Incoming);
                        break;
                    case NoteDateType.Saving:
                        cpn.Saved = e.NewDate;
                        cpn.Content = getNoteContent(note);
                        if (group.Id == (int)SpecialGroups.Backup)
                        {
                            loadBackUpNotes();
                        }
                        else
                        {
                            var selectedNote = getSelectedNote();
                            if (selectedNote != null && selectedNote.Id == cpn.Id)
                            {
                                loadNotePreview(selectedNote);
                            }
                        }
                        if (PNRuntimes.Instance.Settings.Protection.BackupBeforeSaving)
                        {
                            changeSpecialNodeText((int)SpecialGroups.Backup);
                        }
                        break;
                    case NoteDateType.Sending:
                        cpn.SentAt = e.NewDate;
                        cpn.SentTo = note.SentTo;
                        cpn.SentReceived = note.SentReceived;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                var group = getSelectedGroup();
                if (group == null) return;
                var cpn = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (cpn == null) return;
                switch (e.Type)
                {
                    case NoteBooleanTypes.Change:
                    case NoteBooleanTypes.FromDB:
                    case NoteBooleanTypes.Visible:
                        cpn.State = getNoteState(note);
                        if (e.Type == NoteBooleanTypes.FromDB)
                        {
                            changeSpecialNodeText((int)SpecialGroups.Backup);
                        }
                        break;
                    case NoteBooleanTypes.Complete:
                        cpn.Completed = note.Completed;
                        break;
                    case NoteBooleanTypes.Pin:
                        cpn.Pinned = note.Pinned;
                        break;
                    case NoteBooleanTypes.Priority:
                        cpn.Priority = note.Priority;
                        break;
                    case NoteBooleanTypes.Protection:
                        cpn.Protected = note.Protected;
                        break;
                    case NoteBooleanTypes.Favorite:
                        cpn.Favorites = note.Favorite;
                        changeSpecialNodeText((int)SpecialGroups.Favorites);
                        break;
                    case NoteBooleanTypes.Scrambled:
                        cpn.Encrypted = note.Scrambled;
                        break;
                    case NoteBooleanTypes.Password:
                        cpn.Password = note.PasswordString != ""
                            ? PasswordProtectionMode.Note
                            : (group.PasswordString != ""
                                ? PasswordProtectionMode.Group
                                : PasswordProtectionMode.None);
                        cpn.Content = getNoteContent(note);
                        var selectedNote = getSelectedNote();
                        if (selectedNote != null && selectedNote.Id == cpn.Id)
                        {
                            loadNotePreview(selectedNote);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updateStatusBar()
        {
            try
            {
                var png = getSelectedGroup();
                stlName.Text = _CapGroup + ' ' + png.Name;
                switch (png.Id)
                {
                    case (int)SpecialGroups.AllGroups:
                        stlCount.Text = _CapCount + ' ' +
                                        PNCollections.Instance.Notes.Count(n => n.GroupId != (int)SpecialGroups.RecycleBin);
                        break;
                    case (int)SpecialGroups.Favorites:
                        stlCount.Text = _CapCount + ' ' +
                                        PNCollections.Instance.Notes.Count(
                                            n => n.Favorite && n.GroupId != (int)SpecialGroups.RecycleBin);
                        break;
                    case (int)SpecialGroups.Backup:
                        {
                            var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                            var fis = di.GetFiles("*" + PNStrings.NOTE_BACK_EXTENSION);
                            var count =
                                fis.Count(
                                    f =>
                                        PNCollections.Instance.Notes.Any(
                                            n =>
                                                n.Id ==
                                                Path.GetFileNameWithoutExtension(f.Name)
                                                    .Substring(0, Path.GetFileNameWithoutExtension(f.Name).IndexOf('_'))));
                            stlCount.Text = _CapCount + ' ' + count;
                        }
                        break;
                    default:
                        stlCount.Text = _CapCount + ' ' + PNCollections.Instance.Notes.Count(n => n.GroupId == png.Id);
                        break;
                }
                stlTotal.Text = _CapTotal + ' ' + PNCollections.Instance.Notes.Count;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool changeNodeText(PNTreeItem node, int groupId)
        {
            try
            {
                var png = node.Tag as PNGroup;
                if (png == null) return false;
                if (png.Id == groupId)
                {
                    var count = PNCollections.Instance.Notes.Count(n => n.GroupId == groupId);
                    node.Text = png.Name;
                    node.Text += @" (" + count + @")";
                    if (png.PasswordString.Trim().Length > 0)
                    {
                        node.Text += @" ***";
                    }
                    return true;
                }
                if (png.Id == (int)SpecialGroups.AllGroups)
                {
                    var count = PNCollections.Instance.Notes.Count(n => n.GroupId != (int)SpecialGroups.RecycleBin);
                    node.Text = png.Name + @" (" + count + @")";
                }
                return node.Items.OfType<PNTreeItem>().Any(n => changeNodeText(n, groupId));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void changeSpecialNodeText(int groupId)
        {
            try
            {
                foreach (PNTreeItem node in tvwGroups.Items)
                {
                    var png = node.Tag as PNGroup;
                    if (png == null) continue;
                    if (png.Id != groupId) continue;
                    switch (png.Id)
                    {
                        case (int)SpecialGroups.Favorites:
                            {
                                var count =
                                    PNCollections.Instance.Notes.Count(n => n.Favorite && n.GroupId != (int)SpecialGroups.RecycleBin);
                                node.Text = png.Name + @" (" + count.ToString(PNRuntimes.Instance.CultureInvariant) + @")";
                            }
                            break;
                        case (int)SpecialGroups.Diary:
                            {
                                var count = PNCollections.Instance.Notes.Count(n => n.GroupId == (int)SpecialGroups.Diary);
                                node.Text = png.Name + @" (" + count.ToString(PNRuntimes.Instance.CultureInvariant) + @")";
                            }
                            break;
                        case (int)SpecialGroups.Backup:
                            {
                                var di = new DirectoryInfo(PNPaths.Instance.BackupDir);
                                var fis = di.GetFiles("*" + PNStrings.NOTE_BACK_EXTENSION);
                                var count =
                                    fis.Count(
                                        f =>
                                            PNCollections.Instance.Notes.Any(
                                                n =>
                                                    n.Id ==
                                                    Path.GetFileNameWithoutExtension(f.Name)
                                                        .Substring(0, Path.GetFileNameWithoutExtension(f.Name).IndexOf('_'))));
                                node.Text = png.Name + @" (" + count + @")";
                            }
                            break;
                        case (int)SpecialGroups.Incoming:
                            {
                                var count = PNCollections.Instance.Notes.Count(n => n.GroupId == (int)SpecialGroups.Incoming);
                                node.Text = png.Name + @" (" + count.ToString(PNRuntimes.Instance.CultureInvariant) + @")";
                            }
                            break;
                        case (int)SpecialGroups.SearchResults:
                            node.Text = png.Name + @" (" + _SearchResults.Count.ToString(PNRuntimes.Instance.CultureInvariant) +
                                        @")";
                            break;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            try
            {
                //refreshToolbarCommandsBinding(tbrCp);
                //refreshToolbarCommandsBinding(tbrGroups);
                //refreshMenuCommandsBinding(tvwGroups.ContextMenu);
                //applyMenusLanguage();
                applyLanguageExceptMenus();
                ////change tree nodes text and names
                //foreach (TreeNode n in tvwGroups.Nodes)
                //{
                //    var png = n.Tag as PNGroup;
                //    if (png != null) changeFolderName(n, png.Name);
                //}
                ////change "General" png node text and name
                //foreach (TreeNode n in tvwGroups.Nodes[0].Nodes)
                //{
                //    var png = n.Tag as PNGroup;
                //    if (png != null && png.ID == 0)
                //    {
                //        changeFolderName(n, png.Name);
                //    }
                //}
                //updateStatusBar();
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, false);
                PNMenus.PrepareDefaultMenuStrip(ctmList, MenuType.ControlPanel, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Search text procedures

        private void txtQSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = string.IsNullOrEmpty(txtQSearch.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void txtQSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Hidden;
        }

        private void txtQSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearch.Visibility = string.IsNullOrEmpty(txtQSearch.Text) ? Visibility.Visible : Visibility.Hidden;
            if (txtQSearch.Text.Trim().Length == 0)
            {
                actionClearQuickSearch();
            }
        }

        private void txtQSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                txtSearch.Visibility = Visibility.Visible;
            }
            else if (e.Key == Key.Enter && txtQSearch.Text.Trim().Length > 0)
            {
                actionQuickSearch();
            }
        }

        private void txtSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.Visibility = Visibility.Hidden;
            txtQSearch.Focus();
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Hidden;
            txtQSearch.Focus();
        }

        private void txtSearch_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if ((bool)e.NewValue)
                {
                    txtQSearch.CaretBrush = txtQSearch.Background;
                }
                else
                {
                    txtQSearch.CaretBrush = txtSearch.CaretBrush;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Main toolbar CanExecute procedures

        private bool canExecuteNewLoadClipboard()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                return !grCurrent.Id.In((int)SpecialGroups.RecycleBin, (int)SpecialGroups.Diary,
                    (int)SpecialGroups.Backup, (int)SpecialGroups.Favorites, (int)SpecialGroups.SearchResults,
                    (int)SpecialGroups.Incoming);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSaveAsShortcut()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                if (grCurrent.Id.In((int)SpecialGroups.RecycleBin, (int)SpecialGroups.Diary,
                    (int)SpecialGroups.Backup, (int)SpecialGroups.SearchResults))
                    return false;
                var note = getSelectedNote();
                return note != null && note.FromDB && note.GroupId != (int)SpecialGroups.Diary &&
                       grdNotes.SelectedItems.Count == 1;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteDuplicate()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                if (grCurrent.Id.In((int)SpecialGroups.RecycleBin, (int)SpecialGroups.Diary,
                    (int)SpecialGroups.Backup, (int)SpecialGroups.SearchResults,
                    (int)SpecialGroups.Incoming))
                    return false;
                var note = getSelectedNote();
                return note != null && note.GroupId != (int)SpecialGroups.Diary && grdNotes.SelectedItems.Count == 1;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteDelete()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    //case (int)SpecialGroups.RecycleBin:
                    //    return false;
                    case (int)SpecialGroups.Backup:
                        return grdBackup.SelectedItems.Count > 0;
                    default:
                        return grdNotes.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteAddFavorites()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        cmdFavorites.Visibility = mnuFavorites.Visibility = Visibility.Collapsed;
                        return false;
                    default:
                        if (grdNotes.SelectedItems.Count >= 1)
                        {
                            if (getFavoriteStatus() != FavoriteStatus.No)
                            {
                                cmdFavorites.Visibility = mnuFavorites.Visibility = Visibility.Collapsed;
                                return false;
                            }
                            cmdFavorites.Visibility = mnuFavorites.Visibility = Visibility.Visible;
                            return true;
                        }
                        cmdFavorites.Visibility = mnuFavorites.Visibility = Visibility.Collapsed;
                        return false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteRemoveFavorites()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        cmdRemoveFromFavorites.Visibility = mnuRemoveFromFavorites.Visibility = Visibility.Collapsed;
                        return false;
                    default:
                        if (grdNotes.SelectedItems.Count >= 1)
                        {
                            if (getFavoriteStatus() != FavoriteStatus.Yes)
                            {
                                cmdRemoveFromFavorites.Visibility =
                                    mnuRemoveFromFavorites.Visibility = Visibility.Collapsed;
                                return false;
                            }
                            cmdRemoveFromFavorites.Visibility =
                                mnuRemoveFromFavorites.Visibility = Visibility.Visible;
                            return true;
                        }
                        cmdRemoveFromFavorites.Visibility = mnuRemoveFromFavorites.Visibility = Visibility.Collapsed;
                        return false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSave()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSaveAs()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                }
                var note = getSelectedNote();
                return note != null && note.GroupId != (int)SpecialGroups.Diary && grdNotes.SelectedItems.Count == 1;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteAdjust()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count == 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteAdjustAppearance()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                    case (int)SpecialGroups.Diary:
                    case (int)SpecialGroups.Incoming:
                        return false;
                    default:
                        if (grdNotes.SelectedItems.Count != 1) return false;
                        var note = getSelectedNote();
                        if (note == null) return false;
                        if (note.GroupId.In((int)SpecialGroups.Diary, (int)SpecialGroups.RecycleBin,
                            (int)SpecialGroups.Docking)) return false;
                        return true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteHide()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                }
                var note = getSelectedNote();
                if (note == null)
                    return false;
                return note.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteShow()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                }
                var note = getSelectedNote();
                if (note == null)
                    return false;
                return !note.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteCentralize()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSendAsText()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count == 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSendAsAttachmentOrZip()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteRetoreFromBackup()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                        return false;
                    case (int)SpecialGroups.Backup:
                        return grdBackup.SelectedItems.Count == 1;
                    default:
                        return grdNotes.SelectedItems.Count == 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecutePrint()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        var vh = checkGroupVisibility();
                        if ((vh & VisibleHidden.Visible) == VisibleHidden.Visible)
                            return grdNotes.SelectedItems.Count > 0;
                        return false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSelectContacts()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count > 0;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteTagsCurrent()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count == 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteSwitches()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        return grdNotes.SelectedItems.Count == 1;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private bool canExecuteScramble()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                    case (int)SpecialGroups.Backup:
                        return false;
                    default:
                        if (grdNotes.SelectedItems.Count != 1)
                        {
                            return false;
                        }
                        var note = getSelectedNote();
                        return note != null;// && note.Visible;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }


        private bool canExecuteRestoreNote()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null)
                    return false;
                switch (grCurrent.Id)
                {
                    case (int)SpecialGroups.RecycleBin:
                        return grdNotes.SelectedItems.Count == 1;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Main toolbar Executed procedures

        private void actionNewOption()
        {
            try
            {
                var pnGroup = getSelectedGroup();
                if (pnGroup == null) return;
                if (pnGroup.Id > 0)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.NewNoteInGroup, pnGroup.Id);
                }
                else
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.NewNote, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionLoadNote()
        {
            try
            {
                var pnGroup = getSelectedGroup();
                if (pnGroup == null) return;
                if (pnGroup.Id > 0)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.LoadNotesInGroup, pnGroup.Id);
                }
                else
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.LoadNotes, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionNoteFromClipboard()
        {
            try
            {
                var pnGroup = getSelectedGroup();
                if (pnGroup == null) return;
                if (pnGroup.Id > 0)
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.NoteFromClipboardInGroup, pnGroup.Id);
                }
                else
                {
                    PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.NoteFromClipboard, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSaveAsShortcut()
        {
            try
            {
                var note = getSelectedNote();
                if (note != null)
                {
                    PNNotesOperations.SaveAsShortcut(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionDuplicateNote()
        {
            try
            {
                var note = getSelectedNote();
                if (note != null)
                {
                    PNWindows.Instance.FormMain.DuplicateNote(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSaveNotes()
        {
            try
            {
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(n => PNCollections.Instance.Notes.FirstOrDefault(nt => nt.Id == n.Id))
                        .Where(note => note != null && note.Visible && note.Changed))
                {
                    note.Dialog.ApplySaveNote(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSaveNoteAs()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                if (note.Visible)
                    note.Dialog.ApplySaveNote(true);
                else
                    saveNoteAs(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSaveNotesAsText()
        {
            try
            {
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(n => PNCollections.Instance.Notes.FirstOrDefault(nt => nt.Id == n.Id))
                        .Where(note => note != null))
                {
                    if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            return;
                    PNNotesOperations.SaveNoteAsTextFile(note, this);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionDeleteNotes()
        {
            try
            {
                InLongOperation = true;
                var gr = getSelectedGroup();
                if (gr == null) return;
                if (gr.Id != (int)SpecialGroups.Backup)
                {
                    var ids = grdNotes.SelectedItems.OfType<CPNote>().Select(n => n.Id).ToArray();
                    if (ids.Length == 0) return;
                    var temp = PNCollections.Instance.Notes.Note(ids[0]);
                    var type = PNNotesOperations.DeletionWarning(
                        gr.Id == (int)SpecialGroups.RecycleBin || HotkeysStatic.LeftShiftDown(), ids.Length, temp);
                    if (type == NoteDeleteType.None) return;
                    Mouse.OverrideCursor = Cursors.Wait;
                    foreach (var note in ids.Select(id => PNCollections.Instance.Notes.Note(id))
                        .Where(note => note != null))
                    {
                        deleteNote(note, type);
                    }
                }
                else
                {
                    var message = PNLang.Instance.GetMessageText("delete_backups", "Delete selected backup copies?");
                    if (
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) !=
                        MessageBoxResult.Yes) return;
                    Mouse.OverrideCursor = Cursors.Wait;
                    var names = grdBackup.SelectedItems.OfType<CPBackNote>().Select(cpb => cpb.Name).ToArray();
                    var files = grdBackup.SelectedItems.OfType<CPBackNote>().Select(cpb => cpb.FileName);
                    foreach (var f in files.Where(File.Exists))
                    {
                        File.Delete(f);
                    }

                    for (var i = _BackNotes.Count - 1; i >= 0; i--)
                    {
                        if (names.Any(s => s == _BackNotes[i].Name))
                        {
                            _BackNotes.RemoveAt(i);
                        }
                    }

                    changeSpecialNodeText((int)SpecialGroups.Backup);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                InLongOperation = false;
                Mouse.OverrideCursor = null;
            }
        }

        private void actionRestorFromBack()
        {
            try
            {
                var grCurrent = getSelectedGroup();
                if (grCurrent == null) return;
                if (grCurrent.Id != (int)SpecialGroups.Backup)
                {
                    var note = getSelectedNote();
                    if (note != null)
                    {
                        PNNotesOperations.LoadBackupCopy(note, this);
                    }
                }
                else
                {
                    if (!(grdBackup.SelectedItem is CPBackNote cpb))
                        return;
                    var note = PNCollections.Instance.Notes.Note(cpb.Id);
                    if (note == null)
                        return;
                    PNNotesOperations.LoadBackupFile(note, cpb.FileName);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionPrintNotes()
        {
            try
            {
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(n => PNCollections.Instance.Notes.FirstOrDefault(nt => nt.Id == n.Id))
                        .Where(note => note != null && note.Visible))
                {
                    note.Dialog.ApplyPrintNote();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionAdjustAppearance()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var dlgAdjustAppearance = new WndAdjustAppearance(note) { Owner = this };
                dlgAdjustAppearance.NoteAppearanceAdjusted += dlgAdjustAppearance_NoteAppearanceAdjusted;
                var showDialog = dlgAdjustAppearance.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgAdjustAppearance.NoteAppearanceAdjusted -= dlgAdjustAppearance_NoteAppearanceAdjusted;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionAdjustSchedule()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                PNNotesOperations.AdjustNoteSchedule(note, this);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionShowNotes()
        {
            try
            {
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(cp => PNCollections.Instance.Notes.Note(cp.Id))
                        .Where(n => n != null))
                {
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionHideNotes()
        {
            try
            {
                foreach (
                    var note in
                    grdNotes.SelectedItems.OfType<CPNote>()
                        .Select(cp => PNCollections.Instance.Notes.Note(cp.Id))
                        .Where(n => n != null))
                {
                    PNNotesOperations.ShowHideSpecificNote(note, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionCentralize()
        {
            try
            {
                PNNotesOperations.CentralizeNotes(grdNotes.SelectedItems.OfType<CPNote>()
                    .Select(cp => PNCollections.Instance.Notes.Note(cp.Id))
                    .Where(n => n != null));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSendAsText()
        {
            try
            {
                var note = getSelectedNote();
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

        private void actionSendAsAttachment()
        {
            try
            {
                var files = new List<string>();
                var notes = getSelectedNotes();
                if (notes == null) return;
                foreach (var note in notes)
                {
                    if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            continue;
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                    if (!File.Exists(path)) continue;
                    files.Add(path);
                }
                if (files.Count > 0)
                {
                    PNNotesOperations.SendNotesAsAttachments(files);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSendAsZip()
        {
            try
            {
                var files = new List<string>();
                var notes = getSelectedNotes();
                if (notes == null) return;
                foreach (var note in notes)
                {
                    if (PNRuntimes.Instance.Settings.Protection.PromptForPassword)
                        if (!PNNotesOperations.LogIntoNoteOrGroup(note))
                            continue;
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                    if (!File.Exists(path)) continue;
                    files.Add(path);
                }
                if (files.Count <= 0) return;
                var dzip = new WndArchName(files) { Owner = this };
                dzip.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionAddContact()
        {
            try
            {
                var newId = 0;
                if (PNCollections.Instance.Contacts.Count > 0)
                {
                    newId = PNCollections.Instance.Contacts.Max(c => c.Id) + 1;
                }
                var dlgContact = new WndContacts(newId, PNCollections.Instance.ContactGroups) { Owner = this };
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

        private void actionAddContactsGroup()
        {
            try
            {
                var newId = 0;
                if (PNCollections.Instance.ContactGroups.Count > 0)
                {
                    newId = PNCollections.Instance.ContactGroups.Max(g => g.Id) + 1;
                }
                var dlgContactGroup = new WndGroups(newId) { Owner = this };
                dlgContactGroup.ContactGroupChanged += dlgContactGroup_ContactGroupChanged;
                var showDialog = dlgContactGroup.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgContactGroup.ContactGroupChanged -= dlgContactGroup_ContactGroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSelectContact()
        {
            try
            {
                var dlgSelectC = new WndSelectContacts { Owner = this };
                dlgSelectC.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectC.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgSelectC.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSelectContactsGroup()
        {
            try
            {
                var dlgSelectG = new WndSelectGroups { Owner = this };
                dlgSelectG.ContactsSelected += dlgSelectCOrG_ContactsSelected;
                var showDialog = dlgSelectG.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgSelectG.ContactsSelected -= dlgSelectCOrG_ContactsSelected;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionTagsCurrent()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var d = new WndExchangeLists(note.Id, ExchangeLists.Tags) { Owner = this };
                d.ShowDialog();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionToggleOnTop()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var value = !note.Topmost;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Topmost, value, null);
                if (note.Visible)
                {
                    note.Dialog.Topmost = value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionTogglePriority()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var value = !note.Priority;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Priority, value, null);
                if (note.Visible)
                {
                    note.Dialog.PFooter.SetMarkButtonVisibility(MarkType.Priority, value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionToggleProtection()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var value = !note.Protected;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Protection, value, null);
                if (note.Visible)
                {
                    note.Dialog.PFooter.SetMarkButtonVisibility(MarkType.Protection, value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionToggleCompleted()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var value = !note.Completed;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Complete, value, null);
                if (note.Visible)
                {
                    note.Dialog.PFooter.SetMarkButtonVisibility(MarkType.Complete, value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionToggleRollUnroll()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                if (note.Visible)
                {
                    note.Dialog.ApplyRollUnroll(note);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionSetPassword()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                var pwrdCrweate = new WndPasswordCreate(text)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                pwrdCrweate.PasswordChanged += notePasswordSet;
                var showDialog = pwrdCrweate.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    pwrdCrweate.PasswordChanged -= notePasswordSet;
                }
                loadNotePreview(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionRemovePassword()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("note", "Note") + " \"" + note.Name + "\"]";
                var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.DeleteNote, text, note.PasswordString)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                pwrdDelete.PasswordDeleted += notePasswordRemoved;
                var showDialog = pwrdDelete.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    pwrdDelete.PasswordDeleted -= notePasswordRemoved;
                }
                loadNotePreview(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionPinNote()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                var dlgPin = new WndPin(note.Name) { Owner = this };
                dlgPin.PinnedWindowChanged += dlgPin_PinnedWindowChanged;
                var showDialog = dlgPin.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgPin.PinnedWindowChanged -= dlgPin_PinnedWindowChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionUnpinNote()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                note.PinText = "";
                note.PinClass = "";
                note.Pinned = false;
                PNNotesOperations.SaveNotePin(note);
                note.Dialog?.PFooter.SetMarkButtonVisibility(MarkType.Pin, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionScramble()
        {
            try
            {
                var note = getSelectedNote();
                if (note == null) return;
                if (note.Visible)
                {
                    if (!PNNotesOperations.ApplyScramble(note, note.Dialog)) return;
                }
                else
                {
                    if (!PNNotesOperations.ApplyScramble(note, this)) return;
                }
                var value = !note.Scrambled;
                PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Scrambled, value, null);
                if (note.Visible)
                {
                    note.Dialog.Edit.ReadOnly = value || (note.Protected);
                    note.Dialog.PFooter.SetMarkButtonVisibility(MarkType.Encrypted, note.Scrambled);
                }

                loadNotePreview(note);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionFavorites()
        {
            try
            {
                var notes = getSelectedNotes();
                foreach (var note in notes)
                {
                    PNNotesOperations.ApplyBooleanChange(note, NoteBooleanTypes.Favorite, !note.Favorite, null);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionRestoreFromBin()
        {
            try
            {
                foreach (var cpn in grdNotes.SelectedItems.OfType<CPNote>())
                {
                    var note = PNCollections.Instance.Notes.Note(cpn.Id);
                    if (note == null) continue;
                    var gr = PNCollections.Instance.Groups.GetGroupById(note.PrevGroupId);
                    note.GroupId = gr != null ? note.PrevGroupId : 0;
                    PNNotesOperations.SaveGroupChange(note);
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
                foreach (var node in tvwGroups.Items.OfType<PNTreeItem>())
                {
                    if (changeNodeText(node, (int)SpecialGroups.RecycleBin))
                    {
                        break;
                    }
                }
                changeSpecialNodeText((int)SpecialGroups.Favorites);
                changeSpecialNodeText((int)SpecialGroups.Backup);
                changeSpecialNodeText((int)SpecialGroups.SearchResults);
                updateStatusBar();
                var view = (CollectionView)CollectionViewSource.GetDefaultView(grdNotes.ItemsSource);
                view.Refresh();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionEmptyBin()
        {
            try
            {
                InLongOperation = true;
                var message = PNLang.Instance.GetMessageText("empty_bin_warning",
                    "Emptying of Recycle Bin cannot be rolled back. Continue?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                Mouse.OverrideCursor = Cursors.Wait;
                var notes = PNCollections.Instance.Notes.Where(n => n.GroupId == (int)SpecialGroups.RecycleBin);
                var aNotes = notes.ToArray();
                for (var i = aNotes.Length - 1; i >= 0; i--)
                {
                    PNNotesOperations.RemoveDeletedNoteFromLists(aNotes[i]);
                    var id = aNotes[i].Id;
                    var groupId = aNotes[i].GroupId;
                    PNNotesOperations.SaveNoteDeletedState(aNotes[i], false);
                    PNNotesOperations.DeleteNoteCompletely(aNotes[i], CompleteDeletionSource.EmptyBin);
                    PNWindows.Instance.FormMain.RaiseDeletedCompletelyEvent(id, groupId);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                InLongOperation = false;
                Mouse.OverrideCursor = null;
            }
        }

        private void actionShowHideGroupsPane(bool show)
        {
            try
            {
                if (show)
                {
                    OuterGrid.ColumnDefinitions[0].Width = _OuterColsWidth[0];
                    OuterGrid.ColumnDefinitions[1].Width = _OuterColsWidth[1];
                    InnerGrid.Margin = new Thickness(4, 0, 0, 0);
                }
                else
                {
                    hideGroupsPane();
                }
                PNRuntimes.Instance.Settings.Config.CPGroupsShow = show;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionShowHideHorzPane(bool show)
        {
            try
            {
                if (!PNRuntimes.Instance.Settings.Config.CPPvwRight)
                {
                    if (show)
                    {
                        SplitVert.Visibility = Visibility.Collapsed;
                        InnerGrid.ColumnDefinitions[2].Width = new GridLength(0);
                        _EditControl.WinForm.Visible = true;
                        SplitHorz.Visibility = Visibility.Visible;
                        InnerGrid.RowDefinitions[2].Height = _RowHeight;
                    }
                    else
                    {
                        hidePreview();
                    }
                }
                else
                {
                    if (show)
                    {
                        SplitHorz.Visibility = Visibility.Collapsed;
                        InnerGrid.RowDefinitions[2].Height = new GridLength(0);
                        _EditControl.WinForm.Visible = true;
                        SplitVert.Visibility = Visibility.Visible;
                        InnerGrid.ColumnDefinitions[2].Width = _ColumnWidth;
                    }
                    else
                    {
                        hidePreview();
                    }
                }
                PNRuntimes.Instance.Settings.Config.CPPvwShow = show;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionPreviewRight(bool setRight)
        {
            try
            {
                if (setRight)
                {
                    setPreviewRight();
                }
                else
                {
                    _ColumnWidth = InnerGrid.ColumnDefinitions[2].Width;

                    Grid.SetRowSpan(grdBackup, 1);
                    Grid.SetColumnSpan(grdBackup, 3);
                    Grid.SetRowSpan(grdNotes, 1);
                    Grid.SetColumnSpan(grdNotes, 3);

                    SplitVert.Visibility = Visibility.Collapsed;
                    SplitHorz.Visibility = Visibility.Visible;

                    Grid.SetRow(BorderHost, 2);
                    Grid.SetColumn(BorderHost, 0);
                    Grid.SetRowSpan(BorderHost, 1);
                    Grid.SetColumnSpan(BorderHost, 3);

                    InnerGrid.RowDefinitions[2].Height = _RowHeight;
                }
                PNRuntimes.Instance.Settings.Config.CPPvwRight = setRight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionIgnoreCase()
        {
            try
            {
                if (!(TryFindResource("mnuQSearchIgnoreCase") is MenuItem mnuQSearchIgnoreCase)) return;
                PNRuntimes.Instance.Settings.Config.CPIgnoreCase = !PNRuntimes.Instance.Settings.Config.CPIgnoreCase;
                mnuQSearchIgnoreCase.IsCheckable = PNRuntimes.Instance.Settings.Config.CPIgnoreCase;
                PNData.SaveCPProperties();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionUseCustColor()
        {
            try
            {
                if (!(TryFindResource("mnuUseCustColor") is MenuItem mnuUseCustColor)) return;
                PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor = !PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor;
                mnuUseCustColor.IsChecked = PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor;
                PNData.SaveCPProperties();
                if (PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor)
                {
                    _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(PNRuntimes.Instance.Settings.Config.CPPvwColor.A,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.R, PNRuntimes.Instance.Settings.Config.CPPvwColor.G,
                        PNRuntimes.Instance.Settings.Config.CPPvwColor.B);
                }
                else
                {
                    var note = getSelectedNote();
                    _EditControl.WinForm.BackColor = note?.DrawingColor() ?? System.Drawing.Color.FromArgb(255, PNSkinlessDetails.DefColor.R, PNSkinlessDetails.DefColor.G,
                                                         PNSkinlessDetails.DefColor.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionChooseCustColor()
        {
            try
            {
                var d = new WndColorChooser { Owner = this };
                var showDialog = d.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                if (PNRuntimes.Instance.Settings.Config.CPUseCustPvwColor)
                {
                    _EditControl.WinForm.BackColor =
                        System.Drawing.Color.FromArgb(PNRuntimes.Instance.Settings.Config.CPPvwColor.A,
                            PNRuntimes.Instance.Settings.Config.CPPvwColor.R, PNRuntimes.Instance.Settings.Config.CPPvwColor.G,
                            PNRuntimes.Instance.Settings.Config.CPPvwColor.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionCreatePassword()
        {
            try
            {
                var dpc = new WndPasswordCreate { Owner = this };
                dpc.PasswordChanged += dpc_PasswordChanged;
                var showDialog = dpc.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dpc.PasswordChanged -= dpc_PasswordChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionChangePassword()
        {
            try
            {
                var dpc = new WndPasswordChange { Owner = this };
                dpc.PasswordChanged += dpc_PasswordChanged;
                var showDialog = dpc.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dpc.PasswordChanged -= dpc_PasswordChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionDeletePassword()
        {
            try
            {
                var dpd = new WndPasswordDelete(PasswordDlgMode.DeleteMain) { Owner = this };
                dpd.PasswordDeleted += dpd_PasswordDeleted;
                var showDialog = dpd.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dpd.PasswordDeleted -= dpd_PasswordDeleted;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionQuickSearch()
        {
            try
            {
                _SearchResults.Clear();
                if (_IncBinInSearch)
                {
                    _SearchResults.AddRange(
                        PNCollections.Instance.Notes.Where(
                                n =>
                                    n.Name.IndexOf(txtQSearch.Text.Trim(),
                                        PNRuntimes.Instance.Settings.Config.CPIgnoreCase
                                            ? StringComparison.OrdinalIgnoreCase
                                            : StringComparison.Ordinal) >= 0)
                            .Select(n => n.Id));
                }
                else
                {
                    _SearchResults.AddRange(
                        PNCollections.Instance.Notes.Where(
                                n =>
                                    n.Name.IndexOf(txtQSearch.Text.Trim(),
                                        PNRuntimes.Instance.Settings.Config.CPIgnoreCase
                                            ? StringComparison.OrdinalIgnoreCase
                                            : StringComparison.Ordinal) >= 0 &&
                                    n.GroupId != (int)SpecialGroups.RecycleBin)
                            .Select(n => n.Id));
                }
                var gr = getSelectedGroup();
                if (gr == null || gr.Id != (int)SpecialGroups.SearchResults)
                    setSelectedGroup((int)SpecialGroups.SearchResults);
                else
                    CollectionViewSource.GetDefaultView(grdNotes.ItemsSource).Refresh();

                changeSpecialNodeText((int)SpecialGroups.SearchResults);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionClearQuickSearch()
        {
            try
            {
                _SearchResults.Clear();
                var gr = getSelectedGroup();
                if (gr != null && gr.Id == (int)SpecialGroups.SearchResults)
                    CollectionViewSource.GetDefaultView(grdNotes.ItemsSource).Refresh();

                changeSpecialNodeText((int)SpecialGroups.SearchResults);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionResetColumns()
        {
            try
            {
                PNData.ResetGridColumns();
                prepareGridColumns(true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void actionExport(CommandType type)
        {
            try
            {
                var filter = "";
                switch (type)
                {
                    case CommandType.ExpPdf:
                        filter = PNLang.Instance.GetCaptionText("pdf_filter", "PDF files (*.pdf)|*.pdf");
                        break;
                    case CommandType.ExpTif:
                        filter = PNLang.Instance.GetCaptionText("tif_filter", "TIF images (*.tif)|*.tif");
                        break;
                    case CommandType.ExpDoc:
                        filter = PNLang.Instance.GetCaptionText("doc_filter", "Word documents (*.doc)|*.doc");
                        break;
                    case CommandType.ExpRtf:
                        filter = PNLang.Instance.GetCaptionText("rtf_filter", "RTF documents (*.rtf)|*.rtf");
                        break;
                    case CommandType.ExpTxt:
                        filter = PNLang.Instance.GetCaptionText("save_as_filter", "Text files (*.txt)|*.txt");
                        break;
                }
                if (filter == "") return;
                PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ExportNotes, filter);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        #region Main toolbar commands
        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.NewNote:
                        actionNewOption();
                        break;
                    case CommandType.LoadNote:
                        actionLoadNote();
                        break;
                    case CommandType.NoteFromClipboard:
                        actionNoteFromClipboard();
                        break;
                    case CommandType.DuplicateNote:
                        actionDuplicateNote();
                        break;
                    case CommandType.SaveAsShortcut:
                        actionSaveAsShortcut();
                        break;
                    case CommandType.Today:
                        PNNotesOperations.CreateOrShowTodayDiary();
                        break;
                    case CommandType.SaveNote:
                        actionSaveNotes();
                        break;
                    case CommandType.SaveAs:
                        actionSaveNoteAs();
                        break;
                    case CommandType.SaveAsText:
                        actionSaveNotesAsText();
                        break;
                    case CommandType.RestoreFromBackup:
                        actionRestorFromBack();
                        break;
                    case CommandType.Print:
                        actionPrintNotes();
                        break;
                    case CommandType.AdjustAppearance:
                        actionAdjustAppearance();
                        break;
                    case CommandType.AdjustSchedule:
                        actionAdjustSchedule();
                        break;
                    case CommandType.DeleteNote:
                        actionDeleteNotes();
                        break;
                    case CommandType.SaveAll:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SaveAll, null);
                        break;
                    case CommandType.BackupCreate:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.FullBackupCreate, null);
                        break;
                    case CommandType.BackupRestore:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.FullBackupRestore, null);
                        break;
                    case CommandType.SyncLocal:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.LocalSync, null);
                        break;
                    case CommandType.ImportNotes:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ImportNotes, null);
                        break;
                    case CommandType.ImportSettings:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ImportSettings, null);
                        break;
                    case CommandType.ImportFonts:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ImportFonts, null);
                        break;
                    case CommandType.ImportDictionaries:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ImportDictionaries, null);
                        break;
                    case CommandType.DockAllNone:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.DockAllNone, null);
                        break;
                    case CommandType.DockAllLeft:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.DockAllLeft, null);
                        break;
                    case CommandType.DockAllTop:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.DockAllTop, null);
                        break;
                    case CommandType.DockAllRight:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.DockAllRight, null);
                        break;
                    case CommandType.DockAllBottom:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.DockAllBottom, null);
                        break;
                    case CommandType.ShowNote:
                        actionShowNotes();
                        break;
                    case CommandType.HideNote:
                        actionHideNotes();
                        break;
                    case CommandType.ShowAllOnCp:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ShowAll, null);
                        break;
                    case CommandType.HideAllOnCp:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.HideAll, null);
                        break;
                    case CommandType.AllToFront:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.BringToFront, null);
                        break;
                    case CommandType.Centralize:
                        actionCentralize();
                        break;
                    case CommandType.SendAsText:
                        actionSendAsText();
                        break;
                    case CommandType.SendAsAttachment:
                        actionSendAsAttachment();
                        break;
                    case CommandType.SendAsZip:
                        actionSendAsZip();
                        break;
                    case CommandType.ContactAdd:
                        actionAddContact();
                        break;
                    case CommandType.ContactGroupAdd:
                        actionAddContactsGroup();
                        break;
                    case CommandType.ContactSelect:
                        actionSelectContact();
                        break;
                    case CommandType.ContactGroupSelect:
                        actionSelectContactsGroup();
                        break;
                    case CommandType.TagsCurrent:
                        actionTagsCurrent();
                        break;
                    case CommandType.OnTop:
                        actionToggleOnTop();
                        break;
                    case CommandType.HighPriority:
                        actionTogglePriority();
                        break;
                    case CommandType.ProtectionMode:
                        actionToggleProtection();
                        break;
                    case CommandType.MarkAsComplete:
                        actionToggleCompleted();
                        break;
                    case CommandType.RollUnroll:
                        actionToggleRollUnroll();
                        break;
                    case CommandType.SetNotePassword:
                        actionSetPassword();
                        break;
                    case CommandType.RemoveNotePassword:
                        actionRemovePassword();
                        break;
                    case CommandType.Pin:
                        actionPinNote();
                        break;
                    case CommandType.Unpin:
                        actionUnpinNote();
                        break;
                    case CommandType.Scramble:
                    case CommandType.Unscramble:
                        actionScramble();
                        break;
                    case CommandType.AddFavorites:
                    case CommandType.RemoveFavorites:
                        actionFavorites();
                        break;
                    case CommandType.EmptyBin:
                        actionEmptyBin();
                        break;
                    case CommandType.RestoreNote:
                        actionRestoreFromBin();
                        break;
                    case CommandType.Preview:
                        if (cmdPreview.IsChecked != null)
                        {
                            if (mnuPreview.IsChecked != cmdPreview.IsChecked)
                            {
                                mnuPreview.IsChecked = cmdPreview.IsChecked.Value;
                            }
                            actionShowHideHorzPane(cmdPreview.IsChecked.Value);
                        }
                        break;
                    case CommandType.PreviewFromMenu:
                        if (mnuPreview.IsChecked != cmdPreview.IsChecked)
                        {
                            cmdPreview.IsChecked = mnuPreview.IsChecked;
                        }
                        actionShowHideHorzPane(mnuPreview.IsChecked);
                        break;
                    case CommandType.ShowGroupsOnCp:
                        if (cmdShowGroups.IsChecked != null)
                        {
                            if (mnuShowGroups.IsChecked != cmdShowGroups.IsChecked)
                            {
                                mnuShowGroups.IsChecked = cmdShowGroups.IsChecked.Value;
                            }
                            actionShowHideGroupsPane(cmdShowGroups.IsChecked.Value);
                        }
                        break;
                    case CommandType.ShowGroupsOnCpFromMenu:
                        if (mnuShowGroups.IsChecked != cmdShowGroups.IsChecked)
                        {
                            cmdShowGroups.IsChecked = mnuShowGroups.IsChecked;
                        }
                        actionShowHideGroupsPane(mnuShowGroups.IsChecked);
                        break;
                    case CommandType.PreviewRight:
                        if (cmdPreviewRight.IsChecked != null)
                        {
                            if (mnuPreviewRight.IsChecked != cmdPreviewRight.IsChecked)
                            {
                                mnuPreviewRight.IsChecked = cmdPreviewRight.IsChecked.Value;
                            }
                            actionPreviewRight(cmdPreviewRight.IsChecked.Value);
                        }
                        break;
                    case CommandType.PreviewRightFromMenu:
                        if (mnuPreviewRight.IsChecked != cmdPreviewRight.IsChecked)
                        {
                            cmdPreviewRight.IsChecked = mnuPreviewRight.IsChecked;
                        }
                        actionPreviewRight(mnuPreviewRight.IsChecked);
                        break;
                    case CommandType.UseCustColor:
                        actionUseCustColor();
                        break;
                    case CommandType.ChooseCustColor:
                        actionChooseCustColor();
                        break;
                    case CommandType.HotkeysCP:
                        new WndHotkeys { Owner = this }.ShowDialog();
                        break;
                    case CommandType.MenusManagementCP:
                        new WndMenusManager { Owner = this }.ShowDialog();
                        break;
                    case CommandType.Preferences:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Preferences, null);
                        break;
                    case CommandType.PwrdCreate:
                        actionCreatePassword();
                        break;
                    case CommandType.PwrdChange:
                        actionChangePassword();
                        break;
                    case CommandType.PwrdRemove:
                        actionDeletePassword();
                        break;
                    case CommandType.SearchInNotes:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SearchInNotes, null);
                        break;
                    case CommandType.SearchByTags:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SearchByTags, null);
                        break;
                    case CommandType.SearchByDates:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.SearchByDates, null);
                        break;
                    case CommandType.IncBinInQSearch:
                        _IncBinInSearch = !_IncBinInSearch;
                        break;
                    case CommandType.QuickSearch:
                        actionQuickSearch();
                        break;
                    case CommandType.ClearQSearch:
                        if (txtQSearch.Text.Trim().Length > 0) txtQSearch.Text = "";
                        break;
                    case CommandType.QSearchIgnoreCase:
                        actionIgnoreCase();
                        break;
                    case CommandType.Help:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Help, null);
                        break;
                    case CommandType.Support:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.Support, null);
                        break;
                    case CommandType.About:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.About, null);
                        break;
                    case CommandType.ColReset:
                        actionResetColumns();
                        break;
                    case CommandType.ExpDoc:
                    case CommandType.ExpPdf:
                    case CommandType.ExpRtf:
                    case CommandType.ExpTif:
                    case CommandType.ExpTxt:
                        actionExport(command.Type);
                        break;
                    case CommandType.PrintFields:
                        PNWindows.Instance.FormMain.ApplyAction(MainDialogAction.ConfigureReport, null);
                        break;
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
                if (!_Loaded) return;
                switch (command.Type)
                {
                    case CommandType.NewNote:
                    case CommandType.LoadNote:
                    case CommandType.NoteFromClipboard:
                        e.CanExecute = canExecuteNewLoadClipboard();
                        break;
                    case CommandType.DuplicateNote:
                        e.CanExecute = canExecuteDuplicate();
                        break;
                    case CommandType.SaveAsShortcut:
                        e.CanExecute = canExecuteSaveAsShortcut();
                        break;
                    case CommandType.SaveNote:
                    case CommandType.SaveAsText:
                        e.CanExecute = canExecuteSave();
                        break;
                    case CommandType.SaveAs:
                        e.CanExecute = canExecuteSaveAs();
                        break;
                    case CommandType.Adjust:
                    case CommandType.AdjustSchedule:
                        e.CanExecute = canExecuteAdjust();
                        break;
                    case CommandType.AdjustAppearance:
                        e.CanExecute = canExecuteAdjustAppearance();
                        break;
                    case CommandType.RestoreFromBackup:
                        e.CanExecute = canExecuteRetoreFromBackup();
                        break;
                    case CommandType.Print:
                        e.CanExecute = canExecutePrint();
                        break;
                    case CommandType.DeleteNote:
                        e.CanExecute = canExecuteDelete();
                        break;
                    case CommandType.AddFavorites:
                        e.CanExecute = canExecuteAddFavorites();
                        break;
                    case CommandType.RemoveFavorites:
                        e.CanExecute = canExecuteRemoveFavorites();
                        break;
                    case CommandType.Centralize:
                        e.CanExecute = canExecuteCentralize();
                        break;
                    case CommandType.ShowNote:
                        e.CanExecute = canExecuteShow();
                        break;
                    case CommandType.HideNote:
                        e.CanExecute = canExecuteHide();
                        break;
                    case CommandType.SendNetwork:
                        e.CanExecute = PNRuntimes.Instance.Settings.Network.EnableExchange;
                        break;
                    case CommandType.SendAsText:
                        e.CanExecute = canExecuteSendAsText();
                        break;
                    case CommandType.SendAsAttachment:
                    case CommandType.SendAsZip:
                        e.CanExecute = canExecuteSendAsAttachmentOrZip();
                        break;
                    case CommandType.ContactSelect:
                    case CommandType.ContactGroupSelect:
                    case CommandType.ContactAdd:
                    case CommandType.ContactGroupAdd:
                        e.CanExecute = canExecuteSelectContacts();
                        break;
                    case CommandType.TagsCurrent:
                        e.CanExecute = canExecuteTagsCurrent();
                        break;
                    case CommandType.Switches:
                        e.CanExecute = canExecuteSwitches();
                        break;
                    case CommandType.Scramble:
                    case CommandType.Unscramble:
                        e.CanExecute = canExecuteScramble();
                        break;
                    case CommandType.Run:
                        e.CanExecute = PNCollections.Instance.Externals.Count > 0;
                        break;
                    case CommandType.EmptyBin:
                        e.CanExecute = PNCollections.Instance.Notes.Any(n => n.GroupId == (int)SpecialGroups.RecycleBin);
                        break;
                    case CommandType.RestoreNote:
                        e.CanExecute = canExecuteRestoreNote();
                        break;
                    case CommandType.PreviewRight:
                        e.CanExecute = PNRuntimes.Instance.Settings.Config.CPPvwShow;
                        break;
                    case CommandType.QuickSearch:
                        e.CanExecute = txtSearch.Visibility != Visibility.Visible;
                        break;
                    case CommandType.PwrdCreate:
                        e.CanExecute = PNRuntimes.Instance.Settings.Protection.PasswordString.Length == 0;
                        break;
                    case CommandType.PwrdChange:
                    case CommandType.PwrdRemove:
                        e.CanExecute = PNRuntimes.Instance.Settings.Protection.PasswordString.Length > 0;
                        break;

                    default:
                        e.CanExecute = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Groups treeview actions

        private void groupsActionAddGroup()
        {
            try
            {
                _NewGroupTarget = NewGroupTarget.TopLevel;
                var item = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
                if (item == null) return;
                var dlgGroup = new WndNewGroup(null, item) { Owner = this };
                dlgGroup.GroupChanged += dlgGroup_GroupChanged;
                var showDialog = dlgGroup.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgGroup.GroupChanged -= dlgGroup_GroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionAddSubgroup()
        {
            try
            {
                _NewGroupTarget = NewGroupTarget.SubGroup;
                var item = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
                if (item == null) return;
                var dlgGroup = new WndNewGroup(null, item) { Owner = this };
                dlgGroup.GroupChanged += dlgGroup_GroupChanged;
                var showDialog = dlgGroup.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgGroup.GroupChanged -= dlgGroup_GroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionEditGroup()
        {
            try
            {
                _NewGroupTarget = NewGroupTarget.SubGroup;
                var item = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
                if (item == null) return;
                var gr = item.Tag as PNGroup;
                if (gr == null) return;
                var dlgGroup = new WndNewGroup(gr, item) { Owner = this };
                dlgGroup.GroupChanged += dlgGroup_GroupChanged;
                var showDialog = dlgGroup.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgGroup.GroupChanged -= dlgGroup_GroupChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionDeleteGroup()
        {
            try
            {
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                if (gr.Id <= (int)SpecialGroups.AllGroups) return;
                deleteGroupOnDrop(gr, _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionShowAll()
        {
            try
            {
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                recursivelyShowHideGroup(gr, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionHideAll()
        {
            try
            {
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                recursivelyShowHideGroup(gr, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionPasswordCreate()
        {
            try
            {
                _LastTreeItem = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
                if (_LastTreeItem == null) return;
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("group", "Group") + " \"" + gr.Name + "\"]";
                var pwrdCreate = new WndPasswordCreate(text) { Owner = this };
                pwrdCreate.PasswordChanged += pwrdCreate_PasswordChanged;
                _SelectedGroup = gr.Id;
                var showDialog = pwrdCreate.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    pwrdCreate.PasswordChanged -= pwrdCreate_PasswordChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _LastTreeItem = null;
                _SelectedGroup = -1234;
            }
        }

        private void groupsActionPasswordRemove()
        {
            try
            {
                _LastTreeItem = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
                if (_LastTreeItem == null) return;
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                var text = " [" + PNLang.Instance.GetCaptionText("group", "Group") + " \"" + gr.Name + "\"]";
                var pwrdDelete = new WndPasswordDelete(PasswordDlgMode.DeleteGroup, text, gr.PasswordString)
                {
                    Owner = this
                };
                pwrdDelete.PasswordDeleted += pwrdDelete_PasswordDeleted;
                _SelectedGroup = gr.Id;
                var showDialog = pwrdDelete.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    pwrdDelete.PasswordDeleted -= pwrdDelete_PasswordDeleted;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _LastTreeItem = null;
                _SelectedGroup = 0;
            }
        }

        private void groupsActionShowGroup()
        {
            try
            {
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                PNNotesOperations.ShowHideSpecificGroup(gr.Id, true);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void groupsActionHideGroup()
        {
            try
            {
                var gr = _RightSelectedItem != null ? _RightSelectedItem.Tag as PNGroup : getSelectedGroup();
                if (gr == null) return;
                PNNotesOperations.ShowHideSpecificGroup(gr.Id, false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Groups treeview and commands

        private void GroupCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Command is PNRoutedUICommand command)) return;
            switch (command.Type)
            {
                case CommandType.GroupAdd:
                    groupsActionAddGroup();
                    break;
                case CommandType.GroupAddSubgroup:
                    groupsActionAddSubgroup();
                    break;
                case CommandType.GroupEdit:
                    groupsActionEditGroup();
                    break;
                case CommandType.GroupRemove:
                    groupsActionDeleteGroup();
                    break;
                case CommandType.GroupShow:
                    groupsActionShowGroup();
                    break;
                case CommandType.GroupHide:
                    groupsActionHideGroup();
                    break;
                case CommandType.GroupShowAll:
                    groupsActionShowAll();
                    break;
                case CommandType.GroupHideAll:
                    groupsActionHideAll();
                    break;
                case CommandType.GroupPassAdd:
                    groupsActionPasswordCreate();
                    break;
                case CommandType.GroupPassRemove:
                    groupsActionPasswordRemove();
                    break;
            }
        }

        private void GroupCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Command is PNRoutedUICommand command)) return;
            if (!_Loaded) return;

            if (command.Type == CommandType.GroupAdd)
            {
                e.CanExecute = true;
                return;
            }

            var item = _RightSelectedItem ?? tvwGroups.SelectedItem as PNTreeItem;
            if (item?.ItemParent == null)
            {
                e.CanExecute = false;
                return;
            }
            switch (command.Type)
            {
                case CommandType.GroupAddSubgroup:
                case CommandType.GroupEdit:
                    e.CanExecute = true;
                    break;
                case CommandType.GroupRemove:
                    e.CanExecute = ((PNGroup)item.Tag).Id != 0;
                    break;
                case CommandType.GroupPassAdd:
                    e.CanExecute = ((PNGroup)item.Tag).PasswordString == "";
                    break;
                case CommandType.GroupPassRemove:
                    e.CanExecute = ((PNGroup)item.Tag).PasswordString != "";
                    break;
                case CommandType.GroupShow:
                    e.CanExecute = PNCollections.Instance.Notes.Any(n => n.GroupId == ((PNGroup)item.Tag).Id && !n.Visible);
                    break;
                case CommandType.GroupHide:
                    e.CanExecute = PNCollections.Instance.Notes.Any(n => n.GroupId == ((PNGroup)item.Tag).Id && n.Visible);
                    break;
                case CommandType.GroupShowAll:
                    var vhv = ((PNGroup)item.Tag).Subgroups.Aggregate(VisibleHidden.None, (current, g) => current | checkBranchVisibility(g));
                    e.CanExecute = (vhv & VisibleHidden.Hidden) == VisibleHidden.Hidden;
                    break;
                case CommandType.GroupHideAll:
                    var vhh = ((PNGroup)item.Tag).Subgroups.Aggregate(VisibleHidden.None, (current, g) => current | checkBranchVisibility(g));
                    e.CanExecute = (vhh & VisibleHidden.Hidden) == VisibleHidden.Hidden;
                    break;
            }
        }

        private void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                clearPreview();
                if (!(e.NewValue is PNTreeItem item)) return;
                var gr = item.Tag as PNGroup;
                if (gr == null) return;
                _LastGroup = gr.Id;
                setColumnsVisibility(gr.Id);
                if (gr.Id != (int)SpecialGroups.Backup)
                {
                    grdBackup.Visibility = Visibility.Hidden;
                    grdNotes.Visibility = Visibility.Visible;
                    CollectionViewSource.GetDefaultView(grdNotes.ItemsSource).Refresh();
                    if (grdNotes.Items.Count > 0)
                    {
                        var oldItem = grdNotes.SelectedItem;
                        grdNotes.SelectedIndex = 0;
                        if (Equals(oldItem, grdNotes.SelectedItem))
                        {
                            var note = getSelectedNote();
                            if (note != null)
                            {
                                loadNotePreview(note);
                            }
                        }
                    }
                }
                else
                {
                    loadBackUpNotes();
                    grdNotes.Visibility = Visibility.Hidden;
                    grdBackup.Visibility = Visibility.Visible;
                    if (grdBackup.Items.Count > 0)
                    {
                        grdBackup.SelectedIndex = 0;
                    }
                }
                updateStatusBar();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(tvwGroups.GetHierarchyObjectAtPoint<PNTreeItem>(e.GetPosition(tvwGroups)) is PNTreeItem item)) return;
                var gr = item.Tag as PNGroup;
                if (gr == null) return;
                _RightSelectedItem = item;
                removeHighlightOnRightClick(tvwGroups, gr.Id);
                Utils.SetIsHighlighted(_RightSelectedItem, true);
                Utils.SetGroupData(sepGroupsMenu, PNLang.Instance.GetCaptionText("group", "Group") + @": " + gr.Name);
                ctmGroups.IsOpen = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void removeHighlightOnRightClick(ItemsControl parent, int selectedId)
        {
            try
            {
                foreach (var item in parent.Items.OfType<PNTreeItem>())
                {
                    removeHighlightOnRightClick(item, selectedId);
                }
                if (!(parent is PNTreeItem)) return;
                var gr = parent.Tag as PNGroup;
                if (gr == null) return;
                if (gr.Id != selectedId)
                {
                    Utils.SetIsHighlighted(parent, false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ctmGroups_Closed(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_RightSelectedItem == null) return;
                Utils.SetIsHighlighted(_RightSelectedItem, false);
                _RightSelectedItem = null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region List views events handlers
        private void grdNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (grdNotes.Visibility != Visibility.Visible) return;
                if (grdNotes.SelectedItems.Count != 1)
                {
                    clearPreview();
                    return;
                }

                if (!(grdNotes.SelectedItem is CPNote item)) return;
                var selectedNote = getSelectedNote();
                if (selectedNote != null && selectedNote.Id == item.Id)
                {
                    loadNotePreview(selectedNote);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdNotes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdNotes.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdNotes)) is CPNote item)) return;
                var gr = getSelectedGroup();
                if (gr == null || gr.Id == (int)SpecialGroups.RecycleBin) return;
                var note = PNCollections.Instance.Notes.Note(item.Id);
                if (note != null)
                {
                    PNNotesOperations.ShowHideSpecificNote(note, true);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdBackup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (grdBackup.Visibility != Visibility.Visible) return;
                if (grdBackup.SelectedItems.Count != 1)
                {
                    clearPreview();
                    return;
                }

                if (!(grdBackup.SelectedItem is CPBackNote item)) return;
                loadBackupPreview(item.Name);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdBackup_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdBackup.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdBackup)) is CPBackNote item)) return;
                var note = PNCollections.Instance.Notes.Note(item.Id);
                if (note == null) return;
                var message = PNLang.Instance.GetMessageText("restore_question", "Do you want to restore the note's content from selected backup copy?");
                if (WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                    MessageBoxResult.Yes)
                {
                    PNNotesOperations.LoadBackupCopy(note, item.Name + PNStrings.NOTE_BACK_EXTENSION);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region List views headers context menus procedures
        private void NotesHeaderMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (!(sender is MenuItem mi)) return;
                var gc = _NotesGridCols.FirstOrDefault(c => c.Name == (string)mi.Tag);
                if (gc != null)
                    gc.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                if (!(grdNotes.View is GridView gridView)) return;
                var fxc =
                    gridView.Columns.OfType<FixedWidthColumn>()
                        .FirstOrDefault(fc => WPFGridViewHelper.GetColumnName(fc) == (string)mi.Tag);
                if (fxc != null)
                    fxc.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BackHeadeMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (!(sender is MenuItem mi)) return;
                var gc = _BackGridCols.FirstOrDefault(c => c.Name == (string)mi.Tag);
                if (gc != null)
                    gc.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                if (!(grdBackup.View is GridView gridView)) return;
                var fxc =
                    gridView.Columns.OfType<FixedWidthColumn>()
                        .FirstOrDefault(fc => WPFGridViewHelper.GetColumnName(fc) == (string)mi.Tag);
                if (fxc != null)
                    fxc.Visibility = mi.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region List views columns events handlers
        private void NotesColumnVisibilityChanged(object sender, GridColumnVisibilityChangedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (!(sender is FixedWidthColumn column)) return;
                PNData.SaveGridColumnVisibility(column, "grdNotes");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BackColumnVisibilityChanged(object sender, GridColumnVisibilityChangedEventArgs e)
        {
            try
            {
                if (!_AllowColumnsSave) return;
                if (!(sender is FixedWidthColumn column)) return;
                PNData.SaveGridColumnVisibility(column, "grdBackup");
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void NotesColumn_WidthChanged(object sender, GridColumnWidthChangedEventArgs e)
        {
            try
            {
                if (!(sender is FixedWidthColumn fxc)) return;
                var col = _NotesGridCols.FirstOrDefault(c => c.Name == WPFGridViewHelper.GetColumnName(fxc));
                if (col == null) return;
                col.Width = e.ActualWidth;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void BackColumn_WidthChanged(object sender, GridColumnWidthChangedEventArgs e)
        {
            try
            {
                if (!(sender is FixedWidthColumn fxc)) return;
                var col = _BackGridCols.FirstOrDefault(c => c.Name == WPFGridViewHelper.GetColumnName(fxc));
                if (col == null) return;
                col.Width = e.ActualWidth;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion

        #region Context menu and dropdown buttons procedures

        private void PreviewSettings_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                checkCustomColorMenu(db.DropDownMenu);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Diary_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                createDiaryMenu(db.DropDownMenu);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Run_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                createExternalsMenu(db.DropDownMenu);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Switches_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                setSwitches(db.DropDownMenu);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void ListMenu_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mnuDiary.IsEnabled)
                {
                    createDiaryMenu(mnuDiary);
                }
                if (mnuSwitches.IsEnabled)
                {
                    setSwitches(mnuSwitches);
                }
                if (mnuRun.IsEnabled)
                {
                    createExternalsMenu(mnuRun);
                }
                if (mnuTags.IsEnabled)
                {
                    createShowByTagsMenu(mnuTags.Items.OfType<MenuItem>()
                        .FirstOrDefault(it => it.Name == "mnuShowByTag"));
                    createHideByTagsMenu(mnuTags.Items.OfType<MenuItem>()
                        .FirstOrDefault(it => it.Name == "mnuHideByTag"));
                }
                if (mnuPreviewSettings.IsEnabled)
                {
                    checkCustomColorMenu(mnuPreviewSettings);
                }
                if (mnuSearch.IsEnabled)
                {
                    checkIncBinInSearch(mnuSearch);
                    checkIgnoreCaseMenu(mnuSearch);
                }
                if (mnuSendNetwork.IsEnabled)
                {
                    clearContactsMenu(mnuSendNetwork);
                    createContactsMenu(mnuSendNetwork);
                }

                PNMenus.SetEnabledByHiddenMenus(ctmList);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Tags_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                var ddMenu = db.DropDownMenu;
                if (ddMenu == null) return;
                createShowByTagsMenu(ddMenu.Items.OfType<MenuItem>().FirstOrDefault(it => it.Name == "mnuShowByTag"));
                createHideByTagsMenu(ddMenu.Items.OfType<MenuItem>().FirstOrDefault(it => it.Name == "mnuHideByTag"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Search_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is DropDownButton db)) return;
                var ddMenu = db.DropDownMenu;
                if (ddMenu == null) return;
                checkIncBinInSearch(ddMenu);
                checkIgnoreCaseMenu(ddMenu);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion

        private bool _InDrag, _DragReady;
        private Point _StartPoint;
        private bool _inLongOperation;

        private void startDragTreeView()
        {
            try
            {
                _InDrag = true;
                var gr = getSelectedGroup();
                if (gr == null || gr.Id <= (int)SpecialGroups.AllGroups) return;
                _DragSource = DragSource.Tree;
                DragDrop.DoDragDrop(tvwGroups, tvwGroups.SelectedItem, DragDropEffects.Move | DragDropEffects.Scroll);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _DragReady = false;
                _InDrag = false;
                _DragSource = DragSource.None;
                if (_HighLightedItem != null)
                {
                    Utils.SetIsHighlighted(_HighLightedItem, false);
                    _HighLightedItem = null;
                }
            }
        }

        private void startDragListView(DragDropEffects e)
        {
            try
            {
                DataObject dob;
                if (e != DragDropEffects.None)
                {
                    dob = new DataObject("notes_list",
                        Tuple.Create(getSelectedGroup(), grdNotes.SelectedItems.OfType<CPNote>().ToList()));
                }
                else
                {
                    dob = new DataObject("empty_list", "");
                }
                DragDrop.DoDragDrop(grdNotes, dob, e);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _DragSource = DragSource.None;
                if (_HighLightedItem != null)
                {
                    Utils.SetIsHighlighted(_HighLightedItem, false);
                    _HighLightedItem = null;
                }
            }
        }

        private void collectAllBranchIds(PNGroup parentGroup, List<int> ids)
        {
            try
            {
                foreach (var g in parentGroup.Subgroups)
                {
                    collectAllBranchIds(g, ids);
                }
                ids.Add(parentGroup.Id);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (tvwGroups.GetHierarchyObjectAtPoint<ScrollBar>(e.GetPosition(tvwGroups)) is ScrollBar)
                {
                    return;
                }

                if (!(tvwGroups.GetHierarchyObjectAtPoint<PNTreeItem>(e.GetPosition(tvwGroups)) is PNTreeItem)) return;
                _DragReady = true;
                _StartPoint = e.GetPosition(null);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _DragReady = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton != MouseButtonState.Pressed || _InDrag || !_DragReady) return;

                var pt = e.GetPosition(null);
                if (Math.Abs(pt.X - _StartPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(pt.Y - _StartPoint.Y) <= SystemParameters.MinimumVerticalDragDistance) return;
                startDragTreeView();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                switch (_DragSource)
                {
                    case DragSource.ListGroup:
                    case DragSource.ListIncoming:
                        e.Effects = DragDropEffects.Copy | DragDropEffects.Scroll;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_DragLeave(object sender, DragEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                var found = false;
                var item = tvwGroups.GetHierarchyObjectAtPoint<PNTreeItem>(e.GetPosition(tvwGroups)) as PNTreeItem;
                if (item != null)
                {
                    var group = item.Tag as PNGroup;
                    if (group != null)
                    {
                        switch (_DragSource)
                        {
                            case DragSource.Tree:
                                if (e.Data.GetDataPresent("PNotes.NET.PNTreeItem"))
                                {
                                    if (e.Data.GetData("PNotes.NET.PNTreeItem") is PNTreeItem draggedItem)
                                    {
                                        var draggedGroup = draggedItem.Tag as PNGroup;
                                        if (draggedGroup != null)
                                        {
                                            if (group.Id >= (int)SpecialGroups.AllGroups)
                                            {
                                                if (group.Id != draggedGroup.ParentId)
                                                {
                                                    var ids = new List<int>();
                                                    collectAllBranchIds(draggedGroup, ids);
                                                    if (!group.Id.In(ids.ToArray()))
                                                    {
                                                        found = true;
                                                    }
                                                }
                                            }
                                            else if (group.Id == (int)SpecialGroups.RecycleBin)
                                            {
                                                found = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            case DragSource.ListGroup:
                            case DragSource.ListIncoming:
                                if (e.Data.GetDataPresent("notes_list"))
                                {
                                    if (e.Data.GetData("notes_list") is Tuple<PNGroup, List<CPNote>> draggedData)
                                    {
                                        var draggedGroup = draggedData.Item1;
                                        if (group.Id > (int)SpecialGroups.AllGroups)
                                        {
                                            if (group.Id != draggedGroup.Id)
                                            {
                                                found = true;
                                            }
                                        }
                                        else if (group.Id == (int)SpecialGroups.RecycleBin)
                                        {
                                            found = true;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                if (found)
                {
                    e.Effects = DragDropEffects.Move | DragDropEffects.Scroll;
                    if (!item.Equals(_HighLightedItem))
                    {
                        if (_HighLightedItem != null)
                        {
                            Utils.SetIsHighlighted(_HighLightedItem, false);

                        }
                        Utils.SetIsHighlighted(item, true);
                        _HighLightedItem = item;
                    }
                }
                else
                {
                    e.Effects = DragDropEffects.None | DragDropEffects.Scroll;
                    if (_HighLightedItem != null)
                    {
                        Utils.SetIsHighlighted(_HighLightedItem, false);
                        _HighLightedItem = null;
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_HighLightedItem == null) return;
                var png = _HighLightedItem.Tag as PNGroup;
                switch (_DragSource)
                {
                    case DragSource.ListGroup:
                    case DragSource.ListIncoming:
                        if (!e.Data.GetDataPresent("notes_list")) return;
                        if (!(e.Data.GetData("notes_list") is Tuple<PNGroup, List<CPNote>> draggedData)) return;
                        var ids = draggedData.Item2.Select(cpn => cpn.Id).ToArray();
                        if (png == null) return;
                        switch (png.Id)
                        {
                            case (int)SpecialGroups.RecycleBin:
                                deleteSelectedNotes(ids);
                                break;
                            default:
                                changeNotesGroup(png, ids);
                                break;
                        }
                        ((CollectionView)CollectionViewSource.GetDefaultView(grdNotes.ItemsSource)).Refresh
                            ();
                        break;
                    case DragSource.Tree:
                        if (!e.Data.GetDataPresent("PNotes.NET.PNTreeItem")) return;
                        if (!(e.Data.GetData("PNotes.NET.PNTreeItem") is PNTreeItem draggedItem)) return;
                        var draggedGroup = draggedItem.Tag as PNGroup;
                        if (draggedGroup == null) return;
                        if (png == null) return;
                        switch (png.Id)
                        {
                            case (int)SpecialGroups.RecycleBin:
                                deleteGroupOnDrop(draggedGroup, draggedItem);
                                break;
                            default:
                                var parent = PNCollections.Instance.Groups.GetGroupById(draggedGroup.ParentId);
                                if (parent != null)
                                {
                                    parent.Subgroups.RemoveAll(g => g.Id == draggedGroup.Id);
                                }
                                draggedGroup.ParentId = png.Id;
                                parent = PNCollections.Instance.Groups.GetGroupById(draggedGroup.ParentId);
                                if (parent != null)
                                {
                                    parent.Subgroups.Add(draggedGroup);
                                }
                                PNData.SaveGroupNewParent(draggedGroup);

                                if (draggedItem.Parent is PNTreeItem oldParent)
                                {
                                    oldParent.Items.Remove(draggedItem);
                                }

                                insertGroup(draggedGroup, _HighLightedItem, draggedItem);

                                if (!_HighLightedItem.IsExpanded)
                                {
                                    _HighLightedItem.IsExpanded = true;
                                }
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

        private void grdNotes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                grdNotes.Tag = null;

                if (e.ClickCount > 1)
                    return;

                var gr = getSelectedGroup();
                if (gr == null)
                    return;
                if (gr.Id <= (int)SpecialGroups.AllGroups && gr.Id != (int)SpecialGroups.Incoming)
                    return;

                if (!(grdNotes.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdNotes)) is CPNote item))
                    return;

                _StartPoint = e.GetPosition(null);

                if (grdNotes.SelectedItems.Contains(item) && grdNotes.CaptureMouse())
                {
                    e.Handled = true;
                    grdNotes.Tag = item;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdNotes_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    return;
                if (!grdNotes.IsMouseCaptured)
                    return;

                var ef = DragDropEffects.Move | DragDropEffects.Scroll;
                var gr = getSelectedGroup();
                if (gr == null)
                {
                    ef = DragDropEffects.None;
                    _DragSource = DragSource.None;
                }
                else if (gr.Id <= (int)SpecialGroups.AllGroups)
                {
                    if (gr.Id != (int)SpecialGroups.Incoming)
                    {
                        ef = DragDropEffects.None;
                        _DragSource = DragSource.None;
                    }
                    else
                    {
                        _DragSource = DragSource.ListIncoming;
                    }
                }
                else
                {
                    _DragSource = DragSource.ListGroup;
                }

                e.Handled = true;

                var pt = e.GetPosition(null);
                if (Math.Abs(pt.X - _StartPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                    Math.Abs(pt.Y - _StartPoint.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

                grdNotes.ReleaseMouseCapture();

                startDragListView(ef);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdNotes_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdNotes.Tag as CPNote;

                grdNotes.Tag = null;

                if (item == null)
                    return;

                if (!grdNotes.IsMouseCaptured)
                    return;

                e.Handled = true;

                grdNotes.ReleaseMouseCapture();

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (grdNotes.SelectedItems.Contains(item))
                        grdNotes.SelectedItems.Remove(item);
                    else
                        grdNotes.SelectedItems.Add(item);
                    //item.IsSelected = !item.IsSelected;
                }
                else
                {
                    grdNotes.SelectedItems.Clear();
                    grdNotes.SelectedItems.Add(item);
                    //item.IsSelected = true;
                }


                //if (!item.IsKeyboardFocused)
                //    item.Focus();
                grdNotes.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdNotes_LostMouseCapture(object sender, MouseEventArgs e)
        {
            try
            {
                grdNotes.Tag = null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!cmdRemoveGroup.IsEnabled || e.Key != Key.Delete) return;
                groupsActionDeleteGroup();
                e.Handled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdNotes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!cmdDelete.IsEnabled || e.Key != Key.Delete) return;
                actionDeleteNotes();
                e.Handled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void SendContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (!(sender is ItemsControl ctm)) return;
            clearContactsMenu(ctm);
            createContactsMenu(ctm);
        }

        private void clearContactsMenu(ItemsControl parentMenu)
        {
            try
            {
                var count = parentMenu.Items.Count;
                if (count <= 5) return;
                for (var i = count - 3; i > 2; i--)
                {
                    var menu = parentMenu.Items[i] as MenuItem;
                    if (menu != null && menu.Items.Count == 0)
                    {
                        menu.Click -= contactMenu_Click;
                    }
                    else
                    {
                        if (menu != null)
                        {
                            foreach (var mi in menu.Items.OfType<MenuItem>())
                            {
                                mi.Click -= contactMenu_Click;
                            }
                        }
                    }
                    parentMenu.Items.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createContactsMenu(ItemsControl parentMenu)
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
                    Icon = new Image
                    {
                        Source = TryFindResource("contact") as BitmapImage
                    }
                }))
                {
                    mi.Click += contactMenu_Click;
                    parentMenu.Items.Insert(index++, mi);
                }
                // get all other groups
                var groups = PNCollections.Instance.ContactGroups.Where(g => g.Id != -1);
                foreach (var pgroup in groups)
                {
                    var mgi = new MenuItem
                    {
                        Header = pgroup.Name,
                        IsEnabled = false,
                        Icon = new Image
                        {
                            Source = TryFindResource("group") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "group.png"))
                        }
                    };
                    parentMenu.Items.Insert(index++, mgi);
                    var pg = pgroup;
                    contactsGroup = PNCollections.Instance.Contacts.Where(c => c.GroupId == pg.Id);
                    foreach (var mi in contactsGroup.Select(c => new MenuItem
                    {
                        Header = c.Name,
                        Tag = c.Id,
                        Icon = new Image
                        {
                            Source = TryFindResource("contact") as BitmapImage//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "contact.png"))
                        }
                    }))
                    {
                        mi.Click += contactMenu_Click;
                        mgi.Items.Add(mi);
                        mgi.IsEnabled = true;
                    }
                }
                if (index > 3)
                {
                    parentMenu.Items.Insert(index, new Separator());
                }
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
                var notesToSend = getSelectedNotes();
                if (notesToSend == null) return;
                if (!(sender is MenuItem mi)) return;
                var cn = PNCollections.Instance.Contacts.FirstOrDefault(c => c.Id == (int)mi.Tag);
                if (cn == null) return;

                var clientRunner = new PNWCFClientRunner();
                clientRunner.NotesSendNotification += clientRunner_NotesSendNotification;
                clientRunner.NotesSendComplete += clientRunner_NotesSendComplete;
                Task.Factory.StartNew(() => clientRunner.SendNotesToSingleContact(notesToSend, cn));
                PNWindows.Instance.FormMain.ShowSendProgressBaloon();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

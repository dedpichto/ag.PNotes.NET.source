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

using PNotes.NET.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSearchByTags.xaml
    /// </summary>
    public partial class WndSearchByTags
    {
        public WndSearchByTags()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal class AvTag : INotifyPropertyChanged
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

            public string Tag { get; }

            public AvTag(string tag)
            {
                Tag = tag;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal class FoundNote : INotifyPropertyChanged
        {
            private string _iconSource;
            private string _Name;
            private string _Tags;

            public string IconSource
            {
                get => _iconSource;
                set
                {
                    if (Equals(_iconSource, value)) return;
                    _iconSource = value;
                    OnPropertyChanged();
                }
            }

            public string Name
            {
                get => _Name;
                set
                {
                    if (_Name == value) return;
                    _Name = value;
                    OnPropertyChanged();
                }
            }

            public string Tags
            {
                get => _Tags;
                set
                {
                    if (value == _Tags) return;
                    _Tags = value;
                    OnPropertyChanged();
                }
            }

            public string Id { get; }

            public FoundNote(string image, string name, string tags, string id)
            {
                IconSource = image;
                Name = name;
                Tags = tags;
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

        private readonly List<AvTag> _Tags = new List<AvTag>();
        private readonly ObservableCollection<FoundNote> _Notes = new ObservableCollection<FoundNote>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("serach_tags", "Search by tags");
                loadTags();
                PNWindows.Instance.FormMain.NoteBooleanChanged += FormMain_NoteBooleanChanged;
                PNWindows.Instance.FormMain.NoteNameChanged += FormMain_NoteNameChanged;
                PNWindows.Instance.FormMain.NoteTagsChanged += FormMain_NoteTagsChanged;
                PNWindows.Instance.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                grdTagsResults.ItemsSource = _Notes;
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PNWindows.Instance.FormMain.NoteBooleanChanged -= FormMain_NoteBooleanChanged;
            PNWindows.Instance.FormMain.NoteNameChanged -= FormMain_NoteNameChanged;
            PNWindows.Instance.FormMain.NoteTagsChanged -= FormMain_NoteTagsChanged;
            PNWindows.Instance.FormMain.LanguageChanged -= FormMain_LanguageChanged;
            PNWindows.Instance.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
            PNWindows.Instance.FormSearchByTags = null;
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAll.IsChecked == null) return;
                foreach (var t in _Tags)
                {
                    t.Selected = chkAll.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdTagsResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdTagsResults.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdTagsResults)) is FoundNote item)) return;
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

        private void loadTags()
        {
            try
            {
                foreach (var t in PNCollections.Instance.Tags)
                {
                    _Tags.Add(new AvTag(t));
                }
                grdAvailableTags.ItemsSource = _Tags;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void findNotes()
        {
            try
            {
                _Notes.Clear();
                var tags = _Tags.Where(t => t.Selected).Select(t => t.Tag);
                var notes = PNCollections.Instance.Notes.Where(n => n.GroupId != (int)SpecialGroups.RecycleBin);
                foreach (var note in notes)
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt))) continue;
                    var key = PNNotesOperations.GetNoteImage(note);
                    _Notes.Add(new FoundNote(key, note.Name, note.Tags.ToCommaSeparatedString(), note.Id));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        void FormMain_NoteScheduleChanged(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is PNote note) || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (item == null) return;
                item.IconSource = PNNotesOperations.GetNoteImage(note);
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
                if (!(sender is PNote note) || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (item == null) return;
                item.IconSource = PNNotesOperations.GetNoteImage(note);
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
                if (!(sender is PNote note) || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (item == null) return;
                item.Name = note.Name;
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
                if (!(sender is PNote note) || _Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.Id);
                var tags = _Tags.Where(t => t.Selected).Select(t => t.Tag);
                if (item == null)   //note was not in list
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt))) return;
                    var key = PNNotesOperations.GetNoteImage(note);
                    _Notes.Add(new FoundNote(key, note.Name, note.Tags.ToCommaSeparatedString(), note.Id));
                }
                else   //note was in list
                {
                    if (!note.Tags.Any(nt => tags.Any(t => t == nt)))
                    {
                        _Notes.Remove(item);
                    }
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
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("serach_tags", "Search by tags");
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
                    case CommandType.Find:
                        e.CanExecute = _Tags.Any(t => t.Selected);
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = true;
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
                    case CommandType.Find:
                        findNotes();
                        break;
                    case CommandType.Cancel:
                        Close();
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

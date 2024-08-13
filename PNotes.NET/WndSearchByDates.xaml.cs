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
    /// Interaction logic for WndSearchByDates.xaml
    /// </summary>
    public partial class WndSearchByDates
    {
        public WndSearchByDates()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private sealed class SearchField : INotifyPropertyChanged
        {
            private bool _Selected;
            private string _Name;

            public bool Selected
            {
                get => _Selected;
                set
                {
                    if (value.Equals(_Selected)) return;
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }

            public string Name
            {
                private get => _Name;
                set
                {
                    if (value == _Name) return;
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }

            public SearchField(string name)
            {
                Name = name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class Result : INotifyPropertyChanged
        {
            private string _Name;
            private DateTime _Created;
            private DateTime _Saved;
            private DateTime _Sent;
            private DateTime _Received;
            private DateTime _Deleted;
            private string _iconSource;

            public string Id { get; set; }

            public string IconSource
            {
                private get => _iconSource;
                set
                {
                    if (Equals(value, _iconSource)) return;
                    _iconSource = value;
                    OnPropertyChanged();
                }
            }

            public string Name
            {
                private get => _Name;
                set
                {
                    if (value == _Name) return;
                    _Name = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Created
            {
                private get => _Created;
                set
                {
                    if (value.Equals(_Created)) return;
                    _Created = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Saved
            {
                private get => _Saved;
                set
                {
                    if (value.Equals(_Saved)) return;
                    _Saved = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Sent
            {
                private get => _Sent;
                set
                {
                    if (value.Equals(_Sent)) return;
                    _Sent = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Received
            {
                private get => _Received;
                set
                {
                    if (value.Equals(_Received)) return;
                    _Received = value;
                    OnPropertyChanged();
                }
            }

            public DateTime Deleted
            {
                private get => _Deleted;
                set
                {
                    if (value.Equals(_Deleted)) return;
                    _Deleted = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly ObservableCollection<SearchField> _Fields = new ObservableCollection<SearchField>();
        private readonly ObservableCollection<Result> _Notes = new ObservableCollection<Result>();

        private void DlgSearchByDates_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (grdDates.View is GridView grid)
                {
                    for (var i = 1; i < grid.Columns.Count; i++)
                    {
                        _Fields.Add(new SearchField(grid.Columns[i].Header.ToString()));
                    }
                }
                applyLanguage();

                PNWindows.Instance.FormMain.NoteBooleanChanged += FormMain_NoteBooleanChanged;
                PNWindows.Instance.FormMain.LanguageChanged += FormMain_LanguageChanged;
                PNWindows.Instance.FormMain.NoteNameChanged += FormMain_NoteNameChanged;
                PNWindows.Instance.FormMain.NoteDateChanged += FormMain_NoteDateChanged;
                PNWindows.Instance.FormMain.NoteScheduleChanged += FormMain_NoteScheduleChanged;
                grdFields.ItemsSource = _Fields;
                grdDates.ItemsSource = _Notes;
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
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

        void FormMain_NoteDateChanged(object sender, NoteDateChangedEventArgs e)
        {
            try
            {
                if (!(sender is PNote note)) return;
                switch (e.Type)
                {
                    case NoteDateType.Creation:
                        _Notes.Add(new Result
                        {
                            Name = note.Name,
                            Id = note.Id,
                            Created = note.DateCreated,
                            Saved = note.DateSaved
                        });
                        break;
                    case NoteDateType.Saving:
                    case NoteDateType.Sending:
                    case NoteDateType.Receiving:
                    case NoteDateType.Deletion:
                        changeNoteDate(note, e.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void changeNoteDate(PNote note, NoteDateType type)
        {
            try
            {
                if (_Notes.Count == 0) return;
                var item = _Notes.FirstOrDefault(n => n.Id == note.Id);
                if (item == null) return;
                switch (type)
                {
                    case NoteDateType.Saving:
                        item.Saved = note.DateSaved;
                        break;
                    case NoteDateType.Sending:
                        item.Sent = note.DateSent;
                        break;
                    case NoteDateType.Receiving:
                        item.Received = note.DateReceived;
                        break;
                    case NoteDateType.Deletion:
                        item.Deleted = note.DateDeleted;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }


        void FormMain_NoteNameChanged(object sender, NoteNameChangedEventArgs e)
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

        void FormMain_LanguageChanged(object sender, EventArgs e)
        {
            try
            {
                applyLanguage();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        void FormMain_NoteBooleanChanged(object sender, NoteBooleanChangedEventArgs e)
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

        private void DlgSearchByDates_Closed(object sender, EventArgs e)
        {
            PNWindows.Instance.FormMain.NoteBooleanChanged -= FormMain_NoteBooleanChanged;
            PNWindows.Instance.FormMain.LanguageChanged -= FormMain_LanguageChanged;
            PNWindows.Instance.FormMain.NoteNameChanged -= FormMain_NoteNameChanged;
            PNWindows.Instance.FormMain.NoteDateChanged -= FormMain_NoteDateChanged;
            PNWindows.Instance.FormMain.NoteScheduleChanged -= FormMain_NoteScheduleChanged;
            PNWindows.Instance.FormSearchByDates = null;
        }

        private void DlgSearchByDates_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkAll.IsChecked == null) return;
                foreach (var f in _Fields)
                    f.Selected = chkAll.IsChecked.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdDates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdDates.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdDates)) is Result item)) return;
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

        private void applyLanguage()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("search_dates", "Search by dates");
                if (grdDates.View is GridView grid)
                {
                    for (var i = 1; i < grid.Columns.Count; i++)
                    {
                        _Fields[i - 1].Name = grid.Columns[i].Header.ToString();
                    }
                }
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
                var from = dtpFrom.DateValue;
                var to = dtpTo.DateValue;
                foreach (var note in PNCollections.Instance.Notes)
                {
                    if (_Fields[0].Selected)
                    {
                        if (note.DateCreated >= from && note.DateCreated <= to)
                        {
                            addNote(note);
                            continue;
                        }
                    }
                    if (_Fields[1].Selected)
                    {
                        if (note.DateSaved != DateTime.MinValue && note.DateSaved >= from && note.DateSaved <= to)
                        {
                            addNote(note);
                            continue;
                        }
                    }
                    if (_Fields[2].Selected)
                    {
                        if (note.DateSent != DateTime.MinValue && note.DateSent >= from && note.DateSent <= to)
                        {
                            addNote(note);
                            continue;
                        }
                    }
                    if (_Fields[3].Selected)
                    {
                        if (note.DateReceived != DateTime.MinValue && note.DateReceived >= from && note.DateReceived <= to)
                        {
                            addNote(note);
                            continue;
                        }
                    }
                    if (_Fields[4].Selected)
                    {
                        if (note.DateDeleted != DateTime.MinValue && note.DateDeleted >= from && note.DateDeleted <= to)
                        {
                            addNote(note);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addNote(PNote note)
        {
            try
            {
                var key = PNNotesOperations.GetNoteImage(note);
                _Notes.Add(new Result
                {
                    IconSource = key,
                    Created = note.DateCreated,
                    Saved = note.DateSaved,
                    Deleted = note.DateDeleted,
                    Name = note.Name,
                    Received = note.DateReceived,
                    Sent = note.DateSent,
                    Id = note.Id
                });
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
                        e.CanExecute = _Fields.Any(f => f.Selected);
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

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

using PNRichEdit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndOverdue.xaml
    /// </summary>
    public partial class WndOverdue
    {
        public WndOverdue()
        {
            InitializeComponent();
            initializeEdit();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndOverdue(List<PNote>notes) : this()
        {
            _Notes = notes;
        }

        private readonly List<PNote> _Notes;
        private readonly List<OverdueNote> _OverdueNotes = new List<OverdueNote>();
        private readonly DayOfWeekStruct[] _doWeek = new DayOfWeekStruct[7];
        private PNRichEditBox _Edit;
        private EditControl _EditControl;

        private void DlgOverdue_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                createDayOfWeekArray();
                insertNotesIntoList();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertNotesIntoList()
        {
            try
            {
                foreach (var n in _Notes)
                {
                    _OverdueNotes.Add(new OverdueNote(n.Name,
                        PNLang.Instance.GetNoteScheduleDescription(n.Schedule, _doWeek), n.Schedule.TimeZone.ToString(), n.Id));
                }
                grdOverdue.ItemsSource = _OverdueNotes;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void createDayOfWeekArray()
        {
            try
            {
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                var values = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
                for (var i = 0; i < values.Length; i++)
                {
                    _doWeek[i] = new DayOfWeekStruct { DayOfW = values[i], Name = ci.DateTimeFormat.DayNames[i] };
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdOverdue_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(grdOverdue.SelectedItem is OverdueNote item)) return;
                var note = PNCollections.Instance.Notes.Note(item.Id);
                if (note != null)
                {
                    PNNotesOperations.AdjustNoteSchedule(note, this);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdOverdue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(grdOverdue.SelectedItem is OverdueNote item)) return;
            loadNotePreview(item.Id);
        }

        private void initializeEdit()
        {
            try
            {
                _EditControl = new EditControl(brdHost);
                _Edit = _EditControl.EditBox;
                _Edit.ReadOnly = true;
                var clr = PNSkinlessDetails.DefColor;
                _EditControl.WinForm.BackColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadNotePreview(string id)
        {
            try
            {
                var note = PNCollections.Instance.Notes.Note(id);
                if (note == null) return;
                var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                if (File.Exists(path))
                {
                    PNNotesOperations.LoadNoteFile(_Edit, path);
                    if (!PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
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

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }

    public class OverdueNote
    {
        public string Name { get; }
        public string Schedule { get; }
        public string Timezone { get; }
        public string Id { get; }

        public OverdueNote(string name, string schedule, string timezone, string id)
        {
            Name = name;
            Schedule = schedule;
            Timezone = timezone;
            Id = id;
        }
    }
}

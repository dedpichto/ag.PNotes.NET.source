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

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndLinkToNote.xaml
    /// </summary>
    public partial class WndLinkToNote
    {
        internal event EventHandler<CustomRtfReadyEventArgs> LinkReady;

        private sealed class AvailableNote
        {
            public string Name { get; set; }
            public DateTime Created { get; set; }
            public DateTime Saved { get; set; }
            public string Id { get; set; }
            public string Group { get; set; }
        }

        public WndLinkToNote()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        public WndLinkToNote(string id) : this()
        {
            _Id = id;
        }

        //public WndLinkToNote(string id, string selectedText)
        //    : this()
        //{
        //    _SelectedText = selectedText;
        //    _Id = id;
        //}

        //private const string LINK_HERE = "XXX_LINK_HERE_XXX";
        //private const string TEXT_HERE = "XXX_TEXT_HERE_XXX";
        //private const string PATTERN =
        //    @"{\rtf1\ansi\deff0{{\field{\*\fldinst{HYPERLINK file:XXX_LINK_HERE_XXX }}{\fldrslt{XXX_TEXT_HERE_XXX\ul0\cf0}}}}}";
        private readonly ObservableCollection<AvailableNote> _AvailableNotes = new ObservableCollection<AvailableNote>();
        //private readonly string _SelectedText;
        private readonly string _Id;

        private void DlgLinkToNote_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                foreach (
                    var note in
                    PNCollections.Instance.Notes.Where(n => n.Id != _Id && n.GroupId != (int)SpecialGroups.RecycleBin).OrderBy(n => n.Name))
                {
                    var av = new AvailableNote
                    {
                        Name = note.Name,
                        Id = note.Id,
                        Created = note.DateCreated,
                        Saved = note.DateSaved
                    };
                    var gr = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                    if (gr != null)
                        av.Group = gr.Name;
                    _AvailableNotes.Add(av);
                }
                grdLinks.ItemsSource = _AvailableNotes;
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void oKClick()
        {
            try
            {
                if (!(grdLinks.SelectedItem is AvailableNote item)) return;
                var result = '<' + "file:" + PNStrings.LINK_PFX + item.Name + "|" + item.Id + '>';
                //var rtf = PATTERN.Replace(LINK_HERE, PNStrings.LINK_PFX + item.Id);
                //rtf = rtf.Replace(TEXT_HERE, string.IsNullOrWhiteSpace(_SelectedText) ? item.Name : _SelectedText);
                if (LinkReady == null) return;
                LinkReady(this, new CustomRtfReadyEventArgs(result));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdDates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (grdLinks.SelectedItems.Count == 1)
                oKClick();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        e.CanExecute = grdLinks?.SelectedItems.Count == 1;
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = true;
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
                    case CommandType.Ok:
                        oKClick();
                        break;
                    case CommandType.Cancel:
                        DialogResult = false;
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

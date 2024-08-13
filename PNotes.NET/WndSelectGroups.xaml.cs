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
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSelectGroups.xaml
    /// </summary>
    public partial class WndSelectGroups
    {
        internal event EventHandler<ContactsSelectedEventArgs> ContactsSelected;

        private class ContactGroup
        {
            public string Name;
            public int Id;
            public override string ToString()
            {
                return Name;
            }
        }

        public WndSelectGroups()
        {
            InitializeComponent();
        }

        private void DlgSelectGroups_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = PNLang.Instance.GetControlText(Name, "Contacts groups");
                PNLang.Instance.ApplyControlLanguage(this);
                if (PNCollections.Instance.Contacts.Any(c => c.GroupId == -1))
                {
                    var gr = new ContactGroup { Name = "(None)", Id = -1 };
                    var item = new PNListBoxItem(null, gr.Name, gr, gr.Name, false);
                    lstGroups.Items.Add(item);
                }
                foreach (var cg in PNCollections.Instance.ContactGroups.Where(cg => PNCollections.Instance.Contacts.Any(c => c.GroupId == cg.Id)))
                {
                    var gr = new ContactGroup { Name = cg.Name, Id = cg.Id };
                    var item = new PNListBoxItem(null, gr.Name, gr, gr.Name, false);
                    lstGroups.Items.Add(item);
                }
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
                var cse = new ContactsSelectedEventArgs();
                foreach (var pg in lstGroups.Items.OfType<PNListBoxItem>().Where(p => p.IsChecked.HasValue && p.IsChecked.Value))
                {
                    var g = pg.Tag as ContactGroup;
                    var contacts = PNCollections.Instance.Contacts.Where(c => g != null && c.GroupId == g.Id);
                    foreach (var c in contacts)
                    {
                        cse.Contacts.Add(c);
                    }
                }

                ContactsSelected?.Invoke(this, cse);
                DialogResult = true;
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
                    case CommandType.Ok:
                        e.CanExecute = lstGroups.Items.OfType<PNListBoxItem>()
                            .Any(p => p.IsChecked.HasValue && p.IsChecked.Value);
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
                PNStatic.LogException(ex, false);
            }
        }
    }
}

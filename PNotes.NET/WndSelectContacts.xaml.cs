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
    /// Interaction logic for WndSelectContacts.xaml
    /// </summary>
    public partial class WndSelectContacts
    {
        internal event EventHandler<ContactsSelectedEventArgs> ContactsSelected;

        public WndSelectContacts()
        {
            InitializeComponent();
        }

        private void DlgSelectContacts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = PNLang.Instance.GetControlText("lblContacts", "Contacts");
                PNLang.Instance.ApplyControlLanguage(this);
                foreach (var c in PNCollections.Instance.Contacts)
                {
                    var pti = new PNListBoxItem(null, c.Name, c, c.Name, false);
                    lstContacts.Items.Add(pti);
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
                foreach (
                    var pti in
                        lstContacts.Items.OfType<PNListBoxItem>().Where(p => p.IsChecked.HasValue && p.IsChecked.Value))
                {
                    cse.Contacts.Add(pti.Tag as PNContact);
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
                        e.CanExecute = lstContacts.Items.OfType<PNListBoxItem>()
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

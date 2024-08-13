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
using System.Windows;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndGroups.xaml
    /// </summary>
    public partial class WndGroups
    {
        internal event EventHandler<ContactGroupChangedEventArgs> ContactGroupChanged;

        public WndGroups()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndGroups(int newId):this()
        {
            _Mode = AddEditMode.Add;
            _NewId = newId;
        }

        internal WndGroups(PNContactGroup cg):this()
        {
            _Mode = AddEditMode.Edit;
            _Group = cg;
        }

        private readonly AddEditMode _Mode;
        private readonly PNContactGroup _Group;
        private readonly int _NewId;

        private void oKClick()
        {
            try
            {
                ContactGroupChangedEventArgs ce;
                if (_Mode == AddEditMode.Add)
                {
                    var cg = new PNContactGroup { Name = txtGroupName.Text.Trim(), Id = _NewId };
                    ce = new ContactGroupChangedEventArgs(cg, _Mode);
                }
                else
                {
                    _Group.Name = txtGroupName.Text.Trim();
                    ce = new ContactGroupChangedEventArgs(_Group, _Mode);
                }

                ContactGroupChanged?.Invoke(this, ce);
                if (!ce.Accepted)
                {
                    txtGroupName.SelectAll();
                    txtGroupName.Focus();
                    return;
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgGroups_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("group_new", "New group of contacts");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("group_edit", "Edit group of contacts");
                    txtGroupName.Text = _Group.Name;
                }
                txtGroupName.SelectAll();
                txtGroupName.Focus();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        e.CanExecute = txtGroupName.Text.Trim().Length > 0;
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

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
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

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
    /// Interaction logic for WndDeleteContactsGroup.xaml
    /// </summary>
    public partial class WndDeleteContactsGroup
    {
        public WndDeleteContactsGroup()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal DeleteContactsGroupBehavior DeleteBehavior { get; private set; }

        private void DlgDeleteContactsGroup_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            optContsToNone.Content = optContsToNone.Content + " '" +
                                     PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP) + "'";
            FlowDirection = PNLang.Instance.GetFlowDirection();
        }

        private void oKClick()
        {
            DeleteBehavior = optContsToNone.IsChecked != null && optContsToNone.IsChecked.Value
                ? DeleteContactsGroupBehavior.Move
                : DeleteContactsGroupBehavior.DeleteAll;
            DialogResult = true;
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
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

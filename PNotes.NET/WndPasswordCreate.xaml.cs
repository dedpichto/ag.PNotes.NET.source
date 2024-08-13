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

using PNEncryption;
using System;
using System.Windows;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndPasswordCreate.xaml
    /// </summary>
    public partial class WndPasswordCreate
    {
        public WndPasswordCreate()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndPasswordCreate(string textAddition)
            : this()
        {
            _TextAddition = textAddition;
        }

        internal event EventHandler<PasswordChangedEventArgs> PasswordChanged;

        private readonly string _TextAddition = "";

        private void DlgPasswordCreate_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            Title = PNLang.Instance.GetCaptionText("pwrd_create", "Create password") + _TextAddition;
            txtEnterPwrd.Focus();
            ToolTip = Title;
            FlowDirection = PNLang.Instance.GetFlowDirection();
        }

        private void oKClick()
        {
            try
            {
                if (txtEnterPwrd.Password != txtConfirmPwrd.Password)
                {
                    var message = PNLang.Instance.GetMessageText("pwrd_not_identical", "Both password strings should be identical. Please, check the spelling.");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtEnterPwrd.Focus();
                    txtEnterPwrd.SelectAll();
                    return;
                }
                var hash = PNEncryptor.GetHashString(txtEnterPwrd.Password.Trim());
                if (hash != null)
                {
                    PasswordChanged?.Invoke(this, new PasswordChangedEventArgs(hash));
                }
                DialogResult = true;
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
                        e.CanExecute = txtEnterPwrd.Password.Trim().Length > 0 && txtConfirmPwrd.Password.Trim().Length > 0;
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

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
    /// Interaction logic for WndPasswordDelete.xaml
    /// </summary>
    public partial class WndPasswordDelete
    {
        public WndPasswordDelete()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndPasswordDelete(PasswordDlgMode mode)
            : this()
        {
            _Mode = mode;
            if (mode == PasswordDlgMode.LoginMain)
            {
                Topmost = true;
            }
        }

        internal WndPasswordDelete(PasswordDlgMode mode, string additionalText, string hash)
            : this()
        {
            _Mode = mode;
            _AdditionalText = additionalText;
            _Hash = hash;
            if (mode == PasswordDlgMode.LoginMain)
            {
                Topmost = true;
            }
        }

        internal event EventHandler PasswordDeleted;

        private readonly PasswordDlgMode _Mode;
        private readonly string _AdditionalText = "";
        private readonly string _Hash = "";

        private void oKClick()
        {
            try
            {
                var hash = PNEncryptor.GetHashString(txtEnterPwrd.Password.Trim());
                var hashCheck = (_Mode == PasswordDlgMode.DeleteMain || _Mode == PasswordDlgMode.LoginMain)
                    ? PNRuntimes.Instance.Settings.Protection.PasswordString
                    : _Hash;
                if (hash != null)
                {
                    if (hash != hashCheck)
                    {
                        var message = PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password");
                        WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        txtEnterPwrd.Focus();
                        txtEnterPwrd.SelectAll();
                        return;
                    }
                }
                if (_Mode == PasswordDlgMode.DeleteMain || _Mode == PasswordDlgMode.DeleteGroup || _Mode == PasswordDlgMode.DeleteNote)
                {
                    PasswordDeleted?.Invoke(this, new EventArgs());
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgPasswordDelete_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == PasswordDlgMode.DeleteMain || _Mode == PasswordDlgMode.DeleteGroup || _Mode == PasswordDlgMode.DeleteNote)
                {
                    Title = PNLang.Instance.GetCaptionText("pwrd_delete", "Delete password");
                    if (_Mode != PasswordDlgMode.DeleteGroup && _Mode != PasswordDlgMode.DeleteNote)
                    {
                        Activate();
                        txtEnterPwrd.Focus();
                        return;
                    }
                    Title += _AdditionalText;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("pwrd_login", "Enter password");
                    if (_Mode != PasswordDlgMode.LoginGroup && _Mode != PasswordDlgMode.LoginNote)
                    {
                        Activate();
                        txtEnterPwrd.Focus();
                        return;
                    }
                    Title += _AdditionalText;
                }
                ToolTip = Title;
                Activate();
                txtEnterPwrd.Focus();
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
                        e.CanExecute = txtEnterPwrd.Password.Trim().Length > 0;
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

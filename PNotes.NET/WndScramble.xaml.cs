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
using System.Windows.Controls;
using PNRichEdit;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndScramble.xaml
    /// </summary>
    public partial class WndScramble
    {
        public WndScramble()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndScramble(ScrambleMode mode, PNote note, PNRichEditBox edit)
            : this()
        {
            _Mode = mode;
            _Note = note;
            _Edit = edit;
        }

        private readonly ScrambleMode _Mode;
        private readonly PNote _Note;
        private readonly PNRichEditBox _Edit;

        private void DlgScramble_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == ScrambleMode.Scramble)
                {
                    Title = PNLang.Instance.GetCaptionText("scramble_caption", "Encrypt note:") + @" " + _Note.Name;
                    lblScrambleWarning.Text = PNLang.Instance.GetCaptionText("scramble_warning",
                        "Encryption of note's text will remove all rich text formatting!");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("unscramble_caption", "Decrypt note:") + @" " + _Note.Name;
                    lblScrambleWarning.Text = "";
                }
                pwrdKey.Focus();
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
                using (var enc = new PNEncryptor(txtKey.Text.Trim()))
                {
                    _Edit.Text = _Mode == ScrambleMode.Scramble
                        ? enc.EncryptStringWithTrim(_Edit.Text.Trim())
                        : enc.DecryptStringWithTrim(_Edit.Text);
                }
                DialogResult = true;
            }
            catch (System.Security.Cryptography.CryptographicException crex)
            {
                PNStatic.LogException(crex, false);
                WPFMessageBox.Show(this, PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                    PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex, false);
            }
        }

        private void chkSmtpShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            pwrdKey.Visibility = Visibility.Collapsed;
            txtKey.Visibility = Visibility.Visible;
            txtKey.Focus();
            txtKey.SelectionStart = txtKey.Text.Length;
        }

        private void chkSmtpShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            pwrdKey.Visibility = Visibility.Visible;
            txtKey.Visibility = Visibility.Collapsed;
            pwrdKey.Focus();
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        e.CanExecute = txtKey.Text.Trim().Length > 0;
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
                PNStatic.LogException(ex, false);
            }
        }
    }
}

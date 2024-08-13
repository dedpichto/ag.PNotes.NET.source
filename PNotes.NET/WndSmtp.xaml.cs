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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSmtp.xaml
    /// </summary>
    public partial class WndSmtp
    {
        internal event EventHandler<SmtpChangedEventArgs> SmtpChanged;

        public WndSmtp()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSmtp(PNSmtpProfile client)
            : this()
        {
            _Mode = client == null ? AddEditMode.Add : AddEditMode.Edit;
            _Client = client == null ? new PNSmtpProfile() : client.Clone();
        }

        private readonly AddEditMode _Mode;
        private readonly PNSmtpProfile _Client;

        private void DlgSmtp_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Edit)
                {
                    Title = PNLang.Instance.GetCaptionText("smtp_edit", "Edit SMTP client");
                    txtSmtpHost.Text = _Client.HostName;
                    txtSmtpAddress.Text = _Client.SenderAddress;
                    txtSmtpDisplayName.Text = _Client.DisplayName;
                    txtSmtpPort.Value = _Client.Port;
                    using (var encryptor = new PNEncryptor(PNKeys.ENC_KEY))
                    {
                        txtSmtpPassword.Password = encryptor.DecryptString(_Client.Password);
                    }
                    cmdOK.IsEnabled = true;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("smtp_new", "New SMTP client");
                }
                txtSmtpHost.SelectAll();
                txtSmtpHost.Focus();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void chkSmtpShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkSmtpShowPassword.IsChecked == null) return;
                if (chkSmtpShowPassword.IsChecked.Value)
                {
                    txtKey.Visibility = Visibility.Visible;
                    txtSmtpPassword.Visibility = Visibility.Hidden;
                }
                else
                {
                    txtKey.Visibility = Visibility.Hidden;
                    txtSmtpPassword.Visibility = Visibility.Visible;
                }
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
                var hostNameType = Uri.CheckHostName(txtSmtpHost.Text.Trim());
                if (hostNameType == UriHostNameType.Unknown)
                {
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_host", "Invalid host name"),
                        PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSmtpHost.SelectAll();
                    txtSmtpHost.Focus();
                    return;
                }
                var rg = new Regex(PNStrings.MAIL_PATTERN, RegexOptions.IgnoreCase);
                var match = rg.Match(txtSmtpAddress.Text.Trim());
                if (!match.Success)
                {
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_email", "Invalid e-mail address"),
                       PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSmtpAddress.SelectAll();
                    txtSmtpAddress.Focus();
                    return;
                }
                _Client.HostName = txtSmtpHost.Text.Trim();
                _Client.SenderAddress = txtSmtpAddress.Text.Trim();
                using (var encryptor = new PNEncryptor(PNKeys.ENC_KEY))
                {
                    _Client.Password = encryptor.EncryptString(txtSmtpPassword.Password);
                }
                _Client.Port = Convert.ToInt32(txtSmtpPort.Value);
                _Client.DisplayName = txtSmtpDisplayName.Text.Trim().Length > 0 ? txtSmtpDisplayName.Text.Trim() : _Client.SenderAddress;
                if (SmtpChanged != null)
                {
                    var ev = new SmtpChangedEventArgs(_Client, _Mode);
                    SmtpChanged(this, ev);
                    if (!ev.Accepted)
                    {
                        txtSmtpAddress.SelectAll();
                        txtSmtpAddress.Focus();
                        return;
                    }
                }
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
                        e.CanExecute = txtSmtpAddress.Text.Trim().Length > 0 && txtSmtpHost.Text.Trim().Length > 0 &&
                                       txtSmtpPassword.Password.Trim().Length > 0 && txtSmtpPort.Value > 0;
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

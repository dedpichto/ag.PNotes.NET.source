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
using System.Text.RegularExpressions;
using System.Windows;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndMailContact.xaml
    /// </summary>
    public partial class WndMailContact
    {
        internal event EventHandler<MailContactChangedEventArgs> MailContactChanged;

        public WndMailContact()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndMailContact(PNMailContact contact) : this()
        {
            _Contact = contact != null ? contact.Clone() : new PNMailContact();
            _Mode = contact == null ? AddEditMode.Add : AddEditMode.Edit;
        }

        private readonly PNMailContact _Contact;
        private readonly AddEditMode _Mode;

        private void DlgMailContact_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Edit)
                {
                    Title = PNLang.Instance.GetCaptionText("mail_contact_edit", "Edit mail contact");
                    txtMailDisplayName.Text = _Contact.DisplayName;
                    txtMailAddress.Text = _Contact.Address;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("mail_contact_new", "New mail contact");
                }
                txtMailDisplayName.SelectAll();
                txtMailDisplayName.Focus();
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
                var rg = new Regex(PNStrings.MAIL_PATTERN, RegexOptions.IgnoreCase);
                var match = rg.Match(txtMailAddress.Text.Trim());
                if (!match.Success)
                {
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_email", "Invalid e-mail address"),
                       PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtMailAddress.SelectAll();
                    txtMailAddress.Focus();
                    return;
                }
                _Contact.DisplayName = txtMailDisplayName.Text.Trim();
                _Contact.Address = txtMailAddress.Text.Trim();
                if (MailContactChanged != null)
                {
                    var en = new MailContactChangedEventArgs(_Contact, _Mode);
                    MailContactChanged(this, en);
                    if (!en.Accepted)
                        return;
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
                        e.CanExecute = txtMailDisplayName.Text.Trim().Length > 0 &&
                                       txtMailAddress.Text.Trim().Length > 0;
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

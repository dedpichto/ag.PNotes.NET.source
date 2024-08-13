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

using Domino;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndLotusCredentials.xaml
    /// </summary>
    public partial class WndLotusCredentials
    {
        internal event EventHandler<LotusCredentialSetEventArgs> LotusCredentialSet;

        public WndLotusCredentials()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private NotesSession _LocalNotesSession;
        private NotesSession _ServerNotesSession;

        private void DlgLotusCred_Loaded(object sender, RoutedEventArgs e)
        {
            PNLang.Instance.ApplyControlLanguage(this);
            txtPassword.Focus();
            FlowDirection = PNLang.Instance.GetFlowDirection();
        }

        private void oKClick()
        {
            try
            {
                if (_LocalNotesSession == null)
                {
                    _LocalNotesSession = new NotesSessionClass();
                    //Initializing Lotus Notes Session
                    try
                    {
                        _LocalNotesSession.Initialize(txtPassword.Password);
                    }
                    catch (COMException cex)
                    {
                        if (cex.ErrorCode != -2147217504) throw;
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                //Creating Lotus Notes DataBase Object
                var localDatabase = _LocalNotesSession.GetDatabase("", "names.nsf", false);

                if (_ServerNotesSession == null)
                {
                    _ServerNotesSession = new NotesSessionClass();
                    //Initializing Lotus Notes Session
                    try
                    {
                        _ServerNotesSession.Initialize(txtPassword.Password);
                    }
                    catch (COMException cex)
                    {
                        if (cex.ErrorCode != -2147217504) throw;
                        WPFMessageBox.Show(PNLang.Instance.GetMessageText("pwrd_not_match", "Invalid password"),
                            PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                //Creating Lotus Notes DataBase Object
                var serverDatabase = _ServerNotesSession.GetDatabase(txtServer.Text.Trim(), "names.nsf", false);

                //creating Lotus Notes Contact View
                NotesView contactsView = null, peopleView = null;
                if (localDatabase != null)
                    contactsView = localDatabase.GetView("Contacts");
                if (serverDatabase != null)
                    peopleView = serverDatabase.GetView("$People");

                LotusCredentialSet?.Invoke(this, new LotusCredentialSetEventArgs(contactsView, peopleView));
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
                        e.CanExecute = txtPassword.Password.Trim().Length > 0;
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

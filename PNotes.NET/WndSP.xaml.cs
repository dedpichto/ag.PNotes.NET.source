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
using System.Windows.Input;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndSP.xaml
    /// </summary>
    public partial class WndSP
    {
        public WndSP()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndSP(WndSettings prefs)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Add;
        }

        internal WndSP(WndSettings prefs, PNSearchProvider sp)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Edit;
            _SearchProviders = sp;
        }

        private readonly WndSettings _Prefs;
        private readonly AddEditMode _Mode;
        private PNSearchProvider _SearchProviders;

        private void DlgSP_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("sp_new", "New search provider");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("sp_edit", "Edit search provider");
                    txtSPName.Text = _SearchProviders.Name;
                    txtSPQuery.Text = _SearchProviders.QueryString;
                    txtSPName.IsReadOnly = true;
                }
                txtSPName.Focus();
                txtSPName.SelectAll();
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
                var name = txtSPName.Text.Trim();
                if (_Mode == AddEditMode.Add && _Prefs.SearchProviderExists(name))
                {
                    var message = PNLang.Instance.GetMessageText("sp_exists", "Search provider with this name already exists");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (_Mode == AddEditMode.Add)
                    {
                        _SearchProviders = new PNSearchProvider { Name = name, QueryString = txtSPQuery.Text.Trim() };
                        _Prefs.SearchProviderAdd(_SearchProviders);
                    }
                    else
                    {
                        _SearchProviders.QueryString = txtSPQuery.Text.Trim();
                        _Prefs.SearchProviderReplace(_SearchProviders);
                    }
                    DialogResult = true;
                }
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
                        e.CanExecute = txtSPName.Text.Trim().Length > 0 && txtSPQuery.Text.Trim().Length > 0;
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

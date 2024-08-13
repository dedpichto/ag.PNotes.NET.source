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

using Microsoft.Win32;
using System;
using System.Windows;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndExternals.xaml
    /// </summary>
    public partial class WndExternals
    {
        public WndExternals()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndExternals(WndSettings prefs)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Add;
        }

        internal WndExternals(WndSettings prefs, PNExternal ext)
            : this()
        {
            _Prefs = prefs;
            _Mode = AddEditMode.Edit;
            _Ext = ext;
        }

        private readonly WndSettings _Prefs;
        private readonly AddEditMode _Mode;
        private PNExternal _Ext;

        private void DlgExternals_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("ext_new", "New external program");
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("ext_edit", "Edit external program");
                    txtExtName.IsReadOnly = true;
                    txtExtName.Text = _Ext.Name;
                    txtExtProg.Text = _Ext.Program;
                    txtCommandLine.Text = _Ext.CommandLine;
                }
                txtExtName.Focus();
                txtExtName.SelectAll();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void progClick()
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = @"Programs|*.exe",
                    Title = PNLang.Instance.GetCaptionText("choose_new_ext", "Choose external program")
                };
                if (ofd.ShowDialog(this).Value)
                {
                    txtExtProg.Text = ofd.FileName;
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
                var name = txtExtName.Text.Trim();
                if (_Mode == AddEditMode.Add && _Prefs.ExternalExists(name))
                {
                    var message = PNLang.Instance.GetMessageText("ext_exists", "External program with this name already exists");
                    WPFMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (_Mode == AddEditMode.Add)
                    {
                        _Ext = new PNExternal { Name = name, Program = txtExtProg.Text.Trim(), CommandLine = txtCommandLine.Text.Trim() };
                        _Prefs.ExternalAdd(_Ext);
                    }
                    else
                    {
                        _Ext.Program = txtExtProg.Text.Trim();
                        _Ext.CommandLine = txtCommandLine.Text.Trim();
                        _Prefs.ExternalReplace(_Ext);
                    }
                    DialogResult = true;
                }
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
                        e.CanExecute = txtExtName.Text.Trim().Length > 0 && txtExtProg.Text.Trim().Length > 0;
                        break;
                    case CommandType.Cancel:
                    case CommandType.BrowseButton:
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
                    case CommandType.BrowseButton:
                        progClick();
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

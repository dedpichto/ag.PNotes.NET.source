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
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndPin.xaml
    /// </summary>
    public partial class WndPin
    {
        internal event EventHandler<PinnedWindowChangedEventArgs> PinnedWindowChanged;

        public WndPin(string noteName)
            : this()
        {
            _NoteName = noteName;
        }

        public WndPin()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private readonly List<PinWindow> _Windows = new List<PinWindow>();
        private readonly string _NoteName = "";

        private void DlgPin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title += @" [" + _NoteName + @"]";
                PNInterop.EnumWindowsProcDelegate enumProc = EnumWindowsProc;
                PNInterop.EnumWindows(enumProc, 0);
                grdWindows.ItemsSource = _Windows;
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdWindows_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            pinWindow();
        }

        private void grdWindows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!(grdWindows.SelectedItem is PinWindow item))
                {
                    return;
                }
                txtWildcards.Text = item.TextWnd;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            pinWindow();
        }

        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            try
            {
                if (!PNInterop.IsWindowVisible(hwnd)) return true;
                var count = PNInterop.GetWindowTextLength(hwnd);
                if (count <= 0) return true;
                var sb = new StringBuilder(count + 1);
                PNInterop.GetWindowText(hwnd, sb, count + 1);
                var sbClass = new StringBuilder(1024);
                PNInterop.GetClassName(hwnd, sbClass, sbClass.Capacity);
                _Windows.Add(new PinWindow { ClassWnd = sbClass.ToString(), TextWnd = sb.ToString() });
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void chkUseWildcards_Checked(object sender, RoutedEventArgs e)
        {
            txtWildcards.IsEnabled = true;
        }

        private void chkUseWildcards_Unchecked(object sender, RoutedEventArgs e)
        {
            txtWildcards.IsEnabled = false;
        }

        private void pinWindow()
        {
            try
            {
                if (!(grdWindows.SelectedItem is PinWindow item)) return;
                var pe = new PinnedWindowChangedEventArgs(item.ClassWnd,
                    chkUseWildcards.IsChecked != null && chkUseWildcards.IsChecked.Value
                        ? txtWildcards.Text.Trim()
                        : item.TextWnd);
                if (PinnedWindowChanged == null) return;
                PinnedWindowChanged(this, pe);
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
                        e.CanExecute = grdWindows.SelectedItem != null;
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

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        pinWindow();
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

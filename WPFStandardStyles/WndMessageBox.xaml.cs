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
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFStandardStyles
{
    internal enum MessageBoxButtonType
    {
        Ok,
        Cancel,
        Yes,
        No
    }

    /// <summary>
    /// Interaction logic for WndMessageBox.xaml
    /// </summary>
    public partial class WndMessageBox
    {
        /// <summary>
        /// CReates new instance of WndMessageBox
        /// </summary>
        public WndMessageBox()
        {
            Result = MessageBoxResult.None;
            InitializeComponent();
            FlowDirection = Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;

        }
        /// <summary>
        /// Creates new instance of WndMessageBox
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <param name="title">Message box title</param>
        /// <param name="button">Message box buttons</param>
        /// <param name="image">Message box image</param>
        /// <param name="owner">Message box owner</param>
        public WndMessageBox(string text = "", string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, Window owner = null)
            : this()
        {
            Title = title;
            txbText.Text = text;

            switch (button)
            {
                case MessageBoxButton.OK:
                    cmdYes.Visibility = Visibility.Collapsed;
                    cmdNo.Visibility = Visibility.Collapsed;
                    cmdCancel.Visibility = Visibility.Collapsed;
                    cmdOK.IsDefault = true;
                    cmdCancel.IsCancel = true;
                    break;
                case MessageBoxButton.OKCancel:
                    cmdYes.Visibility = Visibility.Collapsed;
                    cmdNo.Visibility = Visibility.Collapsed;
                    cmdOK.IsDefault = true;
                    cmdCancel.IsCancel = true;
                    break;
                case MessageBoxButton.YesNo:
                    cmdOK.Visibility = Visibility.Collapsed;
                    cmdCancel.Visibility = Visibility.Collapsed;
                    cmdYes.IsDefault = true;
                    cmdNo.IsCancel = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    cmdOK.Visibility = Visibility.Collapsed;
                    cmdYes.IsDefault = true;
                    cmdCancel.IsCancel = true;
                    break;
            }
            //if (button != MessageBoxButton.OK && button != MessageBoxButton.OKCancel)
            //    cmdOK.Visibility = Visibility.Collapsed;
            //if (button != MessageBoxButton.YesNo && button != MessageBoxButton.YesNoCancel)
            //{
            //    cmdYes.Visibility = Visibility.Collapsed;
            //    cmdNo.Visibility = Visibility.Collapsed;
            //    if (button == MessageBoxButton.OK)
            //    {
            //        cmdOK.IsDefault = true;
            //        cmdOK.IsCancel = true;
            //    }
            //    else if (button == MessageBoxButton.OKCancel)
            //    {
            //        cmdOK.IsDefault = true;
            //        cmdCancel.IsCancel = true;
            //    }
            //}
            //if (button != MessageBoxButton.OKCancel && button != MessageBoxButton.YesNoCancel)
            //    cmdCancel.Visibility = Visibility.Collapsed;
            switch (image)
            {
                case MessageBoxImage.Information:
                    imgIcon.Source = SystemIcons.Information.ToImageSource();
                    break;
                case MessageBoxImage.Question:
                    imgIcon.Source = SystemIcons.Question.ToImageSource();
                    break;
                case MessageBoxImage.Exclamation:
                    imgIcon.Source = SystemIcons.Exclamation.ToImageSource();
                    break;
                case MessageBoxImage.Error:
                    imgIcon.Source = SystemIcons.Error.ToImageSource();
                    break;
            }
            if (owner != null)
                Owner = owner;
        }

        internal MessageBoxResult Result { get; private set; }

        private bool _ClosedByButton;

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Command is MessageBoxCommand)) return;
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Command is MessageBoxCommand command)) return;
            switch (command.Type)
            {
                case MessageBoxButtonType.Ok:
                    Result = MessageBoxResult.OK;
                    break;
                case MessageBoxButtonType.Cancel:
                    Result = MessageBoxResult.Cancel;
                    break;
                case MessageBoxButtonType.Yes:
                    Result = MessageBoxResult.Yes;
                    break;
                case MessageBoxButtonType.No:
                    Result = MessageBoxResult.No;
                    break;
            }

            _ClosedByButton = true;
            Close();
        }

        private void DlgMessageBox_Closed(object sender, EventArgs e)
        {
            if (!_ClosedByButton)
                Result = cmdCancel.IsCancel ? MessageBoxResult.Cancel :
                    cmdNo.IsCancel ? MessageBoxResult.No : MessageBoxResult.None;
        }
    }

    /// <summary>
    /// Represents custom command for MessageBox buttons
    /// </summary>
    public class MessageBoxCommand : RoutedUICommand
    {
        internal MessageBoxButtonType Type { get; set; }
    }

    /// <summary>
    /// Holds custom commands for MessageBox buttons
    /// </summary>
    public class MessageBoxCommands
    {
        static MessageBoxCommands()
        {
            OkCommand = new MessageBoxCommand { Text = "Ok", Type = MessageBoxButtonType.Ok };
            CancelCommand = new MessageBoxCommand { Text = "Cancel", Type = MessageBoxButtonType.Cancel };
            YesCommand = new MessageBoxCommand { Text = "Yes", Type = MessageBoxButtonType.Yes };
            NoCommand = new MessageBoxCommand { Text = "No", Type = MessageBoxButtonType.No };
        }
        /// <summary>
        /// MessageBox Ok command
        /// </summary>
        public static MessageBoxCommand OkCommand { get; }
        /// <summary>
        /// MessageBox Cancel command
        /// </summary>
        public static MessageBoxCommand CancelCommand { get; }
        /// <summary>
        /// MessageBox Yes command
        /// </summary>
        public static MessageBoxCommand YesCommand { get; }
        /// <summary>
        /// MessageBox No command
        /// </summary>
        public static MessageBoxCommand NoCommand { get; }
    }

    internal static class Extensions
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DeleteObject(IntPtr hObject);

        internal static ImageSource ToImageSource(this Icon icon)
        {
            var bitmap = icon.ToBitmap();
            var hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }
    }
}

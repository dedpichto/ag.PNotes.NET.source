﻿// PNotes.NET - open source desktop notes manager
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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace WPFStandardStyles
{
    /// <summary>
    /// Custom window border styles
    /// </summary>
    public enum CustomWindowBorderStyle
    {
        /// <summary>
        /// Normal border
        /// </summary>
        Normal,
        /// <summary>
        /// Message box window border
        /// </summary>
        MessageBox,
        /// <summary>
        /// No border
        /// </summary>
        NoBorder
    }

    internal static class LocalExtensions
    {
        public static void ForWindowFromChild(this object childDependencyObject, Action<Window> action)
        {
            var element = childDependencyObject as DependencyObject;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (!(element is Window)) 
                    continue;
                action(element as Window); 
                break;
            }
        }

        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        public static Window WindowFromTemplate(this object templateFrameworkElement)
        {
            return ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    /// <summary>
    /// Representts custom window's style
    /// </summary>
    public partial class CustomWindowStyle
    {
        /// <summary>
        /// Attached WindowBorderBroperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty WindowBorderBroperty =
            DependencyProperty.RegisterAttached("WindowBorder", typeof(CustomWindowBorderStyle),
                typeof(CustomWindowStyle), new UIPropertyMetadata(CustomWindowBorderStyle.Normal));

        /// <summary>
        /// Gets WindowBorder property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static CustomWindowBorderStyle GetWindowBorder(DependencyObject obj)
        {
            return (CustomWindowBorderStyle)obj.GetValue(WindowBorderBroperty);
        }
        /// <summary>
        /// Sets WindowBorder property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetWindowBorder(DependencyObject obj, CustomWindowBorderStyle value)
        {
            obj.SetValue(WindowBorderBroperty, value);
        }

        #region sizing event handlers

        private void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        private void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        private void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.East : SizingAction.West); }
        private void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.West : SizingAction.East); }
        private void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.NorthWest : SizingAction.NorthEast); }
        private void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.NorthEast : SizingAction.NorthWest); }
        private void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.SouthEast : SizingAction.SouthWest); }
        private void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, ((FrameworkElement)sender).FlowDirection == FlowDirection.LeftToRight ? SizingAction.SouthWest : SizingAction.SouthEast); }

        private static void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Normal)
                        DragSize(w.GetWindowHandle(), action);
                });
            }
        }

        private void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                sender.ForWindowFromTemplate(w => w.Close());
            }
            else
            {
                sender.ForWindowFromTemplate(w =>
                    SendMessage(w.GetWindowHandle(), WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)' '));
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }

        private void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);
        }

        private void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(
                w =>
                {
                    w.WindowState = (w.WindowState == WindowState.Maximized)
                        ? WindowState.Normal
                        : WindowState.Maximized;
                });

        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            sender.ForWindowFromTemplate(
                w =>
                {
                    if (w.WindowState == WindowState.Maximized)
                    {
                        w.MaxHeight = SystemParameters.WorkArea.Height + 14;
                        w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //var thickness = (Thickness) w.TryFindResource("WindowBorderThickness");
                        //if (thickness.Left > 0)
                        //{
                        //    w.MaxHeight = SystemParameters.WorkArea.Height + 14 ;
                        //    w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //}
                        //else
                        //{
                        //    w.MaxHeight = SystemParameters.WorkArea.Height + 14;
                        //    w.MaxWidth = SystemParameters.WorkArea.Width + 14;
                        //}
                    }
                    //else
                    //{
                    //    w.Margin = new Thickness(7);
                    //}
                });
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = sender.WindowFromTemplate();
            if (window == null) return;
            if (e.ClickCount > 1 && (window.ResizeMode == ResizeMode.CanResize ||
                window.ResizeMode == ResizeMode.CanResizeWithGrip))
            {
                MaxButtonClick(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w => w.DragMove());
            }
        }

        private void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState != WindowState.Maximized) return;
                    w.BeginInit();
                    const double adjustment = 40.0;
                    var mouse1 = e.MouseDevice.GetPosition(w);
                    var width1 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                    w.WindowState = WindowState.Normal;
                    var width2 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                    w.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                    w.Top = -7;
                    w.EndInit();
                    w.DragMove();
                });
            }
        }

        #endregion

        #region P/Invoke

        private const int WM_SYSCOMMAND = 0x112;
        private const int WM_LBUTTONUP = 0x0202;
        private const int SC_SIZE = 0xF000;
        private const int SC_KEYMENU = 0xF100;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Window sizing directions
        /// </summary>
        public enum SizingAction
        {
            /// <summary>
            /// North direction
            /// </summary>
            North = 3,
            /// <summary>
            /// South direction
            /// </summary>
            South = 6,
            /// <summary>
            /// East direction
            /// </summary>
            East = 2,
            /// <summary>
            /// West
            /// </summary>
            West = 1,
            /// <summary>
            /// NorthEast direction
            /// </summary>
            NorthEast = 5,
            /// <summary>
            /// NorthWest direction
            /// </summary>
            NorthWest = 4,
            /// <summary>
            /// SouthEast direction
            /// </summary>
            SouthEast = 8,
            /// <summary>
            /// SouthWest direction
            /// </summary>
            SouthWest = 7
        }

        #endregion
    }
}

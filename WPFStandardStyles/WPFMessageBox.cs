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

using System.Windows;

namespace WPFStandardStyles
{
    /// <summary>
    /// Represents class for custom MessageBoxe
    /// </summary>
    public static class WPFMessageBox
    {
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string text)
        {
            var wmb = new WndMessageBox(text);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string text, string caption)
        {
            var wmb = new WndMessageBox(text, caption);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="owner">Message box owner</param>
        /// <param name="text">Message box text</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string text)
        {
            var wmb = new WndMessageBox(text, "", MessageBoxButton.OK, MessageBoxImage.None, owner);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <param name="button">Message box buttons</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton button)
        {
            var wmb = new WndMessageBox(text, caption, button);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="owner">Message box owner</param>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string text, string caption)
        {
            var wmb = new WndMessageBox(text, caption, MessageBoxButton.OK, MessageBoxImage.None, owner);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <param name="button">Message box buttons</param>
        /// <param name="image">Message box image</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            var wmb = new WndMessageBox(text, caption, button, image);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="owner">Message box owner</param>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <param name="button">Message box buttons</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton button)
        {
            var wmb = new WndMessageBox(text, caption, button, MessageBoxImage.None, owner);
            wmb.ShowDialog();
            return wmb.Result;
        }
        /// <summary>
        /// Shows message box
        /// </summary>
        /// <param name="owner">Message box owner</param>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box caption</param>
        /// <param name="button">Message box buttons</param>
        /// <param name="image">Message box image</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            var wmb = new WndMessageBox(text, caption, button, image, owner);
            wmb.ShowDialog();
            return wmb.Result;
        }
    }
}

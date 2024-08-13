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

using System.Security.Permissions;
using System.Windows.Threading;
using PNStaticFonts;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WPFStandardStyles;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Control = System.Windows.Controls.Control;
using Cursor = System.Windows.Input.Cursor;
using FlowDirection = System.Windows.FlowDirection;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using FontWeight = System.Windows.FontWeight;
using Image = System.Drawing.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using RichTextBox = System.Windows.Forms.RichTextBox;
using Size = System.Windows.Size;

namespace PNotes.NET
{
    internal static class PNStatic
    {
        internal static readonly Dictionary<int, int> StopValues = new Dictionary<int, int> { { 0, -1 }, { 1, 5 }, { 2, 10 }, { 3, 15 }, { 4, 30 }, { 5, 45 }, { 6, 60 }, { 7, 120 }, { 8, 180 }, { 9, 300 }, { 10, 900 }, { 11, 1800 } };

        internal static T FindParent<T>(object child) where T : DependencyObject
        {
            if (!(child is DependencyObject)) return null;

            var parent = VisualTreeHelper.GetParent((DependencyObject)child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        internal static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                if (!(child is T))
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else
                {
                    if (!(child is FrameworkElement frameworkElement)) continue;
                    if (frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                //else if (!string.IsNullOrEmpty(childName))
                //{
                //    var frameworkElement = child as FrameworkElement;
                //    // If the child's name is set for search
                //    if (frameworkElement == null) continue;
                //    if (frameworkElement.Name != childName) continue;
                //    // if the child's name is of the request name
                //    foundChild = (T)child;
                //    break;
                //}
                //else
                //{
                //    // child element found.
                //    foundChild = (T)child;
                //    break;
                //}
            }

            return foundChild;
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses FortammedText
        /// </summary>
        public static Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, Brush foreground)
        {
            try
            {
                var ft = new FormattedText(text,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                        fontSize,
                        foreground);
                return new Size(ft.Width, ft.Height);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return new Size();
            }
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses Glyph's
        /// </summary>
        //public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize, Brush foreground)
        //{
        //    try
        //    {
        //        var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

        //        if (!typeface.TryGetGlyphTypeface(out var glyphTypeface))
        //        {
        //            return MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize, foreground);
        //        }

        //        double totalWidth = 0;
        //        double height = 0;

        //        foreach (var t in text)
        //        {
        //            ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[t];

        //            double width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;

        //            double glyphHeight = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;

        //            if (glyphHeight > height)
        //            {
        //                height = glyphHeight;
        //            }

        //            totalWidth += width;
        //        }

        //        return new Size(totalWidth, height);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(ex);
        //        return new Size();
        //    }
        //}

        internal static void SendKey(Key key)
        {
            if (Keyboard.PrimaryDevice == null) return;
            if (Keyboard.PrimaryDevice.ActiveSource == null) return;
            var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            };
            InputManager.Current.ProcessInput(e);

            // Note: Based on your requirements you may also need to fire events for:
            // RoutedEvent = Keyboard.PreviewKeyDownEvent
            // RoutedEvent = Keyboard.KeyUpEvent
            // RoutedEvent = Keyboard.PreviewKeyUpEvent
        }

        internal static void ApplyTheme(string theme)
        {
            try
            {
                var entry = PNCollections.Instance.Themes[theme];
                if (entry == null) return;
                var rd1 =
                    Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                        r => r.Source.ToString().ToUpper().EndsWith("RESOURCES.XAML"));
                var rd2 =
                    Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                        r => r.Source.ToString().ToUpper().EndsWith("IMAGES.XAML"));
                if (rd1 == null || rd2 == null) return;
                if (rd1.Source.Equals(entry.Item1) && rd2.Source.Equals(entry.Item2)) return;
                rd1.BeginInit();
                rd1.Source = entry.Item1;
                rd1.EndInit();
                rd2.BeginInit();
                rd2.Source = entry.Item2;
                rd2.EndInit();
                //Application.Current.Resources.BeginInit();
                //Application.Current.Resources.MergedDictionaries.Remove(rd1);
                //Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = entry.Item1 });
                //Application.Current.Resources.MergedDictionaries.Remove(rd2);
                //Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = entry.Item2 });
                //Application.Current.Resources.EndInit();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void ToggleAeroPeek(IntPtr handle, bool exclude)
        {
            try
            {
                var status = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(status, exclude ? 1 : 0);
                PNInterop.DwmSetWindowAttribute(handle,
                                          PNInterop.DwmWindowAttribute.DwmwaExcludedFromPeek,
                                          status,
                                          sizeof(int));
                Marshal.FreeHGlobal(status);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        internal static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        internal static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        internal static string GetNoteText(PNote note)
        {
            try
            {
                var text = "";
                if (note.Visible)
                {
                    text = note.Dialog.Edit.Text;
                }
                else
                {
                    var path = Path.Combine(PNPaths.Instance.DataDir, note.Id + PNStrings.NOTE_EXTENSION);
                    if (File.Exists(path))
                    {
                        var rtb = new RichTextBox();
                        PNNotesOperations.LoadNoteFile(rtb, path);
                        text = rtb.Text;
                    }
                }
                return text;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "";
            }
        }

        internal static void SpeakTextAsync(SpeechSynthesizer sp, string text, string voiceName)
        {
            try
            {
                sp.SelectVoice(voiceName);
                sp.Volume = PNRuntimes.Instance.Settings.Schedule.VoiceVolume;
                sp.Rate = PNRuntimes.Instance.Settings.Schedule.VoiceSpeed;
                var prompt = new Prompt(text);
                sp.SpeakAsync(prompt);
            }
            catch (ArgumentException aex)
            {
                LogException(aex);
                LogThis("Voice name: " + voiceName);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void SpeakText(string text, string voiceName)
        {
            try
            {
                using (var sp = new SpeechSynthesizer())
                {
                    sp.SelectVoice(voiceName);
                    sp.Volume = PNRuntimes.Instance.Settings.Schedule.VoiceVolume;
                    sp.Rate = PNRuntimes.Instance.Settings.Schedule.VoiceSpeed;
                    var prompt = new Prompt(text);
                    sp.Speak(prompt);
                }
            }
            catch (ArgumentException aex)
            {
                LogException(aex);
                LogThis("Voice name: " + voiceName);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void LoadPage(string url)
        {
            try
            {
                var commandLine = "\"";
                commandLine += url;
                commandLine += "\"";
                Process.Start(commandLine);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void DeactivateNotesWindows(Guid handle = default(Guid))
        {
            try
            {
                var windows = handle == default(Guid)
                    ? Application.Current.Windows.OfType<WndNote>().ToArray()
                    : Application.Current.Windows.OfType<WndNote>().Where(w => w.Handle != handle).ToArray();
                foreach (var w in windows)
                    w.DeactivateWindow();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static int FindEditString(RichTextBox edit, int start)
        {
            try
            {
                return (PNRuntimes.Instance.FindOptions & RichTextBoxFinds.Reverse) != RichTextBoxFinds.Reverse
                    ? edit.Find(PNRuntimes.Instance.FindString, start, -1, PNRuntimes.Instance.FindOptions)
                    : edit.Find(PNRuntimes.Instance.FindString, 0, start, PNRuntimes.Instance.FindOptions);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return -1;
            }
        }

        internal static int FindEditStringByRegExp(RichTextBox edit, int start, bool reverse)
        {
            try
            {
                var options = new RegexOptions();
                if ((PNRuntimes.Instance.FindOptions & RichTextBoxFinds.MatchCase) != RichTextBoxFinds.MatchCase)
                    options |= RegexOptions.IgnoreCase;
                if (reverse)
                    options |= RegexOptions.RightToLeft;
                var re = new Regex(PNRuntimes.Instance.FindString, options);
                var match = re.Match(edit.Text, start);
                if (!match.Success) return -1;
                edit.SelectionStart = match.Index;
                edit.SelectionLength = match.Length;
                return match.Index + match.Length;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return -1;
            }
        }
        
        internal static Image ImageToDrawingImage(BitmapImage bmp)
        {
            try
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Position = 0;
                    return Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        //internal static Bitmap BitmapPart(Image image, int x, int y, int width, int heigh)
        //{
        //    try
        //    {
        //        var bmp = new Bitmap(width, heigh);
        //        using (var g = Graphics.FromImage(bmp))
        //        {
        //            g.DrawImage(image, new Rectangle(0, 0, width, heigh), x, y, width, heigh,
        //                               GraphicsUnit.Pixel);
        //        }
        //        return bmp;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(ex);
        //        return null;
        //    }
        //}

        internal static Bitmap MakeTransparentBitmap(Image image, System.Drawing.Color mask, int x, int y, int width, int heigh)
        {
            try
            {
                var bmp = new Bitmap(width, heigh);
                using (var g = Graphics.FromImage(bmp))
                {
                    var imgAttribute = new ImageAttributes();
                    imgAttribute.SetColorKey(mask, mask);
                    g.DrawImage(image, new Rectangle(0, 0, width, heigh), x, y, width, heigh,
                                       GraphicsUnit.Pixel, imgAttribute);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        internal static Bitmap MakeTransparentBitmap(Image image, System.Drawing.Color mask)
        {
            try
            {
                var bmp = new Bitmap(image.Width, image.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    var imgAttribute = new ImageAttributes();
                    imgAttribute.SetColorKey(mask, mask);
                    g.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height,
                                       GraphicsUnit.Pixel, imgAttribute);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        internal static BitmapSource ImageFromDrawingImage(Image image, System.Drawing.Color maskColor, int x, int y, int width, int heigh)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bmp = MakeTransparentBitmap(image, maskColor, x, y, width, heigh))
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        return ConvertBitmapTo96Dpi(bitmapImage);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        internal static BitmapSource ImageFromDrawingImage(Image image, System.Drawing.Color maskColor)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var bmp = MakeTransparentBitmap(image, maskColor))
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        return ConvertBitmapTo96Dpi(bitmapImage);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        //internal static BitmapSource ImageFromDrawingImage(Image image, int x, int y, int width, int heigh)
        //{
        //    try
        //    {
        //        using (var ms = new MemoryStream())
        //        {
        //            using (var bmp = BitmapPart(image, x, y, width, heigh))
        //            {
        //                bmp.Save(ms, ImageFormat.Png);
        //                ms.Position = 0;
        //                var bitmapImage = new BitmapImage();
        //                bitmapImage.BeginInit();
        //                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        //                bitmapImage.StreamSource = ms;
        //                bitmapImage.EndInit();
        //                return ConvertBitmapTo96Dpi(bitmapImage);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(ex);
        //        return null;
        //    }
        //}

        internal static BitmapSource ImageFromDrawingImage(Image image)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    return ConvertBitmapTo96Dpi(bitmapImage);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        internal static BitmapSource ConvertBitmapTo96Dpi(BitmapImage bitmapImage)
        {
            const double dpi = 96;
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;

            var stride = width * bitmapImage.Format.BitsPerPixel;
            var pixelData = new byte[stride * height];
            bitmapImage.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, dpi, dpi, bitmapImage.Format, null, pixelData, stride);
        }

        internal static void RunExternalProgram(string progPath, string commandLine)
        {
            try
            {
                if (progPath.StartsWith("%"))
                {
                    progPath = progPath.ToUpper();
                    var leftPart = "";
                    var rightPart = "";
                    if (progPath.StartsWith("%ALLUSERSPROFILE%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        rightPart = progPath.Substring("%ALLUSERSPROFILE%".Length);
                    }
                    else if (progPath.StartsWith("%PROGRAMDATA%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        rightPart = progPath.Substring("%PROGRAMDATA%".Length);
                    }
                    else if (progPath.StartsWith("%APPDATA%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        rightPart = progPath.Substring("%APPDATA%".Length);
                    }
                    else if (progPath.StartsWith("%COMMONPROGRAMFILES%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                        rightPart = progPath.Substring("%COMMONPROGRAMFILES%".Length);
                    }
                    else if (progPath.StartsWith("%COMMONPROGRAMFILES(x86)%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
                        rightPart = progPath.Substring("%COMMONPROGRAMFILES(x86)%".Length);
                    }
                    else if (progPath.StartsWith("%LOCALAPPDATA%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        rightPart = progPath.Substring("%LOCALAPPDATA%".Length);
                    }
                    else if (progPath.StartsWith("%PROGRAMDATA%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        rightPart = progPath.Substring("%PROGRAMDATA%".Length);
                    }
                    else if (progPath.StartsWith("%PROGRAMFILES%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        rightPart = progPath.Substring("%PROGRAMFILES%".Length);
                    }
                    else if (progPath.StartsWith("%PROGRAMFILES(X86)%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        rightPart = progPath.Substring("%PROGRAMFILES(X86)%".Length);
                    }
                    else if (progPath.StartsWith("%SYSTEMROOT%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        rightPart = progPath.Substring("%SYSTEMROOT%".Length);
                    }
                    else if (progPath.StartsWith("%WINDIR%"))
                    {
                        leftPart = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        rightPart = progPath.Substring("%WINDIR%".Length);
                    }
                    if (rightPart.StartsWith(new string(Path.DirectorySeparatorChar, 1)))
                        rightPart = rightPart.Substring(new string(Path.DirectorySeparatorChar, 1).Length);
                    progPath = Path.Combine(leftPart, rightPart);
                }
                Process.Start(progPath, commandLine);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static Cursor CreateCursorFromResource(string resourceName)
        {
            try
            {
                var stream = Application.GetResourceStream(new Uri(resourceName, UriKind.Relative));
                return stream?.Stream != null ? new Cursor(stream.Stream) : null;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        internal static string CreatePinRegexPattern(string str)
        {
            try
            {
                var pattern = "^" + Regex.Escape(str).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                return pattern;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return str;
            }
        }

        internal static void HideMenus(ContextMenu ctm, PNHiddenMenu[] hiddens)
        {
            try
            {
                Separator sep = null;
                var visibleCount = 0;
                foreach (Control item in ctm.Items)
                {
                    if (item is MenuItem ti)
                    {
                        if (hiddens.All(hm => hm.Name != item.Name))
                        {
                            ti.Visibility = Visibility.Visible;
                            visibleCount++;
                        }
                        else
                        {
                            ti.Visibility = Visibility.Collapsed;
                        }

                        if (ti.HasItems)
                        {
                            hideSubMenus(ti, hiddens);
                        }
                    }
                    else if (item is Separator separator)
                    {
                        sep = separator;
                        separator.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
                        visibleCount = 0;
                    }
                }
                if (sep != null) sep.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void hideSubMenus(ItemsControl parent, PNHiddenMenu[] hiddens)
        {
            try
            {
                Separator sep = null;
                var visibleCount = 0;
                foreach (var item in parent.Items)
                {
                    if (item is MenuItem ti)
                    {
                        if (hiddens.All(hm => hm.Name != ti.Name))
                        {
                            ti.Visibility = Visibility.Visible;
                            visibleCount++;
                        }
                        else
                        {
                            ti.Visibility = Visibility.Collapsed;
                        }

                        if (ti.HasItems)
                        {
                            hideSubMenus(ti, hiddens);
                        }
                    }
                    else if (item is Separator separator)
                    {
                        sep = separator;
                        sep.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
                        visibleCount = 0;
                    }
                }
                if (sep != null) sep.Visibility = visibleCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static MenuItem GetMenuByName(ItemsControl parent, string menuName)
        {
            MenuItem menuItem = null;
            foreach (var mi in parent.Items.OfType<MenuItem>())
            {
                if (mi.Name == menuName)
                {
                    menuItem = mi;
                    break;
                }

                menuItem = GetMenuByName(mi, menuName);
                if (menuItem != null)
                    break;
            }
            return menuItem;
        }

        internal static bool IsBitSet(int bits, int index)
        {
            try
            {
                return (bits & (1 << index)) != 0;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        private static byte[] getRgb(int rgb)
        {
            var clr = new byte[] { 0, 0, 0 };
            for (var i = 0; i < 3; i++)
            {
                clr[i] = (byte)(rgb & 255);
                rgb >>= 8;
            }
            return clr;
        }

        internal static string FromIntToColorString(int arg)
        {
            try
            {
                var arr = getRgb(arg);
                var clrW = Color.FromArgb(255, arr[0], arr[1], arr[2]);
                var colorConverter = new ColorConverter();
                var temp = colorConverter.ConvertToString(null, PNRuntimes.Instance.CultureInvariant, clrW);
                return temp ?? "##FF000000";
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "##FF000000";
            }
        }

        internal static string FromIntToDrawinfColorString(int arg)
        {
            try
            {
                var arr = getRgb(arg);
                var clr = new byte[] { 255, 0, 0, 0 };
                for (var i = 0; i < 2; i++)
                    clr[i + 1] = arr[i];
                return clr.ToCommaSeparatedString();
            }
            catch (Exception ex)
            {
                LogException(ex);
                return "255,0,0,0";
            }
        }

        //internal static LOGFONT ToLogFont(PNFont f)
        //{
        //    var lf = new LOGFONT();
        //    lf.Init();
        //    try
        //    {
        //        if (f.FontStyle == FontStyles.Italic)
        //            lf.lfItalic = 1;
        //        lf.lfHeight = PNInterop.ConvertToLogfontHeight((int)f.FontSize);
        //        if (f.FontWeight == FontWeights.Black)
        //            lf.lfWeight = 900;
        //        else if (f.FontWeight == FontWeights.ExtraBold)
        //            lf.lfWeight = 800;
        //        else if (f.FontWeight == FontWeights.Bold)
        //            lf.lfWeight = 700;
        //        else if (f.FontWeight == FontWeights.SemiBold)
        //            lf.lfWeight = 600;
        //        else if (f.FontWeight == FontWeights.Medium)
        //            lf.lfWeight = 500;
        //        else if (f.FontWeight == FontWeights.Normal)
        //            lf.lfWeight = 400;
        //        else if (f.FontWeight == FontWeights.Light)
        //            lf.lfWeight = 300;
        //        else if (f.FontWeight == FontWeights.UltraLight)
        //            lf.lfWeight = 200;
        //        else if (f.FontWeight == FontWeights.Thin)
        //            lf.lfWeight = 100;
        //        return lf;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(ex);
        //        return lf;
        //    }
        //}

        internal static PNFont FromLogFont(LOGFONT lf)
        {
            var fonts = new InstalledFontCollection();
            var f = fonts.Families.Any(ff => ff.Name == lf.lfFaceName)
                ? new PNFont { FontFamily = new FontFamily(lf.lfFaceName) }
                : new PNFont();
            try
            {
                if (lf.lfItalic != 0)
                    f.FontStyle = FontStyles.Italic;
                f.FontSize = lf.GetFontSize();// PNInterop.ConvertFromLogfontHeight(lf.lfHeight);
                f.FontSize *= 96.0 / 72.0;

                switch (lf.lfWeight)
                {
                    case 0:
                    case 400:
                        f.FontWeight = FontWeights.Normal;
                        break;
                    case 100:
                        f.FontWeight = FontWeights.Thin;
                        break;
                    case 200:
                        f.FontWeight = FontWeights.UltraLight;
                        break;
                    case 300:
                        f.FontWeight = FontWeights.Light;
                        break;
                    case 500:
                        f.FontWeight = FontWeights.Medium;
                        break;
                    case 600:
                        f.FontWeight = FontWeights.SemiBold;
                        break;
                    case 700:
                        f.FontWeight = FontWeights.Bold;
                        break;
                    case 800:
                        f.FontWeight = FontWeights.ExtraBold;
                        break;
                    case 900:
                        f.FontWeight = FontWeights.Black;
                        break;
                }

                return f;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return f;
            }
        }

        internal static bool CheckSkinsExistance()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return false;
                if (new DirectoryInfo(PNPaths.Instance.SkinsDir).GetFiles(@"*" + PNStrings.SKIN_EXTENSION).Length == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        internal static Size AllScreensSize()
        {
            try
            {
                var size = new Size();
                var screens = Screen.AllScreens.ToList();
                var factors = GetScalingFactors();
                var xMin = screens.Min(sc => sc.WorkingArea.X / factors.Item1);
                var yMin = screens.Min(sc => sc.WorkingArea.Y / factors.Item2);
                var scr = screens.FirstOrDefault(sc => Math.Abs(sc.WorkingArea.X - xMin) < double.Epsilon);
                if (scr != null)
                {
                    size.Width += scr.WorkingArea.Width / factors.Item1;
                    size.Height += scr.WorkingArea.Height / factors.Item2;
                }
                size.Width += screens.Where(sc => sc.WorkingArea.X / factors.Item1 > xMin).Sum(sc => sc.WorkingArea.Width / factors.Item1);
                size.Height += screens.Where(sc => sc.WorkingArea.Y / factors.Item2 > yMin).Sum(sc => sc.WorkingArea.Height / factors.Item2);
                return size;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return default(Size);
            }
        }

        internal static IPAddress GetLocalIPv4(NetworkInterfaceType type)
        {
            IPAddress address = null;
            foreach (var ip in from item in NetworkInterface.GetAllNetworkInterfaces()
                               where item.NetworkInterfaceType == type && item.OperationalStatus == OperationalStatus.Up
                               from ip in
                                   item.GetIPProperties()
                                       .UnicastAddresses.Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                               select ip)
            {
                address = ip.Address;
            }
            return address;
        }

        internal static void ShowSplash()
        {
            try
            {
                var d = new WndSplash();
                d.ShowDialog();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                LogException(ex, false);
            }
        }

        internal static PNFont FromDrawingFont(string fontString)
        {
            try
            {
                var wpfn = new PNFont();
                var fc = new FontConverter();
                var font = (Font)fc.ConvertFromString(null, PNRuntimes.Instance.CultureInvariant, fontString);
                if (font == null) return wpfn;
                wpfn.FontFamily = new FontFamily(font.FontFamily.Name);
                wpfn.FontSize = font.SizeInPoints * (96.0 / 72.0);
                if (font.Italic)
                    wpfn.FontStyle = FontStyles.Italic;
                if (font.Bold)
                    wpfn.FontWeight = FontWeights.Bold;
                return wpfn;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return new PNFont();
            }
        }

        internal static bool PrepareNewVersionCommandLine()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("0 ");
                sb.Append("\"");
                sb.Append(PNLang.Instance.GetCaptionText("update_progress", "Update in progress..."));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("downloading", "Downloading:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("extracting", "Extracting:"));
                sb.Append(",");
                sb.Append(PNLang.Instance.GetCaptionText("copying", "Coping:"));
                sb.Append("\" \"");
                sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.ExecutablePath);
                sb.Append("\" \"");
                sb.Append("\" \"");
                sb.Append(System.Windows.Forms.Application.StartupPath);
                sb.Append("\"");

                PNSingleton.Instance.UpdaterCommandLine = sb.ToString();
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        internal static void NormalizeStartDate(ref DateTime start)
        {
            try
            {
                var dbf = PNRuntimes.Instance.Settings.GeneralSettings.DateBits;
                if (!dbf[DatePart.Millisecond])
                    start = start.AddMilliseconds(-start.Millisecond);
                if (!dbf[DatePart.Second])
                    start = start.AddSeconds(-start.Second);
                if (!dbf[DatePart.Minute])
                    start = start.AddMinutes(-start.Minute);
                if (!dbf[DatePart.Hour])
                    start = start.AddHours(-start.Hour);
                if (!dbf[DatePart.Day])
                    start = start.AddDays(-start.Day);
                if (!dbf[DatePart.Month])
                    start = start.AddMonths(-start.Month);
                if (!dbf[DatePart.Year])
                    start = start.AddYears(-start.Year);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static int WeekdayOrdinal(DateTime now, DayOfWeek dw, ref bool isLast)
        {
            try
            {
                var ordinal = 0;
                var temp = now;
                temp = temp.AddDays(7);
                if (temp.Month != now.Month)
                {
                    isLast = true;
                }
                temp = now;
                while (temp.Month == now.Month)
                {
                    ordinal++;
                    temp = temp.AddDays(-7);
                }
                return ordinal;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return 0;
            }
        }

        internal static string StripDateTimeFormat(string fmt, char stripChar)
        {
            try
            {
                var chars = stripChar + "!\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~ \t\n\r\x0b\x0c";
                var list = fmt.ToCharArray().ToList();
                var index = list.Count - 1;
                while (index >= 0 && list[index] != 's')
                    index--;
                if (index == -1)
                    return fmt;
                while (chars.Contains(list[index]))
                    list.RemoveAt(index--);

                return new string(list.ToArray());
            }
            catch (Exception ex)
            {
                LogException(ex);
                return fmt;
            }
        }

        internal static bool CreateFullBackup(string fileBackup)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "tempbackuppnotes");
            try
            {
                if (File.Exists(fileBackup))
                {
                    File.Delete(fileBackup);
                }
                using (Package package = Package.Open(fileBackup, FileMode.OpenOrCreate))
                {
                    var di = new DirectoryInfo(PNPaths.Instance.DataDir);
                    var files = di.GetFiles("*" + PNStrings.NOTE_EXTENSION);
                    PackagePart packagePartFile;
                    foreach (var f in files)
                    {
                        var partUriFile = PackUriHelper.CreatePartUri(new Uri(f.Name, UriKind.Relative));
                        packagePartFile = package.CreatePart(partUriFile, MediaTypeNames.Text.RichText,
                                                             CompressionOption.Normal);
                        package.CreateRelationship(partUriFile, TargetMode.Internal, f.Name);
                        File.Copy(f.FullName, tempFile, true);
                        using (
                            var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read,
                                                                   FileShare.Read))
                        {
                            if (packagePartFile != null) CopyStream(fileStream, packagePartFile.GetStream());
                        }
                    }
                    var fi = new FileInfo(PNPaths.Instance.DBPath);
                    var partUri = PackUriHelper.CreatePartUri(new Uri(fi.Name, UriKind.Relative));
                    packagePartFile = package.CreatePart(partUri, MediaTypeNames.Application.Octet,
                                                         CompressionOption.Normal);
                    package.CreateRelationship(partUri, TargetMode.Internal, fi.Name);
                    File.Copy(fi.FullName, tempFile, true);
                    using (
                        var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read)
                        )
                    {
                        if (packagePartFile != null) CopyStream(fileStream, packagePartFile.GetStream());
                    }
                    // save additional data
                    var xdoc = new XDocument();
                    var xdata = new XElement("data");
                    var xhash = new XElement("hash");
                    if (PNRuntimes.Instance.Settings.Protection.StoreAsEncrypted)
                    {
                        xhash.Value = PNRuntimes.Instance.Settings.Protection.PasswordString;
                    }
                    xdata.Add(xhash);
                    xdoc.Add(xdata);
                    // save temp file
                    var tempXml = Path.Combine(Path.GetTempPath(), "data.xml");
                    xdoc.Save(tempXml);

                    var partUriXml = PackUriHelper.CreatePartUri(new Uri("data.xml", UriKind.Relative));
                    var packagePartXml = package.CreatePart(partUriXml, MediaTypeNames.Text.Xml,
                                                                    CompressionOption.NotCompressed);
                    using (var fileStream = new FileStream(tempXml, FileMode.Open, FileAccess.Read))
                    {
                        if (packagePartXml != null) CopyStream(fileStream, packagePartXml.GetStream());
                    }

                    File.Delete(tempXml);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        internal static void CopyStream(Stream source, Stream target)
        {
            try
            {
                const int bufSize = 0x1000;
                var buffer = new byte[bufSize];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, bufSize)) > 0)
                {
                    target.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static void StartProgram(bool nonetwork)
        {
            try
            {
                using (var sp = new SpeechSynthesizer())
                {
                    foreach (var iv in sp.GetInstalledVoices().Where(v => v.Enabled))
                    {
                        PNCollections.Instance.Voices.Add(iv.VoiceInfo.Name);
                    }
                }
                fillDaysOfWeek();
                createScheduleDescription();
                PNData.LoadDBSettings();
                if (nonetwork)
                {
                    PNRuntimes.Instance.Settings.Network.EnableExchange = false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void fillDaysOfWeek()
        {
            try
            {
                string[] names = Enum.GetNames(typeof(DayOfWeek));
                DayOfWeek[] values = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().ToArray();
                for (int i = 0; i < names.Length; i++)
                {
                    PNCollections.Instance.DaysOfWeekPairs.Add(names[i], values[i]);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private static void createScheduleDescription()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("Once at:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.Once, sb.ToString());
                sb = new StringBuilder();
                sb.Append("Every day at:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.EveryDay, sb.ToString());
                sb = new StringBuilder();
                sb.Append("Repeat every:");
                sb.Append(" ");
                sb.Append(PNStrings.YEARS);
                sb.Append(PNStrings.MONTHS);
                sb.Append(PNStrings.WEEKS);
                sb.Append(PNStrings.DAYS);
                sb.Append(PNStrings.HOURS);
                sb.Append(PNStrings.MINUTES);
                sb.Append(PNStrings.SECONDS);
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.RepeatEvery, sb.ToString());
                sb = new StringBuilder();
                sb.Append("Weekly on:");
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.Weekly, sb.ToString());
                sb = new StringBuilder();
                sb.Append("After:");
                sb.Append(" ");
                sb.Append(PNStrings.YEARS);
                sb.Append(PNStrings.MONTHS);
                sb.Append(PNStrings.WEEKS);
                sb.Append(PNStrings.DAYS);
                sb.Append(PNStrings.HOURS);
                sb.Append(PNStrings.MINUTES);
                sb.Append(PNStrings.SECONDS);
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.After, sb.ToString());
                sb = new StringBuilder();
                sb.Append("Monthly (exact date)");
                sb.Append(" ");
                sb.Append("Date:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                sb.Append(" ");
                sb.Append("Time:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER2);
                sb.Append(" ");
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.MonthlyExact, sb.ToString());
                sb = new StringBuilder();
                sb.Append("Monthly (day of week)");
                sb.Append(" ");
                sb.Append("Day of week:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER1);
                sb.Append(" ");
                sb.Append("Ordinal number:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER2);
                sb.Append(" ");
                sb.Append("Time:");
                sb.Append(" ");
                sb.Append(PNStrings.PLACEHOLDER3);
                sb.Append(" ");
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.MonthlyDayOfWeek, sb.ToString());
                sb = new StringBuilder("Multiple alerts");
                PNCollections.Instance.ScheduleDescriptions.Add(ScheduleType.MultipleAlerts, sb.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        internal static bool SetContactIpAddress(PNContact cont)
        {
            try
            {
                IPHostEntry ipHostInfo;
                try
                {
                    ipHostInfo = Dns.GetHostEntry(cont.ComputerName);
                }
                catch (SocketException)
                {
                    return false;
                }
                
                if (ipHostInfo.AddressList.Any(ip => ip.Equals(PNSingleton.Instance.IpAddress)))
                {
                    cont.IpAddress = PNSingleton.Instance.IpAddress.ToString();
                }
                else
                {
                    var ipAddress =
                        ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (ipAddress != null)
                    {
                        cont.IpAddress = ipAddress.ToString();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        internal static Tuple<double, double> GetScalingFactors()
        {
            //this is for WinForms
            //var dc = IntPtr.Zero;
            //var g = Graphics.FromHwnd(IntPtr.Zero);
            //try
            //{
            //    dc = g.GetHdc();
            //    var logicalScreenHeight = GetDeviceCaps(dc, (int)DeviceCap.Vertres);
            //    var physicalScreenHeight = GetDeviceCaps(dc, (int)DeviceCap.Desktopvertres);

            //    return (float)physicalScreenHeight / logicalScreenHeight; // 1.25 = 125%
            //}
            //finally
            //{
            //    if (!dc.Equals(IntPtr.Zero))
            //        g.ReleaseHdc(dc);
            //}
            var tpl = Tuple.Create(1.0, 1.0);
            try
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null) return tpl;
                var presentationSource = PresentationSource.FromVisual(mainWindow);
                if (presentationSource == null) return tpl;
                if (presentationSource.CompositionTarget == null) return tpl;
                var m = presentationSource.CompositionTarget.TransformToDevice;
                return Tuple.Create(m.M11, m.M22);// notice it's divided by 96 already
            }
            catch (Exception ex)
            {
                LogException(ex);
                return tpl;
            }
        }

        internal static Rect AllScreensBounds()
        {
            try
            {
                var rect = new Rect();
                var screens = Screen.AllScreens.ToList();
                var factors = GetScalingFactors();
                var xMin = screens.Min(sc => sc.WorkingArea.X) / factors.Item1;
                var yMin = screens.Min(sc => sc.WorkingArea.Y) / factors.Item2;
                rect.X = xMin;
                rect.Y = yMin;
                var scr = screens.FirstOrDefault(sc => Math.Abs(sc.WorkingArea.X - xMin) < double.Epsilon);
                if (scr != null)
                {
                    rect.Width += scr.WorkingArea.Width / factors.Item1;
                    rect.Height += scr.WorkingArea.Height / factors.Item2;
                }
                rect.Width += screens.Where(sc => sc.WorkingArea.X / factors.Item1 > xMin).Sum(sc => sc.WorkingArea.Width / factors.Item1);
                rect.Height += screens.Where(sc => sc.WorkingArea.Y / factors.Item2 > yMin).Sum(sc => sc.WorkingArea.Height / factors.Item2);
                return rect;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return default(Rect);
            }
        }

        internal static bool areDatesValidInCommandLine(string[] dates)
        {
            if (dates.Length < 2) return true;
            if ((!String.IsNullOrWhiteSpace(dates[0]) && (!dates[0].IsDate()) ||
                 !String.IsNullOrWhiteSpace(dates[1]) && !dates[1].IsDate()))
            {
                LogThis("Invalid dates in command line " + dates[0] + " " + dates[1]);
                WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_dates_com_line",
                    "Invalid dates in command line"));
                return false;
            }
            if (!String.IsNullOrWhiteSpace(dates[0]) && !String.IsNullOrWhiteSpace(dates[1]))
            {
                if (Convert.ToDateTime(dates[0]) > Convert.ToDateTime(dates[1]))
                {
                    LogThis("Starting date shhould not be more than ending date");
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_start_date",
                        "Starting date shhould not be more than ending date"));
                    return false;
                }
            }
            if (dates.Length < 4) return true;
            if ((!String.IsNullOrWhiteSpace(dates[2]) && (!dates[2].IsDate()) ||
                 !String.IsNullOrWhiteSpace(dates[3]) && !dates[3].IsDate()))
            {
                LogThis("Invalid dates in command line" + dates[2] + " " + dates[3]);
                WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_dates_com_line",
                    "Invalid dates in command line"));
                return false;
            }
            if (!String.IsNullOrWhiteSpace(dates[2]) && !String.IsNullOrWhiteSpace(dates[3]))
            {
                if (Convert.ToDateTime(dates[2]) > Convert.ToDateTime(dates[3]))
                {
                    LogThis("Starting date shhould not be more than ending date");
                    WPFMessageBox.Show(PNLang.Instance.GetMessageText("invalid_start_date",
                        "Starting date shhould not be more than ending date"));
                    return false;
                }
            }
            return true;
        }

        internal static int GetFileMajorVersion(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return 0;
                var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return versionInfo.FileMajorPart;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return 0;
            }
        }

        internal static void LogThis(string message)
        {
            try
            {
                using (var w = new StreamWriter(Path.Combine(System.Windows.Forms.Application.StartupPath, "pnotes.log"), true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", PNRuntimes.Instance.CultureInvariant));
                    sb.AppendLine();
                    sb.Append(message);
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                }
            }
            catch
            {
                // ignored
            }
        }

        internal static void LogException(Exception ex, bool showMessage = true)
        {
            try
            {
                var type = ex.GetType();
                using (var w = new StreamWriter(Path.Combine(System.Windows.Forms.Application.StartupPath, "pnotes.log"), true))
                {
                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", PNRuntimes.Instance.CultureInvariant));
                    sb.AppendLine();
#if DEBUG
                    sb.Append(ex.StackTrace);
#else
                    var stack = new StackTrace(ex, true);
                    var frame = stack.GetFrame(stack.FrameCount - 1);

                    sb.Append("Type: ");
                    sb.Append(type);
                    sb.AppendLine();
                    sb.Append("Message: ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append("In: ");
                    sb.Append(frame.GetFileName());
                    sb.Append("; at: ");
                    sb.Append(frame.GetMethod().Name);
                    var line = frame.GetFileLineNumber();
                    var column = frame.GetFileColumnNumber();
                    if (line != 0 || column != 0)
                    {
                        sb.Append("; line: ");
                        sb.Append(line);
                        sb.Append("; column: ");
                        sb.Append(column);
                    }
                    //else
                    //{
                    //    sb.Append("; line: undefined; column: undefined");
                    //}
                    if (type == typeof(SQLiteDataException))
                    {
                        if (ex is SQLiteDataException dataException)
                        {
                            sb.AppendLine();
                            sb.Append("SQL expression: ");
                            sb.Append(dataException.SQLiteMessage);
                        }
                    }
                    sb.Append(" ");
                    sb.Append(PNRegistry.GetOsFriendlyName());
                    sb.AppendLine();
                    sb.Append("***************************");
#endif
                    w.WriteLine(sb.ToString());
                }
                PNRuntimes.Instance.HideSplash = true;
                if (showMessage)
                    WPFMessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch
            {
                //do nothing
            }
        }
    }
}

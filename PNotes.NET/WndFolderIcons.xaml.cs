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

using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFStandardStyles;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndFolderIcons.xaml
    /// </summary>
    public partial class WndFolderIcons
    {
        internal event EventHandler<GroupPropertyChangedEventArgs> GroupPropertyChanged;

        public WndFolderIcons()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private void oKClick()
        {
            try
            {
                var btn = pnlIcons.Children.OfType<FolderButton>().FirstOrDefault(fb => fb.IsChecked != null && fb.IsChecked.Value);
                if (!(btn?.Content is StackPanel st)) return;
                if (!(st.Children[0] is Image image)) return;
                if (GroupPropertyChanged == null) return;
                GroupPropertyChanged(this, new GroupPropertyChangedEventArgs(image.Source, GroupChangeType.Image));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fromFileClick()
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter =
                        "Image files (*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*;tif)|*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.ico;*.tif"
                };
                if (!ofd.ShowDialog(this).Value) return;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.UriSource = new Uri(ofd.FileName, UriKind.Relative);
                bitmapImage.EndInit();
                if (Math.Abs(bitmapImage.Width - 16) > double.Epsilon || Math.Abs(bitmapImage.Height - 16) > double.Epsilon)
                {
                    WPFMessageBox.Show(
                        PNLang.Instance.GetMessageText("image_size_message", "The size of image has to be 16x16"),
                        PNStrings.PROG_NAME);
                    return;
                }
                if (GroupPropertyChanged == null) return;
                GroupPropertyChanged(this, new GroupPropertyChangedEventArgs(bitmapImage, GroupChangeType.Image));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgFolderIcons_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                loadImages();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadImages()
        {
            try
            {
                for (var i = 1; i <= 121; i++)
                {
                    var uri = new Uri(PNStrings.FOLDERS_PREFIX + i.ToString("folders_00000") + ".png");
                    var bmp = new BitmapImage(uri);
                    var st = new StackPanel();
                    var image = new Image
                    {
                        Source = bmp,
                        Stretch = Stretch.None,
                        Margin = new Thickness(4)
                    };
                    var tb = new TextBlock
                    {
                        Text = i.ToString(),
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(4)
                    };
                    st.Children.Add(image);
                    st.Children.Add(tb);
                    var folderButton = new FolderButton {Content = st};
                    pnlIcons.Children.Add(folderButton);
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
                        e.CanExecute = pnlIcons.Children.OfType<FolderButton>().Any(fb => fb.IsChecked != null && fb.IsChecked.Value);
                        break;
                    case CommandType.Cancel:
                    case CommandType.LoadFromFile:
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
                    case CommandType.LoadFromFile:
                        fromFileClick();
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

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

using PNStaticFonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndNewGroup.xaml
    /// </summary>
    public partial class WndNewGroup
    {
        internal event EventHandler<GroupChangedEventArgs> GroupChanged;

        public WndNewGroup()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndNewGroup(PNGroup group, PNTreeItem treeItem)
            : this()
        {
            if (group != null)
            {
                _Group = (PNGroup)group.Clone();
                _Mode = AddEditMode.Edit;
            }
            else
            {
                _Group = new PNGroup { IsDefaultImage = true };
            }
            _TreeItem = treeItem;
        }

        private readonly PNGroup _Group;
        private readonly PNTreeItem _TreeItem;
        private readonly AddEditMode _Mode = AddEditMode.Add;
        private bool _Loaded;
        private bool _Shown;

        private void DlgNewGroup_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                loadSkinsList();
                Title = _Mode == AddEditMode.Add
                           ? PNLang.Instance.GetCaptionText("add_new_group", "Add new group")
                           : PNLang.Instance.GetCaptionText("edit_group", "Edit group") + @" [" + _Group.Name + @"]";
                loadLogFonts();

                fillGroupProperties();

                cboFontColor.IsDropDownOpen = true;
                cboFontColor.IsDropDownOpen = false;

                cboFontSize.IsDropDownOpen = true;
                cboFontSize.IsDropDownOpen = false;
                FlowDirection = PNLang.Instance.GetFlowDirection();
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                    tabGroups.SelectedIndex = 1;
                _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            try
            {
                base.OnContentRendered(e);
                if (_Shown) return;
                _Shown = true;
                txtName.SelectAll();
                txtName.Focus();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void changeIcon()
        {
            try
            {
                var dlgIcons = new WndFolderIcons { Owner = this };
                dlgIcons.GroupPropertyChanged += dlgIcons_GroupPropertyChanged;
                var showDialog = dlgIcons.ShowDialog();
                if (showDialog == null || !showDialog.Value)
                {
                    dlgIcons.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void standardClick()
        {
            try
            {
                _Loaded = false;
                var gr = (PNGroup)_Group.Clone();

                _Group.Clear();
                _Group.Name = gr.Id == 0 ? PNLang.Instance.GetGroupName("General", "General") : gr.Name;
                _Group.Id = gr.Id;
                _Group.ParentId = gr.ParentId;
                var image = TryFindResource("gr") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "gr.png"));
                _Group.Image = image;
                _Group.IsDefaultImage = true;

                imgGroupIcon.Source = _Group.Image;
                fillGroupProperties();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _Loaded = true;
            }
        }

        private void oKClick()
        {
            try
            {
                GroupChanged?.Invoke(this, new GroupChangedEventArgs(_Group, _Mode, _TreeItem));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                if (!_Loaded) return;
                _Group.Skinless.BackColor = e.NewValue;
                pvwSkl.SetProperties(_Group);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void skinlessClick()
        {
            try
            {
                var fc = new WndFontChooser(_Group.Skinless.CaptionFont, _Group.Skinless.CaptionColor) { Owner = this };
                var showDialog = fc.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    _Group.Skinless.CaptionFont = fc.SelectedFont;
                    _Group.Skinless.CaptionColor = fc.SelectedColor;
                    pvwSkl.SetProperties(_Group);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstSkins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lstSkins.SelectedIndex < 0)
                {
                    pvwSkin.EraseProperties();
                    return;
                }

                if (!(lstSkins.SelectedItem is PNListBoxItem item)) return;
                if (_Group.Skin.SkinName != item.Text)
                {
                    _Group.Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, _Group.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, _Group.Skin);
                    }
                }
                if (_Group.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(_Group, _Group.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                    //PNStatic.DrawSkinPreview(_Group, _Group.Skin, imgSkin,
                    //    w - imgSkin.Margin.Left - imgSkin.Margin.Right,
                    //    h - imgSkin.Margin.Top - imgSkin.Margin.Bottom);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadSkinsList()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                var fi = di.GetFiles("*.pnskn");
                var image = TryFindResource("skins") as BitmapImage;//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
                foreach (var f in fi)
                {
                    lstSkins.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name), f.FullName));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadLogFonts()
        {
            try
            {
                var list = new List<LOGFONT>();
                PNStaticFonts.Fonts.GetFontsList(list);
                var ordered = list.OrderBy(f => f.lfFaceName);
                foreach (var lf in ordered)
                {
                    cboFonts.Items.Add(lf);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillGroupProperties()
        {
            try
            {
                imgGroupIcon.Source = _Group.Image;
                txtName.Text = _Group.Name;
                if (_Group.Id == 0)
                {
                    //general
                    txtName.IsReadOnly = true;
                }
                pckBGSknls.SelectedColor = _Group.Skinless.BackColor;
                pvwSkl.SetProperties(_Group);
                if (_Group.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == _Group.Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedItem = lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it =>
                        it.Text == PNCollections.Instance.Groups.GetGroupById(0)?.Skin.SkinName);
                }
                cboFonts.SelectedItem =
                    cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == _Group.Font.lfFaceName);
                foreach (var t in from object t in cboFontColor.Items
                                  let rc = t as Rectangle
                                  where rc != null
                                  let sb = rc.Fill as SolidColorBrush
                                  where sb != null
                                  where
                                      sb.Color ==
                                      Color.FromArgb(_Group.FontColor.A, _Group.FontColor.R, _Group.FontColor.G,
                                          _Group.FontColor.B)
                                  select t)
                {
                    cboFontColor.SelectedItem = t;
                    break;
                }

                var fontHeight = _Group.Font.GetFontSize();
                cboFontSize.SelectedItem = fontHeight;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtName.Text.Trim().Length > 0)
                {
                    _Group.Name = txtName.Text.Trim();
                }
                else
                {
                    if (_Mode == AddEditMode.Add)
                    {
                        _Group.Name = "";
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFonts.SelectedIndex < 0) return;
                var lf = (LOGFONT)e.AddedItems[0];
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(lf.lfFaceName);
                logF.SetFontSize((int)cboFontSize.SelectedItem);
                _Group.Font = logF;
                pvwSkl.SetProperties(_Group);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(_Group, _Group.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                    //PNStatic.DrawSkinPreview(_Group, _Group.Skin, imgSkin,
                    //    w - imgSkin.Margin.Left - imgSkin.Margin.Right,
                    //    h - imgSkin.Margin.Top - imgSkin.Margin.Bottom);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void dlgIcons_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            try
            {
                if (sender is WndFolderIcons d) d.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                imgGroupIcon.Source = (BitmapImage)e.NewStateObject;
                _Group.Image = (BitmapImage)e.NewStateObject;
                _Group.IsDefaultImage = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontColor.SelectedIndex < 0) return;
                if (!(cboFontColor.SelectedItem is Rectangle rc)) return;
                if (!(rc.Fill is SolidColorBrush sb)) return;
                _Group.FontColor = System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                pvwSkl.SetProperties(_Group);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(_Group, _Group.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                    //PNStatic.DrawSkinPreview(_Group, _Group.Skin, imgSkin,
                    //    w - imgSkin.Margin.Left - imgSkin.Margin.Right,
                    //    h - imgSkin.Margin.Top - imgSkin.Margin.Bottom);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboFontSize.SelectedIndex < 0) return;
                var logF = new LOGFONT();
                logF.Init();
                logF.SetFontFace(_Group.Font.lfFaceName);
                logF.SetFontSize((int)cboFontSize.SelectedItem);
                _Group.Font = logF;
                pvwSkl.SetProperties(_Group);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(_Group, _Group.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                    //PNStatic.DrawSkinPreview(_Group, _Group.Skin, imgSkin,
                    //    w - imgSkin.Margin.Left - imgSkin.Margin.Right,
                    //    h - imgSkin.Margin.Top - imgSkin.Margin.Bottom);
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
                        e.CanExecute = txtName.Text.Trim().Length > 0;
                        break;
                    case CommandType.Cancel:
                    case CommandType.StandardView:
                    case CommandType.SkinlessCaptionFont:
                    case CommandType.GroupIcon:
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
                    case CommandType.StandardView:
                        standardClick();
                        break;
                    case CommandType.SkinlessCaptionFont:
                        skinlessClick();
                        break;
                    case CommandType.GroupIcon:
                        changeIcon();
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

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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndAdjustAppearance.xaml
    /// </summary>
    public partial class WndAdjustAppearance
    {
        internal event EventHandler<NoteAppearanceAdjustedEventArgs> NoteAppearanceAdjusted;

        public WndAdjustAppearance()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndAdjustAppearance(PNote note)
            : this()
        {
            _Note = note;
        }

        private readonly PNote _Note;
        private string _TransCaption;
        private double _Opacity;
        private bool _CustomOpacity, _CustomSkinless, _CustomSkin;
        PNGroup _Group;
        private PNSkinlessDetails _Skinless;
        private PNSkinDetails _Skin;

        private void oKClick()
        {
            try
            {
                NoteAppearanceAdjusted?.Invoke(this, new NoteAppearanceAdjustedEventArgs(_CustomOpacity, _CustomSkinless, _CustomSkin, _Opacity, _Skinless, _Skin));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void standardClick()
        {
            _Skinless = (PNSkinlessDetails)_Group.Skinless.Clone();
            _Skin = _Group.Skin.Clone();
            _Opacity = PNRuntimes.Instance.Settings.Behavior.Opacity;
            trkTrans.Value = (int)(100 - (_Opacity * 100));
            pckBGSknls.SelectedColor = _Skinless.BackColor;
            if (GridSkinnable.Visibility == Visibility.Visible)
            {
                if (_Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == _Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedItem = lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it =>
                        it.Text == PNCollections.Instance.Groups.GetGroupById(0)?.Skin.SkinName);
                }
            }
            _CustomOpacity = _CustomSkin = _CustomSkinless = false;
            pvwSkl.SetProperties(_Group, _Skinless);
        }

        private void fontSknlsClick()
        {
            try
            {
                var fontChooser = new WndFontChooser(_Skinless.CaptionFont, _Skinless.CaptionColor) { Owner = this };
                var result = fontChooser.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    _Skinless = new PNSkinlessDetails
                    {
                        CaptionFont = fontChooser.SelectedFont,
                        CaptionColor = fontChooser.SelectedColor,
                        BackColor = _Skinless.BackColor
                    };
                    _CustomSkinless = true;
                    pvwSkl.SetProperties(_Group, _Skinless);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgAdjustAppearance_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNRuntimes.Instance.Settings.GeneralSettings.UseSkins)
                {
                    GridSkinless.Visibility = Visibility.Hidden;
                    GridSkinnable.Visibility = Visibility.Visible;
                }
                _CustomOpacity = _Note.CustomOpacity;
                applyLanguage();
                if (_CustomOpacity)
                {
                    trkTrans.Value = (int)(100 - (_Note.Opacity * 100));
                    _Opacity = _Note.Opacity;
                }
                else
                {
                    trkTrans.Value = (int)(100 - (PNRuntimes.Instance.Settings.Behavior.Opacity * 100));
                    _Opacity = PNRuntimes.Instance.Settings.Behavior.Opacity;
                }

                _Group = PNCollections.Instance.Groups.GetGroupById(_Note.GroupId);
                if (_Group == null)
                {
                    throw new Exception("Group cannot be null");
                }

                if (_Note.Skinless != null)
                {
                    _CustomSkinless = true;
                    _Skinless = (PNSkinlessDetails)_Note.Skinless.Clone();
                }
                else
                {
                    _Skinless = (PNSkinlessDetails)_Group.Skinless.Clone();
                }

                pckBGSknls.SelectedColor = _Skinless.BackColor;

                if (GridSkinnable.Visibility == Visibility.Visible)
                {
                    if (_Note.Skin != null)
                    {
                        _CustomSkin = true;
                        _Skin = _Note.Skin.Clone();
                    }
                    else
                    {
                        if (_Group.Skin == null || _Group.Skin.SkinName == PNSkinDetails.NO_SKIN)
                        {
                            // get General png skin
                            _Skin = PNCollections.Instance.Groups.GetGroupById(0).Skin.Clone();
                        }
                        else
                        {
                            _Skin = _Group.Skin.Clone();
                        }
                    }
                    loadSkinsList();
                    if (_Skin.SkinName != PNSkinDetails.NO_SKIN)
                    {
                        lstSkins.SelectedItem =
                            lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == _Skin.SkinName);
                    }
                    else
                    {
                        lstSkins.SelectedItem = lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it =>
                            it.Text == PNCollections.Instance.Groups.GetGroupById(0)?.Skin.SkinName);
                    }
                }

                pvwSkl.SetProperties(_Group, _Skinless);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void applyLanguage()
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title += @" - " + _Note.Name;
                _TransCaption = PNLang.Instance.GetCaptionText("transparency", "Transparency");
                if (_CustomOpacity)
                {
                    lblTransPerc.Text = _TransCaption + @": " + (100.0 - (_Note.Opacity * 100)).ToString("0%");
                }
                else
                {
                    lblTransPerc.Text = _TransCaption + @": " + (100.0 - (PNRuntimes.Instance.Settings.Behavior.Opacity * 100)).ToString("0%");
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
                var image = TryFindResource("skins") as BitmapImage;// new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
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

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                _Skinless = new PNSkinlessDetails
                {
                    CaptionFont = _Skinless.CaptionFont,
                    CaptionColor = _Skinless.CaptionColor,
                    BackColor = e.NewValue
                };
                _CustomSkinless = true;
                pvwSkl.SetProperties(_Group, _Skinless);
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
                    case CommandType.Cancel:
                    case CommandType.StandardView:
                    case CommandType.SkinlessCaptionFont:
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
                    case CommandType.StandardView:
                        standardClick();
                        break;
                    case CommandType.SkinlessCaptionFont:
                        fontSknlsClick();
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkTrans_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!(sender is Slider slider)) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _CustomOpacity = true;
                _Opacity = (100.0 - e.NewValue) / 100.0;
                lblTransPerc.Text = _TransCaption + @": " + trkTrans.Value.ToString(PNRuntimes.Instance.CultureInvariant) + @"%";
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
                if (_Skin.SkinName != item.Text)
                {
                    _CustomSkin = true;
                    _Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, _Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, _Skin);
                    }
                }
                if (_Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(_Group, _Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}

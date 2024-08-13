using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for UcSchedule.xaml
    /// </summary>
    public partial class UcSchedule : ISettingsPage
    {
        public UcSchedule()
        {
            InitializeComponent();
        }

        private bool _Loaded;
        private WndSettings _ParentWindow;

        public void Init(WndSettings setings)
        {
            _ParentWindow = setings;
        }

        public void InitPage(bool firstTime)
        {
            initPageSchedule(firstTime);
        }

        public ChangesAction DefineChangesAction()
        {
            return ChangesAction.None;
        }

        public bool SavePage()
        {
            PNStatic.Settings.Schedule = (PNSchedule)_ParentWindow.TempSettings.Schedule.Clone();
            PNData.SaveScheduleSettings();
            return true;
        }

        public bool SaveCollections()
        {
            return true;
        }

        public bool CanExecute()
        {
            return false;
        }

        public void RestoreDefaultValues()
        {
        }

        public bool InDefClick { get; set; }

        public event EventHandler PromptToRestart;

        #region Schedule staff

        internal void ApplyPanelLanguage()
        {
            try
            {
                fillDaysOfWeek();
                if (_ParentWindow.TempSettings != null && _ParentWindow.TempSettings.Schedule != null)
                {
                    var current = _ParentWindow.TempSettings.Schedule.FirstDayOfWeek;
                    var cbitem = cboDOW.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (DayOfWeek)i.Tag == current);
                    if (cbitem != null)
                        cboDOW.SelectedItem = cbitem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageSchedule(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    var image = TryFindResource("loudspeaker") as BitmapImage;
                    // new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                    clbSounds.Items.Add(
                        new PNListBoxItem(image, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND, PNSchedule.DEF_SOUND));
                    if (Directory.Exists(PNPaths.Instance.SoundsDir))
                    {
                        var fi = new DirectoryInfo(PNPaths.Instance.SoundsDir).GetFiles("*.wav");
                        foreach (var f in fi)
                        {
                            clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(f.Name),
                                f.FullName));
                        }
                    }
                    foreach (var s in PNStatic.Voices)
                    {
                        lstVoices.Items.Add(new PNListBoxItem(image, s, s));
                    }
                }
                chkAllowSound.IsChecked = _ParentWindow.TempSettings.Schedule.AllowSoundAlert;
                chkVisualNotify.IsChecked = _ParentWindow.TempSettings.Schedule.VisualNotification;
                chkTrackOverdue.IsChecked = _ParentWindow.TempSettings.Schedule.TrackOverdue;
                chkCenterScreen.IsChecked = _ParentWindow.TempSettings.Schedule.CenterScreen;
                var item =
                    clbSounds.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _ParentWindow.TempSettings.Schedule.Sound);
                if (item != null)
                    clbSounds.SelectedItem = item;
                else
                {
                    clbSounds.SelectedIndex = 0;
                    _ParentWindow.TempSettings.Schedule.Sound = PNSchedule.DEF_SOUND;
                }
                if (PNStatic.Voices.Count > 0)
                {

                    item = lstVoices.Items.OfType<PNListBoxItem>()
                        .FirstOrDefault(li => li.Text == _ParentWindow.TempSettings.Schedule.Voice);
                    if (item != null)
                    {
                        lstVoices.SelectedItem = item;
                    }
                    else
                    {
                        lstVoices.SelectedIndex = 0;
                    }
                    txtVoiceSample.Text = "";
                }
                trkVolume.Value = _ParentWindow.TempSettings.Schedule.VoiceVolume;
                trkSpeed.Value = _ParentWindow.TempSettings.Schedule.VoiceSpeed;
                optDOWStandard.IsChecked = _ParentWindow.TempSettings.Schedule.FirstDayOfWeekType == FirstDayOfWeekType.Standard;
                optDOWCustom.IsChecked = _ParentWindow.TempSettings.Schedule.FirstDayOfWeekType != FirstDayOfWeekType.Standard;
                var current = _ParentWindow.TempSettings.Schedule.FirstDayOfWeek;
                var cbitem = cboDOW.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (DayOfWeek)i.Tag == current);
                if (cbitem != null)
                    cboDOW.SelectedItem = cbitem;
                if(firstTime)
                    _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckSchedule_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAllowSound":
                        _ParentWindow.TempSettings.Schedule.AllowSoundAlert = cb.IsChecked.Value;
                        break;
                    case "chkVisualNotify":
                        _ParentWindow.TempSettings.Schedule.VisualNotification = cb.IsChecked.Value;
                        break;
                    case "chkCenterScreen":
                        _ParentWindow.TempSettings.Schedule.CenterScreen = cb.IsChecked.Value;
                        break;
                    case "chkTrackOverdue":
                        _ParentWindow.TempSettings.Schedule.TrackOverdue = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void clbSounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdRemoveSound.IsEnabled = cmdListenSound.IsEnabled = false;
                if (clbSounds.SelectedIndex < 0) return;
                cmdListenSound.IsEnabled = true;
                var item = clbSounds.SelectedItem as PNListBoxItem;
                if (item != null)
                    _ParentWindow.TempSettings.Schedule.Sound = item.Text;
                if (clbSounds.SelectedIndex > 0)
                    cmdRemoveSound.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstVoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdVoiceSample.IsEnabled = txtVoiceSample.Text.Trim().Length > 0 && lstVoices.SelectedIndex >= 0;
                if (lstVoices.SelectedIndex < 0) return;
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                cmdDefVoice.IsEnabled = voice.Text != _ParentWindow.TempSettings.Schedule.Voice;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDefVoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstVoices.SelectedIndex < 0) return;
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                if (_ParentWindow.TempSettings.Schedule.Voice == voice.Text) return;
                _ParentWindow.TempSettings.Schedule.Voice = voice.Text;
                cmdDefVoice.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdVoiceSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var voice = lstVoices.Items[lstVoices.SelectedIndex] as PNListBoxItem;
                if (voice == null) return;
                var voiceName = voice.Text;
                PNStatic.SpeakText(txtVoiceSample.Text.Trim(), voiceName);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtVoiceSample_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdVoiceSample.IsEnabled = txtVoiceSample.Text.Trim().Length > 0 && lstVoices.SelectedIndex >= 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _ParentWindow.TempSettings.Schedule.VoiceVolume = (int)trkVolume.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void trkSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = sender as Slider;
                if (slider == null) return;
                slider.Value = Math.Round(e.NewValue, 0);
                _ParentWindow.TempSettings.Schedule.VoiceSpeed = (int)trkSpeed.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addString = true;
                var ofd = new OpenFileDialog
                {
                    Filter = @"Windows audio files|*.wav",
                    Title = PNLang.Instance.GetCaptionText("choose_sound", "Choose sound")
                };
                if (!ofd.ShowDialog(_ParentWindow).Value) return;
                if (!Directory.Exists(PNPaths.Instance.SoundsDir))
                    Directory.CreateDirectory(PNPaths.Instance.SoundsDir);
                if (File.Exists(PNPaths.Instance.SoundsDir + @"\" + Path.GetFileName(ofd.FileName)))
                {
                    if (
                        PNMessageBox.Show(
                            PNLang.Instance.GetMessageText("sound_exists",
                                "The file already exists in your 'sounds' directory. Copy anyway?"),
                            PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    addString = false;
                }
                var path2 = Path.GetFileName(ofd.FileName);
                if (path2 == null) return;
                File.Copy(ofd.FileName, Path.Combine(PNPaths.Instance.SoundsDir, path2), true);
                if (!addString) return;
                var image = TryFindResource("loudspeaker") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "loudspeaker.png"));
                clbSounds.Items.Add(new PNListBoxItem(image, Path.GetFileNameWithoutExtension(ofd.FileName),
                    ofd.FileName));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRemoveSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PNMessageBox.Show(PNLang.Instance.GetMessageText("sound_delete", "Delete selected sound?"),
                        PNLang.Instance.GetCaptionText("confirm", "Confirmation"), MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                var index = clbSounds.SelectedIndex;
                var item = clbSounds.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var sound = item.Text;
                clbSounds.SelectedIndex = clbSounds.SelectedIndex - 1;
                clbSounds.Items.RemoveAt(index);
                File.Delete(Path.Combine(PNPaths.Instance.SoundsDir, sound + ".wav"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdListenSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ParentWindow.TempSettings.Schedule.Sound == PNSchedule.DEF_SOUND)
                {
                    PNSound.PlayDefaultSound();
                }
                else
                {
                    PNSound.PlaySound(Path.Combine(PNPaths.Instance.SoundsDir, _ParentWindow.TempSettings.Schedule.Sound + ".wav"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillDaysOfWeek()
        {
            try
            {
                cboDOW.Items.Clear();
                var ci = new CultureInfo(PNLang.Instance.GetLanguageCulture());
                foreach (DayOfWeek dw in Enum.GetValues(typeof(DayOfWeek)))
                {
                    cboDOW.Items.Add(new ComboBoxItem { Content = ci.DateTimeFormat.DayNames[(int)dw], Tag = dw });
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionDowChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.Schedule.FirstDayOfWeekType = optDOWStandard.IsChecked != null && optDOWStandard.IsChecked.Value ? FirstDayOfWeekType.Standard : FirstDayOfWeekType.User;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDOW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                var item = cboDOW.SelectedItem as ComboBoxItem;
                if (item == null) return;
                _ParentWindow.TempSettings.Schedule.FirstDayOfWeek = (DayOfWeek)item.Tag;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion
    }
}

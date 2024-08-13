using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml.Linq;
using PNRichEdit;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for UcGeneral.xaml
    /// </summary>
    public partial class UcGeneral : ISettingsPage
    {
        public UcGeneral()
        {
            InitializeComponent();
        }

        private bool _Loaded;
        private WndSettings _ParentWindow;

        public void Init(WndSettings setings)
        {
            _ParentWindow = setings;
            _Externals = PNStatic.Externals.PNClone();
            _SProviders = PNStatic.SearchProviders.PNClone();
            _Tags = PNStatic.Tags.PNClone();
            var f = new PNFont();
            cmdRestoreFontUI.IsEnabled = f != PNSingleton.Instance.FontUser;
        }

        public void InitPage(bool firstTime)
        {
            initPageGeneral(firstTime);
        }

        public ChangesAction DefineChangesAction()
        {
            var result = ChangesAction.None;
            if (PNStatic.Settings.GeneralSettings.DockWidth != _ParentWindow.TempSettings.GeneralSettings.DockWidth ||
                PNStatic.Settings.GeneralSettings.DockHeight != _ParentWindow.TempSettings.GeneralSettings.DockHeight)
            {
                result |= ChangesAction.ChangeDockSize;
            }

            return result;
        }

        public bool SavePage()
        {
            return saveGeneral();
        }

        public bool SaveCollections()
        {
            if (PNStatic.Externals.Inequals(_Externals))
            {
                PNStatic.Externals = _Externals.PNClone();
                PNData.SaveExternals();
            }
            if (PNStatic.SearchProviders.Inequals(_SProviders))
            {
                PNStatic.SearchProviders = _SProviders.PNClone();
                PNData.SaveSearchProviders();
            }
            if (PNStatic.Tags.Inequals(_Tags))
            {
                if (PNNotesOperations.ResetNotesTags(_Tags))
                {
                    PNStatic.Tags = _Tags.PNClone();
                    PNData.SaveTags();
                }
            }
            return true;
        }

        public bool CanExecute()
        {
            if (File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH)) &&
                (chkNoSplash.IsChecked == null || !chkNoSplash.IsChecked.Value))
                return true;
            if (!File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH)) &&
                chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                return true;
            if (PNStatic.Externals.Inequals(_Externals))
                return true;
            if (PNStatic.SearchProviders.Inequals(_SProviders))
                return true;
            if (PNStatic.Tags.Inequals(_Tags))
                return true;
            return false;
        }

        public void RestoreDefaultValues()
        {
            chkNoSplash.IsChecked = false;
            cmdRestoreFontUI.PerformClick();
        }

        public bool InDefClick { get; set; }

        public event EventHandler PromptToRestart;

        #region General staff
        private class _Language
        {
            public string LangName;
            public string LangFile;
            public string Culture;

            public override string ToString()
            {
                return LangName;
            }
        }

        private class _ButtonSize
        {
            public ToolStripButtonSize ButtonSize;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public class SProvider
        {
            public string Name { get; private set; }
            public string Query { get; private set; }

            public SProvider(string n, string q)
            {
                Name = n;
                Query = q;
            }
        }

        public class SExternal
        {
            public string Name { get; private set; }
            public string Prog { get; private set; }
            public string CommLine { get; private set; }

            public SExternal(string n, string p, string c)
            {
                Name = n;
                Prog = p;
                CommLine = c;
            }
        }

        private readonly List<_ButtonSize> _ButtonSizes =
            new List<_ButtonSize>
            {
                new _ButtonSize {ButtonSize = ToolStripButtonSize.Normal, Name = "Normal"},
                new _ButtonSize {ButtonSize = ToolStripButtonSize.Large, Name = "Large"}
            };
        private List<string> _Tags;
        private List<PNExternal> _Externals;
        private List<PNSearchProvider> _SProviders;
        private readonly ObservableCollection<SProvider> _SearchProvidersList = new ObservableCollection<SProvider>();
        private readonly ObservableCollection<SExternal> _ExternalList = new ObservableCollection<SExternal>();

        internal void ApplyFirstTimeLanguage()
        {
            cmdSetFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdSetFontUI", "Change");
            cmdRestoreFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdResetFontUI", "Reset");
        }

        internal void ApplyLanguage()
        {
            cmdSetFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdSetFontUI", "Change");
            cmdRestoreFontUI.Content = PNLang.Instance.GetMenuText("settings_menu", "cmdResetFontUI", "Reset");
        }

        internal void ApplyPanelLanguage()
        {
            try
            {
                //special cases
                if (cboDeleteBin.Items.Count > 0)
                {
                    var index = cboDeleteBin.SelectedIndex;
                    cboDeleteBin.Items.RemoveAt(0);
                    cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never"));
                    cboDeleteBin.SelectedIndex = index;
                }
                _ButtonSizes[0].Name = PNLang.Instance.GetCaptionText("b_size_normal", "Normal");
                _ButtonSizes[1].Name = PNLang.Instance.GetCaptionText("b_size_large", "Large");
                if (cboButtonsSize.Items.Count > 0)
                {
                    var index = cboButtonsSize.SelectedIndex;
                    cboButtonsSize.Items.Clear();
                    foreach (var bs in _ButtonSizes)
                    {
                        cboButtonsSize.Items.Add(bs);
                    }
                    cboButtonsSize.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool ExternalExists(string extName)
        {
            return _Externals.Any(e => e.Name == extName);
        }

        internal void ExternalAdd(PNExternal ext)
        {
            try
            {
                _Externals.Add(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void ExternalReplace(PNExternal ext)
        {
            try
            {
                var e = _Externals.FirstOrDefault(ex => ex.Name == ext.Name);
                if (e == null) return;
                e.Program = ext.Program;
                e.CommandLine = ext.CommandLine;
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal bool SearchProviderExists(string spName)
        {
            return _SProviders.Any(s => s.Name == spName);
        }

        internal void SearchProviderAdd(PNSearchProvider sp)
        {
            try
            {
                _SProviders.Add(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void SearchProviderReplace(PNSearchProvider sp)
        {
            try
            {
                var s = _SProviders.FirstOrDefault(spv => spv.Name == sp.Name);
                if (s != null)
                {
                    s.QueryString = sp.QueryString;
                }
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageGeneral(bool firstTime)
        {
            try
            {
                if (firstTime)
                {
                    addLanguages();
                    fillSearchProviders();
                    fillExternals();
                    fillTags();
                    loadRemoveFromBinPeriods();
                }
                chkHideToolbar.IsChecked = _ParentWindow.TempSettings.GeneralSettings.HideToolbar;
                chkCustomFont.IsChecked = _ParentWindow.TempSettings.GeneralSettings.UseCustomFonts;
                chkHideDelete.IsChecked = _ParentWindow.TempSettings.GeneralSettings.HideDeleteButton;
                chkChangeHideToDelete.IsChecked = _ParentWindow.TempSettings.GeneralSettings.ChangeHideToDelete;
                chkHideHide.IsChecked = _ParentWindow.TempSettings.GeneralSettings.HideHideButton;
                cboIndent.SelectedItem = (int)_ParentWindow.TempSettings.GeneralSettings.BulletsIndent;
                cboParIndent.SelectedItem = _ParentWindow.TempSettings.GeneralSettings.ParagraphIndent;
                cboMargins.SelectedItem = (int)_ParentWindow.TempSettings.GeneralSettings.MarginWidth;
                txtDTFShort.Text = _ParentWindow.TempSettings.GeneralSettings.DateFormat;
                txtTFLong.Text = _ParentWindow.TempSettings.GeneralSettings.TimeFormat;
                chkSaveOnExit.IsChecked = _ParentWindow.TempSettings.GeneralSettings.SaveOnExit;
                chkConfirmDelete.IsChecked = _ParentWindow.TempSettings.GeneralSettings.ConfirmBeforeDeletion;
                chkConfirmSave.IsChecked = _ParentWindow.TempSettings.GeneralSettings.ConfirmSaving;
                chkSaveWithoutConfirm.IsChecked = _ParentWindow.TempSettings.GeneralSettings.SaveWithoutConfirmOnHide;
                chkAutosave.IsChecked = _ParentWindow.TempSettings.GeneralSettings.Autosave;
                updAutosave.Value = _ParentWindow.TempSettings.GeneralSettings.AutosavePeriod;
                cboDeleteBin.SelectedIndex = _ParentWindow.TempSettings.GeneralSettings.RemoveFromBinPeriod > 0
                    ? cboDeleteBin.Items.IndexOf(
                        _ParentWindow.TempSettings.GeneralSettings.RemoveFromBinPeriod.ToString(
                            PNStatic.CultureInvariant))
                    : 0;
                chkWarnBeforeEmptyBin.IsChecked = _ParentWindow.TempSettings.GeneralSettings.WarnOnAutomaticalDelete;
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
                chkRunOnStart.IsChecked = _ParentWindow.TempSettings.GeneralSettings.RunOnStart;
                chkShowCPOnStart.IsChecked = _ParentWindow.TempSettings.GeneralSettings.ShowCPOnStart;
                chkCheckNewVersionOnStart.IsChecked = _ParentWindow.TempSettings.GeneralSettings.CheckNewVersionOnStart;
                chkCheckCriticalOnStart.IsChecked = _ParentWindow.TempSettings.GeneralSettings.CheckCriticalOnStart;
                chkCheckCriticalPeriodically.IsChecked = _ParentWindow.TempSettings.GeneralSettings.CheckCriticalPeriodically;
                chkShowPriority.IsChecked = _ParentWindow.TempSettings.GeneralSettings.ShowPriorityOnStart;
                txtWidthSknlsDef.Value = _ParentWindow.TempSettings.GeneralSettings.Width;
                txtHeightSknlsDef.Value = _ParentWindow.TempSettings.GeneralSettings.Height;
                pckSpell.SelectedColor = Color.FromArgb(_ParentWindow.TempSettings.GeneralSettings.SpellColor.A,
                    _ParentWindow.TempSettings.GeneralSettings.SpellColor.R, _ParentWindow.TempSettings.GeneralSettings.SpellColor.G,
                    _ParentWindow.TempSettings.GeneralSettings.SpellColor.B);
                foreach (var bs in _ButtonSizes)
                {
                    cboButtonsSize.Items.Add(bs);
                }
                for (var i = 0; i < cboButtonsSize.Items.Count; i++)
                {
                    var bs = cboButtonsSize.Items[i] as _ButtonSize;
                    if (bs == null || bs.ButtonSize != _ParentWindow.TempSettings.GeneralSettings.ButtonsSize) continue;
                    cboButtonsSize.SelectedIndex = i;
                    break;
                }
                for (var i = 0; i < cboScrollBars.Items.Count; i++)
                {
                    var scb = (System.Windows.Forms.RichTextBoxScrollBars)i;
                    if (_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar != scb) continue;
                    cboScrollBars.SelectedIndex = i;
                    break;
                }
                chkAutomaticSmilies.IsChecked = _ParentWindow.TempSettings.GeneralSettings.AutomaticSmilies;
                updSpace.Value = _ParentWindow.TempSettings.GeneralSettings.SpacePoints;
                chkNoSplash.IsChecked = File.Exists(Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH));
                chkRestoreAutomatically.IsChecked = _ParentWindow.TempSettings.GeneralSettings.RestoreAuto;
                chkAutoHeight.IsChecked = _ParentWindow.TempSettings.GeneralSettings.AutoHeight;
                chkDeleteShortExit.IsChecked = _ParentWindow.TempSettings.GeneralSettings.DeleteShortcutsOnExit;
                chkRestoreShortStart.IsChecked = _ParentWindow.TempSettings.GeneralSettings.RestoreShortcutsOnStart;
                if(firstTime)
                    _Loaded = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool saveGeneral()
        {
            try
            {
                var applyAutoHeight = false;
                var splashFile = Path.Combine(PNPaths.Instance.DataDir, PNStrings.NOSPLASH);
                if (chkNoSplash.IsChecked != null && chkNoSplash.IsChecked.Value)
                {
                    if (!File.Exists(splashFile))
                    {
                        using (new StreamWriter(splashFile, false))
                        {
                        }
                    }
                }
                else
                {
                    if (File.Exists(splashFile))
                    {
                        File.Delete(splashFile);
                    }
                }
                if (PNStatic.Settings.GeneralSettings.Language != _ParentWindow.TempSettings.GeneralSettings.Language)
                {
                    PNStatic.FormMain.ApplyNewLanguage(_ParentWindow.TempSettings.GeneralSettings.Language);
                }
                if (PNStatic.Settings.GeneralSettings.UseSkins != _ParentWindow.TempSettings.GeneralSettings.UseSkins)
                {
                    //apply or remove skins
                    PNStatic.Settings.GeneralSettings.UseSkins = _ParentWindow.TempSettings.GeneralSettings.UseSkins;
                }
                // hide toolbar
                if (PNStatic.Settings.GeneralSettings.HideToolbar != _ParentWindow.TempSettings.GeneralSettings.HideToolbar &&
                    _ParentWindow.TempSettings.GeneralSettings.HideToolbar)
                {
                    PNNotesOperations.ApplyHideToolbar();
                }
                // hide or show delete button
                if (PNStatic.Settings.GeneralSettings.HideDeleteButton !=
                    _ParentWindow.TempSettings.GeneralSettings.HideDeleteButton)
                {
                    PNNotesOperations.ApplyDeleteButtonVisibility(!_ParentWindow.TempSettings.GeneralSettings.HideDeleteButton);
                }
                // change hide to delete
                if (PNStatic.Settings.GeneralSettings.ChangeHideToDelete !=
                    _ParentWindow.TempSettings.GeneralSettings.ChangeHideToDelete)
                {
                    PNNotesOperations.ApplyUseAlternative(_ParentWindow.TempSettings.GeneralSettings.ChangeHideToDelete);
                }
                // hide or show hide button
                if (PNStatic.Settings.GeneralSettings.HideHideButton !=
                    _ParentWindow.TempSettings.GeneralSettings.HideHideButton)
                {
                    PNNotesOperations.ApplyHideButtonVisibility(!_ParentWindow.TempSettings.GeneralSettings.HideHideButton);
                }
                // scroll bars
                if (PNStatic.Settings.GeneralSettings.ShowScrollbar !=
                    _ParentWindow.TempSettings.GeneralSettings.ShowScrollbar)
                {
                    if (_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar == System.Windows.Forms.RichTextBoxScrollBars.None)
                        PNNotesOperations.ApplyShowScrollBars(_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar);
                    else
                    {
                        if (!_ParentWindow.TempSettings.GeneralSettings.AutoHeight)
                            PNNotesOperations.ApplyShowScrollBars(_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar);
                    }
                }
                // auto height
                if (PNStatic.Settings.GeneralSettings.AutoHeight != _ParentWindow.TempSettings.GeneralSettings.AutoHeight)
                {
                    // auto height after
                    if (_ParentWindow.TempSettings.GeneralSettings.AutoHeight)
                    {
                        // scroll bars after (and may be before)
                        if (_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar !=
                            System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // remove scroll bars
                            PNNotesOperations.ApplyShowScrollBars(System.Windows.Forms.RichTextBoxScrollBars.None);
                        }
                        // apply auto height
                        applyAutoHeight = true;
                    }
                    else
                    {
                        // scroll bars after (and may be before)
                        if (_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar !=
                            System.Windows.Forms.RichTextBoxScrollBars.None)
                        {
                            // restore scroll bars
                            PNNotesOperations.ApplyShowScrollBars(_ParentWindow.TempSettings.GeneralSettings.ShowScrollbar);
                        }
                    }
                }
                // buttons size
                if (PNStatic.Settings.GeneralSettings.ButtonsSize != _ParentWindow.TempSettings.GeneralSettings.ButtonsSize)
                {
                    PNNotesOperations.ApplyButtonsSize(_ParentWindow.TempSettings.GeneralSettings.ButtonsSize);
                }
                // custom fonts
                if (PNStatic.Settings.GeneralSettings.UseCustomFonts !=
                    _ParentWindow.TempSettings.GeneralSettings.UseCustomFonts)
                {
                    if (_ParentWindow.TempSettings.GeneralSettings.UseCustomFonts)
                    {
                        PNInterop.AddCustomFonts();
                    }
                    else
                    {
                        PNInterop.RemoveCustomFonts();
                    }
                }
                // margins
                if (PNStatic.Settings.GeneralSettings.MarginWidth != _ParentWindow.TempSettings.GeneralSettings.MarginWidth)
                {
                    if (!PNStatic.Settings.GeneralSettings.UseSkins)
                    {
                        PNNotesOperations.ApplyMarginsWidth(_ParentWindow.TempSettings.GeneralSettings.MarginWidth);
                    }
                }

                // spell check color
                if (PNStatic.Settings.GeneralSettings.SpellColor != _ParentWindow.TempSettings.GeneralSettings.SpellColor)
                {
                    if (Spellchecking.Initialized)
                    {
                        Spellchecking.ColorUnderlining = _ParentWindow.TempSettings.GeneralSettings.SpellColor;
                        PNNotesOperations.ApplySpellColor();
                    }
                }
                // autosave
                if (PNStatic.Settings.GeneralSettings.Autosave != _ParentWindow.TempSettings.GeneralSettings.Autosave)
                {
                    if (_ParentWindow.TempSettings.GeneralSettings.Autosave)
                    {
                        PNStatic.FormMain.TimerAutosave.Interval =
                            _ParentWindow.TempSettings.GeneralSettings.AutosavePeriod * 60000;
                        PNStatic.FormMain.TimerAutosave.Start();
                    }
                    else
                    {
                        PNStatic.FormMain.TimerAutosave.Stop();
                    }
                }
                else
                {
                    if (PNStatic.Settings.GeneralSettings.AutosavePeriod !=
                        _ParentWindow.TempSettings.GeneralSettings.AutosavePeriod)
                    {
                        if (PNStatic.Settings.GeneralSettings.Autosave)
                        {
                            PNStatic.FormMain.TimerAutosave.Stop();
                            PNStatic.FormMain.TimerAutosave.Interval =
                                _ParentWindow.TempSettings.GeneralSettings.AutosavePeriod * 60000;
                            PNStatic.FormMain.TimerAutosave.Start();
                        }
                    }
                }
                // clean bin
                if (PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod !=
                    _ParentWindow.TempSettings.GeneralSettings.RemoveFromBinPeriod)
                {
                    if (_ParentWindow.TempSettings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // stop timer
                        PNStatic.FormMain.TimerCleanBin.Stop();
                    }
                    else if (PNStatic.Settings.GeneralSettings.RemoveFromBinPeriod == 0)
                    {
                        // start timer
                        PNStatic.FormMain.TimerCleanBin.Start();
                    }
                }

                //create or delete shortcut
                string shortcutFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) +
                                      PNStrings.SHORTCUT_FILE;
                if (PNStatic.Settings.GeneralSettings.RunOnStart != _ParentWindow.TempSettings.GeneralSettings.RunOnStart)
                {
                    if (_ParentWindow.TempSettings.GeneralSettings.RunOnStart)
                    {
                        //create shortcut
                        if (!File.Exists(shortcutFile))
                        {
                            using (var link = new PNShellLink())
                            {
                                link.ShortcutFile = shortcutFile;
                                link.Target = System.Windows.Forms.Application.ExecutablePath;
                                link.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                                link.IconPath = System.Windows.Forms.Application.ExecutablePath;
                                link.IconIndex = 0;
                                link.Save();
                            }
                        }
                    }
                    else
                    {
                        //delete shortcut
                        if (File.Exists(shortcutFile))
                        {
                            File.Delete(shortcutFile);
                        }
                    }
                }
                PNStatic.Settings.GeneralSettings = (PNGeneralSettings)_ParentWindow.TempSettings.GeneralSettings.Clone();
                if (applyAutoHeight)
                {
                    PNNotesOperations.ApplyAutoHeight();
                }
                PNData.SaveGeneralSettings();
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void loadRemoveFromBinPeriods()
        {
            try
            {
                cboDeleteBin.Items.Clear();
                for (var i = 1; i <= 10; i++)
                {
                    cboDeleteBin.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }
                cboDeleteBin.Items.Add("20");
                cboDeleteBin.Items.Add("30");
                cboDeleteBin.Items.Add("60");
                cboDeleteBin.Items.Add("120");
                cboDeleteBin.Items.Add("360");
                cboDeleteBin.Items.Insert(0, PNLang.Instance.GetMiscText("never", "(Never)"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addLanguages()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.LangDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.LangDir);
                var files = di.GetFiles("*.xml").OrderBy(f => f.Name);
                foreach (var fi in files)
                {
                    var xd = XDocument.Load(fi.FullName, LoadOptions.PreserveWhitespace);
                    if (xd.Root == null) continue;
                    var xAttribute = xd.Root.Attribute("culture");
                    if (xAttribute == null) continue;
                    var ci = new CultureInfo(xAttribute.Value);
                    var name = ci.NativeName;
                    name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                    cboLanguage.Items.Add(new _Language { LangName = name, LangFile = fi.Name, Culture = ci.Name });
                }
                for (var i = 0; i < cboLanguage.Items.Count; i++)
                {
                    var ln = cboLanguage.Items[i] as _Language;
                    if (ln == null || ln.Culture != PNLang.Instance.GetLanguageCulture()) continue;
                    cboLanguage.SelectedIndex = i;
                    break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillTags()
        {
            try
            {
                lstTags.Items.Clear();
                foreach (var t in _Tags)
                    lstTags.Items.Add(t);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillExternals(bool firstTime = true)
        {
            try
            {
                _ExternalList.Clear();
                cmdEditExt.IsEnabled = cmdDeleteExt.IsEnabled = false;
                foreach (var ext in _Externals)
                {
                    _ExternalList.Add(new SExternal(ext.Name, ext.Program, ext.CommandLine));
                }
                if (firstTime)
                    grdExternals.ItemsSource = _ExternalList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void fillSearchProviders(bool firstTime = true)
        {
            try
            {
                _SearchProvidersList.Clear();
                cmdEditProv.IsEnabled = cmdDeleteProv.IsEnabled = false;
                foreach (var sp in _SProviders)
                {
                    _SearchProvidersList.Add(new SProvider(sp.Name, sp.QueryString));
                }
                if (firstTime)
                    grdSearchProvs.ItemsSource = _SearchProvidersList;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNSearchProvider getSelectedSearchProvider()
        {
            try
            {
                var sp = grdSearchProvs.SelectedItem as SProvider;
                return sp != null ? _SProviders.FirstOrDefault(p => p.Name == sp.Name) : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private PNExternal getSelectedExternal()
        {
            try
            {
                var ext = grdExternals.SelectedItem as SExternal;
                return ext != null ? _Externals.FirstOrDefault(e => e.Name == ext.Name) : null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void editSearchProvider(PNSearchProvider sp)
        {
            try
            {
                if (sp == null) return;
                var dsp = new WndSP(_ParentWindow, sp) { Owner = _ParentWindow };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void editExternal(PNExternal ext)
        {
            try
            {
                if (ext == null) return;
                var dex = new WndExternals(_ParentWindow, ext) { Owner = _ParentWindow };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboLanguage.SelectedIndex <= -1) return;
                var lang = cboLanguage.Items[cboLanguage.SelectedIndex] as _Language;
                if (lang != null) _ParentWindow.TempSettings.GeneralSettings.Language = lang.LangFile;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddLang_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_LANGS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdSetFontUI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fd = new WndFontChooser(PNSingleton.Instance.FontUser) { Owner = _ParentWindow };
                var showDialog = fd.ShowDialog();
                if (showDialog == null || !showDialog.Value) return;
                PNSingleton.Instance.FontUser.FontFamily = fd.SelectedFont.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = fd.SelectedFont.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = fd.SelectedFont.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = fd.SelectedFont.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = fd.SelectedFont.FontWeight;
                PNData.SaveFontUi();
                cmdRestoreFontUI.IsEnabled = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdRestoreFontUI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = new PNFont();
                PNSingleton.Instance.FontUser.FontFamily = f.FontFamily;
                PNSingleton.Instance.FontUser.FontSize = f.FontSize;
                PNSingleton.Instance.FontUser.FontStretch = f.FontStretch;
                PNSingleton.Instance.FontUser.FontStyle = f.FontStyle;
                PNSingleton.Instance.FontUser.FontWeight = f.FontWeight;
                PNData.SaveFontUi();
                cmdRestoreFontUI.IsEnabled = false;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckGeneral_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkRunOnStart":
                        _ParentWindow.TempSettings.GeneralSettings.RunOnStart = cb.IsChecked.Value;
                        break;
                    case "chkShowCPOnStart":
                        _ParentWindow.TempSettings.GeneralSettings.ShowCPOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckNewVersionOnStart":
                        _ParentWindow.TempSettings.GeneralSettings.CheckNewVersionOnStart = cb.IsChecked.Value;
                        break;
                    case "chkHideToolbar":
                        _ParentWindow.TempSettings.GeneralSettings.HideToolbar = cb.IsChecked.Value;
                        break;
                    case "chkCustomFont":
                        _ParentWindow.TempSettings.GeneralSettings.UseCustomFonts = cb.IsChecked.Value;
                        break;
                    case "chkHideDelete":
                        _ParentWindow.TempSettings.GeneralSettings.HideDeleteButton = cb.IsChecked.Value;
                        if (chkChangeHideToDelete.IsChecked != null &&
                            (!cb.IsChecked.Value && chkChangeHideToDelete.IsChecked.Value))
                        {
                            chkChangeHideToDelete.IsChecked = false;
                        }
                        break;
                    case "chkHideHide":
                        _ParentWindow.TempSettings.GeneralSettings.HideHideButton = cb.IsChecked.Value;
                        break;
                    case "chkChangeHideToDelete":
                        _ParentWindow.TempSettings.GeneralSettings.ChangeHideToDelete = cb.IsChecked.Value;
                        break;
                    case "chkAutosave":
                        _ParentWindow.TempSettings.GeneralSettings.Autosave = cb.IsChecked.Value;
                        break;
                    case "chkSaveOnExit":
                        _ParentWindow.TempSettings.GeneralSettings.SaveOnExit = cb.IsChecked.Value;
                        break;
                    case "chkConfirmSave":
                        _ParentWindow.TempSettings.GeneralSettings.ConfirmSaving = cb.IsChecked.Value;
                        break;
                    case "chkConfirmDelete":
                        _ParentWindow.TempSettings.GeneralSettings.ConfirmBeforeDeletion = cb.IsChecked.Value;
                        break;
                    case "chkSaveWithoutConfirm":
                        _ParentWindow.TempSettings.GeneralSettings.SaveWithoutConfirmOnHide = cb.IsChecked.Value;
                        break;
                    case "chkWarnBeforeEmptyBin":
                        _ParentWindow.TempSettings.GeneralSettings.WarnOnAutomaticalDelete = cb.IsChecked.Value;
                        break;
                    case "chkShowPriority":
                        _ParentWindow.TempSettings.GeneralSettings.ShowPriorityOnStart = cb.IsChecked.Value;
                        break;
                    case "chkAutomaticSmilies":
                        _ParentWindow.TempSettings.GeneralSettings.AutomaticSmilies = cb.IsChecked.Value;
                        break;
                    case "chkRestoreAutomatically":
                        _ParentWindow.TempSettings.GeneralSettings.RestoreAuto = cb.IsChecked.Value;
                        break;
                    case "chkAutoHeight":
                        _ParentWindow.TempSettings.GeneralSettings.AutoHeight = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalOnStart":
                        _ParentWindow.TempSettings.GeneralSettings.CheckCriticalOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCheckCriticalPeriodically":
                        _ParentWindow.TempSettings.GeneralSettings.CheckCriticalPeriodically = cb.IsChecked.Value;
                        break;
                    case "chkDeleteShortExit":
                        _ParentWindow.TempSettings.GeneralSettings.DeleteShortcutsOnExit = cb.IsChecked.Value;
                        break;
                    case "chkRestoreShortStart":
                        _ParentWindow.TempSettings.GeneralSettings.RestoreShortcutsOnStart = cb.IsChecked.Value;
                        break;
                    case "chkCloseOnShortcut":
                        _ParentWindow.TempSettings.GeneralSettings.CloseOnShortcut = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdNewVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var updater = new PNUpdateChecker();
                updater.NewVersionFound += updater_PNNewVersionFound;
                updater.IsLatestVersion += updater_PNIsLatestVersion;
                updater.CheckNewVersion(System.Windows.Forms.Application.ProductVersion);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNIsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_PNIsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("latest_version",
                    "You are using the latest version of PNotes.NET.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_PNNewVersionFound(object sender, NewVersionFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.NewVersionFound -= updater_PNNewVersionFound;
                }
                var message =
                    PNLang.Instance.GetMessageText("new_version_1",
                            "New version of PNotes.NET is available - %PLACEHOLDER1%.")
                        .Replace(PNStrings.PLACEHOLDER1, e.Version);
                message += "\n";
                message += PNLang.Instance.GetMessageText("new_version_2",
                    "Click 'OK' in order to instal new version (restart of program is required).");
                if (
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OKCancel,
                        MessageBoxImage.Information) !=
                    MessageBoxResult.OK) return;
                if (PNStatic.PrepareNewVersionCommandLine())
                {
                    PNStatic.FormMain.Close();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtWidthSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.GeneralSettings.Width = (int)txtWidthSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtHeightSknlsDef_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.GeneralSettings.Height = (int)txtHeightSknlsDef.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboButtonsSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded || cboButtonsSize.SelectedIndex <= -1) return;
                var bs = cboButtonsSize.Items[cboButtonsSize.SelectedIndex] as _ButtonSize;
                if (bs != null)
                {
                    _ParentWindow.TempSettings.GeneralSettings.ButtonsSize = bs.ButtonSize;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboIndent.SelectedIndex > -1 &&
                    (int)cboIndent.SelectedItem != _ParentWindow.TempSettings.GeneralSettings.BulletsIndent)
                {
                    _ParentWindow.TempSettings.GeneralSettings.BulletsIndent = Convert.ToInt16(cboIndent.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboMargins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboMargins.SelectedIndex > -1 &&
                    (int)cboMargins.SelectedItem != _ParentWindow.TempSettings.GeneralSettings.MarginWidth)
                {
                    _ParentWindow.TempSettings.GeneralSettings.MarginWidth = Convert.ToInt16(cboMargins.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboParIndent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboParIndent.SelectedIndex > -1 &&
                    (int)cboParIndent.SelectedItem != _ParentWindow.TempSettings.GeneralSettings.ParagraphIndent)
                {
                    _ParentWindow.TempSettings.GeneralSettings.ParagraphIndent = (int)cboParIndent.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void pckSpell_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                if (_Loaded)
                {
                    _ParentWindow.TempSettings.GeneralSettings.SpellColor = System.Drawing.Color.FromArgb(pckSpell.SelectedColor.A,
                        pckSpell.SelectedColor.R, pckSpell.SelectedColor.G, pckSpell.SelectedColor.B);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updSpace_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _ParentWindow.TempSettings.GeneralSettings.SpacePoints = Convert.ToInt32(updSpace.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updAutosave_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (_Loaded)
                {
                    _ParentWindow.TempSettings.GeneralSettings.AutosavePeriod = Convert.ToInt32(updAutosave.Value);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDeleteBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                if (cboDeleteBin.SelectedIndex > -1)
                {
                    _ParentWindow.TempSettings.GeneralSettings.RemoveFromBinPeriod = cboDeleteBin.SelectedIndex == 0
                        ? 0
                        : Convert.ToInt32(cboDeleteBin.SelectedItem);
                }
                chkWarnBeforeEmptyBin.IsEnabled = cboDeleteBin.SelectedIndex > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtDTFShort.Text.Trim().Length == 0) return;
                var format = txtDTFShort.Text.Trim().Length > 1
                    ? txtDTFShort.Text.Trim().Replace("/", "'/'")
                    : "%" + txtDTFShort.Text.Trim().Replace("/", "'/'");
                _ParentWindow.TempSettings.GeneralSettings.DateFormat = format;
                lblDTShort.Text = DateTime.Now.ToString(_ParentWindow.TempSettings.GeneralSettings.DateFormat);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDTFShort_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.D:
                    case Key.Y:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                        break;
                    case Key.Oem2:
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.M:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDTFShort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNMessageBox.Show(PNLang.Instance.GetDateFormatsText(),
                    PNLang.Instance.GetCaptionText("date_formats", "Possible date formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtTFLong.Text.Trim().Length == 0) return;
                var format = txtTFLong.Text.Trim().Length > 1 ? txtTFLong.Text.Trim() : "%" + txtTFLong.Text.Trim();
                _ParentWindow.TempSettings.GeneralSettings.TimeFormat = format;
                lblTFLong.Text = DateTime.Now.ToString(_ParentWindow.TempSettings.GeneralSettings.TimeFormat);
            }
            catch (FormatException)
            {
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTFLong_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.M:
                    case Key.S:
                    case Key.F:
                    case Key.T:
                    case Key.Space:
                    case Key.Subtract:
                    case Key.Divide:
                    case Key.Decimal:
                    case Key.Delete:
                    case Key.Back:
                    case Key.Left:
                    case Key.Right:
                    case Key.H:
                        break;
                    case Key.OemPeriod:
                    case Key.OemMinus:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    case Key.Oem1:
                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                            e.Handled = true;
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdTFLong_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PNMessageBox.Show(PNLang.Instance.GetTimeFormatsText(),
                    PNLang.Instance.GetCaptionText("time_formats", "Possible time formats"));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSearchProvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditProv.IsEnabled = cmdDeleteProv.IsEnabled = grdSearchProvs.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdSearchProvs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdSearchProvs.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdSearchProvs)) as SProvider;
                if (item == null) return;
                editSearchProvider(_SProviders.FirstOrDefault(p => p.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdExternals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdEditExt.IsEnabled = cmdDeleteExt.IsEnabled = grdExternals.SelectedItems.Count > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void grdExternals_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = grdExternals.GetObjectAtPoint<ListViewItem>(e.GetPosition(grdExternals)) as SExternal;
                if (item == null) return;
                editExternal(_Externals.FirstOrDefault(ext => ext.Name == item.Name));
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                cmdAddTag.IsEnabled = txtTag.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cmdDeleteTag.IsEnabled = lstTags.SelectedIndex >= 0;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboScrollBars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.GeneralSettings.ShowScrollbar =
                    (System.Windows.Forms.RichTextBoxScrollBars)cboScrollBars.SelectedIndex;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dsp = new WndSP(_ParentWindow) { Owner = _ParentWindow };
                var showDialog = dsp.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillSearchProviders(false);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editSearchProvider(getSelectedSearchProvider());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteProv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = getSelectedSearchProvider();
                if (sp == null) return;
                var message = PNLang.Instance.GetMessageText("sp_delete", "Delete selected serach provider?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _SProviders.Remove(sp);
                fillSearchProviders(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dex = new WndExternals(_ParentWindow) { Owner = _ParentWindow };
                var showDialog = dex.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    fillExternals();
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdEditExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editExternal(getSelectedExternal());
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteExt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ext = getSelectedExternal();
                if (ext == null) return;
                var message = PNLang.Instance.GetMessageText("ext_delete", "Delete selected external program?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Externals.Remove(ext);
                fillExternals(false);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Tags.Contains(txtTag.Text.Trim()))
                {
                    string message = PNLang.Instance.GetMessageText("tag_exists", "The same tag already exists");
                    PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTag.SelectAll();
                    txtTag.Focus();
                }
                else
                {
                    lstTags.Items.Add(txtTag.Text.Trim());
                    _Tags.Add(txtTag.Text.Trim());
                    txtTag.Text = "";
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstTags.SelectedIndex < 0) return;
                var message = PNLang.Instance.GetMessageText("tag_delete", "Delete selected tag?");
                if (PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes) return;
                _Tags.Remove((string)lstTags.SelectedItem);
                lstTags.Items.Remove(lstTags.SelectedItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        #endregion
    }
}

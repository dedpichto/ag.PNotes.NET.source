using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using PNStaticFonts;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for UcAppearance.xaml
    /// </summary>
    public partial class UcAppearance : ISettingsPage
    {
        public UcAppearance()
        {
            InitializeComponent();
        }

        private bool _Loaded;
        private WndSettings _ParentWindow;
        private ChangesAction _ChangesAction;

        public void Init(WndSettings setings)
        {
            _ParentWindow = setings;
            _TempDocking = (PNGroup)PNStatic.Docking.Clone();
        }

        public void InitPage(bool firstTime)
        {
            initPageAppearance(firstTime);
        }

        public ChangesAction DefineChangesAction()
        {
            var result = ChangesAction.None;
            if (PNStatic.Settings.GeneralSettings.UseSkins != _ParentWindow.TempSettings.GeneralSettings.UseSkins)
            {
                result |= ChangesAction.SkinsReload;
            }
            _ChangesAction = result;
            return result;
        }

        public bool SavePage()
        {
            if (PNStatic.Settings.GeneralSettings.UseSkins != _ParentWindow.TempSettings.GeneralSettings.UseSkins)
            {
                if (!PNStatic.Settings.GeneralSettings.UseSkins)
                {
                    if (Directory.Exists(PNPaths.Instance.SkinsDir))
                    {
                        var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                        var fi = di.GetFiles("*.pnskn");
                        if (fi.Length > 0)
                        {
                            foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
                            {
                                setDefaultSkin(n, fi[0].FullName);
                            }
                        }
                    }
                }
            }
            var changedSkins = new List<int>();
            foreach (var treeItem in tvwGroups.Items.OfType<PNTreeItem>())
            {
                checkAndApplyGroupChanges(treeItem, changedSkins);
            }

            if ((_ChangesAction & ChangesAction.SkinsReload) != ChangesAction.SkinsReload)
            {
                if (PNStatic.Settings.GeneralSettings.UseSkins && changedSkins.Count > 0)
                {
                    // change skins for notes if we don't have to reload them
                    var notes = PNStatic.Notes.Where(n => n.Visible);
                    foreach (var n in notes)
                    {
                        if (n.Dialog != null && n.Skin == null)
                        {
                            PNSkinsOperations.ApplyNoteSkin(n.Dialog, n);
                        }
                    }
                }
            }

            _ChangedGroups.Clear();
            return true;
        }

        public bool SaveCollections()
        {
            return true;
        }

        public bool CanExecute()
        {
            return _ChangedGroups.Any();
        }

        public void RestoreDefaultValues()
        {
            if (_ParentWindow.chkDefSettIncGroups.IsChecked != null && _ParentWindow.chkDefSettIncGroups.IsChecked.Value)
            {
                foreach (var treeItem in tvwGroups.Items.OfType<PNTreeItem>())
                    restoreAllGroupsDelaults(treeItem);
            }
        }

        public bool InDefClick { get; set; }

        public event EventHandler PromptToRestart;

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var n in tvwGroups.Items.OfType<PNTreeItem>())
            {
                cleanUpGroups(n);
            }
        }

        #region Appearance staff
        private PNGroup _TempDocking;
        private readonly List<int> _ChangedGroups = new List<int>();
        private bool _InTreeGroupsSelectionChanged;


        internal void GroupAddEdit(PNGroup group, int id, AddEditMode mode)
        {
            try
            {
                var treeItem = getTreeItemByGroupId(id, null);
                switch (mode)
                {
                    case AddEditMode.Add:
                        insertGroupToTree(group, treeItem);
                        break;
                    case AddEditMode.Edit:
                        if (treeItem == null) return;
                        replaceGroupOnTree(group, treeItem);
                        if (treeItem.IsSelected)
                            groupSelected(group);
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal void GroupDelete(int id)
        {
            try
            {
                var treeItem = getTreeItemByGroupId(id, null);
                if (treeItem == null) return;
                if (treeItem.ItemParent == null)
                    tvwGroups.Items.Remove(treeItem);
                else
                    treeItem.ItemParent.Items.Remove(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void initPageAppearance(bool firstTime)
        {
            try
            {
                if (!PNStatic.CheckSkinsExistance()) optSkinnable.IsEnabled = false;
                if (_ParentWindow.TempSettings.GeneralSettings.UseSkins)
                {
                    optSkinnable.IsChecked = true;
                }
                else
                {
                    optSkinless.IsChecked = true;
                }
                txtDockWidth.Value = _ParentWindow.TempSettings.GeneralSettings.DockWidth;
                txtDockHeight.Value = _ParentWindow.TempSettings.GeneralSettings.DockHeight;
                chkAddWeekdayName.IsChecked = _ParentWindow.TempSettings.Diary.AddWeekday;
                chkFullWeekdayName.IsChecked = _ParentWindow.TempSettings.Diary.FullWeekdayName;
                chkWeekdayAtTheEnd.IsChecked = _ParentWindow.TempSettings.Diary.WeekdayAtTheEnd;
                chkNoPreviousDiary.IsChecked = _ParentWindow.TempSettings.Diary.DoNotShowPrevious;
                chkDiaryAscOrder.IsChecked = _ParentWindow.TempSettings.Diary.AscendingOrder;

                cboNumberOfDiaries.SelectedItem = _ParentWindow.TempSettings.Diary.NumberOfPages;
                cboDiaryNaming.SelectedItem = _ParentWindow.TempSettings.Diary.DateFormat;
                if (firstTime)
                {
                    //_TempGroups = new List<PNGroup>();
                    //foreach (var group in PNStatic.Groups)
                    //{
                    //    addGroupToTemp(group, null);
                    //}
                    //var gd = PNStatic.Groups.GetGroupByID(Convert.ToInt32(SpecialGroups.Diary));
                    //if (gd != null)
                    //{
                    //    addGroupToTemp(gd, null);
                    //}
                    //addGroupToTemp(_TempDocking, null);

                    //foreach (var g in _TempGroups[0].Subgroups.OrderBy(gr => gr.Name))
                    //{
                    //    addGroupToTree(g, null);
                    //}

                    foreach (var g in PNStatic.Groups[0].Subgroups.OrderBy(gr => gr.Name))
                    {
                        addGroupToTree(g, null);
                    }
                    var gd = PNStatic.Groups.GetGroupByID(Convert.ToInt32(SpecialGroups.Diary));
                    if (gd != null)
                    {
                        addGroupToTree(gd, null);
                    }
                    addGroupToTree(_TempDocking, null);
                    loadSkinsList();
                    loadLogFonts();
                    lstThemes.ItemsSource = PNStatic.Themes.Keys;
                }
                ((TreeViewItem)tvwGroups.Items[0]).IsSelected = true;
                if (PNStatic.Themes.Keys.Contains(_ParentWindow.TempSettings.Behavior.Theme))
                {
                    lstThemes.SelectedItem =
                        lstThemes.Items.OfType<string>().FirstOrDefault(s => s == _ParentWindow.TempSettings.Behavior.Theme);
                }
                else
                {
                    lstThemes.SelectedIndex = 0;
                }
                if (firstTime)
                {
                    cboFontColor.IsDropDownOpen = true;
                    cboFontColor.IsDropDownOpen = false;
                    cboFontSize.IsDropDownOpen = true;
                    cboFontSize.IsDropDownOpen = false;
                    _Loaded = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void restoreAllGroupsDelaults(PNTreeItem treeItem)
        {
            try
            {
                foreach (var ti in treeItem.Items.OfType<PNTreeItem>())
                    restoreAllGroupsDelaults(ti);
                restoreGroupDefaults(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkAndApplyGroupChanges(PNTreeItem node, List<int> changedSkins)
        {
            try
            {
                var isIconChanged = false;
                var gr = node.Tag as PNGroup;
                if (gr == null) return;
                var rg = PNStatic.Groups.GetGroupByID(gr.ID);
                if (rg != null)
                {
                    if (gr != rg)
                    {
                        if (!Equals(gr.Image, rg.Image))
                            isIconChanged = true;
                        if (gr.Skin.SkinName != rg.Skin.SkinName)
                        {
                            changedSkins.Add(gr.ID);
                        }
                        gr.CopyTo(rg);
                        PNData.SaveGroupChanges(rg);
                        if (isIconChanged && PNStatic.FormCP != null)
                            PNStatic.FormCP.GroupIconChanged(rg.ID);
                    }
                    foreach (var n in node.Items.OfType<PNTreeItem>())
                    {
                        checkAndApplyGroupChanges(n, changedSkins);
                    }
                }
                else
                {
                    if (gr.ID != (int)SpecialGroups.Docking) return;
                    if (gr == PNStatic.Docking) return;
                    changedSkins.Add((int)SpecialGroups.Docking);
                    gr.CopyTo(PNStatic.Docking);
                    PNData.SaveGroupChanges(PNStatic.Docking);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void setDefaultSkin(PNTreeItem node, string path)
        {
            try
            {
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    setDefaultSkin(n, path);
                }
                var gr = node.Tag as PNGroup;
                if (gr == null || gr.Skin.SkinName != PNSkinDetails.NO_SKIN) return;
                gr.Skin.SkinName = Path.GetFileNameWithoutExtension(path);
                PNSkinsOperations.LoadSkin(path, gr.Skin);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void insertGroupToTree(PNGroup group, ItemsControl parentItem)
        {
            try
            {
                var temp = (PNGroup)group.Clone();
                group.CopyTo(temp);

                var n = new PNTreeItem(temp.Image, temp.Name, temp);

                var inserted = false;
                if (parentItem == null) parentItem = tvwGroups;
                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    var pni = parentItem.Items[i] as PNTreeItem;
                    if (pni == null) continue;
                    var gr = pni.Tag as PNGroup;
                    if (gr == null) continue;
                    if (string.Compare(gr.Name, group.Name, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        parentItem.Items.Insert(i, n);
                        inserted = true;
                        break;
                    }
                    if (i < parentItem.Items.Count - 1)
                    {
                        pni = parentItem.Items[i + 1] as PNTreeItem;
                        if (pni == null) continue;
                        gr = pni.Tag as PNGroup;
                        if (gr == null) continue;
                        if (gr.ID < (int)SpecialGroups.AllGroups)
                        {
                            parentItem.Items.Insert(i + 1, n);
                            inserted = true;
                            break;
                        }
                    }
                }
                if (!inserted)
                {
                    parentItem.Items.Add(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void replaceGroupOnTree(PNGroup group, PNTreeItem node)
        {
            try
            {
                var temp = (PNGroup)group.Clone();
                group.CopyTo(temp);
                node.Image = temp.Image;
                node.Text = temp.Name;
                node.Tag = temp;
                ItemsControl parentItem;
                if (node.ItemParent != null)
                    parentItem = node.ItemParent;
                else
                    parentItem = tvwGroups;

                var isSelected = node.IsSelected;

                parentItem.Items.Remove(node);
                for (var i = 0; i < parentItem.Items.Count; i++)
                {
                    var ti = parentItem.Items[i] as PNTreeItem;
                    if (ti == null) continue;
                    var gr = ti.Tag as PNGroup;
                    if (gr == null) continue;
                    if (gr.ID < (int)SpecialGroups.AllGroups)
                    {
                        parentItem.Items.Insert(i, node);
                        if (isSelected)
                            node.IsSelected = true;
                        return;
                    }
                    if (string.Compare(gr.Name, node.Text, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        parentItem.Items.Insert(i, node);
                        if (isSelected)
                            node.IsSelected = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNTreeItem getTreeItemByGroupId(int groupId, ItemsControl root)
        {
            try
            {
                if (root == null) root = tvwGroups;
                PNTreeItem result = null;
                foreach (var treeItem in root.Items.OfType<PNTreeItem>())
                {
                    var group = treeItem.Tag as PNGroup;
                    if (group == null) continue;
                    if (group.ID == groupId)
                    {
                        result = treeItem;
                        break;
                    }
                    result = getTreeItemByGroupId(groupId, treeItem);
                    if (result != null)
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void restoreGroupDefaults(PNTreeItem treeItem)
        {
            try
            {
                var pnGroup = treeItem.Tag as PNGroup;
                if (pnGroup == null) return;
                var gr = (PNGroup)pnGroup.Clone();
                pnGroup.Clear();
                pnGroup.Name = gr.ID == 0 ? PNLang.Instance.GetGroupName("general", "General") : gr.Name;
                pnGroup.ID = gr.ID;
                pnGroup.ParentID = gr.ParentID;
                var image = TryFindResource("gr") as BitmapImage;
                pnGroup.Image = image;
                pnGroup.IsDefaultImage = true;
                if (treeItem.IsSelected)
                    groupSelected(pnGroup);

                if (!Equals(treeItem.Image, pnGroup.Image))
                {
                    if (!pnGroup.IsDefaultImage)
                        treeItem.Image = pnGroup.Image;
                    else
                        treeItem.SetImageResource(pnGroup.ImageName);
                }

                compareGroups(pnGroup);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private PNGroup selectedGroup()
        {
            try
            {
                var item = tvwGroups.SelectedItem as PNTreeItem;
                if (item != null)
                    return item.Tag as PNGroup;
                return null;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        private void loadSkinsList()
        {
            try
            {
                if (!Directory.Exists(PNPaths.Instance.SkinsDir)) return;
                var di = new DirectoryInfo(PNPaths.Instance.SkinsDir);
                var fi = di.GetFiles("*.pnskn");
                var image = TryFindResource("skins") as BitmapImage;
                //new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "skins.png"));
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

        private void groupSelected(PNGroup gr)
        {
            try
            {
                pvwSkl.SetProperties(gr);
                pckBGSknls.SelectedColor = gr.Skinless.BackColor;
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    lstSkins.SelectedItem =
                        lstSkins.Items.OfType<PNListBoxItem>().FirstOrDefault(it => it.Text == gr.Skin.SkinName);
                }
                else
                {
                    lstSkins.SelectedIndex = -1;
                }
                cboFonts.SelectedItem =
                    cboFonts.Items.OfType<LOGFONT>().FirstOrDefault(lf => lf.lfFaceName == gr.Font.lfFaceName);
                foreach (var t in from object t in cboFontColor.Items
                                  let rc = t as Rectangle
                                  where rc != null
                                  let sb = rc.Fill as SolidColorBrush
                                  where sb != null
                                  where
                                  sb.Color ==
                                  Color.FromArgb(gr.FontColor.A, gr.FontColor.R, gr.FontColor.G,
                                      gr.FontColor.B)
                                  select t)
                {
                    cboFontColor.SelectedItem = t;
                    break;
                }
                var fontSize = gr.Font.GetFontSize();
                if (cboFontSize.Items.OfType<int>().Any(i => i == fontSize))
                    cboFontSize.SelectedItem = fontSize;
                else
                    cboFontSize.SelectedIndex = 0;
                imgGroupIcon.Source = gr.Image;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void addGroupToTree(PNGroup group, PNTreeItem node)
        {
            try
            {
                var temp = (PNGroup)group.Clone();
                group.CopyTo(temp);

                var n = new PNTreeItem(temp.Image, temp.Name, temp);
                if (node == null)
                {
                    tvwGroups.Items.Add(n);
                }
                else
                {
                    node.Items.Add(n);
                }
                foreach (var g in group.Subgroups.OrderBy(gr => gr.Name))
                {
                    addGroupToTree(g, n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void OptionAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (optSkinnable.IsChecked != null)
                    _ParentWindow.TempSettings.GeneralSettings.UseSkins = optSkinnable.IsChecked.Value;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                _InTreeGroupsSelectionChanged = true;
                var item = e.NewValue as PNTreeItem;
                if (item == null)
                    return;
                var gr = item.Tag as PNGroup;
                if (gr == null)
                    return;
                switch (gr.ID)
                {
                    case (int)SpecialGroups.Diary:
                        pnsDiaryCust.IsEnabled = true;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = true;
                        //stkDockCust.IsEnabled = false;
                        break;
                    case (int)SpecialGroups.Docking:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = true;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = true;
                        break;
                    default:
                        pnsDiaryCust.IsEnabled = false;
                        pnsMiscDocking.IsEnabled = false;
                        //stkDiaryCust.IsEnabled = false;
                        //stkDockCust.IsEnabled = false;
                        break;
                }
                groupSelected(gr);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                _InTreeGroupsSelectionChanged = false;
            }
        }

        private void pckBGSknls_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                gr.Skinless.BackColor = e.NewValue;
                pvwSkl.SetProperties(gr);
                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdFontSknls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gr = selectedGroup();
                if (gr == null) return;
                var fc = new WndFontChooser(gr.Skinless.CaptionFont, gr.Skinless.CaptionColor) { Owner = _ParentWindow };
                var showDialog = fc.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    gr.Skinless.CaptionFont = fc.SelectedFont;
                    gr.Skinless.CaptionColor = fc.SelectedColor;
                    pvwSkl.SetProperties(gr);
                }
                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
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
                var item = lstSkins.SelectedItem as PNListBoxItem;
                if (item == null) return;
                var gr = selectedGroup();
                if (gr == null) return;
                if (gr.Skin.SkinName != item.Text)
                {
                    gr.Skin.SkinName = item.Text;
                    var path = Path.Combine(PNPaths.Instance.SkinsDir, gr.Skin.SkinName + PNStrings.SKIN_EXTENSION);
                    if (File.Exists(path))
                    {
                        PNSkinsOperations.LoadSkin(path, gr.Skin);
                    }
                }
                if (gr.Skin.SkinName != PNSkinDetails.NO_SKIN)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lblMoreSkins_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PNStatic.LoadPage(PNStrings.URL_SKINS);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CheckAppearance_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (cb == null || cb.IsChecked == null) return;
                switch (cb.Name)
                {
                    case "chkAddWeekdayName":
                        _ParentWindow.TempSettings.Diary.AddWeekday = cb.IsChecked.Value;
                        break;
                    case "chkFullWeekdayName":
                        _ParentWindow.TempSettings.Diary.FullWeekdayName = cb.IsChecked.Value;
                        break;
                    case "chkWeekdayAtTheEnd":
                        _ParentWindow.TempSettings.Diary.WeekdayAtTheEnd = cb.IsChecked.Value;
                        break;
                    case "chkNoPreviousDiary":
                        _ParentWindow.TempSettings.Diary.DoNotShowPrevious = cb.IsChecked.Value;
                        break;
                    case "chkDiaryAscOrder":
                        _ParentWindow.TempSettings.Diary.AscendingOrder = cb.IsChecked.Value;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void compareGroups(PNGroup gr)
        {
            try
            {
                var gc = PNStatic.Groups.GetGroupByID(gr.ID);
                if (gc == null) return;
                var equals = gc == gr;
                if (!equals && _ChangedGroups.All(i => i != gr.ID))
                    _ChangedGroups.Add(gr.ID);
                else if (equals && _ChangedGroups.Any(i => i == gr.ID))
                    _ChangedGroups.Remove(gr.ID);
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
                var gr = selectedGroup();
                if (gr == null) return;

                //track
                if (!_InTreeGroupsSelectionChanged)
                {
                    var logF = new LOGFONT();
                    logF.Init();
                    logF.SetFontFace(lf.lfFaceName);
                    logF.SetFontSize((int)cboFontSize.SelectedItem);
                    gr.Font = logF;
                }

                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
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
                var gr = selectedGroup();
                if (gr == null) return;
                var rc = cboFontColor.SelectedItem as Rectangle;
                if (rc == null) return;
                var sb = rc.Fill as SolidColorBrush;
                if (sb == null) return;
                gr.FontColor = System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
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
                var gr = selectedGroup();
                if (gr == null) return;

                //track
                if (!_InTreeGroupsSelectionChanged)
                {
                    var logF = new LOGFONT();
                    logF.Init();
                    logF.SetFontFace(gr.Font.lfFaceName);
                    logF.SetFontSize((int)cboFontSize.SelectedItem);
                    gr.Font = logF;
                }

                pvwSkl.SetProperties(gr);
                if (lstSkins.SelectedIndex >= 0)
                {
                    var w = brdSkin.ActualWidth > 0 ? brdSkin.ActualWidth : brdSkin.Width;
                    var h = brdSkin.ActualHeight > 0 ? brdSkin.ActualHeight : brdSkin.Height;
                    pvwSkin.SetProperties(gr, gr.Skin, w - pvwSkin.Margin.Left - pvwSkin.Margin.Right,
                        h - pvwSkin.Margin.Top - pvwSkin.Margin.Bottom);
                }

                if (!_InTreeGroupsSelectionChanged)
                {
                    compareGroups(gr);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboNumberOfDiaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboNumberOfDiaries.SelectedIndex > -1)
                {
                    _ParentWindow.TempSettings.Diary.NumberOfPages = (int)cboNumberOfDiaries.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cboDiaryNaming_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboDiaryNaming.SelectedIndex > -1)
                {
                    _ParentWindow.TempSettings.Diary.DateFormat = (string)cboDiaryNaming.SelectedItem;
                    lblDiaryExample.Text = DateTime.Today.ToString(_ParentWindow.TempSettings.Diary.DateFormat.Replace("/", "'/'"));
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.GeneralSettings.DockWidth = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void txtDockHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            try
            {
                if (!_Loaded) return;
                _ParentWindow.TempSettings.GeneralSettings.DockHeight = Convert.ToInt32(e.NewValue);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdStandard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var treeItem = tvwGroups.SelectedItem as PNTreeItem;
                if (treeItem == null) return;
                restoreGroupDefaults(treeItem);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdChangeIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlgIcons = new WndFolderIcons { Owner = _ParentWindow };
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

        private void dlgIcons_GroupPropertyChanged(object sender, GroupPropertyChangedEventArgs e)
        {
            try
            {
                var d = sender as WndFolderIcons;
                if (d != null) d.GroupPropertyChanged -= dlgIcons_GroupPropertyChanged;
                var treeItem = tvwGroups.SelectedItem as PNTreeItem;
                if (treeItem == null) return;
                var pnGroup = treeItem.Tag as PNGroup;
                if (pnGroup == null) return;
                imgGroupIcon.Source = (BitmapImage)e.NewStateObject;
                pnGroup.Image = (BitmapImage)e.NewStateObject;
                pnGroup.IsDefaultImage = false;
                if (!Equals(treeItem.Image, pnGroup.Image))
                {
                    if (!pnGroup.IsDefaultImage)
                        treeItem.Image = pnGroup.Image;
                    else
                        treeItem.SetImageResource(pnGroup.ImageName);
                }
                compareGroups(pnGroup);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var key = lstThemes.SelectedItem as string;
                if (key == null) return;
                var de = PNStatic.Themes[key];
                if (de == null) return;
                imgTheme.Source = de.Item3;
                if (!_Loaded) return;
                if (lstThemes.SelectedIndex >= 0)
                {
                    _ParentWindow.TempSettings.Behavior.Theme = (string)lstThemes.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cmdAddTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                checkThemesUpdate();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void checkThemesUpdate()
        {
            try
            {
                if (PNSingleton.Instance.PluginsDownload || PNSingleton.Instance.PluginsChecking ||
                    PNSingleton.Instance.VersionChecking || PNSingleton.Instance.CriticalChecking ||
                    PNSingleton.Instance.ThemesDownload || PNSingleton.Instance.ThemesChecking) return;
                var updater = new PNUpdateChecker();
                updater.ThemesUpdateFound += updater_ThemesUpdateFound;
                updater.IsLatestVersion += updaterThemes_IsLatestVersion;
                Mouse.OverrideCursor = Cursors.Wait;
                updater.CheckThemesNewVersion();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void updaterThemes_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                Mouse.OverrideCursor = null;
                var message = PNLang.Instance.GetMessageText("themes_latest_version",
                    "All themes are up-to-date.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_IsLatestVersion(object sender, EventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null)
                {
                    updater.IsLatestVersion -= updater_IsLatestVersion;
                }
                var message = PNLang.Instance.GetMessageText("plugins_latest_version",
                    "All plugins are up-to-date.");
                PNMessageBox.Show(message, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void updater_ThemesUpdateFound(object sender, ThemesUpdateFoundEventArgs e)
        {
            try
            {
                var updater = sender as PNUpdateChecker;
                if (updater != null) updater.ThemesUpdateFound -= updater_ThemesUpdateFound;
                Mouse.OverrideCursor = null;
                var d = new WndGetThemes(e.ThemesList) { Owner = _ParentWindow };
                var showDialog = d.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    if (PromptToRestart != null) PromptToRestart(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void cleanUpGroups(PNTreeItem node)
        {
            try
            {
                var group = node.Tag as PNGroup;
                if (group != null)
                {
                    group.Dispose();
                }
                foreach (var n in node.Items.OfType<PNTreeItem>())
                {
                    cleanUpGroups(n);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
        #endregion
    }
}

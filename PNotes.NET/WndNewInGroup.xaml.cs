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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndNewInGroup.xaml
    /// </summary>
    public partial class WndNewInGroup
    {
        public WndNewInGroup()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal event EventHandler<NoteGroupChangedEventArgs> NoteGroupChanged;

        private readonly List<PNTreeItem> _Items = new List<PNTreeItem>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                foreach (var g in PNCollections.Instance.Groups[0].Subgroups)
                {
                    loadGroup(g, null);
                }
                tvwGroups.ItemsSource = _Items;
                if (tvwGroups.Items.Count > 0)
                    ((TreeViewItem)tvwGroups.Items[0]).IsSelected = true;
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void oKClick()
        {
            try
            {
                if (!(tvwGroups.SelectedItem is PNTreeItem item)) return;
                var gr = item.Tag as PNGroup;
                if (gr == null) return;
                NoteGroupChanged?.Invoke(this, new NoteGroupChangedEventArgs(gr.Id, (int)SpecialGroups.DummyGroup));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void tvwGroups_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(tvwGroups.SelectedItem is PNTreeItem)) return;
                oKClick();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void loadGroup(PNGroup pgroup, PNTreeItem item)
        {
            try
            {
                var ti = new PNTreeItem(pgroup.Image, pgroup.Name, pgroup) {IsExpanded = true};
                foreach (var sg in pgroup.Subgroups)
                {
                    loadGroup(sg, ti);
                }
                if (item == null)
                    _Items.Add(ti);
                else
                    item.Items.Add(ti);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (!(e.Command is PNRoutedUICommand command)) return;
                switch (command.Type)
                {
                    case CommandType.Ok:
                        e.CanExecute = tvwGroups.SelectedItem != null;
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
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
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}

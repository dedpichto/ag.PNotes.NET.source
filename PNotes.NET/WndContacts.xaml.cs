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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndContacts.xaml
    /// </summary>
    public partial class WndContacts
    {
        private class GroupsSorter : IComparer<PNContactGroup>
        {
            public int Compare(PNContactGroup x, PNContactGroup y)
            {
                if (x == null || y == null) return 0;
                return string.CompareOrdinal(x.Name, y.Name);
            }
        }

        internal event EventHandler<ContactChangedEventArgs> ContactChanged;

        public WndContacts()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndContacts(int newId, List<PNContactGroup> groups)
            : this()
        {
            _Groups = new List<PNContactGroup>(groups.Select(g => g.Clone()));
            _Id = newId;
            _Mode = AddEditMode.Add;
        }

        internal WndContacts(PNContact cn, List<PNContactGroup> groups)
            : this()
        {
            _Groups = new List<PNContactGroup>(groups.Select(g => g.Clone()));
            _Mode = AddEditMode.Edit;
            _Contact = cn;
        }

        private readonly AddEditMode _Mode;
        private PNContact _Contact;
        private readonly List<PNContactGroup> _Groups;
        private readonly int _Id;
        private List<string> _CompsList = new List<string>();

        private void DlgContacts_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                PNLang.Instance.ApplyControlLanguage(this);

                _Groups.Sort(new GroupsSorter());
                cboGroups.Items.Add(PNLang.Instance.GetCaptionText("no_cont_group", PNStrings.NO_GROUP));
                foreach (var g in _Groups)
                {
                    cboGroups.Items.Add(g.Name);
                }

                if (_Mode == AddEditMode.Add)
                {
                    Title = PNLang.Instance.GetCaptionText("contact_new", "New contact");
                    cboGroups.SelectedIndex = 0;
                    optUseCompName.IsChecked = true;
                }
                else
                {
                    Title = PNLang.Instance.GetCaptionText("contact_edit", "Edit contact");
                    txtContactName.Text = _Contact.Name;
                    txtCompName.Text = _Contact.ComputerName;
                    optUseCompName.IsChecked = _Contact.UseComputerName;
                    optUseAddress.IsChecked = !_Contact.UseComputerName;
                    if (_Contact.IpAddress != "")
                    {
                        ipaAddress.SetAddressBytes(IPAddress.Parse(_Contact.IpAddress).GetAddressBytes());
                    }
                    if (_Contact.GroupId == -1)
                    {
                        cboGroups.SelectedIndex = 0;
                    }
                    else
                    {
                        var gr = _Groups.FirstOrDefault(g => g.Id == _Contact.GroupId);
                        cboGroups.SelectedIndex = gr != null ? cboGroups.Items.IndexOf(gr.Name) : 0;
                    }
                }
                txtContactName.SelectAll();
                txtContactName.Focus();
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool enableOk()
        {
            try
            {
                if (txtContactName.Text.Trim().Length == 0)
                {
                    return false;
                }
                if (optUseCompName.IsChecked != null && optUseCompName.IsChecked.Value &&
                    txtCompName.Text.Trim().Length == 0)
                {
                    return false;
                }
                if (optUseAddress.IsChecked != null && optUseAddress.IsChecked.Value && ipaAddress.IsAnyBlank)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void oKClick()
        {
            try
            {
                if (_Mode == AddEditMode.Add)
                {
                    _Contact = new PNContact { Id = _Id };
                }
                _Contact.Name = txtContactName.Text.Trim();
                if (optUseCompName.IsChecked != null)
                    _Contact.UseComputerName = optUseCompName.IsChecked.Value;
                _Contact.ComputerName = txtCompName.Text.Trim();
                if (optUseAddress.IsChecked != null && optUseAddress.IsChecked.Value)
                {
                    _Contact.IpAddress = ipaAddress.Text;
                }
                if (_Contact.UseComputerName && string.IsNullOrEmpty(_Contact.IpAddress))
                {
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        if (!PNStatic.SetContactIpAddress(_Contact))
                        {
                            //var msg = PNLang.Instance.GetMessageText("host_unknown", "No such host is known");
                            //msg = msg.Replace(PNStrings.PLACEHOLDER1, txtCompName.Text.Trim());
                            //PNMessageBox.Show(msg, PNStrings.PROG_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
                            //return;
                        }
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
                if (cboGroups.SelectedIndex > 0)
                {
                    var group = (string)cboGroups.SelectedItem;
                    var g = _Groups.FirstOrDefault(gr => gr.Name == group);
                    if (g != null)
                    {
                        _Contact.GroupId = g.Id;
                    }
                }
                else
                {
                    _Contact.GroupId = -1;
                }
                if (ContactChanged != null)
                {
                    var ce = new ContactChangedEventArgs(_Contact, _Mode);
                    ContactChanged(this, ce);
                    if (!ce.Accepted)
                    {
                        txtContactName.SelectAll();
                        txtContactName.Focus();
                        return;
                    }
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private async void compNamesClick()
        {
            try
            {
                _CompsList.Clear();
                Progress.Visibility = Visibility.Visible;
                _CompsList = await Task.Run(() => PNInterop.GetNetworkComputers());
                Progress.Visibility = Visibility.Collapsed;
                popComps.IsOpen = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void popComps_Opened(object sender, EventArgs e)
        {
            try
            {
                lstComps.Items.Clear();
                
                foreach (var c in _CompsList)
                {
                    var st = new StackPanel { Orientation = Orientation.Horizontal };
                    var img = new Image
                    {
                        Source = TryFindResource("computer") as BitmapImage,//new BitmapImage(new Uri(PNStrings.RESOURCE_PREFIX + "computer.png")),
                        Margin = new Thickness(4),
                        Stretch = Stretch.None,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var tb = new TextBlock
                    {
                        Text = c,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4)
                    };
                    st.Children.Add(img);
                    st.Children.Add(tb);
                    lstComps.Items.Add(st);
                }
                lstComps.Focus();
                if (string.IsNullOrWhiteSpace(txtCompName.Text)) return;
                foreach (
                    var st in
                        lstComps.Items.OfType<StackPanel>()
                            .Where(st => st.Children.OfType<TextBlock>().Any(tb => tb.Text == txtCompName.Text)))
                {
                    lstComps.SelectedItem = st;
                    return;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstComps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(lstComps.SelectedItem is StackPanel st)) return;
                if (!(st.Children[1] is TextBlock tb)) return;
                txtCompName.Text = tb.Text;
                popComps.IsOpen = false;
                //always get ip address
                var ipHostInfo = Dns.GetHostEntry(txtCompName.Text);
                if (ipHostInfo.AddressList.Any(ip => ip.Equals(PNSingleton.Instance.IpAddress)))
                {
                    ipaAddress.SetAddressBytes(PNSingleton.Instance.IpAddress.GetAddressBytes());
                }
                else
                {
                    var ipAddress =
                        ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (ipAddress != null)
                    {
                        ipaAddress.SetAddressBytes(ipAddress.GetAddressBytes());
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void lstComps_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    popComps.IsOpen = false;
                }
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
                        e.CanExecute = enableOk();
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = true;
                        break;
                    case CommandType.Dummy:
                        if (optUseCompName.IsChecked != null)
                            e.CanExecute = optUseCompName.IsChecked.Value && Progress.Visibility != Visibility.Visible;
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
                    case CommandType.Dummy:
                        compNamesClick();
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

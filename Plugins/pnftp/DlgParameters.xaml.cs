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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PNCommon;

namespace pnftp
{
    /// <summary>
    /// Interaction logic for DlgParameters.xaml
    /// </summary>
    public partial class DlgParameters
    {
        public DlgParameters(string server, string directory, string user, string password, string port)
        {
            InitializeComponent();
            SetResourceReference(StyleProperty, "CustomWindowStyle");
            txtServer.Text = server;
            txtDirectory.Text = !string.IsNullOrWhiteSpace(directory) ? directory : "/";
            txtUser.Text = user;
            txtPassword.Password = password;
            txtPort.Text = !string.IsNullOrWhiteSpace(port) ? port : "21";
        }

        internal event EventHandler<ParametersDefinedEventArgs> ParametersDefined;

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableSave();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            enableSave();
        }

        private void enableSave()
        {
            try
            {
                cmdSave.IsEnabled = txtServer.Text.Trim().Length > 0 && txtDirectory.Text.Trim().Length > 0 &&
                                    txtUser.Text.Trim().Length > 0 && txtPassword.Password.Trim().Length > 0 &&
                                    txtPort.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ParametersDefined == null) return;
                ParametersDefined(this,
                                  new ParametersDefinedEventArgs(txtServer.Text.Trim(), txtDirectory.Text.Trim(),
                                                                 txtUser.Text.Trim(), txtPassword.Password.Trim(),
                                                                 txtPort.Text.Trim()));
                DialogResult = true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                lblServer.Text = Utils.GetString(lblServer.Name, lblServer.Text);
                lblDirectory.Text = Utils.GetString(lblDirectory.Name, lblDirectory.Text);
                lblUser.Text = Utils.GetString(lblUser.Name, lblUser.Text);
                lblPassword.Text = Utils.GetString(lblPassword.Name, lblPassword.Text);
                lblPort.Text = Utils.GetString(lblPort.Name, lblPort.Text);
                cmdSave.Content = Utils.GetString(cmdSave.Name, (string)cmdSave.Content);
                cmdCancel.Content = Utils.GetString(cmdCancel.Name, (string)cmdCancel.Content);
                txtServer.SelectAll();
                txtServer.Focus();
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        private void txtPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void txtPort_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }
    }

    internal class ParametersDefinedEventArgs : EventArgs
    {
        internal string Server { get; }
        internal string Directory { get; }
        internal string User { get; }
        internal string Password { get; }
        internal string Port { get; }

        internal ParametersDefinedEventArgs(string server, string directory, string user, string password, string port)
        {
            Server = server;
            Directory = directory;
            User = user;
            Password = password;
            Port = port;
        }
    }
}

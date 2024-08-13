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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndHelpChooser.xaml
    /// </summary>
    public partial class WndHelpChooser
    {
        public WndHelpChooser()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        private string _FileToOpen = "";
        private string _Progress = "";
        
        public bool InProgress { get; private set; }

        private void DlgHelpChooser_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetCaptionText("help_chooser", "Help file chooser");
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgHelpChooser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            Close();
        }

        private void oKClick()
        {
            try
            {
                if (optGoOnlineHelp.IsChecked != null && optGoOnlineHelp.IsChecked.Value)
                {
                    PNStatic.LoadPage(PNStrings.URL_HELP);
                    DialogResult = true;
                }
                else if (optGetCHM.IsChecked != null && optGetCHM.IsChecked.Value)
                {
                    _FileToOpen = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.CHM_FILE);
                    downloadFile(PNStrings.URL_DOWNLOAD_ROOT + PNStrings.CHM_FILE);
                }
                else
                {
                    _FileToOpen = Path.Combine(System.Windows.Forms.Application.StartupPath, PNStrings.PDF_FILE);
                    downloadFile(PNStrings.URL_DOWNLOAD_ROOT + PNStrings.PDF_FILE);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void disableControls()
        {
            try
            {
                lblHelpMissing.IsEnabled =
                    optGetCHM.IsEnabled =
                        optGetPDF.IsEnabled = optGoOnlineHelp.IsEnabled = false;
                elpProgress.Visibility = lblDownloadInProgress.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void downloadFile(string url)
        {
            try
            {
                InProgress = true;
                disableControls();
                _Progress = lblDownloadInProgress.Text;
                var wr = WebRequest.Create(url);
                try
                {
                    wr.Method = "HEAD";
                    wr.GetResponse();
                }
                catch (Exception ex)
                {
                    PNStatic.LogException(ex);
                    DialogResult = false;
                }
                using (var wc = new WebClient())
                {
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    var uri = new Uri(url);

                    wc.DownloadFileAsync(uri, _FileToOpen);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblDownloadInProgress.Text = _Progress + @" " + e.ProgressPercentage + @"%";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                InProgress = false;
                System.Diagnostics.Process.Start(_FileToOpen);
                DialogResult = true;
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
                    case CommandType.Cancel:
                        e.CanExecute = !InProgress;
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

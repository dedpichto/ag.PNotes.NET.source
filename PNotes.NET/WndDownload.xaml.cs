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

using Ionic.Zip;
using System;
using System.IO;
using System.Net;
using System.Windows;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndDownload.xaml
    /// </summary>
    public partial class WndDownload
    {
        public WndDownload()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndDownload(string fileToDownload, string dirToUnzip)
            : this()
        {
            _FileToDownload = fileToDownload;
            _DirToUnzip = dirToUnzip;
        }

        private readonly string _DirToUnzip;
        private readonly string _FileToDownload;
        private WebClient _WebClient;
        private string _TempFile;

        private void DlgDownload_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNSingleton.Instance.ReportViewerDownload = true;
                PNLang.Instance.ApplyControlLanguage(this);
                Title = PNLang.Instance.GetControlText("lblDownloadInProgress", "Download in progress...");
                FlowDirection = PNLang.Instance.GetFlowDirection();
                downloadFiles();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                PNSingleton.Instance.ReportViewerDownload = false;
            }
        }

        private void DlgDownload_Unloaded(object sender, RoutedEventArgs e)
        {
            PNSingleton.Instance.ReportViewerDownload = false;
        }

        private void downloadFiles()
        {
            try
            {
                _TempFile = Path.Combine(PNPaths.Instance.TempDir + _FileToDownload);
                var url = new Uri(PNStrings.URL_DOWNLOAD_ROOT + _FileToDownload);
                _WebClient = new WebClient();
                _WebClient.DownloadFileCompleted += _WebClient_DownloadFileCompleted;
                _WebClient.DownloadProgressChanged += _WebClient_DownloadProgressChanged;
                _WebClient.DownloadFileAsync(url, _TempFile, _FileToDownload);
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblDownload.Text = Convert.ToString(e.UserState) + @" " +
                                       e.ProgressPercentage.ToString(PNRuntimes.Instance.CultureInvariant) + @"%";
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void _WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                lblDownload.Text = PNLang.Instance.GetCaptionText("extracting", "Extracting:") + " " + _TempFile;
                using (var zipFile = new ZipFile(_TempFile))
                {
                    zipFile.ExtractAll(_DirToUnzip, ExtractExistingFileAction.OverwriteSilently);
                }
                PNStatic.LogThis("Downloaded and extracted " + Path.GetFileName(_TempFile));
                File.Delete(_TempFile);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }
    }
}

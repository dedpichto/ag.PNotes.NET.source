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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace PNotes.NET
{
    /// <summary>
    /// Interaction logic for WndGetThemes.xaml
    /// </summary>
    public partial class WndGetThemes
    {
        public WndGetThemes()
        {
            InitializeComponent();
            DataContext = PNSingleton.Instance.FontUser;
        }

        internal WndGetThemes(IEnumerable<ThemesUpdate> listThemes)
            : this()
        {
            PNSingleton.Instance.ThemesDownload = true;
            foreach (var th in listThemes)
            {
                var item = new PNListBoxItem(null, th.FriendlyName + " " + th.Suffix, th, false);
                lstThemes.Items.Add(item);
            }
        }

        private const string ZIP_SUFFIX = ".zip";

        private List<Tuple<string, string, string, string>> _FilesList;
        private WebClient _WebClient;
        private int _Index;
        private bool _InProgress;

        private void DlgGetThemes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PNLang.Instance.ApplyControlLanguage(this);
                FlowDirection = PNLang.Instance.GetFlowDirection();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void DlgGetThemes_Unloaded(object sender, RoutedEventArgs e)
        {
            PNSingleton.Instance.ThemesDownload = false;
        }

        private void downloadClick()
        {
            try
            {
                if (!prepareDownloadList() || _FilesList == null || _FilesList.Count == 0) return;
                downloadFiles();
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private void downloadFiles()
        {
            try
            {
                _InProgress = true;
                _WebClient = new WebClient();
                _WebClient.DownloadFileCompleted += _WebClient_DownloadFileCompleted;
                _WebClient.DownloadProgressChanged += _WebClient_DownloadProgressChanged;
                if (File.Exists(_FilesList[0].Item2)) File.Delete(_FilesList[0].Item2);
                _WebClient.DownloadFileAsync(new Uri(_FilesList[0].Item1), _FilesList[0].Item2,
                                             Path.GetFileName(_FilesList[0].Item2));
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
                if (_Index >= _FilesList.Count) return;
                using (var zipFile = new ZipFile(_FilesList[_Index].Item2))
                {
                    zipFile.ExtractAll(Path.Combine(Path.GetTempPath(), PNStrings.TEMP_THEMES_DIR), ExtractExistingFileAction.OverwriteSilently);
                }
                File.Delete(_FilesList[_Index].Item2);
                _Index++;
                if (_Index < _FilesList.Count)
                {
                    if (File.Exists(_FilesList[_Index].Item2)) File.Delete(_FilesList[_Index].Item2);
                    _WebClient.DownloadFileAsync(new Uri(_FilesList[_Index].Item1), _FilesList[_Index].Item2,
                                                 Path.GetFileName(_FilesList[_Index].Item2));
                }
                else
                {
                    preparePreRunXml();
                    _InProgress = false;
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        private bool prepareDownloadList()
        {
            try
            {
                _FilesList = new List<Tuple<string, string, string, string>>();
                var tempDir = Path.GetTempPath();
                foreach (var item in lstThemes.Items.OfType<PNListBoxItem>().Where(it => it.IsChecked != null && it.IsChecked.Value))
                {
                    var sb = new StringBuilder();
                    if (!(item.Tag is ThemesUpdate tu)) continue;
                    sb.Append(PNStrings.URL_DOWNLOAD_DIR);
                    sb.Append(tu.Name);
                    sb.Append(ZIP_SUFFIX);
                    var tuple = Tuple.Create(sb.ToString(),
                        Path.Combine(tempDir, tu.Name + ZIP_SUFFIX),
                        Path.Combine(PNPaths.Instance.ThemesDir, tu.FileName), tu.FileName);
                    _FilesList.Add(tuple);
                }
                return true;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return false;
            }
        }

        private void preparePreRunXml()
        {
            try
            {
                var filePreRun = Path.Combine(Path.GetTempPath(), PNStrings.PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(PNStrings.ELM_PRE_RUN);
                var addCopy = false;
                var xcopies = xroot.Element(PNStrings.ELM_COPY_THEMES);
                if (xcopies == null)
                {
                    addCopy = true;
                    xcopies = new XElement(PNStrings.ELM_COPY_THEMES);
                }
                else
                {
                    xcopies.RemoveAll();
                }
                foreach (var tuple in _FilesList)
                {
                    var fromPath =
                        Path.Combine(
                            Path.Combine(Path.Combine(Path.GetTempPath(), PNStrings.TEMP_THEMES_DIR), "themes"),
                            tuple.Item4);
                    //var name = string.IsNullOrEmpty(fromPath) ? "" : Path.GetFileName(fromPath);

                    var xc = new XElement(PNStrings.ELM_COPY);
                    xc.Add(new XAttribute(PNStrings.ATT_NAME, tuple.Item4));
                    xc.Add(new XAttribute(PNStrings.ATT_FROM, fromPath));
                    xc.Add(new XAttribute(PNStrings.ATT_TO, tuple.Item3));
                    xcopies.Add(xc);
                }
                if (addCopy)
                {
                    xroot.Add(xcopies);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
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
                    case CommandType.Download:
                        e.CanExecute = !_InProgress && lstThemes.Items.OfType<PNListBoxItem>().Any(it => it.IsChecked != null && it.IsChecked.Value);
                        break;
                    case CommandType.Cancel:
                        e.CanExecute = !_InProgress;
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
                    case CommandType.Download:
                        downloadClick();
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

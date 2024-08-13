// PNUpdater - update utility for PNotes.NET program
// Copyright (C) 2013 Andrey Gruber

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
//using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Ionic.Zip;

namespace PNUpdater
{
    public partial class DlgUpdate : Form
    {
        public DlgUpdate()
        {
            InitializeComponent();
        }

        private const string PRE_RUN_FILE = "pnotesprerun.xml";
        private const string ATT_FROM = "from";
        private const string ATT_TO = "to";
        private const string ATT_NAME = "name";
        private const string ATT_DEL_DIR = "deleteDir";
        private const string ATT_IS_CRITICAL = "isCritical";
        private const string ELM_COPY_FILES = "copy_files";
        private const string ELM_COPY = "copy";
        private const string ELM_PRE_RUN = "pre_run";
        private const string ELM_REMOVE = "remove";
        private const string ELM_DIR = "dir";
        private const string CRITICAL_UPDATE_LOG = "pncritical";

        //private readonly List<string> _Plugins = new List<string>();
        private string _ProgFile;
        //private int _PluginsCounter;

        private void DlgUpdate_Load(object sender, EventArgs e)
        {
            try
            {
                Text = Params.Instance.Captions[0];
                lblAction.Text = Params.Instance.Captions[1];
                Show();
                Application.DoEvents();
                //if (Params.Instance.UpdateType == UpdateType.PostPlugin && Params.Instance.PluginsList.Count > 0)
                //{
                //    readPluginsDirectory(Params.Instance.PluginsList[0]);
                //}
                if (Params.Instance.UpdateType == UpdateType.Program)
                {
                    getProgramUpdate();
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void getProgramUpdate()
        {
            try
            {
                lblPercent.Text = "";
                using (var wc = new WebClient())
                {
                    wc.DownloadFileCompleted += program_DownloadFileCompleted;
                    wc.DownloadProgressChanged += program_DownloadProgressChanged;
                    _ProgFile = Path.Combine(Path.GetTempPath(), Params.Instance.UpdateZip);
                    var uri = new Uri(Params.Instance.UpdateUrl + Params.Instance.UpdateZip);
                    lblProgress.Text = Params.Instance.UpdateZip;
                    Application.DoEvents();
                    wc.DownloadFileAsync(uri, _ProgFile);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        void program_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                lblPercent.Text = e.ProgressPercentage + @"%";
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        void program_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    logException(e.Error);
                    return;
                }
                if (e.Cancelled) return;
                var wc = sender as WebClient;
                if (wc != null)
                {
                    wc.DownloadFileCompleted -= program_DownloadFileCompleted;
                    wc.DownloadProgressChanged -= program_DownloadProgressChanged;
                }
                installProgram();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        //private void pluginDirectoryCopy(string sourceDir, string destDir)
        //{
        //    try
        //    {
        //        var name = Path.GetFileName(sourceDir);
        //        if (string.IsNullOrEmpty(name)) return;
        //        var di = new DirectoryInfo(destDir);
        //        var dirs = di.GetDirectories();
        //        var newName = Path.Combine(destDir, name);
        //        if (dirs.All(d => d.Name != name))
        //        {
        //            Directory.CreateDirectory(newName);
        //        }
        //        var diSource = new DirectoryInfo(sourceDir);
        //        var files = diSource.GetFiles();
        //        foreach (var f in files)
        //        {
        //            lblProgress.Text = f.FullName;
        //            Application.DoEvents();
        //            File.Copy(f.FullName, Path.Combine(newName, f.Name), true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logException(ex);
        //    }
        //}

        private void installProgram()
        {
            try
            {
                var listCriticals = new List<string>(); 
                lblAction.Text = Params.Instance.Captions[2];
                Application.DoEvents();
                using (var zipFile = new ZipFile(_ProgFile))
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), "pntempinstalldir");
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                    Directory.CreateDirectory(tempDir);
                    zipFile.ExtractAll(tempDir, ExtractExistingFileAction.OverwriteSilently);
                    var di = new DirectoryInfo(tempDir);
                    var files = di.GetFiles();
                    foreach (var f in files)
                    {
                        var fileName = Path.GetFileName(Application.ExecutablePath);
                        if (!string.IsNullOrEmpty(fileName)) fileName = fileName.ToUpper();
                        if (fileName == f.Name.ToUpper())
                        {
                            Params.Instance.SelfPath = f.FullName;
                            buildPreRunXml(f.Name, f.FullName, Path.Combine(Params.Instance.ProgramDir, f.Name));
                        }
                        else
                        {
                            var destFile = Path.Combine(Params.Instance.ProgramDir, f.Name);
                            File.Copy(f.FullName, destFile, true);
                            if (Params.Instance.UpdateZip.ToUpper().Contains("CRITICAL"))
                            {
                                listCriticals.Add(f.Name);
                            }
                        }
                    }
                    var dirs = di.GetDirectories();
                    foreach (var d in dirs)
                    {
                        recursivelyCopyDirectories(Params.Instance.ProgramDir, d);
                    }
                    Params.Instance.DirectoriesToDelete.Add(tempDir);
                    buildPreRunRemove(tempDir);
                    //Directory.Delete(tempDir, true);
                    //log critical updates
                    if (listCriticals.Any())
                    {
                        using (
                            var sw = new StreamWriter(Path.Combine(Path.GetTempPath(), CRITICAL_UPDATE_LOG),
                                true))
                        {
                            foreach (var s in listCriticals)
                                sw.WriteLine(s);
                        }
                    }
                }
                File.Delete(_ProgFile);
                Close();
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void buildPreRunRemove(string dirName)
        {
            try
            {
                var addRemove = false;
                var filePreRun = Path.Combine(Path.GetTempPath(), PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(ELM_PRE_RUN);
                var xrem = xroot.Element(ELM_REMOVE);
                if (xrem == null)
                {
                    addRemove = true;
                    xrem = new XElement(ELM_REMOVE);
                }
                xrem.Add(new XElement(ELM_DIR, dirName));
                if (addRemove)
                {
                    xroot.Add(xrem);
                }
                if (xdoc.Root == null)
                    xdoc.Add(xroot);
                xdoc.Save(filePreRun);
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        private void buildPreRunXml(string name, string pathFrom, string pathTo)
        {
            try
            {
                var filePreRun = Path.Combine(Path.GetTempPath(), PRE_RUN_FILE);
                var xdoc = File.Exists(filePreRun) ? XDocument.Load(filePreRun) : new XDocument();
                var xroot = xdoc.Root ?? new XElement(ELM_PRE_RUN);
                var addCopy = false;
                var xcopies = xroot.Element(ELM_COPY_FILES);
                if (xcopies == null)
                {
                    addCopy = true;
                    xcopies = new XElement(ELM_COPY_FILES);
                }
                var xc = new XElement(ELM_COPY);
                xc.Add(new XAttribute(ATT_NAME, name));
                xc.Add(new XAttribute(ATT_FROM, pathFrom));
                xc.Add(new XAttribute(ATT_TO, pathTo));
                if (Params.Instance.UpdateZip.ToUpper().Contains("CRITICAL"))
                    xc.Add(new XAttribute(ATT_IS_CRITICAL, "true"));
                xcopies.Add(xc);
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
                logException(ex);
            }
        }

        private void recursivelyCopyDirectories(string parentPath, DirectoryInfo di)
        {
            try
            {
                var files = di.GetFiles();
                var destPath = Path.Combine(parentPath, di.Name);
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                foreach (var f in files)
                {
                    var destFile = Path.Combine(destPath, f.Name);
                    File.Copy(f.FullName, destFile, true);
                }
                var dirs = di.GetDirectories();
                foreach (var d in dirs)
                {
                    recursivelyCopyDirectories(destPath, d);
                }
            }
            catch (Exception ex)
            {
                logException(ex);
            }
        }

        //private void installPlugins()
        //{
        //    try
        //    {
        //        var dirs = new List<string>();
        //        lblAction.Text = Params.Instance.Captions[2];
        //        Application.DoEvents();
        //        foreach (var s in _Plugins)
        //        {
        //            using (var zipFile = new ZipFile(s))
        //            {
        //                lblProgress.Text = s;
        //                Application.DoEvents();
        //                zipFile.ExtractAll(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently);
        //                var dirName = Path.GetFileNameWithoutExtension(s);
        //                if (!string.IsNullOrEmpty(dirName))
        //                {
        //                    dirName = dirName.Substring(0, dirName.Length - "_bin".Length);
        //                    dirName = Path.Combine(Path.GetTempPath(), dirName);
        //                    dirs.Add(dirName);
        //                }
        //            }
        //            File.Delete(s);
        //        }
        //        lblAction.Text = Params.Instance.Captions[3];
        //        Application.DoEvents();
        //        foreach (var s in dirs)
        //        {
        //            pluginDirectoryCopy(s, Params.Instance.TargetDir);
        //            Params.Instance.DirectoriesToDelete.Add(s);
        //            //Directory.Delete(s, true);
        //        }
        //        Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        logException(ex);
        //    }
        //}

        //private void readPluginsDirectory(string fileName)
        //{
        //    try
        //    {
        //        lblPercent.Text = "";
        //        Application.DoEvents();
        //        using (var wc = new WebClient())
        //        {
        //            wc.DownloadFileCompleted += plugin_DownloadFileCompleted;
        //            wc.DownloadProgressChanged += plugin_DownloadProgressChanged;
        //            var tempFile = Path.Combine(Path.GetTempPath(), fileName + "_bin.zip");
        //            var uri = new Uri(Params.Instance.UpdateUrl + fileName + "_bin.zip");
        //            lblProgress.Text = fileName;
        //            Application.DoEvents();
        //            wc.DownloadFileAsync(uri, tempFile);
        //            _Plugins.Add(tempFile);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logException(ex);
        //    }
        //}

        //private void plugin_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    try
        //    {
        //        lblPercent.Text = e.ProgressPercentage + @"%";
        //        Application.DoEvents();
        //    }
        //    catch (Exception ex)
        //    {
        //        logException(ex);
        //    }
        //}

        //private void plugin_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error != null)
        //        {
        //            logException(e.Error);
        //            return;
        //        }
        //        if (e.Cancelled) return;
        //        var wc = sender as WebClient;
        //        if (wc != null)
        //        {
        //            wc.DownloadFileCompleted -= plugin_DownloadFileCompleted;
        //            wc.DownloadProgressChanged -= plugin_DownloadProgressChanged;
        //        }
        //        _PluginsCounter++;
        //        if (Params.Instance.PluginsList.Count > _PluginsCounter)
        //        {
        //            readPluginsDirectory(Params.Instance.PluginsList[_PluginsCounter]);
        //        }
        //        else
        //        {
        //            lblPercent.Text = "";
        //            Application.DoEvents();
        //            installPlugins();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logException(ex);
        //    }
        //}

        private static void logException(Exception ex, bool showMessage = true)
        {
            try
            {
                var type = ex.GetType();
                using (var w = new StreamWriter("pnupdater.log", true))
                {
                    var stack = new StackTrace(ex, true);
                    var frame = stack.GetFrame(stack.FrameCount - 1);

                    var sb = new StringBuilder();
                    sb.Append(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    sb.AppendLine();
                    sb.Append("Type: ");
                    sb.Append(type);
                    sb.AppendLine();
                    sb.Append("Message: ");
                    sb.Append(ex.Message);
                    sb.AppendLine();
                    sb.Append("In: ");
                    sb.Append(frame.GetFileName());
                    sb.Append("; at: ");
                    sb.Append(frame.GetMethod().Name);
                    var line = frame.GetFileLineNumber();
                    var column = frame.GetFileColumnNumber();
                    if (line != 0 || column != 0)
                    {
                        sb.Append("; line: ");
                        sb.Append(line);
                        sb.Append("; column: ");
                        sb.Append(column);
                    }
                    else
                    {
                        sb.Append("; line: undefined; column: undefined");
                    }
                    sb.AppendLine();
                    sb.Append("***************************");
                    w.WriteLine(sb.ToString());
                }
                if (showMessage)
                    MessageBox.Show(type.ToString() + '\n' + ex.Message);
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}

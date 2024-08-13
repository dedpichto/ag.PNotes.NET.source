using PluginsCore;
using PNCommon;
using PNEncryption;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace pnsftp
{
    internal class SFTPSync
    {
        public event EventHandler<SyncCompleteEventArgs> InnerSyncComplete;

        private class _SFTPFile
        {
            public SftpFile FileInfo;
            public bool Copy;
            public string TempPath;
        }

        private class _NoteFile
        {
            public string Path;
            public string Name;
            public bool Copy;
        }

        private const string CONFIG_FILE = "pnsftpconfig.xml";
        private const string TEMP_SYNC_DIR = "sftptemp";
        private const string TEMP_DB_FILE = "temp.db3";
        private const string LOCALIZATIONS_FILE = "pnsftplocalizations.xml";

        private SyncResult _Result = SyncResult.Error;
        private Dictionary<string, string> _Params;

        private string _Server, _Directory, _User, _Password, _Port;

        public void SynchronizeInBackground(Dictionary<string, string> parameters)
        {
            innerSync(parameters);
        }

        public SyncResult Synchronize(Dictionary<string, string> parameters)
        {
            try
            {
                innerSync(parameters, false);
                return _Result;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return SyncResult.Error;
            }
        }

        public SyncResult Synchronize(Dictionary<string, string> parameters, string culture)
        {
            Utils.XCulture = culture;
            var localPath = Path.Combine(Utils.AssemblyDirectory, LOCALIZATIONS_FILE);
            if (Utils.XLang == null && File.Exists(localPath))
            {
                Utils.XLang = XDocument.Load(localPath);
            }
            return Synchronize(parameters);
        }

        private void innerSync(Dictionary<string, string> parameters, bool silent = true)
        {
            try
            {
                _Params = parameters ?? throw new ArgumentNullException(nameof(parameters));

                //load configuration file
                var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    throw new Exception(Utils.GetString("config_file_not_found", "Configuration file not found"));
                }
                var xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");

                //get parameters from configuration file
                var xServer = xdoc.Root.Element("server");
                if (xServer != null) _Server = xServer.Value;
                var xDirectory = xdoc.Root.Element("directory");
                if (xDirectory != null) _Directory = xDirectory.Value;
                var xUser = xdoc.Root.Element("user");
                if (xUser != null) _User = xUser.Value;
                var xPassword = xdoc.Root.Element("password");
                if (xPassword != null && !string.IsNullOrEmpty(_User))
                {
                    using (var pe = new PNEncryptor(_User))
                    {
                        _Password = pe.DecryptString(xPassword.Value);
                    }
                }
                var xPort = xdoc.Root.Element("port");
                if (xPort != null) _Port = xPort.Value;
                if (string.IsNullOrEmpty(_Server) || string.IsNullOrEmpty(_Directory) || string.IsNullOrEmpty(_User) || string.IsNullOrEmpty(_Password) || string.IsNullOrEmpty(_Port))
                {
                    var dlgP = new DlgParameters(_Server, _Directory, _User, _Password, _Port)
                    {
                        Background = SystemColors.ControlBrush
                    };
                    dlgP.ParametersDefined += dlgP_ParametersDefined;
                    var result = dlgP.ShowDialog();
                    if (!result.HasValue || !result.Value)
                    {
                        throw new Exception(Utils.GetString("parameters_not_set",
                            "FTP connection parameters have not been set"));
                    }
                }

                if (silent)
                {
                    var bg = new BackgroundWorker();
                    bg.DoWork += bg_DoWork;
                    bg.RunWorkerCompleted += bg_RunWorkerCompleted;
                    bg.RunWorkerAsync();
                }
                else
                {
                    _Result = syncFTP();
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                _Result = SyncResult.Error;
            }
        }

        private void dlgP_ParametersDefined(object sender, ParametersDefinedEventArgs e)
        {
            if (sender is DlgParameters dlgP) dlgP.ParametersDefined -= dlgP_ParametersDefined;
            _Server = e.Server;
            _Directory = e.Directory;
            _User = e.User;
            _Password = e.Password;
            _Port = e.Port;
            var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
            var xdoc = XDocument.Load(configPath);
            var xroot = xdoc.Root ?? new XElement("configuration");
            var xServer = xroot.Element("server");
            if (xServer != null)
                xServer.SetValue(_Server);
            else
                xroot.Add(new XElement("server", _Server));
            var xDirectory = xroot.Element("directory");
            if (xDirectory != null)
                xDirectory.SetValue(_Directory);
            else
                xroot.Add(new XElement("directory", _Directory));
            var xUser = xroot.Element("user");
            if (xUser != null)
                xUser.SetValue(_User);
            else
                xroot.Add(new XElement("user", _User));
            var xPort = xroot.Element("port");
            if (xPort != null)
                xPort.SetValue(_Port);
            else
                xroot.Add(new XElement("port", _Port));
            using (var pe = new PNEncryptor(_User))
            {
                var xPassword = xroot.Element("password");
                if (xPassword != null)
                    xPassword.SetValue(pe.EncryptString(_Password));
                else
                    xroot.Add(new XElement("password", pe.EncryptString(_Password)));
            }

            if (xdoc.Root == null)
            {
                xdoc.Add(xroot);
            }
            xdoc.Save(configPath);
        }

        //private void clearConfiguration()
        //{
        //    var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
        //    var xdoc = XDocument.Load(configPath);
        //    var xroot = xdoc.Root ?? new XElement("configuration");
        //    var xServer = xroot.Element("server");
        //    xServer?.Remove();
        //    var xDirectory = xroot.Element("directory");
        //    xDirectory?.Remove();
        //    var xUser = xroot.Element("user");
        //    xUser?.Remove();
        //    var xPassword = xroot.Element("password");
        //    xPassword?.Remove();
        //    xdoc.Save(configPath);
        //}
        
        private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _Result = (SyncResult)e.Result;
            InnerSyncComplete?.Invoke(this, new SyncCompleteEventArgs(_Result));
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = syncFTP();
        }

        private SyncResult syncFTP()
        {
            var tempDir = PNSyncCommon.CreateTempDir(TEMP_SYNC_DIR);
            try
            {

                PNSyncCommon.SetCultureInfo();

                var srcDBPath = _Params["dbPath"];
                var srcDataDir = _Params["dataPath"];
                var includeDeleted = bool.Parse(_Params["includeDeleted"]);
                var dropTriggers = _Params["dropTriggers"];
                var createTriggers = _Params["createTriggers"];
                var noteExt = _Params["noteExt"];
                var dbName = Path.GetFileName(srcDBPath);
                if (string.IsNullOrEmpty(dbName))
                    throw new ArgumentNullException("db" + "Path", @"Source DB path must include database file name");

                var filesDest = new List<_SFTPFile>();
                var filesSrc = new List<_NoteFile>();
                var result = SyncResult.Reload;

                while (_Directory.Length > 0 &&
                       (_Directory.StartsWith(@"/") || _Directory.StartsWith(@"\") || _Directory.StartsWith(@".")))
                {
                    _Directory = _Directory.Substring(1);
                }
                using (var connectionInfo = new PasswordConnectionInfo(_Server, Convert.ToInt32(_Port), _User, _Password))
                {
                    using (var client = new SftpClient(connectionInfo))
                    {
                        client.Connect();
                        if (!string.IsNullOrWhiteSpace(_Directory) && _Directory != "/" && _Directory != "\\")
                        {
                            if (!client.Exists(_Directory))
                            {
                                client.CreateDirectory(_Directory);
                            }
                            client.ChangeDirectory(_Directory);
                        }
                        var tempDBSrc = Path.Combine(tempDir, dbName);
                        var tempDBDest = Path.Combine(tempDir, TEMP_DB_FILE);
                        // copy source db to temp directory
                        File.Copy(srcDBPath, tempDBSrc, true);
                        // build source connection string
                        var srcConnectionString = "data source=\"" + tempDBSrc + "\"";
                        using (var eSrc = new SQLiteDataObject(srcConnectionString))
                        {
                            // drop triggers
                            eSrc.Execute(dropTriggers);
                            // get list of all source notes files
                            var srcNotes = new DirectoryInfo(srcDataDir).GetFiles("*" + noteExt);
                            filesSrc.AddRange(
                                srcNotes.Select(fi => new _NoteFile { Path = fi.FullName, Name = fi.Name, Copy = false }));
                            if (!includeDeleted)
                            {
                                var deletedSrc = PNSyncCommon.DeletedIDs(eSrc);
                                filesSrc.RemoveAll(nf => deletedSrc.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                            }
                            // get list of all destination notes files
                            var destNotes =
                                client.ListDirectory(client.WorkingDirectory).Where(f => !f.IsDirectory && f.Name.EndsWith(noteExt));
                            filesDest.AddRange(destNotes.Select(fir => new _SFTPFile { FileInfo = fir, Copy = false }));

                            // check whether db file exists on remote server
                            if (client.Exists(dbName))
                            {
                                // copy db file to temp directory
                                if (!copyFromRemote(client, tempDBDest, dbName))
                                {
                                    return SyncResult.Error;
                                }
                                // build connection string
                                var destConnectionString = "data source=\"" + tempDBDest + "\"";
                                using (var eDest = new SQLiteDataObject(destConnectionString))
                                {
                                    // drop triggers
                                    eDest.Execute(dropTriggers);
                                    if (PNSyncCommon.AreTablesDifferent(eSrc, eDest))
                                    {
                                        return SyncResult.AbortVersion;
                                    }

                                    var tablesData = PNSyncCommon.GetTablesData(eSrc);

                                    // get deleted ids
                                    if (!includeDeleted)
                                    {
                                        var deletedDest = PNSyncCommon.DeletedIDs(eDest);
                                        filesDest.RemoveAll(
                                            nf => deletedDest.Contains(Path.GetFileNameWithoutExtension(nf.FileInfo.Name)));
                                    }
                                    foreach (var sf in filesSrc)
                                    {
                                        var id = Path.GetFileNameWithoutExtension(sf.Name);
                                        // find destination file with same name
                                        var df = filesDest.FirstOrDefault(f => f.FileInfo.Name == sf.Name);
                                        if (df == null)
                                        {
                                            sf.Copy = true;
                                            if (!PNSyncCommon.InsertToAllTables(eSrc, eDest, id, tablesData))
                                            {
                                                return SyncResult.Error;
                                            }
                                        }
                                        else
                                        {
                                            // check which note is more up to date
                                            var tempFile = Path.Combine(tempDir, sf.Name);
                                            var dLocal = PNSyncCommon.LastSavedTime(eSrc, id);// File.GetLastWriteTime(sf.Path);
                                            var dFTP = PNSyncCommon.LastSavedTime(eDest, id);// df.FileInfo.LastWriteTime;
                                            if (dLocal > dFTP)
                                            {
                                                // compare two files
                                                if (!copyFromRemote(client, tempFile, df.FileInfo.Name))
                                                {
                                                    return SyncResult.Error;
                                                }
                                                if (PNSyncCommon.AreFilesDifferent(sf.Path, tempFile))
                                                {
                                                    // local file is younger then remote - copy it to remote server
                                                    sf.Copy = true;
                                                }
                                            }
                                            else if (dLocal < dFTP)
                                            {
                                                // compare two files
                                                if (!copyFromRemote(client, tempFile, df.FileInfo.Name))
                                                {
                                                    return SyncResult.Error;
                                                }
                                                if (PNSyncCommon.AreFilesDifferent(sf.Path, tempFile))
                                                {
                                                    // remote file is younger than local - copy it to local directory
                                                    df.Copy = true;
                                                    df.TempPath = tempFile;
                                                }
                                            }
                                            if (!PNSyncCommon.ExchangeData(eSrc, eDest, id, tablesData))
                                            {
                                                return SyncResult.Error;
                                            }
                                        }
                                    }
                                    // check remaining destination files
                                    var remDest = filesDest.Where(df => !df.Copy);
                                    foreach (
                                        var df in remDest.Where(df => filesSrc.All(sf => sf.Name != df.FileInfo.Name)))
                                    {
                                        df.Copy = true;
                                        var id = Path.GetFileNameWithoutExtension(df.FileInfo.Name);
                                        if (!PNSyncCommon.ExchangeData(eSrc, eDest, id, tablesData))
                                        {
                                            return SyncResult.Error;
                                        }
                                    }
                                    // synchronize groups
                                    if (!PNSyncCommon.ExchangeGroups(eSrc, eDest, tablesData.FirstOrDefault(td => td.Key == "GROUPS")))
                                    {
                                        return SyncResult.Error;
                                    }
                                    // restore triggers
                                    eSrc.Execute(createTriggers);
                                    eDest.Execute(createTriggers);
                                }
                                // copy files
                                var filesToCopy = filesSrc.Where(sf => sf.Copy);
                                if (filesToCopy.Any(sf => !copyToRemote(client, sf.Path, sf.Name)))
                                {
                                    return SyncResult.Error;
                                }
                                var ftpFilesToCopy = filesDest.Where(df => df.Copy);
                                foreach (var df in ftpFilesToCopy)
                                {
                                    var newPath = Path.Combine(srcDataDir, df.FileInfo.Name);
                                    if (!string.IsNullOrEmpty(df.TempPath))
                                        File.Copy(df.TempPath, newPath, true);
                                    else
                                    {
                                        if (!copyFromRemote(client, newPath, df.FileInfo.Name))
                                            return SyncResult.Error;
                                    }
                                }
                                if (filesDest.Count(df => df.Copy) == 0) result = SyncResult.None;
                                // copy synchronized db files
                                File.Copy(tempDBSrc, srcDBPath, true);
                                if (!copyToRemote(client, tempDBDest, dbName))
                                    return SyncResult.Error;
                            }
                            else
                            {
                                // restore triggers
                                eSrc.Execute(createTriggers);
                                // just copy all notes files and db file to remote server
                                if (!copyToRemote(client, srcDBPath, dbName))
                                    return SyncResult.Error;
                                if (filesSrc.Any(sf => !copyToRemote(client, sf.Path, sf.Name)))
                                {
                                    return SyncResult.Error;
                                }
                                result = SyncResult.None;
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return SyncResult.Error;
            }
            finally
            {
                if (tempDir != "" && Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private bool copyFromRemote(SftpClient client, string localPath, string remotePath)
        {
            try
            {
                using (var fs = File.Open(localPath, FileMode.Create, FileAccess.Write))
                {
                    client.DownloadFile(remotePath, fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private bool copyToRemote(SftpClient client, string localPath, string remotePath)
        {
            try
            {
                using (var fs = File.OpenRead(localPath))
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        client.UploadFile(ms, remotePath, true);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }
    }

}

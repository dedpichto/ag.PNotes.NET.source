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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GDrive.NET;
using PluginsCore;
using PNCommon;
using PNEncryption;
using SQLiteWrapper;

namespace pngoogledrive
{
    internal class GDSync
    {
        public event EventHandler<SyncCompleteEventArgs> InnerSyncComplete;

        private class LocalFile
        {
            public string Path;
            public string Name;
            public bool Copy;
        }

        private class GDFile
        {
            public bool Copy;
            public string TempPath;
            public GDMetaData Data;
        }

        private const string APP_FOLDER = "pnotes.net";
        private const string CONFIG_FILE = "pngoogledriveconfig.xml";
        private const string TEMP_SYNC_DIR = "gdrivetemp";
        private const string TEMP_DB_FILE = "temp.db3";
        private const string LOCALIZATIONS_FILE = "pngoogledrivelocalizations.xml";

        private string _ClientSecret;
        private string _ClientId;
        private string _AccessToken = "";
        private string _RefreshToken = "";
        private DateTime _Expiration;
        private SyncResult _Result = SyncResult.Error;
        private Dictionary<string, string> _Params;
        //private DlgSync _DlgSync;

        public void SynchronizeInBackground(Dictionary<string, string> parameters)
        {
            innerSync(parameters);
        }

        public SyncResult Synchronize(Dictionary<string, string> parameters)
        {
            try
            {
                innerSync(parameters, false);
                //_DlgSync = new DlgSync("Google Drive", Utils.GetString("progress_text", "Synchronization in progress..."));
                //_DlgSync.ShowDialog();
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

        private void innerSync(Dictionary<string, string> parameters, bool silent=true)
        {
            try
            {
                _Params = parameters ?? throw new ArgumentNullException(nameof(parameters));
                //load configuration file
                var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    throw new Exception("Configuration file not found");
                }
                var xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");
                //get parameters from configuration file
                var xClientId = xdoc.Root.Element("clientId");
                var xClientSecret = xdoc.Root.Element("clientSecret");
                if (xClientId == null)
                {
                    throw new Exception("Client Id not found");
                }
                if (xClientSecret == null)
                {
                    throw new Exception("Client secret not found");
                }
                _ClientId = xClientId.Value;
                using (var pe = new PNEncryptor(_ClientId))
                {
                    _ClientSecret = pe.DecryptString(xClientSecret.Value);
                }

                //get tokens
                var xAccessToken = xdoc.Root.Element("accessToken");
                var xRefreshToken = xdoc.Root.Element("refreshToken");
                var xExpiration = xdoc.Root.Element("expiration");
                if (xAccessToken != null && xRefreshToken != null && xExpiration != null)
                {
                    using (var pe = new PNEncryptor(_ClientId))
                    {
                        _AccessToken = pe.DecryptString(xAccessToken.Value);
                        _RefreshToken = pe.DecryptString(xRefreshToken.Value);
                        _Expiration = DateTime.Parse(pe.DecryptString(xExpiration.Value));
                    }
                }

                var gdObject = createGDObject();
                if (gdObject == null)
                {
                    _Result = SyncResult.Error;
                    return;
                }

                if (silent)
                {
                    var bg = new BackgroundWorker();
                    bg.DoWork += bg_DoWork;
                    bg.RunWorkerCompleted += bg_RunWorkerCompleted;
                    bg.RunWorkerAsync(gdObject);
                }
                else
                {
                    _Result = syncDB(gdObject);
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                _Result = SyncResult.Error;
            }
        }

        private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (_DlgSync != null)
            //    _DlgSync.Close();
            _Result = (SyncResult)e.Result;
            InnerSyncComplete?.Invoke(this, new SyncCompleteEventArgs(_Result));
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is GDObject gdObject)
            {
                e.Result = syncDB(gdObject);
            }
        }

        private SyncResult syncDB(GDObject gdObject)
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

                var filesSrc = new List<LocalFile>();
                var filesDest = new List<GDFile>();
                var result = SyncResult.Reload;
                var remoteFolderId = gdObject.GetFolderId(APP_FOLDER);
                if (remoteFolderId == "")
                    remoteFolderId = gdObject.CreateFolder(APP_FOLDER, "root");
                var dbName = Path.GetFileName(srcDBPath);
                if (string.IsNullOrEmpty(dbName))
                    throw new ArgumentNullException("db" + "Path", @"Source DB path must include database file name");
                var tempDBSrc = Path.Combine(tempDir, dbName);
                var tempDBDest = Path.Combine(tempDir, TEMP_DB_FILE);
                // copy source db
                File.Copy(srcDBPath, tempDBSrc, true);
                // build source connection string
                var srcConnectionString = "data source=\"" + tempDBSrc + "\"";
                using (var eSrc = new SQLiteDataObject(srcConnectionString))
                {
                    // drop triggers
                    eSrc.Execute(dropTriggers);
                    // get list of all source notes files
                    var srcNotes = new DirectoryInfo(srcDataDir).GetFiles("*" + noteExt);
                    filesSrc.AddRange(srcNotes.Select(fi => new LocalFile { Path = fi.FullName, Name = fi.Name, Copy = false }));
                    // get deleted ids
                    if (!includeDeleted)
                    {
                        var deletedSrc = PNSyncCommon.DeletedIDs(eSrc);
                        filesSrc.RemoveAll(nf => deletedSrc.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                    }
                    // get list of all remote notes files
                    var data = gdObject.GetFolderContent(remoteFolderId);
                    if (data == null)
                        throw new InvalidDataException("Remote folder content cannot be retrieved");
                    filesDest.AddRange(from d in data
                                       where "." + d.FileExtension == noteExt
                                       select new GDFile {Data = d});
                    // get remote db file
                    var destDB = data.FirstOrDefault(d => d.Name == dbName);
                    if (destDB != null)
                    {
                        // copy remote db to temp directory
                        File.WriteAllBytes(tempDBDest, gdObject.GetFile(destDB.DownloadUrl));
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
                                    nf => deletedDest.Contains(Path.GetFileNameWithoutExtension(nf.Data.Name)));
                            }
                            foreach (var sf in filesSrc)
                            {
                                var id = Path.GetFileNameWithoutExtension(sf.Name);
                                // find remote file with same name
                                var df = filesDest.FirstOrDefault(f => f.Data.Name == sf.Name);
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
                                    var tempFile = Path.Combine(tempDir, sf.Name);
                                    // check which note is more up to date
                                    var dLocal = File.GetLastWriteTime(sf.Path);
                                    var dRemote = df.Data.LastModified;
                                    if (dLocal > dRemote)
                                    {
                                        // copy remote file
                                        File.WriteAllBytes(tempFile, gdObject.GetFile(df.Data.DownloadUrl));
                                        // compare two files
                                        if (PNSyncCommon.AreFilesDifferent(sf.Path, tempFile))
                                        {
                                            // local file is younger then remote - copy it to remote server
                                            sf.Copy = true;
                                        }
                                    }
                                    else if (dLocal < dRemote)
                                    {
                                        // copy remote file
                                        File.WriteAllBytes(tempFile, gdObject.GetFile(df.Data.DownloadUrl));
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
                            // check remaining remote files
                            var remDest = filesDest.Where(df => !df.Copy);
                            foreach (var df in remDest.Where(df => filesSrc.All(sf => sf.Name != df.Data.Name)))
                            {
                                df.Copy = true;
                                var id = Path.GetFileNameWithoutExtension(df.Data.Name);
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
                        foreach (var sf in filesToCopy)
                        {
                            gdObject.UploadFile(new FileInfo(sf.Path), remoteFolderId);
                        }
                        var remoteToCopy = filesDest.Where(df => df.Copy);
                        foreach (var df in remoteToCopy)
                        {
                            var newPath = Path.Combine(srcDataDir, df.Data.Name);
                            if (!string.IsNullOrEmpty(df.TempPath))
                                File.Copy(df.TempPath, newPath, true);
                            else
                                File.WriteAllBytes(newPath, gdObject.GetFile(df.Data.DownloadUrl));
                        }
                        if (filesDest.Count(df => df.Copy) == 0) result = SyncResult.None;
                        // copy synchronized db files
                        File.Copy(tempDBSrc, srcDBPath, true);
                        //rename remote file in order to upload file with needed name
                        File.Delete(tempDBSrc);
                        File.Move(tempDBDest, tempDBSrc);
                        gdObject.UploadFile(new FileInfo(tempDBSrc), remoteFolderId);
                    }
                    else
                    {
                        // restore triggers
                        eSrc.Execute(createTriggers);
                        // just copy all notes files and db file to remote server
                        gdObject.UploadFile(new FileInfo(tempDBSrc), remoteFolderId);
                        foreach (var sf in filesSrc)
                        {
                            gdObject.UploadFile(new FileInfo(sf.Path), remoteFolderId);
                        }
                        result = SyncResult.None;
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

        private GDObject createGDObject()
        {
            var gdObject = new GDObject(_ClientId, _ClientSecret);

            if (!string.IsNullOrEmpty(_AccessToken) && !string.IsNullOrEmpty(_RefreshToken) && _Expiration != DateTime.MinValue)
            {
                if (_Expiration > DateTime.Now)
                {
                    gdObject.AccessToken = _AccessToken;
                    gdObject.RefreshToken = _RefreshToken;
                    gdObject.Expiration = _Expiration;
                    return gdObject;
                }
                gdObject.RefreshToken = _RefreshToken;
                if (gdObject.RefreshAccessToken())
                {
                    _AccessToken = gdObject.AccessToken;
                    _Expiration = gdObject.Expiration;
                    saveParameters();
                    return gdObject;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_RefreshToken))
                {
                    if (gdObject.Authenticate())
                    {
                        _AccessToken = gdObject.AccessToken;
                        _RefreshToken = gdObject.RefreshToken;
                        _Expiration = gdObject.Expiration;
                        saveParameters();
                        return gdObject;
                    }
                }
                else
                {
                    gdObject.RefreshToken = _RefreshToken;
                    if (gdObject.RefreshAccessToken())
                    {
                        _AccessToken = gdObject.AccessToken;
                        _Expiration = gdObject.Expiration;
                        saveParameters();
                        return gdObject;
                    }
                }
            }
            return null;
        }

        private void saveParameters()
        {
            try
            {
                var configPath = Path.Combine(Utils.AssemblyDirectory, CONFIG_FILE);
                if (!File.Exists(configPath))
                {
                    throw new Exception("Configuration file not found");
                }
                var xdoc = XDocument.Load(configPath);
                if (xdoc.Root == null)
                    throw new Exception("Configuration file is empty");

                var xAccessToken = xdoc.Root.Element("accessToken");
                var xRefreshToken = xdoc.Root.Element("refreshToken");
                var xExpiration = xdoc.Root.Element("expiration");

                using (var pe = new PNEncryptor(_ClientId))
                {
                    if (xAccessToken != null)
                    {
                        xAccessToken.SetValue(pe.EncryptString(_AccessToken));
                    }
                    else
                    {
                        xdoc.Root.Add(new XElement("accessToken", pe.EncryptString(_AccessToken)));
                    }
                    if (xRefreshToken != null)
                    {
                        xRefreshToken.SetValue(pe.EncryptString(_RefreshToken));
                    }
                    else
                    {
                        xdoc.Root.Add(new XElement("refreshToken", pe.EncryptString(_RefreshToken)));
                    }
                    if (xExpiration != null)
                    {
                        xExpiration.SetValue(pe.EncryptString(_Expiration.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US"))));
                    }
                    else
                    {
                        xdoc.Root.Add(new XElement("expiration",
                                                   pe.EncryptString(_Expiration.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US")))));
                    }
                }
                xdoc.Save(configPath);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }
    }
}

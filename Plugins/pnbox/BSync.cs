using Box.NET;
using PluginsCore;
using PNCommon;
using PNEncryption;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace pnbox
{
    internal class BSync
    {
        public event EventHandler<SyncCompleteEventArgs> InnerSyncComplete;

        private class _NoteFile
        {
            public string Path;
            public string Name;
            public bool Copy;
        }

        private class _RemoteFile
        {
            public string Id;
            public string Name;
            public bool Copy;
            public string TempPath;
        }

        private const string APP_FOLDER = "pnotes.net";
        private const string CONFIG_FILE = "pnboxconfig.xml";
        private const string TEMP_SYNC_DIR = "boxtemp";
        private const string TEMP_DB_FILE = "temp.db3";
        private const string LOCALIZATIONS_FILE = "pnboxlocalizations.xml";

        private string _ClientId, _ClientSecret;
        private SyncResult _Result = SyncResult.Error;
        private Dictionary<string, string> _Params;

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
                var xroot = xdoc.Root;
                if (xroot == null)
                    throw new Exception("Configuration file is empty");
                //get parameters from configuration file
                var xClientID = xroot.Element("clientId");
                var xClientSecret = xroot.Element("clientSecret");
                if (xClientID == null)
                {
                    throw new Exception(Utils.GetString("id_not_found", "Client ID not found"));
                }
                if (xClientSecret == null)
                {
                    throw new Exception(Utils.GetString("secret_not_found", "Client secret not found"));
                }
                _ClientId = xClientID.Value;
                using (var pe = new PNEncryptor(_ClientId))
                {
                    _ClientSecret = pe.DecryptString(xClientSecret.Value);
                }

                var xAccessToken = xroot.Element("accessToken");
                var xRefreshToken = xroot.Element("refreshToken");
                var xTokenType = xroot.Element("tokenType");
                var xExpAccess = xroot.Element("expireAccess");
                var xExpRefresh = xroot.Element("expireRefresh");
                var bobject = new BoxObject(_ClientId, _ClientSecret);
                if (xAccessToken == null || xRefreshToken == null || xTokenType == null || xExpAccess == null ||
                    xExpRefresh == null)
                {
                    bobject = createTokens(bobject, xroot, configPath);
                    if (bobject == null)
                    {
                        _Result = SyncResult.Error;
                        return;
                    }
                }
                else
                {
                    using (var pe = new PNEncryptor(_ClientId))
                    {
                        var accessToken = pe.DecryptString(xAccessToken.Value);
                        var refreshToken = pe.DecryptString(xRefreshToken.Value);
                        //var tokenType = pe.DecryptString(xTokenType.Value);
                        var expirationAccess = pe.DecryptString(xExpAccess.Value);
                        var expirationRefresh = pe.DecryptString(xExpRefresh.Value);
                        if (DateTime.Parse(expirationAccess) <= DateTime.Now)
                        {
                            bobject.RefreshToken = refreshToken;
                            if (!refreshTokens(bobject, xroot, configPath))
                            {
                                _Result = SyncResult.Error;
                                return;
                            }
                        }
                        else if (DateTime.Parse(expirationRefresh) < DateTime.Now)
                        {
                            bobject = createTokens(bobject, xroot, configPath);
                            if (bobject == null)
                            {
                                _Result = SyncResult.Error;
                                return;
                            }
                        }
                        else
                        {
                            bobject.AccessToken = accessToken;
                        }
                    }
                }

                if (silent)
                {
                    var bg = new BackgroundWorker();
                    bg.DoWork += bg_DoWork;
                    bg.RunWorkerCompleted += bg_RunWorkerCompleted;
                    bg.RunWorkerAsync(bobject);

                }
                else
                {
                    _Result = syncDB(bobject);
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                _Result = SyncResult.Error;
            }
        }

        private bool refreshTokens(BoxObject bobject, XElement xroot, string configPath)
        {
            if (!bobject.RefreshAccessToken()) return false;

            return saveTokens(bobject, xroot, configPath);
        }

        private BoxObject createTokens(BoxObject bobject, XElement xroot, string configPath)
        {
            if (!bobject.Authorize()) return null;

            return saveTokens(bobject, xroot, configPath) ? bobject : null;
        }

        private bool saveTokens(BoxObject bobject, XElement xroot, string configPath)
        {
            try
            {
                var xAccessToken = xroot.Element("accessToken");
                var xRefreshToken = xroot.Element("refreshToken");
                var xTokenType = xroot.Element("tokenType");
                var xExpAccess = xroot.Element("expireAccess");
                var xExpRefresh = xroot.Element("expireRefresh");
                using (var pe = new PNEncryptor(_ClientId))
                {
                    var accessToken = pe.EncryptString(bobject.AccessToken);
                    var refreshToken = pe.EncryptString(bobject.RefreshToken);
                    var tokenType = pe.EncryptString(char.ToUpper(bobject.TokenType[0]) + bobject.TokenType.Substring(1));
                    var expirationAccess = pe.EncryptString(bobject.AccessTokenExpiration.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US")));
                    var expirationRefresh = pe.EncryptString(bobject.RefreshTokenExpiration.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("en-US")));
                    if (xAccessToken == null)
                        xroot.Add(new XElement("accessToken", accessToken));
                    else
                        xAccessToken.SetValue(accessToken);
                    if (xRefreshToken == null)
                        xroot.Add(new XElement("refreshToken", refreshToken));
                    else
                        xRefreshToken.SetValue(refreshToken);
                    if (xTokenType == null)
                        xroot.Add(new XElement("tokenType", tokenType));
                    else
                        xTokenType.SetValue(tokenType);
                    if (xExpAccess == null)
                        xroot.Add(new XElement("expireAccess", expirationAccess));
                    else
                        xExpAccess.SetValue(expirationAccess);
                    if (xExpRefresh == null)
                        xroot.Add(new XElement("expireRefresh", expirationRefresh));
                    else
                        xExpRefresh.SetValue(expirationRefresh);
                }

                xroot.Document?.Save(configPath);
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return false;
            }
        }

        private void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _Result = (SyncResult)e.Result;
            InnerSyncComplete?.Invoke(this, new SyncCompleteEventArgs(_Result));
        }

        private void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is BoxObject bobject)
            {
                e.Result = syncDB(bobject);
            }
        }

        private SyncResult syncDB(BoxObject bobject)
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

                var filesSrc = new List<_NoteFile>();
                var filesDest = new List<_RemoteFile>();
                var result = SyncResult.Reload;
                var folder = bobject.GetFolderMetaDataByName(APP_FOLDER) ??
                             bobject.CreateFolder(APP_FOLDER, "0");
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
                    filesSrc.AddRange(srcNotes.Select(fi => new _NoteFile { Path = fi.FullName, Name = fi.Name, Copy = false }));
                    // get deleted ids
                    if (!includeDeleted)
                    {
                        var deletedSrc = PNSyncCommon.DeletedIDs(eSrc);
                        filesSrc.RemoveAll(nf => deletedSrc.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                    }
                    // get list of all remote notes files
                    if (folder.Items != null)
                    {
                        filesDest.AddRange(
                            folder.Items.Where(f => f.Name.EndsWith(noteExt))
                                  .Select(fl => new _RemoteFile { Id = fl.Id, Name = fl.Name, Copy = false }));
                    }
                    // get remote db file
                    var destDB = folder.Items?.FirstOrDefault(d => d.Name == dbName);
                    if (destDB != null)
                    {
                        // copy remote db to temp directory
                        File.WriteAllBytes(tempDBDest, bobject.GetFile(destDB.Id));
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
                                    nf => deletedDest.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                            }
                            foreach (var sf in filesSrc)
                            {
                                var id = Path.GetFileNameWithoutExtension(sf.Name);
                                // find remote file with same name
                                var df = filesDest.FirstOrDefault(f => f.Name == sf.Name);
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
                                    var fRemote = bobject.GetFileMetaDataById(df.Id);
                                    if (fRemote == null) return SyncResult.Error;
                                    var dRemote = fRemote.ModifiedAt;
                                    if (dLocal > dRemote)
                                    {
                                        // copy remote file
                                        File.WriteAllBytes(tempFile, bobject.GetFile(df.Id));
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
                                        File.WriteAllBytes(tempFile, bobject.GetFile(df.Id));
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
                            foreach (var df in remDest.Where(df => filesSrc.All(sf => sf.Name != df.Name)))
                            {
                                df.Copy = true;
                                var id = Path.GetFileNameWithoutExtension(df.Name);
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
                            bobject.UploadFile(new FileInfo(Path.Combine(srcDataDir, sf.Name)), folder.Id);
                        }
                        var destToCopy = filesDest.Where(df => df.Copy);
                        foreach (var df in destToCopy)
                        {
                            var newPath = Path.Combine(srcDataDir, df.Name);
                            if (!string.IsNullOrEmpty(df.TempPath))
                                File.Copy(df.TempPath, newPath, true);
                            else
                                File.WriteAllBytes(newPath, bobject.GetFile(df.Id));
                        }
                        if (filesDest.Count(df => df.Copy) == 0) result = SyncResult.None;
                        // copy synchronized db files
                        File.Copy(tempDBSrc, srcDBPath, true);
                        //rename remote file in order to upload file with needed name
                        File.Delete(tempDBSrc);
                        File.Move(tempDBDest, tempDBSrc);
                        //upload renamed remote db file
                        bobject.UploadFile(new FileInfo(tempDBSrc), folder.Id);
                    }
                    else
                    {
                        // restore triggers
                        eSrc.Execute(createTriggers);
                        // just copy all notes files and db file to remote server
                        bobject.UploadFile(new FileInfo(tempDBSrc), folder.Id);
                        var listUpload = filesSrc.Select(sf => new FileInfo(Path.Combine(srcDataDir, sf.Name))).ToList();
                        bobject.UploadFiles(listUpload, folder.Id);
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
    }
}

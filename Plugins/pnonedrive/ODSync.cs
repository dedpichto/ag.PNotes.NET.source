using ODrive.NET;
using PluginsCore;
using PNCommon;
using SQLiteWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pnonedrive
{
    internal class ODSync
    {
        public event EventHandler<SyncCompleteEventArgs> InnerSyncComplete;

        private class LocalFile
        {
            public string Path;
            public string Name;
            public bool Upload;
            public bool Replace;
            public string RemoteId;
        }

        private class ODFile
        {
            public bool Copy;
            public string TempPath;
            public ODMetadata Data;
        }

        private const string APP_FOLDER = "pnotes.net";
        private const string TEMP_SYNC_DIR = "odrivetemp";
        private const string TEMP_DB_FILE = "temp.db3";

        private SyncResult _Result = SyncResult.Error;
        private Dictionary<string, string> _Params;

        public async void SynchronizeInBackground(Dictionary<string, string> parameters)
        {
            await innerSync(parameters);
            InnerSyncComplete?.Invoke(this, new SyncCompleteEventArgs(_Result));
        }

        public async Task<SyncResult> Synchronize(Dictionary<string, string> parameters)
        {
            try
            {
                await innerSync(parameters);
                return _Result;
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                return SyncResult.Error;
            }
        }

        private async Task innerSync(Dictionary<string, string> parameters)
        {
            try
            {
                _Params = parameters ?? throw new ArgumentNullException(nameof(parameters));
                var odObject = new ODObject();
                var authenticated = await odObject.AuthenticateAsync();
                if (!authenticated)
                {
                    _Result = SyncResult.Error;
                    return;
                }

                _Result = await syncDB(odObject);
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
                _Result = SyncResult.Error;
            }
        }

        private async Task<SyncResult> syncDB(ODObject odObject)
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
                var filesDest = new List<ODFile>();
                var deletedDest = new List<ODFile>();
                var result = SyncResult.Reload;

                // check whether remote folder exists
                var remoteFolderId = await odObject.GetFolderIdAsync(APP_FOLDER);
                if (remoteFolderId == "")
                {
                    // if not - create remote folder
                    remoteFolderId = await odObject.CreateFolderAsync(APP_FOLDER);
                }
                else
                {
                    // replace folder id by folder name
                    remoteFolderId = APP_FOLDER;
                }
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
                    filesSrc.AddRange(srcNotes.Select(fi => new LocalFile { Path = fi.FullName, Name = fi.Name }));
                    // get deleted ids
                    if (!includeDeleted)
                    {
                        var deletedSrc = PNSyncCommon.DeletedIDs(eSrc);
                        filesSrc.RemoveAll(nf => deletedSrc.Contains(Path.GetFileNameWithoutExtension(nf.Name)));
                    }
                    // get list of all remote notes files
                    var data = await odObject.GetFolderContentAsync(remoteFolderId);
                    if (data == null)
                        throw new InvalidDataException("Remote folder content cannot be retrieved");
                    filesDest.AddRange(from d in data where d.Extension == noteExt select new ODFile { Data = d });
                    // get remote db file
                    var destDB = data.FirstOrDefault(d => d.Name == dbName);
                    if (destDB != null)
                    {
                        // copy remote db to temp directory
                        var bytes = await odObject.GetFileBytesAsync(destDB.DownloadLink);
                        File.WriteAllBytes(tempDBDest, bytes);
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
                                var deletedIds = PNSyncCommon.DeletedIDs(eDest);
                                // store deleted for further checking
                                deletedDest.AddRange(filesDest.Where(nf =>
                                    deletedIds.Contains(Path.GetFileNameWithoutExtension(nf.Data.Name))));
                                // remove deleted from main list
                                filesDest.RemoveAll(
                                    nf => deletedIds.Contains(Path.GetFileNameWithoutExtension(nf.Data.Name)));
                            }

                            foreach (var sf in filesSrc)
                            {
                                var id = Path.GetFileNameWithoutExtension(sf.Name);
                                // find remote file with same name
                                var df = filesDest.FirstOrDefault(f => f.Data.Name == sf.Name);
                                if (df == null)
                                {
                                    df = deletedDest.FirstOrDefault(f => f.Data.Name == sf.Name);
                                    if (df == null)
                                    {
                                        // file not found at all
                                        sf.Upload = true;
                                    }
                                    else
                                    {
                                        // file is found in deleted, so we need to replace it, instead upload
                                        sf.Replace = true;
                                        sf.RemoteId = df.Data.Id;
                                    }
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
                                        bytes = await odObject.GetFileBytesAsync(df.Data.DownloadLink);
                                        File.WriteAllBytes(tempFile, bytes);
                                        // compare two files
                                        if (PNSyncCommon.AreFilesDifferent(sf.Path, tempFile))
                                        {
                                            // local file is younger then remote - copy it to remote server
                                            sf.Replace = true;
                                            sf.RemoteId = df.Data.Id;
                                        }
                                    }
                                    else if (dLocal < dRemote)
                                    {
                                        // copy remote file
                                        bytes = await odObject.GetFileBytesAsync(df.Data.DownloadLink);
                                        File.WriteAllBytes(tempFile, bytes);
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
                        var filesToCopy = filesSrc.Where(sf => sf.Upload).Select(sf => new FileInfo(sf.Path)).ToArray();
                        await odObject.UploadFilesAsync(remoteFolderId, filesToCopy);
                        var filesToReplace = filesSrc.Where(sf => sf.Replace).ToArray();
                        foreach (var rf in filesToReplace)
                        {
                            await odObject.ReplaceFileAsync(rf.RemoteId, new FileInfo(rf.Path));
                        }
                        var remoteToCopy = filesDest.Where(df => df.Copy);
                        foreach (var df in remoteToCopy)
                        {
                            var newPath = Path.Combine(srcDataDir, df.Data.Name);
                            if (!string.IsNullOrEmpty(df.TempPath))
                                File.Copy(df.TempPath, newPath, true);
                            else
                            {
                                bytes = await odObject.GetFileBytesAsync(df.Data.DownloadLink);
                                File.WriteAllBytes(newPath, bytes);
                            }
                        }
                        if (filesDest.Count(df => df.Copy) == 0) result = SyncResult.None;
                        // copy synchronized db files
                        File.Copy(tempDBSrc, srcDBPath, true);
                        //rename remote file in order to upload file with needed name
                        File.Delete(tempDBSrc);
                        File.Move(tempDBDest, tempDBSrc);
                        await odObject.ReplaceFileAsync(destDB.Id, new FileInfo(tempDBSrc));
                    }
                    else
                    {
                        // restore triggers
                        eSrc.Execute(createTriggers);
                        // just copy all notes files and db file to remote server
                        await odObject.UploadFileAsync(remoteFolderId, new FileInfo(tempDBSrc));
                        await odObject.UploadFilesAsync(remoteFolderId,
                            filesSrc.Select(fs => new FileInfo(fs.Path)).ToArray());
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

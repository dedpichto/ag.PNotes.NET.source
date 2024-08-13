using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Box.NET
{
    /// <summary>
    /// Represents class for working with Box server
    /// </summary>
    public class BoxObject
    {
        private class _BObject
        {
            public string Name;
            public string Id;
            public string Type;
        }

        /// <summary>
        /// Creates new instance of BoxObject
        /// </summary>
        /// <param name="clientId">Application Client ID</param>
        /// <param name="clientSecret">Application Client Secret</param>
        public BoxObject(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        #region Public propertiies
        /// <summary>
        /// Gets or sets consumer key
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Gets or sets consumer secret
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// Gets or sets authentication code
        /// </summary>
        public string AuthCode { get; set; }
        /// <summary>
        /// Gets or sets authentication token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Gets or sets refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Gets or sets token type
        /// </summary>
        public string TokenType { get; set; }
        /// <summary>
        /// Gets access token expiration
        /// </summary>
        public DateTime AccessTokenExpiration { get; private set; }
        /// <summary>
        /// Gets refresh toke expiration
        /// </summary>
        public DateTime RefreshTokenExpiration { get; private set; }

        #endregion

        /// <summary>
        /// Authorizes application on LinkedIn
        /// </summary>
        /// <returns>True if authorization was successfull</returns>
        public bool Authorize()
        {
            if (string.IsNullOrEmpty(ClientId))
                throw new Exception("The consumer key is not set");
            if (string.IsNullOrEmpty(ClientSecret))
                throw new Exception("The consumer secret is not set");
            if (string.IsNullOrEmpty(getAuthCode()))
                throw new Exception("Failed to get authorize code");
            if (string.IsNullOrEmpty(getAccessToken()))
                throw new Exception("Failed to get access token");
            return true;
        }

        /// <summary>
        /// Refreshes access token
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        public bool RefreshAccessToken()
        {
            var data = "grant_type=refresh_token";
            data += "&refresh_token=";
            data += RefreshToken;
            data += "&client_id=";
            data += ClientId;
            data += "&client_secret=";
            data += ClientSecret;
            if (string.IsNullOrEmpty(getToken(data)))
                throw new Exception("Failed to get access token");
            return true;
        }

        /// <summary>
        /// Uploads file to Box server
        /// </summary>
        /// <param name="file">File info</param>
        /// <param name="folderId">Parent folder id</param>
        /// <returns>Box server response</returns>
        public string UploadFile(FileInfo file, string folderId)
        {
            try
            {
                var bu = new BoxUploader(AccessToken);
                return bu.UploadFile(file, folderId);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(409)")) //Conflict
                {
                    throw new BoxAlreadyExistsException(ex, "The file with the same name already exists in selected folder");
                }
                throw;
            }
        }

        /// <summary>
        /// Uploads multiple files to Box server
        /// </summary>
        /// <param name="files">Array of FileInfo objects</param>
        /// <param name="folderId">Parent folder id</param>
        /// <returns>Box server response</returns>
        public string[] UploadFiles(IList<FileInfo> files, string folderId)
        {
            try
            {
                var bu = new BoxUploader(AccessToken);
                return bu.UploadFiles(files, folderId);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(409)")) //Conflict
                {
                    throw new BoxAlreadyExistsException(ex, "The file with the same name already exists in selected folder");
                }
                throw;
            }
        }

        /// <summary>
        /// Checks whether folder with specified path exists
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <returns>True if folder exists, false otherwise</returns>
        public bool FolderExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var names = path.Split('/').ToList();
            names.RemoveAll(s => s == "." || s == "");
            var folders = getFolderFolders("0");
            if (folders == null) return false;
            for (var i = 0; i < names.Count; i++)
            {
                if (!folders.Keys.Contains(names[i]))
                {
                    return false;
                }
                if (i == names.Count - 1)
                {
                    return true;
                }
                folders = getFolderFolders(folders[names[i]]);
            }
            return false;
        }

        /// <summary>
        /// Checks whether file with specified path exists
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>True if file exists, false otherwise</returns>
        public bool FileExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var names = path.Split('/').ToList();
            names.RemoveAll(s => s == "." || s == "");
            var bobjects = getFolderContent("0");
            if (bobjects == null) return false;
            if (bobjects.Any(bo => bo.Type == "file" && bo.Name == names[names.Count - 1]))
                return true;
            for (var i = 0; i < names.Count - 1; i++)
            {
                var current = bobjects.FirstOrDefault(bo => bo.Type == "folder" && bo.Name == names[i]);
                if (current == null)
                    return false;
                bobjects = getFolderContent(current.Id);
                if (bobjects.Any(bo => bo.Type == "file" && bo.Name == names[names.Count - 1]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets file metadata by file's Id
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <returns>BoxMetaData object with file properties</returns>
        public BoxMetaData GetFileMetaDataById(string fileId)
        {
            return getFileFull(fileId);
        }

        /// <summary>
        /// Gets file metadata by file's path
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>BoxMetaData object with file properties</returns>
        public BoxMetaData GetFileMetaDataByName(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var names = path.Split('/').ToList();
            names.RemoveAll(s => s == "." || s == "");
            var bobjects = getFolderContent("0");
            if (bobjects == null) return null;
            var file = bobjects.FirstOrDefault(bo => bo.Type == "file" && bo.Name == names[names.Count - 1]);
            if (file != null)
                return getFileFull(file.Id);
            for (var i = 0; i < names.Count - 1; i++)
            {
                var current = bobjects.FirstOrDefault(bo => bo.Type == "folder" && bo.Name == names[i]);
                if (current == null)
                    return null;
                bobjects = getFolderContent(current.Id);
                file = bobjects.FirstOrDefault(bo => bo.Type == "file" && bo.Name == names[names.Count - 1]);
                if (file != null)
                    return getFileFull(file.Id);
            }
            return null;
        }

        /// <summary>
        /// Gets folder metadata by folder's Id
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        /// <returns>BoxMetaData object with folder properties</returns>
        public BoxMetaData GetFolderMetaDataById(string folderId)
        {
            return getFolderFull(folderId);
        }

        /// <summary>
        /// Gets folder metadata by folder's path
        /// </summary>
        /// <param name="path">Path to folder</param>
        /// <returns>BoxMetaData object with folder properties</returns>
        public BoxMetaData GetFolderMetaDataByName(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var names = path.Split('/').ToList();
            names.RemoveAll(s => s == "." || s == "");
            var folders = getFolderFolders("0");
            if (folders == null) return null;
            for (var i = 0; i < names.Count; i++)
            {
                if (!folders.Keys.Contains(names[i]))
                {
                    return null;
                }
                if (i == names.Count - 1)
                {
                    return getFolderFull(folders[names[i]]);
                }
                folders = getFolderFolders(folders[names[i]]);
            }
            return null;
        }

        /// <summary>
        /// Creates new folder
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="parentId">Parent folder Id</param>
        /// <returns>BoxMetaData object with new folder properties</returns>
        public BoxMetaData CreateFolder(string folderName, string parentId)
        {
            try
            {
                var data = "{\"name\":\"" + folderName + "\", \"parent\": {\"id\": \"" + parentId + "\"}}";
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(BoxStatic.FOLDERS_URL, "POST", data, new[] { "Authorization: Bearer " + AccessToken }));
                var jo =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) as JObject;
                return buildFolder(jo);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(409)")) //Conflict
                {
                    throw new BoxAlreadyExistsException(ex, "The folder with the same name already exists at this parent");
                }
                throw;
            }
        }

        /// <summary>
        /// Deletes folder
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        public void DeleteFolder(string folderId)
        {
            var url = BoxStatic.GET_FOLDER_FULL_URL;
            url = url.Replace("FOLDER_ID", folderId);
            url += "?recursive=true";
            BoxStatic.makeRequestString(BoxStatic.createRequest(url, "DELETE", null, new[] { "Authorization: Bearer " + AccessToken }));
        }

        /// <summary>
        /// Deletes file
        /// </summary>
        /// <param name="fileId">File Id</param>
        public void DeleteFile(string fileId)
        {
            var url = BoxStatic.GET_FILE_FULL_URL;
            url = url.Replace("FILE_ID", fileId);
            BoxStatic.makeRequestString(BoxStatic.createRequest(url, "DELETE", null, new[] { "Authorization: Bearer " + AccessToken }));
        }

        /// <summary>
        /// Gets file content as bytes array
        /// </summary>
        /// <param name="fileId">File Id</param>
        /// <returns>Bytes array</returns>
        public byte[] GetFile(string fileId)
        {
            var url = BoxStatic.GET_FILE_CONTENT_URL;
            url = url.Replace("FILE_ID", fileId);
            return BoxStatic.makeRequestBytes(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
        }

        private BoxMetaData buildFile(IDictionary<string, JToken> jo)
        {
            if (jo != null)
            {
                var file = new BoxMetaData
                    {
                        IsFile = true,
                        Id = jo["id"].ToString(),
                        Name = jo["name"].ToString(),
                        CreatedAt = DateTime.Parse(jo["created_at"].ToString()),
                        ModifiedAt = DateTime.Parse(jo["modified_at"].ToString()),
                        //ContentCreatedAt = DateTime.Parse(jo["content_created_at"].ToString()),
                        //ContentModifiedAt = DateTime.Parse(jo["content_modified_at"].ToString()),
                        Description = jo["description"].ToString(),
                        Size = int.Parse(jo["size"].ToString()),
                        IsDeleted = jo["item_status"].ToString() == "active"
                    };
                var parent = jo["parent"];
                if (parent != null)
                {
                    file.Parent = new BoxMetaData
                    {
                        IsFolder = true,
                        Id = parent["id"].ToString(),
                        Name = parent["name"].ToString()
                    };
                }
                return file;
            }
            return null;
        }

        private BoxMetaData buildFolder(IDictionary<string, JToken> jo)
        {
            if (jo != null)
            {
                var folder = new BoxMetaData
                {
                    IsFolder = true,
                    Id = jo["id"].ToString(),
                    Name = jo["name"].ToString(),
                    CreatedAt = DateTime.Parse(jo["created_at"].ToString()),
                    ModifiedAt = DateTime.Parse(jo["modified_at"].ToString()),
                    Description = jo["description"].ToString(),
                    Size = int.Parse(jo["size"].ToString()),
                    IsDeleted = jo["item_status"].ToString() == "active"
                };
                var parent = jo["parent"];
                if (parent != null)
                {
                    folder.Parent = new BoxMetaData
                    {
                        IsFolder = true,
                        Id = parent["id"].ToString(),
                        Name = parent["name"].ToString()
                    };
                }
                var itemCollection = jo["item_collection"];
                var entries = itemCollection?["entries"];
                if (entries != null)
                {
                    folder.Items = entries.Select(e => new BoxMetaData
                    {
                        Id = e["id"].ToString(),
                        Name = e["name"].ToString(),
                        IsFolder = e["type"].ToString() == "folder",
                        IsFile = e["type"].ToString() == "file"
                    }).ToArray();
                }
                return folder;
            }
            return null;
        }

        private BoxMetaData getFileFull(string fileId)
        {
            try
            {
                var url = BoxStatic.GET_FILE_FULL_URL;
                url = url.Replace("FILE_ID", fileId);
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
                var jo =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) as JObject;
                return buildFile(jo);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))// Not Found
                {
                    throw new BoxNotFoundException(ex, "File width id = " + fileId + " not found");
                }
                throw;
            }
        }

        private BoxMetaData getFolderFull(string folderId)
        {
            try
            {
                var url = BoxStatic.GET_FOLDER_FULL_URL;
                url = url.Replace("FOLDER_ID", folderId);
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
                var jo =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) as JObject;
                return buildFolder(jo);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))// Not Found
                {
                    throw new BoxNotFoundException(ex, "Folder width id = " + folderId + " not found");
                }
                throw;
            }
        }

        private List<_BObject> getFolderContent(string folderId)
        {
            try
            {
                var result = new List<_BObject>();
                var url = BoxStatic.GET_FOLDER_CONTENT_URL;
                url = url.Replace("FOLDER_ID", folderId);
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
                if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject dict)
                {
                    var entries = dict["entries"];
                    if (entries != null)
                    {
                        result.AddRange(entries.Select(en => new _BObject
                            {
                                Name = en.SelectToken("name").ToString(),
                                Id = en.SelectToken("id").ToString(),
                                Type = en.SelectToken("type").ToString()
                            }));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))// Not Found
                {
                    throw new BoxNotFoundException(ex, "Folder width id = " + folderId + " not found");
                }
                throw;
            }
        }

        private Dictionary<string, string> getFolderFolders(string folderId)
        {
            try
            {
                var result = new Dictionary<string, string>();
                var url = BoxStatic.GET_FOLDER_CONTENT_URL;
                url = url.Replace("FOLDER_ID", folderId);
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
                if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject dict)
                {
                    var entries = dict["entries"];
                    if (entries != null)
                    {
                        foreach (var en in entries.Where(e => e.SelectToken("type").ToString() == "folder"))
                        {
                            result.Add(en.SelectToken("name").ToString(), en.SelectToken("id").ToString());
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))// Not Found
                {
                    throw new BoxNotFoundException(ex, "Folder width id = " + folderId + " not found");
                }
                throw;
            }
        }

        /// <summary>
        /// Authorizes the app by showing the dialog
        /// </summary>
        /// <returns>The authorization code.</returns>
        private String getAuthCode()
        {
            var aw = new DlgAuth(AuthorizationLink);
            aw.ShowDialog();
            AuthCode = aw.Code;
            return AuthCode;
        }

        /// <summary>
        /// Gets access token and access token's expiration date
        /// </summary>
        /// <returns>The access token</returns>
        private string getAccessToken()
        {
            var data = "grant_type=authorization_code&code=";
            data += AuthCode;
            data += "&client_id=";
            data += ClientId;
            data += "&client_secret=";
            data += ClientSecret;
            data += "&redirect_uri=";
            data += Utils.CALLBACK;

            return getToken(data);
        }

        private string getToken(string data)
        {
            var responseData = BoxStatic.makeRequestString(BoxStatic.createRequest(BoxStatic.ACCESS_TOKEN_URL, "POST", data));
            //elliminate restricted_to array
            responseData = responseData.Replace("[]", "\"[]\"");
            var dict =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
            if (dict["expires_in"] != null && dict["access_token"] != null && dict["token_type"] != null &&
                dict["refresh_token"] != null)
            {
                AccessTokenExpiration = DateTime.Now.AddSeconds(Convert.ToDouble(dict["expires_in"]));
                AccessToken = dict["access_token"];
                TokenType = dict["token_type"];
                RefreshToken = dict["refresh_token"];
                RefreshTokenExpiration = DateTime.Now.AddDays(14);
            }
            else if (dict["error"] != null)
            {
                if (dict["error_description"] != null)
                    throw new Exception(dict["error"] + ": " + dict["error_description"]);
                throw new Exception(dict["error"]);
            }
            return AccessToken;
        }

        /// <summary>
        /// Gets the link to Box's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid parameters.</returns>
        private string AuthorizationLink => BoxStatic.AUTHORIZATION_URL + ClientId + "&state=" + BoxStatic.STATE + "&redirect_uri=" + Utils.CALLBACK;
    }
}

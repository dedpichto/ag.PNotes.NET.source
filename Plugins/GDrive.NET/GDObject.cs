using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GDrive.NET
{
    /// <summary>
    /// Represents object for working with Google Drive REST API
    /// </summary>
    public class GDObject
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Access token expiration
        /// </summary>
        public DateTime Expiration { get; set; }
        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; private set; }

        private readonly string _ClientId, _ClientSecret;

        /// <summary>
        /// Creates new instance of GDObject
        /// </summary>
        /// <param name="clientId">Client Id of calling application</param>
        /// <param name="clientsecret">Client secret of calling application</param>
        public GDObject(string clientId, string clientsecret)
        {
            _ClientId = clientId;
            _ClientSecret = clientsecret;
        }

        /// <summary>
        /// Authenticates user against Google server
        /// </summary>
        /// <returns>True if authentication was successfull, false otherwise</returns>
        public bool Authenticate()
        {
            var url = GDStatic.AUTHORIZATION_URL;
            url =
                url.Replace("REDIRECT_URL", GDStatic.REDIRECT_URL)
                   .Replace("AUTH_STATE", GDStatic.AUTH_STATE)
                   .Replace("CLIENT_ID", _ClientId);
            var dlgAuth = new DlgAuth(url);
            var result = dlgAuth.ShowDialog();
            if (result == null || !result.Value) return false;
            var code = dlgAuth.Code;
            return !string.IsNullOrEmpty(getAccessToken(code));
        }

        /// <summary>
        /// Refreshes access token
        /// </summary>
        /// <returns>True if token has been refreshed successfully, false otherwise</returns>
        public bool RefreshAccessToken()
        {
            var data = "grant_type=refresh_token";
            data += "&refresh_token=";
            data += RefreshToken;
            data += "&client_id=";
            data += _ClientId;
            data += "&client_secret=";
            data += _ClientSecret;
            if (string.IsNullOrEmpty(getToken(data)))
                throw new Exception("Failed to get access token");
            return true;
        }

        /// <summary>
        /// Gets file id on Google drive server
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="parentId">Parent folder id</param>
        /// <returns>File id on Google drive server</returns>
        public string GetFileId(string fileName, string parentId)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException(@"File name cannot be empty", nameof(fileName));
            if( string.IsNullOrEmpty(parentId))
                throw new ArgumentException(@"Parent id cannot be empty", nameof(parentId));
            var url = new StringBuilder(GDStatic.FILES_URL);
            url.Append("?q=title%3D'");
            url.Append(fileName);
            url.Append("'+and+'");
            url.Append(parentId);
            url.Append("'+in+parents+and+trashed%3Dfalse");
            var responseData =
                    GDStatic.makeRequestString(GDStatic.createRequest(url.ToString(), "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject jo)
            {
                var items = jo["items"];
                if (items == null || !items.Any()) return "";
                var file =
                    items.FirstOrDefault(
                        i =>
                        i["title"].ToString() == fileName);

                if (file?["id"] != null)
                    return file["id"].ToString();
            }
            return "";
        }

        /// <summary>
        /// Gets folder id on Google Drive server
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="parentId">Parent folder id (should be empty string for root folder)</param>
        /// <returns>Folder id on Google Drive server</returns>
        public string GetFolderId(string folderName, string parentId = "")
        {
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException(@"Folder name cannot be empty", nameof(folderName));
            var url = new StringBuilder(GDStatic.FILES_URL);
            url.Append("?q=title%3D'");
            url.Append(folderName);
            url.Append("'+and+trashed%3Dfalse");
            if (parentId != "")
            {
                url.Append("+and+'");
                url.Append(parentId);
                url.Append("'+in+parents");
            }
            var responseData =
                    GDStatic.makeRequestString(GDStatic.createRequest(url.ToString(), "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject jo)
            {
                var items = jo["items"];
                if (items == null || !items.Any()) return "";
                var folder =
                    items.FirstOrDefault(
                        i =>
                        i["mimeType"].ToString() == "application/vnd.google-apps.folder" &&
                        i["title"].ToString() == folderName);

                if (folder?["id"] != null)
                    return folder["id"].ToString();
            }
            return "";
        }

        /// <summary>
        /// Gets folder content
        /// </summary>
        /// <param name="folderId">Folder id</param>
        /// <returns>Folder content</returns>
        public GDMetaData[] GetFolderContent(string folderId)
        {
            var data = new List<GDMetaData>();
            if (string.IsNullOrEmpty(folderId))
                throw new ArgumentException(@"Folder id cannot be empty", nameof(folderId));
            var url = new StringBuilder(GDStatic.FILES_URL);
            url.Append("?q='");
            url.Append(folderId);
            url.Append(
                "'+in+parents+and+trashed%3Dfalse&fields=items(downloadUrl%2CfileExtension%2Cid%2CmimeType%2CmodifiedByMeDate%2Cparents%2Fid%2Ctitle)");
            var responseData =
                    GDStatic.makeRequestString(GDStatic.createRequest(url.ToString(), "GET", null, new[] { "Authorization: Bearer " + AccessToken }));
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject jo)
            {
                var items = jo["items"];
                if (items == null || !items.Any()) return data.ToArray();
                foreach (var ji in items)
                {
                    var gdm = new GDMetaData
                        {
                            Id = ji["id"].ToString(),
                            Name = ji["title"].ToString(),
                            LastModified = DateTime.Parse(ji["modifiedByMeDate"].ToString()),
                            FileExtension = ji["fileExtension"].ToString(),
                            DownloadUrl = ji["downloadUrl"].ToString()
                        };
                    if (ji["mimeType"].ToString() == "application/vnd.google-apps.folder")
                        gdm.IsFolder = true;
                    if (ji["parents"] != null && ji["parents"].HasValues)
                        gdm.ParentId = ji["parents"].First["id"].ToString();
                    data.Add(gdm);
                }
            }
            return data.ToArray();
        }

        /// <summary>
        /// Gets file content as byte array
        /// </summary>
        /// <param name="downloadUrl">Download URL</param>
        /// <returns>File content as byte array</returns>
        public byte[] GetFile(string downloadUrl)
        {
            if (string.IsNullOrEmpty(downloadUrl))
                throw new ArgumentException(@"Download URL cannot be empty", nameof(downloadUrl));
            return
                GDStatic.makeRequestBytes(GDStatic.createRequest(downloadUrl, "GET", null,
                                                                 new[] {"Authorization: Bearer " + AccessToken}));
        }

        /// <summary>
        /// Creates new folder on Google Drive server
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="parentId">Parent folder id</param>
        /// <returns>New folder id</returns>
        public string CreateFolder(string folderName, string parentId)
        {
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentException(@"Folder name cannot be empty", nameof(folderName));
            if(string.IsNullOrEmpty(parentId))
                throw new ArgumentException(@"Parent id cannot be empty", nameof(parentId));
            var jo = new JObject();
            var ja = new JArray();
            jo.Add("title", folderName);
            var ji = new JObject { { "id", parentId } };
            ja.Add(ji);
            jo.Add("parents", ja);
            jo.Add("mimeType", "application/vnd.google-apps.folder");

            var responseData =
                GDStatic.makeRequestString(GDStatic.createRequest(GDStatic.FILES_URL, "POST", jo.ToString(),
                                                                  new[] { "Authorization: Bearer " + AccessToken },
                                                                  "application/json"));
            jo =
                Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) as JObject;
            if (jo != null && jo.HasValues)
            {
                if (jo["id"] != null)
                    return jo["id"].ToString();
            }
            return "";
        }

        /// <summary>
        /// Deletes object from Google Drive server
        /// </summary>
        /// <param name="objectId">Object id</param>
        /// <returns>Object id</returns>
        /// <remarks>Do not use this method, deleted objects remain in the folder without ability to delete them (and in synchronized folders on computer</remarks>
        public string DeleteObject(string objectId)
        {
            if (string.IsNullOrEmpty(objectId))
                throw new ArgumentException(@"Object id cannot be empty", nameof(objectId));
            var result = TrashObject(objectId);
            var url = new StringBuilder(GDStatic.FILES_URL);
            url.Append("/");
            url.Append(objectId);
            GDStatic.makeRequestString(GDStatic.createRequest(url.ToString(), "DELETE", null,
                                                              new[] { "Authorization: Bearer " + AccessToken }));
            return result;
        }

        /// <summary>
        /// Moves object to trash
        /// </summary>
        /// <param name="objectId">Object id</param>
        /// <returns>Object id</returns>
        public string TrashObject(string objectId)
        {
            if(string.IsNullOrEmpty(objectId))
                throw new ArgumentException(@"Object id cannot be empty", nameof(objectId));
            var url = new StringBuilder(GDStatic.FILES_URL);
            url.Append("/");
            url.Append(objectId);
            url.Append("/");
            url.Append("trash");
            var responseData =
                    GDStatic.makeRequestString(GDStatic.createRequest(url.ToString(), "POST", null, new[] { "Authorization: Bearer " + AccessToken }));
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject jo && jo.HasValues)
            {
                if (jo["id"] != null)
                    return jo["id"].ToString();
            }
            return "";
        }

        /// <summary>
        /// Uploads file to Google Drive server
        /// </summary>
        /// <param name="fi">FileInfo object</param>
        /// <param name="parentId">Parent folder id</param>
        /// <returns>Uploaded file id</returns>
        public string UploadFile(FileInfo fi, string parentId)
        {
            if (fi == null)
                throw new ArgumentNullException(nameof(fi), @"FileInfo object cannot be null reference");
            if (string.IsNullOrEmpty(parentId))
                throw new ArgumentException(@"Parent id cannot be empty", nameof(parentId));
            var gdu = new GDUploader(AccessToken);
            var sb = new StringBuilder(GDStatic.UPLOAD_URL);
            var fileId = GetFileId(fi.Name, parentId);
            sb.Append("?uploadType=multipart&setModifiedDate=true");
            if (fileId != "")
            {
                TrashObject(fileId);
            }
            var responseData = gdu.UploadFile(fi, parentId, sb.ToString());
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject jo && jo.HasValues)
            {
                if (jo["id"] != null)
                    return jo["id"].ToString();
            }
            return "";
        }

        /// <summary>
        /// Uploads multiple files to Google Drive server
        /// </summary>
        /// <param name="files">FileInfo objects</param>
        /// <param name="parentId">Parent folder id</param>
        /// <returns>Uploaded files ids</returns>
        public string[] UploadFiles(FileInfo[] files, string parentId)
        {
            return files.Select(fi => UploadFile(fi, parentId)).ToArray();
        }

        private string getAccessToken(string code)
        {
            var data = "grant_type=authorization_code&code=";
            data += code;
            data += "&client_id=";
            data += _ClientId;
            data += "&client_secret=";
            data += _ClientSecret;
            data += "&redirect_uri=";
            data += GDStatic.REDIRECT_URL;

            return getToken(data);
        }

        private string getToken(string data)
        {
            var responseData = GDStatic.makeRequestString(GDStatic.createRequest(GDStatic.ACCESS_TOKEN_URL, "POST", data));
            var dict =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseData);
            if (dict.ContainsKey("error") && !string.IsNullOrEmpty(dict["error"]))
            {
                if (dict.ContainsKey("error_description") && !string.IsNullOrEmpty(dict["error_description"]))
                    throw new Exception(dict["error"] + ": " + dict["error_description"]);
                throw new Exception(dict["error"]);
            }
            if (dict.ContainsKey("access_token") && !string.IsNullOrEmpty(dict["access_token"]))
                AccessToken = dict["access_token"];
            if (dict.ContainsKey("refresh_token") && !string.IsNullOrEmpty(dict["refresh_token"]))
                RefreshToken = dict["refresh_token"];
            if (dict.ContainsKey("expires_in") && !string.IsNullOrEmpty(dict["expires_in"]))
                Expiration = DateTime.Now.AddSeconds(Convert.ToDouble(dict["expires_in"]));
            if (dict.ContainsKey("token_type") && !string.IsNullOrEmpty(dict["token_type"]))
                TokenType = dict["token_type"];

            return AccessToken;
        }
    }
}

using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ODrive.NET
{
    /// <summary>
    /// Represents object for working with Microsoft Graph (OneDrive) APIs
    /// </summary>
    public class ODObject
    {
        private readonly App _app;
        private string _accessToken;

        /// <summary>
        /// Creates new instance of ODObject
        /// </summary>
        public ODObject()
        {
            _app = new App(ODStatic.ClientId);
        }

        /// <summary>
        /// Authenticates user against OneDrive server
        /// </summary>
        /// <returns>True, if authentication was successful, false otherwise</returns>
        /// <exception cref="ODException"></exception>
        public async Task<bool> AuthenticateAsync()
        {
            AuthenticationResult authResult;
            var app = _app.PublicClientApp;

            var accounts = await app.GetAccountsAsync();

            try
            {
                authResult = await app.AcquireTokenSilentAsync(ODStatic.Scopes, accounts.FirstOrDefault());
            }
            catch (MsalUiRequiredException)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                // This indicates you need to call AcquireTokenAsync to acquire a token
                try
                {
                    authResult = await app.AcquireTokenAsync(ODStatic.Scopes);
                }
                catch (MsalException msalex)
                {
                    throw new ODException(msalex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new ODException(ex.Message);
            }

            if (authResult == null) return false;
            _accessToken = authResult.AccessToken;
            return true;

        }

        private string prepareErrorMessage(string message, HttpStatusCode statusCode, [CallerMemberName] string caller = "")
        {
            return $"Method: {caller}; Status code: {statusCode}; {message}";
        }

        /// <summary>
        /// Gets OneDrive folder ID
        /// </summary>
        /// <param name="folderName">Folder name to retrieve the ID for</param>
        /// <returns>ID of specified folder or empty string, if folder does not exist</returns>
        public async Task<string> GetFolderIdAsync(string folderName)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var folderUrl = $"{ODStatic.FolderUrl}{folderName}";
                    using (var request = new HttpRequestMessage(HttpMethod.Get, folderUrl))
                    {
                        //Add the token in Authorization header
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                        using (var response = await httpClient.SendAsync(request))
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    {
                                        var auth = await AuthenticateAsync();
                                        if (!auth)
                                        {
                                            throw new ODException("Authorization failed");
                                        }

                                        continue;
                                    }
                                case HttpStatusCode.NotFound:
                                    return "";
                                case HttpStatusCode.OK:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data) is JObject jo &&
                                            jo.HasValues)
                                        {
                                            return jo["id"] != null ? jo["id"].ToString() : "";
                                        }

                                        throw new ODException(prepareErrorMessage(
                                            $"No data in response, the response is: {data}", response.StatusCode));
                                    }
                                default:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        throw new ODException(prepareErrorMessage(
                                            $"Request error, the response is: {data}", response.StatusCode));
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Replaces existing file content on OneDrive
        /// </summary>
        /// <param name="id">File ID</param>
        /// <param name="fileInfo">FileInfo object to get content from</param>
        /// <returns>File ID, if content successfully replaced</returns>
        public async Task<string> ReplaceFileAsync(string id, FileInfo fileInfo)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var requestUri = ODStatic.ReplaceUrl.Replace(ODStatic.ITEM_ID, id);
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                    {
                        using (var streamContent = new StreamContent(fs))
                        {
                            httpClient.DefaultRequestHeaders.Clear();
                            httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", _accessToken);
                            using (var response = await httpClient.PutAsync(requestUri, streamContent))
                            {
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.Unauthorized:
                                        {
                                            var auth = await AuthenticateAsync();
                                            if (!auth)
                                            {
                                                throw new ODException("Authorization failed");
                                            }

                                            continue;
                                        }
                                    case HttpStatusCode.OK:
                                        {
                                            var data = await response.Content.ReadAsStringAsync();
                                            if (Newtonsoft.Json.JsonConvert
                                                    .DeserializeObject<dynamic>(data) is JObject jo &&
                                                jo.HasValues)
                                            {
                                                return jo["id"] != null ? jo["id"].ToString() : "";
                                            }

                                            throw new ODException(prepareErrorMessage(
                                                $"No data in response, the response is: {data}", response.StatusCode));
                                        }
                                    default:
                                        {
                                            var data = await response.Content.ReadAsStringAsync();
                                            throw new ODException(prepareErrorMessage(
                                                $"Request error, the response is: {data}", response.StatusCode));
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads new file to OneDrive
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="fileInfo">FileInfo object to get content from</param>
        /// <returns>File ID, if file successfully uploaded</returns>
        public async Task<string> UploadFileAsync(string folderName, FileInfo fileInfo)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var requestUri = ODStatic.UploadUrl.Replace(ODStatic.FOLDER_NAME, folderName)
                        .Replace(ODStatic.FILE_NAME, fileInfo.Name);
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                    {
                        using (var streamContent = new StreamContent(fs))
                        {
                            httpClient.DefaultRequestHeaders.Clear();
                            httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", _accessToken);
                            using (var response = await httpClient.PutAsync(requestUri, streamContent))
                            {
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.Unauthorized:
                                        {
                                            var auth = await AuthenticateAsync();
                                            if (!auth)
                                            {
                                                throw new ODException("Authorization failed");
                                            }

                                            continue;
                                        }
                                    case HttpStatusCode.Created:
                                        {
                                            var data = await response.Content.ReadAsStringAsync();
                                            if (Newtonsoft.Json.JsonConvert
                                                    .DeserializeObject<dynamic>(data) is JObject jo &&
                                                jo.HasValues)
                                            {
                                                return jo["id"] != null ? jo["id"].ToString() : "";
                                            }

                                            throw new ODException(prepareErrorMessage(
                                                $"No data in response, the response is: {data}", response.StatusCode));
                                        }
                                    default:
                                        {
                                            var data = await response.Content.ReadAsStringAsync();
                                            throw new ODException(prepareErrorMessage(
                                                $"Request error, the response is: {data}", response.StatusCode));
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uploads several files to OneDrive
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <param name="files">Array of FileInfo objects</param>
        /// <returns>IDs of successfully uploaded files as strings array</returns>
        public async Task<string[]> UploadFilesAsync(string folderName, FileInfo[] files)
        {
            var list = new List<string>();
            foreach (var fi in files)
            {
                var id = await UploadFileAsync(folderName, fi);
                list.Add(id);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Creates new folder on OneDrive
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <returns>Folder name, if folder successfully created</returns>
        public async Task<string> CreateFolderAsync(string folderName)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var requestData =
                        "{\"name\":\"" + folderName +
                        "\",\"folder\":{},\"@microsoft.graph.conflictBehavior\":\"rename\"}";
                    using (var stringContent = new StringContent(requestData, Encoding.UTF8, "application/json"))
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", _accessToken);

                        using (var response = await httpClient.PostAsync(ODStatic.RootUrl, stringContent))
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    {
                                        var auth = await AuthenticateAsync();
                                        if (!auth)
                                        {
                                            throw new ODException("Authorization failed");
                                        }

                                        continue;
                                    }
                                case HttpStatusCode.Created:
                                    return folderName;
                                default:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        throw new ODException(prepareErrorMessage(
                                            $"Request error, the response is: {data}", response.StatusCode));
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets content of specified folder from OneDrive as <see cref="ODMetadata" /> objects array
        /// </summary>
        /// <param name="folderName">Folder name</param>
        /// <returns>Content of specified folder as <see cref="ODMetadata" /> objects array</returns>
        public async Task<ODMetadata[]> GetFolderContentAsync(string folderName)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var filesUrl = $"{ODStatic.FolderUrl}{folderName}:/children";
                    using (var request = new HttpRequestMessage(HttpMethod.Get, filesUrl))
                    {
                        //Add the token in Authorization header
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                        using (var response = await httpClient.SendAsync(request))
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    {
                                        var auth = await AuthenticateAsync();
                                        if (!auth)
                                        {
                                            throw new ODException("Authorization failed");
                                        }

                                        continue;
                                    }
                                case HttpStatusCode.OK:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        if (!(Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data) is JObject jo
                                                ) ||
                                            !jo.HasValues)
                                            throw new ODException(prepareErrorMessage(
                                                $"No data in response, the response is: {data}", response.StatusCode));
                                        var list = new List<ODMetadata>();
                                        if (jo["value"] == null)
                                            throw new ODException(prepareErrorMessage(
                                                $"No data in response, the response is: {data}", response.StatusCode));
                                        list.AddRange(jo["value"]
                                            .Select(ji => new ODMetadata
                                            {
                                                Id = ji["id"].ToString(),
                                                DownloadLink = ji["@microsoft.graph.downloadUrl"].ToString(),
                                                Name = ji["name"].ToString(),
                                                LastModified = DateTime.Parse(ji["lastModifiedDateTime"].ToString()),
                                                Extension = Path.GetExtension(ji["name"].ToString()),
                                                IsFile = ji["file"] != null
                                            }));

                                        return list.ToArray();
                                    }
                                default:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        throw new ODException(prepareErrorMessage(
                                            $"Request error, the response is: {data}", response.StatusCode));
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes item with specified id from OneDrive
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True on success</returns>
        public async Task<bool> DeleteItemAsync(string id)
        {
            while (true)
            {
                using (var httpClient = new HttpClient())
                {
                    var requestUri = $"{ODStatic.DeleteUrl}{id}";
                    using (var request = new HttpRequestMessage(HttpMethod.Delete, requestUri))
                    {
                        //Add the token in Authorization header
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                        using (var response = await httpClient.SendAsync(request))
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized:
                                    {
                                        var auth = await AuthenticateAsync();
                                        if (!auth)
                                        {
                                            throw new ODException("Authorization failed");
                                        }

                                        continue;
                                    }
                                case HttpStatusCode.NoContent:
                                    return true;
                                default:
                                    {
                                        var data = await response.Content.ReadAsStringAsync();
                                        throw new ODException(prepareErrorMessage(
                                            $"Request error, the response is: {data}", response.StatusCode));
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves file content as bytes array
        /// </summary>
        /// <param name="link">Link to file</param>
        /// <returns>File content as bytes array</returns>
        public async Task<byte[]> GetFileBytesAsync(string link)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, link))
                {
                    using (var response = await httpClient.SendAsync(request))
                    {
                        var result = await response.Content.ReadAsByteArrayAsync();
                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves file content as string
        /// </summary>
        /// <param name="link">Link to file</param>
        /// <returns>File content as string</returns>
        public async Task<string> GetFileStringAsync(string link)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, link))
                {
                    using (var response = await httpClient.SendAsync(request))
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
        }
    }
}

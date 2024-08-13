using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Box.NET
{
    internal sealed class BoxUploader
    {
        private enum UploadType
        {
            Upload,
            Replace
        }

        private readonly string _Boundary;
        private readonly string _AccessToken;
        private string _ReplaceUrl;

        internal BoxUploader(string token)
        {
            _Boundary = $"----------{Guid.NewGuid():N}";
            _AccessToken = token;
        }

        internal string UploadFile(FileInfo file, string folderId)
        {
            var nvc = new NameValueCollection { { "folder_id", folderId } };
            try
            {
                return uploadFile(file, nvc, UploadType.Upload);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(409)"))
                {
                    var id = getExistingFileId(folderId, file.Name);
                    if (id != "-1")
                    {
                        _ReplaceUrl = BoxStatic.REPLACE_URL;
                        _ReplaceUrl = _ReplaceUrl.Replace("FILE_ID", id);
                        return uploadFile(file, nvc, UploadType.Replace);
                    }
                }
            }
            return "-1";
        }

        internal string[] UploadFiles(IList<FileInfo> files, string folderId)
        {
            var nvc = new NameValueCollection { { "folder_id", folderId } };
            var list = new List<string>();
            foreach (var f in files)
            {
                try
                {
                    list.Add(uploadFile(f, nvc, UploadType.Upload));
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(409)"))
                    {
                        var id = getExistingFileId(folderId, f.Name);
                        if (id != "-1")
                        {
                            _ReplaceUrl = BoxStatic.REPLACE_URL;
                            _ReplaceUrl = _ReplaceUrl.Replace("FILE_ID", id);
                            list.Add(uploadFile(f, nvc, UploadType.Replace));
                        }
                    }
                }
            }
            return list.ToArray();
        }

        private HttpWebRequest createRequestUpload(long contentLength, UploadType uploadType)
        {
            HttpWebRequest webRequest;
            if (uploadType == UploadType.Upload)
                webRequest = (HttpWebRequest) WebRequest.Create(BoxStatic.UPLOAD_URL);
            else
                webRequest = (HttpWebRequest)WebRequest.Create(_ReplaceUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "multipart/form-data;boundary=" + _Boundary;
            webRequest.Headers.Add("Authorization", "Bearer " + _AccessToken);
            webRequest.Headers.Add("Accept-Encoding", "*");
            webRequest.Headers.Add("Accept-Charset", "*");
            webRequest.ContentLength = contentLength;
            webRequest.ServicePoint.ConnectionLeaseTimeout = 0;

            return webRequest;
        }

        private byte[] boundaryStart()
        {
            return Encoding.UTF8.GetBytes("\r\n--" + _Boundary + "\r\n");
        }

        private byte[] boundaryEnd()
        {
            return Encoding.UTF8.GetBytes("\r\n--" + _Boundary + "--\r\n");
        }

        private string uploadFile(FileInfo file, NameValueCollection nvc, UploadType uploadType)
        {
            HttpWebRequest webRequest;

            using (var memStream = new MemoryStream())
            {

                var boundStart = boundaryStart();
                var boundEnd = boundaryEnd();

                var dataTemplate = "\r\n--" + _Boundary +
                                   "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

                foreach (var formItemBytes in from string key in nvc.Keys
                                              select string.Format(dataTemplate, key, nvc[key])
                                                  into formItem
                                                  select Encoding.UTF8.GetBytes(formItem))
                {
                    memStream.Write(formItemBytes, 0, formItemBytes.Length);
                }

                const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";

                memStream.Write(boundStart, 0, boundStart.Length);
                var header = string.Format(headerTemplate, "file", file.Name);

                var headerbytes = Encoding.UTF8.GetBytes(header);

                memStream.Write(headerbytes, 0, headerbytes.Length);

                var fileBuffer = File.ReadAllBytes(file.FullName);
                memStream.Write(fileBuffer, 0, fileBuffer.Length);

                memStream.Write(boundEnd, 0, boundEnd.Length);

                memStream.Flush();
                webRequest = createRequestUpload(memStream.Length, uploadType);

                using (var requestStream = webRequest.GetRequestStream())
                {
                    memStream.Position = 0;
                    var tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                    //File.WriteAllBytes(@"D:\test3.txt", tempBuffer);
                }
            }

            var response = "";
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var responseStream = webResponse.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        var reader = new StreamReader(responseStream);
                        response = reader.ReadToEnd();
                    }
                }
            }
            return getFileId(response);
        }

        private string getFileId(string response)
        {
            if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(response) is JObject jo)
            {
                var entries = jo["entries"];
                if (entries?.First != null)
                {
                    return entries.First["id"].ToString();
                }
            }
            return "-1";
        }

        private string getExistingFileId(string folderId, string fileName)
        {
            try
            {
                var url = BoxStatic.GET_FOLDER_CONTENT_URL;
                url = url.Replace("FOLDER_ID", folderId);
                var responseData =
                    BoxStatic.makeRequestString(BoxStatic.createRequest(url, "GET", null, new[] { "Authorization: Bearer " + _AccessToken }));
                if (Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData) is JObject dict)
                {
                    var entries = dict["entries"];
                    var file = entries?.FirstOrDefault(f => f["name"].ToString() == fileName);
                    if (file != null)
                    {
                        return file["id"].ToString();
                    }
                }
                return "-1";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(404)"))// Not Found
                {
                    return "-1";
                }
                throw;
            }
        }
    }
}

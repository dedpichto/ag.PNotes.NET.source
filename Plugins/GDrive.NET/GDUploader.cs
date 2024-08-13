using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GDrive.NET
{
    internal class GDUploader
    {
        internal GDUploader(string accessToken)
        {
            _AccessToken = accessToken;
            _Boundary = String.Format("{0:N}", Guid.NewGuid());
        }

        private readonly string _Boundary;
        private readonly string _AccessToken;

        internal string UploadFile(FileInfo fi, string parentId, string uploadUrl)
        {
            var request = createRequestUpload(uploadUrl);
            using (var ms = new MemoryStream())
            {
                var str = "\r\n--" + _Boundary + "\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n";
                ms.Write(Encoding.UTF8.GetBytes(str), 0, str.Length);

                var jo = new JObject
                    {
                        {"title", fi.Name},
                        {"modifiedDate", fi.LastWriteTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}
                    };
                var jp = new JObject {{"id", parentId}};
                var ja = new JArray { jp };
                jo.Add("parents", ja);

                str = jo + "\r\n";
                ms.Write(Encoding.UTF8.GetBytes(str), 0, str.Length);

                str = "\r\n--" + _Boundary + "\r\nContent-Type: application/octet-stream\r\n\r\n";
                ms.Write(Encoding.UTF8.GetBytes(str), 0, str.Length);

                var bytes = File.ReadAllBytes(fi.FullName);
                ms.Write(bytes, 0, bytes.Length);

                str = "\r\n\r\n--" + _Boundary + "--";
                ms.Write(Encoding.UTF8.GetBytes(str), 0, str.Length);

                ms.Flush();
                request.ContentLength = ms.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    ms.Position = 0;
                    var tempBuffer = new byte[ms.Length];
                    ms.Read(tempBuffer, 0, tempBuffer.Length);
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }
            }
            var response = "";
            using (var webResponse = (HttpWebResponse)request.GetResponse())
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
            return response;
        }

        private HttpWebRequest createRequestUpload(string uploadUrl)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(uploadUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "multipart/related;boundary=" + _Boundary;
            webRequest.Headers.Add("Authorization", "Bearer " + _AccessToken);
            webRequest.Headers.Add("Accept-Encoding", "*");
            webRequest.Headers.Add("Accept-Charset", "*");
            webRequest.ServicePoint.ConnectionLeaseTimeout = 0;

            return webRequest;
        }
    }
}

// Implements multipart/form-data POST in C# http://www.ietf.org/rfc/rfc2388.txt
// http://www.briangrinstead.com/blog/multipart-form-post-in-c

using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Generic;

public static class FormUpload
{
    private static readonly Encoding encoding = Encoding.UTF8;

    public static float uploadProgress = 0;

    public static void RequestPost(
        string url, string authorization, Dictionary<string, object> postParameters,
        Action<string> callback)
    {
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
        string contentType = "multipart/form-data; boundary=" + formDataBoundary;

        byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

        var t = new Thread(() =>
        {
            try
            {
                var resp = PostForm(url, authorization, contentType, formData);
                var reader = new StreamReader(resp.GetResponseStream());
                var data = reader.ReadToEnd();

                UnityEngine.Debug.Log(data);

                callback(data);
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                callback(null);
            }
        });
        t.Start();
    }
    private static HttpWebResponse PostForm(string url, string authorization, string contentType, byte[] formData)
    {
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

        if (request == null)
            throw new NullReferenceException("request is not a http request");

        request.Method = "POST";
        request.ContentType = contentType;
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        request.Headers.Add("Authorization", authorization);

        using (Stream requestStream = request.GetRequestStream())
        {
            var offset = 0;
            var written = 0;

            uploadProgress = 0;
            while (offset < formData.Length)
            {
                var length = Math.Min(formData.Length - offset, 1024);
                requestStream.Write(formData, offset, length);
                offset += length;
                written += length;

                uploadProgress = ((float)offset) / formData.Length;
            }

            UnityEngine.Debug.Log("WRITTEN : " + written + " / " + formData.Length);

            requestStream.Close();
        }

        return request.GetResponse() as HttpWebResponse;
    }

    private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    {
        Stream formDataStream = new MemoryStream();
        bool needsCLRF = false;

        foreach (var param in postParameters)
        {
            // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
            // Skip it on the first parameter, add it to subsequent parameters.
            if (needsCLRF)
                formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

            needsCLRF = true;

            if (param.Value is FileParameter)
            {
                FileParameter fileToUpload = (FileParameter)param.Value;

                // Add just the first part of this param, since we will write the file data directly to the Stream
                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                    boundary,
                    param.Key,
                    fileToUpload.FileName ?? param.Key,
                    fileToUpload.ContentType ?? "application/octet-stream");

                formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                // Write the file data directly to the Stream, rather than serializing it to a string.
                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
            }
            else
            {
                string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    boundary,
                    param.Key,
                    param.Value);
                formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
            }
        }

        // Add the end of the request.  Start with a newline
        string footer = "\r\n--" + boundary + "--\r\n";
        formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

        // Dump the Stream into a byte[]
        formDataStream.Position = 0;
        byte[] formData = new byte[formDataStream.Length];
        formDataStream.Read(formData, 0, formData.Length);
        formDataStream.Close();

        return formData;
    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }
}

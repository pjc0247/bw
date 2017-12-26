using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Applivery
{
    private static readonly string ApiKey = "";

    public struct AppData
    {
        public string appId;
        public string versionName;
        public string notes;
        public bool notify;
        public string os;
        public string tags;
        public bool autoremove;
    }

    private static Action<bool> callback;

    public static void UploadBuild(string apkPath, AppData data, Action<bool> _callback)
    {
        if (File.Exists(apkPath) == false)
            throw new ArgumentException("apkPath");

        FormUpload.RequestPost(
            "https://dashboard.applivery.com/api/builds",
            ApiKey,
            new Dictionary<string, object>()
            {
                {"app", data.appId },
                {"versionName", data.versionName },
                {"notify", true },
                {"os", data.os },
                {"tags", data.tags },
                {"autoremove", true },
                {"package", new FormUpload.FileParameter(File.ReadAllBytes(apkPath), "binary.apk") }
            },
            (json) =>
            {
                EditorApplication.update -= Update;

                callback(json != null);
            });
        callback = _callback;

        EditorApplication.update += Update;
    }

    private static void Update()
    {
        Debug.Log("UPDATE: " + FormUpload.uploadProgress);
    }
}

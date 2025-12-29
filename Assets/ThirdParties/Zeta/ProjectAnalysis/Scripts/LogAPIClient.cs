using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace Zeta.ProjectAnalysis
{
    [DefaultExecutionOrder(-300)]
    public class LogAPIClient : MonoBehaviour
    {
        protected static LogAPIClient _instance;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                Application.logMessageReceived += Application_logMessageReceived;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log || type == LogType.Warning)
                return;

            var attributes = new Dictionary<string, string>();

            attributes.Add("userData", GetUserDataJson());
            attributes.Add("levelName", GetCurrentLevelName());
            attributes.Add("iap", GetIAPList());
            attributes.Add("project", CoreAPIClient.Instance.Configuration.project_id);
            attributes.Add("type", type.ToString());
            attributes.Add("condition", condition);
            attributes.Add("trace", string.IsNullOrEmpty(stackTrace) ? new System.Diagnostics.StackTrace().ToString() : stackTrace);
            attributes.Add("version", Application.version);
            attributes.Add("platform", Application.platform.ToString());
            attributes.Add("deviceModel", SystemInfo.deviceModel);
            attributes.Add("deviceOS", SystemInfo.operatingSystem);
            attributes.Add("createdAt", DateTime.UtcNow.ToString("o"));

            var www = UnityWebRequest.Post(Path.Combine(CoreAPIClient.Instance.Configuration.base_url, "log"), attributes);
            www.SetRequestHeader("Authorization", "Bearer " + CoreAPIClient.Instance.Configuration.api_key);
            www.SendWebRequest();
        }

        protected virtual string GetUserDataJson()
        {
            return PlayerPrefs.GetString("USER_DATA");
        }

        protected virtual string GetCurrentLevelName()
        {
            return string.Empty;
        }

        protected virtual string GetIAPList()
        {
            return string.Empty;
        }
    }
}
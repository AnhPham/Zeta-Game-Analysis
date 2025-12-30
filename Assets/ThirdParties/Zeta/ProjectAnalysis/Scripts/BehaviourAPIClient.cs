using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;

namespace Zeta.ProjectAnalysis
{
    [Serializable]
    public class BehaviourData
    {
        public string behaviourId;
        public int level;
        public string screen; 
        public string objectId;
        public float x;
        public float y;
        public string version;
        public string time;
    }

    [Serializable]
    public class BehaviourRequest
    {
        public string projectUserId; // optional
        public string projectId;     // required if no projectUserId
        public string deviceModel;   // required if no projectUserId
        public string platform;
        public int width;
        public int height;
        public List<BehaviourData> behaviours;
    }

    [Serializable]
    public class BehaviourResult
    {
        public string projectUserId;
        public string behaviourId;
        public bool success;
    }

    [Serializable]
    public class BehaviourResponse {
        public bool success;
        public string message;
        public string projectUserId;
        public List<BehaviourResult> results;
        public List<BehaviourError> errors;
    }

    [Serializable]
    public class BehaviourError
    {
        public BehaviourData behaviour;
        public string error;
    }

    public class BehaviourAPIClient : MonoBehaviour
    {
        public enum State
        {
            Default,
            Replaying
        }

        List<BehaviourData> _behaviourList = new List<BehaviourData>();
        List<BehaviourData> _behaviourTempList = new List<BehaviourData>();

        bool _sending = false;

        State _state = State.Default;
        
        // Singleton instance
        protected static BehaviourAPIClient _instance;
        public static BehaviourAPIClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("BehaviourAPIClient");
                    _instance = go.AddComponent<BehaviourAPIClient>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Send multi behaviours to server
        /// </summary>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        public void PostBehaviours(Action<BehaviourResponse> onSuccess = null, Action<string> onError = null)
        {
            if (_state != State.Default)
                return;

            StartCoroutine(PostBehavioursCoroutine(onSuccess, onError));
        }

        protected IEnumerator PostBehavioursCoroutine(Action<BehaviourResponse> onSuccess, Action<string> onError)
        {
            _sending = true;
            
            // Create request body
            BehaviourRequest request = new BehaviourRequest
            {
                projectUserId = PlayerPrefs.GetString("ZETA_PROJECT_USER_ID", null),
                projectId = CoreAPIClient.Instance.Configuration.project_id,
                deviceModel = SystemInfo.deviceModel,
                platform = Application.platform.ToString(),
                width = Screen.width,
                height = Screen.height,
                behaviours = _behaviourList
            };

            string jsonBody = JsonConvert.SerializeObject(request);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            // Create UnityWebRequest
            using (UnityWebRequest unityRequest = UnityWebRequest.PostWwwForm(Path.Combine(CoreAPIClient.Instance.Configuration.base_url, "behaviour"), "POST"))
            {
                unityRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                unityRequest.downloadHandler = new DownloadHandlerBuffer();
                unityRequest.SetRequestHeader("Content-Type", "application/json");
                unityRequest.SetRequestHeader("Authorization", "Bearer " + CoreAPIClient.Instance.Configuration.api_key);

                // Send request
                yield return unityRequest.SendWebRequest();

                // Handle response
                if (unityRequest.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = unityRequest.downloadHandler.text;
                        BehaviourResponse response = JsonConvert.DeserializeObject<BehaviourResponse>(responseText);
                        
                        if (response.success)
                        {
                            if (response != null && !string.IsNullOrEmpty(response.projectUserId))
                            {
                                PlayerPrefs.SetString("ZETA_PROJECT_USER_ID", response.projectUserId);
                                PlayerPrefs.Save();
                            }
                            onSuccess?.Invoke(response);

                            _behaviourList.Clear();
                        }
                        else
                        {
                            string errorMsg = "API returned errors:\n";
                            if (response.errors != null && response.errors.Count > 0)
                            {
                                foreach (var err in response.errors)
                                {
                                    errorMsg += $"- {err.error}\n";
                                }
                            }
                            onError?.Invoke(errorMsg);
                        }
                    }
                    catch (Exception e)
                    {
                        string errorMsg = $"Error parsing response: {e.Message}";
                        onError?.Invoke(errorMsg);
                    }
                }
                else
                {
                    string errorMsg = $"Request failed: {unityRequest.error} (Status: {unityRequest.responseCode})";
                    onError?.Invoke(errorMsg);
                }

                _behaviourList.AddRange(_behaviourTempList);
                _behaviourTempList.Clear();

                _sending = false;
            }
        }

        public void CreateBehaviourData(string behaviourId, int level, string screen, string objectId, DateTime? time = null)
        {
            if (_state != State.Default)
                return;

            var behaviour = new BehaviourData
            {
                behaviourId = behaviourId,
                level = level,
                screen = screen,
                objectId = objectId,
                x = Input.mousePosition.x,
                y = Input.mousePosition.y,
                version = Application.version,
                time = (time ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            if (!_sending)
            {
                _behaviourList.Add(behaviour);
            }
            else
            {
                _behaviourTempList.Add(behaviour);
            }
        }

        public static void Add(string behaviourId, int level, string screen, string objectId = "", DateTime? time = null)
        {
            Instance.CreateBehaviourData(behaviourId, level, screen, objectId, time);
        }

        public static void Send(Action<BehaviourResponse> onSuccess = null, Action<string> onError = null)
        {
            Instance.PostBehaviours(onSuccess, onError);
        }

        public static State CurrentState
        {
            get
            {
                return Instance._state;
            }

            set
            {
                Instance._state = value;
            }
        }
    }
}
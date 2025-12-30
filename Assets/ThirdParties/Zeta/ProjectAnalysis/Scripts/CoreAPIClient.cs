using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;

namespace Zeta.ProjectAnalysis
{
    public class Configuration
    {
        public string base_url;
        public string api_key;
        public string project_id;
    }
    public class CoreAPIClient : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] protected TextAsset _configFile;
        [SerializeField] protected bool _behaviourFeature;
        [SerializeField] protected BehaviourAPIClient _behaviourOverride;
        [SerializeField] protected bool _logFeature;
        [SerializeField] protected LogAPIClient _logOverride;

        protected static CoreAPIClient _instance;
        public static CoreAPIClient Instance
        {
            get
            {
                return _instance;
            }
        }
        
        protected Configuration _configuration;
        public Configuration Configuration
        {
            get
            {
                return _configuration;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                if (_configFile == null)
                    return;

                _configuration = JsonConvert.DeserializeObject<Configuration>(_configFile.text);

                if (_behaviourFeature)
                {
                    if (_behaviourOverride == null)
                    {
                        var behaviour = new GameObject("BehaviourAPIClient");
                        behaviour.AddComponent<BehaviourAPIClient>();
                    }
                    else
                    {
                        var behaviour = Instantiate(_behaviourOverride);
                        behaviour.name = "BehaviourAPIClient";
                    }
                }

                if (_logFeature)
                {
                    if (_logOverride == null)
                    {
                        var log = new GameObject("LogAPIClient");
                        log.AddComponent<LogAPIClient>();
                    }
                    else
                    {
                        var log = Instantiate(_logOverride);
                        log.name = "LogAPIClient";
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
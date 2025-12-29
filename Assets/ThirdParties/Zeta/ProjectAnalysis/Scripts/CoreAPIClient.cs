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
        [SerializeField] protected bool _logFeature;

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
                    var behaviour = new GameObject("BehaviourAPIClient");
                    behaviour.AddComponent<BehaviourAPIClient>();
                }

                if (_logFeature)
                {
                    var log = new GameObject("LogAPIClient");
                    log.AddComponent<LogAPIClient>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
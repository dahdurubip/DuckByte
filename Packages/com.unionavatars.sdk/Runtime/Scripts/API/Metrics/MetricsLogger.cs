using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using UnionAvatars.API;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using UnionAvatars.Settings;

namespace UnionAvatars.Metrics
{
    public class MetricsLogger
    {
        const string endpoint = "https://api.unionavatars.com/v2/metrics";
        private static string company,
            product,
            productVersion,
            engine,
            sdkVersion,
            buildTarget,
            buildType;

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeMetricData()
        {
            company = Application.companyName;
            product = Application.productName;
            productVersion = Application.version;
            engine = "Unity " + Application.unityVersion;
            sdkVersion = Resources.Load<UnionAvatarsSDK_Settings>("UnionAvatars/UnionAvatarsSDK_Settings").version;
            buildTarget = GetBuildTarget();
            buildType = Debug.isDebugBuild ? "Development" : "Production";
        }

        private static string GetBuildTarget()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#else
            return Application.platform.ToString();
#endif
        }

        private static MetricData GenerateMetricData(string eventType, params KeyValuePair<string, string>[] parameters)
        {
            Dictionary<string, string> extra = new Dictionary<string, string>()
            {
                { "company", company },
                { "product", product },
                { "product_version", productVersion },
                { "engine", engine },
                { "sdk_version", sdkVersion },
                { "build_target", buildTarget },
                { "build_type", buildType }
            };

            foreach (var param in parameters)
            {
                extra.Add(param.Key, param.Value);
            }

            MetricData newData = new MetricData { EventType = eventType, ExtraInfo = extra };

            return newData;
        }

        public static async void SendMetric(
            string eventType,
            AuthToken token,
            params KeyValuePair<string, string>[] parameters
        )
        {
            if (token.AccessToken == null)
                throw new ArgumentNullException("token");

            MetricData metrics = GenerateMetricData(eventType, parameters);

            string jsonQuery = JsonConvert.SerializeObject(metrics);

            using UnityWebRequest webRequest = new UnityWebRequest(endpoint, "POST");

            //Add body
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonQuery);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            webRequest.SetRequestHeader("Authorization", token.TokenType + " " + token.AccessToken);
            webRequest.SetRequestHeader("usage", "unity_sdk");

            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                await Task.Yield();
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnionAvatars.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace UnionAvatars.Editor.Utils
{
    [InitializeOnLoad]
    public class APIKeyGenerator
    {
        public static async Task<string> GenerateAPIKey()
        {
            if (EditorPrefs.GetString("uniondev_token") == "")
            {
                Debug.LogError("You are not logged in");
                return null;
            }

            using UnityWebRequest webRequest = new UnityWebRequest("https://api.unionavatars.com/v2/keys", "POST");

            webRequest.SetRequestHeader("Content-Type", "application/json");

            DateTime expireDate = DateTime.Now.AddYears(1);
            string expireDateString = expireDate.ToString("yyyy-MM-dd");
            string body =
                $@"{{""description"": ""UnitySDK"",""scope"": ""basic"",""expire_at"": ""{expireDateString}T00:00:00""}}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

            webRequest.SetRequestHeader("Authorization", "Bearer " + EditorPrefs.GetString("uniondev_token"));
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                APIKey apiKey = JsonConvert.DeserializeObject<APIKey>(webRequest.downloadHandler.text);

                return $"{apiKey.key}:{apiKey.expire}";
            }
            else
                Debug.LogWarning("Union Avatars: Failed to generate API Key, " + webRequest.downloadHandler.text);
                return null;
        }
    }

    public class APIKey
    {
        [JsonProperty("api_key")]
        public string key;

        [JsonProperty("expire_at")]
        public string expire;
    }
}

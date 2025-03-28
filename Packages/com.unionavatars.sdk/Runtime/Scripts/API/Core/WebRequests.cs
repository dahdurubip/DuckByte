using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace UnionAvatars.API
{
    public class WebRequests
    {
        /// <summary>
        /// Send a request without body
        /// </summary>
        public static async Task<WebResponse<T>> Send<T>(
            string endpoint,
            string method,
            SessionContext sessionContext,
            CancellationToken cancellationToken = default
        )
        {
            using UnityWebRequest request = await CreateAndSendRequest(
                endpoint,
                method,
                sessionContext,
                cancelToken: cancellationToken
            );
            return GetResponse<T>(request);
        }

        /// <summary>
        /// Send a request with a query
        /// </summary>
        public static async Task<WebResponse<T>> Send<T>(
            string endpoint,
            string method,
            KeyValuePair<string, string>[] query,
            SessionContext sessionContext,
            CancellationToken cancellationToken = default
        )
        {
            using UnityWebRequest request = await CreateAndSendRequest(
                endpoint,
                method,
                sessionContext,
                query: query,
                cancelToken: cancellationToken
            );
            return GetResponse<T>(request);
        }

        /// <summary>
        /// Send a request with a JSON body
        /// </summary>
        public static async Task<WebResponse<T>> Send<T>(
            string endpoint,
            string method,
            string jsonBody,
            SessionContext sessionContext,
            CancellationToken cancellationToken = default
        )
        {
            using UnityWebRequest request = await CreateAndSendRequest(
                endpoint,
                method,
                sessionContext,
                body: jsonBody,
                cancelToken: cancellationToken
            );
            return GetResponse<T>(request);
        }

        /// <summary>
        /// Send a request with a FORM body
        /// </summary>
        public static async Task<WebResponse<T>> Send<T>(
            string endpoint,
            string method,
            WWWForm form,
            SessionContext sessionContext,
            CancellationToken cancellationToken = default
        )
        {
            using UnityWebRequest request = await CreateAndSendRequest(
                endpoint,
                method,
                sessionContext,
                form: form,
                cancelToken: cancellationToken
            );
            return GetResponse<T>(request);
        }

        private static async Task<UnityWebRequest> CreateAndSendRequest(
            string endpoint,
            string method,
            SessionContext sessionContext,
            string body = null,
            KeyValuePair<string, string>[] query = null,
            WWWForm form = null,
            CancellationToken cancelToken = default
        )
        {
            if (sessionContext == null)
                throw new APIOperationFailed("No Session provided when trying to access: " + endpoint);

            // Add query
            if (query != null)
                endpoint = AddQuery(endpoint, query);

            UnityWebRequest webRequest = new UnityWebRequest(endpoint, method);

            // Add body
            if (body != null)
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.SetRequestHeader("Content-Type", "application/json");
            }
            else if (form != null)
            {
                webRequest.uploadHandler = new UploadHandlerRaw(form.data);
                webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            }

            webRequest.downloadHandler = new DownloadHandlerBuffer();

            // Prioritize user token over API key
            if (sessionContext.UserToken != null)
                webRequest.SetRequestHeader(
                    "Authorization",
                    sessionContext.UserToken.TokenType + " " + sessionContext.UserToken.AccessToken
                );
            else if (sessionContext.ApiKey != null)
            {
                webRequest.SetRequestHeader("access_token", sessionContext.ApiKey);
            }

            webRequest.SendWebRequest();

            while (!webRequest.isDone)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    webRequest.Dispose();
                    return null;
                }

                await Task.Yield();
            }

            return webRequest;
        }

        private static WebResponse<T> GetResponse<T>(UnityWebRequest webRequest)
        {
            if (webRequest == null)
                return new WebResponse<T>() { status = ResponseStatus.Dropped };

            WebResponse<T> response = new WebResponse<T>();

            if (
                webRequest.result == UnityWebRequest.Result.ConnectionError
                || webRequest.result == UnityWebRequest.Result.ProtocolError
                || webRequest.result == UnityWebRequest.Result.DataProcessingError
            )
            {
                response.status = ResponseStatus.Failed;
                try
                {
                    response.responseErrorMessage = JsonConvert
                        .DeserializeObject<ErrorResponse>(webRequest.downloadHandler.text)
                        .detail;
                }
                catch (Exception)
                {
                    response.responseErrorMessage = webRequest.downloadHandler.text;
                }

                return response;
            }
            else
            {
                response.status = ResponseStatus.Success;
            }

            if (typeof(T) == typeof(string))
            {
                response.data = (T)System.Convert.ChangeType(webRequest.downloadHandler.text, typeof(T));
                return response;
            }
            else
            {
                response.data = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                return response;
            }
        }

        private static string AddQuery(string url, KeyValuePair<string, string>[] parameters)
        {
            if (parameters != null)
            {
                url += "?";

                for (int i = 0; i < parameters.Length; i++)
                {
                    KeyValuePair<string, string> param = parameters[i];

                    if (i > 0)
                        url += "&";

                    url += param.Key + "=" + UnityWebRequest.EscapeURL(param.Value);
                }
            }

            return url;
        }
    }
}

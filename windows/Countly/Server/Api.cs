using CountlySDK.Entities;
using CountlySDK.Server.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CountlySDK
{
    internal class Api
    {
        internal static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<ResultResponse> SendSession(string serverUrl, SessionEvent sessionEvent, CountlyUserDetails userDetails = null)
        {
            var post = new Dictionary<string, string>(sessionEvent.Content);

            if (userDetails != null)
            {
                post["user_details"] = JsonConvert.SerializeObject(userDetails, JsonSettings);
            }

            return await Call<ResultResponse>(serverUrl, post);
        }

        public static async Task<ResultResponse> SendEvents(string serverUrl, string appKey, string deviceId, List<CountlyEvent> events, CountlyUserDetails userDetails = null)
        {
            var post = CreatePost(appKey, deviceId);

            post["events"] = JsonConvert.SerializeObject(events, JsonSettings);

            if (userDetails != null)
            {
                post["user_details"] = JsonConvert.SerializeObject(userDetails, JsonSettings);
            }

            return await Call<ResultResponse>(serverUrl, post);
        }

        public static async Task<ResultResponse> SendException(string serverUrl, string appKey, string deviceId, ExceptionEvent exception)
        {
            var post = CreatePost(appKey, deviceId);

            post["crash"] = JsonConvert.SerializeObject(exception, JsonSettings);

            return await Call<ResultResponse>(serverUrl, post);
        }

        public static async Task<ResultResponse> UploadUserDetails(string serverUrl, string appKey, string deviceId, CountlyUserDetails userDetails = null)
        {
            var post = CreatePost(appKey, deviceId);

            if (userDetails != null)
            {
                post["user_details"] = JsonConvert.SerializeObject(userDetails, JsonSettings);
            }

            return await Call<ResultResponse>(serverUrl, post);
        }

        private static Dictionary<string, string> CreatePost(string appKey, string deviceId)
        {
            return new Dictionary<string, string>
            {
                { "app_key", appKey },
                { "device_id", deviceId }
            };
        }

        private static async Task<T> Call<T>(string serverUrl, Dictionary<string, string> post)
        {
            try
            {
                string address = serverUrl + "/i";

                if (Countly.IsLoggingEnabled)
                {
                    Countly.Log("POST {0}", address);

                    foreach (var pair in post)
                    {
                        Countly.Log("{0}={1}", pair.Key, pair.Value);
                    }
                }

                var response = await _httpClient.PostAsync(address, new FormUrlEncodedContent(post));
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(json))
                {
                    Countly.Log(json);

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception exc)
            {
                Countly.Log(exc);
            }

            return default(T);
        }
    }
}

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
            return await Call<ResultResponse>(serverUrl, new CountlyRequest
            {
                SessionEvent = sessionEvent,
                UserDetails = userDetails,
            });
        }

        public static Task<ResultResponse> SendEvents(string serverUrl, string appKey, string deviceId, List<CountlyEvent> events, CountlyUserDetails userDetails = null)
        {
            return Call<ResultResponse>(serverUrl, new CountlyRequest
            {
                AppKey = appKey,
                DeviceId = deviceId,
                Events = events,
                UserDetails = userDetails,
            });
        }

        public static async Task<ResultResponse> SendException(string serverUrl, string appKey, string deviceId, ExceptionEvent exception)
        {
            return await Call<ResultResponse>(serverUrl, new CountlyRequest
            {
                AppKey = appKey,
                DeviceId = deviceId,
                Exception = exception,
            });
        }

        public static async Task<ResultResponse> UploadUserDetails(string serverUrl, string appKey, string deviceId, CountlyUserDetails userDetails = null)
        {
            return await Call<ResultResponse>(serverUrl, new CountlyRequest { UserDetails = userDetails });
        }

        private static async Task<T> Call<T>(string serverUrl, CountlyRequest request)
        {
            try
            {
                string address = serverUrl + "/i";

                if (Countly.IsLoggingEnabled)
                {
                    Countly.Log("POST {0}", address);
                }

                var response = await _httpClient.PostAsync(address, request.ToContent());
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

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

        public static async Task<T> Call<T>(string serverUrl, CountlyRequest request)
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

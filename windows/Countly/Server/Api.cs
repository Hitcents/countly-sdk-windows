using CountlySDK.Entities;
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
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<T> Call<T>(string serverUrl, IEnumerable<CountlyRequest> request) where T : new()
        {
            try
            {
                string address = serverUrl + "/i/bulk";
                var json = JsonConvert.SerializeObject(request, JsonSettings);

                if (Countly.IsLoggingEnabled)
                {
                    Countly.Log("POST {0}", address);
                    Countly.Log("BODY: {0}", json);
                }

                var response = await _httpClient.PostAsync(address, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "app_key", Countly.AppKey },
                    { "requests", json },
                }));
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync();
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

            return new T();
        }
    }
}

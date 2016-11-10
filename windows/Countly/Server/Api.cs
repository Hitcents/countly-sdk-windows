﻿using CountlySDK.Entities;
using CountlySDK.Server.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CountlySDK
{
    internal class Api
    {
        private const string DebugLabel = "Count.ly: ";
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<ResultResponse> BeginSession(string serverUrl, string appKey, string deviceId, string sdkVersion, string metricsJson)
        {
            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&sdk_version={3}&begin_session=1&metrics={4}", serverUrl, appKey, deviceId, sdkVersion, WebUtility.UrlEncode(metricsJson)));
        }

        public static async Task<ResultResponse> UpdateSession(string serverUrl, string appKey, string deviceId, int duration)
        {
            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&session_duration={3}", serverUrl, appKey, deviceId, duration));
        }

        public static async Task<ResultResponse> EndSession(string serverUrl, string appKey, string deviceId)
        {
            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&end_session=1", serverUrl, appKey, deviceId));
        }

        public static async Task<ResultResponse> SendSession(string serverUrl, SessionEvent sesisonEvent, CountlyUserDetails userDetails = null)
        {
            string userDetailsJson = String.Empty;

            if (userDetails != null)
            {
                userDetailsJson = "&user_details=" + WebUtility.UrlEncode(JsonConvert.SerializeObject(userDetails, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            }

            return await Call<ResultResponse>(serverUrl + sesisonEvent.Content + userDetailsJson);
        }

        public static async Task<ResultResponse> SendEvents(string serverUrl, string appKey, string deviceId, List<CountlyEvent> events, CountlyUserDetails userDetails = null)
        {
            string eventsJson = JsonConvert.SerializeObject(events, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            string userDetailsJson = String.Empty;

            if (userDetails != null)
            {
                userDetailsJson = "&user_details=" + WebUtility.UrlEncode(JsonConvert.SerializeObject(userDetails, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
            }

            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&events={3}{4}", serverUrl, appKey, deviceId, WebUtility.UrlEncode(eventsJson), userDetailsJson));
        }

        public static async Task<ResultResponse> SendException(string serverUrl, string appKey, string deviceId, ExceptionEvent exception)
        {
            string exceptionJson = JsonConvert.SerializeObject(exception, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&crash={3}", serverUrl, appKey, deviceId, WebUtility.UrlEncode(exceptionJson)));
        }

        public static async Task<ResultResponse> UploadUserDetails(string serverUrl, string appKey, string deviceId, CountlyUserDetails userDetails = null)
        {
            string userDetailsJson = String.Empty;

            if (userDetails != null)
            {
                userDetailsJson = JsonConvert.SerializeObject(userDetails, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }

            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&user_details={3}", serverUrl, appKey, deviceId, userDetailsJson));
        }

        public static async Task<ResultResponse> UploadUserPicture(string serverUrl, string appKey, string deviceId, Stream imageStream, CountlyUserDetails userDetails = null)
        {
            string userDetailsJson = String.Empty;

            if (userDetails != null)
            {
                userDetailsJson = "=" + JsonConvert.SerializeObject(userDetails, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            }

            return await Call<ResultResponse>(String.Format("{0}/i?app_key={1}&device_id={2}&user_details{3}", serverUrl, appKey, deviceId, userDetailsJson), imageStream);
        }

        private static async Task<T> Call<T>(string address, Stream data = null)
        {
            try
            {
                string responseJson = await RequestAsync(address, data);

                if (Countly.IsLoggingEnabled)
                {
                    Debug.WriteLine(DebugLabel + responseJson);
                }

                return JsonConvert.DeserializeObject<T>(responseJson);
            }
            catch (Exception exc)
            {
                if (Countly.IsLoggingEnabled)
                {
                    Debug.WriteLine(DebugLabel + exc);
                }

                return default(T);
            }
        }

        private static async Task<string> RequestAsync(string address, Stream data = null)
        {
            if (Countly.IsLoggingEnabled)
            {
                Debug.WriteLine(DebugLabel + " POST " + address);
            }

            var httpResponseMessage = await _httpClient.PostAsync(address, (data != null) ? new StreamContent(data) : null);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }
    }
}

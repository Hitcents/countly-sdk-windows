﻿/*
Copyright (c) 2012, 2013, 2014 Countly

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

//This allows logging to work, whether you are DEBUG or not
#define DEBUG

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CountlySDK.Entities;
using CountlySDK.Helpers;
using CountlySDK.Server.Responses;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
#if !PCL
using Windows.UI.Xaml;
#endif

namespace CountlySDK
{
    /// <summary>
    /// This class is the public API for the Countly Windows Phone SDK.
    /// </summary>
    public static class Countly
    {
        // Current version of the Count.ly Windows Phone SDK as a displayable string.
        private const string sdkVersion = "1.0";

        // How often update session is sent
        private const int updateInterval = 60;

        // Server url provided by a user
        internal static string ServerUrl;
        // Application key provided by a user
        internal static string AppKey;

        private const string DebugLabel = "Count.ly: ";
        private const string ViewEvent = "[CLY]_view";

        // Used for thread-safe operations
        private static object sync = new object();

        // Events queue
        private static List<CountlyEvent> Events { get; set; } = new List<CountlyEvent>(0);

        // User details info
        public static CountlyUserDetails UserDetails { get; set; } = new CountlyUserDetails();

        private static StringBuilder breadCrumb = new StringBuilder();

        // Start session timestamp
        private static DateTime startTime;
        // Update session timer
#if PCL
        private static Timer Timer;
#else
        private static DispatcherTimer Timer;
#endif

        private static View lastView;

        class View
        {
            public string Name { get; set; }

            public DateTime Time { get; set; }

            /// <summary>
            /// NOTE: this is needed in BeginSession, to send the last screen again
            /// </summary>
            public Segmentation Segmentation { get; set; }
        }

        /// <summary>
        /// Determines if Countly debug messages are displayed to Output window
        /// </summary>
        public static bool IsLoggingEnabled { get; set; }

        /// <summary>
        /// Determines if exception autotracking is enabled
        /// </summary>
        public static bool IsExceptionsLoggingEnabled { get; set; }

        /// <summary>
        /// Starts Countly tracking session.
        /// Call from your App.xaml.cs Application_Launching and Application_Activated events.
        /// Must be called before other SDK methods can be used.
        /// </summary>
        /// <param name="serverUrl">URL of the Countly server to submit data to; use "https://cloud.count.ly" for Countly Cloud</param>
        /// <param name="appKey">app key for the application being tracked; find in the Countly Dashboard under Management > Applications</param>
        public static async Task StartSession(string serverUrl, string appKey)
        {
            if (String.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("invalid server url");
            }

            if (String.IsNullOrWhiteSpace(appKey))
            {
                throw new ArgumentException("invalid application key");
            }

            ServerUrl = serverUrl;
            AppKey = appKey;

            Events.Clear();
            UserDetails = new CountlyUserDetails();
             
            startTime = DateTime.UtcNow;

#if PCL
            var interval = TimeSpan.FromSeconds(updateInterval);
            Timer = new Timer(UpdateSession, null, interval, interval);
#else
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(updateInterval);
            Timer.Tick += UpdateSession;
            Timer.Start();
#endif

            var view = lastView;
            if (view != null)
            {
                view.Time = DateTime.UtcNow;
                AddEvent(new CountlyEvent(ViewEvent, 1, 0, null, view.Segmentation));
            }

            await Upload(new BeginSession(sdkVersion));
        }

        /// <summary>
        /// Sends session duration. Called automatically each <updateInterval> seconds
        /// </summary>
#if PCL
        private static async void UpdateSession(object state)
#else
        private static async void UpdateSession(object sender, object e)
#endif
        {
            await Upload(new UpdateSession((int)DateTime.UtcNow.Subtract(startTime).TotalSeconds));
        }

        /// <summary>
        /// End Countly tracking session.
        /// Call from your App.xaml.cs Application_Deactivated and Application_Closing events.
        /// </summary>
        public static async Task EndSession()
        {
            if (Timer != null)
            {
#if PCL
                Timer.Dispose();
#else
                Timer.Stop();
                Timer.Tick -= UpdateSession;
#endif
                Timer = null;
            }
            
            await Upload(new EndSession());
        }

        /// <summary>
        /// Raised when application unhandled exception is thrown
        /// </summary>
        /// <param name="sender">sender param</param>
        /// <param name="e">exception details</param>
#if !PCL
        private static async void OnApplicationUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (IsExceptionsLoggingEnabled)
            {
                await RecordUnhandledException(e.Exception.Message, e.Exception.StackTrace);
            }
        }
#endif

        /// <summary>
        /// Immediately disables session, event, exceptions & user details tracking and clears any stored sessions, events, exceptions & user details data.
        /// This API is useful if your app has a tracking opt-out switch, and you want to immediately
        /// disable tracking when a user opts out. The EndSession/RecordEvent methods will throw
        /// InvalidOperationException after calling this until Countly is reinitialized by calling StartSession
        /// again.
        /// </summary>
        public static void Halt()
        {
            lock (sync)
            {
                ServerUrl = null;
                AppKey = null;
                lastView = null;

                if (Timer != null)
                {
#if PCL
                    Timer.Dispose();
#else
                    Timer.Stop();
                    Timer.Tick -= UpdateSession;
#endif
                    Timer = null;
                }

                Events.Clear();
                breadCrumb = new StringBuilder();
                UserDetails = new CountlyUserDetails();
            }
        }

        /// <summary>
        /// Records a custom event with the specified values
        /// </summary>
        /// <param name="key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="count">Count to associate with the event, should be more than zero</param>
        /// <param name="sum">Sum to associate with the event</param>
        /// <param name="duration">Duration to associate with the event</param>
        /// <param name="segmentation">Segmentation object to associate with the event, can be null</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static void RecordEvent(string key, int count = 1, double? sum = null, double? duration = null, Segmentation segmentation = null)
        {
            AddEvent(new CountlyEvent(key, count, sum, duration, segmentation));
        }

        public static void RecordView(string name, Segmentation segmentation)
        {
            if (segmentation == null)
                segmentation = new Segmentation();
            segmentation.Add("name", name);
            segmentation.Add("visit", "1");
            segmentation.Add("segment", Device.OS);

            CountlyEvent evt;
            var view = lastView;
            if (view == null)
            {
                segmentation.Add("start", "1");

                evt = new CountlyEvent(ViewEvent, 1, 0, null, segmentation);
            }
            else
            {
                evt = new CountlyEvent(ViewEvent, 1, 0, Math.Round((DateTime.UtcNow - view.Time).TotalSeconds, 2), segmentation);
            }
            lastView = new View { Name = name, Time = DateTime.UtcNow, Segmentation = segmentation };
            AddEvent(evt);
        }

        /// <summary>
        /// Adds event to queue
        /// </summary>
        /// <param name="countlyEvent">event object</param>
        private static void AddEvent(CountlyEvent countlyEvent)
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }
            lock (sync)
            {
                Events.Add(countlyEvent);
            }
        }

        /// <summary>
        /// Records exception
        /// </summary>
        /// <param name="error">exception title</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static Task<bool> RecordException(string error)
        {
            return RecordException(error, null, null);
        }

        /// <summary>
        /// Records exception with stacktrace
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static Task<bool> RecordException(string error, string stackTrace)
        {
            return RecordException(error, stackTrace, null);
        }

        /// <summary>
        /// Records unhandled exception with stacktrace
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        private static Task<bool> RecordUnhandledException(string error, string stackTrace)
        {
            return RecordException(error, stackTrace, null, true);
        }

        /// <summary>
        /// Records exception with stacktrace and custom info
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <param name="customInfo">exception custom info</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static Task<bool> RecordException(string error, string stackTrace, Dictionary<string, string> customInfo)
        {
            return RecordException(error, stackTrace, customInfo, false);
        }

        /// <summary>
        /// Records exception with stacktrace and custom info
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <param name="customInfo">exception custom info</param>
        /// <param name="unhandled">bool indicates is exception is fatal or not</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        private static Task<bool> RecordException(string error, string stackTrace, Dictionary<string, string> customInfo, bool unhandled)
        {
            return Upload(exception: new ExceptionEvent(error, stackTrace, unhandled, breadCrumb, DateTime.UtcNow.Subtract(startTime), customInfo));
        }

        /// <summary>
        /// Adds log breadcrumb
        /// </summary>
        /// <param name="log">log string</param>
        public static void AddBreadCrumb(string log)
        {
            breadCrumb.AppendLine(log);
        }

        /// <summary>
        /// Upload sessions, events & exception queues
        /// </summary>
        /// <returns>True if success</returns>
        public static Task<bool> Upload()
        {
            return Upload(null, null);
        } 

        private static async Task<bool> Upload(SessionEvent sessionEvent = null, ExceptionEvent exception = null)
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }

            var request = new CountlyRequest
            {
                SessionEvent = sessionEvent,
                Exception = exception,
            };

            lock(sync)
            {
                request.Events = new List<CountlyEvent>(Events);
                Events.Clear();

                if (UserDetails.isChanged)
                {
                    request.UserDetails = UserDetails;
                    UserDetails.isChanged = false;
                }
            }

            var response = await Api.Call<ResultResponse>(ServerUrl, request);
            return response.IsSuccess;
        }

        internal static void Log(string message)
        {
            if (IsLoggingEnabled)
                Debug.WriteLine(DebugLabel + message);
        }

        internal static void Log(object value)
        {
            if (IsLoggingEnabled)
                Debug.WriteLine(DebugLabel + value);
        }

        internal static void Log(string format, params object[] args)
        {
            if (IsLoggingEnabled)
                Debug.WriteLine(DebugLabel + format, args);
        }
    }
}

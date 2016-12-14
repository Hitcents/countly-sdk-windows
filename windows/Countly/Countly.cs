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
        private static string ServerUrl;
        // Application key provided by a user
        private static string AppKey;

        // Indicates sync process with a server
        private static bool uploadInProgress;

        // File that stores events objects
        private const string eventsFilename = "events.json";
        // File that stores sessions objects
        private const string sessionsFilename = "sessions.json";
        // File that stores exceptions objects
        private const string exceptionsFilename = "exceptions.json";
        // File that stores user details object
        private const string userDetailsFilename = "userdetails.json";

        private const string DebugLabel = "Count.ly: ";
        private const string ViewEvent = "[CLY]_view";

        // Used for thread-safe operations
        private static object sync = new object();

        // Events queue
        private static List<CountlyEvent> Events { get; set; }

        // Session queue
        private static List<SessionEvent> Sessions { get; set; }

        // Exceptions queue
        private static List<ExceptionEvent> Exceptions { get; set; }

        // User details info
        public static CountlyUserDetails UserDetails { get; set; }

        private static string breadcrumb = String.Empty;

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
        /// Saves events to the storage
        /// </summary>
        private static async Task SaveEvents()
        {
            await Storage.SaveToFile(eventsFilename, Events);
        }

        /// <summary>
        /// Saves sessions to the storage
        /// </summary>
        private static async Task SaveSessions()
        {
            await Storage.SaveToFile(sessionsFilename, Sessions);
        }

        /// <summary>
        /// Saves exceptions to the storage
        /// </summary>
        private static async Task SaveExceptions()
        {
            await Storage.SaveToFile(exceptionsFilename, Exceptions);
        }

        /// <summary>
        /// Saves user details info to the storage
        /// </summary>
        private static async Task SaveUserDetails()
        {
            await Storage.SaveToFile(userDetailsFilename, UserDetails);
        }

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
            lastView = null;

            Events = await Storage.LoadFromFile<List<CountlyEvent>>(eventsFilename) ?? new List<CountlyEvent>();

            Sessions = await Storage.LoadFromFile<List<SessionEvent>>(sessionsFilename) ?? new List<SessionEvent>();

            Exceptions = await Storage.LoadFromFile<List<ExceptionEvent>>(exceptionsFilename) ?? new List<ExceptionEvent>();

            UserDetails = await Storage.LoadFromFile<CountlyUserDetails>(userDetailsFilename) ?? new CountlyUserDetails();

            UserDetails.UserDetailsChanged += OnUserDetailsChanged;
             
            startTime = DateTime.Now;

#if PCL
            var interval = TimeSpan.FromSeconds(updateInterval);
            Timer = new Timer(UpdateSession, null, interval, interval);
#else
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(updateInterval);
            Timer.Tick += UpdateSession;
            Timer.Start();
#endif

            await AddSessionEvent(new BeginSession(AppKey, Device.DeviceId, sdkVersion));
        }

        /// <summary>
        /// Starts Countly background tracking session.
        /// Call from your background agent OnInvoke method.
        /// Must be called before other SDK methods can be used.
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="appKey"></param>
        public static async void StartBackgroundSession(string serverUrl, string appKey)
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
            lastView = null;

            Events = await Storage.LoadFromFile<List<CountlyEvent>>(eventsFilename) ?? new List<CountlyEvent>();

            Sessions = await Storage.LoadFromFile<List<SessionEvent>>(sessionsFilename) ?? new List<SessionEvent>();

            Exceptions = await Storage.LoadFromFile<List<ExceptionEvent>>(exceptionsFilename) ?? new List<ExceptionEvent>();

            UserDetails = await Storage.LoadFromFile<CountlyUserDetails>(userDetailsFilename) ?? new CountlyUserDetails();

            UserDetails.UserDetailsChanged += OnUserDetailsChanged;
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
            await AddSessionEvent(new UpdateSession(AppKey, Device.DeviceId, (int)DateTime.Now.Subtract(startTime).TotalSeconds));
        }

        /// <summary>
        /// End Countly tracking session.
        /// Call from your App.xaml.cs Application_Deactivated and Application_Closing events.
        /// </summary>
        public static async Task EndSession()
        {
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

            await AddSessionEvent(new EndSession(AppKey, Device.DeviceId), true);
        }

        /// <summary>
        ///  Adds session event to queue and uploads
        /// </summary>
        /// <param name="sessionEvent">session event object</param>
        /// <param name="uploadImmediately">indicates when start to upload, by default - immediately after event was added</param>
        private static async Task AddSessionEvent(SessionEvent sessionEvent, bool uploadImmediately = true)
        {
            if (String.IsNullOrWhiteSpace(ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }

            lock (sync)
            {
                Sessions.Add(sessionEvent);
            }

            await SaveSessions();

            if (uploadImmediately)
            {
                await Upload();
            }
        }

        /// <summary>
        /// Uploads sessions queue to Countly server
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> UploadSessions()
        {
            lock (sync)
            {
                if (uploadInProgress) return true;

                uploadInProgress = true;
            }

            SessionEvent sessionEvent = null;

            lock (sync)
            {
                if (Sessions.Count > 0)
                {
                    sessionEvent = Sessions[0];
                }
            }

            if (sessionEvent != null)
            {
                ResultResponse resultResponse = await Api.SendSession(ServerUrl, sessionEvent, (UserDetails.isChanged) ? UserDetails : null);

                if (resultResponse != null && resultResponse.IsSuccess)
                {
                    UserDetails.isChanged = false;

                    await SaveUserDetails();

                    lock (sync)
                    {
                        uploadInProgress = false;

                        try
                        {
                            Sessions.RemoveAt(0);
                        }
                        catch { }
                    }

                    await Storage.SaveToFile<List<SessionEvent>>(sessionsFilename, Sessions);

                    if (Sessions.Count > 0)
                    {
                        return await UploadSessions();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    uploadInProgress = false;

                    return false;
                }
            }
            else
            {
                uploadInProgress = false;

                return true;
            }
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
        public static async void Halt()
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
                Sessions.Clear();
                Exceptions.Clear();
                breadcrumb = String.Empty;
                UserDetails = new CountlyUserDetails();
            }

            await Storage.DeleteFile(eventsFilename);
            await Storage.DeleteFile(sessionsFilename);
            await Storage.DeleteFile(exceptionsFilename);
            await Storage.DeleteFile(userDetailsFilename);
        }

        /// <summary>
        /// Records a custom event with no segmentation values, a count of one and a sum of zero
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key)
        {
            return RecordCountlyEvent(Key, 1, null, null, null);
        }

        /// <summary>
        /// Records a custom event with no segmentation values, the specified count, and a sum of zero.
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key, int Count)
        {
            return RecordCountlyEvent(Key, Count, null, null, null);
        }

        /// <summary>
        /// Records a custom event with no segmentation values, and the specified count and sum.
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <param name="Sum">Sum to associate with the event</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key, int Count, double Sum)
        {
            return RecordCountlyEvent(Key, Count, Sum, null, null);
        }

        /// <summary>
        /// Records a custom event with the specified segmentation values and count, and a sum of zero.
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <param name="Segmentation">Segmentation object to associate with the event, can be null</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key, int Count, Segmentation Segmentation)
        {
            return RecordCountlyEvent(Key, Count, null, null, Segmentation);
        }

        /// <summary>
        /// Records a custom event with the specified segmentation values, count and a sum
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <param name="Sum">Sum to associate with the event</param>
        /// <param name="Segmentation">Segmentation object to associate with the event, can be null</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key, int Count, double Sum, Segmentation Segmentation)
        {
            return RecordCountlyEvent(Key, Count, Sum, null, Segmentation);
        }

        /// <summary>
        /// Records a custom event with the specified segmentation values, count, a sum, and a Dur
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <param name="Sum">Sum to associate with the event</param>
        /// <param name="Dur">Dur parameter, I have no idea guys</param>
        /// <param name="Segmentation">Segmentation object to associate with the event, can be null</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        public static Task<bool> RecordEvent(string Key, int Count, double Sum, double Dur, Segmentation Segmentation)
        {
            return RecordCountlyEvent(Key, Count, Sum, Dur, Segmentation);
        }

        /// <summary>
        /// Records a custom event with the specified values
        /// </summary>
        /// <param name="Key">Name of the custom event, required, must not be the empty string</param>
        /// <param name="Count">Count to associate with the event, should be more than zero</param>
        /// <param name="Sum">Sum to associate with the event</param>
        /// <param name="Segmentation">Segmentation object to associate with the event, can be null</param>
        /// <returns>True if event is uploaded successfully, False - queued for delayed upload</returns>
        private static Task<bool> RecordCountlyEvent(string Key, int Count, double? Sum, double? Dur, Segmentation Segmentation)
        {
            return AddEvent(new CountlyEvent(Key, Count, Sum, Dur, Segmentation));
        }

        public static Task<bool> RecordView(string name, Segmentation segmentation)
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
            lastView = new View { Name = name, Time = DateTime.UtcNow };
            return AddEvent(evt);
        }

        /// <summary>
        /// Adds event to queue and uploads
        /// </summary>
        /// <param name="countlyEvent">event object</param>
        /// <returns>True if success</returns>
        private async static Task<bool> AddEvent(CountlyEvent countlyEvent)
        {
            if (String.IsNullOrWhiteSpace(ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }
            lock (sync)
            {
                Events.Add(countlyEvent);
            }

            await SaveEvents();

            return await Upload();
        }

        /// <summary>
        /// Uploads event queue to Countly server
        /// </summary>
        /// <returns>True if success</returns>
        private static async Task<bool> UploadEvents()
        {
            lock (sync)
            {
                // Allow uploading in one thread only
                if (uploadInProgress) return true;

                uploadInProgress = true;
            }

            int eventsCount;

            lock (sync)
            {
                eventsCount = Events.Count;
            }

            if (eventsCount > 0)
            {
                ResultResponse resultResponse = await Api.SendEvents(ServerUrl, AppKey, Device.DeviceId, Events.Take(eventsCount).ToList(), (UserDetails.isChanged) ? UserDetails : null);

                if (resultResponse != null && resultResponse.IsSuccess)
                {
                    int eventsCountToUploadAgain = 0;

                    UserDetails.isChanged = false;

                    await SaveUserDetails();

                    lock (sync)
                    {
                        uploadInProgress = false;

                        try
                        {
                            for (int i = eventsCount - 1; i >= 0; i--)
                            {
                                Events.RemoveAt(i);
                            }
                        }
                        catch { }

                        eventsCountToUploadAgain = Events.Count;
                    }

                    await Storage.SaveToFile<List<CountlyEvent>>(eventsFilename, Events);

                    if (eventsCountToUploadAgain > 0)
                    {
                        // Upload events added during sync
                        return await UploadEvents();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    uploadInProgress = false;

                    return false;
                }
            }
            else
            {
                uploadInProgress = false;

                return true;
            }
        }

        /// <summary>
        /// Raised when user details propery is changed
        /// </summary>
        private static async void OnUserDetailsChanged()
        {
            UserDetails.isChanged = true;

            await SaveUserDetails();

            await UploadUserDetails();
        }

        /// <summary>
        /// Uploads user details
        /// </summary>
        /// <returns>true if details are successfully uploaded, false otherwise</returns>
        internal static async Task<bool> UploadUserDetails()
        {
            if (String.IsNullOrWhiteSpace(Countly.ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }

            ResultResponse resultResponse = await Api.UploadUserDetails(Countly.ServerUrl, Countly.AppKey, Device.DeviceId, UserDetails);

            if (resultResponse != null && resultResponse.IsSuccess)
            {
                UserDetails.isChanged = false;

                await SaveUserDetails();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Records exception
        /// </summary>
        /// <param name="error">exception title</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static async Task<bool> RecordException(string error)
        {
            return await RecordException(error, null, null);
        }

        /// <summary>
        /// Records exception with stacktrace
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static async Task<bool> RecordException(string error, string stackTrace)
        {
            return await RecordException(error, stackTrace, null);
        }

        /// <summary>
        /// Records unhandled exception with stacktrace
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        private static async Task RecordUnhandledException(string error, string stackTrace)
        {
            await RecordException(error, stackTrace, null, true);
        }

        /// <summary>
        /// Records exception with stacktrace and custom info
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <param name="customInfo">exception custom info</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        public static async Task<bool> RecordException(string error, string stackTrace, Dictionary<string, string> customInfo)
        {
            return await RecordException(error, stackTrace, customInfo, false);
        }

        /// <summary>
        /// Records exception with stacktrace and custom info
        /// </summary>
        /// <param name="error">exception title</param>
        /// <param name="stackTrace">exception stacktrace</param>
        /// <param name="customInfo">exception custom info</param>
        /// <param name="unhandled">bool indicates is exception is fatal or not</param>
        /// <returns>True if exception successfully uploaded, False - queued for delayed upload</returns>
        private static async Task<bool> RecordException(string error, string stackTrace, Dictionary<string, string> customInfo, bool unhandled)
        {
            if (String.IsNullOrWhiteSpace(ServerUrl))
            {
                throw new InvalidOperationException("session is not active");
            }

            TimeSpan run = DateTime.Now.Subtract(startTime);

            lock (sync)
            {
                Exceptions.Add(new ExceptionEvent(error, stackTrace, unhandled, breadcrumb, run, customInfo));
            }

            await SaveExceptions();

            if (!unhandled)
            {
                return await Upload();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Uploads exceptions queue to Countly server
        /// </summary>
        /// <returns>True if success</returns>
        private static async Task<bool> UploadExceptions()
        {
            lock (sync)
            {
                // Allow uploading in one thread only
                if (uploadInProgress) return true;

                uploadInProgress = true;
            }

            int exceptionsCount;

            lock (sync)
            {
                exceptionsCount = Exceptions.Count;
            }

            if (exceptionsCount > 0)
            {
                ResultResponse resultResponse = await Api.SendException(ServerUrl, AppKey, Device.DeviceId, Exceptions[0]);

                if (resultResponse != null && resultResponse.IsSuccess)
                {
                    int exceptionsCountToUploadAgain = 0;

                    lock (sync)
                    {
                        uploadInProgress = false;

                        try
                        {
                            Exceptions.RemoveAt(0);
                        }
                        catch { }

                        exceptionsCountToUploadAgain = Exceptions.Count;
                    }

                    await SaveExceptions();

                    if (exceptionsCountToUploadAgain > 0)
                    {
                        // Upload next exception
                        return await UploadExceptions();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    uploadInProgress = false;

                    return false;
                }
            }
            else
            {
                uploadInProgress = false;

                return false;
            }
        }

        /// <summary>
        /// Adds log breadcrumb
        /// </summary>
        /// <param name="log">log string</param>
        public static void AddBreadCrumb(string log)
        {
            breadcrumb += log + "\r\n";
        }

        /// <summary>
        /// Upload sessions, events & exception queues
        /// </summary>
        /// <returns>True if success</returns>
        private static async Task<bool> Upload()
        {
            bool success = await UploadSessions();

            if (success)
            {
                success = await UploadEvents();
            }

            if (success)
            {
                success = await UploadExceptions();
            }

            return success;
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

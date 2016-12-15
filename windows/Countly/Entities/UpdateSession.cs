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

using System;
using CountlySDK.Helpers;
using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    [DataContractAttribute]
    internal class UpdateSession : SessionEvent
    {
        /// <summary>
        /// Creates UpdateSession object with provided values
        /// </summary>
        /// <param name="appKey">App key for the application being tracked; find in the Countly Dashboard under Management > Applications</param>
        /// <param name="deviceId">Unique ID for the device the app is running on</param>
        /// <param name="duration">Session duration in seconds</param>
        public UpdateSession(string appKey, string deviceId, int duration)
        {
            Content["app_key"] = appKey;
            Content["device_id"] = deviceId;
            Content["session_duration"] = duration.ToString();
            Content["timestamp"] = TimeHelper.ToUnixTime().ToString();
        }
    }
}

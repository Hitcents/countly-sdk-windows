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

namespace CountlySDK.Helpers
{
    internal class TimeHelper
    {
        private static readonly DateTime _epoc = new DateTime(1970, 1, 1);
        private static readonly object _lock = new object();
        private static long _lastTime;

        /// <summary>
        /// Converts DateTime.UtcNow to Unix time format
        /// </summary>
        /// <returns>Unix timestamp</returns>
        public static long UnixNow()
        {
            long time = (long)Math.Round(DateTime.UtcNow.Subtract(_epoc).TotalMilliseconds);
            lock(_lock)
            {
                if (time == _lastTime)
                    time++;
                return _lastTime = time;
            }
        }
    }
}

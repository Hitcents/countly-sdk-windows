/*
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

using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    /// <summary>
    /// Holds device-specific info in json-ready format
    /// </summary>
    [DataContract]
    internal class Metrics
    {
        /// <summary>
        /// Name of the current operating system
        /// </summary>
        [DataMember(Name = "_os")]
        public string OS { get; set; }

        /// <summary>
        /// Current operating system version
        /// </summary>
        [DataMember(Name = "_os_version")]
        public string OSVersion { get; set; }

        /// <summary>
        /// Current device model
        /// </summary>
        [DataMember(Name = "_device")]
        public string Device { get; set; }

        /// <summary>
        /// Device resolution
        /// </summary>
        [DataMember(Name = "_resolution")]
        public string Resolution { get; set; }

        [DataMember(Name = "_locale")]
        public string Locale { get; set; }

        /// <summary>
        /// Cellular mobile operator
        /// </summary>
        [DataMember(Name = "_carrier")]
        public string Carrier { get; set; }

        /// <summary>
        /// The package name of the app that installed this app
        /// </summary>
        [DataMember(Name = "_store")]
        public string Store { get; set; }

        /// <summary>
        /// A string constant representing the current display density, or the empty string if the density is unknown
        /// </summary>
        [DataMember(Name = "_density")]
        public string Density { get; set; }

        /// <summary>
        /// Application version
        /// </summary>
        [DataMember(Name = "_app_version")]
        public string AppVersion { get; set; }

        /// <summary>
        /// Creates Metrics object with provided values
        /// </summary>
        public Metrics()
        {
            OS = Entities.Device.OS;
            OSVersion = Entities.Device.OSVersion;
            Device = Entities.Device.DeviceName;
            Resolution = Entities.Device.Resolution;
            Carrier = Entities.Device.Carrier;
            AppVersion = Entities.Device.AppVersion;
            Locale = Entities.Device.Locale;
            Store = Entities.Device.Store;
            Density = Entities.Device.Density;
        }
    }
}

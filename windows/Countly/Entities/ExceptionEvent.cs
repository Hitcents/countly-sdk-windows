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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CountlySDK.Entities
{
    /// <summary>
    /// This class holds the data about an application exception
    /// </summary>
    [DataContract]
    internal class ExceptionEvent
    {
        //device metrics

        [DataMember(Name = "_os", EmitDefaultValue = true)]
        public string OS { get; set; }

        [DataMember(Name = "_os_version", EmitDefaultValue = true)]
        public string OSVersion { get; set; }

        [DataMember(Name = "_manufacture", EmitDefaultValue = true)]
        public string Manufacture { get; set; }

        [DataMember(Name = "_device", EmitDefaultValue = true)]
        public string Device { get; set; }

        [DataMember(Name = "_resolution", EmitDefaultValue = true)]
        public string Resolution { get; set; }

        [DataMember(Name = "_app_version", EmitDefaultValue = true)]
        public string AppVersion { get; set; }

        [DataMember(Name = "_locale", EmitDefaultValue = true)]
        public string Locale { get; set; }

        //state of device

        [DataMember(Name = "_orientation", EmitDefaultValue = true)]
        public string Orientation { get; set; }

        //error info

        [DataMember(Name = "_name", EmitDefaultValue = true)]
        public string Name { get; set; }

        [DataMember(Name = "_error", EmitDefaultValue = true)]
        public string Error { get; set; }

        [DataMember(Name = "_nonfatal", EmitDefaultValue = true)]
        public bool NonFatal { get; set; }

        [DataMember(Name = "_logs", EmitDefaultValue = true)]
        public string Logs { get; set; }

        [DataMember(Name = "_run", EmitDefaultValue = true)]
        public int Run { get; set; }

        //custom key/values provided by developers

        [DataMember(Name = "_custom", EmitDefaultValue = true)]
        public Dictionary<string, string> Custom { get; set; }

        public ExceptionEvent()
        { }

        public ExceptionEvent(string Error, string StackTrace, bool fatal, StringBuilder breadCrumb, DateTime startTime, Dictionary<string, string> customInfo)
        {
            //device metrics
            OS = CountlySDK.Entities.Device.OS;
            OSVersion = CountlySDK.Entities.Device.OSVersion;
            Manufacture = CountlySDK.Entities.Device.Manufacturer;
            Device = CountlySDK.Entities.Device.DeviceName;
            Resolution = CountlySDK.Entities.Device.Resolution;
            AppVersion = CountlySDK.Entities.Device.AppVersion;
            Locale = CountlySDK.Entities.Device.Locale;

            //state of device
            Orientation = CountlySDK.Entities.Device.Orientation;

            //error info
            this.Name = Error;
            this.Error = StackTrace;
            NonFatal = !fatal;
            Logs = breadCrumb.ToString();
            Run = (int)DateTime.UtcNow.Subtract(startTime).TotalSeconds;
            Custom = customInfo;
        }
    }
}

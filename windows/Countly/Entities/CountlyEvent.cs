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
using System.Linq;
using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    /// <summary>
    /// This class holds the data for a single Count.ly custom event instance.
    /// </summary>
    [DataContract]
    internal class CountlyEvent
    {
        /// <summary>
        /// Key attribute, must be non-empty
        /// </summary>
        [DataMember(Name = "key", EmitDefaultValue = true)]
        public string Key { get; set; }

        /// <summary>
        /// Count parameter, must me positive number
        /// </summary>
        [DataMember(Name = "count", EmitDefaultValue = true)]
        public int Count { get; set; }

        /// <summary>
        /// Sum parameter, can be null
        /// </summary>
        [DataMember(Name = "sum", EmitDefaultValue = true)]
        public double? Sum { get; set; }

        /// <summary>
        /// Dur parameter, I have no idea guys
        /// </summary>
        [DataMember(Name = "dur", EmitDefaultValue = true)]
        public double? Dur { get; set; }

        /// <summary>
        /// Segmentation parameter
        /// </summary>
        [IgnoreDataMember]
        public Segmentation Segmentation { get; internal set; }

        /// <summary>
        /// Segmentation json-ready object
        /// </summary>
        [DataMember(Name = "segmentation", EmitDefaultValue = true)]
        private Dictionary<string, string> segmentation
        {
            get
            {
                if (Segmentation == null || Segmentation.segmentation.Count == 0)
                    return null;

                return Segmentation.segmentation.ToDictionary(s => s.Key, s => s.Value);
            }
        }

        internal CountlyEvent()
        { }

        /// <summary>
        /// Create Countly event with provided values
        /// </summary>
        /// <param name="Key">Key attribute, must be non-empty</param>
        /// <param name="Count">Count parameter, must me positive number</param>
        /// <param name="Sum">Sum parameter, can be null</param>
        /// <param name="Dur">Dur parameter, I have no idea guys</param>
        /// <param name="Segmentation">Segmentation parameter</param>
        public CountlyEvent(string Key, int Count, double? Sum, double? Dur, Segmentation Segmentation)
        {
            if (String.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentException("Event Key must be non-empty string");
            }

            if (Count <= 0)
            {
                throw new ArgumentException("Event Count must be positive number");
            }

            this.Key = Key;
            this.Count = Count;
            this.Sum = Sum;
            this.Dur = Dur;
            this.Segmentation = Segmentation;
        }
    }
}

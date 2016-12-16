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
    /// Holds a dictionary of custom info values
    /// </summary>
    [DataContractAttribute]
    public class CustomInfo
    {
        internal delegate void CollectionChangedEventHandler();

        /// <summary>
        /// Raised when collection is changed
        /// </summary>
        internal event CollectionChangedEventHandler CollectionChanged;

        [DataMemberAttribute]
        internal readonly Dictionary<string, string> Items = new Dictionary<string, string>();

        /// <summary>
        /// Adds new custom item
        /// </summary>
        /// <param name="Name">item name</param>
        /// <param name="Value">item value</param>
        public void Add(string Name, string Value)
        {
            string existing;
            if (Items.TryGetValue(Name, out existing))
            {
                if (existing == Value)
                    return;

                if (Value == null)
                    Items.Remove(Name);
            }

            if (Value != null)
            {
                Items[Name] = Value;
            }

            CollectionChanged?.Invoke();
        }

        /// <summary>
        /// Removes custom item
        /// </summary>
        /// <param name="Name">item name</param>
        public void Remove(string Name)
        {
            if (Items.Remove(Name))
            {
                CollectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Clears items collection
        /// </summary>
        public void Clear()
        {
            Items.Clear();

            CollectionChanged?.Invoke();
        }

        /// <summary>
        /// Gets or sets item value based on provided item name
        /// </summary>
        /// <param name="name">item name</param>
        /// <returns>item value</returns>
        public string this[string name]
        {
            get
            {
                string value;
                Items.TryGetValue(name, out value);
                return value;
            }
            set
            {
                Add(name, value);
            }
        }
    }
}

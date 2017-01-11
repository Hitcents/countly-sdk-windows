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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    /// <summary>
    /// Holds user-specific info in json-ready format
    /// </summary>
    [DataContractAttribute]
    public class CountlyUserDetails
    {
        private string name;

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string username;

        /// <summary>
        /// Username or login info
        /// </summary>
        [DataMember(Name = "username", EmitDefaultValue = false)]
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                if (username != value)
                {
                    username = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string email;

        /// <summary>
        /// User email address
        /// </summary>
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                if (email != value)
                {
                    email = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string organization;

        /// <summary>
        /// User organization
        /// </summary>
        [DataMember(Name = "organization", EmitDefaultValue = false)]
        public string Organization
        {
            get
            {
                return organization;
            }
            set
            {
                if (organization != value)
                {
                    organization = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string phone;

        /// <summary>
        /// User phone
        /// </summary>
        [DataMember(Name = "phone", EmitDefaultValue = false)]
        public string Phone
        {
            get
            {
                return phone;
            }
            set
            {
                if (phone != value)
                {
                    phone = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string picture;

        /// <summary>
        /// Web URL to picture
        /// </summary>
        [DataMember(Name = "picture", EmitDefaultValue = false)]
        public string Picture
        {
            get
            {
                return picture;
            }
            set
            {
                if (picture != value)
                {
                    picture = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private string gender;

        /// <summary>
        /// User gender
        /// </summary>
        [DataMember(Name = "gender", EmitDefaultValue = false)]
        public string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                if (gender != value)
                {
                    gender = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private int? birthYear;

        /// <summary>
        /// User birth year
        /// </summary>
        [DataMember(Name = "byear", EmitDefaultValue = false)]
        public int? BirthYear
        {
            get
            {
                return birthYear;
            }
            set
            {
                if (birthYear != value)
                {
                    birthYear = value;

                    NotifyDetailsChanged();
                }
            }
        }

        private CustomInfo custom;

        /// <summary>
        /// User custom data
        /// </summary>
        [IgnoreDataMember]
        public CustomInfo Custom
        {
            get
            {
                return custom;
            }
            set
            {
                if (custom != value)
                {
                    if (custom != null)
                    {
                        custom.CollectionChanged -= NotifyDetailsChanged;   
                    }

                    if (value != null)
                    {
                        custom = value;

                        custom.CollectionChanged += NotifyDetailsChanged;
                    }
                    else
                    {
                        custom.Clear();
                    }

                    NotifyDetailsChanged();
                }
            }
        }

        /// <summary>
        /// Custom data ready for json serializer
        /// </summary>
        [DataMember(Name = "custom", EmitDefaultValue = false)]
        private Dictionary<string, string> _custom
        {
            get
            {
                if (Custom?.Items?.Count > 0)
                    return Custom.Items;

                return null;
            }
        }

        private void NotifyDetailsChanged()
        {
            //NOTE: leaving this method for now if we need to put the event back
            HasChanges = true;
        }

        [IgnoreDataMember]
        internal bool HasChanges { get; set; }

        public CountlyUserDetails()
        {
            Custom = new CustomInfo();
        }
    }
}

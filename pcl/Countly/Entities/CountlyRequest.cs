using CountlySDK.Helpers;
using System;
using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    [DataContract]
    internal class CountlyRequest
    {
        public static CountlyRequest CreateBeginSession()
        {
            return new CountlyRequest
            {
                SdkVersion = Countly.SdkVersion,
                BeginSession = "1",
                Metrics = new Metrics(),
            };
        }

        public static CountlyRequest CreateUpdateSession()
        {
            return new CountlyRequest
            {
                Duration = Countly.GetSessionDuration(),
            };
        }

        public static CountlyRequest CreateEndSession()
        {
            return new CountlyRequest
            {
                EndSession = "1",
                Duration = Countly.GetSessionDuration(),
            };
        }

        public CountlyRequest()
        {
            DeviceId = Device.DeviceId;
        }

        [DataMember(Name = "sdk_version", EmitDefaultValue = false)]
        public string SdkVersion { get; set; }

        [DataMember(Name = "device_id", EmitDefaultValue = false)]
        public string DeviceId { get; set; }

        [DataMember(Name = "begin_session", EmitDefaultValue = false)]
        public string BeginSession { get; set; }

        [DataMember(Name = "session_duration", EmitDefaultValue = false)]
        public int Duration { get; set; }

        [DataMember(Name = "end_session", EmitDefaultValue = false)]
        public string EndSession { get; set; }

        [DataMember(Name = "timestamp", EmitDefaultValue = false)]
        public int TimeStamp { get; set; }

        [DataMember(Name = "user_details", EmitDefaultValue = false)]
        public CountlyUserDetails UserDetails { get; set; }

        [DataMember(Name = "metrics", EmitDefaultValue = false)]
        public Metrics Metrics { get; set; }

        [DataMember(Name = "events", EmitDefaultValue = false)]
        public CountlyEvent[] Events { get; set; }

        [DataMember(Name = "crash", EmitDefaultValue = false)]
        public ExceptionEvent Exception { get; set; }
    }
}

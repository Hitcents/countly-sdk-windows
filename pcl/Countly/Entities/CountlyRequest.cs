using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace CountlySDK.Entities
{
    internal class CountlyRequest
    {
        private class Names
        {
            public const string AppKey = "app_key";
            public const string DeviceId = "device_id";
            public const string UserDetails = "user_details";
            public const string Events = "events";
            public const string Exception = "crash";
        }

        public string AppKey { get; set; }

        public string DeviceId { get; set; }

        public SessionEvent SessionEvent { get; set; }

        public CountlyUserDetails UserDetails { get; set; }

        public List<CountlyEvent> Events { get; set; }

        public ExceptionEvent Exception { get; set; }

        public FormUrlEncodedContent ToContent()
        {
            var dict = SessionEvent?.Content ?? new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(AppKey))
                dict[Names.AppKey] = AppKey;

            if (!string.IsNullOrEmpty(DeviceId))
                dict[Names.DeviceId] = DeviceId;

            if (UserDetails != null)
                dict[Names.UserDetails] = JsonConvert.SerializeObject(UserDetails, Api.JsonSettings);

            if (Events != null)
                dict[Names.Events] = JsonConvert.SerializeObject(Events, Api.JsonSettings);

            if (Exception != null)
                dict[Names.Exception] = JsonConvert.SerializeObject(Exception, Api.JsonSettings);

            return new FormUrlEncodedContent(dict);
        }
    }
}

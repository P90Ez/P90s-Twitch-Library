using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Stream_Offline : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "";

        internal Stream_Offline(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger) 
            : base(Parent, "stream.offline", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) {}


        /// <summary>
        /// Will be invoked when the specified broadcaster stops a stream.
        /// </summary>
        public event EventHandler<Stream_Offline_Event> Offline;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Stream_Offline_Event>();
            if (data == null) return;
            Offline?.Invoke(this, data);
        }

        public class Stream_Offline_Event
        {
            /// <summary>
            /// The broadcaster’s user id.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; set; }

            /// <summary>
            /// The broadcaster’s user login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; set; }

            /// <summary>
            /// The broadcaster’s user display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; set; }
        }
    }
}

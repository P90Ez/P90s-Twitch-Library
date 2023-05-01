using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Stream_Online : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "";

        internal Stream_Online(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger) 
            : base(Parent, "stream.online", new List<KeyValuePair<string, string>>() {  new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) {}


        /// <summary>
        /// Will be invoked when the specified broadcaster starts a stream.
        /// </summary>
        public event EventHandler<Stream_Online_Event> Online;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Stream_Online_Event>();
            if (data == null) return;
            Online?.Invoke(this, data);
        }

        public class Stream_Online_Event
        {
            /// <summary>
            /// The id of the stream.
            /// </summary>
            [JsonProperty("id")]
            public string StreamId { get; set; }

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

            /// <summary>
            /// The stream type. Valid values are: live, playlist, watch_party, premiere, rerun.
            /// </summary>
            [JsonProperty("type")]
            public string StreamType { get; set; }

            /// <summary>
            /// The timestamp at which the stream went online at.
            /// </summary>
            [JsonProperty("started_at")]
            public DateTime StartedAt { get; set; }
        }
    }
}

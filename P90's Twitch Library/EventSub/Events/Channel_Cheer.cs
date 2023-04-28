using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Cheer : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "bits:read";

        internal Channel_Cheer(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger) : base(Parent, "channel.cheer", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) { }

        /// <summary>
        /// Will be invoked when a user cheers on the specified channel.
        /// </summary>
        public event EventHandler<Channel_Cheer_Event> Cheer;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Cheer_Event>();
            if (data == null) return;
            Cheer?.Invoke(this, data);
        }

        public class Channel_Cheer_Event
        {
            /// <summary>
            /// Whether the user cheered anonymously or not.
            /// </summary>
            [JsonProperty("is_anonymous")]
            public bool IsAnonymous { get; internal set; }

            /// <summary>
            /// The user ID for the user who cheered on the specified channel. This is null if is_anonymous is true.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login for the user who cheered on the specified channel. This is null if is_anonymous is true.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name for the user who cheered on the specified channel. This is null if is_anonymous is true.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The requested broadcaster ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string Broadcaster_Id { get; internal set; }

            /// <summary>
            /// The requested broadcaster login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; internal set; }

            /// <summary>
            /// The requested broadcaster display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; internal set; }

            /// <summary>
            /// The message sent with the cheer.
            /// </summary>
            [JsonProperty("message")]
            public string Message { get; internal set; }

            /// <summary>
            /// The number of bits cheered.
            /// </summary>
            [JsonProperty("bits")]
            public int Bits { get; internal set; }
        }
    }
}

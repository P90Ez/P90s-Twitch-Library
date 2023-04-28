using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Follow : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "moderator:read:followers";

        internal Channel_Follow(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger)
            : base(Parent, "channel.follow", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id), new KeyValuePair<string, string>("moderator_user_id", Parent._Creds.UserId) }, 2, Logger) {}

        
        /// <summary>
        /// Will be invoked when the specified channel recieves a follow.
        /// </summary>
        public event EventHandler<Channel_Follow_Event> Followed;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Follow_Event>();
            if (data == null) return;
            Followed?.Invoke(this, data);
        }

        public class Channel_Follow_Event
        {
            /// <summary>
            /// The user ID for the user now following the specified channel.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login for the user now following the specified channel.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name for the user now following the specified channel.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The requested broadcaster ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; internal set; }

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
            /// Ttimestamp of when the follow occurred.
            /// </summary>
            [JsonProperty("followed_at")]
            public DateTime FollowedAt { get; internal set; }
        }
    }
}

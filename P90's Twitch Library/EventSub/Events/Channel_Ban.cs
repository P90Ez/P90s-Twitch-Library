using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Ban : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "channel:moderate";

        internal Channel_Ban(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger) 
            : base(Parent, "channel.ban", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) {}

        /// <summary>
        /// Will be invoked when a viewer is timed out or banned from the specified channel.
        /// </summary>
        public event EventHandler<Channel_Ban_Event> Banned;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Ban_Event>();
            if (data == null) return;
            Banned?.Invoke(this, data);
        }

        public class Channel_Ban_Event
        {
            /// <summary>
            /// The user ID for the user who was banned on the specified channel.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; set; }

            /// <summary>
            /// The user login for the user who was banned on the specified channel.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; set; }

            /// <summary>
            /// The user display name for the user who was banned on the specified channel.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; set; }

            /// <summary>
            /// The requested broadcaster ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; set; }

            /// <summary>
            /// The requested broadcaster login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; set; }

            /// <summary>
            /// The requested broadcaster display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; set; }

            /// <summary>
            /// The user ID of the issuer of the ban.
            /// </summary>
            [JsonProperty("moderator_user_id")]
            public string Moderator_Id { get; set; }

            /// <summary>
            /// The user login of the issuer of the ban.
            /// </summary>
            [JsonProperty("moderator_user_login")]
            public string ModeratorLogin { get; set; }

            /// <summary>
            /// The user name of the issuer of the ban.
            /// </summary>
            [JsonProperty("moderator_user_name")]
            public string ModeratorName { get; set; }

            /// <summary>
            /// The reason behind the ban.
            /// </summary>
            [JsonProperty("reason")]
            public string Reason { get; set; }

            /// <summary>
            /// The date and time of when the user was banned or put in a timeout.
            /// </summary>
            [JsonProperty("banned_at")]
            public DateTime BannedAt { get; set; }

            /// <summary>
            /// The date and time of when the timeout ends. Is null if the user was banned instead of put in a timeout.
            /// </summary>
            [JsonProperty("ends_at")]
            public DateTime EndsAt { get; set; }

            /// <summary>
            /// Indicates whether the ban is permanent (true) or a timeout (false). If true, <see cref="EndsAt"/> will be null.
            /// </summary>
            [JsonProperty("is_permanent")]
            public bool IsPermanent { get; set; }
        }
    }
}

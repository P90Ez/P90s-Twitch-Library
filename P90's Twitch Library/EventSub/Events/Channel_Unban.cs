using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Unban : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "channel:moderate";

        internal Channel_Unban(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger)
            : base(Parent, "channel.unban", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) { }

        /// <summary>
        /// Will be invoked when a viewer is unbanned from the specified channel.
        /// </summary>
        public event EventHandler<Channel_Unban_Event> Unbanned;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Unban_Event>();
            if (data == null) return;
            Unbanned?.Invoke(this, data);
        }

        public class Channel_Unban_Event
        {
            /// <summary>
            /// The user id for the user who was unbanned on the specified channel.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; set; }

            /// <summary>
            /// The user login for the user who was unbanned on the specified channel.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; set; }

            /// <summary>
            /// The user display name for the user who was unbanned on the specified channel.
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
            /// The user ID of the issuer of the unban.
            /// </summary>
            [JsonProperty("moderator_user_id")]
            public string ModeratorId { get; set; }

            /// <summary>
            /// The user login of the issuer of the unban.
            /// </summary>
            [JsonProperty("moderator_user_login")]
            public string ModeratorLogin { get; set; }

            /// <summary>
            /// The user name of the issuer of the unban.
            /// </summary>
            [JsonProperty("moderator_user_name")]
            public string ModeratorName { get; set; }
        }
    }
}

using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Update : IStandardEvent
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "";

        internal Channel_Update(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger) 
            : base(Parent, "channel.update", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) {}

        /// <summary>
        /// Will be invoked when the broadcaster updates the category, title, mature flag, or broadcast language for their channel.
        /// </summary>
        public event EventHandler<Channel_Update_Event> Update;

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Update_Event>();
            if (data == null) return;
            Update?.Invoke(this, data);
        }
        
        public class Channel_Update_Event
        {
            /// <summary>
            /// The broadcaster’s user ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string ID { get; internal set; }

            /// <summary>
            /// The broadcaster’s user login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string LoginName { get; internal set; }

            /// <summary>
            /// The broadcaster’s user display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The channel’s stream title.
            /// </summary>
            [JsonProperty("title")]
            public string StreamTitle { get; internal set; }

            /// <summary>
            /// The channel’s broadcast language. - The value is an ISO 639-1 two-letter language code (for example, en for English). The value is set to “other” if the language is not a Twitch supported language.
            /// </summary>
            [JsonProperty("language")]
            public string Language { get; internal set; }

            /// <summary>
            /// The channel’s (game) category ID.
            /// </summary>
            [JsonProperty("category_id")]
            public string CategoryId { get; internal set; }

            /// <summary>
            /// The (game) category name.
            /// </summary>
            [JsonProperty("category_name")]
            public string CategoryName { get; internal set; }

            /// <summary>
            /// A boolean identifying whether the channel is flagged as mature.
            /// </summary>
            [JsonProperty("is_mature")]
            public bool IsMature { get; internal set; }
        }
    }
}

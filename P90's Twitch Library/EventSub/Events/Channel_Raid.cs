using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static P90Ez.Twitch.EventSub.Events.Channel_Raid;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Raid
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "";

        internal Channel_Raid(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger)
        {
            new Channel_Raid_ToBroadcaster(Parent, broadcaster_user_id, Logger, this);
            new Channel_Raid_FromBroadcaster(Parent, broadcaster_user_id, Logger, this);
        }

        /// <summary>
        /// Will be invoked when a broadcaster raids the specified broadcaster. [incomming raids]
        /// </summary>
        public event EventHandler<Channel_Raid_Event> ToBroadcaster;
        internal void ToBroadcaster_Invoke(object sender, Channel_Raid_Event data)
        {
            ToBroadcaster?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when the specified broadcaster raids another broadcaster. [outgoing raids]
        /// </summary>
        public event EventHandler<Channel_Raid_Event> FromBroadcaster;
        internal void FromBroadcaster_Invoke(object sender, Channel_Raid_Event data)
        {
            FromBroadcaster?.Invoke(sender, data);
        }

        public class Channel_Raid_Event
        {
            /// <summary>
            /// The broadcaster ID that created the raid.
            /// </summary>
            [JsonProperty("from_broadcaster_user_id")]
            public string From_BroadcasterId { get; set; }

            /// <summary>
            /// The broadcaster login that created the raid.
            /// </summary>
            [JsonProperty("from_broadcaster_user_login")]
            public string From_BroadcasterLogin { get; set; }

            /// <summary>
            /// The broadcaster display name that created the raid.
            /// </summary>
            [JsonProperty("from_broadcaster_user_name")]
            public string From_BroadcasterName { get; set; }

            /// <summary>
            /// The broadcaster ID that received the raid.
            /// </summary>
            [JsonProperty("to_broadcaster_user_id")]
            public string To_BroadcasterId { get; set; }

            /// <summary>
            /// The broadcaster login that received the raid.
            /// </summary>
            [JsonProperty("to_broadcaster_user_login")]
            public string To_BroadcasterLogin { get; set; }

            /// <summary>
            /// The broadcaster display name that received the raid.
            /// </summary>
            [JsonProperty("to_broadcaster_user_name")]
            public string To_BroadcasterName { get; set; }

            /// <summary>
            /// The number of viewers in the raid.
            /// </summary>
            [JsonProperty("viewers")]
            public int Viewers { get; set; }
        }
    }
    public class Channel_Raid_ToBroadcaster : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Raid PartnerObject { get; }

        internal Channel_Raid_ToBroadcaster(EventSubInstance Parent, string to_broadcaster_user_id, ILogger Logger, Channel_Raid PartnerObject) : base(Parent, "channel.raid", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("to_broadcaster_user_id", to_broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Raid_Event>();
            if (data == null) return;
            PartnerObject.ToBroadcaster_Invoke(this, data);
        }
    }

    public class Channel_Raid_FromBroadcaster : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Raid PartnerObject { get; }

        internal Channel_Raid_FromBroadcaster(EventSubInstance Parent, string from_broadcaster_user_id, ILogger Logger, Channel_Raid PartnerObject) : base(Parent, "channel.raid", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("from_broadcaster_user_id", from_broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Raid_Event>();
            if (data == null) return;
            PartnerObject.FromBroadcaster_Invoke(this, data);
        }
    }
}

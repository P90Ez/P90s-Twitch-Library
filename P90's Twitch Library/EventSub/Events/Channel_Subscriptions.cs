using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static P90Ez.Twitch.EventSub.Events.Channel_Subscribe;
using static P90Ez.Twitch.EventSub.Events.Channel_Subscription_End;
using static P90Ez.Twitch.EventSub.Events.Channel_Subscription_Gift;
using static P90Ez.Twitch.EventSub.Events.Channel_Subscription_Message;

namespace P90Ez.Twitch.EventSub.Events
{
    public class Channel_Subscriptions
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "channel:read:subscriptions";

        internal Channel_Subscriptions(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger)
        {
            new Channel_Subscribe(Parent, broadcaster_user_id, Logger, this);
            new Channel_Subscription_End(Parent, broadcaster_user_id, Logger, this);
            new Channel_Subscription_Gift(Parent, broadcaster_user_id, Logger, this);
            new Channel_Subscription_Message(Parent, broadcaster_user_id, Logger, this);
        }

        /// <summary>
        /// Will be invoked when the specified channel specified channel receives a subscriber. This does not include resubscribes. [first time subscribers, gifted sub recievers]
        /// </summary>
        public event EventHandler<Channel_Subscribe_Event> Subscribed;
        internal void Subscribed_Invoke(object sender, Channel_Subscribe_Event data)
        {
            Subscribed?.Invoke(sender, data);
        }


        /// <summary>
        /// Will be invoked when a subscription to the specified channel expires. [any]
        /// </summary>
        public event EventHandler<Channel_Subscription_End_Event> Subscription_Expired;
        internal void Subscription_Expired_Invoke(object sender, Channel_Subscription_End_Event data)
        {
            Subscription_Expired?.Invoke(sender, data);
        }


        /// <summary>
        /// Will be invoked when a user gives one or more gifted subscriptions in a channel.
        /// </summary>
        public event EventHandler<Channel_Subscription_Gift_Event> GiftedSubscription;
        internal void GiftedSubscription_Invoke(object sender, Channel_Subscription_Gift_Event data)
        {
            GiftedSubscription?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when a user sends a resubscription chat message in a specific channel. [resubscribers]
        /// </summary>
        public event EventHandler<Channel_Subscription_Message_Event> Resubscribed;
        internal void Resubscribed_Invoke(object sender, Channel_Subscription_Message_Event data)
        {
            Resubscribed?.Invoke(sender, data);
        }
    }

    public class Channel_Subscribe : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Subscriptions PartnerObject { get; }

        internal Channel_Subscribe(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, Channel_Subscriptions PartnerObject) 
            : base(Parent, "channel.subscribe", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) 
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Subscribe_Event>();
            if (data == null) return;
            PartnerObject.Subscribed_Invoke(this, data);
        }

        public class Channel_Subscribe_Event
        {
            /// <summary>
            /// The user ID for the user who subscribed to the specified channel.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login for the user who subscribed to the specified channel.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name for the user who subscribed to the specified channel.
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
            /// The tier of the subscription. Valid values are 1000, 2000, and 3000.
            /// </summary>
            [JsonProperty("tier")]
            public string Tier { get; internal set; }

            /// <summary>
            /// Whether the subscription is a gift.
            /// </summary>
            [JsonProperty("is_gift")]
            public bool IsGift { get; internal set; }
        }
    }

    public class Channel_Subscription_End : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Subscriptions PartnerObject { get; }

        internal Channel_Subscription_End(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, Channel_Subscriptions PartnerObject)
            : base(Parent, "channel.subscription.end", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) 
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Subscription_End_Event>();
            if (data == null) return;
            PartnerObject.Subscription_Expired_Invoke(this, data);
        }

        public class Channel_Subscription_End_Event
        {
            /// <summary>
            /// The user ID for the user whose subscription ended.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login for the user whose subscription ended.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name for the user whose subscription ended.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The broadcaster user ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; internal set; }

            /// <summary>
            /// The broadcaster login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; internal set; }

            /// <summary>
            /// The broadcaster display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; internal set; }

            /// <summary>
            /// The tier of the subscription that ended. Valid values are 1000, 2000, and 3000.
            /// </summary>
            [JsonProperty("tier")]
            public string Tier { get; internal set; }

            /// <summary>
            /// Whether the subscription was a gift.
            /// </summary>
            [JsonProperty("is_gift")]
            public bool IsGift { get; internal set; }
        }
    }

    public class Channel_Subscription_Gift : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Subscriptions PartnerObject { get; }

        internal Channel_Subscription_Gift(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, Channel_Subscriptions PartnerObject)
            : base(Parent, "channel.subscription.gift", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) 
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Subscription_Gift_Event>();
            if (data == null) return;
            PartnerObject.GiftedSubscription_Invoke(this, data);
        }

        public class Channel_Subscription_Gift_Event
        {
            /// <summary>
            /// The user ID of the user who sent the subscription gift. Set to null if it was an anonymous subscription gift.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login of the user who sent the gift. Set to null if it was an anonymous subscription gift.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name of the user who sent the gift. Set to null if it was an anonymous subscription gift.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The broadcaster user ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; internal set; }

            /// <summary>
            /// The broadcaster login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; internal set; }

            /// <summary>
            /// The broadcaster display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; internal set; }

            /// <summary>
            /// The number of subscriptions in the subscription gift.
            /// </summary>
            [JsonProperty("total")]
            public int Amount { get; internal set; }

            /// <summary>
            /// The tier of subscriptions in the subscription gift.
            /// </summary>
            [JsonProperty("tier")]
            public string Tier { get; internal set; }

            /// <summary>
            /// The number of subscriptions gifted by this user in the channel. This value is null for anonymous gifts or if the gifter has opted out of sharing this information.
            /// </summary>
            [JsonProperty("cumulative_total")]
            public int? TotalGifted { get; internal set; }

            /// <summary>
            /// Whether the subscription gift was anonymous.
            /// </summary>
            [JsonProperty("is_anonymous")]
            public bool IsAnonymous { get; internal set; }
        }
    }

    public class Channel_Subscription_Message : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private Channel_Subscriptions PartnerObject { get; }

        internal Channel_Subscription_Message(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, Channel_Subscriptions PartnerObject)
            : base(Parent, "channel.subscription.message", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) 
        { 
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<Channel_Subscription_Message_Event>();
            if (data == null) return;
            PartnerObject.Resubscribed_Invoke(this, data);
        }

        public class Channel_Subscription_Message_Event
        {
            /// <summary>
            /// The user ID of the user who sent a resubscription chat message.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }

            /// <summary>
            /// The user login of the user who sent a resubscription chat message.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; internal set; }

            /// <summary>
            /// The user display name of the user who a resubscription chat message.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; internal set; }

            /// <summary>
            /// The broadcaster user ID.
            /// </summary>
            [JsonProperty("broadcaster_user_id")]
            public string BroadcasterId { get; internal set; }

            /// <summary>
            /// The broadcaster login.
            /// </summary>
            [JsonProperty("broadcaster_user_login")]
            public string BroadcasterLogin { get; internal set; }

            /// <summary>
            /// The broadcaster display name.
            /// </summary>
            [JsonProperty("broadcaster_user_name")]
            public string BroadcasterName { get; internal set; }

            /// <summary>
            /// The tier of the user’s subscription. Possible values are: 1000, 2000, 3000
            /// </summary>
            [JsonProperty("tier")]
            public string Tier { get; internal set; }

            /// <summary>
            /// An object that contains the resubscription message and emote information needed to recreate the message.
            /// </summary>
            [JsonProperty("message")]
            public Message message { get; internal set; }

            /// <summary>
            /// The total number of months the user has been subscribed to the channel.
            /// </summary>
            [JsonProperty("cumulative_months")]
            public int TotalMonths { get; internal set; }

            /// <summary>
            /// The number of consecutive months the user’s current subscription has been active. This value is null if the user has opted out of sharing this information.
            /// </summary>
            [JsonProperty("streak_months")]
            public int? StreakMonths { get; internal set; }

            /// <summary>
            /// The month duration of the subscription.
            /// </summary>
            [JsonProperty("duration_months")]
            public int DurationMonths { get; internal set; }

            public class Emote
            {
                /// <summary>
                /// The index of where the Emote starts in the text.
                /// </summary>
                [JsonProperty("begin")]
                public int BeginIndex { get; internal set; }

                /// <summary>
                /// The index of where the Emote ends in the text.
                /// </summary>
                [JsonProperty("end")]
                public int EndIndex { get; internal set; }

                /// <summary>
                /// The emote ID.
                /// </summary>
                [JsonProperty("id")]
                public string EmoteId { get; internal set; }
            }

            public class Message
            {
                /// <summary>
                /// The text of the resubscription chat message.
                /// </summary>
                [JsonProperty("text")]
                public string Text { get; internal set; }

                /// <summary>
                /// A List that includes the emote ID and start and end positions for where the emote appears in the text.
                /// </summary>
                [JsonProperty("emotes")]
                public List<Emote> Emotes { get; internal set; }
            }
        }
    }
}

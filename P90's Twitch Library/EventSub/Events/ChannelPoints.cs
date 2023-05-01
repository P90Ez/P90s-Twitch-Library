using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static P90Ez.Twitch.EventSub.Events.ChannelPoints;

namespace P90Ez.Twitch.EventSub.Events
{
    public class ChannelPoints
    {
        /// <summary>
        /// Requiered Scopes to use this subscription.
        /// </summary>
        public static string RequiredScopes { get; } = "channel:read:redemptions";

        public ChannelPoints(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger)
        {
            new ChannelPoints_RewardAdd(Parent, broadcaster_user_id, Logger, this);
            new ChannelPoints_RewardUpdate(Parent, broadcaster_user_id, Logger, this);
            new ChannelPoints_RewardRemove(Parent, broadcaster_user_id, Logger, this);
            new ChannelPoints_Redeemed(Parent, broadcaster_user_id, Logger, this);
            new ChannelPoints_RedemptionUpdate(Parent, broadcaster_user_id, Logger, this);
        }

        /// <summary>
        /// Will be invoked when a custom channel points reward has been created for the specified channel.
        /// </summary>
        public event EventHandler<ChannelPoints_Event> RewardAdded;
        internal void Invoke_RewardAdded(object sender, ChannelPoints_Event data)
        {
            RewardAdded?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when a custom channel points reward has been updated for the specified channel.
        /// </summary>
        public event EventHandler<ChannelPoints_Event> RewardUpdated;
        internal void Invoke_RewardUpdated(object sender, ChannelPoints_Event data)
        {
            RewardUpdated?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when a custom channel points reward has been removed from the specified channel.
        /// </summary>
        public event EventHandler<ChannelPoints_Event> RewardRemoved;
        internal void Invoke_RewardRemoved(object sender, ChannelPoints_Event data)
        {
            RewardRemoved?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when a viewer has redeemed a custom channel points reward on the specified channel.
        /// </summary>
        public event EventHandler<ChannelPoints_RedemptionEvent> RewardRedeemed;
        internal void Invoke_RewardRedeemed(object sender, ChannelPoints_RedemptionEvent data)
        {
            RewardRedeemed?.Invoke(sender, data);
        }

        /// <summary>
        /// Will be invoked when a redemption of a channel points custom reward has been updated for the specified channel.
        /// </summary>
        public event EventHandler<ChannelPoints_RedemptionEvent> RedemptionUpdated;
        internal void Invoke_RedemptionUpdated(object sender, ChannelPoints_RedemptionEvent data)
        {
            RedemptionUpdated?.Invoke(sender, data);
        }

        public class ChannelPoints_Event
        {
            /// <summary>
            /// The reward identifier.
            /// </summary>
            [JsonProperty("id")]
            public string RewardId { get; set; }

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
            /// Is the reward currently enabled. If false, the reward won’t show up to viewers.
            /// </summary>
            [JsonProperty("is_enabled")]
            public bool IsEnabled { get; set; }

            /// <summary>
            /// Is the reward currently paused. If true, viewers can’t redeem.
            /// </summary>
            [JsonProperty("is_paused")]
            public bool IsPaused { get; set; }

            /// <summary>
            /// Is the reward currently in stock. If false, viewers can’t redeem.
            /// </summary>
            [JsonProperty("is_in_stock")]
            public bool InStock { get; set; }

            /// <summary>
            /// The reward title.
            /// </summary>
            [JsonProperty("title")]
            public string Title { get; set; }

            /// <summary>
            /// The reward cost.
            /// </summary>
            [JsonProperty("cost")]
            public int Cost { get; set; }

            /// <summary>
            /// The reward description.
            /// </summary>
            [JsonProperty("prompt")]
            public string Description { get; set; }

            /// <summary>
            /// Does the viewer need to enter information when redeeming the reward.
            /// </summary>
            [JsonProperty("is_user_input_required")]
            public bool UserInputRequired { get; set; }

            /// <summary>
            /// Should redemptions be set to fulfilled status immediately when redeemed and skip the request queue instead of the normal unfulfilled status.
            /// </summary>
            [JsonProperty("should_redemptions_skip_request_queue")]
            public bool SkipRequestQueue { get; set; }

            /// <summary>
            /// Timestamp of the cooldown expiration. null if the reward isn’t on cooldown.
            /// </summary>
            [JsonProperty("cooldown_expires_at")]
            public DateTime CooldownExpiration { get; set; }

            /// <summary>
            /// The number of redemptions redeemed during the current live stream. Counts against the max_per_stream limit. null if the broadcasters stream isn’t live or max_per_stream isn’t enabled.
            /// </summary>
            [JsonProperty("redemptions_redeemed_current_stream")]
            public int RedemptionAmount { get; set; }

            /// <summary>
            /// Whether a maximum per stream is enabled and what the maximum is.
            /// </summary>
            [JsonProperty("max_per_stream")]
            public MaxRedemtions MaxRedemptions_PerStream { get; set; }

            /// <summary>
            /// Whether a maximum per user per stream is enabled and what the maximum is.
            /// </summary>
            [JsonProperty("max_per_user_per_stream")]
            public MaxRedemtions MaxRedemtions_PerStream_PerUSer { get; set; }

            /// <summary>
            /// Whether a cooldown is enabled and what the cooldown is in seconds.
            /// </summary>
            [JsonProperty("global_cooldown")]
            public Cooldown GlobalCooldown { get; set; }

            /// <summary>
            /// Custom background color for the reward. Format: Hex with # prefix. Example: #FA1ED2.
            /// </summary>
            [JsonProperty("background_color")]
            public string BackgroundColor { get; set; }

            /// <summary>
            /// Set of custom images of 1x, 2x and 4x sizes for the reward. Can be null if no images have been uploaded.
            /// </summary>
            [JsonProperty("image")]
            public Image CumstomImage { get; set; }

            /// <summary>
            /// Set of default images of 1x, 2x and 4x sizes for the reward.
            /// </summary>
            [JsonProperty("default_image")]
            public Image DefaultImage { get; set; }
            
            public class Cooldown
            {
                /// <summary>
                /// Is the setting enabled.
                /// </summary>
                [JsonProperty("is_enabled")]
                public bool IsEnabled { get; set; }

                /// <summary>
                /// The cooldown in seconds.
                /// </summary>
                [JsonProperty("seconds")]
                public int CooldownAmount { get; set; }
            }

            public class Image
            {
                /// <summary>
                /// URL for the image at 1x size.
                /// </summary>
                [JsonProperty("url_1x")]
                public string URL1x { get; set; }

                /// <summary>
                /// URL for the image at 2x size.
                /// </summary>
                [JsonProperty("url_2x")]
                public string URL2x { get; set; }

                /// <summary>
                /// URL for the image at 4x size.
                /// </summary>
                [JsonProperty("url_4x")]
                public string URL4x { get; set; }
            }

            public class MaxRedemtions
            {
                /// <summary>
                /// Is the setting enabled.
                /// </summary>
                [JsonProperty("is_enabled")]
                public bool IsEnabled { get; set; }

                /// <summary>
                /// The redemption limit.
                /// </summary>
                [JsonProperty("value")]
                public int Amount { get; set; }
            }

        }
        public class ChannelPoints_RedemptionEvent
        {
            /// <summary>
            /// The redemption identifier.
            /// </summary>
            [JsonProperty("id")]
            public string RedemptionId { get; set; }

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
            /// User ID of the user that redeemed the reward.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; set; }

            /// <summary>
            /// Login of the user that redeemed the reward.
            /// </summary>
            [JsonProperty("user_login")]
            public string UserLogin { get; set; }

            /// <summary>
            /// Display name of the user that redeemed the reward.
            /// </summary>
            [JsonProperty("user_name")]
            public string UserName { get; set; }

            /// <summary>
            /// The user input provided. Empty string if not provided.
            /// </summary>
            [JsonProperty("user_input")]
            public string UserInput { get; set; }

            /// <summary>
            /// Defaults to unfulfilled. Possible values are unknown, unfulfilled, fulfilled, and canceled.
            /// </summary>
            [JsonProperty("status")]
            public string Status { get; set; }

            /// <summary>
            /// Basic information about the reward that was redeemed, at the time it was redeemed.
            /// </summary>
            [JsonProperty("reward")]
            public RewardInfo Reward { get; set; }

            /// <summary>
            /// Timestamp of when the reward was redeemed.
            /// </summary>
            [JsonProperty("redeemed_at")]
            public string RedeemedAt { get; set; }
            public class RewardInfo
            {
                /// <summary>
                /// The reward identifier.
                /// </summary>
                [JsonProperty("id")]
                public string RewardId { get; set; }

                /// <summary>
                /// The reward name.
                /// </summary>
                [JsonProperty("title")]
                public string Title { get; set; }

                /// <summary>
                /// The reward cost.
                /// </summary>
                [JsonProperty("cost")]
                public int Cost { get; set; }

                /// <summary>
                /// The reward description.
                /// </summary>
                [JsonProperty("prompt")]
                public string Description { get; set; }
            }
        }
    }
    internal class ChannelPoints_RewardAdd : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private ChannelPoints PartnerObject { get; }

        internal ChannelPoints_RewardAdd(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, ChannelPoints PartnerObject) 
            : base(Parent, "channel.channel_points_custom_reward.add", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger) 
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<ChannelPoints_Event>();
            if (data == null) return;
            PartnerObject.Invoke_RewardAdded(this, data);
        }
    }
    internal class ChannelPoints_RewardUpdate : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private ChannelPoints PartnerObject { get; }

        internal ChannelPoints_RewardUpdate(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, ChannelPoints PartnerObject) 
            : base(Parent, "channel.channel_points_custom_reward.update", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<ChannelPoints_Event>();
            if (data == null) return;
            PartnerObject.Invoke_RewardUpdated(this, data);
        }
    }
    internal class ChannelPoints_RewardRemove : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private ChannelPoints PartnerObject { get; }

        internal ChannelPoints_RewardRemove(EventSubInstance Parent, string broadcaster_user_id,  ILogger Logger, ChannelPoints PartnerObject) 
            : base(Parent, "channel.channel_points_custom_reward.remove", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<ChannelPoints_Event>();
            if (data == null) return;
            PartnerObject.Invoke_RewardRemoved(this, data);
        }
    }
    internal class ChannelPoints_Redeemed : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private ChannelPoints PartnerObject { get; }

        internal ChannelPoints_Redeemed(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, ChannelPoints PartnerObject) 
            : base(Parent, "channel.channel_points_custom_reward_redemption.add", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<ChannelPoints_RedemptionEvent>();
            if (data == null) return;
            PartnerObject.Invoke_RewardRedeemed(this, data);
        }
    }
    internal class ChannelPoints_RedemptionUpdate : IStandardEvent
    {
        /// <summary>
        /// Management object, contains all events.
        /// </summary>
        private ChannelPoints PartnerObject { get; }

        internal ChannelPoints_RedemptionUpdate(EventSubInstance Parent, string broadcaster_user_id, ILogger Logger, ChannelPoints PartnerObject) 
            : base(Parent, "channel.channel_points_custom_reward_redemption.update", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("broadcaster_user_id", broadcaster_user_id) }, 1, Logger)
        {
            this.PartnerObject = PartnerObject;
        }

        internal override void TriggerEvent(JsonStructure._Payload payload)
        {
            var data = payload.Event.ToObject<ChannelPoints_RedemptionEvent>();
            if (data == null) return;
            PartnerObject.Invoke_RedemptionUpdated(this, data);
        }
    }
}
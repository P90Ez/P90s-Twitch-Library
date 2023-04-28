using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace P90Ez.Twitch.EventSub
{
    internal class JsonStructure
    {
        /// <summary>
        /// An object that identifies the message.
        /// </summary>
        [JsonProperty("metadata")]
        public _Metadata Metadata { get; set; }

        /// <summary>
        /// An object that contains the message.
        /// </summary>
        [JsonProperty("payload")]
        public _Payload Payload { get; set; }

        internal class _Metadata
        {
            /// <summary>
            /// An ID that uniquely identifies the message. Twitch sends messages at least once, but if Twitch is unsure of whether you received a notification, it’ll resend the message. This means you may receive a notification twice. If Twitch resends the message, the message ID will be the same.
            /// </summary>
            [JsonProperty("message_id")]
            public string ID { get; set; }

            /// <summary>
            /// The type of message.
            /// </summary>
            [JsonProperty("message_type")]
            public string Type { get; set; }

            /// <summary>
            /// The UTC date and time that the message was sent.
            /// </summary>
            [JsonProperty("message_timestamp")]
            public DateTime TimeStamp { get; set; }

            /// <summary>
            /// The type of event sent in the message.
            /// </summary>
            [JsonProperty("subscription_type")]
            public string SubscriptionType { get; set; }

            /// <summary>
            /// The version number of the subscription type’s definition. This is the same value specified in the subscription request.
            /// </summary>
            [JsonProperty("subscription_version")]
            public string SubscriptionVersion { get; set; }
        }
        internal class _Payload
        {
            /// <summary>
            /// An object that contains information about the connection.
            /// </summary>
            [JsonProperty("session")]
            public _Session Session { get; set; }

            /// <summary>
            /// An object that contains information about your subscription.
            /// </summary>
            [JsonProperty("subscription")]
            public _Subscription Subscription { get; set; }

            /// <summary>
            /// The event’s data. For information about the event’s data, see the subscription type’s description in <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types">Subscription Types</see>.
            /// </summary>
            [JsonProperty("event")]
            public object Event { get; set; }

            internal class _Session
            {
                /// <summary>
                /// An ID that uniquely identifies this WebSocket connection. Use this ID to set the session_id field in all <see href="https://dev.twitch.tv/docs/eventsub/manage-subscriptions#subscribing-to-events">subscription requests</see>.
                /// </summary>
                [JsonProperty("id")]
                public string ID { get; set; }

                /// <summary>
                /// The connection’s status.
                /// </summary>
                [JsonProperty("status")]
                public string Status { get; set; }

                /// <summary>
                /// The UTC date and time that the connection was created.
                /// </summary>
                [JsonProperty("connected_at")]
                public DateTime connected_at { get; set; }

                /// <summary>
                /// The maximum number of seconds that you should expect silence before receiving a <see href="https://dev.twitch.tv/docs/eventsub/websocket-reference/#keepalive-message">keepalive message</see>. For a welcome message, this is the number of seconds that you have to <see href="https://dev.twitch.tv/docs/eventsub/manage-subscriptions#subscribing-to-events">subscribe to an event</see> after receiving the welcome message. If you don’t subscribe to an event within this window, the socket is disconnected.
                /// </summary>
                [JsonProperty("keepalive_timeout_seconds")]
                public int keepalive_timeout_seconds { get; set; }

                /// <summary>
                /// The URL to reconnect to if you get a <see href="https://dev.twitch.tv/docs/eventsub/websocket-reference/#reconnect-message">Reconnect message</see>.
                /// </summary>
                [JsonProperty("reconnect_url")]
                public string reconnect_url { get; set; }
            }
            internal class _Subscription
            {
                /// <summary>
                /// An ID that uniquely identifies this subscription.
                /// </summary>
                [JsonProperty("id")]
                public string ID { get; set; }

                /// <summary>
                /// The subscription’s status.
                /// </summary>
                [JsonProperty("status")]
                public string Status { get; set; }

                /// <summary>
                /// The type of event sent in the message.
                /// </summary>
                [JsonProperty("type")]
                public string Type { get; set; }

                /// <summary>
                /// The version number of the subscription type’s definition.
                /// </summary>
                [JsonProperty("version")]
                public string Version { get; set; }

                /// <summary>
                /// The event’s cost. See <see href="https://dev.twitch.tv/docs/eventsub/manage-subscriptions#subscription-limits">Subscription limits</see>.
                /// </summary>
                [JsonProperty("cost")]
                public int Cost { get; set; }

                /// <summary>
                /// The conditions under which the event fires. For example, if you requested notifications when a broadcaster gets a new follower, this object contains the broadcaster’s ID. For information about the condition’s data, see the subscription type’s description in <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types">Subscription types</see>.
                /// </summary>
                [JsonProperty("condition")]
                public object condition { get; set; }

                /// <summary>
                /// An object that contains information about the transport used for notifications.
                /// </summary>
                [JsonProperty("transport")]
                public Transport transport { get; set; }

                /// <summary>
                /// The UTC date and time that the subscription was created.
                /// </summary>
                [JsonProperty("created_at")]
                public DateTime created_at { get; set; }


                public class Transport
                {
                    /// <summary>
                    /// The transport method, which is set to websocket.
                    /// </summary>
                    [JsonProperty("method")]
                    public string method { get; set; }

                    /// <summary>
                    /// An ID that uniquely identifies the WebSocket connection.
                    /// </summary>
                    [JsonProperty("session_id")]
                    public string session_id { get; set; }
                }
            }
        }
    }
}

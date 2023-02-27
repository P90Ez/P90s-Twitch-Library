using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;
using static P90Ez.Twitch.API.Endpoints.UpdateUserChatColor;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Updates the broadcaster’s chat settings. (emote chat, follower mode, subscriber mode, ...)</strong>
    /// </summary>
    public class UpdateChatSettings : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Patch;
        private static string EndpointURL = "https://api.twitch.tv/helix/chat/settings";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "moderator:manage:chat_settings";
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        public static Login.TokenType RequieredTokenType
        {
            get
            {
                if (RequieredScopes == "") return Login.TokenType.Any;
                else return Login.TokenType.UserAccessToken;
            }
        }

        /// <summary>
        /// <strong>Activates or deactivates the emote only chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Emote_Mode">A Boolean value that determines whether chat messages must contain only emotes.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings EmoteChat(Login.Credentials credentials, string Broadcaster_ID, bool Emote_Mode, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { emote_mode = Emote_Mode }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Activates or deactivates the follower only chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Follower_Mode">A Boolean value that determines whether the broadcaster restricts the chat room to followers only.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings FollowerMode(Login.Credentials credentials, string Broadcaster_ID, bool Follower_Mode, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { follower_mode = Follower_Mode }, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Activates or deactivates the follower only chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Follower_Mode">A Boolean value that determines whether the broadcaster restricts the chat room to followers only.</param>
        /// <param name="Follow_Duration">The length of time, in minutes, that users must follow the broadcaster before being able to participate in the chat room. Possible values are: 0 (no restriction) through 129600 (3 months).</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings FollowerMode(Login.Credentials credentials, string Broadcaster_ID, bool Follower_Mode, int Follow_Duration, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { follower_mode = Follower_Mode, follower_mode_duration = Follow_Duration }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Activates or deactivates the chat delay for non moderators.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="ModerationChatDelay">A Boolean value that determines whether the broadcaster adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings ModerationChatDelay(Login.Credentials credentials, string Broadcaster_ID, bool ModerationChatDelay, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { non_moderator_chat_delay = ModerationChatDelay }, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Activates or deactivates the chat delay for non moderators.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="ModerationChatDelay">A Boolean value that determines whether the broadcaster adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message.</param>
        /// <param name="DelayDuration">The amount of time, in seconds, that messages are delayed before appearing in chat. Possible values are: 2, 4, 6 (seconds).</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings ModerationChatDelay(Login.Credentials credentials, string Broadcaster_ID, bool ModerationChatDelay, int DelayDuration, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { non_moderator_chat_delay = ModerationChatDelay, non_moderator_chat_delay_duration = DelayDuration }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Activates or deactivates the slow chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Slow_Mode">A Boolean value that determines whether the broadcaster limits how often users in the chat room are allowed to send messages.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings SlowMode(Login.Credentials credentials, string Broadcaster_ID, bool Slow_Mode, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { slow_mode = Slow_Mode }, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Activates or deactivates the slow chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Slow_Mode">A Boolean value that determines whether the broadcaster limits how often users in the chat room are allowed to send messages.</param>
        /// <param name="Wait_Time">The amount of time, in seconds, that users must wait between sending messages. Possible value rage: 3-120 (seconds).</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings SlowMode(Login.Credentials credentials, string Broadcaster_ID, bool Slow_Mode, int Wait_Time, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { slow_mode = Slow_Mode, slow_mode_wait_time = Wait_Time }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Activates or deactivates the subscriber only chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Subscriber_Mode">A Boolean value that determines whether only users that subscribe to the broadcaster’s channel may talk in the chat room.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings SubscriberMode(Login.Credentials credentials, string Broadcaster_ID, bool Subscriber_Mode, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { subscriber_mode = Subscriber_Mode}, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Activates or deactivates the unique chat mode.</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Unique_Mode">A Boolean value that determines whether the broadcaster requires users to post only unique messages in the chat room.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings UniqueMode(Login.Credentials credentials, string Broadcaster_ID, bool Unique_Mode, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, Broadcaster_ID, new Application() { unique_chat_mode = Unique_Mode }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Updates the broadcaster’s chat settings. (emote chat, follower mode, subscriber mode, ...)</strong>
        /// <para>Scope: <em>moderator:manage:chat_settings</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-chat-settings">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="application">All chat settings. All fields are optional, but you must specify at least one.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of UpdateChatSettings, containing response variables from this request. (only if request was successful)</returns>
        public static UpdateChatSettings Go(Login.Credentials credentials, string Broadcaster_ID, Application application, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (Broadcaster_ID == null || Broadcaster_ID == "")
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id={Broadcaster_ID}&moderator_id={credentials.user_id}", method, application.ToString()); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            JsonDataArray raw = (JsonDataArray)request.BodyToClassObject(typeof(JsonDataArray)); //deserialize into 'json-housing'
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            else if (raw.data.Length > 0)
                return JsonConvert.DeserializeObject<UpdateChatSettings>(raw.data[0].ToString()); //deserialize into an object of UpdateChatSettings
            else return null; //request successful but empty response
        }

        #region Object Var
        /// <summary>
        /// The ID of the broadcaster specified in the request.
        /// </summary>
        [JsonProperty]
        public string broadcaster_id { get; internal set; }
        /// <summary>
        /// The id of the moderator who changed the settings.
        /// </summary>
        [JsonProperty]
        public string moderator_id { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the broadcaster limits how often users in the chat room are allowed to send messages.
        /// Is true if the broadcaster applies a delay; otherwise, false.
        /// </summary>
        [JsonProperty]
        public bool slow_mode { get; internal set; }
        /// <summary>
        /// The amount of time, in seconds, that users must wait between sending messages.
        /// </summary>
        [JsonProperty]
        public int? slow_mode_wait_time { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the broadcaster restricts the chat room to followers only.
        /// Is true if the broadcaster restricts the chat room to followers only; otherwise, false.
        /// </summary>
        [JsonProperty]
        public bool follower_mode { get; internal set; }
        /// <summary>
        /// The length of time, in minutes, that users must follow the broadcaster before being able to participate in the chat room. Is null if follower_mode is false.
        /// </summary>
        [JsonProperty]
        public int? follower_mode_duration { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether only users that subscribe to the broadcaster’s channel may talk in the chat room.
        /// </summary>
        [JsonProperty]
        public bool subscriber_mode { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether chat messages must contain only emotes. Is true if chat messages may contain only emotes; otherwise, false.
        /// </summary>
        [JsonProperty]
        public bool emote_mode { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the broadcaster requires users to post only unique messages in the chat room.
        /// </summary>
        [JsonProperty]
        public bool unique_chat_mode { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the broadcaster adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message.
        /// </summary>
        [JsonProperty]
        public bool non_moderator_chat_delay { get; internal set; }
        /// <summary>
        /// The amount of time, in seconds, that messages are delayed before appearing in chat. Is null if non_moderator_chat_delay is false.
        /// </summary>
        [JsonProperty]
        public int? non_moderator_chat_delay_duration { get; internal set; }
        #endregion

        public class Application : JsonApplication
        {
            /// <summary>
            /// A Boolean value that determines whether chat messages must contain only emotes.
            /// </summary>
            public bool emote_mode { get; set; }
            /// <summary>
            /// A Boolean value that determines whether the broadcaster restricts the chat room to followers only.
            /// Set to true if the broadcaster restricts the chat room to followers only; otherwise, false. 
            /// </summary>
            public bool follower_mode { get; set; }
            /// <summary>
            /// The length of time, in minutes, that users must follow the broadcaster before being able to participate in the chat room.
            /// </summary>
            public int? follower_mode_duration { get; set; }
            /// <summary>
            /// A Boolean value that determines whether the broadcaster adds a short delay before chat messages appear in the chat room. This gives chat moderators and bots a chance to remove them before viewers can see the message.
            /// </summary>
            public bool non_moderator_chat_delay { get; set; }
            /// <summary>
            /// The amount of time, in seconds, that messages are delayed before appearing in chat. Set only if non_moderator_chat_delay is true. Possible values are: 2, 4, 6 (seconds)
            /// </summary>
            public int? non_moderator_chat_delay_duration { get; set; }
            /// <summary>
            /// A Boolean value that determines whether the broadcaster limits how often users in the chat room are allowed to send messages. Set to true if the broadcaster applies a wait period between messages; otherwise, false.
            /// </summary>
            public bool slow_mode { get; set; }
            /// <summary>
            /// The amount of time, in seconds, that users must wait between sending messages. Set only if slow_mode is true. Value range: 3 - 120 (seconds)
            /// </summary>
            public int? slow_mode_wait_time { get; set; }
            /// <summary>
            /// A Boolean value that determines whether only users that subscribe to the broadcaster’s channel may talk in the chat room.
            /// </summary>
            public bool subscriber_mode { get; set; }
            /// <summary>
            /// A Boolean value that determines whether the broadcaster requires users to post only unique messages in the chat room.
            /// </summary>
            public bool unique_chat_mode { get; set; }
        }
    }
}

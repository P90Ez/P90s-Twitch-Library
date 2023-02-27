using P90Ez.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Removes a single chat message or all chat messages from the broadcaster’s chat room.</strong>
    /// </summary>
    public class DeleteChatMessage : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Delete;
        private static string EndpointURL = "https://api.twitch.tv/helix/moderation/chat";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "moderator:manage:chat_messages";
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
        /// <strong>Removes all chat messages from the broadcaster’s chat room.</strong>
        /// <para>Scope: <em>moderator:manage:chat_messages</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-chat-messages">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void ClearChat(Login.Credentials credentials, string Broadcaster_ID, out bool isSuccess, out int httpStatuscode)
        {
            Go(credentials, Broadcaster_ID, new QueryParams() { broadcaster_id = Broadcaster_ID, moderator_id = credentials.user_id }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Removes a single chat message from the broadcaster’s chat room.</strong>
        /// <para>Scope: <em>moderator:manage:chat_messages</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-chat-messages">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="Message_ID">The ID of the message to remove. </param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void DeleteMessage(Login.Credentials credentials, string Message_ID, string Broadcaster_ID, out bool isSuccess, out int httpStatuscode)
        {
            Go(credentials, Broadcaster_ID, new QueryParams() { broadcaster_id = Broadcaster_ID, moderator_id = credentials.user_id, message_id = Message_ID }, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Removes a single chat message from the broadcaster’s chat room.</strong>
        /// <para>Scope: <em>moderator:manage:chat_messages</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-chat-messages">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Broadcaster_ID">The ID of the broadcaster whose chat settings you want to update.</param>
        /// <param name="privmsg">An object of irc_privmsg. Can be obtained trough the irc controller.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void DeleteMessage(Login.Credentials credentials, irc_privsmg privmsg, string Broadcaster_ID, out bool isSuccess, out int httpStatuscode)
        {
            if (privmsg.Permissionlevel < irc_Enums.Permissionlevels.Mod && DateTime.Now.Subtract(privmsg.MessageSentTimeStamp).TotalHours < 6.0)
            {
                Go(credentials, Broadcaster_ID, new QueryParams() { broadcaster_id = Broadcaster_ID, moderator_id = credentials.user_id, message_id = privmsg.MessageID }, out isSuccess, out httpStatuscode);
            }
            else
            {
                isSuccess = false;
                httpStatuscode = 0;
            }
        }

        private static void Go(Login.Credentials credentials, string Broadcaster_ID, QueryParams application, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;

            if (Broadcaster_ID == null || Broadcaster_ID == "" || application == null)
                return;

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            
            if (!isSuccess) //return if credential ceck failed
                return;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + application.ToQueryParameters(), method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;
        }

        private class QueryParams : JsonApplication
        {
            /// <summary>
            /// The ID of the broadcaster that owns the chat room to remove messages from.
            /// </summary>
            public string broadcaster_id { get; set; }
            /// <summary>
            /// The ID of the broadcaster or a user that has permission to moderate the broadcaster’s chat room. This ID must match the user ID in the user access token.
            /// </summary>
            public string moderator_id { get; set; }
            /// <summary>
            /// The ID of the message to remove.
            /// </summary>
            public string message_id { get; set; }
        }
    }
}

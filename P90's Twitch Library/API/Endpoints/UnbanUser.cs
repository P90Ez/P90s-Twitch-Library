using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Removes the ban or timeout that was placed on the specified user.</strong>
    /// </summary>
    public class UnbanUser : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Delete;
        private static string EndpointURL = "https://api.twitch.tv/helix/moderation/bans";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "moderator:manage:banned_users";
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
        /// <strong>Removes the ban or timeout that was placed on the specified user.</strong>
        /// <para>Scope: <em>moderator:manage:banned_users</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#unban-user">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">The ID of the broadcaster whose chat room the user is banned from chatting in.</param>
        /// <param name="UserID">The ID of the user to remove the ban or timeout from.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void Go(Login.Credentials credentials, string broadcaster_id, long UserID, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (broadcaster_id == null || broadcaster_id == "" || UserID < 1)
                return;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id={broadcaster_id}&moderator_id={credentials.UserId}&user_id={UserID}", method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;
        }
    }
}

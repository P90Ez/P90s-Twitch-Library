using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;
using static System.Net.Mime.MediaTypeNames;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Updates a channel’s properties.</strong>
    /// </summary>
    public class ModifyChannelInformation : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Patch;
        private static string EndpointURL = "https://api.twitch.tv/helix/channels";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "channel:manage:broadcast";
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
        /// <strong>Updates a channel’s properties.</strong>
        /// <para>Scope: <em>channel:manage:broadcast</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#modify-channel-information">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">The ID of the broadcaster whose channel you want to update.</param>
        /// <param name="application">All fields are optional, but you must specify at least one field.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void Go(Login.Credentials credentials, string broadcaster_id, Application application, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (broadcaster_id == null || broadcaster_id == "" || application == null)
                return;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id={broadcaster_id}", method, application.ToString()); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            return;
        }

        /// <summary>
        /// All fields are optional, but you must specify at least one field.
        /// </summary>
        public class Application : JsonApplication
        {
            /// <summary>
            /// <em>[OPTIONAL]</em> The ID of the game that the user plays. The game is not updated if the ID isn’t a game ID that Twitch recognizes. To unset this field, use “0” or “” (an empty string).
            /// </summary>
            public string game_id { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The user’s preferred language. Set the value to an ISO 639-1 two-letter language code (for example, en for English). Set to “other” if the user’s preferred language is not a Twitch supported language. The language isn’t updated if the language code isn’t a Twitch supported language.
            /// </summary>
            public string broadcaster_language { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The title of the user’s stream. You may not set this field to an empty string. (Don't assign a value if you don't want to change the title)
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// <em>[OPTIONAL, <strong>PARTNER ONLY</strong>]</em> The number of seconds you want your broadcast buffered before streaming it live. The delay helps ensure fairness during competitive play. Only users with Partner status may set this field. The maximum delay is 900 seconds (15 minutes).
            /// </summary>
            public uint? delay { get; set; }
        }
    }
}

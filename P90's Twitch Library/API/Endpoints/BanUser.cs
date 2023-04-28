using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Bans a user from participating in the specified broadcaster’s chat room or puts them in a timeout.</strong>
    /// </summary>
    public class BanUser : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Post;
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
        /// <strong>Puts a user in the specified broadcaster’s chat room in a timeout.</strong>
        /// <para>Scope: <em>moderator:manage:banned_users</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#ban-user">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id"> The ID of the broadcaster whose chat room the user is being timeouted from.</param>
        /// <param name="UserID">The ID of the user to put in a timeout.</param>
        /// <param name="duration">Duration of the timeout, in seconds. The minimum timeout is 1 second and the maximum is 1,209,600 seconds (2 weeks). To end a user’s timeout early, set this field to 1, or use the Unban endpoint.</param>
        /// <param name="reason">The reason the you’re putting the user in a timeout. The text is user defined and is limited to a maximum of 500 characters.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of BanUser, containing response variables from this request. (only if request was successful)</returns>
        public static BanUser TimeOut(Login.Credentials credentials, string broadcaster_id, long UserID, int duration, string reason, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, broadcaster_id, UserID, duration, reason, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Puts a user in the specified broadcaster’s chat room in a timeout.</strong>
        /// <para>Scope: <em>moderator:manage:banned_users</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#ban-user">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id"> The ID of the broadcaster whose chat room the user is being timeouted from.</param>
        /// <param name="UserID">The ID of the user to put in a timeout.</param>
        /// <param name="duration">Duration of the timeout, in seconds. The minimum timeout is 1 second and the maximum is 1,209,600 seconds (2 weeks). To end a user’s timeout early, set this field to 1, or use the Unban endpoint.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of BanUser, containing response variables from this request. (only if request was successful)</returns>
        public static BanUser TimeOut(Login.Credentials credentials, string broadcaster_id, long UserID, int duration, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, broadcaster_id, UserID, duration, "", out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Bans a user from participating in the specified broadcaster’s chat room.</strong>
        /// <para>Scope: <em>moderator:manage:banned_users</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#ban-user">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id"> The ID of the broadcaster whose chat room the user is being banned from.</param>
        /// <param name="UserID"> The ID of the user to ban.</param>
        /// <param name="reason">The reason the you’re banning the user. The text is user defined and is limited to a maximum of 500 characters.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of BanUser, containing response variables from this request. (only if request was successful)</returns>
        public static BanUser Ban(Login.Credentials credentials, string broadcaster_id, long UserID, string reason, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, broadcaster_id, UserID, null, reason, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Bans a user from participating in the specified broadcaster’s chat room.</strong>
        /// <para>Scope: <em>moderator:manage:banned_users</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#ban-user">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id"> The ID of the broadcaster whose chat room the user is being banned from.</param>
        /// <param name="UserID"> The ID of the user to ban.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of BanUser, containing response variables from this request. (only if request was successful)</returns>
        public static BanUser Ban(Login.Credentials credentials, string broadcaster_id, long UserID, out bool isSuccess, out int httpStatuscode)
        {
            return Go(credentials, broadcaster_id, UserID, null, "", out isSuccess, out httpStatuscode);
        }
        private static BanUser Go(Login.Credentials credentials, string broadcaster_id, long UserID, int? duration, string reason, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (broadcaster_id == null || broadcaster_id == "" || UserID < 1 || reason.Length > 500)
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            var application = new Application() { duration= duration, user_id = UserID.ToString(), reason = reason };

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id={broadcaster_id}&moderator_id={credentials.UserId}", method, application.ToDataObjectString()); //make request

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
                return JsonConvert.DeserializeObject<BanUser>(raw.data[0].ToString()); //deserialize into an object of BanUser
            else return null; //request successful but empty response
        }
        

        #region Object Vars
        /// <summary>
        /// The broadcaster whose chat room the user was banned from chatting in.
        /// </summary>
        [JsonProperty]
        public string broadcaster_id { get; internal set; }
        /// <summary>
        /// The moderator that banned or put the user in the timeout.
        /// </summary>
        [JsonProperty]
        public string moderator_id { get; internal set; }
        /// <summary>
        /// The user that was banned or put in a timeout.
        /// </summary>
        [JsonProperty]
        public string user_id { get; internal set; }
        /// <summary>
        /// The date and time that the ban or timeout was placed.
        /// </summary>
        [JsonProperty]
        public DateTime created_at { get; internal set; }
        /// <summary>
        /// The UTC date and time that the timeout will end. Is null if the user was banned instead of being put in a timeout.
        /// </summary>
        [JsonProperty]
        public DateTime end_time { get; internal set; }
        #endregion

        private class Application : JsonApplication
        {
            public string user_id { get; set; }
            public int? duration { get; set; }
            public string reason { get; set; }
        }

    }
}

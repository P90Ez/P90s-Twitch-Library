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
    /// <strong>Starts a commercial.</strong>
    /// </summary>
    public class StartCommercial : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Post;
        private static string EndpointURL = "https://api.twitch.tv/helix/channels/commercial";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "channel:edit:commercial";
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
        /// <strong>Starts a commercial.</strong>
        /// <para>Scope: <em>channel:edit:commercial</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#start-commercial">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="length">The length of the commercial to run, in seconds</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of StartCommercial, containing response variables from this request. (only if request was successful)</returns>
        public static StartCommercial Go(Login.Credentials credentials, int length, out bool isSuccess, out int httpStatuscode)
        {
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            httpStatuscode = 0;
            if (!isSuccess) //return if credential ceck failed
                return null;

            var application = new Application() { broadcaster_id = credentials.user_id, length = length }; //build application object
            var request = APICom.Request(out isSuccess, credentials, EndpointURL, method, application.ToString()); //make request

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
                return JsonConvert.DeserializeObject<StartCommercial>(raw.data[0].ToString()); //deserialize into an object of StartCommercial
            else return null; //request successful but empty response
        }
        /// <summary>
        /// <strong>Starts a commercial.</strong>
        /// <para>Scope: <em>channel:edit:commercial</em></para>
        /// <para>TokenType: <em>User Acces Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#start-commercial">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="length">The length of the commercial to run, in seconds</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <returns>An object of StartCommercial, containing response variables from this request. (only if request was successful)</returns>
        public static StartCommercial Go(Login.Credentials credentials, int length, out bool isSuccess)
        {
            int httpstatuscode = 0;
            return Go(credentials, length, out isSuccess, out httpstatuscode);
        }
        /// <summary>
        /// <strong>Starts a commercial.</strong>
        /// <para>Scope: <em>channel:edit:commercial</em></para>
        /// <para>TokenType: <em>User Acces Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#start-commercial">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="length">The length of the commercial to run, in seconds</param>
        /// <returns>An object of StartCommercial, containing response variables from this request. (only if request was successful)</returns>
        public static StartCommercial Go(Login.Credentials credentials, int length)
        {
            int httpstatuscode = 0;
            bool isSuccess = false;
            return Go(credentials, length, out isSuccess, out httpstatuscode);
        }
        #region Object Vars
        /// <summary>
        /// The length of the commercial you requested. If you request a commercial that’s longer than 180 seconds, the API uses 180 seconds.
        /// </summary>
        [JsonProperty]
        public int length { get; internal set; }
        /// <summary>
        /// The number of seconds you must wait before running another commercial.
        /// </summary>
        [JsonProperty]
        public int retry_after { get; internal set; }
        /// <summary>
        /// A message that indicates whether Twitch was able to serve an ad.
        /// </summary>
        [JsonProperty]
        public string message { get; internal set; }
        #endregion
        private class Application : JsonApplication
        {
            public string broadcaster_id;
            public int length;
        }
    }
}

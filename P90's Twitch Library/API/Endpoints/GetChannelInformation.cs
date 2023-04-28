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
    /// <strong>Gets information about one or more channels.</strong>
    /// </summary>
    public class GetChannelInformation : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/channels";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "";
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
        /// <strong>Gets information about one or more channels.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-channel-information">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">The ID of the broadcaster (or user) whose channel you want to get. To specify more than one ID, use an overload of this method.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetChannelInformation, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetChannelInformation Go(Login.Credentials credentials, string broadcaster_id, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            GetChannelInformation[] response = Go(credentials, new string[1] { broadcaster_id }, out isSuccess, out httpStatuscode, skipCache);
            if (!isSuccess || response.Length == 0)
                return null;
            return response[0];
        }

        /// <summary>
        /// <strong>Gets information about one or more channels.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-channel-information">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">The IDs of the broadcasters (or users) whose channels you want to get. To specify one ID, use an overload of this method.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetChannelInformation, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetChannelInformation[] Go(Login.Credentials credentials, string[] broadcaster_id, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (broadcaster_id == null || broadcaster_id.Length <= 0)
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, broadcaster_id); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetChannelInformation[])cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + broadcaster_id.ToQueryParams("broadcaster_id"), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var raw = (JsonDataArray)request.BodyToClassObject(typeof(JsonDataArray)); //deserialize into 'json-housing'
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (raw.data.Length == 0)
                return null; //Request successful, but empty response
            var finalobject = JsonConvert.DeserializeObject<GetChannelInformation[]>(JsonConvert.SerializeObject(raw.data));
            if (finalobject == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (finalobject.Length == 0)
                return null; //Request successful, but empty response

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// An ID that uniquely identifies the broadcaster.
        /// </summary>
        [JsonProperty]
        public string broadcaster_id { get; internal set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_login { get; internal set; }
        /// <summary>
        /// The broadcaster’s display name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_name { get; internal set; }
        /// <summary>
        /// The broadcaster’s preferred language. The value is an ISO 639-1 two-letter language code (for example, en for English). The value is set to “other” if the language is not a Twitch supported language.
        /// </summary>
        [JsonProperty]
        public string broadcaster_language { get; internal set; }
        /// <summary>
        /// The name of the game that the broadcaster is playing or last played. The value is an empty string if the broadcaster has never played a game.
        /// </summary>
        [JsonProperty]
        public string game_name { get; internal set; }
        /// <summary>
        /// An ID that uniquely identifies the game that the broadcaster is playing or last played. The value is an empty string if the broadcaster has never played a game.
        /// </summary>
        [JsonProperty]
        public string game_id { get; internal set; }
        /// <summary>
        ///  	The title of the stream that the broadcaster is currently streaming or last streamed. The value is an empty string if the broadcaster has never streamed.
        /// </summary>
        [JsonProperty]
        public string title { get; internal set; }
        /// <summary>
        /// The value of the broadcaster’s stream delay setting, in seconds. Reserved for users with Partner status.
        /// </summary>
        [JsonProperty]
        public uint? delay { get; internal set; }
        #endregion
    }
}

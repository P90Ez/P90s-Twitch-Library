using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Gets a list of all streams. The list is in descending order by the number of viewers watching the stream.</strong>
    /// </summary>
    public class GetStreams : JsonPagination, IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/streams";
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
        /// <strong>Gets a list of all streams. The list is in descending order by the number of viewers watching the stream.</strong>
        /// <para>Scope: <em>none</em></para>
        /// <para>TokenType: <em>User Access Token</em> or <em>App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-streams">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParams"><em>[OPTIONAL]</em> request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetStreams, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetStreams Go(Login.Credentials credentials, QueryParams queryParams, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.user_id, queryParams.ToQueryParameters()); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetStreams)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParams.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var finalobject = (GetStreams)request.BodyToClassObject(typeof(GetStreams)); 

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }
        /// <summary>
        /// Request query parameters - all are optional
        /// </summary>
        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// A user ID used to filter the list of streams. Returns only the streams of those users that are broadcasting. You may specify a maximum of 100 IDs.
            /// </summary>
            public List<string> user_id { get; set; }
            /// <summary>
            /// A user login name used to filter the list of streams. Returns only the streams of those users that are broadcasting. You may specify a maximum of 100 login names.
            /// </summary>
            public List<string> user_login { get; set; }
            /// <summary>
            /// A game (category) ID used to filter the list of streams. Returns only the streams that are broadcasting the game (category). You may specify a maximum of 100 IDs. 
            /// </summary>
            public List<string> game_id { get; set; }
            /// <summary>
            /// The type of stream to filter the list of streams by. Possible values are: all, live
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// A language code used to filter the list of streams. Returns only streams that broadcast in the specified language. Specify the language using an ISO 639-1 two-letter language code or other if the broadcast uses a language not in the list of supported stream languages. You may specify a maximum of 100 language codes.
            /// </summary>
            public List<string> language { get; set; }
            /// <summary>
            /// The maximum number of items to return per page in the response. The minimum page size is 1 item per page and the maximum is 100 items per page. The default is 20.
            /// </summary>
            public int? first { get; set; }
            /// <summary>
            /// The cursor used to get the previous page of results. The Pagination object in the response contains the cursor’s value. 
            /// </summary>
            public string before { get; set; }
            /// <summary>
            /// The cursor used to get the next page of results. The Pagination object in the response contains the cursor’s value.
            /// </summary>
            public string after { get; set; }
        }
        /// <summary>
        /// The list of streamers matching the provided parameters.
        /// </summary>
        [JsonProperty]
        public new Data[] data { get; internal set; }
        public class Data
        {
            /// <summary>
            /// An ID that identifies the stream. You can use this ID later to look up the video on demand (VOD).
            /// </summary>
            [JsonProperty]
            public string id { get; internal set; }
            /// <summary>
            /// The ID of the user that’s broadcasting the stream. Also known as broadcaster_id.
            /// </summary>
            [JsonProperty]
            public string user_id { get; internal set; }
            /// <summary>
            /// The user’s login name.
            /// </summary>
            [JsonProperty]
            public string user_login { get; internal set; }
            /// <summary>
            /// The user’s display name. May differ from the login name (-> use user_login in API requests)
            /// </summary>
            [JsonProperty]
            public string user_name { get; internal set; }
            /// <summary>
            /// The ID of the category or game being played.
            /// </summary>
            [JsonProperty]
            public string game_id { get; internal set; }
            /// <summary>
            /// The name of the category or game being played.
            /// </summary>
            [JsonProperty]
            public string game_name { get; internal set; }
            /// <summary>
            /// The type of stream. Possible values are: live - If an error occurs, this field is set to an empty string.
            /// </summary>
            [JsonProperty]
            public string type { get; internal set; }
            /// <summary>
            /// The stream’s title. Is an empty string if not set.
            /// </summary>
            [JsonProperty]
            public string title { get; internal set; }
            /// <summary>
            /// The tags applied to the stream.
            /// </summary>
            [JsonProperty]
            public List<string> tags { get; internal set; }
            /// <summary>
            /// The number of users watching the stream.
            /// </summary>
            [JsonProperty]
            public int viewer_count { get; internal set; }
            /// <summary>
            /// The date and time of when the broadcast began.
            /// </summary>
            [JsonProperty]
            public DateTime started_at { get; internal set; }
            /// <summary>
            /// The language that the stream uses. This is an ISO 639-1 two-letter language code or other if the stream uses a language not in the list of supported stream languages.
            /// </summary>
            [JsonProperty]
            public string language { get; internal set; }
            /// <summary>
            /// A URL to an image of a frame from the last 5 minutes of the stream. Replace the width and height placeholders in the URL ({width}x{height}) with the size of the image you want, in pixels.
            /// </summary>
            [JsonProperty]
            public string thumbnail_url { get; internal set; }
            /// <summary>
            /// <strong>[DEPRECATED]</strong> Use the field tags instead.
            /// <para/> The list of tags that apply to the stream. The list contains IDs only when the channel is steaming live.
            /// </summary>
            [JsonProperty]
            public List<string> tag_ids { get; internal set; }
            /// <summary>
            /// A Boolean value that indicates whether the stream is meant for mature audiences.
            /// </summary>
            [JsonProperty]
            public bool is_mature { get; internal set; }
        }
    }
}

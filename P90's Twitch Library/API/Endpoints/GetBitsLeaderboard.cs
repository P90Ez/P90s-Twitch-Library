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
    /// <strong>Gets the Bits leaderboard for the authenticated broadcaster.</strong>
    /// </summary>
    public class GetBitsLeaderboard : IStandardEndpoint, ICacheRequest
    {
        /// <summary>
        /// 
        /// </summary>
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/bits/leaderboard";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "bits:read";
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
        /// <strong>Gets the Bits leaderboard for the authenticated broadcaster.</strong>
        /// <para>Scope: <em>bits:read</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-bits-leaderboard">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="requestQueryParameter"><em>[OPTIONAL]</em> request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetExtensionAnalytics, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetBitsLeaderboard Go(Login.Credentials credentials, QueryParams requestQueryParameter, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            if (requestQueryParameter == null)
                requestQueryParameter = new QueryParams();
            httpStatuscode = 0;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, requestQueryParameter); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetBitsLeaderboard)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + requestQueryParameter.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var finalobject = JsonConvert.DeserializeObject<GetBitsLeaderboard>(request.BodyToString()); //deserialize into an object of GetBitsLeaderboard

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// A list of leaderboard leaders. The leaders are returned in rank order by how much they’ve cheered. The array is empty if nobody has cheered bits.
        /// </summary>
        /// 
        [JsonProperty]
        public BitLeaderboardData[] data { get; internal set; }
        /// <summary>
        /// The reporting window’s start and end dates, in RFC3339 format. The dates are calculated by using the started_at and period query parameters. If you don’t specify the started_at query parameter, the fields contain empty strings.
        /// </summary>
        /// 
        [JsonProperty]
        public Response_date_range date_Range { get; internal set; }
        /// <summary>
        /// The number of ranked users in <seealso cref="data"/>. This is the value in the count query parameter or the total number of entries on the leaderboard, whichever is less.
        /// </summary>
        /// 
        [JsonProperty]
        public int total { get; internal set; } 
        /// <summary>
        /// Leaderboard Data (used here: <seealso cref="data"/>)
        /// </summary>
        public class BitLeaderboardData
        {
            /// <summary>
            /// An ID that identifies a user on the leaderboard.
            /// </summary>
            /// 
            [JsonProperty]
            public string user_id { get; internal set; }
            /// <summary>
            /// The user’s login name.
            /// </summary>
            /// 
            [JsonProperty]
            public string user_login { get; internal set; }
            /// <summary>
            /// The user’s display name.
            /// </summary>
            /// 
            [JsonProperty]
            public string user_name { get; internal set; }
            /// <summary>
            /// The user’s position on the leaderboard.
            /// </summary>
            /// 
            [JsonProperty]
            public string rank { get; internal set; }
            /// <summary>
            /// The number of Bits the user has cheered.
            /// </summary>
            /// 
            [JsonProperty]
            public string score { get; internal set; }
        }
        #endregion

        /// <summary>
        /// Contains parameter for the request, all of them are optional.
        /// </summary>
        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// The number of results to return. The minimum count is 1 and the maximum is 100. The default is 10.
            /// </summary>
            public int? count { get; set; }
            /// <summary>
            /// The time period over which data is aggregated (uses the PST time zone). (See <see href="https://dev.twitch.tv/docs/api/reference#get-bits-leaderboard">documentation</see> for options)
            /// </summary>
            public string period { get; set; }
            /// <summary>
            /// The start date, in RFC3339 format, used for determining the aggregation period. Specify this parameter only if you specify the period query parameter. The start date is ignored if period is all.
            /// </summary>
            public string started_at { get; set; }
            /// <summary>
            /// An ID that identifies a user that cheered bits in the channel. If count is greater than 1, the response may include users ranked above and below the specified user. To get the leaderboard’s top leaders, don’t specify a user ID.
            /// </summary>
            public string user_id { get; set; }
        }
    }
}

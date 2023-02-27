using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    public class GetCustomRewardRedemption : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "channel:read:redemptions";
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
        /// <strong>Gets an array of redemptions for the specified custom reward.</strong>
        /// <para>Scope: <em>channel:read:redemptions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-custom-reward-redemption">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParameters"><strong>[REQUIRED]</strong> Request Query Parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetCustomRewardRedemption, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetCustomRewardRedemption[] Go(Login.Credentials credentials, QueryParameters queryParameters, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (queryParameters == null || queryParameters.broadcaster_id == null || queryParameters.broadcaster_id == "" || queryParameters.reward_id == null || queryParameters.reward_id == "")
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.user_id, queryParameters); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetCustomRewardRedemption[])cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParameters.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var raw = (JsonPagination)request.BodyToClassObject(typeof(JsonPagination)); //deserialize into 'json-housing'
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (raw.data.Length == 0)
                return null; //Request successful, but empty response
            var finalobject = JsonConvert.DeserializeObject<GetCustomRewardRedemption[]>(JsonConvert.SerializeObject(raw.data));
            if (finalobject == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (finalobject.Length == 0)
                return null; //Request successful, but empty response
            finalobject[0].cursor = raw.pagination.cursor; //set cursor

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Var
        /// <summary>
        /// <strong>BE CAREFUL! The cursor only is set in the '0' Element of the Array!</strong> (ignore this warning if a single object was returned) <para><em>Usage: redemtions[0].cursor</em></para>
        /// <para/>The cursor used to get the next page of results. Use the cursor to set the request’s after query parameter.
        /// </summary>
        [JsonIgnore]
        public string cursor { get; internal set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_name { get; internal set; }
        /// <summary>
        /// The broadcaster’s display name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_login { get; internal set; }
        /// <summary>
        /// The ID that uniquely identifies the broadcaster.
        /// </summary>
        [JsonProperty]
        public string broadcaster_id { get; internal set; }
        /// <summary>
        /// The ID that uniquely identifies this redemption.
        /// </summary>
        [JsonProperty]
        public string id { get; internal set; }
        /// <summary>
        /// The user’s login name.
        /// </summary>
        [JsonProperty]
        public string user_login { get; internal set; }
        /// <summary>
        /// The ID that uniquely identifies the user that redeemed the reward.
        /// </summary>
        [JsonProperty]
        public string user_id { get; internal set; }
        /// <summary>
        /// The user’s display name.
        /// </summary>
        [JsonProperty]
        public string user_name { get; internal set; }
        /// <summary>
        /// The text the user entered at the prompt when they redeemed the reward; otherwise, an empty string if user input was not required.
        /// </summary>
        [JsonProperty]
        public string user_input { get; internal set; }
        /// <summary>
        /// The state of the redemption. Possible values are: CANCELED, FULFILLED, UNFULFILLED
        /// <para/>Note: Case sensitive!
        /// </summary>
        [JsonProperty]
        public string status { get; internal set; }
        /// <summary>
        /// The date and time of when the reward was redeemed, in RFC3339 format.
        /// </summary>
        [JsonProperty]
        public DateTime redeemed_at { get; internal set; }
        /// <summary>
        /// The reward that the user redeemed.
        /// </summary>
        [JsonProperty]
        public Reward reward { get; internal set; }

        /// <summary>
        /// The reward that the user redeemed.
        /// </summary>
        public class Reward
        {
            /// <summary>
            /// The ID that uniquely identifies the redeemed reward.
            /// </summary>
            [JsonProperty]
            public string id { get; internal set; }
            /// <summary>
            /// The reward’s title.
            /// </summary>
            [JsonProperty]
            public string title { get; internal set; }
            /// <summary>
            ///  	The prompt displayed to the viewer if user input is required.
            /// </summary>
            [JsonProperty]
            public string prompt { get; internal set; }
            /// <summary>
            /// The reward’s cost, in Channel Points.
            /// </summary>
            [JsonProperty]
            public long cost { get; internal set; }
        }

        #endregion

        /// <summary>
        /// Contains Request Query Parameters, broadcaster_id, reward_id,and status are required!
        /// </summary>
        public class QueryParameters : JsonApplication
        {
            /// <summary>
            /// <strong>[REQUIRED]</strong> The ID of the broadcaster that owns the custom reward.
            /// </summary>
            public string broadcaster_id { get; set; }
            /// <summary>
            /// <strong>[REQUIRED]</strong> The ID that identifies the custom reward whose redemptions you want to get.
            /// </summary>
            public string reward_id { get; set; }
            /// <summary>
            /// <strong>[REQUIRED]</strong> The status of the redemptions to return.
            /// <para/> <em>Note: </em> This field is required only if you don’t specify the id query parameter.
            /// </summary>
            [JsonIgnore]
            public StatusEnum Status { get; set; }
            internal string status { get { switch (Status) { case StatusEnum.Canceled: return "CANCELED"; case StatusEnum.Fullfilled: return "FULFILLED"; case StatusEnum.Unfulfilled: return "UNFULFILLED"; default: return null; } } }
            /// <summary>
            /// <em>[OPTIONAL]</em> An array of IDs to filter the redemptions by.
            /// </summary>
            public string[] id { get; set; }
            [JsonProperty]
            internal string sort { get { if (sortBy == SortBy.Newest) return "NEWEST"; else return "OLDEST"; } }
            /// <summary>
            /// <em>[OPTIONAL]</em> The order to sort redemptions by. (Default: oldest)
            /// </summary>
            [JsonIgnore]
            public SortBy sortBy { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The <seealso cref="cursor">cursor</seealso> used to get the next page of results.
            /// </summary>
            public string after { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The maximum number of redemptions to return per page in the response. The minimum page size is 1 redemption per page and the maximum is 50. The default is 20.
            /// </summary>
            public int? first { get; set; }


            public enum StatusEnum
            {
                Canceled,
                Fullfilled,
                Unfulfilled
            }
            public enum SortBy
            {
                Oldest,
                Newest
            }

        }
    }
}

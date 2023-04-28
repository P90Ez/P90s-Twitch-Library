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
    /// <strong>Gets a list of custom channelpoint rewards that the specified broadcaster created.</strong>
    /// </summary>
    public class GetCustomReward : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/channel_points/custom_rewards";
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
        /// <strong>Gets a list of custom channelpoint rewards that the specified broadcaster created.</strong>
        /// <para>Scope: <em>channel:read:redemptions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-custom-reward">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParameters"><strong>[REQUIRED]</strong> Request Query Parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetCustomReward, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetCustomReward[] Go(Login.Credentials credentials, QueryParameters queryParameters, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (queryParameters == null|| queryParameters.broadcaster_id == null || queryParameters.broadcaster_id == "")
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, queryParameters); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetCustomReward[])cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParameters.ToQueryParameters(), method);

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
            var finalobject = JsonConvert.DeserializeObject<GetCustomReward[]>(JsonConvert.SerializeObject(raw.data));
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
        /// The broadcaster’s display name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_name { get; internal set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        [JsonProperty]
        public string broadcaster_login { get; internal set; }
        /// <summary>
        /// The ID that uniquely identifies the broadcaster.
        /// </summary>
        [JsonProperty]
        public string broadcaster_id { get; internal set; }
        /// <summary>
        /// The ID that uniquely identifies this custom reward.
        /// </summary>
        [JsonProperty]
        public string id { get; internal set; }
        /// <summary>
        /// A set of custom images for the reward. This field is null if the broadcaster didn’t upload images.
        /// </summary>
        [JsonProperty]
        public Image image { get; internal set; }
        /// <summary>
        /// The background color to use for the reward. The color is in Hex format (for example, #00E5CB).
        /// </summary>
        [JsonProperty]
        public string background_color { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the reward is enabled. Is true if enabled; otherwise, false. Disabled rewards aren’t shown to the user.
        /// </summary>
        [JsonProperty]
        public bool is_enabled { get; internal set; }
        /// <summary>
        /// The cost of the reward in Channel Points.
        /// </summary>
        [JsonProperty]
        public int cost { get; internal set; }
        /// <summary>
        /// The title of the reward.
        /// </summary>
        [JsonProperty]
        public string title { get; internal set; }
        /// <summary>
        /// The prompt shown to the viewer when they redeem the reward if user input is required (see the is_user_input_required field).
        /// </summary>
        [JsonProperty]
        public string prompt { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the user must enter information when redeeming the reward. Is true if the user is prompted.
        /// </summary>
        [JsonProperty]
        public bool is_user_input_required { get; internal set; }
        /// <summary>
        /// The settings used to determine whether to apply a maximum to the number of redemptions allowed per live stream.
        /// </summary>
        [JsonProperty]
        public MaxPerStreamSetting max_per_stream_setting { get; internal set; }
        /// <summary>
        /// The settings used to determine whether to apply a maximum to the number of redemptions allowed per user per live stream.
        /// </summary>
        [JsonProperty]
        public MaxPerUserPerStreamSetting max_per_user_per_stream_setting { get; internal set; }
        /// <summary>
        /// The settings used to determine whether to apply a cooldown period between redemptions and the length of the cooldown.
        /// </summary>
        [JsonProperty]
        public GlobalCooldownSetting global_cooldown_setting { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the reward is currently paused. Is true if the reward is paused. Viewers can’t redeem paused rewards.
        /// </summary>
        [JsonProperty]
        public bool is_paused { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether the reward is currently in stock. Is true if the reward is in stock. Viewers can’t redeem out of stock rewards.
        /// </summary>
        [JsonProperty]
        public bool is_in_stock { get; internal set; }
        /// <summary>
        /// A set of default images for the reward.
        /// </summary>
        [JsonProperty]
        public Image default_image { get; internal set; }
        /// <summary>
        /// A Boolean value that determines whether redemptions should be set to FULFILLED status immediately when a reward is redeemed. If false, status is set to UNFULFILLED and follows the normal request queue process.
        /// </summary>
        [JsonProperty]
        public bool should_redemptions_skip_request_queue { get; internal set; }
        /// <summary>
        /// The number of redemptions redeemed during the current live stream. The number counts against the max_per_stream_setting limit. This field is null if the broadcaster’s stream isn’t live or max_per_stream_setting isn’t enabled.
        /// </summary>
        [JsonProperty]
        public int? redemptions_redeemed_current_stream { get; internal set; }
        /// <summary>
        /// The timestamp of when the cooldown period expires. Is null if the reward isn’t in a cooldown state. See the global_cooldown_setting field.
        /// </summary>
        [JsonProperty]
        public string cooldown_expires_at { get; internal set; }
        public class Image
        {
            /// <summary>
            /// The URL to a small version of the image.
            /// </summary>
            [JsonProperty]
            public string url_1x { get; internal set; }
            /// <summary>
            /// The URL to a medium version of the image.
            /// </summary>
            [JsonProperty]
            public string url_2x { get; internal set; }
            /// <summary>
            /// The URL to a large version of the image.
            /// </summary>
            [JsonProperty]
            public string url_4x { get; internal set; }
        }

        public class GlobalCooldownSetting
        {
            /// <summary>
            /// A Boolean value that determines whether to apply a cooldown period. Is true if a cooldown period is enabled.
            /// </summary>
            [JsonProperty]
            public bool is_enabled { get; internal set; }
            /// <summary>
            /// The cooldown period, in seconds.
            /// </summary>
            [JsonProperty]
            public long global_cooldown_seconds { get; internal set; }
        }

        public class MaxPerStreamSetting
        {
            /// <summary>
            /// A Boolean value that determines whether the reward applies a limit on the number of redemptions allowed per live stream. Is true if the reward applies a limit.
            /// </summary>
            [JsonProperty]
            public bool is_enabled { get; internal set; }
            /// <summary>
            /// The maximum number of redemptions allowed per live stream.
            /// </summary>
            [JsonProperty]
            public long max_per_stream { get; internal set; }
        }

        public class MaxPerUserPerStreamSetting
        {
            /// <summary>
            /// A Boolean value that determines whether the reward applies a limit on the number of redemptions allowed per user per live stream. Is true if the reward applies a limit.
            /// </summary>
            [JsonProperty]
            public bool is_enabled { get; internal set; }
            /// <summary>
            /// The maximum number of redemptions allowed per user per live stream.
            /// </summary>
            [JsonProperty]
            public long max_per_user_per_stream { get; internal set; }
        }
        #endregion

        public class QueryParameters : JsonApplication
        {
            /// <summary>
            /// <strong>[REQUIRED]</strong> The ID of the broadcaster whose custom rewards you want to get.
            /// </summary>
            public string broadcaster_id { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A list of IDs to filter the rewards by. Duplicate IDs are ignored. The response contains only the IDs that were found. If none of the IDs were found, the response is 404 Not Found.
            /// </summary>
            public string[] id { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether the response contains only the custom rewards that the app may manage (the app is identified by the ID in the Client-Id header). Set to true to get only the custom rewards that the app may manage. The default is false.
            /// </summary>
            public bool only_manageable_rewards { get; set; }
        }
    }
}

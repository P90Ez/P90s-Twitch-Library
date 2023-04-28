using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Gets information about one or more users.</strong>
    /// </summary>
    public class GetUsers : JsonPagination, IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/users";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "";
        /// <summary>
        /// Additional Scopes (not required)
        /// <para/>Add this scope when creating the user access token in order to be able to read your own email adress field.
        /// <para/>More information <see href="https://dev.twitch.tv/docs/api/reference/#get-users">here.</see>
        /// </summary>
        public static string AdditionalScopes { get; } = "user:read:email";
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
        /// <strong>Gets information about one or more users.</strong>
        /// <para>Scope: <em>none</em></para>
        /// <para>TokenType: <em>User Access Token</em> or <em>App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-users">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParams"><em>[OPTIONAL]</em> request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetStreams, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetUsers Go(Login.Credentials credentials, QueryParams queryParams, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, queryParams.ToQueryParameters()); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetUsers)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParams.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var finalobject = (GetUsers)request.BodyToClassObject(typeof(GetUsers));

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }
        /// <summary>
        /// Request query parameters - all are optional.
        /// <para/>Note: The maximum total of specified ids and login names is 100.
        /// </summary>
        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// The ID of the user to get. The maximum number of IDs you may specify is 100.
            /// </summary>
            public List<string> id { get; set; }
            /// <summary>
            /// The login name of the user to get. The maximum number of login names you may specify is 100.
            /// </summary>
            public List<string> login { get; set; }
        }
        /// <summary>
        /// The list of users matching the provided parameters.
        /// </summary>
        [JsonProperty]
        public new Data[] data { get; internal set; }
        public class Data
        {
            /// <summary>
            /// An ID that identifies the user.
            /// </summary>
            [JsonProperty]
            public string id { get; internal set; }
            /// <summary>
            /// The user’s login name.
            /// </summary>
            [JsonProperty]
            public string login { get; internal set; }
            /// <summary>
            /// The user’s display name. May differ from the login name (-> use login name in API requests)
            /// </summary>
            [JsonProperty]
            public string display_name { get; internal set; }
            /// <summary>
            /// The type of user. Possible values are: admin, global_mod, staff, ""
            /// </summary>
            [JsonProperty]
            public string type { get; internal set; }
            /// <summary>
            /// The type of broadcaster. Possible values are: affiliate, partner, ""
            /// </summary>
            [JsonProperty]
            public string broadcaster_type { get; internal set; }
            /// <summary>
            /// The user’s description of their channel.
            /// </summary>
            [JsonProperty]
            public string description { get; internal set; }
            /// <summary>
            /// A URL to the user’s profile image.
            /// </summary>
            [JsonProperty]
            public string profile_image_url { get; internal set; }
            /// <summary>
            /// A URL to the user’s offline image.
            /// </summary>
            [JsonProperty]
            public string offline_image_url { get; internal set; }
            /// <summary>
            /// <strong>[DEPRECATED]</strong> The number of times the user’s channel has been viewed.
            /// </summary>
            [JsonProperty]
            public int view_count { get; internal set; }
            /// <summary>
            /// The user’s verified email address. The object includes this field only if the user access token includes the user:read:email scope.
            /// <para/>If the request contains more than one user, only the user associated with the access token that provided consent will include an email address — the email address for all other users will be empty.
            /// </summary>
            [JsonProperty]
            public string email { get; internal set; }
            /// <summary>
            /// The date and time that the user’s account was created.
            /// </summary>
            [JsonProperty]
            public DateTime created_at { get; internal set; }
        }
    }
}

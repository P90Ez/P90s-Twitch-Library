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
    /// Gets information about the top categories or games.
    /// </summary>
    public class GetTopGames : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/games/top";
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
        /// <strong>Gets information about the top categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-top-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParams"><em>[OPTIONAL]</em> request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetTopGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetTopGames Go(Login.Credentials credentials, QueryParams queryParams, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (queryParams == null)
                queryParams = new QueryParams();
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.user_id, queryParams); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetTopGames)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParams.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var finalobject = JsonConvert.DeserializeObject<GetTopGames>(request.BodyToString()); //deserialize into an object of GetTopGames
            if (finalobject == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }


            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// The cursor used to get the next page of results. Use the cursor to set the request’s <seealso cref="QueryParams.after">after</seealso> query parameter. (in this instance you can also use it with the <seealso cref="QueryParams.before">before</seealso> parameter)
        /// </summary>
        [JsonIgnore]
        public string cursor { get { if (pagination == null) return null; return pagination.cursor; } }
        /// <summary>
        /// The list of categories/games. The categories/games are sorted by the number of viewers, with the most popular first.
        /// </summary>
        [JsonProperty]
        public GetGames[] data { get; internal set; }
        /// <summary>
        /// Use <seealso cref="cursor">cursor</seealso> variable. For deserialization only.
        /// </summary>
        [JsonProperty]
        internal Pagination pagination { get; set; }

        internal class Pagination
        {
            public string cursor { get; set; }
        }
        #endregion

        /// <summary>
        /// All parameter are optional.
        /// </summary>
        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// <em>[OPTIONAL]</em> The maximum number of items to return per page in the response. The minimum page size is 1 item per page and the maximum is 100 items per page. The default is 20.
            /// </summary>
            public int? first { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The <seealso cref="cursor">cursor</seealso> used to get the next page of results.
            /// </summary>
            public string after { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The <seealso cref="cursor">cursor</seealso> used to get the previous page of results.
            /// </summary>
            public string before { get; set; }
        }
    }
}

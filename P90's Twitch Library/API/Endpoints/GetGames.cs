using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;
using static System.Net.WebRequestMethods;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Gets information about specified categories or games.</strong>
    /// </summary>
    public class GetGames : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/games";
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

        #region ByNames
        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="name">The name of the category or game to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want multiple games/categorys)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames ByName(Login.Credentials credentials, string name, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            var result = ByName(credentials, new string[1] { name }, out isSuccess, out httpStatuscode, skipCache);
            if (!isSuccess || result == null || result.Length == 0)
                return null;
            return result[0];
        }

        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="names">The names of the categorys or games to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want 1 game/category)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames[] ByName(Login.Credentials credentials, string[] names, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            return Go(credentials, names.ToQueryParams("name"), out isSuccess, out httpStatuscode, skipCache);
        }
        #endregion

        #region ByIDs
        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="ids">The ids of the categorys or games to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want 1 game/category)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames[] ByID(Login.Credentials credentials, string[] ids, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            return Go(credentials, ids.ToQueryParams("id"), out isSuccess, out httpStatuscode, skipCache);
        }
        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="id">The id of the category or game to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want multiple games/categorys)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames ByID(Login.Credentials credentials, string id, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            var result = ByID(credentials, new string[1] { id }, out isSuccess, out httpStatuscode, skipCache);
            if (!isSuccess || result == null || result.Length == 0)
                return null;
            return result[0];
        }
        #endregion

        #region ByIGDB_IDs
        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="igdb_id">The igdb_id of the category or game to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want multiple games/categorys)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames ByIGDB_ID(Login.Credentials credentials, string igdb_id, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            var result = ByIGDB_ID(credentials, new string[1] { igdb_id }, out isSuccess, out httpStatuscode, skipCache);
            if (!isSuccess || result == null || result.Length == 0)
                return null;
            return result[0];
        }

        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="igdb_ids">The igdb_ids of the categorys or games to get. The name must exactly match the category’s or game’s title. (use an overload of this method if you only want 1 game/category)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetGames[] ByIGDB_ID(Login.Credentials credentials, string[] igdb_ids, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            return Go(credentials, igdb_ids.ToQueryParams("igdb_id"), out isSuccess, out httpStatuscode, skipCache);
        }
        #endregion

        /// <summary>
        /// <strong>Gets information about specified categories or games.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-games">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="queryParams">Formated Query Parameters for this request (you can find those in the docs linked below)</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetGames, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        private static GetGames[] Go(Login.Credentials credentials, string queryParams, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (queryParams == null || queryParams == "")
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, queryParams); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetGames[])cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + queryParams, method);

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
            var finalobject = JsonConvert.DeserializeObject<GetGames[]>(JsonConvert.SerializeObject(raw.data)); //deserialize into an object of GetExtensionAnalytics
            if (finalobject == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (raw.data.Length == 0)
                return null; //Request successful, but empty response

            finalobject[0].cursor = raw.NextPage(); //add cursor (for pagination) to final object

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// <strong>BE CAREFUL! The cursor only is set in the '0' Element of the Array!</strong> (ignore this warning if a single object was returned) <para><em>Usage: Games[0].cursor</em></para>
        /// <para/>The cursor used to get the next page of results. Use the cursor to set the request’s after query parameter.
        /// </summary>
        [JsonIgnore]
        public string cursor { get; private set; }
        /// <summary>
        /// An ID that identifies the category or game.
        /// </summary>
        [JsonProperty]
        public string id { get; internal set; }
        /// <summary>
        /// The category’s or game’s name.
        /// </summary>
        [JsonProperty]
        public string name { get; internal set; }
        /// <summary>
        /// A URL to the category’s or game’s box art. You must replace the {width}x{height} placeholder with the size of image you want (or use <see cref="Get_BoxArtURL(int, int)"/> instead).
        /// </summary>
        [JsonProperty]
        public string box_art_url { get; internal set; }
        /// <summary>
        /// The ID that IGDB uses to identify this game. If the IGDB ID is not available to Twitch, this field is set to an empty string.
        /// <para><see href="https://www.igdb.com/">What is igdb?</see> - IGDB.com is gathering all relevant information about games in one place. You can use their API to get more infomation about a game.</para>
        /// </summary>
        [JsonProperty]
        public string igdb_id { get; internal set; }
        #endregion
        public string Get_BoxArtURL(int width, int height)
        {
            return box_art_url.Replace("{width}x{height}}", $"{width}x{height}");
        }
    }
}

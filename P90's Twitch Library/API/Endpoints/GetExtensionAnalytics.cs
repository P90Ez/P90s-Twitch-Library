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
    /// <strong>Gets an analytics report for one or more extensions.</strong>
    /// </summary>
    public class GetExtensionAnalytics : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/analytics/extensions";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "analytics:read:extensions";
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
        /// <strong>Gets an analytics report for one or more extensions.</strong>
        /// <para>Scope: <em>analytics:read:extensions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-channel-editors">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="requestQueryParameter"><em>[OPTIONAL]</em> request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetExtensionAnalytics, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetExtensionAnalytics Go(Login.Credentials credentials, QueryParams requestQueryParameter, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            if(requestQueryParameter == null)
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
                    return (GetExtensionAnalytics)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + requestQueryParameter.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var raw = (JsonPagination)request.BodyToClassObject(typeof(JsonPagination)); //deserialize into 'json-housing'
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            } if (raw.data.Length == 0)
                return null; //Request successful, but empty response
            var finalobject = JsonConvert.DeserializeObject<GetExtensionAnalytics>(raw.data[0].ToString()); //deserialize into an object of GetExtensionAnalytics
            finalobject.cursor = raw.NextPage(); //add cursor (for pagination) to final object

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// The cursor used to get the next page of results. Use the cursor to set the request’s after query parameter.
        /// </summary>
        [JsonIgnore]
        public string cursor { get; internal set; }
        /// <summary>
        /// An ID that identifies the extension that the report was generated for.
        /// </summary>
        [JsonProperty]
        public string extension_id { get; internal set; }
        /// <summary>
        /// The URL that you use to download the report. The URL is valid for 5 minutes.
        /// </summary>
        [JsonProperty]
        public string URL { get; internal set; }
        /// <summary>
        /// The type of report.
        /// </summary>
        [JsonProperty]
        public string type { get; internal set; }
        /// <summary>
        /// The reporting window’s start and end dates, in RFC3339 format.
        /// </summary>
        [JsonProperty]
        public Response_date_range date_range { get; internal set; }
        
        #endregion

        /// <summary>
        /// Contains parameter for the request, all of them are optional.
        /// </summary>
        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The extension’s client ID. If specified, the response contains a report for the specified extension. If not specified, the response includes a report for each extension that the authenticated user owns.
            /// </summary>
            public string extension_id { get; set; }
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The type of analytics report to get. Possible values are: overview_v2
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The reporting window’s start date, in RFC3339 format. Set the time portion to zeroes (for example, 2021-10-22T00:00:00Z).
            /// </summary>
            public string started_at { get; set; }
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The reporting window’s end date, in RFC3339 format. Set the time portion to zeroes (for example, 2021-10-27T00:00:00Z). The report is inclusive of the end date.
            /// </summary>
            public string ended_at { get; set; }
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The maximum number of report URLs to return per page in the response. The minimum page size is 1 URL per page and the maximum is 100 URLs per page. The default is 20.
            /// NOTE: While you may specify a maximum value of 100, the response will contain at most 20 URLs per page.
            /// </summary>
            public int? first { get; set; }
            /// <summary>
            /// <strong>[OPTIONAL]</strong>
            /// The cursor used to get the next page of results. The Pagination object in the response contains the cursor’s value.
            /// This parameter is ignored if the extension_id parameter is set.
            /// <para><strong>Note</strong>: The cursor will be recieved with the first request -> <seealso cref="cursor"/></para>
            /// </summary>
            public string after { get; set; }
        }
    }
}

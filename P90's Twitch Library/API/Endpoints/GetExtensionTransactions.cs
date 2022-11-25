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
    /// <strong>Gets a list of transactions for an extension. A transaction records the exchange of a currency (for example, Bits) for a digital product.</strong>
    /// </summary>
    public class GetExtensionTransactions : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/extensions/transactions";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "";
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        public static Login.TokenType RequieredTokenType { get; } = Login.TokenType.AppAccessToken;

        /// <summary>
        /// <strong>Gets a list of transactions for an extension. A transaction records the exchange of a currency (for example, Bits) for a digital product.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-extension-transactions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="requestQueryParameter">[REQUIRED] request parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetExtensionAnalytics, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetExtensionTransactions Go(Login.Credentials credentials, QueryParams requestQueryParameter, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (requestQueryParameter == null) //query params are essential to use this endpoint!
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.user_id, requestQueryParameter); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetExtensionTransactions)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + requestQueryParameter.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            
            var finalobject = JsonConvert.DeserializeObject<GetExtensionTransactions>(request.BodyToString()); //deserialize into an object of GetExtensionAnalytics
            if (finalobject == null || finalobject.data == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (finalobject.data.Length == 0)
                return null; //Request successful, but empty response

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// The list of transactions.
        /// </summary>
        [JsonProperty]
        public GetExtensionTransactionsData[] data { get; internal set; }
        /// <summary>
        /// The cursor used to get the next page of results. Use the cursor to set the request’s after query parameter.
        /// </summary>
        [JsonIgnore]
        public string cursor { get { if (page == null) return null; return page.cursor; } }
        [JsonProperty]
        internal Pagination page { get; set; }
        /// <summary>
        /// (object use only)
        /// </summary>
        public class Cost
        {
            /// <summary>
            /// The amount exchanged for the digital product.
            /// </summary>
            [JsonProperty]
            public int amount { get; internal set; }
            /// <summary>
            /// The type of currency exchanged. Possible values are: bits
            /// </summary>
            [JsonProperty]
            public string type { get; internal set; }
        }

        /// <summary>
        /// (object use only) Contains data from transactions.
        /// </summary>
        public class GetExtensionTransactionsData
        {
            /// <summary>
            /// An ID that identifies the transaction.
            /// </summary>
            [JsonProperty]
            public string id { get; internal set; }
            /// <summary>
            /// The UTC date and time (in RFC3339 format) of the transaction.
            /// </summary>
            [JsonProperty]
            public DateTime timestamp { get; internal set; }
            /// <summary>
            /// The ID of the broadcaster that owns the channel where the transaction occurred.
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
            /// The ID of the user that purchased the digital product.
            /// </summary>
            [JsonProperty]
            public string user_id { get; internal set; }
            /// <summary>
            /// The login name of the user that purchased the digital product.
            /// </summary>
            [JsonProperty]
            public string user_login { get; internal set; }
            /// <summary>
            /// The display name of the user that purchased the digital product.
            /// </summary>
            [JsonProperty]
            public string user_name { get; internal set; }
            /// <summary>
            /// The type of transaction. Possible values are:BITS_IN_EXTENSION
            /// </summary>
            [JsonProperty]
            public string product_type { get; internal set; }
            /// <summary>
            /// Contains details about the digital product.
            /// </summary>
            [JsonProperty]
            public ProductData product_data { get; internal set; }
        }

        /// <summary>
        /// (object use only) Contains details about the digital product.
        /// </summary>
        public class ProductData
        {
            /// <summary>
            /// Set to 'twitch.ext.&lt;the extension's ID&gt;'.
            /// </summary>
            public string domain { get; internal set; }
            /// <summary>
            /// An ID that identifies the digital product.
            /// </summary>
            [JsonProperty]
            public string sku { get; internal set; }
            /// <summary>
            /// Contains details about the digital product’s cost.
            /// </summary>
            [JsonProperty]
            public Cost cost { get; internal set; }
            /// <summary>
            /// A Boolean value that determines whether the product is in development. Is true if the digital product is in development and cannot be exchanged.
            /// </summary>
            [JsonProperty]
            public bool inDevelopment { get; internal set; }
            /// <summary>
            /// The name of the digital product.
            /// </summary>
            [JsonProperty]
            public string displayName { get; internal set; }
            /// <summary>
            /// This field is always empty since you may purchase only unexpired products.
            /// </summary>
            [JsonProperty]
            public string expiration { get; internal set; }
            /// <summary>
            /// A Boolean value that determines whether the data was broadcast to all instances of the extension. Is true if the data was broadcast to all instances.
            /// </summary>
            [JsonProperty]
            public bool broadcast { get; internal set; }
        }
        internal class Pagination
        {
            [JsonProperty]
            public string cursor { get; internal set; }
        }

        #endregion

        public class QueryParams : JsonApplication
        {
            /// <summary>
            /// [REQUIRED] The ID of the extension whose list of transactions you want to get. (The ID in the extension_id query parameter must match the client ID in the access token.)
            /// </summary>
            public string extension_id { get; set; }
            /// <summary>
            /// [OPTIONAL] A transaction ID used to filter the list of transactions. Specify this parameter for each transaction you want to get. For example, {1234, 5678}. You may specify a maximum of 100 IDs.
            /// </summary>
            public string[] id { get; set; }
            /// <summary>
            /// The maximum number of items to return per page in the response. The minimum page size is 1 item per page and the maximum is 100 items per page. The default is 20.
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

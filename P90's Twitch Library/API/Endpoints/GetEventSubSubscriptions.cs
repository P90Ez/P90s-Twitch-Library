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
    /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
    /// </summary>
    public class GetEventSubSubscriptions : JsonPagination, IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/eventsub/subscriptions";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "";
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        private static Login.TokenType RequieredTokenType { get; } = Login.TokenType.Any;
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        public static Login.TokenType RequieredTokenType_Websocket { get; } = Login.TokenType.UserAccessToken;
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        public static Login.TokenType RequieredTokenType_Webhook { get; } = Login.TokenType.AppAccessToken;

        /// <summary>
        /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>, EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-eventsub-subscriptions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="application">Specify filters - the filters are mutually exclusive; the request fails if you specify more than one filter.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        private static GetEventSubSubscriptions Go(Login.Credentials credentials, Application application, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (application != null && !application.ContainsOnlyOneFilter()) return null;
            if(application == null) application = new Application();    

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.UserId, application.ToString()); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetEventSubSubscriptions)cachedObject;
                }
            }

            //make request
            var request = APICom.Request(out isSuccess, credentials, EndpointURL + application.ToQueryParameters(), method);

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            var finalobject = request.BodyToClassObject<GetEventSubSubscriptions>(); //deserialize into an object of GetEventSubSubscriptions

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Websocket
        /// <summary>
        /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-eventsub-subscriptions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="application">Specify filters - the filters are mutually exclusive; the request fails if you specify more than one filter.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetEventSubSubscriptions ForWebsocket(Login.Credentials credentials, Application application, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (credentials == null || credentials.TokenType != RequieredTokenType_Websocket) return null;
            return Go(credentials, application, out isSuccess, out httpStatuscode, skipCache);
        }
        /// <summary>
        /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-eventsub-subscriptions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetEventSubSubscriptions ForWebsocket(Login.Credentials credentials, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (credentials == null || credentials.TokenType != RequieredTokenType_Websocket) return null;
            return Go(credentials, null, out isSuccess, out httpStatuscode, skipCache);
        }
        #endregion
        #region Webhook
        /// <summary>
        /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-eventsub-subscriptions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="application">Specify filters - the filters are mutually exclusive; the request fails if you specify more than one filter.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetEventSubSubscriptions ForWebhook(Login.Credentials credentials, Application application, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (credentials == null || credentials.TokenType != RequieredTokenType_Webhook) return null;
            return Go(credentials, application, out isSuccess, out httpStatuscode, skipCache);
        }
        /// <summary>
        /// <strong>Gets a list of EventSub subscriptions that the client in the access token created.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#get-eventsub-subscriptions">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetEventSubSubscriptions ForWebhook(Login.Credentials credentials, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (credentials == null || credentials.TokenType != RequieredTokenType_Webhook) return null;
            return Go(credentials, null, out isSuccess, out httpStatuscode, skipCache);
        }
        #endregion

        #region Object Vars
        /// <summary>
        /// The list of subscriptions. The list is ordered by the oldest subscription first. The list is empty if the client hasn’t created subscriptions or there are no subscriptions that match the specified filter criteria.
        /// </summary>
        [JsonProperty("data")]
        public new List<SubscriptionData> data { get; set; }

        /// <summary>
        /// The total number of subscriptions you’ve created.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; internal set; }

        /// <summary>
        /// The sum of all of your subscription costs. <see href="https://dev.twitch.tv/docs/eventsub/manage-subscriptions/#subscription-limits">Learn More</see>
        /// </summary>
        [JsonProperty("total_cost")]
        public int Total_Cost { get; internal set; }
        /// <summary>
        /// The maximum total cost that you’re allowed to incur for all subscriptions you create.
        /// </summary>
        [JsonProperty("max_total_cost")]
        public int Max_Total_Cost { get; internal set; }

        public class SubscriptionData
        {
            /// <summary>
            /// An ID that identifies the subscription.
            /// </summary>
            [JsonProperty("id")]
            public string ID { get; internal set; }

            /// <summary>
            /// The subscription’s status. The subscriber receives events only for enabled subscriptions. Possible values are: 
            /// <list type="bullet">
            /// <item><strong>enabled</strong> - The subscription is enabled.</item>
            /// <item><strong>webhook_callback_verification_pending</strong> <para/>- The subscription is pending verification of the specified callback URL (see <see href="https://dev.twitch.tv/docs/eventsub/handling-webhook-events#responding-to-a-challenge-request"> Responding to a challenge request</see>).</item>
            /// <item><strong>webhook_callback_verification_failed</strong> - The specified callback URL failed verification.</item>
            /// <item><strong>notification_failures_exceeded</strong> - The notification delivery failure rate was too high.</item>
            /// <item><strong>authorization_revoked</strong> - The authorization was revoked for one or more users specified in the Condition object.</item>
            /// <item><strong>moderator_removed</strong> - The moderator that authorized the subscription is no longer one of the broadcaster's moderators.</item>
            /// <item><strong>user_removed</strong> - One of the users specified in the Condition object was removed.</item>
            /// <item><strong>version_removed</strong> - The subscribed to subscription type and version is no longer supported.</item>
            /// <item><strong>websocket_disconnected</strong> - The client closed the connection.</item>
            /// <item><strong>websocket_failed_ping_pong</strong> - The client failed to respond to a ping message.</item>
            /// <item><strong>websocket_received_inbound_traffic</strong><para/> - The client sent a non-pong message. Clients may only send pong messages (and only in response to a ping message).</item>
            /// <item><strong>websocket_connection_unused</strong> - The client failed to subscribe to events within the required time.</item>
            /// <item><strong>websocket_internal_error</strong> - The Twitch WebSocket server experienced an unexpected error.</item>
            /// <item><strong>websocket_network_timeout</strong> - The Twitch WebSocket server timed out writing the message to the client.</item>
            /// <item><strong>websocket_network_error</strong> - The Twitch WebSocket server experienced a network error writing the message to the client.</item>
            /// </list>
            /// </summary>
            [JsonProperty("status")]
            public string Status { get; internal set; }

            /// <summary>
            /// The subscription’s type. See <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types</see>.
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; internal set; }

            /// <summary>
            /// The version number that identifies this definition of the subscription’s data.
            /// </summary>
            [JsonProperty("version")]
            public string version { get; internal set; }

            /// <summary>
            /// The subscription’s parameter values. This is a string-encoded JSON object whose contents are determined by the subscription type.
            /// </summary>
            [JsonProperty("condition")]
            public object Condition { get; internal set; }

            /// <summary>
            /// The date and time of when the subscription was created.
            /// </summary>
            [JsonProperty("created_at")]
            public DateTime created_at { get; internal set; }

            /// <summary>
            /// The transport details used to send the notifications.
            /// </summary>
            [JsonProperty("transport")]
            public _Transport Transport { get; internal set; }

            /// <summary>
            /// The amount that the subscription counts against your limit. <see href="https://dev.twitch.tv/docs/eventsub/manage-subscriptions/#subscription-limits">Learn More</see>
            /// </summary>
            [JsonProperty("cost")]
            public int Cost { get; internal set; }
        }

        public class _Transport
        {
            /// <summary>
            /// The transport method. Possible values are: "webhook", "websocket"
            /// </summary>
            [JsonProperty("method")]
            public string Method { get; internal set; }

            /// <summary>
            /// The callback URL where the notifications are sent. <strong>Included only if method is set to webhook</strong>.
            /// </summary>
            [JsonProperty("callback")]
            public string Callback { get; internal set; }

            /// <summary>
            /// An ID that identifies the WebSocket that notifications are sent to. <strong>Included only if method is set to websocket</strong>.
            /// </summary>
            [JsonProperty("session_id")]
            public string Session_ID { get; internal set; }

            /// <summary>
            /// The UTC date and time that the WebSocket connection was established. <strong>Included only if method is set to websocket</strong>.
            /// </summary>
            [JsonProperty("connected_at")]
            public string connected_at { get; internal set; }
        }
        #endregion

        /// <summary>
        /// Use the status, type, and user_id query parameters to filter the list of subscriptions that are returned. The filters are mutually exclusive; the request fails if you specify more than one filter.
        /// </summary>
        public class Application : JsonApplication
        {
            /// <summary>
            /// Filter subscriptions by its status. Possible values are:  
            /// <list type="bullet">
            /// <item><strong>enabled</strong> - The subscription is enabled.</item>
            /// <item><strong>webhook_callback_verification_pending</strong> <para/>- The subscription is pending verification of the specified callback URL (see <see href="https://dev.twitch.tv/docs/eventsub/handling-webhook-events#responding-to-a-challenge-request"> Responding to a challenge request</see>).</item>
            /// <item><strong>webhook_callback_verification_failed</strong> - The specified callback URL failed verification.</item>
            /// <item><strong>notification_failures_exceeded</strong> - The notification delivery failure rate was too high.</item>
            /// <item><strong>authorization_revoked</strong> - The authorization was revoked for one or more users specified in the Condition object.</item>
            /// <item><strong>moderator_removed</strong> - The moderator that authorized the subscription is no longer one of the broadcaster's moderators.</item>
            /// <item><strong>user_removed</strong> - One of the users specified in the Condition object was removed.</item>
            /// <item><strong>version_removed</strong> - The subscribed to subscription type and version is no longer supported.</item>
            /// <item><strong>websocket_disconnected</strong> - The client closed the connection.</item>
            /// <item><strong>websocket_failed_ping_pong</strong> - The client failed to respond to a ping message.</item>
            /// <item><strong>websocket_received_inbound_traffic</strong><para/> - The client sent a non-pong message. Clients may only send pong messages (and only in response to a ping message).</item>
            /// <item><strong>websocket_connection_unused</strong> - The client failed to subscribe to events within the required time.</item>
            /// <item><strong>websocket_internal_error</strong> - The Twitch WebSocket server experienced an unexpected error.</item>
            /// <item><strong>websocket_network_timeout</strong> - The Twitch WebSocket server timed out writing the message to the client.</item>
            /// <item><strong>websocket_network_error</strong> - The Twitch WebSocket server experienced a network error writing the message to the client.</item>
            /// </list>
            /// </summary>
            [JsonProperty("status")]
            public string Status { get; set; }
            /// <summary>
            /// Filter subscriptions by subscription type. For a list of subscription types, see <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types</see>.
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; set; }
            /// <summary>
            /// Filter subscriptions by user ID. The response contains subscriptions where this ID matches a user ID that you specified in the Condition object when you created the subscription.
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; set; }
            /// <summary>
            /// The cursor used to get the next page of results. The pagination object in the response contains the cursor’s value (use <see cref="Background.JsonPagination.NextPage">NextPage()</see> function).
            /// </summary>
            [JsonProperty("after")]
            public string PageId { get; set; }

            internal bool ContainsOnlyOneFilter()
            {
                ushort filterAmount = 0;
                if (Status != null) filterAmount++;
                if (Type != null) filterAmount++;
                if (UserId != null) filterAmount++;
                return filterAmount <= 1;
            }
        }

    }
}

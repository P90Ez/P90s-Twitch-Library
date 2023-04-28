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
    public class CreateEventSubSubscription : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Post;
        private static string EndpointURL = "https://api.twitch.tv/helix/eventsub/subscriptions";
        /// <summary>
        /// Requiered Scopes to use this endpoint. - Not relevant for this endpoint. Too many different scopes.
        /// </summary>
        private static string RequieredScopes { get; } = "";
        /// <summary>
        /// Requiered Tokentype to use this endpoint. - EventSub via Webhook requieres an <strong>App Access Token</strong>, EventSub via WebSocket requieres an <strong>User Access Token</strong>!
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
        /// <strong>Creates an EventSub subscription.</strong> - use the builtin EventSub module for easy access!
        /// <para>Scopes: depends on the event you want to subscribe to. <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Learn more</see></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>, EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="application">Request body parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetEventSubSubscriptions, containing response variables from this request. (only if request was successful)</returns>
        public static GetEventSubSubscriptions Go(Login.Credentials credentials, Application application, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (application == null || application.type == null ||application.transport.method == null)
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL, method, application.ToString()); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            //deserialize into an object of GetEventSubSubscriptions
            var obj = request.BodyToClassObject<GetEventSubSubscriptions>();

            return obj;
        }

        #region Websocket
        /// <summary>
        /// <strong>Creates an EventSub subscription (for EventSub with Websocket).</strong> - use the builtin EventSub module for easy access!
        /// <para>Scopes: depends on the event you want to subscribe to. <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Learn more</see></para>
        /// <para>TokenType: EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Session_Id">An ID that identifies the WebSocket to send notifications to. When you connect to EventSub using WebSockets, the server returns the ID in the <see href="https://dev.twitch.tv/docs/eventsub/handling-websocket-events#welcome-message">Welcome message.</see></param>
        /// <param name="Type">The subscription’s type. See <see hcref="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types.</see></param>
        /// <param name="Condition">A Key-Value-Pair that contains the parameter values that are specific to the specified subscription type. For the object’s required and optional fields, see the subscription type’s documentation. <strong>Example</strong>: Key: "user_id" Value: "1234"</param>
        /// <param name="Version">The version number that identifies the definition of the subscription type that you want the response to use.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetCustomReward, containing response variables from this request. (only if request was successful)</returns>
        public static GetEventSubSubscriptions ForWebsocket(Login.Credentials credentials, string Session_Id, string Type, ushort Version, List<KeyValuePair<string, string>> Condition, out bool isSuccess, out int httpStatuscode)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (Session_Id == null || Session_Id == "" || Condition == null || Condition.Count == 0) return null; //check input
            if (credentials.TokenType != RequieredTokenType_Websocket) return null; //wrong credentials
            //package into application object
            Application application = new Application() { transport = new Application.Transport() { method = "websocket", session_id = Session_Id }, type = Type, version = Version.ToString(), condition = Condition.ToObject() };
            return Go(credentials, application, out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Creates an EventSub subscription (for EventSub with Websocket).</strong> - use the builtin EventSub module for easy access!
        /// <para>Scopes: depends on the event you want to subscribe to. <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Learn more</see></para>
        /// <para>TokenType: EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Session_Id">An ID that identifies the WebSocket to send notifications to. When you connect to EventSub using WebSockets, the server returns the ID in the <see href="https://dev.twitch.tv/docs/eventsub/handling-websocket-events#welcome-message">Welcome message.</see></param>
        /// <param name="Type">The subscription’s type. See <see hcref="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types.</see></param>
        /// <param name="Condition">A Key-Value-Pair that contains the parameter values that are specific to the specified subscription type. For the object’s required and optional fields, see the subscription type’s documentation. <strong>Example</strong>: Key: "user_id" Value: "1234"</param>
        /// <param name="Version">The version number that identifies the definition of the subscription type that you want the response to use.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetCustomReward, containing response variables from this request. (only if request was successful)</returns>
        public static GetEventSubSubscriptions ForWebsocket(Login.Credentials credentials, string Session_Id, string Type, ushort Version, KeyValuePair<string, string> Condition, out bool isSuccess, out int httpStatuscode)
        {
            return ForWebsocket(credentials, Session_Id, Type, Version, new List<KeyValuePair<string, string>>() { Condition},out isSuccess, out httpStatuscode);
        }
        #endregion

        #region Webhook
        /// <summary>
        /// <strong>Creates an EventSub subscription (for EventSub with Webhook).</strong> - use the builtin EventSub module for easy access!
        /// <para>Scopes: depends on the event you want to subscribe to. <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Learn more</see></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Type">The subscription’s type. See <see hcref="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types.</see></param>
        /// <param name="Condition">A Key-Value-Pair that contains the parameter values that are specific to the specified subscription type. For the object’s required and optional fields, see the subscription type’s documentation. <strong>Example</strong>: Key: "user_id" Value: "1234"</param>
        /// <param name="Version">The version number that identifies the definition of the subscription type that you want the response to use.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetCustomReward, containing response variables from this request. (only if request was successful)</returns>
        public static GetEventSubSubscriptions ForWebhook(Login.Credentials credentials, string CallBack, string Secret, string Type, ushort Version, List<KeyValuePair<string, string>> Condition, out bool isSuccess, out int httpStatuscode)
        {
            isSuccess = false;
            httpStatuscode = 0;
            if (CallBack == null || CallBack == "" || Secret == null || Secret == "" || Condition == null || Condition.Count == 0) return null; //check input
            if (credentials.TokenType != RequieredTokenType_Webhook) return null; //wrong credentials
            //package into application object
            Application application = new Application() { transport = new Application.Transport() { method = "webhook", callback = CallBack, secret = Secret}, type = Type, version = Version.ToString(), condition = Condition.ToObject() };
            return Go(credentials, application, out isSuccess, out httpStatuscode);
        }

        /// <summary>
        /// <strong>Creates an EventSub subscription (for EventSub with Webhook).</strong> - use the builtin EventSub module for easy access!
        /// <para>Scopes: depends on the event you want to subscribe to. <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Learn more</see></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="Type">The subscription’s type. See <see hcref="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types">Subscription Types.</see></param>
        /// <param name="Condition">A Key-Value-Pair that contains the parameter values that are specific to the specified subscription type. For the object’s required and optional fields, see the subscription type’s documentation. <strong>Example</strong>: Key: "user_id" Value: "1234"</param>
        /// <param name="Version">The version number that identifies the definition of the subscription type that you want the response to use.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetCustomReward, containing response variables from this request. (only if request was successful)</returns>
        public static GetEventSubSubscriptions ForWebhook(Login.Credentials credentials, string CallBack, string Secret, string Type, ushort Version, KeyValuePair<string, string> Condition, out bool isSuccess, out int httpStatuscode)
        {
            return ForWebhook(credentials, CallBack, Secret, Type, Version, new List<KeyValuePair<string, string>>() { Condition}, out isSuccess, out httpStatuscode);
        }
        #endregion

        

        public class Application : JsonApplication
        {
            /// <summary>
            /// The type of subscription to create. For a list of subscriptions that you can create, see <see href="https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types#subscription-types"> Subscription Types </see>. Set this field to the value in the Name column of the Subscription Types table.
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// The version number that identifies the definition of the subscription type that you want the response to use.
            /// </summary>
            public string version { get; set; }
            /// <summary>
            /// An object that contains the parameter values that are specific to the specified subscription type. For the object’s required and optional fields, see the subscription type’s documentation.
            /// </summary>
            public object condition { get; set; }
            /// <summary>
            /// The transport details that you want Twitch to use when sending you notifications.
            /// </summary>
            public Transport transport { get; set; }


            public class Transport
            {
                /// <summary>
                /// The transport method. Possible values are: "webhook", "websocket"
                /// </summary>
                public string method { get; set; }
                /// <summary>
                ///  The callback URL where the notifications are sent. The URL must use the HTTPS protocol and port 443. See <see href="https://dev.twitch.tv/docs/eventsub/handling-webhook-events#processing-an-event"> Processing an event </see>.
                ///  <para/><strong>Specify this field only if method is set to webhook.</strong>
                ///  <para/>NOTE: Redirects are not followed.
                /// </summary>
                public string callback { get; set; }
                /// <summary>
                /// The secret used to verify the signature. The secret must be an ASCII string that’s a minimum of 10 characters long and a maximum of 100 characters long. For information about how the secret is used, see <see href="https://dev.twitch.tv/docs/eventsub/handling-webhook-events#verifying-the-event-message"> Verifying the event message </see>. 
                /// <para/><strong>Specify this field only if method is set to webhook.</strong>
                /// </summary>
                public string secret { get; set; }
                /// <summary>
                /// An ID that identifies the WebSocket to send notifications to. When you connect to EventSub using WebSockets, the server returns the ID in the <see href="https://dev.twitch.tv/docs/eventsub/handling-websocket-events#welcome-message">Welcome message</see>. 
                /// <para/><strong>Specify this field only if method is set to websocket.</strong>
                /// </summary>
                public string session_id { get; set; }
            }
        }
    }
}

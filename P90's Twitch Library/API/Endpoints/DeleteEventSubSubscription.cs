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
    /// <strong>Deletes an EventSub subscription.</strong>
    /// </summary>
    public class DeleteEventSubSubscription : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Delete;
        private static string EndpointURL = "https://api.twitch.tv/helix/eventsub/subscriptions";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "";
        /// <summary>
        /// Requiered Tokentype to use this endpoint.
        /// </summary>
        public static Login.TokenType RequieredTokenType { get; } = Login.TokenType.Any;

        /// <summary>
        /// <strong>Deletes an EventSub subscription.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: EventSub via Webhook requieres an <strong>App Access Token</strong>, EventSub via WebSocket requieres an <strong>User Access Token</strong>!</para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#delete-eventsub-subscription">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="ID">The ID of the subscription to delete.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void Go(Login.Credentials credentials, string ID, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;

            if (ID == null || ID == "") return; //check if id is not empty

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) return; //return if credential ceck failed


            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?id={ID}", method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;
        }
    }
}

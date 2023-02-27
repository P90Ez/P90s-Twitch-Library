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
    /// <strong>Deletes a custom channelpoint reward that the broadcaster created.</strong>
    /// </summary>
    public class DeleteCustomReward : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Delete;
        private static string EndpointURL = "https://api.twitch.tv/helix/channel_points/custom_rewards";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "channel:manage:redemptions";
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
        /// <strong>Deletes a custom channelpoint reward that the broadcaster created.</strong>
        /// <para/>Note: The app used to create the reward is the only app that may delete it. If the reward’s redemption status is UNFULFILLED at the time the reward is deleted, its redemption status is marked as FULFILLED.
        /// <para>Scope: <em>channel:manage:redemptions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#delete-custom-reward">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">[REQUIRED] The ID of the broadcaster that created the custom reward.</param>
        /// <param name="id">[REQUIRED] The ID of the custom reward to delete.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of StartCommercial, containing response variables from this request. (only if request was successful)</returns>
        public static void Go(Login.Credentials credentials, string broadcaster_id, string id, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;

            if (broadcaster_id == null || broadcaster_id == "" || id == null || id == "")
                return;

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            
            if (!isSuccess) //return if credential ceck failed
                return;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id={broadcaster_id}&id={id}", method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;
        }

    }
}

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
    /// <strong>Creates a Custom Channelpoint Reward in the broadcaster’s channel.</strong>
    /// </summary>
    public class CreateCustomRewards : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Post;
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
        /// <strong>Creates a Custom Channelpoint Reward in the broadcaster’s channel.</strong>
        /// <para>Scope: <em>channel:manage:redemptions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#create-custom-rewards">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id">The ID of the broadcaster to add the custom reward to.</param>
        /// <param name="application">Request body parameters</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        /// <returns>An object of GetCustomReward, containing response variables from this request. (only if request was successful)</returns>
        public static GetCustomReward Go(Login.Credentials credentials, string broadcaster_id, Application application, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (broadcaster_id == null || broadcaster_id == "" || application == null)
                return null;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL+ $"?broadcaster_id={broadcaster_id}", method, application.ToString()); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            JsonDataArray raw = (JsonDataArray)request.BodyToClassObject(typeof(JsonDataArray)); //deserialize into 'json-housing'
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            else if (raw.data.Length > 0)
                return JsonConvert.DeserializeObject<GetCustomReward>(raw.data[0].ToString()); //deserialize into an object of GetCustomReward
            else return null; //request successful but empty response
        }

        public class Application : JsonApplication
        {
            /// <summary>
            /// <strong>[REQUIRED]</strong> The custom reward’s title. The title may contain a maximum of 45 characters and it must be unique amongst all of the broadcaster’s custom rewards.
            /// </summary>
            public string title { get; set; }
            /// <summary>
            /// <strong>[REQUIRED]</strong> The cost of the reward, in Channel Points. The minimum is 1 point.
            /// </summary>
            public long? cost { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The prompt shown to the viewer when they redeem the reward. Specify a prompt if is_user_input_required is true. The prompt is limited to a maximum of 200 characters.
            /// </summary>
            public string prompt { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether the reward is enabled. Viewers see only enabled rewards. The default is true.
            /// </summary>
            public bool is_enabled { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The background color to use for the reward. Specify the color using Hex format (for example, #9147FF).
            /// </summary>
            public string background_color { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether the user needs to enter information when redeeming the reward. See the prompt field. The default is false.
            /// </summary>
            public bool is_user_input_required { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether to limit the maximum number of redemptions allowed per live stream (see the max_per_stream field). The default is false.
            /// </summary>
            public bool is_max_per_stream_enabled { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The maximum number of redemptions allowed per live stream. Applied only if is_max_per_stream_enabled is true. The minimum value is 1.
            /// </summary>
            public long? max_per_stream { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether to limit the maximum number of redemptions allowed per user per stream (see the max_per_user_per_stream field). The default is false.
            /// </summary>
            public bool is_max_per_user_per_stream_enabled { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The maximum number of redemptions allowed per user per stream. Applied only if is_max_per_user_per_stream_enabled is true. The minimum value is 1.
            /// </summary>
            public long? max_per_user_per_stream { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether to apply a cooldown period between redemptions (see the global_cooldown_seconds field for the duration of the cooldown period). The default is false.
            /// </summary>
            public bool is_global_cooldown_enabled { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> The cooldown period, in seconds. Applied only if the is_global_cooldown_enabled field is true. The minimum value is 1; however, the minimum value is 60 for it to be shown in the Twitch UX.
            /// </summary>
            public long? global_cooldown_seconds { get; set; }
            /// <summary>
            /// <em>[OPTIONAL]</em> A Boolean value that determines whether redemptions should be set to FULFILLED status immediately when a reward is redeemed. If false, status is set to UNFULFILLED and follows the normal request queue process. The default is false.
            /// </summary>
            public bool should_redemptions_skip_request_queue { get; set; }
        }
    }
}

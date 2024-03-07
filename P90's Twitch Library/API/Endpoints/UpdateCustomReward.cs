using Newtonsoft.Json;
using P90Ez.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    public class UpdateCustomReward
    {
        private static HttpMethod method = HttpMethod.Patch;
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
        /// <strong>Updates a custom reward. The app used to create the reward is the only app that may update the reward.</strong>
        /// <para>Scope: <em>channel:manage:redemptions</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-custom-reward">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="BroadcasterId">The ID of the broadcaster that’s updating the reward. This ID must match the user ID found in the OAuth token.</param>
        /// <param name="RewardId">The ID of the reward to update.</param>
        /// <param name="Body">The body of the request should contain only the fields you’re updating.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <returns>An array of GetCustomReward, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetCustomReward Go(Login.Credentials credentials, string BroadcasterId, string RewardId, RequestBody Body, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;

            if (BroadcasterId == null || BroadcasterId == String.Empty) return null;
            if (RewardId == null || RewardId == String.Empty) return null;
            if (Body == null) return null;

            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) return null; //return if credential ceck failed

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?broadcaster_id=={BroadcasterId}&id={RewardId}", method, Body.ToJsonString()); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            //deserialize into an object of GetEventSubSubscriptions
            var obj = request.BodyToClassObject<GetCustomReward>();

            return obj;
        }

        /// <summary>
        /// The body of the request should contain only the fields you’re updating.
        /// </summary>
        public class RequestBody : JsonApplication
        {
            /// <summary>
            /// The reward’s title. The title may contain a maximum of 45 characters and it must be unique amongst all of the broadcaster’s custom rewards.
            /// </summary>
            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            /// <summary>
            /// The prompt shown to the viewer when they redeem the reward. Specify a prompt if <seealso cref="UserInputRequired"/> is true. The prompt is limited to a maximum of 200 characters.
            /// </summary>
            [JsonProperty("prompt", NullValueHandling = NullValueHandling.Ignore)]
            public string Prompt { get; set; }

            /// <summary>
            /// The cost of the reward, in channel points. The minimum is 1 point.
            /// </summary>
            [JsonProperty("cost", NullValueHandling = NullValueHandling.Ignore)]
            public int? Cost { get; set; }

            /// <summary>
            /// The background color to use for the reward. Specify the color using Hex format (for example, #00E5CB).
            /// </summary>
            [JsonProperty("background_color", NullValueHandling = NullValueHandling.Ignore)]
            public string BackgroundColor { get; set; }

            /// <summary>
            /// A Boolean value that indicates whether the reward is enabled. Set to true to enable the reward. Viewers see only enabled rewards.
            /// </summary>
            [JsonProperty("is_enabled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsEnabled { get; set; }

            /// <summary>
            /// A Boolean value that determines whether users must enter information to redeem the reward. Set to true if user input is required. See the <seealso cref="Prompt"/> field.
            /// </summary>
            [JsonProperty("is_user_input_required", NullValueHandling = NullValueHandling.Ignore)]
            public bool? UserInputRequired { get; set; }

            /// <summary>
            /// A Boolean value that determines whether to limit the maximum number of redemptions allowed per live stream (see the <seealso cref="MaxPerStream"/> field). Set to true to limit redemptions.
            /// </summary>
            [JsonProperty("is_max_per_stream_enabled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? MaxPerStreamEnabled { get; set; }

            /// <summary>
            /// The maximum number of redemptions allowed per live stream. Applied only if <seealso cref="MaxPerStreamEnabled"/> is true. The minimum value is 1.
            /// </summary>
            [JsonProperty("max_per_stream", NullValueHandling = NullValueHandling.Ignore)]
            public long? MaxPerStream { get; set; }

            /// <summary>
            /// A Boolean value that determines whether to limit the maximum number of redemptions allowed per user per stream (see <seealso cref="MaxPerUserPerStream"/>). The minimum value is 1. Set to true to limit redemptions.
            /// </summary>
            [JsonProperty("is_max_per_user_per_stream_enabled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? MaxPerUserPerStreamEnabled { get; set; }

            /// <summary>
            /// The maximum number of redemptions allowed per user per stream. Applied only if <seealso cref="MaxPerUserPerStream"/> is true.
            /// </summary>
            [JsonProperty("max_per_user_per_stream", NullValueHandling = NullValueHandling.Ignore)]
            public long? MaxPerUserPerStream { get; set; }

            /// <summary>
            /// A Boolean value that determines whether to apply a cooldown period between redemptions. Set to true to apply a cooldown period. For the duration of the cooldown period, see <seealso cref="GlobalCooldown"/>.
            /// </summary>
            [JsonProperty("is_global_cooldown_enabled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? GloabCooldownEnabled { get; set; }

            /// <summary>
            /// The cooldown period, in seconds. Applied only if <seealso cref="GloabCooldownEnabled"/> is true. The minimum value is 1; however, for it to be shown in the Twitch UX, the minimum value is 60.
            /// </summary>
            [JsonProperty("global_cooldown_seconds", NullValueHandling = NullValueHandling.Ignore)]
            public long? GlobalCooldown { get; set; }

            /// <summary>
            /// A Boolean value that determines whether to pause the reward. Set to true to pause the reward. Viewers can’t redeem paused rewards.
            /// </summary>
            [JsonProperty("is_paused", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPaused { get; set; }

            /// <summary>
            /// A Boolean value that determines whether redemptions should be set to FULFILLED status immediately when a reward is redeemed. If false, status is set to UNFULFILLED and follows the normal request queue process.
            /// </summary>
            [JsonProperty("should_redemptions_skip_request_queue", NullValueHandling = NullValueHandling.Ignore)]
            public bool? SkipRequestQueue { get; set; }
        }
    }
}

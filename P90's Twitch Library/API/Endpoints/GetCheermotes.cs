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
    /// <strong>Gets a list of Cheermotes that users can use to cheer Bits in any Bits-enabled channel’s chat room.</strong>
    /// </summary>
    public class GetCheermotes : IStandardEndpoint, ICacheRequest
    {
        private static HttpMethod method = HttpMethod.Get;
        private static string EndpointURL = "https://api.twitch.tv/helix/bits/cheermotes";
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
        /// <strong>Gets a list of Cheermotes that users can use to cheer Bits in any Bits-enabled channel’s chat room.</strong>
        /// <para>Scope: <em>-</em></para>
        /// <para>TokenType: <em>User Access Token or App Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference#get-cheermotes">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="broadcaster_id"><em>[OPTIONAL]</em> The ID of the broadcaster whose custom Cheermotes you want to get.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server, -> -200 if data is from cache)</param>
        /// <param name="skipCache">if True, cache won't be read</param>
        /// <returns>An array of GetCheermotes, containing response variables from this request. (only if item was found in cache, or request was successful)</returns>
        public static GetCheermotes[] Go(Login.Credentials credentials, string broadcaster_id, out bool isSuccess, out int httpStatuscode, bool skipCache = false)
        {
            httpStatuscode = 0;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return null;

            string cacheInputParas = IStandardEndpoint.InputParasBuilder(credentials.user_id, broadcaster_id); //builds the cache input parameter
            if (!skipCache) //should cache be skipped?
            {               //no? -> Read Cache!
                bool isCachedObjValid = false;
                var cachedObject = ICacheRequest.TryGetCachedObject(cacheInputParas, out isCachedObjValid); //Try to get the cached object if there is one.
                if (isCachedObjValid && cachedObject != null) //if cache is still valid then return cached object
                {
                    httpStatuscode = ICacheRequest.CacheStatusCode; //-200: object from cache.
                    isSuccess = true;
                    return (GetCheermotes[])cachedObject;
                }
            }

            string tmpUrl = broadcaster_id != null && broadcaster_id != "" ? EndpointURL + $"?broadcaster_id={broadcaster_id}" : EndpointURL; //if broadcaster_id is not null & not empty -> add as query parameter
            var request = APICom.Request(out isSuccess, credentials, tmpUrl, method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;

            if (!isSuccess) //return if request failed
                return null;

            
             JsonDataArray raw = (JsonDataArray)request.BodyToClassObject(typeof(JsonDataArray)); //deserialize into an object of GetCheermotes
            if (!raw.IsDataNotNull()) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (raw.data.Length == 0)
                return null; //Request successful, but empty response
            var finalobject = JsonConvert.DeserializeObject<GetCheermotes[]>(JsonConvert.SerializeObject(raw.data));
            if (finalobject == null) //check if data has been extracted to data array
            {
                isSuccess = false;
                return null;
            }
            if (finalobject.Length == 0)
                return null; //Request successful, but empty response

            //add final object to cache
            ICacheRequest.AddToCache(cacheInputParas, finalobject);

            return finalobject;
        }

        #region Object Vars
        /// <summary>
        /// The name portion of the Cheermote string that you use in chat to cheer Bits. The full Cheermote string is the concatenation of {prefix} + {number of Bits}. For example, if the prefix is “Cheer” and you want to cheer 100 Bits, the full Cheermote string is Cheer100. When the Cheermote string is entered in chat, Twitch converts it to the image associated with the Bits tier that was cheered.
        /// </summary>
        public string prefix { get; set; }
        /// <summary>
        /// A list of tier levels that the Cheermote supports. Each tier identifies the range of Bits that you can cheer at that tier level and an image that graphically identifies the tier level.
        /// </summary>
        public List<Tier> tiers { get; set; }
        /// <summary>
        /// The type of Cheermote. Possible values are:
        /// <list type="bullet">
        /// <item>global_first_party — A Twitch-defined Cheermote that is shown in the Bits card.</item>
        /// <item>global_third_party — A Twitch-defined Cheermote that is not shown in the Bits card.</item>
        /// <item>channel_custom — A broadcaster-defined Cheermote.</item>
        /// <item>display_only — Do not use; for internal use only.</item>
        /// <item>sponsored — A sponsor-defined Cheermote. When used, the sponsor adds additional Bits to the amount that the user cheered. For example, if the user cheered Terminator100, the broadcaster might receive 110 Bits, which includes the sponsor's 10 Bits contribution.</item>
        /// </list>
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The order that the Cheermotes are shown in the Bits card. The numbers may not be consecutive. For example, the numbers may jump from 1 to 7 to 13. The order numbers are unique within a Cheermote type (for example, global_first_party) but may not be unique amongst all Cheermotes in the response.
        /// </summary>
        public int order { get; set; }
        /// <summary>
        /// The date and time, in RFC3339 format, when this Cheermote was last updated.
        /// </summary>
        public DateTime last_updated { get; set; }
        /// <summary>
        /// A Boolean value that indicates whether this Cheermote provides a charitable contribution match during charity campaigns.
        /// </summary>
        public bool is_charitable { get; set; }



        public class Tier
        {
            /// <summary>
            /// The minimum number of Bits that you must cheer at this tier level. The maximum number of Bits that you can cheer at this level is determined by the required minimum Bits of the next tier level minus 1. For example, if min_bits is 1 and min_bits for the next tier is 100, the Bits range for this tier level is 1 through 99. The minimum Bits value of the last tier is the maximum number of Bits you can cheer using this Cheermote. For example, 10000.
            /// </summary>
            public int min_bits { get; set; }
            /// <summary>
            /// The tier level. Possible tiers are: 1, 100, 500, 1000, 5000, 10000, 100000
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// The hex code of the color associated with this tier level (for example, #979797).
            /// </summary>
            public string color { get; set; }
            /// <summary>
            /// The animated and static image sets for the Cheermote.
            /// </summary>
            public Images images { get; set; }
            /// <summary>
            /// A Boolean value that determines whether users can cheer at this tier level.
            /// </summary>
            public bool can_cheer { get; set; }
            /// <summary>
            /// A Boolean value that determines whether this tier level is shown in the Bits card. Is true if this tier level is shown in the Bits card.
            /// </summary>
            public bool show_in_bits_card { get; set; }
        }
        public class Images
        {
            public Brightnes dark { get; set; }
            public Brightnes light { get; set; }
        }
        public class Brightnes
        {
            [JsonProperty("animated")]
            public URLs _animated { get; set; }
            [JsonProperty("static")]
            public URLs _static { get; set; }
        }
        public class URLs
        {
            [JsonProperty("1")]
            public string size_1x { get; set; }

            [JsonProperty("1.5")]
            public string size_1_5x { get; set; }

            [JsonProperty("2")]
            public string size_2x { get; set; }

            [JsonProperty("3")]
            public string size_3x { get; set; }

            [JsonProperty("4")]
            public string size_4x { get; set; }
        }
        #endregion
    }
}

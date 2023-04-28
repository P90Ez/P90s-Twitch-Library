using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using static P90Ez.Twitch.API.Background;

namespace P90Ez.Twitch.API.Endpoints
{
    /// <summary>
    /// <strong>Updates the color used for the user’s name in chat.</strong>
    /// </summary>
    public class UpdateUserChatColor : IStandardEndpoint
    {
        private static HttpMethod method = HttpMethod.Put;
        private static string EndpointURL = "https://api.twitch.tv/helix/chat/color";
        /// <summary>
        /// Requiered Scopes to use this endpoint.
        /// </summary>
        public static string RequieredScopes { get; } = "user:manage:chat_color";
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
        /// <strong>Updates the color used for the user’s name in chat.</strong>
        /// <para>Scope: <em>user:manage:chat_color</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-user-chat-color">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="color">The color to use for the user’s name in chat. All users are allowed to use the colors in the Colors enum. Only Turbo and Prime users are allowed to use Hex color code.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void WithStandardColor(Login.Credentials credentials, Colors color, out bool isSuccess, out int httpStatuscode)
        {
            Go(credentials, color.ToString(), out isSuccess, out httpStatuscode);
        }
        /// <summary>
        /// <strong>Updates the color used for the user’s name in chat.</strong>
        /// <para>Scope: <em>user:manage:chat_color</em></para>
        /// <para>TokenType: <em>User Access Token</em></para>
        /// <see href="https://dev.twitch.tv/docs/api/reference/#update-user-chat-color">Twitch Docs (Error Handling)</see> 
        /// </summary>
        /// <param name="credentials">Your generated login credentials</param>
        /// <param name="color">The Hex color to use for the user’s name in chat. Only Turbo and Prime users are allowed to use a Hex color code like #9146FF.</param>
        /// <param name="isSuccess">True if everything was successful</param>
        /// <param name="httpStatuscode">Contains a status code for error handling (-> 0 if no data was recieved from the server)</param>
        public static void WithHexColor(Login.Credentials credentials, string color, out bool isSuccess, out int httpStatuscode)
        {
            Go(credentials, UrlEncoder.Default.Encode(color), out isSuccess, out httpStatuscode);
        }

        private static void Go(Login.Credentials credentials, string color, out bool isSuccess, out int httpStatuscode)
        {
            httpStatuscode = 0;
            isSuccess = false;
            if (color == null || color == "")
                return;
            isSuccess = IStandardEndpoint.CheckCredentials(credentials, RequieredScopes, RequieredTokenType); //check if credentails are ok and fulfill requirements
            if (!isSuccess) //return if credential ceck failed
                return;

            var request = APICom.Request(out isSuccess, credentials, EndpointURL + $"?user_id={credentials.UserId}&color={color}", method); //make request

            if (request != null) //if request is not null, get status code (for later debugging)
                httpStatuscode = (int)request.StatusCode;
        }
        public enum Colors
        {
            blue,
            blue_violet,
            cadet_blue,
            chocolate,
            coral,
            dodger_blue,
            firebrick,
            golden_rod,
            green,
            hot_pink,
            orange_red,
            red,
            sea_green,
            spring_green,
            yellow_green
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch
{
    public partial class Login
    {
        /// <summary>
        /// Objects of this class contain login information used by api/irc/pubsub/...
        /// </summary>
        public class Credentials
        {
            /// <summary>
            /// Is True if Token is valid.
            /// </summary>
            [JsonIgnore]
            public bool IsSuccess { get; internal set; } = false;
            /// <summary>
            /// Contains error messages if something has gone wrong. (Report Bugs via Discord (P90Ez#9675) or via Mail office@p90ez.com - THANK YOU!)
            /// </summary>
            [JsonIgnore]
            public string ErrorMessage { get; internal set; } = "";
            /// <summary>
            /// Generated User/App Acces Token, empty if request failed
            /// </summary>
            [JsonIgnore]
            public string AuthToken { get; internal set; } = "";
            /// <summary>
            /// Only provided by Authorization code grant flow, empty if failed
            /// </summary>
            [JsonIgnore]
            public string RefreshToken { get; internal set; } = "";
            /// <summary>
            /// Your (Twitch) app's client id
            /// </summary>
            [JsonProperty("client_id")]
            public string ClientId { get; internal set; }
            /// <summary>
            /// Your (Twitch) app's client secret. Is ONLY used for automatically refreshing the token.
            /// </summary>
            [JsonProperty("client_secret")]
            internal string ClientSecret { get; set; }
            /// <summary>
            /// Only provided with a User Acces Token
            /// </summary>
            [JsonProperty("login")]
            public string UserLogin { get; internal set; }
            /// <summary>
            /// Only provided with a User Acces Token
            /// </summary>
            [JsonProperty("scopes")]
            public List<string> Scopes { get; internal set; }
            /// <summary>
            /// Only provided with a User Acces Token
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; internal set; }
            /// <summary>
            /// Seconds till the token expires.
            /// </summary>
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; internal set; }
            /// <summary>
            /// Date and time when the token expires. - Might be a few seconds off due to processing time.
            /// </summary>
            [JsonIgnore]
            public DateTime ExpirationDate { get 
                {
                    if (ValidationDate == null) return DateTime.Now;
                    return ValidationDate.AddSeconds(ExpiresIn); 
                } }
            /// <summary>
            /// DateTime from last token validation. - Might be a few seconds off due to processing time.
            /// </summary>
            [JsonIgnore]
            public DateTime ValidationDate;
            /// <summary>
            /// Type of token. Depends on the method used to obtain the token.
            /// </summary>
            [JsonIgnore]
            public TokenType TokenType { get; internal set; }
            /// <summary>
            /// Method used to generate Token.
            /// </summary>
            [JsonIgnore]
            internal TokenGenType TokenGenType { get; set; }

            /// <summary>
            /// DO NOT USE THIS CONSTRUCTOR - Constructor for Json Converter
            /// </summary>
            [JsonConstructor]
            internal Credentials() { ValidationDate = DateTime.Now; } //Constructor for Json converter
            /// <summary>
            /// DO NOT USE THIS CONSTRUCTOR - Constructor for error messages
            /// </summary>
            /// <param name="ErrorMessage"></param>
            internal Credentials(string ErrorMessage, ILogger Logger = null) { this.ErrorMessage = ErrorMessage; if (Logger != null) Logger.Log(ErrorMessage, ILogger.Severety.Critical); } //Constructor with errormessage


            /// <summary>
            /// Checks if Token is generated using provided scope.
            /// </summary>
            /// <param name="requieredScope"></param>
            /// <returns></returns>
            internal bool ContainsScope(string requieredScope)
            {
                if (requieredScope == "") return true;
                return Scopes.Contains(requieredScope.ToLower());
            }
            /// <summary>
            /// Checks if Token has the requiered TokenType.
            /// </summary>
            /// <param name="requieredType"></param>
            /// <returns></returns>
            internal bool IsCorrectTokenType(TokenType requieredType)
            {
                if (requieredType == Login.TokenType.Any)
                    return true;
                else
                    return TokenType == requieredType;
            }
        }
        public enum TokenType
        {
            /// <summary>
            /// Both, User Acces Tokens and App Acces Tokens are allowed.
            /// </summary>
            Any,
            /// <summary>
            /// Is an App Acces Token / Only App Access Tokens are allowed.
            /// </summary>
            AppAccessToken,
            /// <summary>
            /// Is an User Acces Token / Only User Acces Tokens are allowed (is the case as soon as scopes are requiered)
            /// </summary>
            UserAccessToken
        }
        internal enum TokenGenType
        {
            ImplicitGrantFlow,
            AuthorizationCodeFlow,
            ClientCredentialsGrantFlow,
        }
    }
}

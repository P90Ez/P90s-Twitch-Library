using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace P90Ez.Twitch
{
    /// <summary>
    /// <strong>Contains functions to generate/get Tokens.</strong>
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// Your app’s registered redirect URI (default: http://localhost:3000)
        /// </summary>
        public static string redirecturl = "http://localhost:3000";
        #region ImplicitGrantFlow
        /// <summary>
        /// <strong>This flow is meant for apps that don’t use a server, such as client-side apps or mobile apps.</strong>
        /// <para>Generated Token Type: <em>User Access Token</em></para>
        /// </summary>
        public static Credentials ImplicitGrantFlow(string ClientId, List<string> Scopes)
        {
            string state = stateGenerator();                                //generate a validation string to prevent CSRF attacks
            string url = "https://id.twitch.tv/oauth2/authorize" +          //generate URL with parameters - these parameters can be found in the documentation
                "?response_type=token" +
                $"&client_id={ClientId}" +
                $"&redirect_uri={redirecturl}" +
                $"&{GenerateScopes(Scopes)}" +
                $"&state={state}";
            OpenBrowser(url);                                               //Open URL in browser
            var paras = GetUrlParams(Listener(redirecturl));                //gets & parses url parameters after user authorization
            if (!ImplicitGrantFlow_CheckParas(paras, state)) return new Credentials("Para Check failed");       //check parameters
            var creds = ValidateToken(paras["access_token"], "Bearer");                                         //Checks if generated token is valid
            if (creds.isSuccess)
            {                          
                creds.isSuccess = true;
                creds.AuthToken = paras["access_token"];
                creds.tokenType = TokenType.UserAccessToken;
            }
            return creds;
        }
        /// <summary>
        /// Checks the parameter returned by the browser
        /// </summary>
        private static bool ImplicitGrantFlow_CheckParas(Dictionary<string, StringValues> paras, string state)
        {
            if (paras == null) return false;
            if (!paras.ContainsKey("state")) return false;
            if (paras["state"] != state) return false;
            if (!paras.ContainsKey("access_token") || paras["access_token"] == "") return false;
            if (!paras.ContainsKey("token_type") || paras["token_type"] == "") return false;
            return true;
        }
        #endregion
        #region AuthorizationCodeFlow
        /// <summary>
        /// (Front End Part) 
        /// <para><strong>Opens a browsertab, returns a "code" that can be used in your backend to get the AuthToken. - This flow is meant for apps that use a server, can securely store a client secret, and can make server-to-server requests to the Twitch API.</strong></para>
        /// <para>Generated Token Type: <em>User Access Token</em></para>
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="Scopes"></param>
        /// <returns></returns>
        public static string AuthorizationCodeFlow_FrontEnd(string ClientId, List<string> Scopes)
        {
            string state = stateGenerator();                                //generate a validation string to prevent CSRF attacks
            string url = "https://id.twitch.tv/oauth2/authorize" +          //generate URL with parameters - these parameters can be found in the documentation
                "?response_type=code" +
                $"&client_id={ClientId}" +
                $"&redirect_uri={redirecturl}" +
                $"&{GenerateScopes(Scopes)}" +
                $"&state={state}";
            OpenBrowser(url);                                                       //Open URL in browser
            var paras = GetUrlParams(Listener(redirecturl));                        //gets & parses url parameters after user authorization
            if (!AuthorizationCodeFlow_CheckParas(paras, state)) return null;       //check parameters - These parameters only contain a code. This code has to be used in the next step to get an acces token.
            return paras["code"];
        }

        /// <summary>
        /// (Back End Part) 
        /// <strong><para>Uses the code (from the <seealso cref="AuthorizationCodeFlow_FrontEnd(string, List{string})">Front End Part</seealso>) to get the AuthToken - This flow is meant for apps that use a server, can securely store a client secret, and can make server-to-server requests to the Twitch API.</para></strong>
        /// <para>Generated Token Type: <em>User Access Token</em></para>
        /// </summary>
        public static Credentials AuthorizationCodeFlow_BackEnd(string ClientId, string ClientSecret, string code)
        {
            //Create request body - parameters can be found in the documentation
            var application = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("client_id", ClientId), new KeyValuePair<string, string>("client_secret", ClientSecret), new KeyValuePair<string, string>("code", code), new KeyValuePair<string, string>("grant_type", "authorization_code"), new KeyValuePair<string, string>("redirect_uri", redirecturl) };
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", application, HttpMethod.Post);                     //Send request to optain the acces token
            if (!tokenresponse.IsSuccessStatusCode) return new Credentials("OAuth Generation Request Failed");                                      //Check if request is succesful
            var tempcreds = JsonConvert.DeserializeObject<AuthorizationCodeGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);      //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type);                                                                //Checks if generated token is valid
            if (creds.isSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.RefreshToken = tempcreds.refresh_token;
                creds.tokenType = TokenType.UserAccessToken;
            }
            return creds;
        }
        /// <summary>
        /// Checks the parameter returned by the browser
        /// </summary>
        private static bool AuthorizationCodeFlow_CheckParas(Dictionary<string, StringValues> paras, string state)
        {
            if (paras == null) return false;
            if (!paras.ContainsKey("state")) return false;
            if (paras["state"] != state) return false;
            if (paras.ContainsKey("error")) return false;
            if (!paras.ContainsKey("code") || paras["code"] == "") return false;
            return true;
        }
        private class AuthorizationCodeGrantFlow_Creds
        {
            public string access_token { get; set; }
            /// <summary>
            /// not provided when refreshing a token
            /// </summary>
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public List<string> scope { get; set; }
            public string token_type { get; set; }
        }
        #endregion
        #region ClientCredentialsGrantFlow
        /// <summary>
        /// <strong>The client credentials grant flow is meant only for server-to-server API requests.</strong>
        /// <para>Generated Token Type: <em>App Access Token</em></para>
        /// </summary>
        public static Credentials ClientCredentialsGrantFlow(string ClientId, string ClientSecret)
        {
            //Create request body - parameters can be found in the documentation
            var application = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("client_id", ClientId), new KeyValuePair<string, string>("client_secret", ClientSecret), new KeyValuePair<string, string>("grant_type", "client_credentials") };
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", application, HttpMethod.Post);                     //Send request
            if (!tokenresponse.IsSuccessStatusCode) return new Credentials("OAuth Generation Request Failed");                                      //Check if request is succesful
            var tempcreds = JsonConvert.DeserializeObject<ClientCredentialsGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);      //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type);                                                                //Checks if generated token is valid
            if (creds.isSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.tokenType = TokenType.AppAccessToken;
            }
            return creds;
        }
        private class ClientCredentialsGrantFlow_Creds
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }
        #endregion
        #region TokenValidation
        /// <summary>
        /// Checks if a token is still valid using a request to the Twitch server
        /// </summary>
        public static bool IsTokenValid(string token, string tokentype)
        {
            var valiresponse = ValidateTokenRequest(tokentype, token); //Validate token using a request to twitch server
            return CheckValidationRequest(valiresponse); //Check validation response
        }
        /// <summary>
        /// Checks if a token is still valid using a request to the Twitch server
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokentype">Only 'Bearer' and 'OAuth' are valid!</param>
        /// <returns></returns>
        public static Credentials ValidateToken(string token, string tokentype)
        {
            var valiresponse = ValidateTokenRequest(tokentype, token);                                          //Validate token using another request to twitch server
            if (!CheckValidationRequest(valiresponse)) return new Credentials("Token Validation Failed");       //Check validation response
            try
            {
                var creds = JsonConvert.DeserializeObject<Credentials>(valiresponse.Content.ReadAsStringAsync().Result);
                creds.isSuccess = true;
                return creds;
            }
            catch { return new Credentials("Credential Deserialization Failed"); }
        }
        #endregion
        #region Refresh Token
        /// <summary>
        /// Refreshes your acces token. Only works if AuthorizationCodeFlow was used to obtain your token.
        /// </summary>
        /// <param name="InCreds"></param>
        /// <param name="client_secret"></param>
        /// <returns></returns>
        public static Credentials RefreshToken(Credentials InCreds, string client_secret)
        {
            if (InCreds.RefreshToken == null || InCreds.RefreshToken == "") return new Credentials("Refresh Token is null or empty");           //Check if Refresh Token is provided. Can only be obtained by using AuthorizationCodeFlow.
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("grant_type", "refresh_token"), new KeyValuePair<string, string>("refresh_token", InCreds.RefreshToken), new KeyValuePair<string, string>("client_id", InCreds.client_id), new KeyValuePair<string, string>("client_secret", client_secret) }, HttpMethod.Post);
            if (!tokenresponse.IsSuccessStatusCode) return new Credentials("OAuth Generation Request Failed");                                  //Check if request is succesful
            var tempcreds = JsonConvert.DeserializeObject<AuthorizationCodeGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);  //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type);                                                            //Checks if generated token is valid
            if (creds.isSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.RefreshToken = tempcreds.refresh_token;
                creds.tokenType = TokenType.UserAccessToken;
            }
            return creds;
        }
        #endregion
    }
}

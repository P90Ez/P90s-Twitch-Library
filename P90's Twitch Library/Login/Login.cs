//#define Debug
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using P90Ez.Extensions;
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
        public static Credentials ImplicitGrantFlow(string ClientId, List<string> Scopes, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (ClientId == null || ClientId == "") throw new Exceptions.ArgumentNullException(nameof(ClientId), Logger);
    
            string state = stateGenerator();                                //generate a validation string to prevent CSRF attacks
            string url = "https://id.twitch.tv/oauth2/authorize" +          //generate URL with parameters - these parameters can be found in the documentation
                "?response_type=token" +
                $"&client_id={ClientId}" +
                $"&redirect_uri={redirecturl}" +
                $"&{GenerateScopes(Scopes)}" +
                $"&state={state}";
            var ListenerTask = Listener(redirecturl);                       //Start listener (http server)
            OpenBrowser(url);                                               //Open URL in browser
            var paras = GetUrlParams(ListenerTask.Result);                  //gets & parses url parameters after user authorization
            if (!ImplicitGrantFlow_CheckParas(paras, state))                //check parameters
            {
#if Debug
                Logger.Log($"ImplicitGrantFlow: ParaCheck failed! Paras: {paras.ToJsonString()}, State: {state}", ILogger.Severety.Critical);
#else
                Logger.Log("Parameter check failed! Recieved parameters did not match expected parameters!", ILogger.Severety.Critical);
#endif
                return new Credentials("Para Check failed", Logger);
            }
            var creds = ValidateToken(paras["access_token"], "Bearer", Logger);                                     //Checks if generated token is valid
            if (creds.IsSuccess)
            {                          
                creds.IsSuccess = true;
                creds.AuthToken = paras["access_token"];
                creds.TokenType = TokenType.UserAccessToken;
                creds.TokenGenType = TokenGenType.ImplicitGrantFlow;
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
        public static string AuthorizationCodeFlow_FrontEnd(string ClientId, List<string> Scopes, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (ClientId == null || ClientId == "") throw new Exceptions.ArgumentNullException(nameof(ClientId), Logger);

            string state = stateGenerator();                                //generate a validation string to prevent CSRF attacks
            string url = "https://id.twitch.tv/oauth2/authorize" +          //generate URL with parameters - these parameters can be found in the documentation
                "?response_type=code" +
                $"&client_id={ClientId}" +
                $"&redirect_uri={redirecturl}" +
                $"&{GenerateScopes(Scopes)}" +
                $"&state={state}";
            var ListenerTask = Listener(redirecturl);                       //Start listener (http server)
            OpenBrowser(url);                                               //Open URL in browser
            var paras = GetUrlParams(ListenerTask.Result);                  //gets & parses url parameters after user authorization
            if (!AuthorizationCodeFlow_CheckParas(paras, state))            //check parameters - These parameters only contain a code. This code has to be used in the next step to get an acces token.
            {
#if Debug
                Logger.Log($"AuthorizationCodeFlow: ParaCheck failed! Paras: {paras.ToJsonString()}, State: {state}", ILogger.Severety.Critical);
#else
                Logger.Log("Parameter check failed! Recieved parameters did not match expected parameters!", ILogger.Severety.Critical);
#endif
                return null;
            }
            return paras["code"];
        }

        /// <summary>
        /// (Back End Part) 
        /// <strong><para>Uses the code (from the <seealso cref="AuthorizationCodeFlow_FrontEnd(string, List{string}, ILogger)">Front End Part</seealso>) to get the AuthToken - This flow is meant for apps that use a server, can securely store a client secret, and can make server-to-server requests to the Twitch API.</para></strong>
        /// <para>Generated Token Type: <em>User Access Token</em></para>
        /// </summary>
        public static Credentials AuthorizationCodeFlow_BackEnd(string ClientId, string ClientSecret, string code, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (ClientId == null || ClientId == "") throw new Exceptions.ArgumentNullException(nameof(ClientId), Logger);
            if (ClientSecret == null || ClientSecret == "") throw new Exceptions.ArgumentNullException(nameof(ClientSecret), Logger);
            if (code == null || code == "") throw new Exceptions.ArgumentNullException(nameof(code), Logger);

            //Create request body - parameters can be found in the documentation
            var application = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("client_id", ClientId), new KeyValuePair<string, string>("client_secret", ClientSecret), new KeyValuePair<string, string>("code", code), new KeyValuePair<string, string>("grant_type", "authorization_code"), new KeyValuePair<string, string>("redirect_uri", redirecturl) };
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", application, HttpMethod.Post);                     //Send request to optain the acces token
            if (!tokenresponse.IsSuccessStatusCode) //Check if request is succesful
            {
#if Debug
                Logger.Log($"AuthorizationCodeFlow: Code {tokenresponse.StatusCode}, {tokenresponse.ReasonPhrase}", ILogger.Severety.Critical);
#else
                Logger.Log("OAuth generation request failed!", ILogger.Severety.Critical);
#endif
                return new Credentials("OAuth generation request failed", Logger);
            }
            var tempcreds = JsonConvert.DeserializeObject<AuthorizationCodeGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);      //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type, Logger);                                                        //Checks if generated token is valid
            if (creds.IsSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.RefreshToken = tempcreds.refresh_token;
                creds.ClientSecret = ClientSecret;
                creds.TokenType = TokenType.UserAccessToken;
                creds.TokenGenType = TokenGenType.AuthorizationCodeFlow;
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
        public static Credentials ClientCredentialsGrantFlow(string ClientId, string ClientSecret, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (ClientId == null || ClientId == "") throw new Exceptions.ArgumentNullException(nameof(ClientId), Logger);
            if (ClientSecret == null || ClientSecret == "") throw new Exceptions.ArgumentNullException(nameof(ClientSecret), Logger);

            //Create request body - parameters can be found in the documentation
            var application = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("client_id", ClientId), new KeyValuePair<string, string>("client_secret", ClientSecret), new KeyValuePair<string, string>("grant_type", "client_credentials") };
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", application, HttpMethod.Post);     //Send request
            if (!tokenresponse.IsSuccessStatusCode)                                                                                 //Check if request is succesful
            {
#if DEBUG
                Logger.Log($"ClientCredentialsGrantFlow: Code {tokenresponse.StatusCode}, {tokenresponse.ReasonPhrase}", ILogger.Severety.Critical);
#else
                Logger.Log("OAuth generation request failed!", ILogger.Severety.Critical);
#endif
                return new Credentials("OAuth generation request failed", Logger);
            }
            var tempcreds = JsonConvert.DeserializeObject<ClientCredentialsGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);      //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type, Logger);                                                        //Checks if generated token is valid
            if (creds.IsSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.TokenType = TokenType.AppAccessToken;
                creds.TokenGenType = TokenGenType.ClientCredentialsGrantFlow;
                creds.ClientSecret = ClientSecret;
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
        /// <param name="token"></param>
        /// <param name="tokentype">Only 'Bearer' and 'OAuth' are valid!</param>
        public static bool IsTokenValid(string token, string tokentype = "Bearer")
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
        private static Credentials ValidateToken(string token, string tokentype, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (token == null || token == "") throw new Exceptions.ArgumentNullException(nameof(token), Logger);

            var valiresponse = ValidateTokenRequest(tokentype, token);                                              //Validate token using another request to twitch server
            if (!CheckValidationRequest(valiresponse)) return new Credentials("Token Validation Failed", Logger);   //Check validation response
            try
            {
                var creds = JsonConvert.DeserializeObject<Credentials>(valiresponse.Content.ReadAsStringAsync().Result);
                creds.IsSuccess = true;
                return creds;
            }
            catch { return new Credentials("Credential Deserialization Failed", Logger); }
        }
        #endregion
        #region Refresh Token
        /// <summary>
        /// Refreshes your acces token. Only works if AuthorizationCodeFlow was used to obtain your token.
        /// </summary>
        /// <param name="InCreds"></param>
        /// <returns></returns>
        public static Credentials RefreshToken(Credentials InCreds, ILogger Logger = null)
        {
            if (Logger == null) Logger = new Logger();
            if (InCreds == null) throw new Exceptions.ArgumentNullException(nameof(InCreds), Logger);

            if (InCreds.RefreshToken == null || InCreds.RefreshToken == "") return new Credentials("Refresh Token is null or empty", Logger);   //Check if Refresh Token is provided. Can only be obtained by using AuthorizationCodeFlow.
            if (InCreds.ClientSecret == null || InCreds.ClientSecret == "") return new Credentials("Client Secret is null or empty", Logger);
            if (InCreds.ClientId == null || InCreds.ClientId == "") return new Credentials("Client Id is null or empty", Logger);
            
            var tokenresponse = HeaderlessURLEncodedRequest("https://id.twitch.tv/oauth2/token", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("grant_type", "refresh_token"), new KeyValuePair<string, string>("refresh_token", InCreds.RefreshToken), new KeyValuePair<string, string>("client_id", InCreds.ClientId), new KeyValuePair<string, string>("client_secret", InCreds.ClientSecret) }, HttpMethod.Post);
            if (!tokenresponse.IsSuccessStatusCode) //Check if request is succesful
            {
#if DEBUG
                Logger.Log($"RefreshToken: Code {tokenresponse.StatusCode}, {tokenresponse.ReasonPhrase}", ILogger.Severety.Critical);
#else
                Logger.Log("OAuth generation request failed!", ILogger.Severety.Critical);
#endif
                return new Credentials("OAuth generation request failed", Logger);
            }

            var tempcreds = JsonConvert.DeserializeObject<AuthorizationCodeGrantFlow_Creds>(tokenresponse.Content.ReadAsStringAsync().Result);  //Deserialize data
            var creds = ValidateToken(tempcreds.access_token, tempcreds.token_type, Logger);                                                    //Checks if generated token is valid
            if (creds.IsSuccess)
            {
                creds.AuthToken = tempcreds.access_token;
                creds.RefreshToken = tempcreds.refresh_token;
                creds.ClientSecret = InCreds.ClientSecret;
                creds.TokenType = TokenType.UserAccessToken;
                creds.TokenGenType = TokenGenType.AuthorizationCodeFlow;
            }
            return creds;
        }
        #endregion
        #region Load/Store
        /// <summary>
        /// Loads credentials from encrypted file, checks if token is still valid and refreshes it if necessary.
        /// </summary>
        /// <param name="Filename">Path to file where the credentials were stored.</param>
        /// <param name="Key">The key used to encrypt the file.</param>
        /// <param name="creds">Valid credentials or null if validation and refreshing failed.</param>
        /// <returns>True if credentials are valid. Otherwise false.</returns>
        public static bool LoadFromFile(string Filename, string Key, out Credentials creds, ILogger Logger = null)
        {
            creds = null;
            if(Filename == null || Filename == "") return false;
            
            if(Logger == null) Logger = new Logger();

            string decryptedFile = Decrypt(Filename, Key, Logger); //Read and decrypt file.
            if (decryptedFile == null || decryptedFile == "") return false; //Decryption failed.

            try
            {
                creds = JsonConvert.DeserializeObject<Credentials>(decryptedFile);
            } catch { return false; } //Deserialization or decryption failed.
            if (creds == null) return false; //Deserialization or decryption failed.

            if (!creds.IsSuccess) return false; //Token generation failed before saving.

            if(creds.ExpirationDate.Subtract(DateTime.Now).TotalSeconds <= 0) //Token is expired
            {
                if (creds.ClientSecret != null || creds.ClientSecret != "")
                {
                    if (creds.TokenGenType == TokenGenType.AuthorizationCodeFlow && creds.RefreshToken != null && creds.RefreshToken != "")
                    {
                        creds = RefreshToken(creds, Logger);
                    }
                    else if (creds.TokenGenType == TokenGenType.ClientCredentialsGrantFlow)
                    {
                        creds = ClientCredentialsGrantFlow(creds.ClientId, creds.ClientSecret, Logger);
                    }
                    else
                        return false; //Token is expired and cannot be refreshed.
                }
                else
                    return false; //Token is expired and cannot be refreshed.
            }
            else //Token is not expired
            {
                if (!IsTokenValid(creds.AuthToken)) return false; //Token validation failed.
            }

            return true; //Token is valid.
        }

        /// <summary>
        /// Stores encrypted credentials to a file.
        /// </summary>
        /// <param name="Filename">Path to store the file.</param>
        /// <param name="Key">Key to encrypt the file with. Use this key to decrypt the file again.</param>
        /// <param name="creds"></param>
        public static void StoreToFile(string Filename, string Key, Credentials creds, ILogger Logger = null)
        {
            if (Filename == null || Filename == "" || Key == null) return;

            if(Logger == null) Logger = new Logger();

            string data = creds.ToJsonString();

            Encrypt(Filename, data, Key, Logger);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using P90Ez.Extensions;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace P90Ez.Twitch
{
    partial class Login
    {

        #region important methods
        /// <summary>
        /// Generates a validation hexstring to prevent CSRF attacks (range: 0x1000 - 0x7HHHHHHH)
        /// </summary>
        /// <returns></returns>
        private static string stateGenerator()
        {
            Random rnd = new Random();
            string s = "";
            for (int i = 0; i < 4; i++) {
                s += rnd.Next(4096, int.MaxValue).ToString("x");
            }
            return s;
        }
        /// <summary>
        /// Creates a HttpListener and returns the redirected url
        /// </summary>
        /// <param name="redirecturl"></param>
        /// <returns>returns the full redirected url</returns>
        private static string Listener(string redirecturl)
        {
            if (redirecturl == null) return null;
            if (redirecturl.Length == 0) return null;
            if (redirecturl[redirecturl.Length - 1] != '/') //Check if last Char is '/', if not add '/' (listener won't work if the last char is not '/')
                redirecturl += '/';
            HttpListener server = new HttpListener(); //Create server
            server.Prefixes.Add(redirecturl); //Bind url to server
            server.Start(); //Start server
            do
            {
                HttpListenerContext context = server.GetContext(); //Get the request
                if (context.Request.Url.ToString() == redirecturl) //Thanks twitch, this is just pain.
                                                                   //This next line sends a response with a script, to make another request with the URI fragment parsed to a proper "readable" url (URI fragments are usualy not part of a request, so we need another way to get to our information)
                    SendHttpResponse("<!DOCTYPE html>\r\n<html>\r\n<body onload=\"const Http = new XMLHttpRequest(); Http.open('GET','" + redirecturl + "?' + window.location.hash.substring(1)); Http.send(); Http.onreadystatechange = function(){if(this.readyState==4 && this.status == 200) {window.close();}}\">\r\n</html>", context.Response); //there could be a more elegant way to do this
                else
                {
                    SendHttpResponse("<body onload=\"window.close();\">", context.Response); //Send response to close browser tab
                    return context.Request.Url.ToString(); //returns url with params
                }
            } while (true);
        }

        /// <summary>
        /// Sends a message to a HttpRequest
        /// </summary>
        private static void SendHttpResponse(string message, HttpListenerResponse response)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
        /// <summary>
        /// Deserializes an URL
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, StringValues> GetUrlParams(string url)
        {
            string content;
            if (url.Contains("/?"))
                content = url.Split("/?")[1];
            else return null;
            using var reader = new FormReader(content);
            return reader.ReadForm();
        }
        /// <summary>
        /// Opens a browser with an url. (Can be simplified in non .net core apps with Process.Start("URL");)
        /// </summary>
        /// <param name="url"></param>
        private static void OpenBrowser(string url)
        {
            Process browser = new Process();
            browser.StartInfo.UseShellExecute = true;
            browser.StartInfo.FileName = url;
            browser.Start();
        }
        /// <summary>
        /// Checks the ValidateToken request.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool CheckValidationRequest(HttpResponseMessage message)
        {
            if (message == null) return false;
            if (!message.IsSuccessStatusCode) return false;
            try
            {
                var valifailed = JsonConvert.DeserializeObject<ValidationFailed>(message.Content.ReadAsStringAsync().Result);
                if (valifailed.status != 200 && valifailed.status != 0) return false;
            }
            catch { }
            return true;
        }
        /// <summary>
        /// Takes a list of scopes and returns a URL encoded string containing scopes.
        /// </summary>
        /// <param name="Scopes"></param>
        /// <returns></returns>
        private static string GenerateScopes(List<string> Scopes)
        {
            return new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("scope", String.Join(" ", Scopes.ToArray()))
                }).ReadAsStringAsync().Result;
        }

        #region File Encryption / Decryption
        private static int EncryptionKeySize = 256;
        private static int EncryptionBlockSize = 128;
        /// <summary>
        /// Decrypts a file while reading. - uses AES256 to decrypt data
        /// </summary>
        /// <returns>Decrypted file as string, Empty string if decryption has failed.</returns>
        private static string Decrypt(string Filename, string Password, ILogger Logger)
        {
            if (Filename == null || Filename == "" || Password == null) return "";
            if (!File.Exists(Filename)) return "";
            string decryptedFile = "";

            //generating key and initialization vector from password
            Rfc2898DeriveBytes Key = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes("keykeykeykey"), 1000);
            Rfc2898DeriveBytes IV = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes("iviviviviviv"), 1000);

            try
            {
                byte[] fromFile = File.ReadAllBytes(Filename);
                using (MemoryStream memstream = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        //set key and iv
                        aes.KeySize = EncryptionKeySize;
                        aes.Key = Key.GetBytes(EncryptionKeySize / 8);
                        aes.BlockSize = EncryptionBlockSize;
                        aes.IV = IV.GetBytes(EncryptionBlockSize / 8);

                        using (CryptoStream cryptoStream = new CryptoStream(
                            memstream,
                            aes.CreateDecryptor(),
                            CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(fromFile, 0, fromFile.Length); //decrypting
                        }
                    }
                    decryptedFile = new UTF8Encoding(false).GetString(memstream.ToArray()); //convert decrypted bytes to string
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"The decryption failed. {ex.Message}", ILogger.Severety.Warning);
            }

            return decryptedFile;
        }
        /// <summary>
        /// Encrypts a file while writing the provided data. - uses AES256 to encrypt data
        /// </summary>
        private static void Encrypt(string Filename, string Data, string Password, ILogger Logger)
        {
            if (Filename == null || Filename == "" || Data == null || Data == "" || Password == null) return;

            //generating key and initialization vector from password
            Rfc2898DeriveBytes Key = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes("keykeykeykey"), 1000);
            Rfc2898DeriveBytes IV = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes("iviviviviviv"), 1000);

            try
            {
                using (MemoryStream memstream = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        //set key and iv
                        aes.KeySize = EncryptionKeySize;
                        aes.Key = Key.GetBytes(EncryptionKeySize / 8);
                        aes.BlockSize = EncryptionBlockSize;
                        aes.IV = IV.GetBytes(EncryptionBlockSize / 8);

                        using (CryptoStream cryptoStream = new CryptoStream(
                            memstream,
                            aes.CreateEncryptor(),
                            CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new StreamWriter(cryptoStream))
                            {
                                encryptWriter.WriteLine(Data); //encrypting
                            }
                        }
                    }
                    File.WriteAllBytes(Filename, memstream.ToArray()); //writing encrypted bytes to file
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"The encryption failed. {ex.Message}", ILogger.Severety.Warning);
            }
        }
        #endregion
        #endregion
        #region HttpRequests
        /// <summary>
        /// HTTP request to the Twitch servers to check if a token is valid.
        /// </summary>
        /// <param name="tokentype"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static HttpResponseMessage ValidateTokenRequest(string tokentype, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"{tokentype.FirstCharToUpper()} {token}");
                    var request = new HttpRequestMessage //Anfrage erstellen
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("https://id.twitch.tv/oauth2/validate"),
                        Content = new StringContent("")
                    };
                    return client.SendAsync(request).Result; //Anfrage senden und Antwort abwarten
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); //Wenn fehler -> in Console ausgeben lassen
            }
            return null;
        }
        /// <summary>
        /// HTTP request to the Twitch servers. Used by AuthorizationCodeFlow, ClientCredentialsGrantFlow and RefreshToken to get tokens.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="application"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static HttpResponseMessage HeaderlessURLEncodedRequest(string url, List<KeyValuePair<string, string>> application, HttpMethod method)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage //Anfrage erstellen
                    {
                        Method = method,
                        RequestUri = new Uri(url),
                        Content = new FormUrlEncodedContent(application)
                    };
                    return client.SendAsync(request).Result; //Anfrage senden und Antwort abwarten
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); //Wenn fehler -> in Console ausgeben lassen
            }
            return null;
        }
        #endregion
        /// <summary>
        /// Used by CheckValidationRequest to determine if the token is valid
        /// </summary>
        private class ValidationFailed
        {
            public int status { get; set; }
            public string message { get; set; }
        }

    }
}

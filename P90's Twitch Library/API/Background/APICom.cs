using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.Login;

namespace P90Ez.Twitch.API
{
    public partial class Background
    {
        public class APICom
        {
            /// <summary>
            /// HTTP Request with credentials and application type 'application/json'
            /// </summary>
            /// <param name="isSuccessful"></param>
            /// <param name="creds">Credentials object</param>
            /// <param name="url">endpoint url</param>
            /// <param name="method"></param>
            /// <param name="application"></param>
            /// <returns>returns an object of HttpResponseMessage OR null when failed, check isSuccessful</returns>
            /// <exception cref="Exception"></exception>
            public static HttpResponseMessage Request(out bool isSuccessful, Credentials creds, string url, HttpMethod method, string application = "")
            {
                if (creds == null || !creds.isSuccess) throw new Exception("Credentials error.");
                if (url == null|| url == "") throw new Exception("URL is empty");
                if (method == null) throw new Exception("No HTTP Method defined");
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {creds.AuthToken}"); //Autorisierung
                        client.DefaultRequestHeaders.Add("Client-Id", creds.client_id); //Autorisierung
                        var request = new HttpRequestMessage
                        {
                            Method = method, //Art der Anfrage festlegen
                            RequestUri = new Uri(url), //URI, bzw. URL festlegen
                            Content = new StringContent(application, Encoding.UTF8, "application/json") //Content (bzw. Body) der Anfrage festlegen
                        };
                        var response = client.SendAsync(request).Result; //Antwort abwarten & zwischenspeichern
                        isSuccessful = IsRequestSuccessful(response);
                        return response;
                    }
                }
                catch //(Exception ex)
                {
                    isSuccessful = false;
                    //Console.WriteLine(ex.Message);
                }
                return null; //Gibt null zurück wenn die Anfrage fehlschlägt
            }
            /// <summary>
            /// Checks if the request is Successful
            /// </summary>
            /// <param name="responseMessage"></param>
            /// <returns></returns>
            private static bool IsRequestSuccessful(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null) return false;
                if (!responseMessage.IsSuccessStatusCode) return false;
                return true;
            }
        }
    }
    
}

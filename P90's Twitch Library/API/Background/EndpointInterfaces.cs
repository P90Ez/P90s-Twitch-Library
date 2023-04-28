using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using static P90Ez.Twitch.Login;

namespace P90Ez.Twitch.API
{
    public partial class Background
    {
        #region Chache Requests Interface
        /// <summary>
        /// "Interface" for endpoint classes equiped with a cache function
        /// </summary>
        public interface ICacheRequest
        {
            /// <summary>
            /// Key: input paras, Value: CacheClass containing the cached object and the timestamp
            /// </summary>
            private static Dictionary<string, CacheStruct> cache = new Dictionary<string, CacheStruct>();
            /// <summary>
            /// Cache lifecycle in seconds
            /// </summary>
            private static int CacheTime = 30;
            /// <summary>
            /// "HTTP"-statuscode for an object from cache (= -200)
            /// </summary>
            internal static readonly int CacheStatusCode = -200;

            /// <summary>
            /// Trys to get a cached object. Checks if cache has reached it's end of life (and then removes it from cache).
            /// </summary>
            /// <param name="paras"></param>
            /// <param name="isValid">if paras match a cached item and item is still valid</param>
            /// <returns></returns>
            internal static object TryGetCachedObject(string paras, out bool isValid)
            {
                isValid = false;
                if (!cache.ContainsKey(paras)) //Prüfen, ob Eintrag existiert
                    return null;
                var cobj = cache[paras];
                if (cobj == null || cobj.CacheDate == null) //Prüfen, ob Chache Zeitpunkt nicht null
                {
                    RemoveCachedObject(paras); //Wenn null, Eintrag entfernen
                    return null;
                }
                if (DateTime.Now.Subtract(cobj.CacheDate).TotalSeconds >= CacheTime) //Zeit im Cache prüfen
                {
                    RemoveCachedObject(paras); //Wenn überschritten, Eintrag entfernen
                    return null;
                }
                isValid = true;
                return cobj.CachedObject;
            }
            internal static void RemoveCachedObject(string paras)
            {
                if (!cache.ContainsKey(paras))
                    return;
                cache.Remove(paras);
            }
            internal static void AddToCache(string paras, object obj)
            {
                cache[paras] = new CacheStruct(obj);
            }
            //Cache class
            public class CacheStruct
            {
                public object CachedObject;
                public DateTime CacheDate;
                public CacheStruct(object cachedObject)
                {
                    CachedObject = cachedObject;
                    CacheDate = DateTime.Now;
                }
            }
        }

        #endregion
        #region Standard Endpoint Interface
        public interface IStandardEndpoint //not realy an interface, more like a "class" with standard methods. Might fix in the future
        {
            #pragma warning disable 0169 //"ahh shitty code"... yeah i know. These fields only exist to copy them to the actual endpoint class (may remove in the future)
            private static HttpMethod method;
            #pragma warning disable 0169
            private static string EndpointURL;
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
            /// Converts this object to a Json String.
            /// </summary>
            /// <returns>Json String of this object</returns>
            public string ToString(bool FormattingIntended = false)
            {
                if (!FormattingIntended)
                    return JsonConvert.SerializeObject(this);
                else
                    return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            /// <summary>
            /// Checks if the credentials are the required type &amp; if token contains required scopes
            /// </summary>
            /// <param name="creds">Credentials to check, null is not a problem.</param>
            /// <param name="RequieredScopes"></param>
            /// <param name="RequieredTokenType"></param>
            /// <returns></returns>
            static internal bool CheckCredentials(Login.Credentials creds, string RequieredScopes, TokenType RequieredTokenType)
            {
                if (creds == null)
                    return false;
                if (!creds.IsSuccess)
                    return false;
                return creds.ContainsScope(RequieredScopes) && creds.IsCorrectTokenType(RequieredTokenType);
            }
            /// <summary>
            /// Builds a string used for caching
            /// </summary>
            /// <param name="channelID"></param>
            /// <param name="queryParams"></param>
            /// <returns></returns>
            internal static string InputParasBuilder(string channelID, object queryParams)
            {
                return channelID + ":" + JsonConvert.SerializeObject(queryParams).ToLower();
            }
        }
        #endregion
    }
}

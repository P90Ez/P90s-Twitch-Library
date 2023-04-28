using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace P90Ez.Twitch.API
{
    public partial class Background
    {
        /// <summary>
        /// "data": [{},{},...]
        /// </summary>
        public class JsonDataArray
        {

            [JsonProperty]
            public object[] data;
            public bool IsDataNotNull()
            {
                return data != null;
            }
        }
        /// <summary>
        /// "data": [{},{},...],
        /// "pagination": { "cursor": "..." }
        /// </summary>
        public class JsonPagination : JsonDataArray
        {

            [JsonProperty]
            internal JsonPaginationSub pagination { get; set; }
            /// <summary>
            /// 'after' parameter to get the next list of items
            /// </summary>
            /// <returns></returns>
            public string NextPage()
            {
                if (pagination == null)
                    return "";
                return pagination.cursor;
            }
            /// <summary>
            /// IGNORE - Sub Class for Pagination Deserilization
            /// </summary>
            internal class JsonPaginationSub
            {
                [JsonProperty]
                public string cursor { get; internal set; }
            }
        }
        /// <summary>
        /// Class/Interface for Json-Applications &amp; Query Parameter.
        /// <para/><strong>Note:</strong> use nullable int (-> int?) for query parameters!
        /// </summary>
        public class JsonApplication
        {
            private string objectstring = null; //used to only build the string one -> better performance
            /// <summary>
            /// Converts this application to a Json String.
            /// <para><strong>NOTE:</strong> this string is only built ONCE. Use <seealso cref="ToString(bool)"/> (&amp; pass true) to force rebuild the Json String!</para>
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if(objectstring == null)
                    objectstring = JsonConvert.SerializeObject(this, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                return objectstring;
            }
            /// <summary>
            /// Converts this application to a Json String. Pass true to force rebuild the Json String.
            /// </summary>
            /// <param name="forcerebuild"></param>
            /// <returns></returns>
            public string ToString(bool forcerebuild)
            {
                if (forcerebuild)
                    objectstring = null;
                return ToString();
            }
            private string dataobjectstring = null;
            /// <summary>
            /// Converts this application to a Json String containing the data in a separate object called data. Pass true to force rebuild the Json String.
            /// <para/>Looks like this: {"data": {...}}
            /// </summary>
            /// <param name="forcerebuild"></param>
            /// <returns></returns>
            public string ToDataObjectString(bool forcerebuild = false)
            {
                if (dataobjectstring == null || forcerebuild)
                {
                    var tmp = new { data = this };
                    dataobjectstring = JsonConvert.SerializeObject(tmp, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                }
                return dataobjectstring;
            }
            /// <summary>
            /// Converts this application to query parameters. (format: '?key1=value1&amp;key2=value2&amp;...)
            /// </summary>
            /// <returns></returns>
            public string ToQueryParameters()
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(ToString());
                if (dictionary == null)
                    return "";
                string _out = "";
                foreach(var kvp in dictionary)
                {
                    
                    if(kvp.Value != null)
                    {
                        if (_out == "")
                            _out = "?";
                        else
                            _out += "&";

                        var valuetype = kvp.Value.GetType();
                        if (valuetype.FullName == "Newtonsoft.Json.Linq.JArray")
                        {
                            //+		object.GetType returned	{Name = "JArray" FullName = "Newtonsoft.Json.Linq.JArray"}	System.RuntimeType
                            foreach (var val in (JArray)kvp.Value) //if object is an array, add every item seperatly
                            {
                                if (_out[_out.Length - 1] != '&')
                                    _out += '&';
                                _out += $"{kvp.Key}={val}";
                            }
                        }else
                            _out += $"{kvp.Key}={kvp.Value}";
                    }
                }
                return _out;
            }
        }
        /// <summary>
        /// (used in (parent)object) The reporting window’s start and end dates, in RFC3339 format.
        /// </summary>
        public class Response_date_range
        {
            /// <summary>
            /// The reporting window’s start date.
            /// </summary>
            /// 
            [JsonProperty]
            public static string started_at { get; internal set; }
            /// <summary>
            /// The reporting window’s end date.
            /// </summary>
            /// 
            [JsonProperty]
            public static string ended_at { get; internal set; }
        }

    }
    
}

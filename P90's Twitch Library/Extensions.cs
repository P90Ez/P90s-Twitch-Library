using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace P90Ez.Extensions
{
    public static class HttpMessageExtensions
    {
        /// <summary>
        /// Gets the body of the response as a string
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static string BodyToString(this HttpResponseMessage responseMessage)
        {
            return responseMessage.Content.ReadAsStringAsync().Result;
        }
        /// <summary>
        /// Converts the body of the response to an object.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object BodyToClassObject(this HttpResponseMessage responseMessage, Type type)
        {
            return JsonConvert.DeserializeObject(responseMessage.BodyToString(), type);
        }
    }
    public static class StringExtensions
    {
        /// <summary>
        /// Converts an array of value to a request query parameter string.
        /// </summary>
        /// <param name="value">String Array with values</param>
        /// <param name="parameterName">The name of the parameter</param>
        /// <returns></returns>
        public static string ToQueryParams(this string[] value, string parameterName)
        {
            if (value == null | value.Length == 0) return "";
            string rtrn = "?";
            foreach (string item in value)
            {
                if (rtrn[rtrn.Length - 1] != '?')
                    rtrn += "&";
                rtrn += parameterName + '=' + item;
            }
            return rtrn;
        }
    }
}

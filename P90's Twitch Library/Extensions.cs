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
        public static object BodyToClassObject(this HttpResponseMessage responseMessage, Type type)
        {
            return JsonConvert.DeserializeObject(responseMessage.BodyToString(), type);
        }
        /// <summary>
        /// Converts the body of the response to an object.
        /// </summary>
        public static T BodyToClassObject<T>(this HttpResponseMessage responseMessage)
        {
            return JsonConvert.DeserializeObject<T>(responseMessage.BodyToString());
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

        /// <summary>
        /// Converts the first char of the input string to uppercase.
        /// </summary>
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

    }
    public static class KeyValuePairExtensions
    {
        /// <summary>
        /// Generates a Json string, using the keys as variable names.
        /// </summary>
        public static string ToJsonString(this List<KeyValuePair<string,string>> value)
        {
            string _out = "{";
            foreach(var v in value)
            {
                if(_out != "{")
                    _out += ",";
                _out += $"\"{v.Key}\":\"{v.Value}\"";
            }
            _out += "}";
            return _out;
        }
        /// <summary>
        /// Generates a Json object, using the keys as variable names.
        /// </summary>
        public static object ToObject(this List<KeyValuePair<string, string>> value)
        {
            return JsonConvert.DeserializeObject(value.ToJsonString());
        }
        /// <summary>
        /// Generates a Json string, using the key as variable name.
        /// </summary>
        public static string ToJsonString(this KeyValuePair<string, string> value)
        {
            return "{\"" + value.Key + ":\"" + value.Value + "\"}";
        }
    }
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts an object to an object of the right type, using JsonConvert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ToObject<T>(this object obj)
        {
            return JsonConvert.DeserializeObject<T>(obj.ToJsonString());
        }
        /// <summary>
        /// Converts this object to a Json string using JsonConvert.
        /// </summary>
        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}

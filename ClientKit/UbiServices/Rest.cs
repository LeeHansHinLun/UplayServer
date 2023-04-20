﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ClientKit.UbiServices
{
    public class Rest
    {
        #region JObject
        public static JObject? Put(RestClient client, RestRequest request)
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.PutAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JObject.Parse(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }

        public static JObject? Post(RestClient client, RestRequest request)
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.PostAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JObject.Parse(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }

        public static JObject? Get(RestClient client, RestRequest request)
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.GetAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JObject.Parse(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }
        #endregion
        #region T Class
        public static T? Put<T>(RestClient client, RestRequest request) where T : class
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.PutAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }

        public static T? Post<T>(RestClient client, RestRequest request) where T : class
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.PostAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }

        public static T? Get<T>(RestClient client, RestRequest request) where T : class
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.GetAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }
        #endregion
        #region StatusCode
        public static HttpStatusCode? Delete(RestClient client, RestRequest request)
        {
            try
            {
                RestResponse response = client.DeleteAsync(request).Result;
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }
        #endregion
        #region String
        public static string? GetString(RestClient client, RestRequest request)
        {
            try
            {
                Debug.WriteDebug(JsonConvert.SerializeObject(request));
                RestResponse response = client.GetAsync(request).Result;
                Debug.WriteDebug(JsonConvert.SerializeObject(response));
                if (response.Content != null)
                {
                    Console.WriteLine(response.StatusCode);
                    return response.Content;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                InternalEx.WriteEx(ex);
                return null;
            }
        }
        #endregion
    }
}

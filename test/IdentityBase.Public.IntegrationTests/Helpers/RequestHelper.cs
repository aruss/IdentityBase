using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace IdentityBase.Public.IntegrationTests
{
    public static class RequestHelper
    {
        public static HttpRequestMessage CreatePostRequest(String path, Dictionary<string, string> formPostBodyData)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new FormUrlEncodedContent(ToFormPostData(formPostBodyData))
            };

            return httpRequestMessage;
        }

        public static List<KeyValuePair<string, string>> ToFormPostData(Dictionary<string, string> formPostBodyData)
        {
            var result = new List<KeyValuePair<string, string>>();
            formPostBodyData.Keys.ToList().ForEach(key =>
            {
                result.Add(new KeyValuePair<string, string>(key, formPostBodyData[key]));
            });

            return result;
        }

        public static HttpRequestMessage CreatePostRequest(this HttpResponseMessage response, string path, Dictionary<string, string> formPostBodyData)
        {
            var httpRequestMessage = CreatePostRequest(path, formPostBodyData);
            return CookiesHelper.CopyCookiesFromResponse(httpRequestMessage, response);
        }

        public static HttpRequestMessage CreateGetRequest(this HttpResponseMessage response, string path)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            return CookiesHelper.CopyCookiesFromResponse(httpRequestMessage, response);
        }
    }
}
namespace ServiceBase.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// <see cref="HttpRequestMessage"/> extenion methods.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Add cookie to request message.
        /// </summary>
        /// <param name="request">Instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="cookies">Cookies</param>
        /// <returns>Instance of <see cref="HttpRequestMessage"/></returns>
        public static HttpRequestMessage AddCookies(
            this HttpRequestMessage request,
            IDictionary<string, string> cookies)
        {
            cookies.Keys.ToList().ForEach(key =>
            {
                request.Headers.Add(
                    "Cookie",
                    new CookieHeaderValue(key, cookies[key]).ToString()
                );
            });

            return request;
        }

        /// <summary>
        /// Pass cookies from response to request. 
        /// </summary>
        /// <param name="request">Instance of <see cref="HttpRequestMessage"/>
        /// </param>
        /// <param name="response">Instance of
        /// <see cref="HttpResponseMessage"/></param>
        /// <returns>Instance of <see cref="HttpRequestMessage"/></returns>
        public static HttpRequestMessage AddCookiesFromResponse(
            this HttpRequestMessage request,
            HttpResponseMessage response)
        {
            return request.AddCookies(response.ExtractCookies());
        }
    }
}


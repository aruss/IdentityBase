namespace ServiceBase.Tests
{
    using Microsoft.Net.Http.Headers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    /// <summary>
    /// <see cref="HttpResponseMessage"/> extension methods.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response">Instance of
        /// <see cref="HttpResponseMessage"/>.</param>
        /// <returns></returns>
        public static IDictionary<string, string> ExtractCookies(
            this HttpResponseMessage response)
        {
            var result = new Dictionary<string, string>();

            if (response.Headers
                .TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                SetCookieHeaderValue
                    .ParseList(values.ToList())
                    .ToList()
                    .ForEach(cookie =>
                    {
                        result.Add(
                            cookie.Name.ToString(),
                            cookie.Value.ToString()
                        );
                    });
            }

            return result;
        }
    }
}
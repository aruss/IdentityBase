namespace ServiceBase.Tests
{
    using Microsoft.Net.Http.Headers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public static class HttpResponseMessageExtensions
    {
        // Inspired from:
        // https://github.com/aspnet/Mvc/blob/538cd9c19121f8d3171cbfddd5d842cbb756df3e/test/Microsoft.AspNet.Mvc.FunctionalTests/TempDataTest.cs#L201-L202

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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdentityBase.Public.IntegrationTests
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PutJsonAsync(
            this HttpClient client, string requestUri, object obj = null)
        {
            string json; 
            if (obj == null)
            {
                json = "{}";
            }
            else
            {
                // Serialize object
                throw new NotImplementedException(); 
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PutAsync("/api/invitations", content);
        }

        public static async Task<HttpResponseMessage> PostFormAsync(
            this HttpClient client, string requestUri,
            IEnumerable<KeyValuePair<string, string>> form = null,
            HttpResponseMessage previousRequest = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            if (form != null)
            {
                request.Content = new FormUrlEncodedContent(form);
            }

            if (previousRequest != null)
            {
                request.AddCookiesFromResponse(previousRequest);
            }

            return await client.SendAsync(request); 
        }

        public static async Task<HttpResponseMessage> GetAsync(
              this HttpClient client, string requestUri,
              HttpResponseMessage previousRequest = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            
            if (previousRequest != null)
            {
                request.AddCookiesFromResponse(previousRequest);
            }

            return await client.SendAsync(request);
        }
    }
}
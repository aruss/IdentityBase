namespace ServiceBase.Tests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using ServiceBase.Json;

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PutJsonAsync(
            this HttpClient client,
            string requestUri,
            object obj = null)
        {
            string json;
            if (obj == null)
            {
                json = "{}";
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                    JsonSerializerSettingsExtensions.CreateWithDefaults());
            }

            StringContent content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            return await client.PutAsync(requestUri, content);
        }

        public static async Task<HttpResponseMessage> PostAsync(
            this HttpClient client,
            string requestUri,
            IEnumerable<KeyValuePair<string, string>> form = null,
            HttpResponseMessage prevResponse = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

            if (form != null)
            {
                request.Content = new FormUrlEncodedContent(form);
            }

            if (prevResponse != null)
            {
                request.AddCookiesFromResponse(prevResponse);
            }

            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> GetAsync(
              this HttpClient client,
              string requestUri,
              HttpResponseMessage prevResponse = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

            if (prevResponse != null)
            {
                request.AddCookiesFromResponse(prevResponse);
            }

            return await client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> FollowRedirect(
            this HttpClient client,
            HttpResponseMessage prevResponse)
        {
            string requestUri = prevResponse.Headers.Location.ToString();
            return await client.GetAsync(requestUri, prevResponse);
        }
    }
}
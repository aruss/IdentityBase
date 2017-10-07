namespace AspNetCoreWeb
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PutJsonAsync(
            this HttpClient client,
            string requestUrl,
            object model)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json"
            );

            return await client.PutAsync(requestUrl, content);
        }
    }
}

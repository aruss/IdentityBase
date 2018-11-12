namespace AspNetCoreApi
{
    public class ApplicationOptions
    {
        public string ApiName { get; set; } = "api1";
        public string ApiSecret { get; set; } = "secret";
        public string Authority { get; set; } = "http://localhost:5000";
    }
}

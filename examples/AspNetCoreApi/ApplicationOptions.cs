namespace AspNetCoreApi
{
    public class ApplicationOptions
    {
        public string ApiSecret { get; set; } = "secret";
        public string Authority { get; set; } = "http://localhost:5000";
    }
}

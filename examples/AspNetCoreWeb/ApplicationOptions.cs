using System.Runtime.Serialization;

namespace AspNetCoreWeb
{
    public class ApplicationOptions
    {
        public string ClientId { get; set; } = "mvc";
        public string ClientSecret { get; set; } = "secret";
        public string Authority { get; set; } = "http://localhost:5000";
    }
}

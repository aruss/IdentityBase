namespace IdentityBase
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    // Workarround, since I dont really know how to pass custom parameters
    // to Autofac.Module Load method 
    public static class Current
    {
        public static IConfiguration Configuration { get; set; }
        public static ILogger Logger { get; set; }
        public static IContainer Container { get; set; }
    }
}

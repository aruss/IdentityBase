namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Events;

    public class DefaultEventModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("Events")
                .Get<EventOptions>() ?? new EventOptions());

            services.AddScoped<IEventService, DefaultEventService>();
            services.AddScoped<IEventSink, DefaultEventSink>();
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}

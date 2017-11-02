
namespace ServiceBase.Modules
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public interface IModule
    {
        void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration);

        void Configure(IApplicationBuilder app);
    }
}

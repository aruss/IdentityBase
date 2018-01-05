
// Code from https://www.strathweb.com/2017/04/running-multiple-independent-asp-net-core-pipelines-side-by-side-in-the-same-application/

namespace IdentityBase.Extensions
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static partial class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder MapStartup /*<TStartup>*/(
            this IApplicationBuilder app,
            PathString path,
            IHostingEnvironment environment,
            IConfiguration configuration,
            Action<IServiceCollection> servicesConfiguration,
            Action<IApplicationBuilder> appBuilderConfiguration)
            // where TStartup : class
        {
            /*var webHost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(environment.ContentRootPath)
                .UseConfiguration(configuration)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(hostingContext.Configuration);
                })
                .UseStartup<TStartup>()
                .Build();*/

            IWebHost webHost = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(environment.ContentRootPath)
                .UseConfiguration(configuration)
                .ConfigureServices(servicesConfiguration)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(hostingContext.Configuration);
                })
                .UseStartup<EmptyStartup>()
                .Build();
            
            IServiceProvider serviceProvider = webHost.Services;
            IFeatureCollection serverFeatures = webHost.ServerFeatures;

            IApplicationBuilderFactory appBuilderFactory = serviceProvider
                .GetRequiredService<IApplicationBuilderFactory>();

            IApplicationBuilder branchBuilder = appBuilderFactory
                .CreateBuilder(serverFeatures);

            IServiceScopeFactory factory = serviceProvider
                .GetRequiredService<IServiceScopeFactory>();

            branchBuilder.Use(async (context, next) =>
            {
                using (var scope = factory.CreateScope())
                {
                    context.RequestServices = scope.ServiceProvider;
                    await next();
                }
            });

            appBuilderConfiguration(branchBuilder);

            RequestDelegate branchDelegate = branchBuilder.Build();

            return app.Map(path, builder =>
            {
                builder.Use(async (context, next) =>
                {
                    await branchDelegate(context);
                });
            });
        }

        private class EmptyStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }
    }
}

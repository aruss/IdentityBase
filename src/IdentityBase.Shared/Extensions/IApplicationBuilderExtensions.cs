namespace IdentityBase.Extensions
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    public static class IApplicationBuilderExtensions
    {
        /*public static IApplicationBuilder UseBranchWithServices<TServer>(
            this IApplicationBuilder app,
            PathString path,
            Action<IServiceCollection> servicesConfiguration,
            Action<IApplicationBuilder> appBuilderConfiguration)

            where TServer : class, IServer
        {*/

        public static IApplicationBuilder UseBranchWithServices(
            this IApplicationBuilder app,
            PathString path,
            Action<IServiceCollection> servicesConfiguration,
            Action<IApplicationBuilder> appBuilderConfiguration)
        {
            IWebHost webHost = new WebHostBuilder()
                //.ConfigureServices(s => s.AddSingleton<IServer, TServer>())
                .UseKestrel()
                .ConfigureServices(servicesConfiguration)
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

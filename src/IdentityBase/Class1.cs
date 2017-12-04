namespace IdentityBase
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Builder;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public class FromAssemblyFeatureProvider : ControllerFeatureProvider
    {
        public static FromAssemblyFeatureProvider WithAssemblyOf<TFoo>()
        {
            return new FromAssemblyFeatureProvider(typeof(TFoo)
              .GetTypeInfo().Assembly);
        }

        private Assembly _assembly;

        public FromAssemblyFeatureProvider(Assembly assembly)
        {
            this._assembly = assembly ?? throw new ArgumentNullException();
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return typeInfo.Assembly == this._assembly &&
                base.IsController(typeInfo);
        }
    }

    public static class ParallelApplicationPipelinesExtensions
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
            var webHost = new WebHostBuilder()
                //.ConfigureServices(s => s.AddSingleton<IServer, TServer>())
                .UseKestrel()
                .ConfigureServices(servicesConfiguration)
                .UseStartup<EmptyStartup>()
                .Build();

            var serviceProvider = webHost.Services;
            var serverFeatures = webHost.ServerFeatures;

            var appBuilderFactory = serviceProvider
                .GetRequiredService<IApplicationBuilderFactory>();

            var branchBuilder = appBuilderFactory
                .CreateBuilder(serverFeatures);

            var factory = serviceProvider
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

            var branchDelegate = branchBuilder.Build();

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
            public void ConfigureServices(IServiceCollection services) { }

            public void Configure(IApplicationBuilder app) { }
        }
    }
}

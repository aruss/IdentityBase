
namespace IdentityBase.WebApi
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Services;
    using IdentityBase.WebApi.Actions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;
    using ServiceBase.Modules;
    using ServiceBase.Mvc;

    public class WebApiModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {

        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            IConfiguration config = appBuilder.ApplicationServices
                .GetRequiredService<IConfiguration>();

            appBuilder.UseBranchWithServices(
                "/api",
                (services) =>
                {
                    this.ServiceConfig(services, config);
                },
                (app) =>
                {
                    app.UseMvcWithDefaultRoute();
                });
        }

        private void ServiceConfig(
            IServiceCollection services,
            IConfiguration config)
        {
            Assembly assembly = typeof(WebApiModule)
                .GetTypeInfo().Assembly;

            ApplicationOptions applicationOptions =
                config.GetSection("App").Get<ApplicationOptions>() ??
                new ApplicationOptions();

            WebApiOptions webApiOptions =
                config.GetSection("WebApi").Get<WebApiOptions>() ??
                new WebApiOptions();

            services
                .AddRouting((options) =>
                {
                    options.LowercaseUrls = true;
                });

            services
                .AddMvc(mvcOptions =>
                {
                    mvcOptions.OutputFormatters
                        .AddDefaultJsonOutputFormatter();
                })
                .AddApplicationPart(assembly)
                .AddControllersAsServices()
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Clear();
                    manager.FeatureProviders.Add(
                        new TypedControllerFeatureProvider<WebApiController>());
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddScopePolicies<WebApiController>(
                        applicationOptions.PublicUrl,
                        assembly: assembly,
                        fromReferenced: true
                    );
                });

            services
               .AddAuthentication()
               .AddIdentityServerAuthentication(options =>
               {
                   options.Authority = applicationOptions.PublicUrl;

                   // TODO: extract to string extension
                   options.RequireHttpsMetadata =
                      applicationOptions.PublicUrl.IndexOf("https") > -1;

                   // TODO: move to constants
                   options.ApiName = WebApiConstants.ApiName;
                   options.ApiSecret = webApiOptions.ApiSecret;
               });
        }
    }
}

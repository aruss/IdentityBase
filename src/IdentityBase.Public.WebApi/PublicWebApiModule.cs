
namespace IdentityBase.Public.WebApi
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using IdentityBase.Configuration;
    using IdentityBase.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;
    using ServiceBase.Modules;
    using ServiceBase.Mvc;

    public class PublicWebApiModule : IModule
    {
        private List<ServiceDescriptor> _sharedServices =
            new List<ServiceDescriptor>();
             
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
           
            // get all required services from shared service collection

            this._sharedServices.AddRange(
                services.Where(d => d.ServiceType == typeof(UserAccountService)).ToArray());

        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            IConfiguration config = appBuilder.ApplicationServices
                .GetRequiredService<IConfiguration>();

            appBuilder.UseBranchWithServices(
                "/api",
                (services) =>
                {
                    // Shared services ....

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
            Assembly assembly = typeof(PublicWebApiModule)
                .GetTypeInfo().Assembly;

            ApplicationOptions applicationOptions =
                config.GetSection("App").Get<ApplicationOptions>() ??
                new ApplicationOptions();

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
                        new TypedControllerFeatureProvider<PublicApiController>());
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddScopePolicies<PublicApiController>(
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
                   options.ApiName = "idbase";
                   options.ApiSecret = applicationOptions.ApiSecret;
               });
        }
    }
}

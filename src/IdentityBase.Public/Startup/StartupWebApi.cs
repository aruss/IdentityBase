namespace IdentityBase.Public
{
    using System.Reflection;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Authorization;

    public static class StartupWebApi
    {
        public static void AddWebApi(
            this IServiceCollection services,
            ApplicationOptions applicationOptions)
        {
            services.AddAuthorization(options =>
            {
                options.AddScopePolicies<ApiController>(
                    applicationOptions.PublicUrl,
                    assembly: typeof(Startup).GetTypeInfo().Assembly,
                    fromReferenced: true
                );
            });

            services
                .AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = applicationOptions.PublicUrl;
                    options.RequireHttpsMetadata = applicationOptions
                        ?.PublicUrl.IndexOf("https") > -1;

                    options.ApiName = "idbase";
                    options.ApiSecret = applicationOptions.ApiSecret;
                });

        }

        public static void UseWebApi(
            this IApplicationBuilder app,
            ApplicationOptions applicationOptions)
        {
            
        }
    }
}

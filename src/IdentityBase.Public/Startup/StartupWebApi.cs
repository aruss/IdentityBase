namespace IdentityBase.Public
{
    using System.Net.Http;
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
            ApplicationOptions applicationOptions,
            HttpMessageHandler httpMessageHandler = null)
        {
            services.AddAuthorization(options =>
            {
                options.AddScopePolicies<ApiController>(
                    applicationOptions.PublicUrl,
                    assembly: typeof(StartupWebApi).GetTypeInfo().Assembly,
                    fromReferenced: true
                );
            });

            services
                .AddAuthentication()
                .AddIdentityServerAuthentication(options =>
                {
                    if (httpMessageHandler != null)
                    {
                        options.JwtBackChannelHandler =
                        options.IntrospectionDiscoveryHandler =
                        options.IntrospectionBackChannelHandler =
                            httpMessageHandler;
                    }

                    options.Authority = applicationOptions.PublicUrl;

                    options.RequireHttpsMetadata =
                        applicationOptions.PublicUrl.IndexOf("https") > -1;

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

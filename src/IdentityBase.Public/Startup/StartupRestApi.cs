using IdentityBase.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace IdentityBase.Public
{
    public static class StartupRestApi
    {
        public static void AddRestApi(this IServiceCollection services, ApplicationOptions options)
        {
            services.AddAuthorization(authOptions =>
            {
                authOptions.AddScopePolicies<ApiController>(options.PublicUrl);
            });
        }

        public static void UseRestApi(this IApplicationBuilder app, ApplicationOptions options)
        {
            app.Map("/api", appApi =>
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                appApi.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = options.PublicUrl,
                    RequireHttpsMetadata = false,
                    AllowedScopes = ScopeAuthorizeHelper
                        .GetAllScopeAuthorizeAttributes<ApiController>()
                        .Select(s => s.Scope).ToArray(),
                    AutomaticAuthenticate = true
                });

                appApi.UseMvc();
            });
        }
    }
}

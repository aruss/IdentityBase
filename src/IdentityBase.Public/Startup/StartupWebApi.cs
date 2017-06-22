using IdentityBase.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace IdentityBase.Public
{
    public static class StartupWebApi
    {
        public static void AddWebApi(this IServiceCollection services, ApplicationOptions options)
        {
            if (options.IsWebApiEnabled())
            {
                services.AddAuthorization(authOptions =>
                {
                    authOptions.AddScopePolicies<ApiController>(
                        options.PublicUrl,
                        fromReferenced: true);
                });
            }
        }

        public static void UseWebApi(this IApplicationBuilder app, ApplicationOptions options)
        {
            if (options.IsWebApiEnabled())
            {
                app.Map("/api", appApi =>
                {
                    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                    appApi.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                    {
                        Authority = options.PublicUrl,
                        RequireHttpsMetadata = false,
                        AllowedScopes = ScopeAuthorizeHelper
                            .GetAllScopeAuthorizeAttributes<ApiController>(fromReferenced: true)
                            .Select(s => s.Scope).ToArray(),
                        AutomaticAuthenticate = true
                    });

                    appApi.UseMvc();
                });
            }
        }
    }
}

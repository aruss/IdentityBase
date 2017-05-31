using IdentityBase.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public
{
    public static class StartupRestApi
    {
        public static void AddRestApi(this IServiceCollection services, ApplicationOptions options)
        {
            var issuer = "http://localhost:5000";

            services.AddAuthorization(authOptions =>
            {
                //authOptions.AddScopePolicy("useraccount:read", issuer);
                //authOptions.AddScopePolicy("useraccount:write", issuer);
                //authOptions.AddScopePolicy("useraccount:delete", issuer);
                authOptions.AddScopePolicy("api1", issuer);
            });
        }

        public static void AddScopePolicy(this AuthorizationOptions options, string scope, string issuer)
        {
            options.AddPolicy(scope, policy => policy.Requirements.Add(new HasScopeRequirement(scope, issuer)));
        }

        public static void UseRestApi(this IApplicationBuilder app, ApplicationOptions options)
        {
            app.Map("/api", appApi =>
            {
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                appApi.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = "http://localhost:5000",
                    RequireHttpsMetadata = false,
                    AllowedScopes = { "api1" },
                    AutomaticAuthenticate = true
                });

                appApi.UseMvc();
            });
        }
    }

    public class HasScopeRequirement : AuthorizationHandler<HasScopeRequirement>, IAuthorizationRequirement
    {
        private readonly string issuer;
        private readonly string scope;

        public HasScopeRequirement(string scope, string issuer)
        {
            this.scope = scope;
            this.issuer = issuer;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == issuer))
                return Task.CompletedTask;

            // Split the scopes string into an array
            //var scopes = context.User.FindFirst(c => c.Type == "scope" && c.Issuer == issuer).Value.Split(' ');
            var scopes = context.User.FindAll(c => c.Type == "scope" && c.Issuer == issuer).Select(s => s.Value); 

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s == scope))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}

namespace AspNetCoreWeb
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OAuth.Claims;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        public Startup()
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(
                options => options.ResourcesPath = "Resources");

            services
                .AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new LocationExpander());
                });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture =
                    new RequestCulture("en-US");

                options.SupportedCultures =
                options.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };
            });

            // https://leastprivilege.com/2017/11/15/missing-claims-in-the-asp-net-core-2-openid-connect-handler/
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.Cookie.Name = "mvchybrid";
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "http://localhost:5000";
                options.RequireHttpsMetadata = false;

                options.ClientSecret = "secret";
                options.ClientId = "mvc.hybrid";

                options.ResponseType = "code id_token";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("api1");
                options.Scope.Add("idbase");
                options.Scope.Add("offline_access");

                options.ClaimActions.Remove("amr");
                options.ClaimActions.MapJsonKey("website", "website");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                // Map here the claims for name and role 
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Email,
                        RoleClaimType = JwtClaimTypes.Role,
                    };

                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.SetParameter("culture",
                        CultureInfo.CurrentUICulture.Name);

                    return Task.FromResult(0);
                };

                options.Events.OnTicketReceived = async context =>
                {

                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestLocalization();
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}

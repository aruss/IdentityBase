//using ServiceBase.Notification.Email;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCoreWeb
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json",
                    optional: true,
                    reloadOnChange: true
                )
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json",
                    optional: true
                );

            /*if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }*/

            builder.AddEnvironmentVariables();

            _configuration = builder.Build();
            _environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders
                        .Add(new UI.CustomViewLocationExpander());
                });

            services
                .AddAuthentication(authOptions =>
                {
                    authOptions.DefaultScheme =
                        CookieAuthenticationDefaults.AuthenticationScheme;

                    authOptions.DefaultChallengeScheme =
                        OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(cookieOptions =>
                {

                })
                .AddOpenIdConnect(oidcOptions =>
                {
                    // oidcOptions.AuthenticationScheme = "oidc";
                    oidcOptions.SignInScheme = "Cookies";
                    oidcOptions.Authority = "http://localhost:5000";
                    oidcOptions.RequireHttpsMetadata = false;
                    oidcOptions.SignedOutRedirectUri = "http://localhost:3308/";
                    oidcOptions.ClientId = "mvc";
                    oidcOptions.ClientSecret = "secret";
                    oidcOptions.ResponseType = "code id_token";
                    oidcOptions.GetClaimsFromUserInfoEndpoint = true;
                    oidcOptions.SaveTokens = true;
                    oidcOptions.Events = new OpenIdConnectEvents
                    {
                        OnTicketReceived = async context =>
                        {
                            var profileId = Guid.Parse(context.Principal
                                .FindFirst("sub").Value);

                            var emailClaim = context?.Principal?
                                .FindFirst("email");

                            // Load current domain profile/user object if dont
                            // find any create one                       
                        },

                        // Provide idTokenHint and PostLogoutRedirectUri for
                        // better logout flow
                        OnRedirectToIdentityProviderForSignOut
                            = async context =>
                        {
                            var idTokenHint = await context.HttpContext
                                .GetTokenAsync("id_token");

                            if (idTokenHint != null)
                            {
                                context.ProtocolMessage
                                    .IdTokenHint = idTokenHint;

                                context.ProtocolMessage
                                    .PostLogoutRedirectUri = "http://localhost:3308/";
                            }
                        }
                    };

                    oidcOptions.Scope.Clear();
                    oidcOptions.Scope.Add("openid");
                    oidcOptions.Scope.Add("profile");
                    oidcOptions.Scope.Add("api1");
                });

            //services.AddTransient<IEmailService, DefaultEmailService>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            //loggerFactory.AddConsole();
            //loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
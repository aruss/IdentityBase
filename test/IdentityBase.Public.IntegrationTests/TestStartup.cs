using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using IdentityBase.Crypto;
using IdentityBase.Public.EntityFramework;
using IdentityBase.Extensions;
using IdentityBase.Services;
using ServiceBase.Notification.Email;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using ServiceBase;
using IdentityBase.Configuration;
using ServiceBase.Authorization;

namespace IdentityBase.Public.IntegrationTests
{
    public class TestStartup
    {
        public TestStartup(IHostingEnvironment environment)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRestApi(new ApplicationOptions()); 

            services.AddIdentityServer((options) =>
            {
                //options.RequireSsl = false;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ConsentUrl = "/consent";
                options.UserInteraction.ErrorUrl = "/error";
                options.Authentication.FederatedSignOutPaths.Add("/signout-oidc");
            })
                .AddTemporarySigningCredential()
                .AddProfileService<ProfileService>()
                .AddSecretParser<ClientAssertionSecretParser>()
                .AddSecretValidator<PrivateKeyJwtSecretValidator>()
                .AddTemporarySigningCredential();

            // Register mocked email service in case none of the tests registered one already
            if (!services.Any(c => c.ServiceType == typeof(IEmailService)))
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);
            }

            services.AddAuthorization(opt =>
            {
                opt.AddScopePolicy("api1", "http://localhost");
                opt.AddScopePolicy("useraccount.read", "http://localhost"); 
            }); 
            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();

            #region Entity Framework Store Layer

            services.AddEntityFrameworkStores((options) =>
            {
                options.DbContextOptions = (dbOptions) =>
                {
                    dbOptions.UseInMemoryDatabase(Guid.NewGuid().ToString()); 
  
                }; 
            });

            // Register default store initializer in case none of the tests registered one already
            if (!services.Any(c => c.ServiceType == typeof(IStoreInitializer)))
            {
                services.AddTransient<IStoreInitializer, TestStoreInitializer>();
            }

            #endregion Entity Framework Store Layer

            services
              .AddMvc()
              .AddRazorOptions(razor =>
              {
                  razor.ViewLocationExpanders.Add(new Public.Razor.CustomViewLocationExpander());
              });
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseExceptionHandler("/error");
            app.UseIdentityServer();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseRestApi(new ApplicationOptions());
            app.InitializeStores();
        }
    }
}
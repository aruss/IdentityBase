using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Extensions;
using IdentityBase.Services;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase;
using ServiceBase.Configuration;
using System;

//var myService = (IService)DependencyResolver.Current.GetService(typeof(IService));

namespace IdentityBase.Public
{

    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup : IStartup
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private IContainer _applicationContainer;

        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="logger"></param>
        public Startup(IHostingEnvironment environment, ILogger<Startup> logger)
        {
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (Configuration == null)
            {
                Configuration = ConfigurationSetup.Configure(_environment, (confBuilder) =>
                {
                    if (_environment.IsDevelopment())
                    {
                        confBuilder.AddUserSecrets<Startup>();
                    }
                });
            }
            
            var options = Configuration.GetSection("App").Get<ApplicationOptions>() ?? new ApplicationOptions();
            services.AddSingleton(Configuration);
            services.AddSingleton(options);
            services.AddIdentityServer(Configuration, _logger, _environment);
            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services.AddCors();
            if (options.EnableRestApi)
            {
                services.AddRestApi(options);
            }
            services.AddMvc().AddRazorOptions(razor =>
            {
                razor.ViewLocationExpanders.Add(
                    new Razor.CustomViewLocationExpander(Configuration["App:ThemePath"]));
            });

            
            
            // Update current instances 
            Current.Configuration = Configuration;
            Current.Logger = _logger;
            
            // Add AutoFac continer and register modules form config 
            var builder = new ContainerBuilder();
            builder.Populate(services);
            if (Configuration.ContainsSection("Services"))
            {
                builder.RegisterModule(new ConfigurationModule(Configuration.GetSection("Services")));
            }
            _applicationContainer = builder.Build();
            _applicationContainer.ValidateDataLayerServices(_logger);
            _applicationContainer.ValidateEmailSenderServices(_logger);
            _applicationContainer.ValidateSmsServices(_logger);
            _applicationContainer.ValidateEventServices(_logger);

            Current.Container = _applicationContainer; 

            return new AutofacServiceProvider(_applicationContainer);
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var appLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            var options = app.ApplicationServices.GetRequiredService<ApplicationOptions>();

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseIdentityServer();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            #region Use third party authentication

            if (!String.IsNullOrWhiteSpace(Configuration["Authentication:Google:ClientId"]))
            {
                _logger.LogInformation("Registering Google authentication scheme");

                app.UseGoogleAuthentication(new GoogleOptions
                {
                    AuthenticationScheme = "Google",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    ClientId = Configuration["Authentication:Google:ClientId"],
                    ClientSecret = Configuration["Authentication:Google:ClientSecret"]
                });
            }

            if (!String.IsNullOrWhiteSpace(Configuration["Authentication:Facebook:AppId"]))
            {
                _logger.LogInformation("Registering Facebook authentication scheme");

                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    AuthenticationScheme = "Facebook",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    AppId = Configuration["Authentication:Facebook:AppId"],
                    AppSecret = Configuration["Authentication:Facebook:AppSecret"]
                });
            }

            #endregion Use third party authentication

            if (options.EnableRestApi)
            {
                app.UseRestApi(options);
            }

            /*app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });*/
            app.UseMvcWithDefaultRoute(); 

            app.UseStaticFiles(Configuration, _logger, _environment);
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseCors("AllowAll");

           

            appLifetime.ApplicationStarted.Register(() =>
            {
                _logger.LogInformation("Application started");

                // TODO: implement leader election
                options.Leader = true;

                app.InitializeStores();
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                _logger.LogInformation("Stopping application...");
                app.CleanupStores();
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                _logger.LogInformation("Application stopped");
            });
        }
    }
}

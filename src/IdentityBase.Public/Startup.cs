using System;
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
using Serilog;
using ServiceBase;
using ServiceBase.Configuration;

//var myService = (IService)DependencyResolver.Current.GetService(typeof(IService));

namespace IdentityBase.Public
{
    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup : IStartup
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
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

            _logger.LogInformation("Services Configure");

            var options = Configuration.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();

            services.AddSingleton(Configuration);
            services.AddSingleton(options);
            services.AddIdentityServer(Configuration, _logger, _environment);
            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();

            services.AddCors(corsOpts =>
            {
                corsOpts.AddPolicy("CorsPolicy",
                    corsBuilder => corsBuilder.WithOrigins(
                        Configuration.GetValue<string>("Host:Cors")));
            });

            services.AddWebApi(options);
            services.AddMvc(options, _environment);

            // Update current instances
            Current.Configuration = Configuration;
            Current.Logger = _logger;

            // Add AutoFac continer and register modules form config
            var builder = new ContainerBuilder();
            builder.Populate(services);
            if (Configuration.ContainsSection("Services"))
            {
                builder.RegisterModule(
                    new ConfigurationModule(Configuration.GetSection("Services")));
            }
            _applicationContainer = builder.Build();
            _applicationContainer.ValidateDataLayerServices(_logger);
            _applicationContainer.ValidateEmailSenderServices(_logger);
            _applicationContainer.ValidateSmsServices(_logger);
            _applicationContainer.ValidateEventServices(_logger);

            Current.Container = _applicationContainer;

            _logger.LogInformation("Services Configured");

            return new AutofacServiceProvider(_applicationContainer);
        }

        public void Configure(IApplicationBuilder app)
        {
            _logger.LogInformation("Application Configure");

            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var appLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            var options = app.ApplicationServices.GetRequiredService<ApplicationOptions>();

            if (Program.Logger != null)
            {
                loggerFactory.AddSerilog(Program.Logger);
            }
            else if (Configuration.ContainsSection("Serilog"))
            {
                loggerFactory.AddSerilog(new LoggerConfiguration()
                   .ReadFrom.ConfigurationSection(Configuration.GetSection("Serilog"))
                   .CreateLogger());
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseCors("CorsPolicy");
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseStaticFiles(Configuration, _logger, _environment);

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

            app.UseWebApi(options);
            app.UseMvcWithDefaultRoute();

            appLifetime.ApplicationStarted.Register(() =>
            {
                // TODO: implement leader election
                options.Leader = true;

                app.InitializeStores();

                _logger.LogInformation("Application Started");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                _logger.LogInformation("Application Stopping");
                app.CleanupStores();
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                _logger.LogInformation("Application Stopped");
            });

            _logger.LogInformation("Application Configured");
        }
    }
}
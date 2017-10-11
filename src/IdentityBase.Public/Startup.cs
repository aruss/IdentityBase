namespace IdentityBase.Public
{
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
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase;

    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup : IStartup
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///
        /// </summary>
        /// <param name="configuration">Instance of <see cref="configuration"/>
        /// </param>
        /// <param name="environment">Instance of
        /// <see cref="IHostingEnvironment"/></param>
        /// <param name="logger">Instance of <see cref="ILogger{Startup}"/>
        /// </param>
        public Startup(
             IConfiguration configuration,
             IHostingEnvironment environment,
             ILogger<Startup> logger)
        {
            this._logger = logger;
            this._environment = environment;
            this._configuration = configuration;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            this._logger.LogInformation("Services Configure");

            ApplicationOptions options = this._configuration.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();

            services.AddSingleton(this._configuration);
            services.AddSingleton(options);

            services.AddIdentityServer(
                this._configuration,
                this._logger,
                this._environment);

            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddCors(corsOpts =>
            {
                corsOpts.AddPolicy("CorsPolicy",
                    corsBuilder => corsBuilder.WithOrigins(
                        this._configuration.GetValue<string>("Host:Cors")));
            });

            services.AddWebApi(options);
            services.AddMvc(options, _environment);

            // https://github.com/aspnet/Security/issues/1310
            services
                .AddAuthentication(
                    IdentityServerConstants.ExternalCookieAuthenticationScheme)
                .AddCookie();

            // Update current instances
            Current.Configuration = this._configuration;
            Current.Logger = this._logger;

            // Add AutoFac continer and register modules form config
            ContainerBuilder builder = new ContainerBuilder();
            builder.Populate(services);

            if (this._configuration.ContainsSection("Services"))
            {
                builder.RegisterModule(
                    new ConfigurationModule(
                        this._configuration.GetSection("Services")));
            }

            IContainer container = builder.Build();
            container.ValidateDataLayerServices(this._logger);
            container.ValidateEmailSenderServices(this._logger);
            container.ValidateSmsServices(this._logger);
            container.ValidateEventServices(this._logger);

            Current.Container = container;

            this._logger.LogInformation("Services Configured");

            return new AutofacServiceProvider(container);
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            this._logger.LogInformation("Application Configure");

            IHostingEnvironment env = app.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            IApplicationLifetime appLifetime = app.ApplicationServices
                .GetRequiredService<IApplicationLifetime>();

            ApplicationOptions options = app.ApplicationServices
                .GetRequiredService<ApplicationOptions>();

            app.UseMiddleware<RequestIdMiddleware>();
            app.UseLogging();            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseCors("CorsPolicy");

            app.UseStaticFiles(
                this._configuration,
                this._logger,
                this._environment);

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseWebApi(options);
            app.UseMvcWithDefaultRoute();

            appLifetime.ApplicationStarted.Register(() =>
            {
                // TODO: implement leader election
                options.Leader = true;

                app.InitializeStores();

                this._logger.LogInformation("Application Started");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                this._logger.LogInformation("Application Stopping");
                app.CleanupStores();
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                this._logger.LogInformation("Application Stopped");
            });

            this._logger.LogInformation("Application Configured");
        }
    }
}
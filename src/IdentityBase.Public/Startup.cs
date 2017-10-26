namespace IdentityBase.Public
{
    using System;
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
        private readonly ILogger logger;
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;
        private readonly ModuleHost moduleHost;

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
            this.logger = logger;
            this.environment = environment;
            this.configuration = configuration;

            this.moduleHost = new ModuleHost(this.configuration);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            this.logger.LogInformation("Services Configure");

            ApplicationOptions options = this.configuration.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();

            services.AddSingleton(this.configuration);
            services.AddSingleton(options);

            services.AddIdentityServer(
                this.configuration,
                this.logger,
                this.environment);

            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddCors(corsOpts =>
            {
                corsOpts.AddPolicy("CorsPolicy",
                    corsBuilder => corsBuilder.WithOrigins(
                        this.configuration.GetValue<string>("Host:Cors")));
            });

            services.AddWebApi(options);
            services.AddMvc(options, this.environment);

            // https://github.com/aspnet/Security/issues/1310
            services
                .AddAuthentication(
                    IdentityServerConstants.ExternalCookieAuthenticationScheme)
                .AddCookie();

            this.moduleHost.ConfigureServices(services);

            services.ValidateDataLayerServices(this.logger);
            services.ValidateEmailSenderServices(this.logger);
            services.ValidateSmsServices(this.logger);
            services.ValidateEventServices(this.logger);

            this.logger.LogInformation("Services Configured");

            return services.BuildServiceProvider();
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            this.logger.LogInformation("Application Configure");

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
            app.UseStaticFiles(options, this.environment);
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseWebApi(options);
            app.UseMvcWithDefaultRoute();

            this.moduleHost.Configure(app);

            appLifetime.ApplicationStarted.Register(() =>
            {
                // TODO: implement leader election
                options.Leader = true;

                app.InitializeStores();

                this.logger.LogInformation("Application Started");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                this.logger.LogInformation("Application Stopping");
                app.CleanupStores();
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                this.logger.LogInformation("Application Stopped");
            });

            this.logger.LogInformation("Application Configured");
        }
    }
}
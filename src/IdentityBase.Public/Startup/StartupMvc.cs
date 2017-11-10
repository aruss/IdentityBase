// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Mvc;
    using ServiceBase.Razor;

    public static class StartupMvc
    {
        public static void AddMvc(
            this IServiceCollection services,
            ApplicationOptions appOptions,
            IHostingEnvironment environment)
        {
            services
                .AddRouting((options) =>
                {
                    options.LowercaseUrls = true;
                });

            services
                .AddMvc(mvcOptions =>
                {
                    mvcOptions.OutputFormatters
                        .AddDefaultJsonOutputFormatter();
                })
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(
                        new ThemeViewLocationExpander(appOptions.ThemePath)
                    );
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    StartupMvc
                        .ConfigureApplicationPartManager(appOptions, manager);
                }); 
        }

        private static void ConfigureApplicationPartManager(
            ApplicationOptions appOptions,
            ApplicationPartManager manager)
        {
            // Remove default ControllerFeatureProvider 
            IApplicationFeatureProvider item = manager.FeatureProviders
                .FirstOrDefault(c => c.GetType()
                    .Equals(typeof(ControllerFeatureProvider)));

            if (item != null)
            {
                manager.FeatureProviders.Remove(item);
            }

            // Register new IApplicationFeatureProvider with a blacklist
            // depending on current configuration
            manager.FeatureProviders.Add(
                new BlackListedControllerFeatureProvider(new List<TypeInfo>()

                .AddIf<Api.Invitations.InvitationsGetController>(
                    !appOptions.EnableInvitationGetEndpoint)

                .AddIf<Api.Invitations.InvitationsPutController>(
                    !appOptions.EnableInvitationCreateEndpoint)

                .AddIf<Api.Invitations.InvitationsDeleteController>(
                    !appOptions.EnableInvitationDeleteEndpoint)

                .AddIf<Actions.Recover.RecoverController>(
                    !appOptions.EnableAccountRecovery)

                .AddIf<Actions.Register.RegisterController>(
                    !appOptions.EnableAccountRegistration)

                .AddIf<Api.UserAccounts.UserAccountDeleteController>(
                    !appOptions.EnableUserAccountDeleteEndpoint)

                .AddIf<Api.UserAccounts.ChangeEmailPostController>(
                    !appOptions.EnableAccountChangeEmailEndpoint)

                .AddIf<Actions.Account.ChangeEmailController>(
                    !appOptions.EnableAccountChangeEmailEndpoint)
            ));
        }

        private static List<TypeInfo> AddIf<TController>(
            this List<TypeInfo> list,
            bool assertion)
        {
            if (assertion)
            {
                list.Add(typeof(TController).GetTypeInfo());
            }

            return list;
        }
    }
}

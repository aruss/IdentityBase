using IdentityBase.Configuration;
using IdentityBase.Public.Actions.Recover;
using IdentityBase.Public.Actions.Register;
using IdentityBase.Public.Api;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IdentityBase.Public
{
    public static class StartupMvc
    {
        public static void AddMvc(this IServiceCollection services, ApplicationOptions options)
        {
            services.AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(
                        new Razor.CustomViewLocationExpander(options.ThemePath));
                })
                .ConfigureApplicationPartManager(manager =>
                {
                    // Remove default ControllerFeatureProvider 
                    var item = manager.FeatureProviders.FirstOrDefault(c => c.GetType()
                                    .Equals(typeof(ControllerFeatureProvider)));
                    if (item != null)
                    {
                        manager.FeatureProviders.Remove(item);
                    }

                    // Register new 
                    manager.FeatureProviders.Add(new BlackListedControllerFeatureProvider(new List<TypeInfo>()
                        .AddIf<UserAccountInviteController>(options.EnableUserInviteEndpoint)
                        .AddIf<RecoverController>(options.EnableAccountRecover)
                        .AddIf<RegisterController>(options.EnableAccountRegistration)
                    ));
                });
        }
        
        private static List<TypeInfo> AddIf<TController>(this List<TypeInfo> list, bool assertion)
        {
            if (!assertion)
            {
                list.Add(typeof(TController).GetTypeInfo());
            }

            return list;
        }
    }
}

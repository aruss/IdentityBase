using IdentityBase.Configuration;
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

                    // Register new IApplicationFeatureProvider with a blacklist depending on current configuration
                    manager.FeatureProviders.Add(new BlackListedControllerFeatureProvider(new List<TypeInfo>()
                        .AddIf<Api.UserAccountInvite.UserAccountInviteController>(!options.EnableUserInviteEndpoint)
                        .AddIf<Actions.Recover.RecoverController>(!options.EnableAccountRecover)
                        .AddIf<Actions.Register.RegisterController>(!options.EnableAccountRegistration)
                    ));
                });
        }
        
        private static List<TypeInfo> AddIf<TController>(this List<TypeInfo> list, bool assertion)
        {
            if (assertion)
            {
                list.Add(typeof(TController).GetTypeInfo());
            }

            return list;
        }
    }
}

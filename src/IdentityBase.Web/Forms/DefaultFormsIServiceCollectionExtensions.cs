
namespace IdentityBase.Web.Forms
{
    using IdentityBase.Forms;
    using Microsoft.Extensions.DependencyInjection;

    public static class DefaultFormsIServiceCollectionExtensions
    {
        public static void AddDefaultForms(this IServiceCollection service)
        {
            service.AddScoped<ILoginCreateViewModelAction,
                LoginCreateViewModelAction>();

            service.AddScoped<IRecoverCreateViewModelAction,
                RecoverCreateViewModelAction>();

            service.AddScoped<IRegisterCreateViewModelAction,
                RegisterCreateViewModelAction>(); 
        }
    }
}

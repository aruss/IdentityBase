// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
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

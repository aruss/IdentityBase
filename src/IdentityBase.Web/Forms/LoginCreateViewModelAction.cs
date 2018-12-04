// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Threading.Tasks;
    using IdentityBase.Configuration;

    public class LoginCreateViewModelAction : ILoginCreateViewModelAction
    {
        private readonly ApplicationOptions _options;

        public LoginCreateViewModelAction(ApplicationOptions options)
        {
            this._options = options;
        }

        public int Step => 0;

        public async Task ExecuteAsync(CreateViewModelContext context)
        {
            context.FormElements.Add(new FormElement
            {
                Name = "Email",
                ViewName = "LoginFormElements/Email"
            });

            context.FormElements.Add(new FormElement
            {
                Name = "Password",
                ViewName = "LoginFormElements/Password"
            });

            if (this._options.EnableRememberMe)
            {
                context.FormElements.Add(new FormElement
                {
                    Name = "RememberMe",
                    ViewName = "LoginFormElements/RememberMe"
                });
            }

            context.FormElements.Add(new FormElement
            {
                Name = "Submit",
                ViewName = "LoginFormElements/Submit"
            });
        }
    }
}

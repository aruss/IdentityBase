// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Threading.Tasks;

    public class LoginCreateViewModelAction : ILoginCreateViewModelAction
    {
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

            context.FormElements.Add(new FormElement
            {
                Name = "RememberMe",
                ViewName = "LoginFormElements/RememberMe"
            });

            context.FormElements.Add(new FormElement
            {
                Name = "Submit",
                ViewName = "LoginFormElements/Submit"
            });
        }
    }
}

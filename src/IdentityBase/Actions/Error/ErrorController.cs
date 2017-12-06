// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Error
{
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : WebController
    {
        private readonly IIdentityServerInteractionService _interaction;

        public ErrorController(IIdentityServerInteractionService interaction)
        {
            this._interaction = interaction;
        }

        [Route("error", Name ="Error")]
        public async Task<IActionResult> Index(string errorId)
        {
            ErrorViewModel vm = new ErrorViewModel();

            if (errorId != null)
            {
                ErrorMessage message = await this._interaction
                    .GetErrorContextAsync(errorId);

                if (message != null)
                {
                    vm.Error = message;
                }
            }

            return this.View("Error", vm);
        }
    }
}

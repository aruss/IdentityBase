// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Error
{
    using System.Threading.Tasks;
    using IdentityBase.Web;
    using IdentityBase.Web.ViewModels.Error;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class ErrorController : WebController
    {
        public ErrorController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<ErrorController> logger)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
        }

        [Route("error", Name = "Error")]
        public async Task<IActionResult> Error(string errorId)
        {
            ErrorViewModel vm = new ErrorViewModel();

            if (errorId != null)
            {
                ErrorMessage message = await this.InteractionService
                    .GetErrorContextAsync(errorId);

                if (message != null)
                {
                    vm.Error = message;
                }
            }

            return this.View(vm);
        }
    }
}

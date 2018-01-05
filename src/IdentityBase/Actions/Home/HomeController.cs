// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Home
{
    using System.Globalization;
    using System.Threading.Tasks;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class HomeController : WebController
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<HomeController> _logger; 

        public HomeController(
            IIdentityServerInteractionService interaction,
            ILogger<HomeController> logger)
        {
            this._interaction = interaction;
            this._logger = logger; 
        }

        /// <summary>
        /// Show landing page
        /// </summary>
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            this._logger.LogInformation("CurrentUICulture " +
                CultureInfo.CurrentUICulture); 
            
            // TODO: redirect to default RP since index page does not
            // provide any value
            return this.View();
        }
    }
}
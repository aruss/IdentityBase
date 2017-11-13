// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.Actions.Home
{
    using System.Threading.Tasks;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            this._interaction = interaction;
        }

        /// <summary>
        /// Show landing page
        /// </summary>
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            // TODO: redirect to default RP since index page does not
            // provide any value
            return this.View();
        }
    }
}
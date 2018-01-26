// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Consent
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class ConsentController : WebController
    {
        private readonly ILogger<ConsentController> _logger;
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IResourceStore _resourceStore;

        public ConsentController(
            ILogger<ConsentController> logger,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore)
        {
            this._logger = logger;
            this._interaction = interaction;
            this._clientStore = clientStore;
            this._resourceStore = resourceStore;
        }

        [HttpGet("consent", Name = "Consent")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            ConsentViewModel vm = await this.BuildViewModelAsync(returnUrl);

            if (vm != null)
            {
                return this.View("Index", vm);
            }

            return this.View("Error");
        }

        [HttpPost("consent", Name = "Consent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string button,
            ConsentInputModel model)
        {
            AuthorizationRequest request = await this._interaction
                .GetAuthorizationContextAsync(model.ReturnUrl);

            ConsentResponse response = null;

            if (button == "no")
            {
                response = ConsentResponse.Denied;
            }
            else if (button == "yes" && model != null)
            {
                if (model.ScopesConsented != null &&
                    model.ScopesConsented.Any())
                {
                    response = new ConsentResponse
                    {
                        RememberConsent = model.RememberConsent,
                        ScopesConsented = model.ScopesConsented
                    };
                }
                else
                {
                    this.ModelState.AddModelError(
                        "You must pick at least one permission.");
                }
            }
            else
            {
                this.ModelState.AddModelError("Invalid Selection");
            }

            if (response != null)
            {
                await this._interaction.GrantConsentAsync(request, response);
                return this.Redirect(model.ReturnUrl);
            }

            ConsentViewModel vm =
                await this.BuildViewModelAsync(model.ReturnUrl, model);

            if (vm != null)
            {
                return this.View("Index", vm);
            }

            return this.View("Error");
        }

        [NonAction]
        private async Task<ConsentViewModel> BuildViewModelAsync(
            string returnUrl,
            ConsentInputModel model = null)
        {
            AuthorizationRequest request = await this._interaction
                .GetAuthorizationContextAsync(returnUrl);

            if (request != null)
            {
                Client client = await this._clientStore
                    .FindEnabledClientByIdAsync(request.ClientId);

                if (client != null)
                {
                    Resources resources = await this._resourceStore
                        .FindEnabledResourcesByScopeAsync(
                            request.ScopesRequested);

                    if (resources != null &&
                        (resources.IdentityResources.Any() ||
                            resources.ApiResources.Any()))
                    {
                        return this.CreateConsentViewModel(model,
                            returnUrl,
                            request,
                            client,
                            resources);
                    }
                    else
                    {
                        this._logger.LogError(
                            "No scopes matching: {0}",
                            request.ScopesRequested
                                .Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    this._logger.LogError(
                        "Invalid client id: {0}",
                        request.ClientId);
                }
            }
            else
            {
                this._logger.LogError(
                    "No consent request matching request: {0}",
                    returnUrl);
            }

            return null;
        }

        [NonAction]
        private ConsentViewModel CreateConsentViewModel(
            ConsentInputModel model,
            string returnUrl,
            AuthorizationRequest request,
            Client client,
            Resources resources)
        {
            ConsentViewModel vm = new ConsentViewModel()
            {
                RememberConsent = model?.RememberConsent ?? true,

                ScopesConsented = model?.ScopesConsented ??
                    Enumerable.Empty<string>(),

                ReturnUrl = returnUrl,
                ClientName = client.ClientName,
                ClientUrl = client.ClientUri,
                ClientLogoUrl = client.LogoUri,
                AllowRememberConsent = client.AllowRememberConsent
            };

            vm.IdentityScopes = resources.IdentityResources
                .Select(x => CreateScopeViewModel(x, vm.ScopesConsented
                    .Contains(x.Name) || model == null))
                .ToArray();

            vm.ResourceScopes = resources.ApiResources
                .SelectMany(x => x.Scopes)
                .Select(x => CreateScopeViewModel(x, vm.ScopesConsented
                    .Contains(x.Name) || model == null))
                .ToArray();

            // TODO: make use of application settings
            if (resources.OfflineAccess)
            {
                vm.ResourceScopes = vm.ResourceScopes
                    .Union(new ScopeViewModel[] {
                        this.GetOfflineAccessScope(vm.ScopesConsented.Contains(
                            IdentityServerConstants.StandardScopes.OfflineAccess) ||
                            model == null
                        )
                    });

                // You must prompt for consent every time offline_access is requested.
                // https://github.com/IdentityServer/IdentityServer3/issues/2057
                vm.AllowRememberConsent = false;
            }

            return vm;
        }

        [NonAction]
        private ScopeViewModel CreateScopeViewModel(
            IdentityResource identity,
            bool check)
        {
            return new ScopeViewModel
            {
                Name = identity.Name,
                DisplayName = identity.DisplayName,
                Description = identity.Description,
                Emphasize = identity.Emphasize,
                Required = identity.Required,
                Checked = check || identity.Required,
            };
        }

        [NonAction]
        private ScopeViewModel CreateScopeViewModel(Scope scope, bool check)
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                Checked = check || scope.Required,
            };
        }

        [NonAction]
        private ScopeViewModel GetOfflineAccessScope(bool check)
        {
            return new ScopeViewModel
            {
                Name = IdentityServerConstants.StandardScopes.OfflineAccess,
                DisplayName = "Offline Access",
                Description = "Access to your applications and resources, even when you are offline",
                Emphasize = true,
                Checked = check
            };
        }
    }
}

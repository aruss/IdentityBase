namespace IdentityBase.Public.Actions.Consent
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class ConsentController : Controller
    {
        private readonly ILogger<ConsentController> logger;
        private readonly IClientStore clientStore;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IResourceStore resourceStore;

        public ConsentController(
            ILogger<ConsentController> logger,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore)
        {
            this.logger = logger;
            this.interaction = interaction;
            this.clientStore = clientStore;
            this.resourceStore = resourceStore;
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

        [HttpPost("consent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string button, ConsentInputModel model)
        {
            var request = await interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            ConsentResponse response = null;

            if (button == "no")
            {
                response = ConsentResponse.Denied;
            }
            else if (button == "yes" && model != null)
            {
                if (model.ScopesConsented != null && model.ScopesConsented.Any())
                {
                    response = new ConsentResponse
                    {
                        RememberConsent = model.RememberConsent,
                        ScopesConsented = model.ScopesConsented
                    };
                }
                else
                {
                    ModelState.AddModelError("", "You must pick at least one permission.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid Selection");
            }

            if (response != null)
            {
                await interaction.GrantConsentAsync(request, response);
                return Redirect(model.ReturnUrl);
            }

            var vm = await BuildViewModelAsync(model.ReturnUrl, model);
            if (vm != null)
            {
                return View("Index", vm);
            }

            return View("Error");
        }

        private async Task<ConsentViewModel> BuildViewModelAsync(string returnUrl, ConsentInputModel model = null)
        {
            var request = await interaction.GetAuthorizationContextAsync(returnUrl);
            if (request != null)
            {
                var client = await clientStore.FindEnabledClientByIdAsync(request.ClientId);
                if (client != null)
                {
                    var resources = await resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
                    if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
                    {
                        return CreateConsentViewModel(model, returnUrl, request, client, resources);
                    }
                    else
                    {
                        logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    logger.LogError("Invalid client id: {0}", request.ClientId);
                }
            }
            else
            {
                logger.LogError("No consent request matching request: {0}", returnUrl);
            }

            return null;
        }

        private ConsentViewModel CreateConsentViewModel(
            ConsentInputModel model, string returnUrl,
            AuthorizationRequest request,
            Client client, Resources resources)
        {
            var vm = new ConsentViewModel()
            {
                RememberConsent = model?.RememberConsent ?? true,
                ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
                ReturnUrl = returnUrl,
                ClientName = client.ClientName,
                ClientUrl = client.ClientUri,
                ClientLogoUrl = client.LogoUri,
                AllowRememberConsent = client.AllowRememberConsent
            };

            vm.IdentityScopes = resources.IdentityResources.Select(x => CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();
            vm.ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => CreateScopeViewModel(x, vm.ScopesConsented.Contains(x.Name) || model == null)).ToArray();

            // TODO: make use of application settings
            if (resources.OfflineAccess)
            {
                vm.ResourceScopes = vm.ResourceScopes.Union(new ScopeViewModel[] {
                    GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null)
                });
            }

            return vm;
        }

        public ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
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

        public ScopeViewModel CreateScopeViewModel(Scope scope, bool check)
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

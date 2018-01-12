namespace IdentityBase
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Http;

    public class IdentityBaseContextFactory :
        IServiceFactory<IdentityBaseContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IClientStore _clientStore;

        public IdentityBaseContextFactory(
            IHttpContextAccessor httpContextAccessor,
            IIdentityServerInteractionService interactionService,
            IClientStore clientStore,
            ApplicationOptions appOptions)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._interactionService = interactionService;
            this._clientStore = clientStore;
        }

        public IdentityBaseContext Build()
        {
            return this.GetIdentityBaseContext().Result;
        }

        private async Task<IdentityBaseContext> GetIdentityBaseContext()
        {
            var context = new IdentityBaseContext();

            string returnUrl = this._httpContextAccessor
               .HttpContext.Request.Query["ReturnUrl"];

            if (this._interactionService.IsValidReturnUrl(returnUrl))
            {
                await this.SetAuthorizationRequest(context, returnUrl);

                await this.SetClientInfo(context,
                    context.AuthorizationRequest.ClientId);

                return context;
            }

            string logoutId = this._httpContextAccessor
               .HttpContext.Request.Query["LogoutId"];

            if (!String.IsNullOrWhiteSpace(logoutId))
            {
                await this.SetLogoutRequest(context, logoutId);

                await this.SetClientInfo(context,
                    context.LogoutRequest.ClientId);

                return context;
            }

            string clientId = this._httpContextAccessor
               .HttpContext.Request.Query["ClientId"];

            if (!String.IsNullOrWhiteSpace(clientId))
            {
                await this.SetClientInfo(context, clientId);
                return context;
            }

            return context;
        }

        /// <summary>
        /// Creates <see cref="IdentityBaseContext"/>from logoutId.
        /// </summary>
        /// <param name="logoutId"></param>
        /// <returns></returns>
        private async Task SetLogoutRequest(
            IdentityBaseContext context,
            string logoutId)
        {
            LogoutRequest request = await this._interactionService
               .GetLogoutContextAsync(logoutId);

            if (request == null)
            {
                return;
            }

            context.LogoutId = logoutId;
            context.LogoutRequest = request;
        }

        /// <summary>
        /// Creates <see cref="IdentityBaseContext"/>from returnUrl.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private async Task SetAuthorizationRequest(
            IdentityBaseContext context,
            string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl))
            {
                return;
            }

            AuthorizationRequest request =
                await this._interactionService
                    .GetAuthorizationContextAsync(returnUrl);

            if (request == null)
            {
                return;
            }

            context.ReturnUrl = returnUrl;
            context.AuthorizationRequest = request;
        }

        private async Task SetClientInfo(
            IdentityBaseContext context,
            string clientId)
        {
            Client client = await this._clientStore
                .FindClientByIdAsync(clientId);

            if (client != null && client.Enabled == true)
            {
                context.Client = client;

                context.ClientProperties = client.Properties
                    .ToObject<ClientProperties>();
            }

            // fill other junk
        }
    }
}
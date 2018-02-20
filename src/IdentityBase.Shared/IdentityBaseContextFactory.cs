namespace IdentityBase
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Extensions;

    public class IdentityBaseContextBasicFactory
        : IServiceFactory<IdentityBaseContext>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientStore _clientStore;

        public IdentityBaseContextBasicFactory(
            IHttpContextAccessor httpContextAccessor,
            IClientStore clientStore,
            ApplicationOptions appOptions)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._clientStore = clientStore;
        }

        public IdentityBaseContext Build()
        {
            return this.GetIdentityBaseContextAsync().Result;
        }

        internal virtual async Task<IdentityBaseContext>
            GetIdentityBaseContextAsync()
        {
            var context = new IdentityBaseContext();

            string clientId = this._httpContextAccessor
               .HttpContext.Request.Query["clientid"]; 

            if (String.IsNullOrWhiteSpace(clientId))
            {
                clientId = this._httpContextAccessor
                    .HttpContext.Request.Query["client_id"];
            }

            if (String.IsNullOrWhiteSpace(clientId))
            {
                clientId = this._httpContextAccessor
                  .HttpContext.Request.Headers["X-ClientId"].FirstOrDefault();
            }

            if (!String.IsNullOrWhiteSpace(clientId))
            {
                await this.SetClientInfoAsync(context, clientId);
                return context;
            }
            
            return context;
        }

        internal async Task SetClientInfoAsync(
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

    public class IdentityBaseContextIdSrvFactory : IdentityBaseContextBasicFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IClientStore _clientStore;

        public IdentityBaseContextIdSrvFactory(
            IHttpContextAccessor httpContextAccessor,
            IIdentityServerInteractionService interactionService,
            IClientStore clientStore,
            ApplicationOptions appOptions)
            : base(httpContextAccessor, clientStore, appOptions)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._interactionService = interactionService;
            this._clientStore = clientStore;
        }

        public IdentityBaseContext Build()
        {
            return this.GetIdentityBaseContextAsync().Result;
        }

        internal override async Task<IdentityBaseContext>
            GetIdentityBaseContextAsync()
        {
            var context = new IdentityBaseContext();

            string returnUrl = this._httpContextAccessor
               .HttpContext.Request.Query["ReturnUrl"];

            if (this._interactionService.IsValidReturnUrl(returnUrl))
            {
                await this.SetAuthorizationRequest(context, returnUrl);

                await this.SetClientInfoAsync(context,
                    context.AuthorizationRequest.ClientId);

                return context;
            }

            string logoutId = this._httpContextAccessor
               .HttpContext.Request.Query["LogoutId"];

            if (!String.IsNullOrWhiteSpace(logoutId))
            {
                await this.SetLogoutRequest(context, logoutId);

                await this.SetClientInfoAsync(context,
                    context.LogoutRequest.ClientId);

                return context;
            }
            
            return await base.GetIdentityBaseContextAsync();
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
    }
}
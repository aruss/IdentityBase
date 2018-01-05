namespace IdentityBase
{
    using System;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    
    public class IdentityBaseContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IClientStore _clientStore;

        public IdentityBaseContextMiddleware(RequestDelegate next,
            IIdentityServerInteractionService interactionService,
            IClientStore clientStore)
        {
            this._next = next;
            this._interactionService = interactionService;
            this._clientStore = clientStore; 
        }

        public async Task Invoke(HttpContext httpContext)
        {           
            IdentityBaseContext identityBaseContext =
               httpContext.RequestServices
                   .GetService<IdentityBaseContext>();

            string returnUrl = httpContext.Request.Query["ReturnUrl"];

            if (!String.IsNullOrEmpty(returnUrl))
            {
                await this.SetContext(returnUrl, identityBaseContext);
            }

            await this._next(httpContext);
        }

        private async Task SetContext(
            string returnUrl,
            IdentityBaseContext context)
        {
            AuthorizationRequest request =
                await this._interactionService
                    .GetAuthorizationContextAsync(returnUrl);

            context.AuthorizationRequest = request; 

            if (!context.IsValid)
            {
                return;
            }

            Client client = await this._clientStore
                .FindClientByIdAsync(request.ClientId);

            if (client != null && client.Enabled == true)
            {
                context.Client = client;
            }
            
            // fill other junk
        }
    }
}
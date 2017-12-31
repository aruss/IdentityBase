namespace IdentityBase.Services
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using ServiceBase;

    public class AuthenticationService
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationOptions _applicationOptions;
        private readonly IDateTimeAccessor _dateTimeAccessor;

        public AuthenticationService(
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationOptions applicationOptions,
            IDateTimeAccessor dateTimeAccessor)
        {
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._httpContextAccessor = httpContextAccessor;
            this._applicationOptions = applicationOptions;
            this._dateTimeAccessor = dateTimeAccessor; 
        }

        /// <summary>
        /// Signs the <see cref="UserAccount"/> in.
        /// </summary>
        /// <param name="userAccount">The instance of
        /// <see cref="UserAccount"/>.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="properties">The properties.</param>
        public async Task SignInAsync(
           UserAccount userAccount,
           string returnUrl,
           bool rememberLogin = false)
        {
            AuthenticationProperties properties = null;

            if (this._applicationOptions.EnableRememberLogin &&
                rememberLogin)
            {
                properties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = this._dateTimeAccessor.UtcNow.Add(
                        TimeSpan.FromSeconds(
                            this._applicationOptions.RememberMeLoginDuration
                        )
                    )
                };
            };

            await this._httpContextAccessor.HttpContext.SignInAsync(
                userAccount.Id.ToString(),
                userAccount.Email,
                properties);
            
            await this._userAccountService
                    .PerceiveSuccessfulLoginAsync(userAccount);         
        }

        public async Task SignOutAsync()
        {
            // TODO: signout
            // TODO: log signout event 
        }
    }
}

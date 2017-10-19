namespace IdentityBase.Services
{
    using IdentityModel;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Models;

    public class ProfileService : IProfileService
    {
        private readonly IUserAccountStore userAccountStore;

        public ProfileService(IUserAccountStore userAccountStore)
        {
            this.userAccountStore = userAccountStore;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Guid userAccountId = Guid.Parse(context.Subject
                .FindFirst(JwtClaimTypes.Subject).Value);

            UserAccount userAccount = this.userAccountStore
                .LoadByIdAsync(userAccountId).Result;

            // TODO: get claims from db user
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, userAccount.Id.ToString()),
                new Claim(JwtClaimTypes.Email, userAccount.Email),

                new Claim(
                    JwtClaimTypes.EmailVerified,
                    userAccount.IsEmailVerified.ToString().ToLower(),
                    ClaimValueTypes.Boolean
                )
            };

            context.IssuedClaims = claims;

            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            Guid userId = Guid.Parse(context.Subject
                .FindFirst(JwtClaimTypes.Subject).Value);

            UserAccount user = this.userAccountStore
                .LoadByIdAsync(userId).Result;

            context.IsActive = user != null && user.IsLoginAllowed;

            return Task.FromResult(0);
        }
    }
}
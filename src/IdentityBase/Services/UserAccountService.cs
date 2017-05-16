using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.Extensions.Options;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Events;
using IdentityBase.Extensions;
using IdentityBase.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Services
{
    public class UserAccountService
    {
        private ApplicationOptions _applicationOptions;
        private ICrypto _crypto;
        private IUserAccountStore _userAccountStore;
        private IEventService _eventService;

        public UserAccountService(
            IOptions<ApplicationOptions> applicationOptions,
            ICrypto crypto,
            IUserAccountStore userAccountStore,
            IEventService eventService)
        {
            _applicationOptions = applicationOptions.Value;
            _crypto = crypto;
            _userAccountStore = userAccountStore;
            _eventService = eventService;
        }

        /// <summary>
        /// Used for local authentication
        /// </summary>
        /// <param name="email">Local user account email</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public async Task<UserAccountVerificationResult> VerifyByEmailAndPasswordAsyc(string email, string password)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            var result = new UserAccountVerificationResult();
            var userAccount = await _userAccountStore.LoadByEmailAsync(email.ToLower());

            if (userAccount == null)
            {
                return result;
            }

            result.IsPasswordValid = _crypto.VerifyPasswordHash(userAccount.PasswordHash, password,
                _applicationOptions.PasswordHashingIterationCount);

            result.UserAccount = userAccount;
            result.IsLoginAllowed = userAccount.IsLoginAllowed;
            result.NeedChangePassword = false;
            result.IsLocalAccount = userAccount.HasPassword();

            return result;
        }

        #region Load UserAccounts

        public async Task<UserAccount> LoadByEmailAsync(string email)
        {
            return await _userAccountStore.LoadByEmailAsync(email);
        }

        public async Task<UserAccount> LoadByEmailWithExternalAsync(string email)
        {
            return await _userAccountStore.LoadByEmailWithExternalAsync(email);
        }

        public async Task<UserAccount> LoadByExternalProviderAsync(string provider, string subject)
        {
            return await _userAccountStore.LoadByExternalProviderAsync(provider, subject);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await _userAccountStore.DeleteByIdAsync(id);
        }

        #endregion

        #region Create new UserAccount

        public async Task<UserAccount> CreateNewLocalUserAccountAsync(string email, string password, string returnUrl = null)
        {
            var now = DateTime.UtcNow;

            var userAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = _crypto.HashPassword(password,
                    _applicationOptions.PasswordHashingIterationCount),
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsLoginAllowed = _applicationOptions.RequireLocalAccountVerification,
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (_applicationOptions.RequireLocalAccountVerification &&
                !String.IsNullOrWhiteSpace(returnUrl))
            {
                // Set verification key
                userAccount.SetVerification(
                    _crypto.Hash(_crypto.GenerateSalt()).StripUglyBase64(),
                    VerificationKeyPurpose.ConfirmAccount,
                    returnUrl,
                    now);
            }

            await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountCreatedEventAsync(
                userAccount.Id,
                IdentityServerConstants.LocalIdentityProvider);

            return userAccount;
        }

        public async Task<UserAccount> CreateNewExternalUserAccountAsync(string email, string provider, string subject, string returnUrl = null)
        {
            var now = DateTime.UtcNow;

            var userAccountId = Guid.NewGuid();

            var userAccount = new UserAccount
            {
                Id = userAccountId,
                Email = email,
                PasswordHash = null,
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsLoginAllowed = !_applicationOptions.RequireExternalAccountVerification,
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
                Accounts = new ExternalAccount[]
                {
                    new ExternalAccount
                    {
                        Email = email,
                        UserAccountId = userAccountId,
                        Provider = provider,
                        Subject = subject,
                        CreatedAt = now,
                        UpdatedAt  = now,
                        LastLoginAt = null,
                        IsLoginAllowed = true
                    }
                }
            };

            if (_applicationOptions.RequireExternalAccountVerification &&
                !String.IsNullOrWhiteSpace(returnUrl))
            {
                // Set verification key
                userAccount.SetVerification(
                    _crypto.Hash(_crypto.GenerateSalt()).StripUglyBase64(),
                    VerificationKeyPurpose.ConfirmAccount,
                    returnUrl,
                    now);
            }

            await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountCreatedEventAsync(
                userAccount.Id,
                provider);

            return userAccount;
        }

        #endregion Create new UserAccount

        #region Add Local/External Accounts to existing one

        public async Task<ExternalAccount> AddExternalAccountAsync(Guid userAccountId, string email, string provider, string subject)
        {
            var now = DateTime.UtcNow; // TODO: user time service
            var externalAccount = new ExternalAccount
            {
                UserAccountId = userAccountId,
                Email = email,
                Provider = provider,
                Subject = subject,
                CreatedAt = now,
                UpdatedAt = now,
                LastLoginAt = null,
                IsLoginAllowed = true
            };

            await _userAccountStore.WriteExternalAccountAsync(externalAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(userAccountId);

            return externalAccount;
        }

        public async Task<UserAccount> AddLocalCredentialsAsync(Guid userAccountId, string password)
        {
            var userAccount = await _userAccountStore.LoadByIdAsync(userAccountId);
            return await AddLocalCredentialsAsync(userAccount, password);
        }

        public async Task<UserAccount> AddLocalCredentialsAsync(UserAccount userAccount, string password)
        {
            var now = DateTime.UtcNow; // TODO: user time service

            // Set user account password
            userAccount.PasswordHash = _crypto.HashPassword(password,
                _applicationOptions.PasswordHashingIterationCount);

            userAccount.UpdatedAt = now;
            userAccount = await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(userAccount.Id);

            return userAccount;
        }

        #endregion

        #region Update UserAccount in case user is authenticated

        public async Task UpdateLastUsedExternalAccountAsync(UserAccount userAccount, string provider, string subject)
        {
            var externalAccount = userAccount.Accounts.FirstOrDefault(c => c.Provider.Equals(provider) && c.Subject.Equals(subject));

            if (externalAccount != null)
            {
                await this.UpdateLastUsedExternalAccountAsync(externalAccount);
            }
        }

        public async Task UpdateLastUsedExternalAccountAsync(ExternalAccount externalAccount)
        {
            // TODO: user time service
            var now = DateTime.UtcNow;

            externalAccount.LastLoginAt = now;
            externalAccount.UpdatedAt = now;

            await _userAccountStore.WriteExternalAccountAsync(externalAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(
                externalAccount.UserAccountId);
        }

        public async Task UpdateLastUsedLocalAccountAsync(UserAccount userAccount, bool success)
        {
            var now = DateTime.UtcNow;

            if (success)
            {
                userAccount.FailedLoginCount = 0;
                userAccount.LastFailedLoginAt = null;
                userAccount.LastLoginAt = now;
            }
            else
            {
                userAccount.FailedLoginCount++;
                userAccount.LastFailedLoginAt = now;
                if (userAccount.FailedLoginCount >= _applicationOptions
                    .AccountLockoutFailedLoginAttempts)
                {
                    userAccount.IsLoginAllowed = false;
                }
            }

            // Update user account
            userAccount.UpdatedAt = now;
            userAccount = await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(
                userAccount.Id);
        }

        #endregion

        #region Recover UserAccount

        /// <summary>
        /// Create verification key and updates the <see cref="UserAccount"/>.
        /// Will also raise a event
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task SetResetPasswordVirificationKey(UserAccount userAccount, string returnUrl = null)
        {
            // Set verification key
            userAccount.SetVerification(
                _crypto.Hash(_crypto.GenerateSalt()).StripUglyBase64(),
                VerificationKeyPurpose.ResetPassword,
                returnUrl,
                DateTime.UtcNow); // TODO: use time service

            // Update user account
            await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(
                userAccount.Id);
        }

        #endregion

        #region Confirm UserAccount

        public async Task SetEmailVerifiedAsync(UserAccount userAccount)
        {
            var now = DateTime.UtcNow;
            userAccount.IsLoginAllowed = true;
            userAccount.IsEmailVerified = true;
            userAccount.EmailVerifiedAt = now;
            userAccount.UpdatedAt = now;

            await ClearVerificationKeyAsync(userAccount);
        }

        #endregion

        /// <summary>
        /// Validate if verification key is valid, if yes it will load a corresponding <see cref="UserAccount"/>
        /// </summary>
        /// <param name="key">Verification key</param>
        /// <param name="purpose"><see cref="VerificationKeyPurpose"/></param>
        /// <returns></returns>
        public async Task<TokenVerificationResult> HandleVerificationKey(
            string key,
            VerificationKeyPurpose purpose)
        {
            var result = new TokenVerificationResult();
            var userAccount = await _userAccountStore.LoadByVerificationKeyAsync(key);
            if (userAccount == null)
            {
                return result;
            }

            result.UserAccount = userAccount;
            result.PurposeValid = userAccount.VerificationPurpose == (int)purpose;

            if (userAccount.VerificationKeySentAt.HasValue)
            {
                var validTill = userAccount.VerificationKeySentAt.Value +
                    TimeSpan.FromMinutes(_applicationOptions.VerificationKeyLifetime);
                var now = DateTime.Now;

                result.TokenExpired = now > validTill;
            }

            return result;
        }

        /// <summary>
        /// Clears all verification key data and saves the <see cref="UserAccount"/>
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        public async Task ClearVerificationKeyAsync(UserAccount userAccount)
        {
            userAccount.ClearVerification();

            // Update user account
            await _userAccountStore.WriteAsync(userAccount);

            // Emit event
            await _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(
                userAccount.Id);
        }
    }

    public class TokenVerificationResult
    {
        public UserAccount UserAccount { get; set; }
        public bool TokenExpired { get; set; }
        public bool PurposeValid { get; set; }
    }

    public class UserAccountVerificationResult
    {
        public UserAccount UserAccount { get; set; }
        public bool NeedChangePassword { get; set; }
        public bool IsLoginAllowed { get; set; }
        public bool IsLocalAccount { get; set; }
        public bool IsPasswordValid { get; set; }
    }
}
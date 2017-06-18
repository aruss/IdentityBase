using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Events;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using ServiceBase.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Services
{
    public class UserAccountService
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ICrypto _crypto;
        private readonly IUserAccountStore _userAccountStore;
        private readonly IEventService _eventService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccountService(
            ApplicationOptions applicationOptions,
            ICrypto crypto,
            IUserAccountStore userAccountStore,
            IEventService eventService,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationOptions = applicationOptions;
            _crypto = crypto;
            _userAccountStore = userAccountStore;
            _eventService = eventService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Used for local authentication
        /// </summary>
        /// <param name="email">Local user account email</param>
        /// <param name="password">Password</param>
        /// <returns><see cref="UserAccountVerificationResult"/></returns>
        public async Task<UserAccountVerificationResult> VerifyByEmailAndPasswordAsync(string email, string password)
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

            if (userAccount.HasPassword())
            {
                result.IsLocalAccount = true;
                result.IsPasswordValid = _crypto.VerifyPasswordHash(userAccount.PasswordHash, password,
                    _applicationOptions.PasswordHashingIterationCount);
            }

            result.UserAccount = userAccount;
            result.IsLoginAllowed = userAccount.IsLoginAllowed;
            result.NeedChangePassword = false;

            if (!result.IsPasswordValid && !result.IsLocalAccount)
            {
                var hints = userAccount.Accounts.Select(s => s.Provider).ToArray();
            }

            return result;
        }

        public async Task<UserAccount> LoadByIdAsync(Guid id)
        {
            return await _userAccountStore.LoadByIdAsync(id);
        }

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

        public void UpdateSuccessfulLogin(UserAccount userAccount)
        {
            var now = DateTime.UtcNow;

            userAccount.FailedLoginCount = 0;
            userAccount.LastFailedLoginAt = null;
            userAccount.LastLoginAt = now;
        }

        public async Task UpdateSuccessfulLoginAsync(UserAccount userAccount)
        {
            UpdateSuccessfulLogin(userAccount);
            await WriteUserAccountAsync(userAccount);
        }

        public UserAccount CreateNewLocalUserAccount(string email, string password = null, string returnUrl = null)
        {
            var now = DateTime.UtcNow;

            var userAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = email,
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsLoginAllowed = _applicationOptions.RequireLocalAccountVerification,
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (!String.IsNullOrWhiteSpace(password))
            {
                userAccount.PasswordHash = _crypto.HashPassword(password,
                    _applicationOptions.PasswordHashingIterationCount);
            }

            if (_applicationOptions.RequireLocalAccountVerification &&
                !String.IsNullOrWhiteSpace(returnUrl))
            {
                SetVerification(userAccount,
                    _crypto.Hash(_crypto.GenerateSalt()).StripUglyBase64(),
                    VerificationKeyPurpose.ConfirmAccount,
                    returnUrl, now);
            }

            return userAccount;
        }

        public async Task<UserAccount> CreateNewLocalUserAccountAsync(string email, string password, string returnUrl = null)
        {
            var userAccount = CreateNewLocalUserAccount(email, password, returnUrl);
            return await WriteUserAccountAsync(userAccount);
        }

        public void ClearVerification(UserAccount userAccount)
        {
            userAccount.VerificationKey = null;
            userAccount.VerificationPurpose = null;
            userAccount.VerificationKeySentAt = null;
            userAccount.VerificationStorage = null;
        }

        public async Task ClearVerificationAsync(UserAccount userAccount)
        {
            ClearVerification(userAccount);
            await UpdateUserAccountAsync(userAccount);
        }

        public void SetVerification(UserAccount userAccount,
            string key,
            VerificationKeyPurpose purpose,
            string storage = null,
            DateTime? sentAt = null)
        {
            userAccount.VerificationKey = key.ToLowerInvariant();
            userAccount.VerificationPurpose = (int)purpose;
            userAccount.VerificationKeySentAt = sentAt ?? DateTime.UtcNow;
            userAccount.VerificationStorage = storage;
        }

        public async Task<UserAccount> WriteUserAccountAsync(UserAccount userAccount)
        {
            var now = DateTime.UtcNow;
            userAccount.CreatedAt = now;
            userAccount.UpdatedAt = now;

            var userAccount2 = await _userAccountStore.WriteAsync(userAccount);

            _eventService.RaiseSuccessfulUserAccountCreatedEventAsync(userAccount.Id,
                IdentityServerConstants.LocalIdentityProvider);

            return userAccount2;
        }
        
        public async Task<UserAccount> UpdateUserAccountAsync(UserAccount userAccount)
        {
            // Update user account
            userAccount.UpdatedAt = DateTime.UtcNow; 
            var userAccount2 = await _userAccountStore.WriteAsync(userAccount);

            _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(userAccount.Id);

            return userAccount2;
        }

        public void SetEmailVerified(UserAccount userAccount)
        {
            ClearVerification(userAccount);

            var now = DateTime.UtcNow;
            userAccount.IsLoginAllowed = true;
            userAccount.IsEmailVerified = true;
            userAccount.EmailVerifiedAt = now;
        }

        public async Task SetEmailVerifiedAsync(UserAccount userAccount)
        {
            SetEmailVerified(userAccount);
            await UpdateUserAccountAsync(userAccount);
        }

        /// <summary>
        /// Validate if verification key is valid, if yes it will load a corresponding <see cref="UserAccount"/>
        /// </summary>
        /// <param name="key">Verification key</param>
        /// <param name="purpose"><see cref="VerificationKeyPurpose"/></param>
        /// <returns></returns>
        public async Task<TokenVerificationResult> HandleVerificationKeyAsync(
            string key,
            VerificationKeyPurpose purpose)
        {
            var result = new TokenVerificationResult();
            var userAccount = await _userAccountStore.LoadByVerificationKeyAsync(key.ToLowerInvariant());
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

        public void AddLocalCredentials(UserAccount userAccount, string password)
        {
            var now = DateTime.UtcNow; // TODO: user time service

            // Set user account password
            userAccount.PasswordHash = _crypto.HashPassword(password,
                _applicationOptions.PasswordHashingIterationCount);

            userAccount.PasswordChangedAt = DateTime.UtcNow;
        }

        public async Task AddLocalCredentialsAsync(UserAccount userAccount, string password)
        {
            AddLocalCredentials(userAccount, password); 

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task UpdateLastUsedExternalAccountAsync(UserAccount userAccount, string provider, string subject)
        {
            var externalAccount = userAccount.Accounts.FirstOrDefault(c => c.Provider.Equals(provider) && c.Subject.Equals(subject));

            // TODO: user time service
            var now = DateTime.UtcNow;

            externalAccount.LastLoginAt = now;
            externalAccount.UpdatedAt = now;

            await _userAccountStore.WriteExternalAccountAsync(externalAccount);

            // Emit event
            _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(externalAccount.UserAccountId);
        }

        public async Task<UserAccount> CreateNewExternalUserAccountAsync(string email, string provider, string subject, string returnUrl = null)
        {
            var userAccount = CreateNewLocalUserAccount(email);
            userAccount.Accounts = new ExternalAccount[]
            {
                new ExternalAccount
                {
                    Email = email,
                    UserAccountId = userAccount.Id,
                    Provider = provider,
                    Subject = subject,
                    CreatedAt = userAccount.CreatedAt,
                    UpdatedAt  = userAccount.UpdatedAt,
                    LastLoginAt = null,
                    IsLoginAllowed = true
                }
            };

            return await WriteUserAccountAsync(userAccount);
        }

        public async Task<ExternalAccount> AddExternalAccountAsync(Guid userAccountId, string email, string provider, string subject)
        {
            var now = DateTime.UtcNow; // TODO: user time service
            var externalAccount = await _userAccountStore.WriteExternalAccountAsync(new ExternalAccount
            {
                UserAccountId = userAccountId,
                Email = email,
                Provider = provider,
                Subject = subject,
                CreatedAt = now,
                UpdatedAt = now,
                LastLoginAt = null,
                IsLoginAllowed = true
            });

            // Emit event
            _eventService.RaiseSuccessfulUserAccountUpdatedEventAsync(userAccountId);

            return externalAccount;
        }

        public async Task<PagedList<UserAccount>> LoadInvitedUserAccountsAsync(int take, int skip)
        {
            return await _userAccountStore.LoadInvitedUserAccountsAsync(take, skip);
        }

        /// <summary>
        /// Creates an local user account and sends email to the user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="invitedBy"></param>
        /// <returns><see cref="UserAccount"/></returns>
        public async Task<UserAccount> CreateNewLocalUserAccountAsync(string email, Guid? invitedBy, string returnUrl)
        {
            var foo = CreateNewLocalUserAccount(email);
            foo.CreationKind = CreationKind.Invitation;

            var userAccount = await _userAccountStore.WriteAsync(foo);

            // Emit events
            _eventService.RaiseSuccessfulUserAccountCreatedEventAsync(userAccount.Id, IdentityServerConstants.LocalIdentityProvider);
            _eventService.RaiseSuccessfulUserAccountInvitedEventAsync(userAccount.Id, invitedBy);

            return userAccount;
        }

        /// <summary>
        /// Create verification key and updates the <see cref="UserAccount"/>.
        /// Will also raise a event
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task SetResetPasswordVirificationKeyAsync(UserAccount userAccount, string returnUrl = null)
        {
            // Set verification key
            SetVerification(userAccount,
                _crypto.Hash(_crypto.GenerateSalt()).StripUglyBase64(),
                VerificationKeyPurpose.ResetPassword,
                returnUrl,
                DateTime.UtcNow); // TODO: use time service

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task SetNewPasswordAsync(UserAccount userAccount, string password)
        {
            ClearVerification(userAccount);

            // TODO: reset failed login attempts

            userAccount.PasswordHash = _crypto.HashPassword(password,
                      _applicationOptions.PasswordHashingIterationCount);
            userAccount.PasswordChangedAt = DateTime.UtcNow;

            await UpdateUserAccountAsync(userAccount);
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
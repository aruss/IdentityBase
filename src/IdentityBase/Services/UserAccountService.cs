// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
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

    public class UserAccountService
    {
        private readonly ApplicationOptions applicationOptions;
        private readonly ICrypto crypto;
        private readonly IUserAccountStore userAccountStore;
        private readonly IEventService eventService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserAccountService(
            ApplicationOptions applicationOptions,
            ICrypto crypto,
            IUserAccountStore userAccountStore,
            IEventService eventService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.applicationOptions = applicationOptions;
            this.crypto = crypto;
            this.userAccountStore = userAccountStore;
            this.eventService = eventService;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Used for local authentication
        /// </summary>
        /// <param name="email">Local user account email</param>
        /// <param name="password">Password</param>
        /// <returns><see cref="UserAccountVerificationResult"/></returns>
        public async Task<UserAccountVerificationResult>
            VerifyByEmailAndPasswordAsync(
            string email,
            string password)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            var result = new UserAccountVerificationResult();

            UserAccount userAccount = await userAccountStore
                .LoadByEmailAsync(email.ToLower());

            if (userAccount == null)
            {
                return result;
            }

            if (userAccount.HasPassword())
            {
                result.IsLocalAccount = true;

                result.IsPasswordValid = crypto.VerifyPasswordHash(
                    userAccount.PasswordHash,
                    password,
                    applicationOptions.PasswordHashingIterationCount
                );
            }

            result.UserAccount = userAccount;
            result.IsLoginAllowed = userAccount.IsLoginAllowed;
            result.NeedChangePassword = false;

            if (!result.IsPasswordValid && !result.IsLocalAccount)
            {
                string[] hints = userAccount.Accounts
                    .Select(s => s.Provider).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Loads user by its primary key 
        /// </summary>
        /// <param name="id">UserId</param>
        /// <returns></returns>
        public async Task<UserAccount> LoadByIdAsync(
            Guid id)
        {
            return await userAccountStore.LoadByIdAsync(id);
        }

        /// <summary>
        /// Loads user by email 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<UserAccount> LoadByEmailAsync(string email)
        {
            return await userAccountStore.LoadByEmailAsync(email);
        }

        /// <summary>
        /// Loads user by email and with all external accounts 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<UserAccount> LoadByEmailWithExternalAsync(
            string email)
        {
            return await userAccountStore.LoadByEmailWithExternalAsync(email);
        }

        /// <summary>
        /// Get by external provider information, 
        /// </summary>
        /// <param name="provider">Provider name (facebook, twitter, ...)</param>
        /// <param name="subject">Provider account ID</param>
        /// <returns></returns>
        public async Task<UserAccount> LoadByExternalProviderAsync(
            string provider,
            string subject)
        {
            return await userAccountStore
                .LoadByExternalProviderAsync(provider, subject);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            await userAccountStore.DeleteByIdAsync(id);
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

        public UserAccount CreateNewLocalUserAccount(
            string email,
            string password = null,
            string returnUrl = null)
        {
            DateTime now = DateTime.UtcNow;

            UserAccount userAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = email,
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsLoginAllowed = applicationOptions
                    .RequireLocalAccountVerification,
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (!String.IsNullOrWhiteSpace(password))
            {
                userAccount.PasswordHash = crypto.HashPassword(
                    password,
                    applicationOptions.PasswordHashingIterationCount
                );
            }

            if (applicationOptions.RequireLocalAccountVerification &&
                !String.IsNullOrWhiteSpace(returnUrl))
            {
                this.SetVerification(
                    userAccount,
                    VerificationKeyPurpose.ConfirmAccount,
                    returnUrl,
                    now
                );
            }

            return userAccount;
        }

        public async Task<UserAccount> CreateNewLocalUserAccountAsync(
            string email,
            string password,
            string returnUrl = null)
        {
            UserAccount userAccount = this.CreateNewLocalUserAccount(
                email,
                password,
                returnUrl
            );

            return await this.WriteUserAccountAsync(userAccount);
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
            this.ClearVerification(userAccount);
            await UpdateUserAccountAsync(userAccount);
        }

        public void SetVerification(
            UserAccount userAccount,
            VerificationKeyPurpose purpose,
            string storage = null,
            DateTime? sentAt = null)
        {
            userAccount.VerificationKey = crypto
                .Hash(crypto.GenerateSalt())
                .StripUglyBase64()
                .ToLowerInvariant();

            userAccount.VerificationPurpose = (int)purpose;
            userAccount.VerificationKeySentAt = sentAt ?? DateTime.UtcNow;
            userAccount.VerificationStorage = storage;
        }

        public async Task<UserAccount> WriteUserAccountAsync(
            UserAccount userAccount)
        {
            DateTime now = DateTime.UtcNow;
            userAccount.CreatedAt = now;
            userAccount.UpdatedAt = now;

            UserAccount userAccount2 = await userAccountStore
                .WriteAsync(userAccount);

            await eventService.RaiseUserAccountCreatedSuccessEventAsync(
                userAccount,
                IdentityServerConstants.LocalIdentityProvider);

            return userAccount2;
        }

        public async Task<UserAccount> UpdateUserAccountAsync(
            UserAccount userAccount)
        {
            // Update user account
            userAccount.UpdatedAt = DateTime.UtcNow;

            UserAccount userAccount2 = await userAccountStore
                .WriteAsync(userAccount);

            await eventService
                .RaiseUserAccountUpdatedSuccessEventAsync(userAccount);

            return userAccount2;
        }

        public void SetEmailVerified(UserAccount userAccount)
        {
            ClearVerification(userAccount);

            DateTime now = DateTime.UtcNow;
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
        /// Validate if verification key is valid, if yes it will load a
        /// corresponding <see cref="UserAccount"/>
        /// </summary>
        /// <param name="key">Verification key</param>
        /// <param name="purpose"><see cref="VerificationKeyPurpose"/></param>
        /// <returns></returns>
        public async Task<TokenVerificationResult> HandleVerificationKeyAsync(
            string key,
            VerificationKeyPurpose purpose)
        {
            var result = new TokenVerificationResult();

            var userAccount = await userAccountStore
                .LoadByVerificationKeyAsync(key.ToLowerInvariant());

            if (userAccount == null)
            {
                return result;
            }

            result.UserAccount = userAccount;

            result.PurposeValid = userAccount
                .VerificationPurpose == (int)purpose;

            if (userAccount.VerificationKeySentAt.HasValue)
            {
                var validTill = userAccount.VerificationKeySentAt.Value +
                    TimeSpan.FromSeconds(
                        applicationOptions.VerificationKeyLifetime
                    );

                var now = DateTime.Now;

                result.TokenExpired = now > validTill;
            }

            return result;
        }

        public void AddLocalCredentials(
            UserAccount userAccount,
            string password)
        {
            var now = DateTime.UtcNow; // TODO: user time service

            // Set user account password
            userAccount.PasswordHash = crypto.HashPassword(password,
                applicationOptions.PasswordHashingIterationCount);

            userAccount.PasswordChangedAt = DateTime.UtcNow;
        }

        public async Task AddLocalCredentialsAsync(
            UserAccount userAccount,
            string password)
        {
            AddLocalCredentials(userAccount, password);

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task UpdateLastUsedExternalAccountAsync(
            UserAccount userAccount,
            string provider,
            string subject)
        {
            var externalAccount = userAccount.Accounts
                .FirstOrDefault(c =>
                    c.Provider.Equals(provider) &&
                    c.Subject.Equals(subject)
                );

            // TODO: user time service
            var now = DateTime.UtcNow;

            externalAccount.LastLoginAt = now;
            externalAccount.UpdatedAt = now;

            await userAccountStore.WriteExternalAccountAsync(externalAccount);

            // Emit event
            await eventService.RaiseUserAccountUpdatedSuccessEventAsync(
                userAccount);
        }

        public async Task<UserAccount> CreateNewExternalUserAccountAsync(
            string email,
            string provider,
            string subject,
            string returnUrl = null)
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

        public async Task<ExternalAccount> AddExternalAccountAsync(
            UserAccount userAccount,
            string email,
            string provider,
            string subject)
        {
            var now = DateTime.UtcNow; // TODO: user time service
            var externalAccount = await userAccountStore
                .WriteExternalAccountAsync(new ExternalAccount
                {
                    UserAccountId = userAccount.Id,
                    Email = email,
                    Provider = provider,
                    Subject = subject,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastLoginAt = null,
                    IsLoginAllowed = true
                });

            // Emit event
            await eventService.RaiseUserAccountUpdatedSuccessEventAsync(userAccount);

            return externalAccount;
        }

        public async Task<PagedList<UserAccount>> LoadInvitedUserAccountsAsync(
            int take,
            int skip)
        {
            return await userAccountStore
                .LoadInvitedUserAccountsAsync(take, skip);
        }

        /// <summary>
        /// Creates an local user account and sends email to the user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="invitedBy"></param>
        /// <returns><see cref="UserAccount"/></returns>
        public async Task<UserAccount> CreateNewLocalUserAccountAsync(
            string email,
            string returnUrl,
            UserAccount invitedByUserAccount)
        {
            var foo = CreateNewLocalUserAccount(email);
            foo.CreationKind = CreationKind.Invitation;
            SetVerification(foo, VerificationKeyPurpose.ConfirmAccount, returnUrl);

            var userAccount = await userAccountStore.WriteAsync(foo);

            // Emit events
            await eventService.RaiseUserAccountCreatedSuccessEventAsync(
                userAccount, IdentityServerConstants.LocalIdentityProvider);

            await eventService.RaiseUserAccountInvitedSuccessEventAsync(
                userAccount, invitedByUserAccount);

            return userAccount;
        }

        /// <summary>
        /// Create verification key and updates the <see cref="UserAccount"/>.
        /// Will also raise a event
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task SetResetPasswordVirificationKeyAsync(
            UserAccount userAccount,
            string returnUrl = null)
        {
            // Set verification key
            SetVerification(userAccount,
                VerificationKeyPurpose.ResetPassword,
                returnUrl,
                DateTime.UtcNow); // TODO: use time service

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task SetNewPasswordAsync(
            UserAccount userAccount,
            string password)
        {
            ClearVerification(userAccount);

            // TODO: reset failed login attempts

            userAccount.PasswordHash = crypto.HashPassword(password,
                      applicationOptions.PasswordHashingIterationCount);
            userAccount.PasswordChangedAt = DateTime.UtcNow;

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task SetEmailChangeVirificationKeyAsync(
            UserAccount userAccount,
            string email, 
            string returnUrl = null)
        {
            // TODO: Move to verification storage reader or something
            string storage = Newtonsoft.Json.JsonConvert
                .SerializeObject(new string[] { email, returnUrl }); 
            
            // Set verification key
            this.SetVerification(userAccount,
                VerificationKeyPurpose.ChangeEmail,
                storage,
                DateTime.UtcNow); // TODO: use time service

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task SetNewEmailAsync(
            UserAccount userAccount,
            string email)
        {
            ClearVerification(userAccount);
            
            userAccount.Email = email; 

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
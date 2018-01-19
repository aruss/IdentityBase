// Copyright (c) Russlan Akiev. All rights reserved. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.Events;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Http;
    using ServiceBase;
    using ServiceBase.Collections;
    using ServiceBase.Events;

    public class UserAccountService
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ICrypto _crypto;
        private readonly IUserAccountStore _userAccountStore;
        private readonly IEventService _eventService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDateTimeAccessor _dateTimeAccessor;

        public UserAccountService(
            ApplicationOptions applicationOptions,
            ICrypto crypto,
            IUserAccountStore userAccountStore,
            IEventService eventService,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeAccessor dateTimeAccessor)
        {
            this._applicationOptions = applicationOptions;
            this._crypto = crypto;
            this._userAccountStore = userAccountStore;
            this._eventService = eventService;
            this._httpContextAccessor = httpContextAccessor;
            this._dateTimeAccessor = dateTimeAccessor;
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

            UserAccount userAccount = await this._userAccountStore
                .LoadByEmailAsync(email);

            if (userAccount == null)
            {
                return result;
            }

            if (userAccount.HasPassword())
            {
                result.IsLocalAccount = true;

                result.IsPasswordValid = this.VerifiyPasswordHash(
                    userAccount, password);
            }

            result.UserAccount = userAccount;
            result.IsLoginAllowed = userAccount.IsLoginAllowed;

            // TODO: validate if user need a password change, eg. time expired
            // or explicit flag is set.
            result.NeedChangePassword = false;

            // In case user tries to login and has external accounts
            //if (!result.IsPasswordValid && !result.IsLocalAccount)
            //{
            //    result.Hints = userAccount.Accounts
            //        .Select(s => s.Provider).ToArray();
            //}

            return result;
        }

        private bool VerifiyPasswordHash(UserAccount userAccount, string password)
        {
            return this._crypto.VerifyPasswordHash(
                userAccount.PasswordHash,
                password,
                this._applicationOptions.PasswordHashingIterationCount
            );
        }

        /// <summary>
        /// Loads <see cref="UserAccount"/> by ID.
        /// </summary>
        /// <param name="id">UserAccount ID.</param>
        /// <returns>Instance of <see cref="UserAccount"/>.</returns>
        public async Task<UserAccount> LoadByIdAsync(Guid id)
        {
            return await this._userAccountStore.LoadByIdAsync(id);
        }

        /// <summary>
        /// Loads <see cref="UserAccount"/> by email address.
        /// </summary>
        /// <param name="id">UserAccount email address.</param>
        /// <returns>Instance of <see cref="UserAccount"/>.</returns>
        public async Task<UserAccount> LoadByEmailAsync(string email)
        {
            return await this._userAccountStore
                .LoadByEmailAsync(email);
        }

        /// <summary>
        /// Loads <see cref="UserAccount"/> by email and with all external
        /// accounts.
        /// </summary>
        /// <param name="id">UserAccount email address.</param>
        /// <returns>Instance of <see cref="UserAccount"/>.</returns>
        public async Task<UserAccount> LoadByEmailWithExternalAsync(
            string email)
        {
            return await this._userAccountStore
                .LoadByEmailWithExternalAsync(email);
        }

        /// <summary>
        /// Loads <see cref="UserAccount"/> by external provider information.
        /// </summary>
        /// <param name="provider">Provider name (facebook, twitter, ...)</param>
        /// <param name="subject">Provider account ID</param>
        /// <returns>Instance of <see cref="UserAccount"/>.</returns>
        public async Task<UserAccount> LoadByExternalProviderAsync(
            string provider,
            string subject)
        {
            return await this._userAccountStore
                .LoadByExternalProviderAsync(provider, subject);
        }

        /// <summary>
        /// Perceives the successfull <see cref="UserAccount"/> login.
        /// It will clear confirmation data and failed login attemps.
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        public async Task PerceiveSuccessfulLoginAsync(UserAccount userAccount)
        {
            userAccount.LastLoginAt = this._dateTimeAccessor.UtcNow;

            this.ResetFailedLoginAttempts(userAccount);

            await this._eventService
                .RaiseUserAccountLoginSuccessEventAsync(userAccount);

            await this.UpdateUserAccountAsync(userAccount);
        }

        /// <summary>
        /// Updates the <see cref="UserAccount"/> and emits the
        /// <see cref="UserAccountUpdatedSuccessEvent"/>.
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount">The instance of <see cref="UserAccount"/>.</param>
        /// <returns>The instance of updated <see cref="UserAccount"/>.</returns>
        public async Task<UserAccount> UpdateUserAccountAsync(
           UserAccount userAccount)
        {
            // TODO: move to db layer interceptor
            userAccount.UpdatedAt = this._dateTimeAccessor.UtcNow;

            return await this._userAccountStore.WriteAsync(userAccount);
        }

        /// <summary>
        /// Resets info about failed logins.
        /// </summary>
        /// <param name="userAccount"></param>
        public void ResetFailedLoginAttempts(UserAccount userAccount)
        {
            userAccount.FailedLoginCount = 0;
            userAccount.LastFailedLoginAt = null;
        }

        /// <summary>
        /// Count up failed logins.
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>
        public async Task PerceiveFailedLoginAsync(UserAccount userAccount)
        {
            userAccount.FailedLoginCount++;
            userAccount.LastFailedLoginAt = this._dateTimeAccessor.UtcNow;

            await this.UpdateUserAccountAsync(userAccount);
        }

        /// <summary>
        /// Creates a verification key and stores data on user object.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="purpose"></param>
        /// <param name="storage"></param>
        /// <param name="sentAt"></param>
        public void SetVerificationData(
            UserAccount userAccount,
            VerificationKeyPurpose purpose,
            string storage = null,
            DateTime? sentAt = null)
        {
            userAccount.VerificationKey = this._crypto
                .Hash(this._crypto.GenerateSalt())
                .StripUglyBase64()
                .ToLowerInvariant();

            userAccount.VerificationPurpose = (int)purpose;
            userAccount.VerificationStorage = storage;

            userAccount.VerificationKeySentAt = sentAt ??
                this._dateTimeAccessor.UtcNow;
        }

        /// <summary>
        /// Resets verification data. 
        /// </summary>
        /// <param name="userAccount"></param>
        public void ClearVerificationData(UserAccount userAccount)
        {
            userAccount.VerificationKey = null;
            userAccount.VerificationPurpose = null;
            userAccount.VerificationKeySentAt = null;
            userAccount.VerificationStorage = null;
        }

        /// <summary>
        /// Create verification key and updates the <see cref="UserAccount"/>.
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task SetVirificationDataForResetPasswordAsync(
            UserAccount userAccount,
            string returnUrl = null)
        {
            // Set verification key
            this.SetVerificationData(userAccount,
                VerificationKeyPurpose.ResetPassword,
                returnUrl);

            await this.UpdateUserAccountAsync(userAccount);
        }

        /// <summary>
        /// Set new password for user account
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task SetNewPasswordAsync(
          UserAccount userAccount,
          string password)
        {
            this.ClearVerificationData(userAccount);

            userAccount.PasswordHash = this._crypto.HashPassword(password,
                      this._applicationOptions.PasswordHashingIterationCount);

            userAccount.PasswordChangedAt = this._dateTimeAccessor.UtcNow;

            await this.UpdateUserAccountAsync(userAccount);
        }

                








        public async Task ClearVerificationAsync(UserAccount userAccount)
        {
            this.ClearVerificationData(userAccount);
            await UpdateUserAccountAsync(userAccount);
        }

        // TODO: allow deletion of invitations only 
        public async Task DeleteByIdAsync(Guid id)
        {
            await this._userAccountStore.DeleteByIdAsync(id);
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
                IsLoginAllowed = this._applicationOptions
                    .RequireLocalAccountVerification,
                PasswordChangedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (!String.IsNullOrWhiteSpace(password))
            {
                userAccount.PasswordHash = this._crypto.HashPassword(
                    password,
                    this._applicationOptions.PasswordHashingIterationCount
                );
            }

            if (this._applicationOptions.RequireLocalAccountVerification &&
                !String.IsNullOrWhiteSpace(returnUrl))
            {
                this.SetVerificationData(
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

        public async Task<UserAccount> WriteUserAccountAsync(
                    UserAccount userAccount)
        {
            DateTime now = DateTime.UtcNow;
            userAccount.CreatedAt = now;
            userAccount.UpdatedAt = now;

            UserAccount userAccount2 = await this._userAccountStore
                .WriteAsync(userAccount);

            await this._eventService.RaiseUserAccountCreatedSuccessEventAsync(
                userAccount,
                IdentityServer4.IdentityServerConstants.LocalIdentityProvider);

            return userAccount2;
        }


        public void SetEmailVerified(UserAccount userAccount)
        {
            ClearVerificationData(userAccount);

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

            var userAccount = await this._userAccountStore
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
                        this._applicationOptions.VerificationKeyLifetime
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
            userAccount.PasswordHash = this._crypto.HashPassword(password,
                this._applicationOptions.PasswordHashingIterationCount);

            userAccount.PasswordChangedAt = DateTime.UtcNow;
        }

        public async Task AddLocalCredentialsAsync(
            UserAccount userAccount,
            string password)
        {
            AddLocalCredentials(userAccount, password);

            await UpdateUserAccountAsync(userAccount);
        }

        // public async Task UpdateLastUsedExternalAccountAsync(
        //     UserAccount userAccount,
        //     string provider,
        //     string subject)
        // {
        //     var externalAccount = userAccount.Accounts
        //         .FirstOrDefault(c =>
        //             c.Provider.Equals(provider) &&
        //             c.Subject.Equals(subject)
        //         );
        // 
        //     // TODO: user time service
        //     var now = DateTime.UtcNow;
        // 
        //     externalAccount.LastLoginAt = now;
        //     externalAccount.UpdatedAt = now;
        // 
        //     await this._userAccountStore.WriteExternalAccountAsync(externalAccount);
        // 
        //     // Emit event
        //     await this._eventService.RaiseUserAccountUpdatedSuccessEventAsync(
        //         userAccount);
        // }

        //public async Task<UserAccount> CreateNewExternalUserAccountAsync(
        //    string email,
        //    string provider,
        //    string subject,
        //    string returnUrl = null)
        //{
        //    var userAccount = CreateNewLocalUserAccount(email);
        //    userAccount.Accounts = new ExternalAccount[]
        //    {
        //        new ExternalAccount
        //        {
        //            Email = email,
        //            UserAccountId = userAccount.Id,
        //            Provider = provider,
        //            Subject = subject,
        //            CreatedAt = userAccount.CreatedAt,
        //            UpdatedAt  = userAccount.UpdatedAt,
        //            LastLoginAt = null,
        //            IsLoginAllowed = true
        //        }
        //    };
        //
        //    return await WriteUserAccountAsync(userAccount);
        //}

        //public async Task<ExternalAccount> AddExternalAccountAsync(
        //    UserAccount userAccount,
        //    string email,
        //    string provider,
        //    string subject)
        //{
        //    var now = DateTime.UtcNow; // TODO: user time service
        //    var externalAccount = await this._userAccountStore
        //        .WriteExternalAccountAsync(new ExternalAccount
        //        {
        //            UserAccountId = userAccount.Id,
        //            Email = email,
        //            Provider = provider,
        //            Subject = subject,
        //            CreatedAt = now,
        //            UpdatedAt = now,
        //            LastLoginAt = null,
        //            IsLoginAllowed = true
        //        });
        //
        //    // Emit event
        //    await this._eventService
        //        .RaiseUserAccountUpdatedSuccessEventAsync(userAccount);
        //
        //    return externalAccount;
        //}

        public async Task<PagedList<UserAccount>> LoadInvitedUserAccountsAsync(
            int take,
            int skip)
        {
            return await this._userAccountStore
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
            UserAccount userAccount = this.CreateNewLocalUserAccount(email);
            userAccount.CreationKind = CreationKind.Invitation;

            this.SetVerificationData(userAccount,
                VerificationKeyPurpose.ConfirmAccount, returnUrl);

            userAccount = await this._userAccountStore.WriteAsync(userAccount);

            // Emit events
            await this._eventService.RaiseUserAccountCreatedSuccessEventAsync(
                userAccount,
                IdentityServer4.IdentityServerConstants.LocalIdentityProvider);

            await this._eventService.RaiseUserAccountInvitedSuccessEventAsync(
                userAccount,
                invitedByUserAccount);

            return userAccount;
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
            this.SetVerificationData(userAccount,
                VerificationKeyPurpose.ChangeEmail,
                storage,
                DateTime.UtcNow); // TODO: use time service

            await UpdateUserAccountAsync(userAccount);
        }

        public async Task SetNewEmailAsync(
            UserAccount userAccount,
            string email)
        {
            ClearVerificationData(userAccount);

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
        public string[] Hints { get; internal set; }
    }
}
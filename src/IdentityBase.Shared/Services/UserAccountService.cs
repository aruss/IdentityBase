// Copyright (c) Russlan Akiev. All rights reserved. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.Models;
    using ServiceBase.Extensions;

    public class UserAccountService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IUserAccountStore _userAccountStore;
        private readonly ApplicationOptions _applicationOptions;

        public UserAccountService(
            ICryptoService cryptoService,
            IUserAccountStore userAccountStore,
            ApplicationOptions applicationOptions)
        {
            this._cryptoService = cryptoService;
            this._userAccountStore = userAccountStore;
            this._applicationOptions = applicationOptions;
        }

        /// Set new password for user account
        /// This method will write to persistent store.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public void SetPassword(UserAccount userAccount, string password)
        {
            this.ClearVerificationData(userAccount);

            userAccount.PasswordHash = this._cryptoService.HashPassword(
                password,
                this._applicationOptions.PasswordHashingIterationCount
            );

            userAccount.PasswordChangedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Check ifs password is valid 
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPasswordValid(string hash, string password)
        {
            return this._cryptoService.VerifyPasswordHash(
                    hash,
                    password,
                    this._applicationOptions.PasswordHashingIterationCount);
        }

        /// <summary>
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
            userAccount.VerificationKey = this._cryptoService
                .Hash(this._cryptoService.GenerateSalt())
                .StripUglyBase64()
                .ToLowerInvariant();

            userAccount.VerificationPurpose = (int)purpose;
            userAccount.VerificationStorage = storage;
            userAccount.VerificationKeySentAt = sentAt ?? DateTime.UtcNow;
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

        public void SetEmailAsVerified(UserAccount userAccount)
        {
            this.ClearVerificationData(userAccount);

            userAccount.IsActive = true;
            userAccount.IsEmailVerified = true;
            userAccount.EmailVerifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validate if verification key is valid, if yes it will load a
        /// corresponding <see cref="UserAccount"/>
        /// </summary>
        /// <param name="key">Verification key</param>
        /// <param name="purpose"><see cref="VerificationKeyPurpose"/></param>
        /// <returns></returns>
        public async Task<TokenVerificationResult> GetVerificationResultAsync(
            string key,
            VerificationKeyPurpose purpose)
        {
            var result = new TokenVerificationResult();

            UserAccount userAccount = await this._userAccountStore
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
                DateTime validTill = userAccount.VerificationKeySentAt.Value +
                    TimeSpan.FromSeconds(
                        this._applicationOptions.VerificationKeyLifetime
                    );

                result.TokenExpired = DateTime.Now > validTill;
            }

            return result;
        }

        public void SetSuccessfullSignIn(UserAccount userAccount)
        {
            userAccount.FailedLoginCount = 0;
            userAccount.LastFailedLoginAt = null;
            userAccount.IsActive = true;
            userAccount.LastLoginAt = DateTime.UtcNow;
        }

        public void SetFailedSignIn(UserAccount userAccount)
        {
            userAccount.FailedLoginCount++;
            userAccount.LastFailedLoginAt = DateTime.UtcNow;

            if (userAccount.FailedLoginCount >=
                this._applicationOptions.AccountLockoutFailedLoginAttempts)
            {
                userAccount.IsActive = false;
            }
        }
    }
}
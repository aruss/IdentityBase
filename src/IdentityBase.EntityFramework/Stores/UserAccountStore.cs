namespace IdentityBase.EntityFramework.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.EntityFramework.DbContexts;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ServiceBase;
    using ServiceBase.Collections;

    // TODO: make use of value type  System.Security.Claims.ClaimValueTypes
    // while create UserClaim
    // http://www.npgsql.org/doc/faq.html

    public class UserAccountStore : IUserAccountStore
    {
        private readonly UserAccountDbContext _context;
        private readonly ILogger<UserAccountStore> _logger;

        public UserAccountStore(
            UserAccountDbContext context,
            ILogger<UserAccountStore> logger)
        {
            this._context = context ?? throw
                new ArgumentNullException(nameof(context));

            this._logger = logger;
        }

        public Task DeleteByIdAsync(Guid id)
        {
            var userAccount = this._context.UserAccounts
             //.Include(x => x.Accounts)
             //.Include(x => x.Claims)
             .FirstOrDefault(x => x.Id == id);

            if (userAccount != null)
            {
                this._context.Remove(userAccount);
                this._context.SaveChanges();
            }

            return Task.FromResult(0);
        }

        public Task DeleteExternalAccountAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserAccount> LoadByEmailAsync(string email)
        {
            var userAccount = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefault(x => x.Email
                    .Equals(email, StringComparison.OrdinalIgnoreCase));

            var model = userAccount?.ToModel();

            this._logger.LogDebug(
                "{email} found in database: {userAccountFound}",
                email,
                model != null);

            return Task.FromResult(model);
        }

        public Task<UserAccount> LoadByEmailWithExternalAsync(string email)
        {
            var userAccount = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefault(x => x.Email
                    .Equals(email, StringComparison.OrdinalIgnoreCase) ||
                         x.Accounts.Any(c => c.Email
                        .Equals(email, StringComparison.OrdinalIgnoreCase)));

            var model = userAccount?.ToModel();

            this._logger.LogDebug(
                "{email} found in database: {userAccountFound}",
                email,
                model != null);

            return Task.FromResult(model);
        }

        public Task<UserAccount> LoadByExternalProviderAsync(
            string provider,
            string subject)
        {
            var userAccount = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefault(x =>
                    x.Accounts.Any(c => c.Provider.Equals(
                        provider, StringComparison.OrdinalIgnoreCase)) ||
                    x.Accounts.Any(c => c.Subject.Equals(
                        subject, StringComparison.OrdinalIgnoreCase)));

            var model = userAccount?.ToModel();

            this._logger.LogDebug(
                "{provider}, {subject} found in database: {userAccountFound}",
                provider,
                subject,
                model != null);

            return Task.FromResult(model);
        }

        public Task<UserAccount> LoadByIdAsync(Guid id)
        {
            var userAccount = this._context.UserAccounts
               .Include(x => x.Accounts)
               .Include(x => x.Claims)
               .FirstOrDefault(x => x.Id == id);

            var model = userAccount?.ToModel();

            this._logger.LogDebug(
                "{id} found in database: {userAccountFound}",
                id,
                model != null);

            return Task.FromResult(model);
        }

        public Task<UserAccount> LoadByVerificationKeyAsync(string key)
        {
            var userAccount = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefault(x => x.VerificationKey == key);

            var model = userAccount?.ToModel();

            this._logger.LogDebug(
                "{key} found in database: {userAccountFound}",
                key, model != null);

            return Task.FromResult(model);
        }

        public Task<UserAccount> WriteAsync(UserAccount userAccount)
        {
            if (userAccount == null) throw
                    new ArgumentNullException(nameof(userAccount));

            var userAccountEntity = this._context.UserAccounts
                .SingleOrDefault(x => x.Id == userAccount.Id);

            if (userAccountEntity == null)
            {
                this._logger.LogDebug(
                    "{userAccountId} not found in database",
                    userAccount.Id);

                userAccountEntity = this._context.UserAccounts
                    .Add(userAccount.ToEntity()).Entity;
            }
            else
            {
                this._logger.LogDebug(
                    "{userAccountId} found in database",
                    userAccount.Id);

                userAccount.UpdateEntity(userAccountEntity);

                // HACK:
                // EF, Automapper exception, “Attaching an entity of type …
                // failed because another entity of the same type already has
                // the same primary key value”
                userAccountEntity.Claims = null;
                userAccountEntity.Accounts = null;
            }

            try
            {
                this._context.SaveChanges();
                return Task.FromResult(userAccountEntity.ToModel());
            }
            catch (Exception ex)
            {
                this._logger.LogError(0, ex, "Exception storing user account");
            }

            return Task.FromResult<UserAccount>(null);
        }

        public Task<ExternalAccount> WriteExternalAccountAsync(
            ExternalAccount externalAccount)
        {
            var userAccountId = externalAccount.UserAccount != null ?
                externalAccount.UserAccount.Id : externalAccount.UserAccountId;
            var userAccountEntity = this._context.UserAccounts
                .SingleOrDefault(x => x.Id == userAccountId);

            if (userAccountEntity == null)
            {
                this._logger.LogError(
                    "{existingUserAccountId} not found in database",

                    userAccountId);
                return null;
            }

            var externalAccountEntity = this._context.ExternalAccounts
                .SingleOrDefault(x =>
                    x.Provider == externalAccount.Provider &&
                    x.Subject == externalAccount.Subject);

            if (externalAccountEntity == null)
            {
                this._logger.LogDebug("{0} {1} not found in database",
                    externalAccount.Provider, externalAccount.Subject);

                externalAccountEntity = externalAccount.ToEntity();
                this._context.ExternalAccounts.Add(externalAccountEntity);
            }
            else
            {
                this._logger.LogDebug("{0} {1} found in database",
                    externalAccountEntity.Provider,
                    externalAccountEntity.Subject);

                externalAccount.UpdateEntity(externalAccountEntity);
            }

            try
            {
                this._context.SaveChanges();
                return Task.FromResult(externalAccountEntity.ToModel());
            }
            catch (Exception ex)
            {
                this._logger.LogError(0, ex, "Exception storing external account");
            }

            return Task.FromResult<ExternalAccount>(null);
        }

        public async Task<PagedList<UserAccount>> LoadInvitedUserAccountsAsync(
            int take,
            int skip = 0,
            Guid? invitedBy = null)
        {
            var baseQuery = this._context.UserAccounts
                .Where(c => c.CreationKind == (int)CreationKind.Invitation);

            if (invitedBy.HasValue)
            {
                baseQuery = baseQuery.Where(c => c.CreatedBy == invitedBy);
            }

            var result = new PagedList<UserAccount>
            {
                Skip = skip,
                Take = take,
                Total = baseQuery.Count(),
                Items = baseQuery
                     .Include(x => x.Accounts)
                     .Include(x => x.Claims)
                     .OrderByDescending(c => c.CreatedAt)
                     .Skip(skip)
                     .Take(take)
                     .Select(s => s.ToModel()).ToArray(),
                Sort = new List<SortInfo>
                {
                    new SortInfo("CreatedAt".Camelize(),
                        SortDirection.Descending)
                }
            };

            //_logger.LogDebug("{email} found in database: {userAccountFound}",
            //    email,
            //    model != null);

            return result;
        }
    }
}
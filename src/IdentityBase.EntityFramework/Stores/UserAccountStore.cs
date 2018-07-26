namespace IdentityBase.EntityFramework.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.DbContexts;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ServiceBase;
    using ServiceBase.Collections;
    using ServiceBase.Extensions;
    using UserAccountEntity = Entities.UserAccount;

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

            this._logger = logger ?? throw
                new ArgumentNullException(nameof(logger));
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            UserAccountEntity entity = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefault(x => x.Id == id);

            if (entity != null)
            {
                this._context.Remove(entity);
                await this._context.SaveChangesAsync();
            }
        }

        public async Task<UserAccount> LoadByEmailAsync(string email)
        {
            UserAccountEntity entity = await this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefaultAsync(x => x.Email
                    .Equals(email, StringComparison.OrdinalIgnoreCase));

            UserAccount model = entity?.ToModel();

            this._logger.LogDebug(
                "{email} found in database: {userAccountFound}",
                email,
                model != null);

            return model;
        }

        /*public async Task<UserAccount> LoadByEmailWithExternalAsync(
            string email)
        {
            UserAccountEntity entity = await this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefaultAsync(x => x.Email
                    .Equals(email, StringComparison.OrdinalIgnoreCase) ||
                         x.Accounts.Any(c => c.Email
                        .Equals(email, StringComparison.OrdinalIgnoreCase)));

            UserAccount model = entity?.ToModel();

            this._logger.LogDebug(
                "{email} found in database: {userAccountFound}",
                email,
                model != null);

            return model;
        }*/

        public async Task<UserAccount> LoadByExternalProviderAsync(
            string provider,
            string subject)
        {
            UserAccountEntity entity = await this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefaultAsync(x =>
                    x.Accounts.Any(c => c.Provider.Equals(
                        provider, StringComparison.OrdinalIgnoreCase)) &&
                    x.Accounts.Any(c => c.Subject.Equals(
                        subject, StringComparison.OrdinalIgnoreCase)));

            UserAccount model = entity?.ToModel();

            this._logger.LogDebug(
                "{provider}, {subject} found in database: {userAccountFound}",
                provider,
                subject,
                model != null);

            return model;
        }

        public async Task<UserAccount> LoadByIdAsync(Guid id)
        {
            UserAccountEntity entity = await this._context.UserAccounts
               .Include(x => x.Accounts)
               .Include(x => x.Claims)
               .FirstOrDefaultAsync(x => x.Id == id);

            UserAccount model = entity?.ToModel();

            this._logger.LogDebug(
                "{id} found in database: {userAccountFound}",
                id,
                model != null);

            return model;
        }

        public async Task<UserAccount> LoadByVerificationKeyAsync(string key)
        {
            UserAccountEntity entity = await this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .FirstOrDefaultAsync(x => x.VerificationKey == key);

            UserAccount model = entity?.ToModel();

            this._logger.LogDebug(
                "{key} found in database: {userAccountFound}",
                key, model != null);

            return model;
        }

        public async Task<UserAccount> WriteAsync(UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            UserAccountEntity entity = this._context.UserAccounts
                .Include(x => x.Accounts)
                .Include(x => x.Claims)
                .SingleOrDefault(x => x.Id == userAccount.Id);

            if (entity == null)
            {
                this._logger.LogDebug(
                    "{userAccountId} not found in database",
                    userAccount.Id);

                entity = this._context.UserAccounts
                    .Add(userAccount.ToEntity()).Entity;
            }
            else
            {
                this._logger.LogDebug(
                    "{userAccountId} found in database",
                    userAccount.Id);

                // Update parent
                UserAccountEntity entityUpdated = userAccount.ToEntity();

                this._context.Entry(entity)
                    .CurrentValues.SetValues(entityUpdated);

                this.UpdateExternalAccounts(entity, entityUpdated);
                this.UpdateUserAccountClaims(entity, entityUpdated); 
            }

            await this._context.SaveChangesAsync();
            return entity.ToModel();
        }

        private void UpdateExternalAccounts(
            UserAccountEntity entity,
            UserAccountEntity entityUpdated)
        {
            var (removed, added, updated) =
                entity.Accounts.Diff(entityUpdated.Accounts);

            foreach (var item in removed)
            {
                this._context.ExternalAccounts.Remove(item);
                entity.Accounts.Remove(item);
            }

            foreach (var item in added)
            {
                this._context.ExternalAccounts.Add(item);
                entity.Accounts.Add(item);
            }

            foreach (var item in updated)
            {
                this._context.Entry(
                    entity.Accounts.FirstOrDefault(c => c.Equals(item))
                ).CurrentValues.SetValues(item);
            }
        }

        private void UpdateUserAccountClaims(
            UserAccountEntity entity,
            UserAccountEntity entityUpdated)
        {
            var (removed, added, updated) =
                entity.Claims.Diff(entityUpdated.Claims);

            foreach (var item in removed)
            {
                this._context.UserAccountClaims.Remove(item);
                entity.Claims.Remove(item);
            }

            foreach (var item in added)
            {
                this._context.UserAccountClaims.Add(item);
                entity.Claims.Add(item);
            }

            foreach (var item in updated)
            {
                this._context.Entry(
                    entity.Claims.FirstOrDefault(c => c.Equals(item))
                ).CurrentValues.SetValues(item);
            }
        }

        //public Task<ExternalAccount> WriteExternalAccountAsync(
        //    ExternalAccount externalAccount)
        //{
        //    var userAccountId = externalAccount.UserAccount != null ?
        //        externalAccount.UserAccount.Id : externalAccount.UserAccountId;
        //    var userAccountEntity = this._context.UserAccounts
        //        .SingleOrDefault(x => x.Id == userAccountId);
        //
        //    if (userAccountEntity == null)
        //    {
        //        this._logger.LogError(
        //            "{existingUserAccountId} not found in database",
        //
        //            userAccountId);
        //        return null;
        //    }
        //
        //    var externalAccountEntity = this._context.ExternalAccounts
        //        .SingleOrDefault(x =>
        //            x.Provider == externalAccount.Provider &&
        //            x.Subject == externalAccount.Subject);
        //
        //    if (externalAccountEntity == null)
        //    {
        //        this._logger.LogDebug("{0} {1} not found in database",
        //            externalAccount.Provider, externalAccount.Subject);
        //
        //        externalAccountEntity = externalAccount.ToEntity();
        //        this._context.ExternalAccounts.Add(externalAccountEntity);
        //    }
        //    else
        //    {
        //        this._logger.LogDebug("{0} {1} found in database",
        //            externalAccountEntity.Provider,
        //            externalAccountEntity.Subject);
        //
        //        externalAccount.UpdateEntity(externalAccountEntity);
        //    }
        //
        //    try
        //    {
        //        this._context.SaveChanges();
        //        return Task.FromResult(externalAccountEntity.ToModel());
        //    }
        //    catch (Exception ex)
        //    {
        //        this._logger.LogError(0, ex, "Exception storing external account");
        //    }
        //
        //    return Task.FromResult<ExternalAccount>(null);
        //}

        public async Task<PagedList<UserAccount>> LoadInvitedUserAccountsAsync(
            int take,
            int skip = 0,
            Guid? invitedBy = null)
        {
            IQueryable<UserAccountEntity> baseQuery = this._context
                .UserAccounts
                .Where(c => c.CreationKind == (int)CreationKind.Invitation);

            if (invitedBy.HasValue)
            {
                baseQuery = baseQuery.Where(c => c.CreatedBy == invitedBy);
            }

            PagedList<UserAccount> result = new PagedList<UserAccount>
            {
                Skip = skip,
                Take = take,
                Total = baseQuery.Count(),
                Items = await baseQuery
                     .Include(x => x.Accounts)
                     .Include(x => x.Claims)
                     .OrderByDescending(c => c.CreatedAt)
                     .Skip(skip)
                     .Take(take)
                     .Select(s => s.ToModel()).ToArrayAsync(),
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

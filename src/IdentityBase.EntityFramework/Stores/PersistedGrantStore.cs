// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Stores
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of IPersistedGrantStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IPersistedGrantStore" />
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private readonly IPersistedGrantDbContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedGrantStore"/>
        /// class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public PersistedGrantStore(
            IPersistedGrantDbContext context,
            ILogger<PersistedGrantStore> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        /// <summary>
        /// Stores the asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public Task StoreAsync(PersistedGrant token)
        {
            Entities.PersistedGrant existing = this._context.PersistedGrants
                .SingleOrDefault(x => x.Key == token.Key);

            if (existing == null)
            {
                this._logger.LogDebug(
                    "{persistedGrantKey} not found in database",
                    token.Key);

                var persistedGrant = token.ToEntity();
                this._context.PersistedGrants.Add(persistedGrant);
            }
            else
            {
                this._logger.LogDebug(
                    "{persistedGrantKey} found in database",
                    token.Key);

                token.UpdateEntity(existing);
            }

            try
            {
                this._context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                this._logger.LogWarning(
                    "exception updating {persistedGrantKey} persisted grant in database: {error}",
                    token.Key, ex.Message);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets the grant.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = this._context.PersistedGrants
                .FirstOrDefault(x => x.Key == key);

            var model = persistedGrant?.ToModel();

            this._logger.LogDebug(
                "{persistedGrantKey} found in database: {persistedGrantKeyFound}",
                key, model != null);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Gets all grants for a given subject id.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = this._context.PersistedGrants
                .Where(x => x.SubjectId == subjectId).ToList();

            var model = persistedGrants.Select(x => x.ToModel());

            this._logger.LogDebug(
                "{persistedGrantCount} persisted grants found for {subjectId}",
                persistedGrants.Count,
                subjectId);

            return Task.FromResult(model);
        }

        /// <summary>
        /// Removes the grant by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            var persistedGrant = this._context.PersistedGrants
                .FirstOrDefault(x => x.Key == key);

            if (persistedGrant != null)
            {
                this._logger.LogDebug(
                    "removing {persistedGrantKey} persisted grant from database",
                    key);

                this._context.PersistedGrants.Remove(persistedGrant);

                try
                {
                    this._context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    this._logger.LogInformation(
                        "exception removing {persistedGrantKey} persisted grant from database: {error}",
                        key,
                        ex.Message);
                }
            }
            else
            {
                this._logger.LogDebug(
                    "no {persistedGrantKey} persisted grant found in database",
                    key);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes all grants for a given subject id and client id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            var persistedGrants = this._context.PersistedGrants
                .Where(x => x.SubjectId == subjectId &&
                    x.ClientId == clientId).ToList();

            this._logger.LogDebug(
                "removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}",
                persistedGrants.Count,
                subjectId,
                clientId);

            this._context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                this._context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                this._logger.LogInformation(
                    "removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}: {error}",
                    persistedGrants.Count,
                    subjectId,
                    clientId,
                    ex.Message);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes all grants of a give type for a given subject id and client
        /// id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Task RemoveAllAsync(
            string subjectId,
            string clientId,
            string type)
        {
            var persistedGrants = this._context.PersistedGrants.Where(x =>
                x.SubjectId == subjectId &&
                x.ClientId == clientId &&
                x.Type == type).ToList();

            this._logger.LogDebug(
                "removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}",
                persistedGrants.Count,
                subjectId,
                clientId,
                type);

            this._context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                this._context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                this._logger.LogInformation(
                    "exception removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}: {error}",
                    persistedGrants.Count,
                    subjectId,
                    clientId,
                    type,
                    ex.Message);
            }

            return Task.FromResult(0);
        }
    }
}
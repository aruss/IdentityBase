namespace IdentityBase.EntityFramework.Configuration
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkOptions
    {
        /// <summary>
        /// Gets or sets the default schema.
        /// </summary>
        /// <value>
        /// The default schema.
        /// </value>
        public string DefaultSchema { get; set; } = null;

        /// <summary>
        /// Gets or sets the identity resource table configuration.
        /// </summary>
        /// <value>
        /// The identity resource.
        /// </value>
        public TableConfiguration IdentityResource { get; set; } =
            new TableConfiguration("IdentityResources");

        /// <summary>
        /// Gets or sets the identity claim table configuration.
        /// </summary>
        /// <value>
        /// The identity claim.
        /// </value>
        public TableConfiguration IdentityClaim { get; set; } =
            new TableConfiguration("IdentityClaims");

        /// <summary>
        /// Gets or sets the API resource table configuration.
        /// </summary>
        /// <value>
        /// The API resource.
        /// </value>
        public TableConfiguration ApiResource { get; set; } =
            new TableConfiguration("ApiResources");

        /// <summary>
        /// Gets or sets the API secret table configuration.
        /// </summary>
        /// <value>
        /// The API secret.
        /// </value>
        public TableConfiguration ApiSecret { get; set; } =
            new TableConfiguration("ApiSecrets");

        /// <summary>
        /// Gets or sets the API scope table configuration.
        /// </summary>
        /// <value>
        /// The API scope.
        /// </value>
        public TableConfiguration ApiScope { get; set; } =
            new TableConfiguration("ApiScopes");

        /// <summary>
        /// Gets or sets the API claim table configuration.
        /// </summary>
        /// <value>
        /// The API claim.
        /// </value>
        public TableConfiguration ApiClaim { get; set; } =
            new TableConfiguration("ApiClaims");

        /// <summary>
        /// Gets or sets the API scope claim table configuration.
        /// </summary>
        /// <value>
        /// The API scope claim.
        /// </value>
        public TableConfiguration ApiScopeClaim { get; set; } =
            new TableConfiguration("ApiScopeClaims");

        /// <summary>
        /// Gets or sets the client table configuration.
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public TableConfiguration Client { get; set; } =
            new TableConfiguration("Clients");

        /// <summary>
        /// Gets or sets the type of the client grant table configuration.
        /// </summary>
        /// <value>
        /// The type of the client grant.
        /// </value>
        public TableConfiguration ClientGrantType { get; set; } =
            new TableConfiguration("ClientGrantTypes");

        /// <summary>
        /// Gets or sets the client redirect URI table configuration.
        /// </summary>
        /// <value>
        /// The client redirect URI.
        /// </value>
        public TableConfiguration ClientRedirectUri { get; set; } =
            new TableConfiguration("ClientRedirectUris");

        /// <summary>
        /// Gets or sets the client post logout redirect URI table configuration.
        /// </summary>
        /// <value>
        /// The client post logout redirect URI.
        /// </value>
        public TableConfiguration ClientPostLogoutRedirectUri { get; set; } =
            new TableConfiguration("ClientPostLogoutRedirectUris");

        /// <summary>
        /// Gets or sets the client scopes table configuration.
        /// </summary>
        /// <value>
        /// The client scopes.
        /// </value>
        public TableConfiguration ClientScopes { get; set; } =
            new TableConfiguration("ClientScopes");

        /// <summary>
        /// Gets or sets the client secret table configuration.
        /// </summary>
        /// <value>
        /// The client secret.
        /// </value>
        public TableConfiguration ClientSecret { get; set; } =
            new TableConfiguration("ClientSecrets");

        /// <summary>
        /// Gets or sets the client claim table configuration.
        /// </summary>
        /// <value>
        /// The client claim.
        /// </value>
        public TableConfiguration ClientClaim { get; set; } =
            new TableConfiguration("ClientClaims");

        /// <summary>
        /// Gets or sets the client IdP restriction table configuration.
        /// </summary>
        /// <value>
        /// The client IdP restriction.
        /// </value>
        public TableConfiguration ClientIdPRestriction { get; set; } =
            new TableConfiguration("ClientIdPRestrictions");

        /// <summary>
        /// Gets or sets the client cors origin table configuration.
        /// </summary>
        /// <value>
        /// The client cors origin.
        /// </value>
        public TableConfiguration ClientCorsOrigin { get; set; } =
            new TableConfiguration("ClientCorsOrigins");

        /// <summary>
        /// Gets or sets the client property table configuration.
        /// </summary>
        /// <value>
        /// The client property.
        /// </value>
        public TableConfiguration ClientProperty { get; set; } =
            new TableConfiguration("ClientProperties");

        /// <summary>
        /// Gets or sets the persisted grants table configuration.
        /// </summary>
        /// <value>
        /// The persisted grants.
        /// </value>
        public TableConfiguration PersistedGrants { get; set; } =
            new TableConfiguration("PersistedGrants");

        /// <summary>
        /// Gets or sets a value indicating whether stale entries will be
        /// automatically cleaned up from the database.
        /// This is implemented by perodically connecting to the database
        /// (according to the TokenCleanupInterval) from the hosting
        /// application. Defaults to false.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable token cleanup]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTokenCleanup { get; set; } = false;

        /// <summary>
        /// Gets or sets the token cleanup interval (in seconds). The default
        /// is 3600 (1 hour).
        /// </summary>
        /// <value>
        /// The token cleanup interval.
        /// </value>
        public int TokenCleanupInterval { get; set; } = 3600;

        /// <summary>
        /// Gets or sets the user account table configuration.
        /// </summary>
        /// <value>
        /// The user account.
        /// </value>
        public TableConfiguration UserAccount { get; set; } =
            new TableConfiguration("UserAccounts");

        /// <summary>
        /// Gets or sets the external account table configuration.
        /// </summary>
        /// <value>
        /// The external account.
        /// </value>
        public TableConfiguration ExternalAccount { get; set; } =
            new TableConfiguration("ExternalAccounts");

        /// <summary>
        /// Gets or sets the user account claim table configuration.
        /// </summary>
        /// <value>
        /// The user account claim.
        /// </value>
        public TableConfiguration UserAccountClaim { get; set; } =
            new TableConfiguration("UserAccountClaims");

        /// <summary>
        /// Callback to configure the EF DbContext.
        /// </summary>
        /// <value>
        /// The configure database context.
        /// </value>
        public Action<DbContextOptionsBuilder> DbContextOptions { get; set; }

        public bool SeedExampleData { get; set; } = false;
        public string SeedExampleDataPath { get; set; }
        public bool MigrateDatabase { get; set; } = false;
        public bool EnsureDeleted { get; set; } = false;
    }
}
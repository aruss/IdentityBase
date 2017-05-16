using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityBase.Public.EntityFramework.Options
{
    public class EntityFrameworkOptions
    {
        public Action<DbContextOptionsBuilder>  DbContextOptions { get; set; }

        public bool SeedExampleData { get; set; } = false;
        public string SeedExampleDataPath { get; set; }
        public bool MigrateDatabase { get; set; } = false;

        public bool CleanupTokens { get; set; } = true;
        public int TokenCleanupInterval { get; set; } = 60;

        public string DefaultSchema { get; set; } = null;

        public TableConfiguration UserAccount { get; set; } = new TableConfiguration("UserAccounts");
        public TableConfiguration ExternalAccount { get; set; } = new TableConfiguration("ExternalAccounts");
        public TableConfiguration UserAccountClaim { get; set; } = new TableConfiguration("UserAccountClaims");

        public TableConfiguration PersistedGrants { get; set; } = new TableConfiguration("PersistedGrants");

        public TableConfiguration IdentityResource { get; set; } = new TableConfiguration("IdentityResources");
        public TableConfiguration IdentityClaim { get; set; } = new TableConfiguration("IdentityClaims");

        public TableConfiguration ApiResource { get; set; } = new TableConfiguration("ApiResources");
        public TableConfiguration ApiSecret { get; set; } = new TableConfiguration("ApiSecrets");
        public TableConfiguration ApiScope { get; set; } = new TableConfiguration("ApiScopes");
        public TableConfiguration ApiClaim { get; set; } = new TableConfiguration("ApiClaims");
        public TableConfiguration ApiScopeClaim { get; set; } = new TableConfiguration("ApiScopeClaims");

        public TableConfiguration Client { get; set; } = new TableConfiguration("Clients");
        public TableConfiguration ClientGrantType { get; set; } = new TableConfiguration("ClientGrantTypes");
        public TableConfiguration ClientRedirectUri { get; set; } = new TableConfiguration("ClientRedirectUris");
        public TableConfiguration ClientPostLogoutRedirectUri { get; set; } = new TableConfiguration("ClientPostLogoutRedirectUris");
        public TableConfiguration ClientScopes { get; set; } = new TableConfiguration("ClientScopes");
        public TableConfiguration ClientSecret { get; set; } = new TableConfiguration("ClientSecrets");
        public TableConfiguration ClientClaim { get; set; } = new TableConfiguration("ClientClaims");
        public TableConfiguration ClientIdPRestriction { get; set; } = new TableConfiguration("ClientIdPRestrictions");
        public TableConfiguration ClientCorsOrigin { get; set; } = new TableConfiguration("ClientCorsOrigins");
    }
}
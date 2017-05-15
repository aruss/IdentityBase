using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ServiceBase.IdentityServer.Public.EntityFramework;

namespace ServiceBase.IdentityServer.Public.EntityFramework.Migrations
{
    [DbContext(typeof(DefaultDbContext))]
    partial class DefaultDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResource", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 1000);

                    b.Property<string>("DisplayName")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Enabled");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ApiResources");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResourceClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ApiResourceId")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("ApiResourceId");

                    b.ToTable("ApiClaims");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiScope", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ApiResourceId")
                        .IsRequired();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 1000);

                    b.Property<string>("DisplayName")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Emphasize");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Required");

                    b.Property<bool>("ShowInDiscoveryDocument");

                    b.HasKey("Id");

                    b.HasIndex("ApiResourceId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ApiScopes");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiScopeClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ApiScopeId")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("ApiScopeId");

                    b.ToTable("ApiScopeClaims");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiSecret", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ApiResourceId")
                        .IsRequired();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 1000);

                    b.Property<DateTime?>("Expiration");

                    b.Property<string>("Type")
                        .HasAnnotation("MaxLength", 250);

                    b.Property<string>("Value")
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("ApiResourceId");

                    b.ToTable("ApiSecrets");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AbsoluteRefreshTokenLifetime");

                    b.Property<int>("AccessTokenLifetime");

                    b.Property<int>("AccessTokenType");

                    b.Property<bool>("AllowAccessTokensViaBrowser");

                    b.Property<bool>("AllowOfflineAccess");

                    b.Property<bool>("AllowPlainTextPkce");

                    b.Property<bool>("AllowRememberConsent");

                    b.Property<bool>("AlwaysIncludeUserClaimsInIdToken");

                    b.Property<bool>("AlwaysSendClientClaims");

                    b.Property<int>("AuthorizationCodeLifetime");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("ClientName")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("ClientUri")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<bool>("EnableLocalLogin");

                    b.Property<bool>("Enabled");

                    b.Property<int>("IdentityTokenLifetime");

                    b.Property<bool>("IncludeJwtId");

                    b.Property<string>("LogoUri");

                    b.Property<bool>("LogoutSessionRequired");

                    b.Property<string>("LogoutUri");

                    b.Property<bool>("PrefixClientClaims");

                    b.Property<string>("ProtocolType")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<int>("RefreshTokenExpiration");

                    b.Property<int>("RefreshTokenUsage");

                    b.Property<bool>("RequireClientSecret");

                    b.Property<bool>("RequireConsent");

                    b.Property<bool>("RequirePkce");

                    b.Property<int>("SlidingRefreshTokenLifetime");

                    b.Property<bool>("UpdateAccessTokenClaimsOnRefresh");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 250);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 250);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientClaims");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientCorsOrigin", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 150);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientCorsOrigins");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientGrantType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("GrantType")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 250);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientGrantTypes");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientIdPRestriction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientIdPRestrictions");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientPostLogoutRedirectUri", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("PostLogoutRedirectUri")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientPostLogoutRedirectUris");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientRedirectUri", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("RedirectUri")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientRedirectUris");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientScope", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("Scope")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientScopes");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientSecret", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ClientId")
                        .IsRequired();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<DateTime?>("Expiration");

                    b.Property<string>("Type")
                        .HasAnnotation("MaxLength", 250);

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("ClientSecrets");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ExternalAccount", b =>
                {
                    b.Property<string>("Provider");

                    b.Property<string>("Subject");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 250);

                    b.Property<bool>("IsLoginAllowed");

                    b.Property<DateTime?>("LastLoginAt");

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<Guid>("UserAccountId");

                    b.HasKey("Provider", "Subject");

                    b.HasIndex("UserAccountId");

                    b.ToTable("ExternalAccounts");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.IdentityClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("IdentityResourceId")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Id");

                    b.HasIndex("IdentityResourceId");

                    b.ToTable("IdentityClaims");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.IdentityResource", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 1000);

                    b.Property<string>("DisplayName")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Emphasize");

                    b.Property<bool>("Enabled");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<bool>("Required");

                    b.Property<bool>("ShowInDiscoveryDocument");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("IdentityResources");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.PersistedGrant", b =>
                {
                    b.Property<string>("Key")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("Type")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<DateTime>("CreationTime");

                    b.Property<string>("Data")
                        .IsRequired();

                    b.Property<DateTime?>("Expiration");

                    b.Property<string>("SubjectId")
                        .HasAnnotation("MaxLength", 200);

                    b.HasKey("Key", "Type");

                    b.HasIndex("SubjectId");

                    b.HasIndex("SubjectId", "ClientId");

                    b.HasIndex("SubjectId", "ClientId", "Type");

                    b.ToTable("PersistedGrants");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.UserAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 254);

                    b.Property<DateTime?>("EmailVerifiedAt");

                    b.Property<int>("FailedLoginCount");

                    b.Property<bool>("IsEmailVerified");

                    b.Property<bool>("IsLoginAllowed");

                    b.Property<DateTime?>("LastFailedLoginAt");

                    b.Property<DateTime?>("LastLoginAt");

                    b.Property<DateTime?>("PasswordChangedAt");

                    b.Property<string>("PasswordHash")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<string>("VerificationKey")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<DateTime?>("VerificationKeySentAt");

                    b.Property<int?>("VerificationPurpose");

                    b.Property<string>("VerificationStorage")
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.UserAccountClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 250);

                    b.Property<Guid>("UserAccountId");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 250);

                    b.Property<string>("ValueType")
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("Id");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserAccountClaims");
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResourceClaim", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResource", "ApiResource")
                        .WithMany("UserClaims")
                        .HasForeignKey("ApiResourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiScope", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResource", "ApiResource")
                        .WithMany("Scopes")
                        .HasForeignKey("ApiResourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiScopeClaim", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiScope", "ApiScope")
                        .WithMany("UserClaims")
                        .HasForeignKey("ApiScopeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiSecret", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ApiResource", "ApiResource")
                        .WithMany("Secrets")
                        .HasForeignKey("ApiResourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientClaim", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("Claims")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientCorsOrigin", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("AllowedCorsOrigins")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientGrantType", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("AllowedGrantTypes")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientIdPRestriction", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("IdentityProviderRestrictions")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientPostLogoutRedirectUri", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("PostLogoutRedirectUris")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientRedirectUri", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("RedirectUris")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientScope", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("AllowedScopes")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ClientSecret", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.Client", "Client")
                        .WithMany("ClientSecrets")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.ExternalAccount", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.UserAccount", "UserAccount")
                        .WithMany("Accounts")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.IdentityClaim", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.IdentityResource", "IdentityResource")
                        .WithMany("UserClaims")
                        .HasForeignKey("IdentityResourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServiceBase.IdentityServer.Public.EntityFramework.Entities.UserAccountClaim", b =>
                {
                    b.HasOne("ServiceBase.IdentityServer.Public.EntityFramework.Entities.UserAccount", "UserAccount")
                        .WithMany("Claims")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}

namespace IdentityBase.EntityFramework.UnitTests.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.Models;
    using Xunit;
    using ExternalAccountEntity = Entities.ExternalAccount;
    using UserAccountClaimEntity = Entities.UserAccountClaim;
    using UserAccountEntity = Entities.UserAccount;

    public class UserAccountMappersTest
    {
        [Fact]
        public void UserAccountModelToEntityConfigurationIsValid()
        {
            var model = new UserAccount
            {
                Email = "test@test",
                Claims = new List<UserAccountClaim>
                {
                    new UserAccountClaim("foo", "foovalue", "footype")
                },
                Accounts = new List<ExternalAccount>
                {
                    new ExternalAccount
                    {
                        Email = "test@test"
                    }
                }
            };
            
            var entity = model.ToEntity();

            Assert.NotNull(entity);
            Assert.NotSame(entity, model);

            Assert.NotSame(
                entity.Claims.FirstOrDefault(c => c.Type.Equals("foo")),
                model.Claims.FirstOrDefault(c => c.Type.Equals("foo"))
            );

            Assert.NotSame(
                entity.Accounts.FirstOrDefault(c => c.Email.Equals("test@test")),
                model.Accounts.FirstOrDefault(c => c.Email.Equals("test@test"))
            );

            UserAccountMappers.Mapper.ConfigurationProvider
                .AssertConfigurationIsValid();
        }

        [Fact]
        public void UserAccountEntityToModelConfigurationIsValid()
        {
            var model = new UserAccountEntity();

            model.Accounts = new List<ExternalAccountEntity>
            {
                new ExternalAccountEntity
                {
                    UserAccount = model
                }
            };

            var mappedModel = model.ToModel();
            var mappedEntity = mappedModel.ToEntity();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            UserAccountMappers.Mapper.ConfigurationProvider
                .AssertConfigurationIsValid();
        }
    }
}

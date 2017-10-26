namespace IdentityBase.Public.EntityFramework.UnitTests.Mappers
{
    using System.Collections.Generic;
    using IdentityBase.Public.EntityFramework.Entities;
    using IdentityBase.Public.EntityFramework.Mappers;
    using Xunit;

    public class UserAccountMappersTest
    {
        [Fact]
        public void UserAccountModelToEntityConfigurationIsValid()
        {
            var model = new Models.UserAccount();

            model.Accounts = new List<Models.ExternalAccount>
            {
                new Models.ExternalAccount
                {
                    UserAccount = model
                }
            };

            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            UserAccountMappers.Mapper.ConfigurationProvider
                .AssertConfigurationIsValid();
        }

        [Fact]
        public void UserAccountEntityToModelConfigurationIsValid()
        {
            var model = new UserAccount();

            model.Accounts = new List<ExternalAccount>
            {
                new ExternalAccount
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

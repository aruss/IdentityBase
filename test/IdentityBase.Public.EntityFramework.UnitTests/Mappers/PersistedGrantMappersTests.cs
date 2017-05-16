using IdentityBase.Public.EntityFramework.Entities;
using IdentityBase.Public.EntityFramework.Mappers;
using Xunit;

namespace IdentityBase.Public.EntityFramework.UnitTests.Mappers
{
    public class PersistedGrantMappersTests
    {
        [Fact]
        public void PersistedGrantModelToEntityConfigurationIsValid()
        {
            var model = new IdentityServer4.Models.PersistedGrant();

            // TODO: set references

            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            PersistedGrantMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void PersistedGrantEntityToModelConfigurationIsValid()
        {
            var model = new PersistedGrant();

            // TODO: set references

            var mappedModel = model.ToModel();
            var mappedEntity = mappedModel.ToEntity();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            PersistedGrantMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
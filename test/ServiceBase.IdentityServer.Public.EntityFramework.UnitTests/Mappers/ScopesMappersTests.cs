using ServiceBase.IdentityServer.Public.EntityFramework.Entities;
using ServiceBase.IdentityServer.Public.EntityFramework.Mappers;
using Xunit;

namespace ServiceBase.IdentityServer.Public.EntityFramework.UnitTests.Mappers
{
    public class ScopesMappersTests
    {
        [Fact]
        public void IdentityResourceModelToEntityConfigurationIsValid()
        {
            var model = new IdentityServer4.Models.IdentityResource();

            // TODO: set references

            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            IdentityResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void IdentityResourceEntityToModelConfigurationIsValid()
        {
            var model = new IdentityResource();

            // TODO: set references

            var mappedModel = model.ToModel();
            var mappedEntity = mappedModel.ToEntity();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            IdentityResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void ApiResourceModelToEntityConfigurationIsValid()
        {
            var model = new IdentityServer4.Models.ApiResource();

            // TODO: set references

            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            ApiResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void ApiResourceEntityToModelConfigurationIsValid()
        {
            var model = new ApiResource();

            // TODO: set references

            var mappedModel = model.ToModel();
            var mappedEntity = mappedModel.ToEntity();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            ApiResourceMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
using ServiceBase.IdentityServer.Public.EntityFramework.Entities;
using ServiceBase.IdentityServer.Public.EntityFramework.Mappers;
using System;
using System.Collections.Generic;
using Xunit;

namespace ServiceBase.IdentityServer.Public.EntityFramework.UnitTests.Mappers
{
    public class ClientMappersTests
    {
        [Fact]
        public void ClientModelToEntityConfigurationIsValid()
        {
            var model = new IdentityServer4.Models.Client();

            // TODO: set references

            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            ClientMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void ClientEntityToModelConfigurationIsValid()
        {
            var model = new Client();

            model.ClientId = "client";
            model.ClientSecrets = new List<ClientSecret> {
                new ClientSecret
                {
                    Id = Guid.NewGuid(),
                    Value = "secret",
                    Description = "description",
                    Type = "SharedSecret",
                    Client = model
                }
            };

            model.AllowedGrantTypes = new List<ClientGrantType> {
                new ClientGrantType
                {
                    Id = Guid.NewGuid(),
                    GrantType = "sd",
                    Client = model
                }
            };

            model.AllowedScopes = new List<ClientScope>{
                new ClientScope
                {
                    Id = Guid.NewGuid(),
                    Scope = "api1",
                    Client = model
                }
            };

            var mappedModel = model.ToModel();
            var mappedEntity = mappedModel.ToEntity();

            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);
            ClientMappers.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.UnitTests.Mappers
{
    using IdentityBase.EntityFramework.Mappers;
    using IdentityServer4.Models;
    using Xunit;

    public class PersistedGrantMappersTests
    {
        [Fact]
        public void PersistedGrantAutomapperConfigurationIsValid()
        {
            var model = new PersistedGrant();
            var mappedEntity = model.ToEntity();
            var mappedModel = mappedEntity.ToModel();
            
            Assert.NotNull(mappedModel);
            Assert.NotNull(mappedEntity);

            PersistedGrantMappers.Mapper.ConfigurationProvider
                .AssertConfigurationIsValid();
        }
    }
}
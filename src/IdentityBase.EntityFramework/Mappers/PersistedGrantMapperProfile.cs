// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Mappers
{
    using AutoMapper;

    /// <summary>
    /// Defines entity/model mapping for persisted grants.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class PersistedGrantMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="PersistedGrantMapperProfile">
        /// </see>
        /// </summary>
        public PersistedGrantMapperProfile()
        {
            // entity to model
            CreateMap<Entities.PersistedGrant, IdentityServer4.Models
                .PersistedGrant>(MemberList.Destination);

            // model to entity
            CreateMap<IdentityServer4.Models.PersistedGrant, Entities
                .PersistedGrant>(MemberList.Source);
        }
    }
}
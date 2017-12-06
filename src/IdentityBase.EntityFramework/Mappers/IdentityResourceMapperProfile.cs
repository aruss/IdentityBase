// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Mappers
{
    using System.Linq;
    using AutoMapper;
    using IdentityBase.EntityFramework.Entities;
    using IdSrv = IdentityServer4.Models;

    /// <summary>
    /// Defines entity/model mapping for identity resources.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class IdentityResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="IdentityResourceMapperProfile"/>
        /// </summary>
        public IdentityResourceMapperProfile()
        {
            // entity to model
            CreateMap<IdentityResource,
                IdSrv.IdentityResource>(MemberList.Destination)
                    .ConstructUsing(src => new IdSrv.IdentityResource())
                    .ForMember(x => x.UserClaims, opt => opt
                    .MapFrom(src => src.UserClaims.Select(x => x.Type)));

            // model to entity
            CreateMap<IdSrv.IdentityResource,
                IdentityResource>(MemberList.Source)
                    .ForMember(x => x.UserClaims, opts => opts
                    .MapFrom(src => src.UserClaims
                    .Select(x => new IdentityClaim { Type = x })));
        }
    }
}
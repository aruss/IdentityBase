// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Mappers
{
    using System.Linq;
    using AutoMapper;
    using IdentityBase.EntityFramework.Entities;
    using IdSrv = IdentityServer4.Models;

    /// <summary>
    /// Defines entity/model mapping for API resources.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ApiResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="ApiResourceMapperProfile"/>
        /// </summary>
        public ApiResourceMapperProfile()
        {
            // entity to model
            CreateMap<ApiResource, IdSrv.ApiResource>(MemberList.Destination)
                .ConstructUsing(src => new IdSrv.ApiResource())
                .ForMember(x => x.ApiSecrets, opt => opt
                    .MapFrom(src => src.Secrets.Select(x => x))
                )
                .ForMember(x => x.Scopes, opt => opt
                    .MapFrom(src => src.Scopes.Select(x => x))
                )
                .ForMember(x => x.UserClaims, opt => opt
                    .MapFrom(src => src.UserClaims.Select(x => x.Type))
                );

            CreateMap<ApiSecret, IdSrv.Secret>(MemberList.Destination);

            CreateMap<ApiScope, IdSrv.Scope>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt
                    .MapFrom(src => src.UserClaims.Select(x => x.Type))
                );

            // model to entity
            CreateMap<IdSrv.ApiResource, ApiResource>(MemberList.Source)
                .ForMember(x => x.Secrets, opts => opts
                    .MapFrom(src => src.ApiSecrets.Select(x => x))
                )
                .ForMember(x => x.Scopes, opts => opts
                    .MapFrom(src => src.Scopes.Select(x => x))
                )
                .ForMember(x => x.UserClaims, opts => opts
                    .MapFrom(src => src.UserClaims
                        .Select(x => new ApiResourceClaim { Type = x })
                    )
                );

            CreateMap<IdSrv.Secret, ApiSecret>(MemberList.Source);

            CreateMap<IdSrv.Scope, ApiScope>(MemberList.Source)
                .ForMember(x => x.UserClaims, opt => opt
                    .MapFrom(src => src.UserClaims
                        .Select(x => new ApiScopeClaim { Type = x })
                    )
                );
        }
    }
}
// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace IdentityBase.EntityFramework.Mappers
{
    using AutoMapper;

    /// <summary>
    /// Extension methods to map to/from entity/model for clients.
    /// </summary>
    public static class ClientMappers
    {
        static ClientMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg
                .AddProfile<ClientMapperProfile>()).CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        /// Maps an entity to a model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static IdentityServer4.Models.Client ToModel(
            this Entities.Client entity)
        {
            return entity == null ? null : Mapper
                .Map<IdentityServer4.Models.Client>(entity);
        }

        /// <summary>
        /// Maps a model to an entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static Entities.Client ToEntity(
            this IdentityServer4.Models.Client model)
        {
            return model == null ? null : Mapper
                .Map<Entities.Client>(model);
        }
    }
}
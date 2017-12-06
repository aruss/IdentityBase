// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Api.Invitations
{
    public class PagedListInputModel
    {
        /// <summary>
        /// Items to retrieve
        /// </summary>
        [Range(IdentityBaseConstants.TakeMin, IdentityBaseConstants.TakeMax)]
        public int Take { get; set; } = IdentityBaseConstants.TakeDefault;

        /// <summary>
        /// Items to skip 
        /// </summary>
        [Range(IdentityBaseConstants.SkipMin, IdentityBaseConstants.SkipMax)]
        public int Skip { get; set; } = IdentityBaseConstants.SkipDefault;
    }
}
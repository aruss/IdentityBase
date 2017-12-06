// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityBase.WebApi.Actions.Invitations
{
    public class PagedListInputModel
    {
        /// <summary>
        /// Items to retrieve
        /// </summary>
        [Range(WebApiConstants.TakeMin, WebApiConstants.TakeMax)]
        public int Take { get; set; } = WebApiConstants.TakeDefault;

        /// <summary>
        /// Items to skip 
        /// </summary>
        [Range(WebApiConstants.SkipMin, WebApiConstants.SkipMax)]
        public int Skip { get; set; } = WebApiConstants.SkipDefault;
    }
}
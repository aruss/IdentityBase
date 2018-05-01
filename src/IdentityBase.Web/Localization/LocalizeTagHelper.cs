// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Localization
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.Localization;

    [HtmlTargetElement(Attributes = "localize")]
    public class LocalizeTagHelper : TagHelper
    {
        private readonly IStringLocalizer _localizer;

        public LocalizeTagHelper(IStringLocalizer localizer)
        {
            this._localizer = localizer;
        }

        [HtmlAttributeName("localize")]
        public string Localize { get; set; }

        public override async Task ProcessAsync(
            TagHelperContext context,
            TagHelperOutput output)
        {
            string key = this.Localize;
            if (String.IsNullOrWhiteSpace(this.Localize))
            {
                TagHelperContent childContent =
                    await output.GetChildContentAsync();

                key = childContent.GetContent().Trim();
            }

            output.Content.SetContent(this._localizer[key]);
        }
    }
}

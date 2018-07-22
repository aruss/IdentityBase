// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Logout
{
    public class LoggedOutViewModel
    {
        public string LogoutId { get; set; }
        public bool ShowLogoutPrompt { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string ClientName { get; set; }
        public string SignOutIframeUrl { get; set; }
        public string ExternalAuthenticationScheme { get; set; }
        public bool AutomaticRedirectAfterSignOut { get; set; } = false;

        public bool TriggerExternalSignout =>
            this.ExternalAuthenticationScheme != null;
    }
}
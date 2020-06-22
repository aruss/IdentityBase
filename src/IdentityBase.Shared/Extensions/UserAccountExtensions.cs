// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using IdentityBase.Models;
    using IdentityModel;
    using ServiceBase.Extensions;

    public static partial class UserAccountExtensions
    {
        public static bool HasPassword(this UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentException(nameof(userAccount));
            }

            return !String.IsNullOrWhiteSpace(userAccount.PasswordHash);
        }

        public static bool HasExternalAccounts(this UserAccount userAccount)
        {
            return userAccount.Accounts != null &&
                userAccount.Accounts.Count() > 0;
        }

        public static bool IsNew(this UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentException(nameof(userAccount));
            }

            return !userAccount.LastLoginAt.HasValue;
        }

        public static string GetTwoFactorAuthInfo(this UserAccount userAccount)
        {
            return "..."; 
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(this UserAccount userAccount)
        {
            // if (SubjectId.IsMissing()) throw new ArgumentException("SubjectId is mandatory", nameof(SubjectId));

            var claims = new List<Claim> {
                new Claim(
                    JwtClaimTypes.Subject,
                    userAccount.Id.ToString()
                )
            };

            if (userAccount.Email.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.Name, DisplayName));
            }


            if (DisplayName.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.Name, DisplayName));
            }

            if (IdentityProvider.IsPresent())
            {
                claims.Add(new Claim(JwtClaimTypes.IdentityProvider, IdentityProvider));
            }

            if (AuthenticationTime.HasValue)
            {
                claims.Add(new Claim(JwtClaimTypes.AuthenticationTime, new DateTimeOffset(AuthenticationTime.Value).ToUnixTimeSeconds().ToString()));
            }

            if (AuthenticationMethods.Any())
            {
                foreach (var amr in AuthenticationMethods)
                {
                    claims.Add(new Claim(JwtClaimTypes.AuthenticationMethod, amr));
                }
            }

            claims.AddRange(userAccount.Claims.Select(c => new Claim(c.Type, c.Value)));

            var id = new ClaimsIdentity(claims.Distinct(new ClaimComparer()), Constants.IdentityServerAuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }
    }
}

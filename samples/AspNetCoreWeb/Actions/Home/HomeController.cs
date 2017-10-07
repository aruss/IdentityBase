namespace AspNetCoreWeb.Actions.Home
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;
    using IdentityModel.Client;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using System.Globalization;
    using Microsoft.AspNetCore.Http;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApi()
        {
            string token = await HttpContext.GetTokenAsync("access_token");

            HttpClient client = new HttpClient();
            client.SetBearerToken(token);

            string response = await client
                .GetStringAsync("http://localhost:3721/identity");

            ViewBag.Json = JArray.Parse(response).ToString();

            return View();
        }

        public async Task<IActionResult> RenewTokens()
        {
            DiscoveryResponse disco = await DiscoveryClient
                .GetAsync("http://localhost:5000");

            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            TokenClient tokenClient = new TokenClient(disco.TokenEndpoint,
                "mvc.hybrid", "secret");

            string rt = await HttpContext.GetTokenAsync("refresh_token");
            TokenResponse tokenResult = await tokenClient
                .RequestRefreshTokenAsync(rt);

            if (!tokenResult.IsError)
            {
                string old_id_token = await HttpContext
                    .GetTokenAsync("id_token");

                string new_access_token = tokenResult.AccessToken;
                string new_refresh_token = tokenResult.RefreshToken;

                var tokens = new List<AuthenticationToken>
                {
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.IdToken,
                        Value = old_id_token
                    },
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.AccessToken,
                        Value = new_access_token
                    },
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.RefreshToken,
                        Value = new_refresh_token
                    }
                };

                DateTime expiresAt = DateTime.UtcNow +
                    TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                tokens.Add(new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt
                        .ToString("o", CultureInfo.InvariantCulture)
                });

                AuthenticateResult info = await HttpContext
                    .AuthenticateAsync("Cookies");

                info.Properties.StoreTokens(tokens);

                await HttpContext
                    .SignInAsync("Cookies", info.Principal, info.Properties);

                return Redirect("~/Home/Secure");
            }

            ViewData["Error"] = tokenResult.Error;
            return View("Error");
        }

        public IActionResult Logout()
        {
            return new SignOutResult(new[] { "Cookies", "oidc" });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

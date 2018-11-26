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
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.Logging;

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationOptions _appOptions;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationOptions appOptions)
        {
            this._logger = logger;
            this._appOptions = appOptions;
        }

        public IActionResult Index()
        {
            this._logger.LogInformation("CurrentUICulture " +
                CultureInfo.CurrentUICulture);

            return this.View();
        }

        [Authorize]
        [HttpGet("/secure")]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        [HttpGet("/callapi-user-token")]
        public async Task<IActionResult> CallApiUserToken()
        {
            string accessToken =
                await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(accessToken);

            string content =
                await client.GetStringAsync("http://localhost:5001/identity");

            this.ViewBag.Json = JArray.Parse(content).ToString();
            return this.View("Json");
        }

        [Authorize]
        [HttpGet("/callapi-client-credentials")]
        public async Task<IActionResult> CallApiClientCredentials()
        {
            TokenClient tokenClient = new TokenClient(
                this._appOptions.Authority + "/connect/token",
                this._appOptions.ClientId,
                this._appOptions.ClientSecret
            );

            TokenResponse tokenResponse =
                await tokenClient.RequestClientCredentialsAsync("api1");

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            string content =
                await client.GetStringAsync("http://localhost:5001/identity");

            this.ViewBag.Json = JArray.Parse(content).ToString();
            return this.View("Json");
        }


        [Authorize]
        [HttpGet("/renew-tokens")]
        public async Task<IActionResult> RenewTokens()
        {
            DiscoveryResponse discoClient = await DiscoveryClient
                .GetAsync(this._appOptions.Authority);

            if (discoClient.IsError)
            {
                throw new Exception(discoClient.Error);
            }

            TokenClient tokenClient = new TokenClient(
                discoClient.TokenEndpoint,
                this._appOptions.ClientId,
                this._appOptions.ClientSecret
            );

            string refreshToken =
                await HttpContext.GetTokenAsync("refresh_token");

            TokenResponse tokenResult = await tokenClient
                .RequestRefreshTokenAsync(refreshToken);

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

                await this.HttpContext
                    .SignInAsync("Cookies", info.Principal, info.Properties);

                return this.RedirectToAction("Secure", "Home"); 
            }

            this.ViewData["Error"] = tokenResult.Error;
            return this.View("Error");
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            return new SignOutResult(new[] {
                "Cookies",
                "oidc"
            });
        }

        [HttpGet("/set-language")]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            this.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [HttpGet("/error")]
        public IActionResult Error()
        {
            return this.View();
        }
    }
}

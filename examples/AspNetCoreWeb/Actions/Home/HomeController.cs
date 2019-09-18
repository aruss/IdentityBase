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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationOptions appOptions,
            IHttpClientFactory httpClientFactory,
            IDiscoveryCache discoveryCache)
        {
            this._logger = logger;
            this._appOptions = appOptions;
            this._httpClientFactory = httpClientFactory;
            this._discoveryCache = discoveryCache;
        }

        public IActionResult Index()
        {
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
            string token = await this.HttpContext.GetTokenAsync("access_token");
            HttpClient client = this._httpClientFactory.CreateClient();
            client.SetBearerToken(token);

            string response = await client.GetStringAsync(
                this._appOptions.Api1BaseAddress + "/identity");

            this.ViewBag.Json = JArray.Parse(response).ToString();
            return this.View("Json");
        }

        [Authorize]
        [HttpGet("/callapi-client-credentials")]
        public async Task<IActionResult> CallApiClientCredentials()
        {
            HttpClient tokenClienst = this._httpClientFactory.CreateClient();
            TokenResponse tokenResponse = await tokenClienst
                .RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = this._appOptions.Authority + "/connect/token",
                    ClientId = this._appOptions.ClientId,
                    ClientSecret = this._appOptions.ClientSecret,
                    Scope = "api1"
                });

            HttpClient client = this._httpClientFactory.CreateClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            string content =
                await client.GetStringAsync(
                    this._appOptions.Api1BaseAddress + "/identity");

            this.ViewBag.Json = JArray.Parse(content).ToString();
            return this.View("Json");
        }


        [Authorize]
        [HttpGet("/renew-tokens")]
        public async Task<IActionResult> RenewTokens()
        {
            DiscoveryDocumentResponse disco = await this._discoveryCache.GetAsync();

            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            string rt = await this.HttpContext.GetTokenAsync("refresh_token");
            HttpClient tokenClient = this._httpClientFactory.CreateClient();

            TokenResponse tokenResult =
                await tokenClient.RequestRefreshTokenAsync(
                    new RefreshTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = this._appOptions.ClientId,
                        ClientSecret = this._appOptions.ClientSecret,
                        RefreshToken = rt,
                        
                    });

            if (!tokenResult.IsError)
            {
                string oldIdToken =
                    await this.HttpContext.GetTokenAsync("id_token");

                string newAccessToken = tokenResult.AccessToken;
                string newRefreshToken = tokenResult.RefreshToken;

                DateTime expiresAt =
                    DateTime.UtcNow +
                    TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                AuthenticateResult info =
                    await HttpContext.AuthenticateAsync("Cookies");

                info.Properties
                    .UpdateTokenValue("refresh_token", newRefreshToken);

                info.Properties
                    .UpdateTokenValue("access_token", newAccessToken);

                info.Properties.UpdateTokenValue(
                    "expires_at",
                    expiresAt.ToString("o", CultureInfo.InvariantCulture)
                );

                await this.HttpContext.SignInAsync(
                    "Cookies",
                    info.Principal,
                    info.Properties
                );

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
                CookieRequestCultureProvider
                    .MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                }
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

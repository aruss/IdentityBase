namespace IdentityBase.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
    using ServiceBase.Tests;

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> ConstentPostFormAsync(
            this HttpClient client,
            bool rememberMe,
            HttpResponseMessage formGetResponse)
        {
            formGetResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            HttpResponseMessage authorizeResponse = await client
                .FollowRedirect(formGetResponse);

            authorizeResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            HttpResponseMessage constentGetResponse = await client
                .FollowRedirect(authorizeResponse);

            // just submit form as is 
            IHtmlDocument doc = await constentGetResponse.Content
                .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            //form["RememberLogin"] = rememberMe ? "true" : "false";

            HttpResponseMessage constentPostResponse = await client
                .PostAsync(doc.GetFormAction(), form, constentGetResponse);

            constentPostResponse.EnsureSuccessStatusCode();
            return constentPostResponse;
        }

        public static async Task<HttpResponseMessage> LoginGetAndPostFormAsync(
            this HttpClient client,
            string emailAddress,
            string password,
            bool rememberMe = false,
            HttpResponseMessage prevResponse = null)
        {
            HttpResponseMessage getResponse = await client.GetAsync(
                $"/login?returnUrl={Constants.ReturnUrl}",
                prevResponse
            );

            getResponse.EnsureSuccessStatusCode();
            string html = await getResponse.Content.ReadAsStringAsync();

            // 5. Fill out the login form and submit 
            IHtmlDocument doc = await getResponse.Content
                .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            form["Email"] = emailAddress;
            form["Password"] = password;
            form["RememberLogin"] = rememberMe ? "true" : "false";

            HttpResponseMessage postResponse = await client
                .PostAsync(doc.GetFormAction(), form, getResponse);

            return postResponse;
        }

        public static async Task<HttpResponseMessage> RecoveryConfirmGetAndPostFormAsync(
            this HttpClient client,
            string confirmUrl,
            string newPassword,
            HttpResponseMessage prevResponse = null)
        {
            HttpResponseMessage getResponse = client.GetAsync(confirmUrl).Result;
            getResponse.EnsureSuccessStatusCode();

            IHtmlDocument doc = await getResponse.Content
                .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            form["Password"] = newPassword;
            form["PasswordConfirm"] = newPassword;

            HttpResponseMessage postResponse = await client
                .PostAsync(doc.GetFormAction(), form, getResponse);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            postResponse.Headers.Location.ToString().Should()
                .StartWith("/connect/authorize/callback");

            return postResponse;
        }

        public static async Task<HttpResponseMessage> RecoveryGetFormAsync(
            this HttpClient client,
            HttpResponseMessage prevResponse = null)
        {
            HttpResponseMessage response = await client.GetAsync(
                $"/recover?returnUrl={Constants.ReturnUrl}",
                prevResponse
            );

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryPostFormAsync(
            this HttpClient client,
            string emailAddress,
            HttpResponseMessage formGetResponse)
        {
            IHtmlDocument doc = await formGetResponse.Content
                            .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            form["Email"] = emailAddress;

            string uri = doc.GetFormAction();

            HttpResponseMessage response = await client
                .PostAsync(uri, form, formGetResponse);

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryGetAndPostFormAsync(
            this HttpClient client,
            string emailAddress,
            HttpResponseMessage prevResponse = null)
        {
            HttpResponseMessage response = await client
                .RecoveryGetFormAsync(prevResponse);

            return await client.RecoveryPostFormAsync(emailAddress, response);
        }

        public static async Task<HttpResponseMessage> RecoveryCancelGetValidAsync(
            this HttpClient client,
            string cancelUrl)
        {
            HttpResponseMessage response = client
               .GetAsync(cancelUrl).Result;

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryCancelGetInvalidAsync(
           this HttpClient client,
           string cancelUrl)
        {
            HttpResponseMessage response = await client
                .RecoveryCancelGetValidAsync(cancelUrl);

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryConfirmGetValidAsync(
            this HttpClient client,
            string confirmUrl)
        {
            HttpResponseMessage response = client
               .GetAsync(confirmUrl).Result;

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryConfirmGetInvalidAsync(
           this HttpClient client,
           string confirmUrl)
        {
            HttpResponseMessage response = await client
                .RecoveryConfirmGetValidAsync(confirmUrl);

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RegisterConfirmGetAndPostFormAsync(
            this HttpClient client,
            string confirmUrl,
            string password = null,
            HttpResponseMessage prevResponse = null)
        {
            HttpResponseMessage getResponse = client.GetAsync(confirmUrl).Result;
            getResponse.EnsureSuccessStatusCode();

            IHtmlDocument doc = await getResponse.Content
                .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            if (!String.IsNullOrWhiteSpace(password))
            {
                form["Password"] = password;
                form["PasswordConfirm"] = password;
            }

            HttpResponseMessage postResponse = await client
                .PostAsync(doc.GetFormAction(), form, getResponse);

            return postResponse;
        }

        public static void ShouldBeRedirectedToAuthorizeEndpoint(
            this HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.ToString().Should()
                .StartWith("/connect/authorize/callback");
        }
    }
}
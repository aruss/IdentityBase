namespace IdentityBase.Public.IntegrationTests
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
        public static async Task<HttpResponseMessage> ConstentPostForm(
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

        public static async Task<HttpResponseMessage> LoginGetAndPostForm(
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

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);

            postResponse.Headers.Location.ToString().Should()
                .StartWith("/connect/authorize/callback");

            return postResponse;
        }
        
        public static async Task<HttpResponseMessage> RecoveryConfirmGetAndPostForm(
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

        public static async Task<HttpResponseMessage> RecoveryGetForm(
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

        public static async Task<HttpResponseMessage> RecoveryPostForm(
            this HttpClient client,
            string emailAddress,
            HttpResponseMessage formGetResponse)
        {
            IHtmlDocument doc = await formGetResponse.Content
                            .ReadAsHtmlDocumentAsync();

            Dictionary<string, string> form = doc.GetFormInputs();
            form["Email"] = emailAddress;

            HttpResponseMessage response = await client
                .PostAsync(doc.GetFormAction(), form, formGetResponse);

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryGetAndPostForm(
            this HttpClient client,
            string emailAddress,
            HttpResponseMessage prevResponse = null)
        {
            return await client.RecoveryPostForm(
                emailAddress,
                await client.RecoveryGetForm(prevResponse));
        }

        public static async Task<HttpResponseMessage> RecoveryCancelGetValid(
            this HttpClient client,
            string cancelUrl)
        {
            HttpResponseMessage response = client
               .GetAsync(cancelUrl).Result;

            response.EnsureSuccessStatusCode();

            return response; 
        }

        public static async Task<HttpResponseMessage> RecoveryCancelGetInvalid(
           this HttpClient client,
           string cancelUrl)
        {
            HttpResponseMessage response = await client
                .RecoveryCancelGetValid(cancelUrl); 

            // TODO: check for error message 

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryConfirmGetValid(
            this HttpClient client,
            string confirmUrl)
        {
            HttpResponseMessage response = client
               .GetAsync(confirmUrl).Result;

            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task<HttpResponseMessage> RecoveryConfirmGetInvalid(
           this HttpClient client,
           string confirmUrl)
        {
            HttpResponseMessage response = await client
                .RecoveryConfirmGetValid(confirmUrl);

            // TODO: check for error message 

            return response;
        }
    }
}
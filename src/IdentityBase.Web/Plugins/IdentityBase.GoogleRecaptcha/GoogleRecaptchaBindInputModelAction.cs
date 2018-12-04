// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.GoogleRecaptcha
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityBase.Forms;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public class GoogleRecaptchaBindInputModelAction :
        ILoginBindInputModelAction,
        IRecoverBindInputModelAction,
        IRegisterBindInputModelAction
    {
        private readonly GoogleRecaptchaOptions _recaptchaOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleRecaptchaBindInputModelAction(
            GoogleRecaptchaOptions recaptchaOptions,
            IHttpClientFactory httpClientFactory)
        {
            this._recaptchaOptions = recaptchaOptions;
            this._httpClientFactory = httpClientFactory;
        }

        public int Step => 0;

        // https://developers.google.com/recaptcha/docs/verify
        public async Task ExecuteAsync(BindInputModelContext context)
        {
            string inputValue = context.Controller
                .Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(inputValue))
            {
                context.Controller.ModelState.AddModelError(
                    "GoogleRecaptcha",
                    "Invalid ReCaptcha input"
                );

                return;
            }

            HttpClient client = _httpClientFactory
                .CreateClient("GoogleRecaptchaClient");

            var payload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret",
                    this._recaptchaOptions.Secret),

                new KeyValuePair<string, string>("response", inputValue)

                //new KeyValuePair<string, string>("remoteip",
                //    context.Controller.HttpContext.Connection
                //        .RemoteIpAddress.ToString())
            });

            HttpResponseMessage response = await client
                .PostAsync("/recaptcha/api/siteverify", payload);

            if (!response.IsSuccessStatusCode)
            {
                context.Controller.ModelState.AddModelError("ReCaptcha error");
                return;
            }

            GoogleRecaptchaVerificationResponse verificationResponse =
                JsonConvert.DeserializeObject<
                    GoogleRecaptchaVerificationResponse>(
                        await response.Content.ReadAsStringAsync());

            if (!verificationResponse.Success)
            {
                context.Controller.ModelState.AddModelError("ReCaptcha error");
                return;
            }
        }
    }
}

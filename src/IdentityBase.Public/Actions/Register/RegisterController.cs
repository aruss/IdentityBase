// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.Actions.Register
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Extensions;
    using ServiceBase.Notification.Email;

    public class RegisterController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RegisterController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterController(
            ApplicationOptions applicationOptions,
            ILogger<RegisterController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            ClientService clientService,
            IHttpContextAccessor httpContextAccessor)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._emailService = emailService;
            this._userAccountService = userAccountService;
            this._clientService = clientService;
            this._httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("register", Name = "Register")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            RegisterViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this._logger.LogError(
                    "Register attempt with missing returnUrl parameter");

                return this.Redirect(this.Url.Action("Index", "Error"));
            }

            return this.View(vm);
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterInputModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(await this.CreateViewModelAsync(model));
            }

            string email = model.Email.ToLower();

            // Check if user with same email exists
            UserAccount userAccount = await this._userAccountService
                .LoadByEmailWithExternalAsync(email);

            // If user dont exists create a new one
            if (userAccount == null)
            {
                return await this.TryCreateNewUserAccount(model);
            }
            // User is just disabled by whatever reason
            else if (!userAccount.IsLoginAllowed)
            {
                this.ModelState
                    .AddModelError("Your user account has be disabled");
            }
            // If user has a password then its a local account
            else if (userAccount.HasPassword())
            {
                // User has to follow a link in confirmation mail
                if (this._applicationOptions.RequireLocalAccountVerification &&
                    !userAccount.IsEmailVerified)
                {
                    this.ModelState
                        .AddModelError("Please confirm your email account");

                    // TODO: show link for resent confirmation link
                }

                // If user has a password then its a local account
                this.ModelState.AddModelError("User already exists");
            }
            else
            {
                // External account with same email
                return await this.TryMergeWithExistingUserAccount(
                    userAccount,
                    model
                );
            }

            return this.View(
                await this.CreateViewModelAsync(model, userAccount)
            );
        }

        [HttpGet("register/confirm/{key}", Name = "RegisterConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                this.ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            // User account requires completion 
            if (this._applicationOptions.EnableInvitationCreateEndpoint &&
                result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                ConfirmViewModel vm = new ConfirmViewModel
                {
                    Key = key,
                    RequiresPassword = !result.UserAccount.HasPassword(),
                    Email = result.UserAccount.Email
                };

                return this.View(vm);
            }
            // User profile already fine and just needs to be activated
            else
            {
                string returnUrl = result.UserAccount.VerificationKey;

                await this._userAccountService
                    .SetEmailVerifiedAsync(result.UserAccount);

                if (this._applicationOptions.LoginAfterAccountConfirmation)
                {
                    await this.HttpContext
                        .SignInAsync(result.UserAccount, null);

                    if (returnUrl != null &&
                        this._interaction.IsValidReturnUrl(returnUrl))
                    {
                        return this.Redirect(returnUrl);
                    }
                }

                return this.Redirect(
                    this.Url.Action(
                        "Login",
                        "Login",
                        new { ReturnUrl = returnUrl }
                    )
                );
            }
        }

        // Currently is only used for invitations 
        [HttpPost("register/confirm/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmInputModel model)
        {
            if (!this._applicationOptions.EnableInvitationCreateEndpoint)
            {
                return this.NotFound();
            }

            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    model.Key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            if (result.UserAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                // TODO: clear token if account is there 

                this.ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(new ConfirmViewModel
                {
                    Key = model.Key,
                    Email = result.UserAccount.Email
                });
            }

            string returnUrl = result.UserAccount.VerificationStorage;
            this._userAccountService.SetEmailVerified(result.UserAccount);

            this._userAccountService
                .AddLocalCredentials(result.UserAccount, model.Password);

            await this._userAccountService
                .UpdateUserAccountAsync(result.UserAccount);

            if (result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                // TODO: validate 
                return this.Redirect(returnUrl);
            }
            else
            {
                if (this._applicationOptions.LoginAfterAccountRecovery)
                {
                    await this.HttpContext
                        .SignInAsync(result.UserAccount, null);

                    if (this._interaction.IsValidReturnUrl(returnUrl))
                    {
                        return this.Redirect(returnUrl);
                    }
                }

                return this.Redirect(
                    this.Url.Action(
                        "Index",
                        "Login",
                        new { ReturnUrl = returnUrl }
                    )
                );
            }
        }

        [HttpGet("register/cancel/{key}", Name = "RegisterCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                // TODO: clear token if account is there 

                this.ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return View("InvalidToken");
            }

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);


            if (this._interaction.IsValidReturnUrl(returnUrl))
            {
                return this.Redirect(
                    this.Url.Action(
                        "Index",
                        "Login",
                        new { ReturnUrl = returnUrl }
                    )
                );
            }
            else
            {
                return this.Redirect(returnUrl);
            }
        }

        //   [HttpGet("register/complete")]
        //   public async Task<IActionResult> Complete(string returnUrl)
        //   {
        //       var userAccount = await GetUserAccountFromCoockyValue();
        //       if (userAccount == null)
        //       {
        //           ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
        //           return View("InvalidToken");
        //       }
        //
        //       var vm = new RegisterCompleteViewModel
        //       {
        //           ReturnUrl = returnUrl,
        //           UserAccountId = userAccount.Id,
        //           Email = userAccount.Email
        //       };
        //
        //       return View(vm);
        //   }

        //   [HttpPost("register/complete")]
        //   [ValidateAntiForgeryToken]
        //   public async Task<IActionResult> Complete(RegisterCompleteInputModel model)
        //   {
        //       // update user
        //       // authenticate user
        //       // redirect to return url
        //
        //       var userAccount = await GetUserAccountFromCoockyValue();
        //       httpContextAccessor.HttpContext.Response.Cookies.Delete("ConfirmUserAccountId");
        //
        //       throw new NotImplementedException();
        //
        //       /*
        //       // TODO: cleanup
        //       userAccount.ClearVerification();
        //       var now = DateTime.UtcNow;
        //       userAccount.IsLoginAllowed = true;
        //       userAccount.IsEmailVerified = true;
        //       userAccount.EmailVerifiedAt = now;
        //       userAccount.UpdatedAt = now;
        //
        //       await Task.WhenAll(
        //           userAccountService.AddLocalCredentialsAsync(userAccount, model.Password),
        //           httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null)
        //       ); */
        //
        //       // && interaction.IsValidReturnUrl(returnUrl)
        //
        //       if (model.ReturnUrl != null)
        //       {
        //           return Redirect(model.ReturnUrl);
        //       }
        //
        //       return Redirect("/");
        //   }

        //   [NonAction]
        //   internal async Task<UserAccount> GetUserAccountFromCoockyValue()
        //   {
        //       if (httpContextAccessor.HttpContext.Request.Cookies
        //           .TryGetValue("ConfirmUserAccountId", out string userIdStr))
        //       {
        //           if (Guid.TryParse(userIdStr, out Guid userId))
        //           {
        //               return await userAccountService.LoadByIdAsync(userId);
        //           }
        //       }
        //
        //       return null;
        //   }

        [NonAction]
        internal async Task<RegisterViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(
                new RegisterViewModel { ReturnUrl = returnUrl }
            );
        }

        [NonAction]
        internal async Task<RegisterViewModel> CreateViewModelAsync(
            RegisterInputModel inputModel,
            UserAccount userAccount = null)
        {
            AuthorizationRequest context = await this._interaction
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            Client client = await this._clientService
                .FindEnabledClientByIdAsync(context.ClientId);

            IEnumerable<ExternalProvider> providers = await this._clientService
                .GetEnabledProvidersAsync(client);

            RegisterViewModel vm = new RegisterViewModel(inputModel)
            {
                EnableAccountRecover =
                    this._applicationOptions.EnableAccountRecovery,

                EnableLocalLogin = client != null ?
                    client.EnableLocalLogin :
                    false && this._applicationOptions.EnableLocalLogin,

                ExternalProviders = providers.ToArray(),

                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider)
            };

            return vm;
        }

        [NonAction]
        internal SuccessViewModel CreateSuccessViewModel(
            UserAccount userAccount,
            string returnUrl)
        {
            return new SuccessViewModel()
            {
                ReturnUrl = returnUrl,
                Provider = userAccount.Email.Split('@').LastOrDefault()
            };
        }

        [NonAction]
        internal async Task<IActionResult> TryMergeWithExistingUserAccount(
            UserAccount userAccount,
            RegisterInputModel model)
        {
            // Merge accounts without user consent
            if (this._applicationOptions.AutomaticAccountMerge)
            {
                await this._userAccountService
                    .AddLocalCredentialsAsync(userAccount, model.Password);

                if (this._applicationOptions.LoginAfterAccountCreation)
                {
                    await this.HttpContext.SignInAsync(userAccount, null);

                    if (model.ReturnUrl != null &&
                        this._interaction.IsValidReturnUrl(model.ReturnUrl))
                    {
                        return this.Redirect(model.ReturnUrl);
                    }
                }
                else
                {
                    return this.View(
                        "Success",
                        CreateSuccessViewModel(userAccount, model.ReturnUrl)
                    );
                }
            }
            // Ask user if he wants to merge accounts
            else
            {
                throw new NotImplementedException();
            }

            // Return list of external account providers as hint
            RegisterViewModel vm = new RegisterViewModel(model)
            {
                ExternalProviderHints = userAccount.Accounts
                    .Select(s => s.Provider).ToArray()
            };

            return View(vm);
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            string baseUrl = this.HttpContext.GetIdentityServerBaseUrl()
                .EnsureTrailingSlash();

            await this._emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountCreated,
                userAccount.Email,
                new
                {
                    ConfirmUrl =
                        $"{baseUrl}register/confirm/{userAccount.VerificationKey}",

                    CancelUrl =
                        $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
                },
                true
            );
        }

        [NonAction]
        internal async Task<IActionResult> TryCreateNewUserAccount(
            RegisterInputModel model)
        {
            UserAccount userAccount = await this._userAccountService
                .CreateNewLocalUserAccountAsync(
                    model.Email,
                    model.Password,
                    model.ReturnUrl
                );

            // Send confirmation mail
            if (this._applicationOptions.RequireLocalAccountVerification)
            {
                await this.SendEmailAsync(userAccount);
            }

            if (this._applicationOptions.LoginAfterAccountCreation)
            {
                await this.HttpContext.SignInAsync(userAccount, null);

                if (model.ReturnUrl != null &&
                    this._interaction.IsValidReturnUrl(model.ReturnUrl))
                {
                    return this.Redirect(model.ReturnUrl);
                }
            }

            return this.View("Success",
                this.CreateSuccessViewModel(userAccount, model.ReturnUrl)
            );
        }
    }
}
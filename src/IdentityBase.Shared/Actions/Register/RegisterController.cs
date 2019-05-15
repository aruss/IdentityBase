// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Forms;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Mvc;

    public class RegisterController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly UserAccountService _userAccountService;
        private readonly NotificationService _notificationService;
        private readonly AuthenticationService _authenticationService;
        private readonly IUserAccountStore _userAccountStore;
        private readonly IEmailProviderInfoService _emailProviderInfoService;

        public RegisterController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<RegisterController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            UserAccountService userAccountService,
            ClientService clientService,
            NotificationService notificationService,
            AuthenticationService authenticationService,
            IUserAccountStore userAccountStore,
            IEmailProviderInfoService emailProviderInfoService)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._userAccountService = userAccountService;
            this._notificationService = notificationService;
            this._authenticationService = authenticationService;
            this._userAccountStore = userAccountStore;
            this._emailProviderInfoService = emailProviderInfoService;
        }

        [HttpGet("/register", Name = "Register")]
        [RestoreModelState]
        public async Task<IActionResult> RegisterGet(
            string returnUrl)
        {
            RegisterViewModel vm = await this.CreateViewModelAsync(returnUrl);
            return this.View("Register", vm);
        }

        [HttpPost("/register", Name = "Register")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> RegisterPost(
            RegisterInputModel model)
        {
            BindInputModelResult formResult = await this
                .BindFormInputModelAsync<IRegisterBindInputModelAction>();

            if (!this.ModelState.IsValid)
            {
                return this.RedirectToRoute(
                    "Register",
                    new { ReturnUrl = model.ReturnUrl }
                );
            }

            string email = model.Email.ToLower();

            // Check if user with same email exists
            UserAccount userAccount = await this._userAccountStore
                .LoadByEmailAsync(email);

            // If user dont exists create a new one
            if (userAccount == null)
            {
                return await this.TryCreateNewUserAccount(model);
            }
            // If user has a password then its a local account
            else if (userAccount.HasPassword())
            {
                // If user has a password then its a local account
                this.ModelState.AddModelError(
                    nameof (RegisterViewModel.Email),
                    ErrorMessages.UserAccountAlreadyExists);
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
                "Register",
                await this.CreateViewModelAsync(model, userAccount)
            );
        }

        [HttpGet("/register/confirm", Name = "RegisterConfirm")]
        public async Task<IActionResult> ConfirmGet([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            UserAccount userAccount = result.UserAccount;

            if (userAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                if (userAccount != null)
                {
                    this._userAccountService
                        .ClearVerificationData(userAccount);

                    await this._userAccountStore.WriteAsync(userAccount);
                                       
                    // TODO: emit user updated event 
                }

                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            // User account requires completion.
            if (this._applicationOptions.EnableAccountInvitation &&
                userAccount.CreationKind == CreationKind.Invitation)
            {
                // TODO: move invitation confirmation to own contoller
                //       listening on /invitation/confirm

                ConfirmViewModel vm = new ConfirmViewModel
                {
                    RequiresPassword = !userAccount.HasPassword(),
                    Email = userAccount.Email
                };

                return this.View("Confirm", vm);
            }
            // User account already fine and can be authenticated.
            else
            {
                string returnUrl = userAccount.VerificationStorage;

                this._userAccountService.SetEmailAsVerified(userAccount);

                if (this._applicationOptions.LoginAfterAccountConfirmation)
                {
                    this._userAccountService.SetSuccessfullSignIn(userAccount);
                }

                await this._userAccountStore.WriteAsync(userAccount);

                // TODO: emit user updated event 

                if (this._applicationOptions.LoginAfterAccountConfirmation)
                {
                    await this._authenticationService
                        .SignInAsync(result.UserAccount, returnUrl);

                    // TODO: emit user authenticated event 

                    return this.RedirectToReturnUrl(returnUrl);
                }

                return this.RedirectToLogin(returnUrl);
            }
        }

        // Currently is only used for invitations 
        [HttpPost("/register/confirm", Name = "RegisterConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPost(
            [FromQuery]string key,
            ConfirmInputModel model)
        {
            if (!this._applicationOptions.EnableAccountInvitation)
            {
                return this.NotFound();
            }

            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            UserAccount userAccount = result.UserAccount;

            if (userAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                if (userAccount != null)
                {
                    this._userAccountService
                        .ClearVerificationData(userAccount);

                    await this._userAccountStore.WriteAsync(userAccount);

                    // TODO: emit user updated event 
                }

                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("Confirm", new ConfirmViewModel
                {
                    Email = userAccount.Email
                });
            }

            string returnUrl = userAccount.VerificationStorage;
            this._userAccountService.SetEmailAsVerified(userAccount);
            this._userAccountService.SetPassword(userAccount, model.Password);

            if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                this._userAccountService.SetSuccessfullSignIn(userAccount);
            }

            await this._userAccountStore.WriteAsync(userAccount);

            // TODO: emit user updated event 

            if (result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                return this.RedirectToReturnUrl(returnUrl);
            }
            else if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                await this._authenticationService
                    .SignInAsync(result.UserAccount, returnUrl);

                // TODO: emit user authenticated event

                return this.RedirectToReturnUrl(returnUrl);
            }

            return this.RedirectToLogin(returnUrl);
        }

        /// <summary>
        /// This method removes the user if cancelation token is valid.
        /// </summary>
        /// <param name="key">Cancelation token.</param>
        [HttpGet("/register/cancel", Name = "RegisterCancel")]
        public async Task<IActionResult> CancelGet([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            UserAccount userAccount = result.UserAccount;

            if (userAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                this.AddModelStateError(ErrorMessages.TokenIsInvalid);
                return this.View("InvalidToken");
            }

            string returnUrl = userAccount.VerificationStorage;
            await this._userAccountStore.DeleteByIdAsync(userAccount.Id);

            // TODO: emit user removed event 

            return this.RedirectToReturnUrl(returnUrl);
        }

        [NonAction]
        private async Task<RegisterViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(
                new RegisterInputModel { ReturnUrl = returnUrl }
            );
        }

        [NonAction]
        private async Task<RegisterViewModel> CreateViewModelAsync(
            RegisterInputModel inputModel,
            UserAccount userAccount = null)
        {
            AuthorizationRequest context = await this.InteractionService
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            Client client = this.IdentityBaseContext.Client;

            //Client client = await this._clientService
            //    .FindEnabledClientByIdAsync(context.ClientId);

            //IEnumerable<ExternalProvider> providers = await this._clientService
            //    .GetEnabledProvidersAsync(client);

            RegisterViewModel vm = new RegisterViewModel
            {
                Email = inputModel.Email,
                Password = inputModel.Password,
                PasswordConfirm = inputModel.PasswordConfirm,
                ReturnUrl = inputModel.ReturnUrl,

                EnableAccountRecover =
                    this._applicationOptions.EnableAccountRecovery,

                EnableLocalLogin = client != null ?
                    client.EnableLocalLogin :
                    false && this._applicationOptions.EnableAccountLogin,

                ExternalProviders = await this._authenticationService
                    .GetExternalProvidersAsync(),

                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider),
            };

            vm.FormModel = await this
                .CreateFormViewModelAsync<IRegisterCreateViewModelAction>(vm);

            return vm;
        }

        [NonAction]
        private async Task<IActionResult> CreateSuccessResult(
            UserAccount userAccount,
            string returnUrl)
        {
            return this.View(
                "Success",
                new SuccessViewModel
                {
                    ReturnUrl = returnUrl,

                    Provider = await this._emailProviderInfoService
                        .GetProviderInfo(userAccount.Email)
                }
            );
        }

        [NonAction]
        private async Task<IActionResult> TryMergeWithExistingUserAccount(
            UserAccount userAccount,
            RegisterInputModel inputModel)
        {
            // Merge accounts without user consent
            if (this._applicationOptions.AutomaticAccountMerge)
            {
                this._userAccountService
                    .SetPassword(userAccount, inputModel.Password);

                await this._userAccountStore.WriteAsync(userAccount);

                // TODO: emit user updated event 

                if (this._applicationOptions.LoginAfterAccountCreation)
                {
                    await this._authenticationService
                        .SignInAsync(userAccount, inputModel.ReturnUrl);

                    this._userAccountService.SetSuccessfullSignIn(userAccount);

                    await this._userAccountStore.WriteAsync(userAccount);

                    // TODO: emit user authenticated event 
                }
                else
                {
                    return await this.CreateSuccessResult(
                        userAccount,
                        inputModel.ReturnUrl
                    );
                }
            }
            // Ask user if he wants to merge accounts
            else
            {
                throw new NotImplementedException();
            }

            // Return list of external account providers as hint
            RegisterViewModel vm = new RegisterViewModel
            {
                Email = inputModel.Email,
                Password = inputModel.Password,
                PasswordConfirm = inputModel.PasswordConfirm,
                ReturnUrl = inputModel.ReturnUrl,

                //TODO:; 

                ExternalProviderHints = userAccount.Accounts
                    .Select(s => s.Provider).ToArray()
            };

            return View(vm);
        }

        [NonAction]
        private async Task<IActionResult> TryCreateNewUserAccount(
            RegisterInputModel model)
        {
            DateTime now = DateTime.UtcNow;
            Guid userAccountId = Guid.NewGuid();

            var userAccount = new UserAccount
            {
                Id = userAccountId,
                Email = model.Email,
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsActive = true
            };

            this._userAccountService.SetPassword(userAccount, model.Password);

            if (this._applicationOptions.RequireLocalAccountVerification)
            {
                this._userAccountService.SetVerificationData(
                     userAccount,
                     VerificationKeyPurpose.ConfirmAccount,
                     model.ReturnUrl,
                     now
                );
            }

            if (this._applicationOptions.LoginAfterAccountCreation)
            {
                this._userAccountService.SetSuccessfullSignIn(userAccount);
            }

            await this._userAccountStore.WriteAsync(userAccount);
            // TODO: Emit user updated event

            // Send email but only after successfull user account update 
            if (this._applicationOptions.RequireLocalAccountVerification)
            {
                await this._notificationService
                    .SendUserAccountCreatedEmailAsync(userAccount);
            }

            // Authenticate user but only after success account update 
            if (this._applicationOptions.LoginAfterAccountCreation)
            {
                await this._authenticationService
                  .SignInAsync(userAccount, model.ReturnUrl);

                // TODO: emit user authenticated event
                return this.RedirectToReturnUrl(model.ReturnUrl);
            }

            return await this.CreateSuccessResult(
                userAccount,
                model.ReturnUrl
            );
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
        //       userAccount.IsActive = true;
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
    }
}
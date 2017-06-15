using AngleSharp.Parser.Html;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServiceBase.Extensions;
using IdentityBase.Configuration;
using ServiceBase.Notification.Email;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests
{
    // http://stackoverflow.com/questions/30557521/how-to-access-httpcontext-inside-a-unit-test-in-asp-net-5-mvc-6

    [Collection("Login")]
    public class RegisterTests
    {
        private string _returnUrl = "%2Fconnect%2Fauthorize%2Flogin%3Fclient_id%3Dmvc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%253A3308%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dopenid%2520profile%2520api1%26response_mode%3Dform_post%26nonce%3D636170876883483776.ZGUwYWY2NDctNDJlNy00MTVmLTkwZTYtZjVjMTQ4ZWVlMzAwMWM2OWNhODQtYzZjOS00ZDljLTk3NTktYWE1ZWExMDEwYzk2%26state%3DCfDJ8McEKbBuVCdHkFjjPyy6vSPN5QZvt6xKTHnnKEyNzXwN1YpWo0Mslqn-wBoHhp9vMSjqo3GQGU7emMMhZlgu0BK3G03m2uqLc5vrYBz06tcWr8S4f9oKl2u1S0cAiJEOw13GnuF-EJ0E3by0nUJ3m1MhhnovobqqTEpKMldmLGpaUxPS4YGxSQVgzDzo3XsyHB4KvWlsdnb3InqNoPKnTQ4ljgDOAeKTAMj39Jz1SMauTcfOXHDyCnJdLt7I0v0up1oY5Az9b7xjzk0oBq5P7lADyq88YTEG0EALJG8SgjYi-Ch-0jd26w74LJ5UyQNScc1ZS4n9dMKUHXvuuIWllzNK86la5X-ydnsNZo2a1HsHyPT4NHe6EG2LdVkh6Y-2-A";

        #region Invalid input

        [Fact]
        public async Task RegisterWithInvalidPassword()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true,
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var doc = (new HtmlParser().Parse(response.Content.ReadAsStringAsync().Result));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "one that does not match" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();

            // doc = (new HtmlParser().Parse(postResponse.Content.ReadAsStringAsync().Result));
        }

        [Fact]
        public async Task RegisterWithInvalidEmail()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true,
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","johnocalhost"},
                {"Password", "password"},
                {"PasswordConfirm", "password" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();
        }

        #endregion Invalid input

        #region Valid input, invalid user

        /// <summary>
        /// Try to register with a email that is already registered and confirmed
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterWithExistingActiveAccount()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","alice@localhost"},
                {"Password", "password"},
                {"PasswordConfirm", "password" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();
        }

        [Fact]
        public async Task RegisterWithDisabledAccount()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","jim@localhost"},
                {"Password", "password"},
                {"PasswordConfirm", "password" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();
        }

        [Fact]
        public async Task RegisterWithUnconfirmedAccount()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","paul@localhost"},
                {"Password", "password"},
                {"PasswordConfirm", "password" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();
        }

        #endregion Valid input, invalid user

        #region Valid input, successfull registration

        /// <summary>
        /// Register valid user, confirm the email, login after confirmation automatically
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterValidUser_ConfirmLink_LoginAfterConfirmation()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            Func<string, string, object, Task> sendEmailAsync = async (templateName, email, viewData) =>
            {
                // 4. Get email and confirm registration by calling confirmation link
                var dict = viewData.ToDictionary();
                var confirmResponse = await client.GetAsync((string)dict["ConfirmUrl"]);

                // 5. After confirmation user should be logged in
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.Found);
                confirmResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
            };

            emailServiceMock.Setup(c =>
                c.SendEmailAsync("UserAccountCreated", "john@localhost", It.IsAny<object>(), It.IsAny<bool>())).Returns(sendEmailAsync);

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "john@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/register/success");
        }

        /// <summary>
        /// Register valid user, confirm the mail, get redirected to login page
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterValidUser_ConfirmLink_RedirectToLoginPageAfterConfirmation()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            Func<string, string, object, Task> sendEmailAsync = async (templateName, email, viewData) =>
            {
                // 4. Get email and confirm registration by calling confirmation link
                var dict = viewData.ToDictionary();
                var confirmResponse = await client.GetAsync((string)dict["ConfirmUrl"]);

                // 5. After confirmation user should be logged in
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.Found);
                confirmResponse.Headers.Location.ToString().Should().StartWith("/login");
            };

            emailServiceMock.Setup(c =>
                c.SendEmailAsync("UserAccountCreated", "john@localhost", It.IsAny<object>(), It.IsAny<bool>())).Returns(sendEmailAsync);

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = false,
                    RequireLocalAccountVerification = true
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "john@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/register/success");
        }

        /// <summary>
        /// Register valid user and get automatically authenticated,
        /// confirm the mail and stay authenticated,
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterValidUser_LoginAfterRegistration_ConfirmLink()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            Func<string, string, object, Task> sendEmailAsync = async (templateName, email, viewData) =>
            {
                // 4. Get email and confirm registration by calling confirmation link
                var dict = viewData.ToDictionary();
                var confirmResponse = await client.GetAsync((string)dict["ConfirmUrl"]);

                // 5. After confirmation user should be logged in
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.Found);
                confirmResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
            };

            emailServiceMock.Setup(c =>
                c.SendEmailAsync("UserAccountCreated", "john@localhost", It.IsAny<object>(), It.IsAny<bool>())).Returns(sendEmailAsync);

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = true,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "john@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
        }

        /// <summary>
        /// Register valid user and get automatically authenticated,
        /// IdSrv will not send any confirmation mails
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterValidUser_LoginAfterRegistration_NoConfirmMail()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = true,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = false
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "john@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");

            // There should be no email sent
            emailServiceMock.Verify(c => c.SendEmailAsync("UserAccountCreated", "john@localhost", It.IsAny<object>(), true), Times.Never());
        }

        /// <summary>
        /// Register valid user, cancel the mail, get redirected to login page
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterValidUser_CancelLink()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            Func<string, string, object, Task> sendEmailAsync = async (templateName, email, viewData) =>
            {
                // 4. Get email and confirm registration by calling confirmation link
                var dict = viewData.ToDictionary();
                var confirmResponse = await client.GetAsync((string)dict["CancelUrl"]);

                // 5. After confirmation user should be redirected to login page
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.Found);
                confirmResponse.Headers.Location.ToString().Should().StartWith("/login");
            };

            emailServiceMock.Setup(c =>
                c.SendEmailAsync("UserAccountCreated", "john@localhost", It.IsAny<object>(), It.IsAny<bool>())).Returns(sendEmailAsync);

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","john@localhost"},
                {"Password", "john@localhost"},
                {"PasswordConfirm", "john@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/register/success");
        }

        #endregion Valid input, successfull registration

        [Fact]
        public async Task RegisterUserWhichHasAlreadyAExternalAccount_MergeAccountsAutomatically()
        {
            var emailServiceMock = new Mock<IEmailService>();
            HttpClient client = null;

            Func<string, string, object, Task> sendEmailAsync = async (templateName, email, viewData) =>
            {
                // 4. Get email and confirm registration by calling confirmation link
                var dict = viewData.ToDictionary();
                var confirmResponse = await client.GetAsync((string)dict["ConfirmUrl"]);

                // 5. After confirmation user should be logged in
                confirmResponse.StatusCode.Should().Be(HttpStatusCode.Found);
                confirmResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
            };

            emailServiceMock.Setup(c =>
                c.SendEmailAsync("UserAccountCreated", "bob@localhost", It.IsAny<object>(), true)).Returns(sendEmailAsync);

            var server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = true,
                    AutomaticAccountMerge = true
                });
            });

            client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","bob@localhost"},
                {"Password", "bob@localhost"},
                {"PasswordConfirm", "bob@localhost" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The registration success page should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/register/success");
        }

        /*[Fact]
        public async Task RegisterUserWhichHasAlreadyAExternalAccount_AskIfMerge_Merge()
        {
        }

        [Fact]
        public async Task RegisterWithExistingInactiveNotConfirmedAccount()
        {
            var server = ServerHelper.CreateServer((services) =>
            {
                var emailServiceMock = new Mock<IEmailService>();
                services.AddSingleton<IEmailService>(emailServiceMock.Object);

                services.AddSingleton(new ApplicationOptions
                {
                    LoginAfterAccountCreation = false,
                    LoginAfterAccountConfirmation = true,
                    RequireLocalAccountVerification = false
                });
            });

            var client = server.CreateClient();

            // 1. Navigate to register page
            var response = await client.GetAsync("/register?returnUrl=" + _returnUrl);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(html));

            // 2. Post registration form
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","paul@localhost"},
                {"Password", "password"},
                {"PasswordConfirm", "password" },
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = response.CreatePostRequest("/register", formPostBodyData);
            var postResponse = await client.SendAsync(postRequest);

            // 3. The error should be shown
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            postResponse.Headers.Location.Should().BeNull();
        }

         */
    }
}
namespace IdentityBase.Public.IntegrationTests.Tests
{
    // Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
    // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IdentityBase.Public.Api.Invitations;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;
    using Xunit;

    namespace IdentityModel.Client
    {
        [Collection("Login Tests")]
        public class InviteTests
        {
            [Fact(DisplayName = "Disco Client")]
            public async Task FooTask()
            {
                IConfigurationRoot config = ConfigBuilder.Default
                      .Alter("App:EnableInvitationCreateEndpoint", "true")
                      .Build();

                TestServer testServer = TestServerBuilder
                    .BuildServer<Startup>(config);

                HttpClient client = await testServer
                    .LoginAndGetAuthorizedClientAsync();

                HttpResponseMessage response = await client
                    .PutJsonAsync("/invitations", new
                    {
                        Email = "invited@localhost",
                        // Target client, is mostly one with GUI
                        ClientId = "mvc.hybrid"
                    });

                response.EnsureSuccessStatusCode();


                var schema = SchemaUtils.GenerateSchema<InvitationsPutResultModel>();

                response.AssertSchema(@"{
                      'type': 'object',
                      'additionalProperties' : false,
                      'properties': {
                        'id': {
                          'type': [
                            'string',
                            'null'
                          ]
                        },
                        'error': {},
                        'stackTrace': {
                          'type': [
                            'string',
                            'null'
                          ]
                        }
                      },
                      'required': [
                        'type',
                        'error',
                        'stackTrace'
                      ]
                    }");
            }







            [Fact(DisplayName = "Invite_User")]
            public async Task Invite_User()
            {
                string confirmUrl = null;
                string cancelUrl = null;


                // Mock the email service to intercept the outgoing email messages
                var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                    IdentityBaseConstants.EmailTemplates.UserAccountInvited,
                    "invited@localhost",
                    (templateName, emailTo, viewData, isHtml) =>
                    {
                        // 3. Get confirm url 
                        confirmUrl = viewData.ToDictionary()["ConfirmUrl"] as string;
                        cancelUrl = viewData.ToDictionary()["CancelUrl"] as string;
                    });

                // Create a server with custom configuration 
                var config = ConfigBuilder.Default
                   // remove the default service since we mocking it
                   .RemoveDefaultMailService()
                   .Alter("App:EnableInvitationCreateEndpoint", "true")
                   .Build();

                var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
                {
                    services.AddSingleton(emailServiceMock.Object);
                });
                var client = server.CreateClient();

                // Act
                var response = await client.PutJsonAsync("/invitations", new
                {
                    Email = "invited@localhost",
                    ClientId = "mvc.hybrid"
                });
                response.EnsureSuccessStatusCode();

                // Try to follow the confirmation link again it should return an error 
                var response3 = await client.GetAsync(confirmUrl);
                response3.StatusCode.Should().Be(HttpStatusCode.OK);
                var doc2 = await response3.Content.ReadAsHtmlDocumentAsync();
            }
        }
    }
}

namespace ServiceBase.Tests
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using ServiceBase.Notification.Email;
    using Xunit;

    /// <summary>
    /// <see cref="IEmailService"/> test helper
    /// </summary>
    public static class EmailServiceHelper
    {
        /// <summary>
        /// Creates a <see cref="Mock{IEmailService}"/> instance that will
        /// mock a single SendEmailAsync call
        /// </summary>
        /// <param name="templateName">Email template.</param>
        /// <param name="email">Email address.</param>
        public static Mock<IEmailService> GetEmailServiceMock(
            string templateName,
            string email,
            Action<string, string, object, bool> returns = null)
        {
            var emailServiceMock = new Mock<IEmailService>();

            emailServiceMock.Setup(c => c.SendEmailAsync(
                templateName,
                email,
                It.IsAny<object>(),
                It.IsAny<bool>())
            )
            .Returns(new Func<string, string, object, bool, Task>(
                async (templateNameOut, emailOut, viewData, isHtml) =>
                {
                    Assert.Equal(templateName, templateNameOut);
                    Assert.Equal(email, emailOut);

                    returns?.Invoke(
                        templateNameOut,
                        emailOut,
                        viewData,
                        isHtml);
                }));

            return emailServiceMock;
        }
    }
}

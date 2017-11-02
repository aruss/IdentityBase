namespace ServiceBase.Tests
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using ServiceBase.Notification.Email;
    using Xunit;

    public static class EmailServiceHelper
    {
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

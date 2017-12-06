namespace IdentityBase.IntegrationTests
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
    using ServiceBase.Tests;

    public static class IHtmlDocumentExtensions
    {
        public static void ShouldContainErrors(
            this IHtmlDocument doc,
            params string[] errors)
        {
            IElement elm = doc.QuerySelector(".alert.alert-danger");

            foreach (var item in errors)
            {
                elm.TextContent.Contains(item).Should().BeTrue(); 
            }          
        }
        
        public static string GetReturnUrl(this IHtmlDocument doc)
        {
            return doc.GetInputValue("ReturnUrl");
        }
    }
}

namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using AngleSharp.Parser.Html;
    using FluentAssertions;

    // http://www.stefanhendriks.com/2016/04/29/integration-testing-your-dot-net-core-app-with-an-in-memory-database/
    // http://www.stefanhendriks.com/2016/05/11/integration-testing-your-asp-net-core-app-dealing-with-anti-request-forgery-csrf-formdata-and-cookies/

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

        public static async Task<IHtmlDocument> ReadAsHtmlDocumentAsync(
            this HttpContent content)
        {
            string html = await content.ReadAsStringAsync();
            return new HtmlParser().Parse(html);
        }

        public static string GetInputValue(
            this IHtmlDocument doc,
            string name)
        {
            IElement node = doc.QuerySelector($"input[name=\"{name}\"]");

            if (node != null)
            {
                return node.GetAttribute("value");
            }

            return null;
        }

        public static string GetAntiForgeryToken(this IHtmlDocument doc)
        {
            return doc.GetInputValue("__RequestVerificationToken"); 
        }

        public static string GetReturnUrl(this IHtmlDocument doc)
        {
            return doc.GetInputValue("ReturnUrl");
        }

        public static string GetFormAction(this IHtmlDocument doc)
        {
            return doc.QuerySelector("form").GetAttribute("action"); 
        }
    }
}

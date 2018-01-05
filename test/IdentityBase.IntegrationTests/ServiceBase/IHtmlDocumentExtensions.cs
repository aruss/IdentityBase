namespace ServiceBase.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using AngleSharp.Parser.Html;

    // http://www.stefanhendriks.com/2016/04/29/integration-testing-your-dot-net-core-app-with-an-in-memory-database/
    // http://www.stefanhendriks.com/2016/05/11/integration-testing-your-asp-net-core-app-dealing-with-anti-request-forgery-csrf-formdata-and-cookies/

    public static class IHtmlDocumentExtensions
    {
        /// <summary>
        /// Serialize the HTTP content to a string and parses to
        /// <see cref="IHtmlDocument"/> as an asynchronous operation. 
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Instance of <see cref="HttpContent"/>.</returns>
        public static async Task<IHtmlDocument> ReadAsHtmlDocumentAsync(
            this HttpContent content)
        {
            string html = await content.ReadAsStringAsync();
            return new HtmlParser().Parse(html);
        }

        /// <summary>
        /// Returns all form element values as
        /// <see cref="Dictionary{string, string}"/>. Does not support options
        /// and textareas 
        /// </summary>
        /// <param name="doc">Instance of <see cref="IHtmlDocument"/>.</param>
        /// <returns>Instance of <see cref="IHtmlDocument"/>.</returns>
        public static Dictionary<string, string> GetFormInputs(
            this IHtmlDocument doc)
        {
            var nodes = doc.QuerySelectorAll($"input");
            var result = new Dictionary<string, string>();

            var groups = nodes.GroupBy(s => s.GetAttribute("name"));
            foreach (var group in groups)
            {
                var idx = 0;
                var count = group.Count();

                foreach (var item in group)
                {
                    result.Add(
                        count > 1 ? group.Key + "[" + idx++ + "]" : group.Key,
                        item.GetAttribute("value")
                    );
                }
            }

            return result;
        }

        /// <summary>
        /// Returns value of input element with name provided via
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="doc">Instance of <see cref="IHtmlDocument"/>.</param>
        /// <param name="name">Name of the input element.</param>
        /// <returns>Instance of <see cref="IHtmlDocument"/>.</returns>
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

        /// <summary>
        /// Returns value of __RequestVerificationToken element. 
        /// </summary>
        /// <param name="doc">Instance of <see cref="IHtmlDocument"/>.</param>
        /// <returns>Request verification token.</returns>
        public static string GetAntiForgeryToken(this IHtmlDocument doc)
        {
            return doc.GetInputValue("__RequestVerificationToken");
        }

        /// <summary>
        /// Returns value of action attribute of the first form element.
        /// </summary>
        /// <param name="doc">Instance of <see cref="IHtmlDocument"/>.</param>
        /// <returns>Form action URL.</returns>
        public static string GetFormAction(this IHtmlDocument doc)
        {
            return doc.QuerySelector("form").GetAttribute("action");
        }
    }
}

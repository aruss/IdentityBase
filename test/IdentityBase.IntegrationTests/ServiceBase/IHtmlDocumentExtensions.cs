namespace ServiceBase.Tests
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using AngleSharp.Parser.Html;
    using System.Collections.Specialized;

    // http://www.stefanhendriks.com/2016/04/29/integration-testing-your-dot-net-core-app-with-an-in-memory-database/
    // http://www.stefanhendriks.com/2016/05/11/integration-testing-your-asp-net-core-app-dealing-with-anti-request-forgery-csrf-formdata-and-cookies/

    public static class IHtmlDocumentExtensions
    {
        public static async Task<IHtmlDocument> ReadAsHtmlDocumentAsync(
            this HttpContent content)
        {
            string html = await content.ReadAsStringAsync();
            return new HtmlParser().Parse(html);
        }

        /// <summary>
        /// Does not support options and textareas 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
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

        public static string GetFormAction(this IHtmlDocument doc)
        {
            return doc.QuerySelector("form").GetAttribute("action");
        }
    }
}

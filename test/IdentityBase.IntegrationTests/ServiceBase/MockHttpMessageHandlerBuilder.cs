namespace ServiceBase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;

    public class MockHttpMessageHandlerBuilder
    {
        private Dictionary<string, Func<HttpResponseMessage>> responses;

        public MockHttpMessageHandlerBuilder()
        {
            this.responses =
                new Dictionary<string, Func<HttpResponseMessage>>();
        }

        public HttpMessageHandler Build()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns((
                    HttpRequestMessage request,
                    CancellationToken cancellationToken) =>
                        GetMockResponse(request, cancellationToken));

            return mockHttpMessageHandler.Object;
        }

        private Task<HttpResponseMessage> GetMockResponse(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (this.responses.ContainsKey(request.RequestUri.LocalPath))
            {
                Func<HttpResponseMessage> func =
                    this.responses[request.RequestUri.LocalPath];

                HttpResponseMessage response = func.Invoke();
                response.RequestMessage = request;

                return Task.FromResult(response);
            }

            throw new NotImplementedException();
        }

        public MockHttpMessageHandlerBuilder AddEndpoint(
            string path,
            Func<HttpResponseMessage> func)
        {
            this.responses.Add(path, func);
            return this;
        }
    }
}

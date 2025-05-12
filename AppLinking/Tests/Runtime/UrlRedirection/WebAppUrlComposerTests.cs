#if NUGET_MOQ_AVAILABLE && !ENABLE_IL2CPP
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Unity.Cloud.Common;

namespace Unity.Cloud.AppLinking.Tests
{
    public class WebAppUrlComposerTests
    {
        static class TestParameters
        {
            internal const string WebAppEndpoint = "https://test.domain/app-linking-v1/web-app-urls";
        }

         WebAppUrlComposer m_WebAppUrlComposer;

        [SetUp]
        public void SetUp()
        {
            var mockServiceHostResolver = new Mock<IServiceHostResolver>();
            var mockHttpClient = new Mock<IHttpClient>();
            mockServiceHostResolver.Setup(p => p.GetResolvedRequestUri(It.IsAny<string>(), ServiceProtocol.Http))
                .Returns(TestParameters.WebAppEndpoint);

            // Return simulated JSON result
            var webAppBaseUrlsJson = @"{
                ""asset-manager"": ""https://cloud.unity.com/home"",
                ""docs"": ""https://docs.unity.com"",
                ""unsupported"": null,
            }";

            var webAppResponse = new HttpResponseMessage();
            webAppResponse.Content = new StringContent(webAppBaseUrlsJson);

            mockHttpClient.Setup(p => p.SendAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<HttpCompletionOption>(),
                    It.IsAny<IProgress<HttpProgress>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(webAppResponse));

            m_WebAppUrlComposer = new WebAppUrlComposer(mockServiceHostResolver.Object, mockHttpClient.Object);

        }

        [TestCase("docs", null, "https://docs.unity.com")]
        [TestCase("docs", "/simple_value", "https://docs.unity.com/simple_value")]
        [TestCase("docs", "/a/long/path", "https://docs.unity.com/a/long/path")]
        [TestCase("asset-manager", "/a/long/path?query=true", "https://cloud.unity.com/home/a/long/path?query=true")]
        [TestCase("unsupported", "any_value", null)]
        [TestCase("not-found", "any_value", null)]
        public void ComposedUrlAsyncTest(string webAppName, string pathAndQuery, string expectedUrl)
        {

            if (expectedUrl == null)
            {
                // If null, we expect an InvalidArgumentException
                var aggregateException = Assert.Catch<AggregateException>(() =>
                {
                    _ = m_WebAppUrlComposer.ComposeUrlAsync(webAppName, pathAndQuery).Result;
                });
                Assert.IsInstanceOf<InvalidArgumentException>(aggregateException.InnerException);
            }
            else
            {
                var redirectionUrl = m_WebAppUrlComposer.ComposeUrlAsync(webAppName, pathAndQuery).Result;
               Assert.AreEqual(expectedUrl, redirectionUrl);
            }
        }

        [TestCase("docs", true)]
        [TestCase("asset-manager", true)]
        [TestCase("unsupported", false)]
        [TestCase("not-found", false)]
        public void IsWebAppSupportedAsyncTest(string webAppName, bool expectedResult)
        {
            var isWebAppSupported = m_WebAppUrlComposer.IsWebAppSupportedAsync(webAppName).Result;
            Assert.AreEqual(expectedResult, isWebAppSupported);
        }
    }
}
#endif

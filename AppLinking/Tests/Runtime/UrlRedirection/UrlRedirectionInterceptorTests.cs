#if NUGET_MOQ_AVAILABLE && !ENABLE_IL2CPP
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Moq;
using NUnit.Framework;
using Unity.Cloud.AppLinking.Runtime;
using Task = System.Threading.Tasks.Task;

namespace Unity.Cloud.AppLinking.Tests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class UrlRedirectionInterceptorTests
    {
        static readonly Dictionary<string, DeepLinkContainer> k_DeepLinks = new ()
        {

            // Invalid
            {
                "Null url", new (deepLink: null, isValidUri: false, canBeProcessed: false)
            },
            {
                "Empty url", new (deepLink: "", isValidUri: false, canBeProcessed: false)
            },
            {
                "Whitespace", new (deepLink: "  ", isValidUri: false, canBeProcessed: false)
            },
            {
                "Url: abc123", new (deepLink: "abc123", isValidUri: false, canBeProcessed: false)
            },
            {
                "Null url and null arguments",
                new
                (
                    scheme: null,
                    host: null,
                    restFragments: null,
                    awaitedQueryArguments: null,
                    queryArgs: null,
                    isValidUri: false,
                    canBeProcessed: false
                )
            },
            {
                "Empty url and  null arguments",
                new
                (
                    scheme: string.Empty,
                    host: string.Empty,
                    restFragments: new List<string>(),
                    awaitedQueryArguments: new List<string>(),
                    queryArgs: new Dictionary<string, string>(),
                    isValidUri: false,
                    canBeProcessed: false
                )

            },
            {
                "Url: reflect://implicit/callback/login",
                new
                (
                    scheme: "reflect",
                    host: "implicit",
                    restFragments: new List<string> {"callback", "login"},
                    awaitedQueryArguments: new List<string> { "code", "state" },
                    queryArgs: null,
                    isValidUri: true,
                    canBeProcessed: false
                )

            },
            {
                "Url: http://localhost:1234/",
                new
                (
                    scheme: "http",
                    host: "localhost:1234",
                    restFragments: null,
                    awaitedQueryArguments: null,
                    queryArgs: null,
                    isValidUri: true,
                    canBeProcessed: false
                )
            },
            {
                "Url: reflecto://implicit/callback/login/?unexpected=123&query=abc",
                new
                (
                    scheme: "reflecto",
                    host: "implicit",
                    restFragments: new List<string> {"callback", "login"},
                    awaitedQueryArguments: new List<string> { "code", "state" },
                    queryArgs: new Dictionary<string, string>() { {"unexpected", "123"} , {"query", "abc"}},
                    isValidUri: true,
                    canBeProcessed: false
                )

            },
            {
                "Url: scheme://some-host/",
                new
                (
                    scheme: "scheme",
                    host: "some-host",
                    restFragments: new List<string> {"random", "rest", "fragment"},
                    awaitedQueryArguments: new List<string> { "some", "query" },
                    queryArgs: new Dictionary<string, string>() { {"some", "query"} },
                    isValidUri: true,
                    canBeProcessed: true
                )
            },

            // Valid
            {
                "Url: http://localhost:1234/callback/login",
                new
                (
                    scheme: "http",
                    host: "localhost:1234",
                    restFragments: new List<string> {"callback", "login"},
                    awaitedQueryArguments: new List<string> { "code", "state" },
                    queryArgs: null,
                    isValidUri: true,
                    canBeProcessed: true
                )
            },
            {
                "Url: https://mock.unity.com/implicit/callback/login/",
                new
                (
                    scheme: "https",
                    host: "mock.unity.com",
                    restFragments: new List<string> {"callback", "login"},
                    awaitedQueryArguments: new List<string> { "code", "state" },
                    queryArgs: null,
                    isValidUri: true,
                    canBeProcessed: true
                )
            },
            {
                "Url: reflecto://implicit/callback/login/?code=123&state=abc",
                new
                (
                    scheme: "reflecto",
                    host: "implicit",
                    restFragments: new List<string> {"callback", "login"},
                    awaitedQueryArguments: new List<string> { "code", "state" },
                    queryArgs: new Dictionary<string, string>() { {"code", "123"} , {"state", "abc"}},
                    isValidUri: true,
                    canBeProcessed: true
                )
            },
        };

        Mock<IUrlRedirectAwaiter> m_AwaiterMock;
        readonly bool m_MockHostDomain;

        public UrlRedirectionInterceptorTests(bool mockHostDomain)
        {
            m_MockHostDomain = mockHostDomain;
        }

        [SetUp]
        public void Setup()
        {
            // Reduce the timeout delay to accelerate tests
            m_AwaiterMock = new Mock<IUrlRedirectAwaiter>();
        }

        [Test]
        public void AwaitRedirectTest_ResultReceived()
        {
            // Given a UrlRedirectionInterceptor
            var urlRedirectionInterceptor = GivenAUrlRedirectionInterceptor();
            Assert.IsNull(urlRedirectionInterceptor.GetRedirectionResult());

            // With an awaiter that will receive a result
            var expectedRedirectResult = new UrlRedirectResult { Status = UrlRedirectStatus.Success };
            m_AwaiterMock.Setup(a => a.HasTimedOut)
                .Returns(false);
            m_AwaiterMock.Setup(a => a.WaitForRefreshAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            m_AwaiterMock.Setup(a => a.RedirectResult)
                .Returns(expectedRedirectResult);

            // Returns result after 2 Waits
            m_AwaiterMock.SetupSequence(a => a.HasResult)
                .Returns(false)
                .Returns(false)
                .Returns(true);

            var result = urlRedirectionInterceptor.AwaitRedirectAsync().Result;

            // Then the a redirect result is returned
            Assert.AreEqual(expectedRedirectResult, result);

            // And the result is stored in the UrlRedirectionInterceptor
            var storedResult = urlRedirectionInterceptor.GetRedirectionResult();
            Assert.IsTrue(storedResult.HasValue);
            Assert.AreEqual(expectedRedirectResult, storedResult.Value);
        }

        [Test]
        public void AwaitRedirectTest_Timeout()
        {
            // Given a UrlRedirectionInterceptor
            var urlRedirectionInterceptor = GivenAUrlRedirectionInterceptor();

            // With an awaiter that will timeout
            m_AwaiterMock.Setup(a => a.HasTimedOut)
                .Returns(true);

            // When awaiting a deeplink activation that is never received
            // Then a TimeoutException is thrown
            var aggregateException = Assert.Catch<AggregateException>(() => urlRedirectionInterceptor.AwaitRedirectAsync().Wait());
            Assert.IsInstanceOf<TimeoutException>(aggregateException.InnerException);
        }

        [Test]
        public void InterceptAwaitedUrlTest([ValueSource(nameof(k_DeepLinks))] KeyValuePair<string, DeepLinkContainer> testCase)
        {
            var deepLinkContainer = testCase.Value;

            // Given a UrlRedirectionInterceptor
            var urlRedirectionInterceptor = GivenAUrlRedirectionInterceptor(m_MockHostDomain ? deepLinkContainer.Host : null);

            // When a deeplink is processed
            if (deepLinkContainer.CanBeProcessed)
                // Given the deeplink parameters are valid
                // Then processing the deeplink does not throw an exception
                Assert.DoesNotThrow(() => urlRedirectionInterceptor.InterceptAwaitedUrl(deepLinkContainer.DeepLink, deepLinkContainer.AwaitedQueryArguments));
        }

        [Test]
        public void OnDeepLinkTest([ValueSource(nameof(k_DeepLinks))] KeyValuePair<string, DeepLinkContainer> testCase)
        {
            var deepLinkContainer = testCase.Value;

            // Given a UrlRedirectionInterceptor
            var urlRedirectionInterceptor = GivenAUrlRedirectionInterceptor(m_MockHostDomain ? deepLinkContainer.Host : null);

            // When a deeplink is received
            if (deepLinkContainer.IsValidUri)
            {
                // Given the deeplink uri can be parsed
                if (deepLinkContainer.CanBeProcessed)
                    // Given the deeplink can be parsed
                    // Then processing the deeplink does not throw an exception
                    Assert.DoesNotThrow(() => urlRedirectionInterceptor.InterceptAwaitedUrl(deepLinkContainer.DeepLink));
            }
            else
            {
                // Given the deeplink uri cannot be parsed
                // Then the correct exception is thrown
                Assert.Throws<ArgumentException>(() => urlRedirectionInterceptor.InterceptAwaitedUrl(deepLinkContainer.DeepLink));
            }
        }

        UrlRedirectionInterceptor GivenAUrlRedirectionInterceptor(string hostDomain = null)
        {
            var urlRedirectionInterceptor = new UrlRedirectionInterceptor(m_AwaiterMock.Object, hostDomain);
            return urlRedirectionInterceptor;
        }
    }

    /// <summary>
    /// A container to facilitate the validation of deeplink test cases.
    /// Can be used to build a URI and indicate it's expected components, route, and validity
    /// </summary>
    public class DeepLinkContainer
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public List<string> RestFragments { get; set; }
        public List<string> AwaitedQueryArguments { get; set; }
        public Dictionary<string, string> QueryArgs { get; set; }
        public bool IsValidUri { get; set; }
        public bool CanBeProcessed { get; set; }

        public string DeepLink { get; }

        /// <summary>
        /// Construct a deeplink out of provided parameters
        /// </summary>
        public DeepLinkContainer(string scheme, string host, List<string> restFragments, List<string> awaitedQueryArguments, Dictionary<string, string> queryArgs, bool isValidUri, bool canBeProcessed)
        {
            // Build up the deeplink
            var deepLinkBuilder = new StringBuilder();

            deepLinkBuilder.Append($"{scheme}://");

            if (!string.IsNullOrEmpty(host))
                deepLinkBuilder.Append($"{host}/");

            if (restFragments != null && restFragments.Count > 0)
            {
                for (int i = 0; i < restFragments.Count; ++i)
                {
                    if (i > 0)
                        deepLinkBuilder.Append("/");
                    deepLinkBuilder.Append($"{restFragments[i]}");
                }
            }

            if (queryArgs != null && queryArgs.Count > 0)
            {
                var queryCount = 0;
                foreach (var queryArg in queryArgs)
                {
                    deepLinkBuilder.Append(queryCount == 0 ? "/?" : "&");
                    deepLinkBuilder.Append($"{queryArg.Key}={queryArg.Value}");
                    queryCount++;
                }
            }

            // Store expected results
            Scheme = scheme;
            Host = host;
            RestFragments = restFragments;
            AwaitedQueryArguments = awaitedQueryArguments;
            QueryArgs = queryArgs;
            IsValidUri = isValidUri;
            CanBeProcessed = canBeProcessed;

            DeepLink = deepLinkBuilder.ToString();
        }

        /// <summary>
        /// Directly assign a deeplink string
        /// </summary>
        public DeepLinkContainer(string deepLink, bool isValidUri, bool canBeProcessed)
        {
            DeepLink = deepLink;
            IsValidUri = isValidUri;
            CanBeProcessed = canBeProcessed;
        }
    }
}
#endif

#if UC_DEV_TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Unity.Cloud.Common
{
    [Serializable]
    class HttpRequestMessageModifierSchema
    {
        public HttpRequestMessageModifierRequestSchema[] Requests { get; set; }
    }

    [Serializable]
    class HttpRequestMessageModifierRequestSchema
    {
        public HttpRequestMessageModifierFilterSchema Filter { get; set; }
        public HttpRequestMessageModifierReplaceSchema Replace { get; set; }
    }

    [Serializable]
    class HttpRequestMessageModifierFilterSchema
    {
        public string Method { get; set; }
        public string UriProperty { get; set; }
        public string IsMatch { get; set; }
    }

    [Serializable]
    class HttpRequestMessageModifierReplaceSchema
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Scheme { get; set; }
    }

    interface IHttpRequestMessageModifier
    {
        void Modify(HttpRequestMessage request);
    }

    class HttpRequestMessageModifier: IHttpRequestMessageModifier
    {
        readonly List<HttpRequestMessageModifierRequest> m_RequestModifiers = new();

        public HttpRequestMessageModifier(HttpRequestMessageModifierSchema schema)
        {
            Initialize(schema);
        }

        public HttpRequestMessageModifier(string schemaJsonContent)
        {
            Initialize(JsonSerialization.Deserialize<HttpRequestMessageModifierSchema>(schemaJsonContent));
        }

        void Initialize(HttpRequestMessageModifierSchema schema)
        {
            foreach (var requestModifierSchema in schema.Requests)
            {
                m_RequestModifiers.Add(new HttpRequestMessageModifierRequest(requestModifierSchema));
            }
        }

        public void Modify(HttpRequestMessage request)
        {
#pragma warning disable S3267
            // Applying SonarQube's solution for code smell S3257 (Loops should be simplified with "LINQ" expressions)
            // creates another code smell and makes the code more confusing.
            foreach (var requestModifier in m_RequestModifiers)
            {
                if (requestModifier.Modify(request))
                    break; // only apply the first match
            }
#pragma warning disable S3267
        }
    }

    static class HttpRequestMessageModifierFactory
    {
        static readonly UCLogger s_Logger = LoggerProvider.GetLogger(typeof(HttpRequestMessageModifierFactory).FullName);

        const string kEnvironmentVariable = "UNITY_CLOUD_MIDDLEWARE_MODIFIER";

        public static HttpRequestMessageModifier CreateFromEnvironmentVariable()
        {
            var environmentVariableValue = Environment.GetEnvironmentVariable(kEnvironmentVariable);
            if (string.IsNullOrEmpty(environmentVariableValue))
                return null;

            s_Logger.LogDebug($"Found environment variable {kEnvironmentVariable} with content {environmentVariableValue}.");
            return CreateFromPath(environmentVariableValue);
        }

        public static HttpRequestMessageModifier CreateFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                s_Logger.LogWarning($"Unable to create a {typeof(HttpRequestMessageModifier).Name} from empty file path.");
                return null;
            }

            if (!File.Exists(filePath))
            {
                s_Logger.LogWarning($"Unable to create a {typeof(HttpRequestMessageModifier).Name} from file {filePath}. File does not exists.");
                return null;
            }

            HttpRequestMessageModifier result = null;
            try
            {
                var fileContent = File.ReadAllText(filePath);
                result = new HttpRequestMessageModifier(fileContent);
            }
            catch (Exception)
            {
                s_Logger.LogWarning($"Unable to create a {typeof(HttpRequestMessageModifier).Name} from file {filePath}. Check file content.");
            }

            return result;
        }
    }

    class HttpRequestMessageModifierRequest
    {
        readonly HttpRequestMessageModifierFilter m_Filter;
        readonly HttpRequestMessageModifierReplace m_Replace;

        public HttpRequestMessageModifierRequest(HttpRequestMessageModifierRequestSchema schema)
        {
            m_Filter = new HttpRequestMessageModifierFilter(schema.Filter);
            m_Replace = new HttpRequestMessageModifierReplace(schema.Replace);
        }

        public bool Modify(HttpRequestMessage request)
        {
            if (m_Filter.Filter(request))
            {
                m_Replace.Replace(request);

                return true;
            }

            return false;
        }
    }

    class HttpRequestMessageModifierFilter
    {
        static readonly UCLogger s_Logger = LoggerProvider.GetLogger<HttpRequestMessageModifierFilter>();

        readonly HttpRequestMessageModifierFilterSchema m_Schema;

        readonly Regex m_MethodRegex;
        readonly Regex m_IsMatchRegex;

        readonly bool m_Valid;

        public HttpRequestMessageModifierFilter(HttpRequestMessageModifierFilterSchema schema)
        {
            m_Schema = schema;
            m_Valid = true;

            try
            {
                m_MethodRegex = new Regex(schema.Method);
            }
            catch (ArgumentException)
            {
                s_Logger.LogError($"Unable to create regex expression from method {schema.Method}");
                m_Valid = false;
            }

            try
            {
                m_IsMatchRegex = new Regex(schema.IsMatch);
            }
            catch (ArgumentException)
            {
                s_Logger.LogError($"Unable to create regex expression from isMatch {schema.IsMatch}");
                m_Valid = false;
            }
        }

        public bool Filter(HttpRequestMessage request)
        {
            if (!m_Valid)
                return false;

            if (m_MethodRegex.IsMatch(request.Method.ToString()))
            {
                var uriPropertyValue = GetUriPropertyValue(request.RequestUri, m_Schema.UriProperty);
                if (uriPropertyValue != null)
                {
                    return m_IsMatchRegex.IsMatch(uriPropertyValue);
                }
            }

            return false;
        }

        string GetUriPropertyValue(Uri uri, string propertyName)
        {
            var propertyInfo = uri.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var propertyValue = propertyInfo.GetValue(uri);
                if (propertyValue != null)
                    return propertyValue.ToString();
            }

            return null;
        }
    }

    class HttpRequestMessageModifierReplace
    {
        readonly HttpRequestMessageModifierReplaceSchema m_Schema;
        public HttpRequestMessageModifierReplace(HttpRequestMessageModifierReplaceSchema schema)
        {
            m_Schema = schema;
        }

        public void Replace(HttpRequestMessage request)
        {
            var newUri = new UriBuilder(request.RequestUri);
            if (!string.IsNullOrEmpty(m_Schema.Host))
                newUri.Host = m_Schema.Host;

            if (m_Schema.Port > 0)
                newUri.Port = m_Schema.Port;

            if (!string.IsNullOrEmpty(m_Schema.Scheme))
                newUri.Scheme = m_Schema.Scheme;

            request.RequestUri = newUri.Uri;
        }
    }

    class HttpClientModifierMiddleware : IHttpClient
    {
        readonly IHttpClient m_BaseClient;
        readonly IHttpRequestMessageModifier m_RequestModifier;

        public HttpClientModifierMiddleware(IHttpClient httpClient, IHttpRequestMessageModifier requestModifier)
        {
            m_BaseClient = httpClient;
            m_RequestModifier = requestModifier;
        }

        public TimeSpan Timeout
        {
            get => m_BaseClient.Timeout;
            set => m_BaseClient.Timeout = value;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            m_RequestModifier?.Modify(request);
            return m_BaseClient.SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> DownloadFileAsync(HttpRequestMessage request, string downloadFilePath, CancellationToken cancellationToken = default)
        {
            m_RequestModifier?.Modify(request);
            return m_BaseClient.DownloadFileAsync(request, downloadFilePath, cancellationToken);
        }
    }

    static class HttpClientModifierMiddlewareExtension
    {
        static readonly UCLogger s_Logger = LoggerProvider.GetLogger(typeof(HttpClientModifierMiddlewareExtension).FullName);

        public static IHttpClient WithModifierMiddleware(this IHttpClient baseHttpClient)
        {
            var httpRequestMessageModifier = HttpRequestMessageModifierFactory.CreateFromEnvironmentVariable();
            if (httpRequestMessageModifier != null)
            {
                s_Logger.LogInfo($"Installing {typeof(HttpRequestMessageModifier).Name} middleware on {typeof(IHttpClient).Name}: {baseHttpClient}");
                return new HttpClientModifierMiddleware(baseHttpClient, httpRequestMessageModifier);
            }

            s_Logger.LogInfo($"{typeof(HttpRequestMessageModifier).Name} middleware not installed on {typeof(IHttpClient).Name}: {baseHttpClient}");
            return baseHttpClient;
        }
    }
}
#endif

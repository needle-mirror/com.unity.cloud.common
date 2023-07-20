using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    public static class UrlRedirectUtils
    {
        public static bool UrlHasAwaitedQueryArguments(Dictionary<string, string> queryArgumentDictionary, List<string> awaitedQueryArguments)
        {
            if (awaitedQueryArguments == null)
            {
                return true;
            }
            var hasAllQueryArguments = true;
            foreach (var keyName in awaitedQueryArguments)
            {
                if (!queryArgumentDictionary.ContainsKey(keyName))
                {
                    hasAllQueryArguments = false;
                    break;
                }
            }
            return hasAllQueryArguments;
        }

        public static void ValidateUrlArgument(string url, out Uri uri)
        {
            if (url == null)
                throw new ArgumentException("The url cannot be null.", nameof(url));

            if (string.IsNullOrEmpty(url?.Trim()))
                throw new ArgumentException("The url cannot be empty.", nameof(url));

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                throw new ArgumentException("The url is not a valid uri.", nameof(url));
        }

        public static bool TryInterceptRedirectionUrl(Uri uri, List<string> awaitedQueryArguments, out UrlRedirectResult urlRedirectResult)
        {
            urlRedirectResult = default;
            var queryArgs = QueryArgumentsParser.GetDictionaryFromArguments(uri);
            if (awaitedQueryArguments != null && UrlHasAwaitedQueryArguments(queryArgs, awaitedQueryArguments))
            {
                urlRedirectResult = new UrlRedirectResult
                {
                    Status = UrlRedirectStatus.Success,
                    QueryArguments = queryArgs
                };
                return true;
            }
            return false;
        }
    }
}

using System;

namespace Unity.Cloud.Common
{
    static class Utilities
    {
        internal static Uri GetUrlWithCheckpoint(Uri uri, long checkpointEpochMilliseconds)
        {
            if (checkpointEpochMilliseconds != default)
            {
                var uriBuilder = new UriBuilder(uri);
                var query = uriBuilder.Query;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    query += "&";
                }

                query = $"{query}checkpoint={checkpointEpochMilliseconds}";
                uriBuilder.Query = query;
                return uriBuilder.Uri;
            }
            return uri;
        }
    }
}

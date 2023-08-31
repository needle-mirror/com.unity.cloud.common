using System;
using System.Runtime.InteropServices;

namespace Unity.Cloud.Common.Runtime
{
    public static class CommonBrowserInterop
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern string GetURLFromPage();

        [DllImport("__Internal")]
        public static extern string GetQueryParam(string paramId);

        [DllImport("__Internal")]
        public static extern bool CopyToClipboard(string value);

        [DllImport("__Internal")]
        public static extern void CacheValue(string key, string value);

        [DllImport("__Internal")]
        public static extern void ClearCache(string key);

        [DllImport("__Internal")]
        public static extern void SaveAuthorizationCookie(string token);

        [DllImport("__Internal")]
        public static extern void Navigate(string url, string windowId = "_self");

        [DllImport("__Internal")]
        public static extern string RetrieveCachedValue(string key);
#else
        public static string GetURLFromPage() => throw new PlatformNotSupportedException();

        public static string GetQueryParam(string paramId) => throw new PlatformNotSupportedException();

        public static bool CopyToClipboard(string value) => throw new PlatformNotSupportedException();

        public static void CacheValue(string key, string value) => throw new PlatformNotSupportedException();

        public static void ClearCache(string key) => throw new PlatformNotSupportedException();

        public static void SaveAuthorizationCookie(string token) => throw new PlatformNotSupportedException();

        public static void Navigate(string url, string windowId = "_self") => throw new PlatformNotSupportedException();

        public static string RetrieveCachedValue(string key) => throw new PlatformNotSupportedException();
#endif
    }
}

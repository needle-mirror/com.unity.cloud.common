using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A static class that holds the uri scheme prefix used to secure app binding on a device.
    /// </summary>
    public static class UriSchemeRedirection
    {
        /// <summary>
        /// The string value of the uri scheme prefix used to secure app binding on a device.
        /// </summary>
        public readonly static string s_UriSchemePrefix = "com.unity.dt.";
    }
}

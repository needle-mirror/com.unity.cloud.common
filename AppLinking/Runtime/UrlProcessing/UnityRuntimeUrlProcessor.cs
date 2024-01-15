using System;
using UnityEngine;

namespace Unity.Cloud.AppLinking.Runtime
{
    /// <inheritdoc/>
    public class UnityRuntimeUrlProcessor : IUrlProcessor
    {
        /// <inheritdoc/>
        public string HostUrl
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return Application.absoluteURL;
#else
                return null;
#endif
            }
        }

        /// <inheritdoc/>
        public void ProcessURL(string url)
        {
            Application.OpenURL(url);
        }

    }
}

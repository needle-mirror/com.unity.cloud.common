using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    /// <inheritdoc/>
    public class UrlProcessor : IUrlProcessor
    {
        /// <inheritdoc/>
        public void ProcessURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}

using System.Diagnostics;

namespace Unity.Cloud.Common
{
    /// <inheritdoc/>
    public class CoreUrlProcessor : IUrlProcessor
    {
        /// <inheritdoc/>
        public void ProcessURL(string url)
        {
            Process.Start(url);
        }
    }
}

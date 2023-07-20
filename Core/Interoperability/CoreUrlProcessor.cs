using System.Diagnostics;

namespace Unity.Cloud.Common
{
    public class CoreUrlProcessor : IUrlProcessor
    {
        public void ProcessURL(string url)
        {
            Process.Start(url);
        }
    }
}

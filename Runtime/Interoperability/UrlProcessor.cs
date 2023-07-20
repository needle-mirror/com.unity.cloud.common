using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{
    public class UrlProcessor : IUrlProcessor
    {
        public void ProcessURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}

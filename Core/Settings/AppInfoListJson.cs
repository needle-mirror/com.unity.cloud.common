using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    [Serializable]
    internal class AppInfoListJson
    {
        public List<AppInfo> Applications { get; set; }
    }
}

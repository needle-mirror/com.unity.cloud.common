using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A class containing identifying information about an App.
    /// </summary>
    [Serializable]
    public class AppInfo
    {
        /// <summary>
        /// The App's ID.
        /// </summary>
        [JsonConverter(typeof(AppIdConverter))]
        public AppId Id { get; set; }

        /// <summary>
        /// The App's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The App's display name.
        /// </summary>
        public string DisplayName { get; set; }
    }
}

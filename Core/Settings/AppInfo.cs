using System;

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
        public string Id { get; set; }

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

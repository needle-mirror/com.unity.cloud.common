using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This class contains information about a single permission verb for a resource.
    /// </summary>
    [Serializable]
    public class Permission
    {
        // Temp set to mock UX

        /// <summary>
        /// The name of the permission.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes and returns an instance of <see cref="Permission"/>.
        /// </summary>
        public Permission(string name)
        {
            Name = name;
        }
    }
}

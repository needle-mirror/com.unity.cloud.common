using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Enum of all supported service environments.
    /// </summary>
    public enum ServiceEnvironment
    {
        /// <summary>
        /// Test service environment.
        /// </summary>
        Test,

        /// <summary>
        /// A staging service environment.
        /// </summary>
        Staging,

        /// <summary>
        /// Production service environment.
        /// </summary>
        Production
    }
}

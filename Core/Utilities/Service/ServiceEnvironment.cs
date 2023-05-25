namespace Unity.Cloud.Common
{
    /// <summary>
    /// Enum of all supported service environments.
    /// </summary>
    public enum ServiceEnvironment
    {
        /// <summary>
        /// Url service environment.
        /// </summary>
        /// <remarks>
        /// Used when UNITY_CLOUD_SERVICES_ENV is set to a URL instead of a specific environment
        /// </remarks>
        Url,

        /// <summary>
        /// Local service environment.
        /// </summary>
        Local,

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

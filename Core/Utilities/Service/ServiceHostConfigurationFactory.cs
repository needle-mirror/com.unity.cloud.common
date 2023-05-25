using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// A factory class used to create a <see cref="ServiceHostConfiguration"/>.
    /// </summary>
    public static class ServiceHostConfigurationFactory
    {
        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with default values.
        /// Any system-level overrides set via environment variables will take priority.
        /// </summary>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration Create()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            return new ServiceHostConfiguration();
#else
            throw new InvalidOperationException("Consider using UnityRuntimeServiceHostConfigurationFactory from the Unity.Cloud.Common.Runtime assembly");
#endif
        }

        /// <summary>
        /// Create a <see cref="ServiceHostConfiguration"/> with an optional application-level override for service host options.
        /// Any system-level overrides set via environment variables will take priority.
        /// </summary>
        /// <param name="applicationOverride">An application-level override value for for service host options.</param>
        /// <returns>The created configuration.</returns>
        public static ServiceHostConfiguration CreateWithOverride(ServiceHost applicationOverride)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            return new ServiceHostConfiguration(applicationOverride);
#else
            throw new InvalidOperationException("Consider using UnityRuntimeServiceHostConfigurationFactory from the Unity.Cloud.Common.Runtime assembly");
#endif
        }
    }
}

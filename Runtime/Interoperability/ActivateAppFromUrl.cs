using System;
using UnityEngine;

namespace Unity.Cloud.Common.Runtime
{

    /// <summary>
    /// This class exposes an Activation Url field in the Unity Editor inspector. Use it to inject an activation url in PlayMode.
    /// </summary>
    /// <remarks>Use this class to simulate the interception of any url or deep link when the application is executed in PlayMode.</remarks>
    public class ActivateAppFromUrl : MonoBehaviour
    {
        public string ActivationUrl => m_ActivationUrl;
        public bool ActivateAtStartUp => m_ActivateAtStartUp;

        /// <summary>
        /// The activation url to inject.
        /// </summary>
        [SerializeField]
        internal string m_ActivationUrl;

        /// <summary>
        /// A boolean indicating if the ActivationUrl is to be injected at start up time or if it will be manually injected by clicking the Activate button at a later time.
        /// </summary>
        [SerializeField]
        internal bool m_ActivateAtStartUp;
    }
}

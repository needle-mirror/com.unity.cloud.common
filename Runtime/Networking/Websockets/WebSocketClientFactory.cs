namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// Create WebSocket Client according to the current platform
    /// </summary>
    public static class WebSocketClientFactory
    {
        /// <summary>
        /// Initializes and returns an instance of <see cref="IWebSocketClient"/> for the current platform.
        /// </summary>
        /// <returns>An instance of <see cref="IWebSocketClient"/> appropriate for the current platform.</returns>
        public static IWebSocketClient Create()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            return new NativeWebSocketClient();
#elif UNITY_WEBGL
            return new WebglWebSocketClient();
#else
            throw new NotImplementedException("No WebSocket Client implemented for the current platform");
#endif
        }
    }
}

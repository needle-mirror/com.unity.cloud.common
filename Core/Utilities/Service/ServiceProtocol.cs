using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Enum of all supported protocols to connect to a Server.
    /// </summary>
    public enum ServiceProtocol
    {
        /// <summary>
        /// HTTP protocol.
        /// </summary>
        Http,

        /// <summary>
        /// Websocket protocol.
        /// </summary>
        WebSocket,
        /// <summary>
        /// Websocket secure protocol.
        /// </summary>
        WebSocketSecure
    }
}

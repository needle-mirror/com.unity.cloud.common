using System;
using System.Threading.Tasks;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// Interface for an adapter used to expose javascript methods from the WebSocketAdapter.jslib plugin
    /// Primarily used for mocking the adapter for testing purposes
    /// </summary>
    interface IWebglWebSocketAdapter : IDisposable
    {
        Task Disconnect();
        Task Connect(Uri uri);
        void Send(string msg);
        void Send(ArraySegment<byte> data);
        event Action<Exception> ConnectionErrorOccured;
        event Action<string> MessageReceived;
        event Action<ArraySegment<byte>> DataReceived;
        void UpdateCheckpointTimestamp(long checkpointEpochMilliseconds);
    }
}

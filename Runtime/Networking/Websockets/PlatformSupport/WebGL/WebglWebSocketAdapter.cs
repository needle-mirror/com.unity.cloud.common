#if UNITY_WEBGL
// Javascript functions are static and shared between clients, therefore a Factory pattern
// is used to dispatch calls to the right instances.
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// Exposes javascript methods in the WebSocketAdapter.jslib plugin used for websocket connection/communication
    /// </summary>
    class WebglWebSocketAdapter : IWebglWebSocketAdapter
    {
        // Callbacks triggered via javascript
        delegate void OnOpenCallback(int instanceId);
        delegate void OnCloseCallback(int instanceId);
        delegate void OnMessageCallback(int instanceId, IntPtr msgPtr, int msgSize);
        delegate void OnDataCallback(int instanceId, IntPtr msgPtr, int msgSize);
        delegate void OnErrorCallback(int instanceId, int errorType);
        delegate void OnRetryCallback(int instanceId);

        // Calls to Javascript

        // Functions to save callbacks for later invocation
        [DllImport("__Internal")] static extern void JsWs_SetOnOpenCallback(OnOpenCallback callback);
        [DllImport("__Internal")] static extern void JsWs_SetOnCloseCallback(OnCloseCallback callback);
        [DllImport("__Internal")] static extern void JsWs_SetOnMessageCallback(OnMessageCallback callback);
        [DllImport("__Internal")] static extern void JsWs_SetOnDataCallback(OnDataCallback callback);
        [DllImport("__Internal")] static extern void JsWs_SetOnErrorCallback(OnErrorCallback callback);

        // Operations on WebSockets
        [DllImport("__Internal")] static extern void JsWs_Connect(int id, string url);
        [DllImport("__Internal")] static extern void JsWs_Close(int id);
        [DllImport("__Internal")] static extern void JsWs_Send_Message(int id, string msg);
        [DllImport("__Internal")] static extern void JsWs_Send_Data(int id, byte[] data, int offset, int count);

        /// <summary>
        /// Local class used to manage multiple connections using the WebSocketAdapter.jslib plugin
        /// </summary>
        class Connection
        {
            public Connection(Uri uri = null)
            {
                Uri = uri;
            }

            public Uri Uri { get; private set; }
            public long CheckpointEpochMilliseconds { get; set; }

            public TaskCompletionSource<bool> ConnectionOpened;
            public TaskCompletionSource<bool> ConnectionClosed;

            public event Action<Exception> OnError;
            public event Action<string> OnMessage;
            public event Action<ArraySegment<byte>> OnData;

            public void SetConnection(Uri uri)
            {
                Uri = uri;
            }

            public void InvokeErrorEvent(Exception err)
            {
                OnError?.Invoke(err);
            }

            public void InvokeMessageEvent(string message)
            {
                OnMessage?.Invoke(message);
            }

            public void InvokeDataEvent(ArraySegment<byte> data)
            {
                OnData?.Invoke(data);
            }
        }
        public event Action<Exception> ConnectionErrorOccured;
        public event Action<string> MessageReceived;
        public event Action<ArraySegment<byte>> DataReceived;

        static readonly Dictionary<int, Connection> s_Connections = new Dictionary<int, Connection>();
        static int s_LastId = 0;
        static bool s_Initialized = false;

        int m_Id;

        public WebglWebSocketAdapter()
        {
            m_Id = Allocate();
            s_Connections[m_Id].OnError += InvokeError;
            s_Connections[m_Id].OnMessage += InvokeMessage;
            s_Connections[m_Id].OnData += InvokeData;
        }

        /// <summary>
        /// Ensure disposal of any IDisposable references.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ensure internal disposal of any IDisposable references.
        /// </summary>
        /// <param name="disposing">Dispose pattern boolean value received from public Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                s_Connections[m_Id].OnError -= InvokeError;
                s_Connections[m_Id].OnMessage -= InvokeMessage;
                s_Connections[m_Id].OnData -= InvokeData;
                Deallocate(m_Id);
            }
        }

        static int Allocate()
        {
            if (!s_Initialized)
            {
                // Delegated to BrowserWebSocketAdapter
                JsWs_SetOnOpenCallback(DelegateOnOpenEvent);
                JsWs_SetOnCloseCallback(DelegateOnCloseEvent);
                JsWs_SetOnMessageCallback(DelegateOnMessageEvent);
                JsWs_SetOnDataCallback(DelegateOnDataEvent);
                JsWs_SetOnErrorCallback(DelegateOnErrorEvent);

                s_Initialized = true;
            }

            var connection = new Connection();
            var id = s_LastId++;
            s_Connections.Add(id, connection);

            return id;
        }

        static void Deallocate(int id)
        {
            if (!s_Connections.ContainsKey(id))
            {
                throw new InvalidOperationException("Release: WebSocket not allocated");
            }

            s_Connections.Remove(id);
        }

        public async Task Disconnect()
        {
            if (!s_Connections.ContainsKey(m_Id))
            {
                throw new InvalidOperationException("Release: WebSocket not allocated");
            }

            s_Connections[m_Id].ConnectionClosed = new TaskCompletionSource<bool>();
            JsWs_Close(m_Id);
            await s_Connections[m_Id].ConnectionClosed.Task;
            s_Connections[m_Id].ConnectionClosed = null;
        }

        public async Task Connect(Uri uri)
        {
            if (!s_Connections.ContainsKey(m_Id))
            {
                throw new InvalidOperationException("Connect: WebSocket not allocated");
            }

            s_Connections[m_Id].SetConnection(uri);
            s_Connections[m_Id].ConnectionOpened = new TaskCompletionSource<bool>();
            s_Connections[m_Id].ConnectionClosed = new TaskCompletionSource<bool>();

            JsWs_Connect(m_Id, uri.AbsoluteUri);

            await Task.WhenAny(new Task[]
            {
                s_Connections[m_Id].ConnectionOpened.Task,
                s_Connections[m_Id].ConnectionClosed.Task
            });

            var connected = s_Connections[m_Id].ConnectionOpened.Task.IsCompletedSuccessfully;

            s_Connections[m_Id].ConnectionOpened = null;
            s_Connections[m_Id].ConnectionClosed = null;

            if (!connected)
                throw new WebSocketException ($"Connection could not be established to {uri}.");
        }

        public void Send(string msg)
        {
            if (!s_Connections.ContainsKey(m_Id))
            {
                throw new InvalidOperationException("Send: WebSocket not allocated");
            }

            JsWs_Send_Message(m_Id, msg);
        }

        public void Send(ArraySegment<byte> data)
        {
            if (!s_Connections.ContainsKey(m_Id))
            {
                throw new InvalidOperationException("Send: WebSocket not allocated");
            }

            JsWs_Send_Data(m_Id, data.Array, data.Offset, data.Count);
        }


        public void UpdateCheckpointTimestamp(long checkpointEpochMilliseconds)
        {
            if (!s_Connections.ContainsKey(m_Id))
            {
                throw new InvalidOperationException("UpdateCheckpointTimestamp: WebSocket not allocated");
            }

            s_Connections[m_Id].CheckpointEpochMilliseconds = checkpointEpochMilliseconds;
        }

        // Functions to relay messages from Javascript to C# receiver:

        // Complete awaited Connect call
        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        static void DelegateOnOpenEvent(int id)
        {
            if (s_Connections.TryGetValue(id, out var connection))
            {
                connection.ConnectionOpened?.SetResult(true);
            }
        }

        // Complete awaited Disconnect call
        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        static void DelegateOnCloseEvent(int id)
        {
            if (s_Connections.TryGetValue(id, out var connection))
            {
                if (connection.ConnectionClosed == null)
                    connection.InvokeErrorEvent(new ApplicationException("Disconnected from server"));
                else
                    connection.ConnectionClosed?.SetResult(true);
            }
        }

        // Dispatch MessageReceived events
        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        static void DelegateOnMessageEvent(int id, IntPtr msgPtr, int msgSize)
        {
            if (s_Connections.TryGetValue(id, out var connection))
            {
                byte[] msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                var message = System.Text.Encoding.UTF8.GetString(msg, 0, msgSize);
                connection.InvokeMessageEvent(message);
            }
        }

        // Dispatch MessageReceived events
        [MonoPInvokeCallback(typeof(OnDataCallback))]
        static void DelegateOnDataEvent(int id, IntPtr msgPtr, int msgSize)
        {
            if (s_Connections.TryGetValue(id, out var connection))
            {
                var buffer = new byte[msgSize];
                Marshal.Copy(msgPtr, buffer, 0, msgSize);
                connection.InvokeDataEvent(new ArraySegment<byte>(buffer, 0, msgSize));
            }
        }

        // Dispatch ConnectionErrorOccured events
        // errorType 0 - EventSource.onError callback
        // errorType 1 - connection error
        // errorType 2 - send error
        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        static void DelegateOnErrorEvent(int id, int errorType)
        {
            if (s_Connections.TryGetValue(id, out var connection))
            {
                if (connection.ConnectionOpened != null)
                    connection.ConnectionOpened.SetException(new ApplicationException("Failed to Connect"));
                else if (connection.ConnectionClosed != null)
                    connection.ConnectionClosed.SetException(new ApplicationException("Failed to Disconnect"));
                else if (errorType == 0)
                    connection.InvokeErrorEvent(new ApplicationException("Unexpected WebSocket Exception (EventSource.onError)"));
                else if (errorType == 1)
                    connection.InvokeErrorEvent(new ApplicationException("Connection Error"));
                else if (errorType == 2)
                    connection.InvokeErrorEvent(new ApplicationException("Send Error"));
            }
        }

        // Dispatch OnClose events
        [MonoPInvokeCallback(typeof(OnRetryCallback))]
        static void HandleOnRetryEvent(int id)
        {
            if (!s_Connections.ContainsKey(id))
            {
                throw new InvalidOperationException("Connect: WebSocket not allocated");
            }

            var connection = s_Connections[id];
            var uri = Utilities.GetUrlWithCheckpoint(connection.Uri, connection.CheckpointEpochMilliseconds);
            JsWs_Connect(id, uri.AbsoluteUri);
        }

        void InvokeError(Exception err)
        {
            ConnectionErrorOccured?.Invoke(err);
        }

        void InvokeMessage(string message)
        {
            MessageReceived?.Invoke(message);
        }

        void InvokeData(ArraySegment<byte> data)
        {
            DataReceived?.Invoke(data);
        }
    }
}
#endif

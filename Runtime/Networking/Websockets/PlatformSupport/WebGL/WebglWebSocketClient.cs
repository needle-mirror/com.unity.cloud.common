#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Unity.Cloud.Common.Runtime
{
    /// <summary>
    /// IWebSocketClient implementation for WebGL builds
    /// </summary>
    class WebglWebSocketClient : IWebSocketClient
    {
        public event Action<Exception> ConnectionErrorOccured;
        public event Action<string> MessageReceived;
        public event Action<ArraySegment<byte>> DataReceived;
        public event Action<ConnectionState> ConnectionStateChanged;

        IWebglWebSocketAdapter m_Adapter;

        static readonly UCLogger s_Logger = LoggerProvider.GetLogger<WebglWebSocketClient>();

        ConnectionState m_State;

        public ConnectionState State
        {
            get => m_State;
            private set
            {
                m_State = value;
                ConnectionStateChanged?.Invoke(m_State);
            }
        }

        public WebglWebSocketClient() : this(new WebglWebSocketAdapter()) {}

        internal WebglWebSocketClient(IWebglWebSocketAdapter adapter)
        {
            m_Adapter = adapter;

            m_Adapter.ConnectionErrorOccured += OnErrorReceived;
            m_Adapter.MessageReceived += OnMessageReceived;
            m_Adapter.DataReceived += OnDataReceived;

            State = ConnectionState.Disconnected;
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
                m_Adapter.ConnectionErrorOccured -= OnErrorReceived;
                m_Adapter.MessageReceived -= OnMessageReceived;
                m_Adapter.DataReceived -= OnDataReceived;
                m_Adapter.Dispose();
            }
        }

        public async Task ConnectAsync(Uri uri, HttpHeaders headers = null)
        {
            if (State == ConnectionState.Connected)
                throw new InvalidOperationException("Already connected");
            if (State == ConnectionState.Connecting)
                throw new InvalidOperationException("Already attempting connection");
            if (State == ConnectionState.Disconnecting)
                throw new InvalidOperationException("Currently attempting disconnection");

            try
            {
                State = ConnectionState.Connecting;

                s_Logger.LogDebug($"Connecting to URL => {uri}");
                await m_Adapter.Connect(uri);
                s_Logger.LogDebug("Opened connection to server.");
            }
            catch (Exception)
            {
                State = ConnectionState.Disconnected;
                throw;
            }

            State = ConnectionState.Connected;
        }

        public async Task DisconnectAsync() // TODO prevent reentrance
        {
            if (State == ConnectionState.Disconnected)
                throw new InvalidOperationException("Already disconnected");
            if (State == ConnectionState.Disconnecting)
                throw new InvalidOperationException("Already attempting disconnection");
            if (State == ConnectionState.Connecting)
                throw new InvalidOperationException("Currently attempting connection");

            try
            {
                State = ConnectionState.Disconnecting;
                await m_Adapter.Disconnect();
            }
            catch (Exception)
            {
                State = ConnectionState.Disconnected;
                throw;
            }

            State = ConnectionState.Disconnected;
            s_Logger.LogDebug("Closed connection to server.");
        }

        public async Task SendAsync(string msg)
        {
            if (State != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Connection closed");
            }

            try
            {
                m_Adapter.Send(msg);
                await Task.CompletedTask;  // No threads in WebGL
            }
            catch (Exception)
            {
                State = ConnectionState.Disconnected;
                throw;
            }
        }
        public async Task SendAsync(ArraySegment<byte> data)
        {
            if (State != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Connection closed");
            }

            try
            {
                m_Adapter.Send(data);
                await Task.CompletedTask;  // No threads in WebGL
            }
            catch (Exception)
            {
                State = ConnectionState.Disconnected;
                throw;
            }
        }

        public void UpdateCheckpointTimestamp(long checkpointEpochMilliseconds)
        {
            m_Adapter.UpdateCheckpointTimestamp(checkpointEpochMilliseconds);
        }

        // Not used in implementation, but can be used to test platform parity:

        [DllImport("__Internal")]
        static extern int JsWs_GetState(int id);

        // Javascript websocket state enum
        enum WebSocketReadyState
        {
            Connecting,
            Open,
            Closing,
            Closed
        };

        void OnDataReceived(ArraySegment<byte> data)
        {
            s_Logger.LogDebug("Data received");
            DataReceived?.Invoke(data);
        }

        void OnMessageReceived(string jsonMessages)
        {
            s_Logger.LogDebug("Messages received");
            MessageReceived?.Invoke(jsonMessages);
        }

        void OnErrorReceived(Exception err)
        {
            s_Logger.LogError("Connection Error.");
            ConnectionErrorOccured?.Invoke(err);
        }
    }
}
#endif

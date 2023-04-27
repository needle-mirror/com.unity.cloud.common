using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Cloud.Common.Runtime
{
    interface IMainThreadIODispatcher
    {
        Task<T> RunAsync<T>(Action<Action<T>> action) where T : class;
    }

    class LegacyRequestHandler
    {
        const string k_EmptyStringContent = "";
        const string k_HttpVerbPatch = "PATCH";
        const string k_TimeoutErrorMessage = "Request timeout";

        readonly IMainThreadIODispatcher m_Dispatcher;
        readonly TaskScheduler m_Scheduler;

        public TimeSpan Timeout { get; set; }

        public LegacyRequestHandler(IMainThreadIODispatcher dispatcher = null)
        {
            // UnityWebRequest must run on the main unity thread
            m_Scheduler = dispatcher != null ? TaskScheduler.Default : UnitySynchronizationContextGrabber.s_UnityMainThreadScheduler;
            m_Dispatcher = dispatcher;
        }

        /// <summary>
        /// Send an asynchronous HTTP request or file download request.
        /// </summary>
        /// <param name="httpRequestMessage">The request message.</param>
        /// <param name="downloadFilePath">Optional path to save downloaded files.</param>
        /// <param name="cancellationToken">Optional cancellation token that will try to cancel the operation.</param>
        /// <returns>A task that will hold the HttpResponseMessage once the request is completed.</returns>
        /// <exception cref="HttpRequestException">Thrown when an HTTP response can't be obtained from the server.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the request is cancelled by a cancellation token.</exception>
        /// <exception cref="TimeoutException">Thrown when the request failed due to timeout.</exception>
        public async Task<HttpResponseMessage> RequestAsync(HttpRequestMessage httpRequestMessage, string downloadFilePath, CancellationToken cancellationToken)
        {
            var factoryTask = await Task.Factory.StartNew(
                async () => await RequestInternalAsync(httpRequestMessage, downloadFilePath, cancellationToken),
                cancellationToken,
                TaskCreationOptions.DenyChildAttach,
                m_Scheduler);

            var response = await factoryTask;

            return response;
        }

        async Task<HttpResponseMessage> RequestInternalAsync(HttpRequestMessage httpRequestMessage, string downloadFilePath = null, CancellationToken cancellationToken = default)
        {
            string stringContent = null;
            byte[] bytesContent = null;
            switch (httpRequestMessage.Content)
            {
                case StringContent _:
                    stringContent = await httpRequestMessage.Content.ReadAsStringAsync();
                    break;
                case ByteArrayContent _:
                case StreamContent _:
                case ReadOnlyMemoryContent _:
                    bytesContent = await httpRequestMessage.Content.ReadAsByteArrayAsync();
                    break;
            }

            var state = new RequestState(httpRequestMessage, stringContent, bytesContent, downloadFilePath, null, null, Timeout);

            if (m_Dispatcher != null)
            {
                await m_Dispatcher.RunAsync<object>(onCompleted => PrepareAndStartRequest(state, onCompleted, cancellationToken)).ConfigureAwait(false);
                await m_Dispatcher.RunAsync<object>(onCompleted => CompleteRequest(state, onCompleted)).ConfigureAwait(false);
            }
            else
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                PrepareAndStartRequest(state, res => tcs.TrySetResult(res), cancellationToken);
                await tcs.Task;

                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException(tcs.Task);

                CompleteRequest(state, res => { });
            }

            return state.Response;
        }

        static UnityWebRequest CreateDeleteRequest(Uri requestUri, string stringContent = null, byte[] bytesContent = null)
        {
            var request = UnityWebRequest.Delete(requestUri);

            byte[] requestContent = bytesContent;
            if (stringContent != null)
            {
                requestContent = Encoding.UTF8.GetBytes(stringContent);
            }
            if (requestContent != null)
            {
                request.uploadHandler = new UploadHandlerRaw(requestContent);
            }
            return request;
        }

        static void PrepareAndStartRequest(RequestState state, Action<object> onCompleted, CancellationToken cancellationToken)
        {
            var httpRequestMessage = state.HttpRequestMessage;
            var stringContent = state.StringContent;
            var bytesContent = state.BytesContent;
            var downloadFilePath = state.DownloadFilePath;

            var methodString = httpRequestMessage.Method.ToString();
            var request = methodString switch
            {
                UnityWebRequest.kHttpVerbGET => UnityWebRequest.Get(httpRequestMessage.RequestUri),
                // NOTE: For POST and PATCH, create a PUT request, then override the verb, see https://manuelotheo.com/uploading-raw-json-data-through-unitywebrequest/
                UnityWebRequest.kHttpVerbPUT or UnityWebRequest.kHttpVerbPOST or k_HttpVerbPatch when bytesContent != null => UnityWebRequest.Put(httpRequestMessage.RequestUri, bytesContent),
                UnityWebRequest.kHttpVerbPUT or UnityWebRequest.kHttpVerbPOST or k_HttpVerbPatch => UnityWebRequest.Put(httpRequestMessage.RequestUri, stringContent ?? k_EmptyStringContent),
                UnityWebRequest.kHttpVerbDELETE when bytesContent != null => CreateDeleteRequest(httpRequestMessage.RequestUri, bytesContent: state.BytesContent),
                UnityWebRequest.kHttpVerbDELETE => CreateDeleteRequest(httpRequestMessage.RequestUri, stringContent: state.StringContent),
                _ => throw new NotImplementedException()
            };

            state.Request = request;

            if (methodString == UnityWebRequest.kHttpVerbPOST || methodString == k_HttpVerbPatch)
            {
                // Override the put if necessary
                request.method = methodString;
            }

            foreach (var header in httpRequestMessage.Headers)
            {
                var value = string.Join(",", header.Value);
                request.SetRequestHeader(header.Key, value);
            }

            if (httpRequestMessage.Content != null)
            {
                foreach (var header in httpRequestMessage.Content.Headers)
                {
                    var value = string.Join(",", header.Value);
                    request.SetRequestHeader(header.Key, value);
                }
            }

            var isDownload = !string.IsNullOrEmpty(downloadFilePath);

            if (isDownload)
                request.downloadHandler = new DownloadHandlerFile(downloadFilePath);
            else if (httpRequestMessage.Method == HttpMethod.Delete)
            {
                //To ensure content reception for Delete-requests
                request.downloadHandler = new DownloadHandlerBuffer();
            }

            state.CancellationTokenRegistration = cancellationToken.Register(() =>
            {
                request.Abort();
                request.Dispose();
            });

            if (state.Timeout != default)
            {
                request.timeout = state.Timeout.Seconds;
            }

            var asyncOp = request.SendWebRequest();
            asyncOp.completed += obj => { onCompleted(null); };
        }

        static void CompleteRequest(RequestState state, Action<object> onCompleted)
        {
            state.CancellationTokenRegistration?.Dispose();

            var request = state.Request;
            var httpRequestMessage = state.HttpRequestMessage;
            var isDownload = !string.IsNullOrEmpty(state.DownloadFilePath);

            var errorMessage = request.error;
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                request.Dispose();

                if (errorMessage == k_TimeoutErrorMessage)
                {
                    throw new TimeoutException(errorMessage);
                }

                throw new HttpRequestException(errorMessage);
            }

            var response = new HttpResponseMessage();
            response.RequestMessage = httpRequestMessage;

            if (!isDownload)
            {
                // Parse response message
                if (request.GetResponseHeader("Content-Type") == "application/octet-stream")
                    response.Content = new ByteArrayContent(request.downloadHandler?.data ?? new byte[0]);
                else
                    response.Content = new StringContent(GetResponseTextContent(request));
            }

            response.StatusCode = (HttpStatusCode) request.responseCode;

            state.Response = response;
            onCompleted(null);
            request.Dispose();
        }

        static string GetResponseTextContent(UnityWebRequest request)
        {
            try
            {
                return request.downloadHandler?.text ?? string.Empty;
            }
            catch (NotSupportedException)
            {
                // Some download handlers don't have string accessors
                return string.Empty;
            }
        }

        class RequestState
        {
            public HttpRequestMessage HttpRequestMessage;
            public string StringContent;
            public byte[] BytesContent;
            public string DownloadFilePath;
            public UnityWebRequest Request;
            public HttpResponseMessage Response;
            public IDisposable CancellationTokenRegistration;
            public TimeSpan Timeout;

            public RequestState(HttpRequestMessage httpRequestMessage, string stringContent, byte[] bytesContent, string downloadFilePath, UnityWebRequest request, HttpResponseMessage response, TimeSpan timeout)
            {
                HttpRequestMessage = httpRequestMessage;
                StringContent = stringContent;
                BytesContent = bytesContent;
                DownloadFilePath = downloadFilePath;
                Request = request;
                Response = response;
                Timeout = timeout;
            }
        }
    }
}

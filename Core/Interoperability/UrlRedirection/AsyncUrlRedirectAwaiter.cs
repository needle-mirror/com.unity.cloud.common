using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    public class AsyncUrlRedirectAwaiter : IUrlRedirectAwaiter
    {
        const int k_DefaultRefreshDelay = 500;
        const int k_DefaultTimeoutDelay = 600000; // 10 minutes

        readonly ITimeAwaiter m_Awaiter = new TimeAwaiter();

        int m_RefreshDelay;
        int m_TimeoutDelay;
        int m_CurrentWait;
        UrlRedirectResult? m_RedirectResult;

        public int RefreshDelay
        {
            get => m_RefreshDelay;
            set => m_RefreshDelay = Math.Max(value, 0);
        }

        public int TimeoutDelay
        {
            get => m_TimeoutDelay;
            set => m_TimeoutDelay = Math.Max(value, 0);
        }

        public bool HasTimedOut => m_CurrentWait >= m_TimeoutDelay;
        public bool HasResult => m_RedirectResult.HasValue;
        public UrlRedirectResult? RedirectResult => m_RedirectResult;

        public AsyncUrlRedirectAwaiter(int refreshDelay = k_DefaultRefreshDelay, int timeoutDelay = k_DefaultTimeoutDelay)
        {
            RefreshDelay = refreshDelay;
            TimeoutDelay = timeoutDelay;
        }

        public AsyncUrlRedirectAwaiter(ITimeAwaiter awaiter, int refreshDelay = k_DefaultRefreshDelay, int timeoutDelay = k_DefaultTimeoutDelay)
            : this(refreshDelay, timeoutDelay)
        {
            m_Awaiter = awaiter;
        }

        public void BeginWait()
        {
            m_CurrentWait = 0;
            m_RedirectResult = null;
        }

        public void SetResult(UrlRedirectResult? result)
        {
            m_RedirectResult = result;
        }

        public async Task WaitForRefreshAsync(CancellationToken cancellationToken = default)
        {
            await m_Awaiter.AwaitTimeAsync(RefreshDelay, cancellationToken);
            m_CurrentWait += RefreshDelay;
        }
    }
}

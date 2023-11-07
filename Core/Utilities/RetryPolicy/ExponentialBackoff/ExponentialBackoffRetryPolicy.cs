using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An implementation of <see cref="IRetryPolicy"/> that employs an "Exponential Backoff With Jitter" strategy.
    /// </summary>
    public class ExponentialBackoffRetryPolicy : IRetryPolicy
    {
        readonly IEnumerable<TimeSpan> m_TimeSeries;
        readonly ITimeAwaiter m_TimeAwaiter;

        internal ExponentialBackoffRetryPolicy(IEnumerable<TimeSpan> timeSeries, ITimeAwaiter timeAwaiter)
        {
            m_TimeSeries = timeSeries;
            m_TimeAwaiter = timeAwaiter;
        }

        internal ExponentialBackoffRetryPolicy(ITimeAwaiter timeAwaiter) : this(TimeSeriesBuilder.Default(), timeAwaiter)
        { }

        /// <summary>
        /// Creates a default <see cref="ExponentialBackoffRetryPolicy"/>.
        /// </summary>
        public ExponentialBackoffRetryPolicy() : this(TimeSeriesBuilder.Default(), new TimeAwaiter())
        { }

        /// <summary>
        /// Creates a &lt;see cref="ExponentialBackoffRetryPolicy"/&gt; with customized behavior.
        /// </summary>
        /// <param name="initialWaitTime">The initial wait time.</param>
        /// <param name="maxWaitTime">The maximum wait time per backoff.</param>
        /// <param name="maxTotalWaitTime">The maximum total wait time.</param>
        /// <param name="maxJitter">The maximum jitter.</param>
        public ExponentialBackoffRetryPolicy(TimeSpan initialWaitTime, TimeSpan maxWaitTime, TimeSpan maxTotalWaitTime, TimeSpan maxJitter = default(TimeSpan))
            : this(TimeSeriesBuilder.ExponentialBackoffWithJitter(initialWaitTime, maxWaitTime, maxTotalWaitTime, maxJitter)(), new TimeAwaiter())
        { }

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(IRetryPolicy.RetriedOperation<T> retriedOperation, IRetryPolicy.ShouldRetryChecker<T> shouldRetryChecker,
            CancellationToken cancellationToken = default, IProgress<RetryQueuedProgress> progress = default)
        {
            var retryCount = 0;

            using (var retryDelayEnumerator = m_TimeSeries.GetEnumerator())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var (actionTask, shouldRetryResult) = await RetryPolicyHelpers.RunRetryOperation(retriedOperation, shouldRetryChecker, cancellationToken).UnityConfigureAwait(false);

                    if (shouldRetryResult && retryDelayEnumerator.MoveNext())
                    {
                        // Let's trigger a retry.
                        var delay = retryDelayEnumerator.Current;
                        retryCount++;

                        progress?.Report(new RetryQueuedProgress(retryCount, delay));

                        if (delay > TimeSpan.Zero)
                            await m_TimeAwaiter.AwaitTimeAsync(delay, cancellationToken).UnityConfigureAwait(false);
                    }
                    else
                    {
                        return RetryPolicyHelpers.GetRetryResult(actionTask, shouldRetryResult);
                    }
                }
            }

            throw new OperationCanceledException();
        }
    }
}

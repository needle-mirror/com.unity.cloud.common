using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An implementation of <see cref="IRetryPolicy"/> that tries only once.
    /// </summary>
    public class NoRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(IRetryPolicy.RetriedOperation<T> retriedOperation, IRetryPolicy.ShouldRetryChecker<T> shouldRetryChecker,
            CancellationToken cancellationToken = default, IProgress<RetryQueuedProgress> progress = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (actionTask, shouldRetryResult) = await RetryPolicyHelpers.RunRetryOperation(retriedOperation, shouldRetryChecker, cancellationToken).UnityConfigureAwait(false);

            return RetryPolicyHelpers.GetRetryResult(actionTask, shouldRetryResult);
        }
    }
}

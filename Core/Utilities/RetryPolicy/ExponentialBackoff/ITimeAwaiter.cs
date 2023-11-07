using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface for awaiting a specific amount of time.
    /// </summary>
    public interface ITimeAwaiter
    {
        /// <summary>
        /// Awaits a specified amount of time.
        /// </summary>
        /// <param name="delay">The amount of time to wait.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A <see cref="Task"/> for the wait operation.</returns>
        Task AwaitTimeAsync(TimeSpan delay, CancellationToken cancellationToken);
    }

    internal class TimeAwaiter : ITimeAwaiter
    {
        async Task ITimeAwaiter.AwaitTimeAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            await UnityTask.Delay(delay, cancellationToken).UnityConfigureAwait(false);
        }
    }

    /// <summary>
    /// An class for awaiting a specific amount of time.
    /// </summary>
    public class CoreTimeAwaiter : ITimeAwaiter
    {
        /// <inheritdoc/>
        async Task ITimeAwaiter.AwaitTimeAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken).UnityConfigureAwait(false);
        }
    }

    /// <summary>
    /// Helper methods for <see cref="ITimeAwaiter"/>.
    /// </summary>
    internal static class TimeAwaiterExtensions
    {
        internal static Task AwaitTimeAsync(this ITimeAwaiter timeAwaiter, int delayMilliseconds, CancellationToken cancellationToken)
        {
            return timeAwaiter.AwaitTimeAsync(new TimeSpan(0, 0, 0, 0, delayMilliseconds), cancellationToken);
        }
    }
}

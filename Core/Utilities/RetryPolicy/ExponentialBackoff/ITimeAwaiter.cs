using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    internal interface ITimeAwaiter
    {
        Task AwaitTimeAsync(TimeSpan delay, CancellationToken cancellationToken);
    }

    internal class TimeAwaiter : ITimeAwaiter
    {
        async Task ITimeAwaiter.AwaitTimeAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            await UnityTask.Delay(delay, cancellationToken).UnityConfigureAwait(false);
        }
    }

    internal static class TimeAwaiterExtensions
    {
        internal static Task AwaitTimeAsync(this ITimeAwaiter timeAwaiter, int delayMilliseconds, CancellationToken cancellationToken)
        {
            return timeAwaiter.AwaitTimeAsync(new TimeSpan(0, 0, 0, 0, delayMilliseconds), cancellationToken);
        }
    }
}

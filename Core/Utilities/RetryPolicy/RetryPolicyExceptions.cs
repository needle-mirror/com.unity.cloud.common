using System;
using System.Runtime.Serialization;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This exception is thrown by <see cref="IRetryPolicy.ExecuteAsync{T}(IRetryPolicy.RetriedOperation{T}, IRetryPolicy.ShouldRetryChecker{T}, System.Threading.CancellationToken, IProgress{RetryQueuedProgress})"/>
    /// when the policy expires after too many retries.
    /// </summary>
    [Serializable]
    public class RetryExpiredException : Exception
    {
        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public RetryExpiredException() : base()
        { }

        protected RetryExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown by <see cref="IRetryPolicy.ExecuteAsync{T}(IRetryPolicy.RetriedOperation{T}, IRetryPolicy.ShouldRetryChecker{T}, System.Threading.CancellationToken, IProgress{RetryQueuedProgress})"/>
    /// when a critical exception (passed as innerException) occurs during execution of the retry policy.
    /// </summary>
    [Serializable]
    public class RetryExecutionFailedException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="innerException">The inner exception that triggered this failure of retry execution.</param>
        public RetryExecutionFailedException(Exception innerException) : base(default, innerException)
        { }

        protected RetryExecutionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }

}

#if NUGET_MOQ_AVAILABLE && !ENABLE_IL2CPP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Unity.Cloud.Common;
using UnityEngine.TestTools;

namespace Unity.Cloud.AppLinking.Tests
{
    public class AsyncUrlRedirectAwaiterTests
    {
        static class TestParameters
        {
            internal const int RefreshDelay = 10;
            internal const int TimeoutDelay = 30;
        }

        static readonly List<int> k_DelayTestCases = new() { -100, - 1, 0, 1, 100 };

        [Test]
        public void RefreshDelay_AssignDelay_ReturnsValidValue([ValueSource(nameof(k_DelayTestCases))] int delay)
        {
            // Given an AsyncPkceRedirectAwaiter
            var awaiter = GivenAnAwaiter();

            // When the refresh delay is set
            awaiter.RefreshDelay = delay;

            // Then the assigned delay is the expected value or 0 if passed value was negative
            Assert.AreEqual(delay < 0 ? 0 : delay, awaiter.RefreshDelay);
        }

        [Test]
        public void TimeoutDelay_AssignDelay_ReturnsValidValue([ValueSource(nameof(k_DelayTestCases))] int delay)
        {
            // Given an AsyncPkceRedirectAwaiter
            var awaiter = GivenAnAwaiter();

            // When the timeout delay is set
            awaiter.TimeoutDelay = delay;

            // Then the assigned delay is the expected value or 0 if passed value was negative
            Assert.AreEqual(delay < 0 ? 0 : delay, awaiter.TimeoutDelay);
        }

        [Test]
        public void SetResult_AssignRedirectResult_HasValidResult()
        {
            // Given an AsyncPkceRedirectAwaiter which has no result
            var awaiter = GivenAnAwaiter();
            Assert.IsFalse(awaiter.HasResult);

            // When setting the result
            var redirectResult = new UrlRedirectResult() {Status = UrlRedirectStatus.Success};
            awaiter.SetResult(redirectResult);

            // Then HasResult should be true
            Assert.IsTrue(awaiter.HasResult);
            Assert.IsTrue(awaiter.RedirectResult.HasValue);
            Assert.AreEqual(redirectResult, awaiter.RedirectResult.Value);
        }

        [Test]
        public void SetResult_NullResult_HasNoResult()
        {
            // Given an AsyncPkceRedirectAwaiter which has no result
            var awaiter = GivenAnAwaiter();
            Assert.IsFalse(awaiter.HasResult);

            // When setting the result to null
            awaiter.SetResult(null);

            // Then HasResult should be true
            Assert.IsFalse(awaiter.HasResult);
            Assert.IsFalse(awaiter.RedirectResult.HasValue);
        }

        [Test]
        public void BeginWait_AwaiterContainedResult_ResultIsReset()
        {
            // Given an AsyncPkceRedirectAwaiter with a result
            var awaiter = GivenAnAwaiter();
            awaiter.SetResult(new UrlRedirectResult());
            Assert.IsTrue(awaiter.HasResult);

            // When beginning a new wait
            awaiter.BeginWait();

            // Then the awaiter should have no result
            Assert.IsFalse(awaiter.HasResult);
            Assert.IsNull(awaiter.RedirectResult);
        }

        [UnityTest]
        public IEnumerator HasTimedOut_WaitLongerThanTimeout_AwaiterTimesOut()
        {
            const int numWaits = 3;
            // Given an AsyncPkceRedirectAwaiter with a timeout delay
            var awaiter = GivenAnAwaiter();
            awaiter.RefreshDelay = 10;
            awaiter.TimeoutDelay = awaiter.RefreshDelay * numWaits;

            // When beginning a new wait and waiting enough times for a timeout
            awaiter.BeginWait();
            for (int i = 0; i < numWaits; ++i)
            {
                Assert.IsFalse(awaiter.HasTimedOut);
                var waitTask = awaiter.WaitForRefreshAsync();
                while (!waitTask.IsCompleted)
                    yield return null;
            }

            // Then the awaiter should be timed out
            Assert.IsTrue(awaiter.HasTimedOut);
        }

        [Test]
        public void WaitForRefreshAsync_TaskCancelled_ThrowsException()
        {
            // Given an AsyncPkceRedirectAwaiter with a time-awaiter that is cancelled
            var mockTimeAwaiter = new Mock<ITimeAwaiter>();
            mockTimeAwaiter.Setup(m => m.AwaitTimeAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Throws(new TaskCanceledException());

            var awaiter = new AsyncUrlRedirectAwaiter(mockTimeAwaiter.Object);

            // When waiting for a refresh
            async Task WaitOperation() => await awaiter.WaitForRefreshAsync();

            // Then the awaiter should throw a tack cancelled exception
            var aggregateException = Assert.Catch(() => WaitOperation().Wait());
            Assert.IsInstanceOf<TaskCanceledException>(aggregateException.InnerException);
        }

        static AsyncUrlRedirectAwaiter GivenAnAwaiter()
        {
            var mockTimeAwaiter = new Mock<ITimeAwaiter>();
            mockTimeAwaiter.Setup(m => m.AwaitTimeAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var awaiter = new AsyncUrlRedirectAwaiter(mockTimeAwaiter.Object);
            awaiter.RefreshDelay = TestParameters.RefreshDelay;
            awaiter.TimeoutDelay = TestParameters.TimeoutDelay;

            return awaiter;
        }
    }
}
#endif

using System;
using System.Runtime;
using System.Threading;

internal sealed class BackoffTimeoutHelper
{
    private static readonly int maxSkewMilliseconds = (int)(IOThreadTimer.SystemTimeResolutionTicks / 10000L);
    private static readonly long maxDriftTicks = IOThreadTimer.SystemTimeResolutionTicks * 2L;
    private static readonly TimeSpan defaultInitialWaitTime = TimeSpan.FromMilliseconds(1.0);
    private static readonly TimeSpan defaultMaxWaitTime = TimeSpan.FromMinutes(1.0);
    private DateTime deadline;
    private TimeSpan maxWaitTime;
    private TimeSpan waitTime;
    private IOThreadTimer backoffTimer;
    private Action<object> backoffCallback;
    private object backoffState;
    private Random random;
    private TimeSpan originalTimeout;

    internal BackoffTimeoutHelper(TimeSpan timeout)
      : this(timeout, BackoffTimeoutHelper.defaultMaxWaitTime)
    {
    }

    internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime)
      : this(timeout, maxWaitTime, BackoffTimeoutHelper.defaultInitialWaitTime)
    {
    }

    internal BackoffTimeoutHelper(TimeSpan timeout, TimeSpan maxWaitTime, TimeSpan initialWaitTime)
    {
        this.random = new Random(this.GetHashCode());
        this.maxWaitTime = maxWaitTime;
        this.originalTimeout = timeout;
        this.Reset(timeout, initialWaitTime);
    }

    public TimeSpan OriginalTimeout
    {
        get
        {
            return this.originalTimeout;
        }
    }

    private void Reset(TimeSpan timeout, TimeSpan initialWaitTime)
    {
        this.deadline = !(timeout == TimeSpan.MaxValue) ? DateTime.UtcNow + timeout : DateTime.MaxValue;
        this.waitTime = initialWaitTime;
    }

    public bool IsExpired()
    {
        return !(this.deadline == DateTime.MaxValue) && DateTime.UtcNow >= this.deadline;
    }

    public void WaitAndBackoff(Action<object> callback, object state)
    {
        if (this.backoffCallback != callback || this.backoffState != state)
        {
            if (this.backoffTimer != null)
                this.backoffTimer.Cancel();
            this.backoffCallback = callback;
            this.backoffState = state;
            this.backoffTimer = new IOThreadTimer(callback, state, false, BackoffTimeoutHelper.maxSkewMilliseconds);
        }
        TimeSpan timeFromNow = this.WaitTimeWithDrift();
        this.Backoff();
        this.backoffTimer.Set(timeFromNow);
    }

    public void WaitAndBackoff()
    {
        Thread.Sleep(this.WaitTimeWithDrift());
        this.Backoff();
    }

    private TimeSpan WaitTimeWithDrift()
    {
        return Ticks.ToTimeSpan(Math.Max(Ticks.FromTimeSpan(BackoffTimeoutHelper.defaultInitialWaitTime), Ticks.Add(Ticks.FromTimeSpan(this.waitTime), (long)(uint)this.random.Next() % (2L * BackoffTimeoutHelper.maxDriftTicks + 1L) - BackoffTimeoutHelper.maxDriftTicks)));
    }

    private void Backoff()
    {
        this.waitTime = this.waitTime.Ticks < this.maxWaitTime.Ticks / 2L ? TimeSpan.FromTicks(this.waitTime.Ticks * 2L) : this.maxWaitTime;
        if (!(this.deadline != DateTime.MaxValue))
            return;
        TimeSpan timeSpan = this.deadline - DateTime.UtcNow;
        if (!(this.waitTime > timeSpan))
            return;
        this.waitTime = timeSpan;
        if (!(this.waitTime < TimeSpan.Zero))
            return;
        this.waitTime = TimeSpan.Zero;
    }
}

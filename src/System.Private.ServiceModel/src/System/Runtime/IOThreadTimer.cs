// Decompiled with JetBrains decompiler
// Type: System.Runtime.IOThreadTimer
// Assembly: System.ServiceModel.Internals, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
// MVID: B89734F1-999A-409B-A8EF-2CA1D9D0F581
// Assembly location: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.ServiceModel.Internals.dll

using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Security;
using System.ServiceModel.Channels;
using System.Threading;

namespace System.Runtime
{
    internal class IOThreadTimer
    {
        private static long systemTimeResolutionTicks = -1;
        private const int maxSkewInMillisecondsDefault = 100;
        private Action<object> callback;
        private object callbackState;
        private long dueTime;
        private int index;
        private long maxSkew;
        private IOThreadTimer.TimerGroup timerGroup;

        public IOThreadTimer(
          Action<object> callback,
          object callbackState,
          bool isTypicallyCanceledShortlyAfterBeingSet)
          : this(callback, callbackState, isTypicallyCanceledShortlyAfterBeingSet, 100)
        {
        }

        public IOThreadTimer(
          Action<object> callback,
          object callbackState,
          bool isTypicallyCanceledShortlyAfterBeingSet,
          int maxSkewInMilliseconds)
        {
            this.callback = callback;
            this.callbackState = callbackState;
            this.maxSkew = Ticks.FromMilliseconds(maxSkewInMilliseconds);
            this.timerGroup = isTypicallyCanceledShortlyAfterBeingSet ? IOThreadTimer.TimerManager.Value.VolatileTimerGroup : IOThreadTimer.TimerManager.Value.StableTimerGroup;
        }

        public static long SystemTimeResolutionTicks
        {
            get
            {
                if (IOThreadTimer.systemTimeResolutionTicks == -1L)
                    IOThreadTimer.systemTimeResolutionTicks = IOThreadTimer.GetSystemTimeResolution();
                return IOThreadTimer.systemTimeResolutionTicks;
            }
        }

        [SecuritySafeCritical]
        private static long GetSystemTimeResolution()
        {
            uint increment;
            return UnsafeNativeMethods.GetSystemTimeAdjustment(out int _, out increment, out uint _) != 0U ? (long)increment : 150000L;
        }

        public bool Cancel()
        {
            return IOThreadTimer.TimerManager.Value.Cancel(this);
        }

        public void Set(TimeSpan timeFromNow)
        {
            if (!(timeFromNow != TimeSpan.MaxValue))
                return;
            this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromTimeSpan(timeFromNow)));
        }

        public void Set(int millisecondsFromNow)
        {
            this.SetAt(Ticks.Add(Ticks.Now, Ticks.FromMilliseconds(millisecondsFromNow)));
        }

        public void SetAt(long dueTime)
        {
            IOThreadTimer.TimerManager.Value.Set(this, dueTime);
        }

        private class TimerManager
        {
            private static IOThreadTimer.TimerManager value = new IOThreadTimer.TimerManager();
            private const long maxTimeToWaitForMoreTimers = 10000000;
            private Action<object> onWaitCallback;
            private IOThreadTimer.TimerGroup stableTimerGroup;
            private IOThreadTimer.TimerGroup volatileTimerGroup;
            private IOThreadTimer.WaitableTimer[] waitableTimers;
            private bool waitScheduled;

            public TimerManager()
            {
                this.onWaitCallback = new Action<object>(this.OnWaitCallback);
                this.stableTimerGroup = new IOThreadTimer.TimerGroup();
                this.volatileTimerGroup = new IOThreadTimer.TimerGroup();
                this.waitableTimers = new IOThreadTimer.WaitableTimer[2]
                {
          this.stableTimerGroup.WaitableTimer,
          this.volatileTimerGroup.WaitableTimer
                };
            }

            private object ThisLock
            {
                get
                {
                    return (object)this;
                }
            }

            public static IOThreadTimer.TimerManager Value
            {
                get
                {
                    return IOThreadTimer.TimerManager.value;
                }
            }

            public IOThreadTimer.TimerGroup StableTimerGroup
            {
                get
                {
                    return this.stableTimerGroup;
                }
            }

            public IOThreadTimer.TimerGroup VolatileTimerGroup
            {
                get
                {
                    return this.volatileTimerGroup;
                }
            }

            public void Set(IOThreadTimer timer, long dueTime)
            {
                long num = dueTime - timer.dueTime;
                if (num < 0L)
                    num = -num;
                if (num <= timer.maxSkew)
                    return;
                lock (this.ThisLock)
                {
                    IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
                    IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                    if (timer.index > 0)
                    {
                        if (!timerQueue.UpdateTimer(timer, dueTime))
                            return;
                        this.UpdateWaitableTimer(timerGroup);
                    }
                    else
                    {
                        if (!timerQueue.InsertTimer(timer, dueTime))
                            return;
                        this.UpdateWaitableTimer(timerGroup);
                        if (timerQueue.Count != 1)
                            return;
                        this.EnsureWaitScheduled();
                    }
                }
            }

            public bool Cancel(IOThreadTimer timer)
            {
                lock (this.ThisLock)
                {
                    if (timer.index <= 0)
                        return false;
                    IOThreadTimer.TimerGroup timerGroup = timer.timerGroup;
                    IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                    timerQueue.DeleteTimer(timer);
                    if (timerQueue.Count > 0)
                    {
                        this.UpdateWaitableTimer(timerGroup);
                    }
                    else
                    {
                        IOThreadTimer.TimerGroup otherTimerGroup = this.GetOtherTimerGroup(timerGroup);
                        if (otherTimerGroup.TimerQueue.Count == 0)
                        {
                            long now = Ticks.Now;
                            long num1 = timerGroup.WaitableTimer.DueTime - now;
                            long num2 = otherTimerGroup.WaitableTimer.DueTime - now;
                            if (num1 > 10000000L && num2 > 10000000L)
                                timerGroup.WaitableTimer.Set(Ticks.Add(now, 10000000L));
                        }
                    }
                    return true;
                }
            }

            private void EnsureWaitScheduled()
            {
                if (this.waitScheduled)
                    return;
                this.ScheduleWait();
            }

            private IOThreadTimer.TimerGroup GetOtherTimerGroup(
              IOThreadTimer.TimerGroup timerGroup)
            {
                return timerGroup == this.volatileTimerGroup ? this.stableTimerGroup : this.volatileTimerGroup;
            }

            private void OnWaitCallback(object state)
            {
                WaitHandle.WaitAny((WaitHandle[])this.waitableTimers);
                long now = Ticks.Now;
                lock (this.ThisLock)
                {
                    this.waitScheduled = false;
                    this.ScheduleElapsedTimers(now);
                    this.ReactivateWaitableTimers();
                    this.ScheduleWaitIfAnyTimersLeft();
                }
            }

            private void ReactivateWaitableTimers()
            {
                this.ReactivateWaitableTimer(this.stableTimerGroup);
                this.ReactivateWaitableTimer(this.volatileTimerGroup);
            }

            private void ReactivateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
            {
                IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                if (timerQueue.Count > 0)
                    timerGroup.WaitableTimer.Set(timerQueue.MinTimer.dueTime);
                else
                    timerGroup.WaitableTimer.Set(long.MaxValue);
            }

            private void ScheduleElapsedTimers(long now)
            {
                this.ScheduleElapsedTimers(this.stableTimerGroup, now);
                this.ScheduleElapsedTimers(this.volatileTimerGroup, now);
            }

            private void ScheduleElapsedTimers(IOThreadTimer.TimerGroup timerGroup, long now)
            {
                IOThreadTimer.TimerQueue timerQueue = timerGroup.TimerQueue;
                while (timerQueue.Count > 0)
                {
                    IOThreadTimer minTimer = timerQueue.MinTimer;
                    if (minTimer.dueTime - now > minTimer.maxSkew)
                        break;
                    timerQueue.DeleteMinTimer();
                    ActionItem.Schedule(minTimer.callback, minTimer.callbackState);
                }
            }

            private void ScheduleWait()
            {
                ActionItem.Schedule(this.onWaitCallback, (object)null);
                this.waitScheduled = true;
            }

            private void ScheduleWaitIfAnyTimersLeft()
            {
                if (this.stableTimerGroup.TimerQueue.Count <= 0 && this.volatileTimerGroup.TimerQueue.Count <= 0)
                    return;
                this.ScheduleWait();
            }

            private void UpdateWaitableTimer(IOThreadTimer.TimerGroup timerGroup)
            {
                IOThreadTimer.WaitableTimer waitableTimer = timerGroup.WaitableTimer;
                IOThreadTimer minTimer = timerGroup.TimerQueue.MinTimer;
                long num = waitableTimer.DueTime - minTimer.dueTime;
                if (num < 0L)
                    num = -num;
                if (num <= minTimer.maxSkew)
                    return;
                waitableTimer.Set(minTimer.dueTime);
            }
        }

        private class TimerGroup
        {
            private IOThreadTimer.TimerQueue timerQueue;
            private IOThreadTimer.WaitableTimer waitableTimer;

            public TimerGroup()
            {
                this.waitableTimer = new IOThreadTimer.WaitableTimer();
                this.waitableTimer.Set(long.MaxValue);
                this.timerQueue = new IOThreadTimer.TimerQueue();
            }

            public IOThreadTimer.TimerQueue TimerQueue
            {
                get
                {
                    return this.timerQueue;
                }
            }

            public IOThreadTimer.WaitableTimer WaitableTimer
            {
                get
                {
                    return this.waitableTimer;
                }
            }
        }

        private class TimerQueue
        {
            private int count;
            private IOThreadTimer[] timers;

            public TimerQueue()
            {
                this.timers = new IOThreadTimer[4];
            }

            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public IOThreadTimer MinTimer
            {
                get
                {
                    return this.timers[1];
                }
            }

            public void DeleteMinTimer()
            {
                IOThreadTimer minTimer = this.MinTimer;
                this.DeleteMinTimerCore();
                minTimer.index = 0;
                minTimer.dueTime = 0L;
            }

            public void DeleteTimer(IOThreadTimer timer)
            {
                int index1 = timer.index;
                IOThreadTimer[] timers = this.timers;
                while (true)
                {
                    int index2 = index1 / 2;
                    if (index2 >= 1)
                    {
                        IOThreadTimer ioThreadTimer = timers[index2];
                        timers[index1] = ioThreadTimer;
                        ioThreadTimer.index = index1;
                        index1 = index2;
                    }
                    else
                        break;
                }
                timer.index = 0;
                timer.dueTime = 0L;
                timers[1] = (IOThreadTimer)null;
                this.DeleteMinTimerCore();
            }

            public bool InsertTimer(IOThreadTimer timer, long dueTime)
            {
                IOThreadTimer[] ioThreadTimerArray = this.timers;
                int index1 = this.count + 1;
                if (index1 == ioThreadTimerArray.Length)
                {
                    ioThreadTimerArray = new IOThreadTimer[ioThreadTimerArray.Length * 2];
                    Array.Copy((Array)this.timers, (Array)ioThreadTimerArray, this.timers.Length);
                    this.timers = ioThreadTimerArray;
                }
                this.count = index1;
                if (index1 > 1)
                {
                    while (true)
                    {
                        int index2 = index1 / 2;
                        if (index2 != 0)
                        {
                            IOThreadTimer ioThreadTimer = ioThreadTimerArray[index2];
                            if (ioThreadTimer.dueTime > dueTime)
                            {
                                ioThreadTimerArray[index1] = ioThreadTimer;
                                ioThreadTimer.index = index1;
                                index1 = index2;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
                ioThreadTimerArray[index1] = timer;
                timer.index = index1;
                timer.dueTime = dueTime;
                return index1 == 1;
            }

            public bool UpdateTimer(IOThreadTimer timer, long dueTime)
            {
                int index1 = timer.index;
                IOThreadTimer[] timers = this.timers;
                int count = this.count;
                int index2 = index1 / 2;
                if (index2 == 0 || timers[index2].dueTime <= dueTime)
                {
                    int index3 = index1 * 2;
                    if (index3 > count || timers[index3].dueTime >= dueTime)
                    {
                        int index4 = index3 + 1;
                        if (index4 > count || timers[index4].dueTime >= dueTime)
                        {
                            timer.dueTime = dueTime;
                            return index1 == 1;
                        }
                    }
                }
                this.DeleteTimer(timer);
                this.InsertTimer(timer, dueTime);
                return true;
            }

            private void DeleteMinTimerCore()
            {
                int count = this.count;
                if (count == 1)
                {
                    this.count = 0;
                    this.timers[1] = (IOThreadTimer)null;
                }
                else
                {
                    IOThreadTimer[] timers = this.timers;
                    IOThreadTimer ioThreadTimer1 = timers[count];
                    int num;
                    this.count = num = count - 1;
                    int index1 = 1;
                    int index2;
                    do
                    {
                        index2 = index1 * 2;
                        if (index2 <= num)
                        {
                            IOThreadTimer ioThreadTimer2;
                            int index3;
                            if (index2 < num)
                            {
                                IOThreadTimer ioThreadTimer3 = timers[index2];
                                int index4 = index2 + 1;
                                IOThreadTimer ioThreadTimer4 = timers[index4];
                                if (ioThreadTimer4.dueTime < ioThreadTimer3.dueTime)
                                {
                                    ioThreadTimer2 = ioThreadTimer4;
                                    index3 = index4;
                                }
                                else
                                {
                                    ioThreadTimer2 = ioThreadTimer3;
                                    index3 = index2;
                                }
                            }
                            else
                            {
                                index3 = index2;
                                ioThreadTimer2 = timers[index3];
                            }
                            if (ioThreadTimer1.dueTime > ioThreadTimer2.dueTime)
                            {
                                timers[index1] = ioThreadTimer2;
                                ioThreadTimer2.index = index1;
                                index1 = index3;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                    while (index2 < num);
                    timers[index1] = ioThreadTimer1;
                    ioThreadTimer1.index = index1;
                    timers[num + 1] = (IOThreadTimer)null;
                }
            }
        }

        private class WaitableTimer : WaitHandle
        {
            private long dueTime;

            [SecuritySafeCritical]
            public WaitableTimer()
            {
                this.SafeWaitHandle = IOThreadTimer.WaitableTimer.TimerHelper.CreateWaitableTimer();
            }

            public long DueTime
            {
                get
                {
                    return this.dueTime;
                }
            }

            [SecuritySafeCritical]
            public void Set(long dueTime)
            {
                this.dueTime = IOThreadTimer.WaitableTimer.TimerHelper.Set(this.SafeWaitHandle, dueTime);
            }

            [SecurityCritical]
            private static class TimerHelper
            {
                public static SafeWaitHandle CreateWaitableTimer()
                {
                    SafeWaitHandle waitableTimer = UnsafeNativeMethods.CreateWaitableTimer(IntPtr.Zero, false, (string)null);
                    if (waitableTimer.IsInvalid)
                    {
                        Exception exception = (Exception)new Win32Exception();
                        waitableTimer.SetHandleAsInvalid();
                        throw Fx.Exception.AsError(exception);
                    }
                    return waitableTimer;
                }

                public static long Set(SafeWaitHandle timer, long dueTime)
                {
                    if (!UnsafeNativeMethods.SetWaitableTimer(timer, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false))
                        throw Fx.Exception.AsError((Exception)new Win32Exception());
                    return dueTime;
                }
            }
        }
    }
}

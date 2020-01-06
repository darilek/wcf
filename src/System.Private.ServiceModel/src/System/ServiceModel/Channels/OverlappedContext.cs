// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Channels.OverlappedContext
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.ServiceModel.Channels
{
    internal delegate void OverlappedIOCompleteCallback(bool haveResult, int error, int bytesRead);

    internal class OverlappedContext
    {
        private static byte[] dummyBuffer = new byte[0];
        private const int HandleOffsetFromOverlapped32 = -4;
        private const int HandleOffsetFromOverlapped64 = -3;
        private static IOCompletionCallback completeCallback;
        private static WaitOrTimerCallback eventCallback;
        private static WaitOrTimerCallback cleanupCallback;
        private object[] bufferHolder;
        private unsafe byte* bufferPtr;
        private unsafe NativeOverlapped* nativeOverlapped;
        private GCHandle pinnedHandle;
        private object pinnedTarget;
        private Overlapped overlapped;
        private OverlappedContext.RootedHolder rootedHolder;
        private OverlappedIOCompleteCallback pendingCallback;
        private bool deferredFree;
        private bool syncOperationPending;
        private ManualResetEvent completionEvent;
        private IntPtr eventHandle;
        private RegisteredWaitHandle registration;


        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe OverlappedContext()
        {
            if (OverlappedContext.completeCallback == null)
                OverlappedContext.completeCallback = Fx.ThunkCallback(new IOCompletionCallback(OverlappedContext.CompleteCallback));
            if (OverlappedContext.eventCallback == null)
                OverlappedContext.eventCallback = Fx.ThunkCallback(new WaitOrTimerCallback(OverlappedContext.EventCallback));
            if (OverlappedContext.cleanupCallback == null)
                OverlappedContext.cleanupCallback = Fx.ThunkCallback(new WaitOrTimerCallback(OverlappedContext.CleanupCallback));
            this.bufferHolder = new object[1]
            {
        (object) OverlappedContext.dummyBuffer
            };
            this.overlapped = new Overlapped();
            this.nativeOverlapped = this.overlapped.UnsafePack(OverlappedContext.completeCallback, (object)this.bufferHolder);
            this.pinnedHandle = GCHandle.FromIntPtr(((IntPtr*)this.nativeOverlapped)[(IntPtr.Size == 4 ? new IntPtr(-4) : new IntPtr(-3)).ToInt64()]);
            this.pinnedTarget = this.pinnedHandle.Target;
            this.rootedHolder = new OverlappedContext.RootedHolder();
            this.overlapped.AsyncResult = (IAsyncResult)this.rootedHolder;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        unsafe ~OverlappedContext()
        {
            if ((IntPtr)this.nativeOverlapped == IntPtr.Zero || AppDomain.CurrentDomain.IsFinalizingForUnload() || Environment.HasShutdownStarted)
                return;
            if (this.syncOperationPending)
                ThreadPool.UnsafeRegisterWaitForSingleObject((WaitHandle)this.rootedHolder.EventHolder, OverlappedContext.cleanupCallback, (object)this, -1, true);
            else
                Overlapped.Free(this.nativeOverlapped);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe void Free()
        {
            if (this.pendingCallback != null)
                throw Fx.AssertAndThrow("OverlappedContext.Free called while async operation is pending.");
            if (this.syncOperationPending)
                throw Fx.AssertAndThrow("OverlappedContext.Free called while sync operation is pending.");
            if ((IntPtr)this.nativeOverlapped == IntPtr.Zero)
                throw Fx.AssertAndThrow("OverlappedContext.Free called multiple times.");
            this.pinnedTarget = (object)null;
            NativeOverlapped* nativeOverlapped = this.nativeOverlapped;
            this.nativeOverlapped = (NativeOverlapped*)null;
            Overlapped.Free(nativeOverlapped);
            if (this.completionEvent != null)
                this.completionEvent.Close();
            GC.SuppressFinalize((object)this);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public bool FreeOrDefer()
        {
            if (this.pendingCallback != null || this.syncOperationPending)
            {
                this.deferredFree = true;
                return false;
            }
            this.Free();
            return true;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public bool FreeIfDeferred()
        {
            return this.deferredFree && this.FreeOrDefer();
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe void StartAsyncOperation(
          byte[] buffer,
          OverlappedIOCompleteCallback callback,
          bool bound)
        {
            if (callback == null)
                throw Fx.AssertAndThrow("StartAsyncOperation called with null callback.");
            if (this.pendingCallback != null)
                throw Fx.AssertAndThrow("StartAsyncOperation called while another is in progress.");
            if (this.syncOperationPending)
                throw Fx.AssertAndThrow("StartAsyncOperation called while a sync operation was already pending.");
            if ((IntPtr)this.nativeOverlapped == IntPtr.Zero)
                throw Fx.AssertAndThrow("StartAsyncOperation called on freed OverlappedContext.");
            this.pendingCallback = callback;
            if (buffer != null)
            {
                this.bufferHolder[0] = (object)buffer;
                this.pinnedHandle.Target = this.pinnedTarget;
                this.bufferPtr = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement((Array)buffer, 0);
            }
            if (bound)
            {
                this.overlapped.EventHandleIntPtr = IntPtr.Zero;
                this.rootedHolder.ThisHolder = this;
            }
            else
            {
                if (this.completionEvent != null)
                    this.completionEvent.Reset();
                this.overlapped.EventHandleIntPtr = this.EventHandle;
                this.registration = ThreadPool.UnsafeRegisterWaitForSingleObject((WaitHandle)this.completionEvent, OverlappedContext.eventCallback, (object)this, -1, true);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe void CancelAsyncOperation()
        {
            this.rootedHolder.ThisHolder = (OverlappedContext)null;
            if (this.registration != null)
            {
                this.registration.Unregister((WaitHandle)null);
                this.registration = (RegisteredWaitHandle)null;
            }
            this.bufferPtr = (byte*)null;
            this.bufferHolder[0] = (object)OverlappedContext.dummyBuffer;
            this.pendingCallback = (OverlappedIOCompleteCallback)null;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe void StartSyncOperation(byte[] buffer, ref object holder)
        {
            if (this.syncOperationPending)
                throw Fx.AssertAndThrow("StartSyncOperation called while an operation was already pending.");
            if (this.pendingCallback != null)
                throw Fx.AssertAndThrow("StartSyncOperation called while an async operation was already pending.");
            if ((IntPtr)this.nativeOverlapped == IntPtr.Zero)
                throw Fx.AssertAndThrow("StartSyncOperation called on freed OverlappedContext.");
            this.overlapped.EventHandleIntPtr = this.EventHandle;
            this.rootedHolder.EventHolder = this.completionEvent;
            this.syncOperationPending = true;
            if (buffer == null)
                return;
            holder = (object)buffer;
            this.pinnedHandle.Target = this.pinnedTarget;
            this.bufferPtr = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement((Array)buffer, 0);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public bool WaitForSyncOperation(TimeSpan timeout)
        {
            return this.WaitForSyncOperation(timeout, ref this.bufferHolder[0]);
        }

        [SecurityCritical]
        public unsafe bool WaitForSyncOperation(TimeSpan timeout, ref object holder)
        {
            if (!this.syncOperationPending)
                throw Fx.AssertAndThrow("WaitForSyncOperation called while no operation was pending.");
            if (!UnsafeNativeMethods.HasOverlappedIoCompleted(this.nativeOverlapped) && !TimeoutHelper.WaitOne((WaitHandle)this.completionEvent, timeout))
            {
                GC.SuppressFinalize((object)this);
                ThreadPool.UnsafeRegisterWaitForSingleObject((WaitHandle)this.completionEvent, OverlappedContext.cleanupCallback, (object)this, -1, true);
                return false;
            }
            this.CancelSyncOperation(ref holder);
            return true;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe void CancelSyncOperation(ref object holder)
        {
            this.bufferPtr = (byte*)null;
            holder = (object)OverlappedContext.dummyBuffer;
            this.syncOperationPending = false;
            this.rootedHolder.EventHolder = (ManualResetEvent)null;
        }

        public object[] Holder
        {
            [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                return this.bufferHolder;
            }
        }

        public unsafe byte* BufferPtr
        {
            [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                byte* bufferPtr = this.bufferPtr;
                if ((IntPtr)bufferPtr == IntPtr.Zero)
                    throw Fx.AssertAndThrow("Pointer requested while no operation pending or no buffer provided.");
                return bufferPtr;
            }
        }

        public unsafe System.Threading.NativeOverlapped* NativeOverlapped
        {
            [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                System.Threading.NativeOverlapped* nativeOverlapped = this.nativeOverlapped;
                if ((IntPtr)nativeOverlapped == IntPtr.Zero)
                    throw Fx.AssertAndThrow("NativeOverlapped pointer requested after it was freed.");
                return nativeOverlapped;
            }
        }

        private IntPtr EventHandle
        {
            get
            {
                if (this.completionEvent == null)
                {
                    this.completionEvent = new ManualResetEvent(false);
                    this.eventHandle = (IntPtr)(1L | (long)this.completionEvent.SafeWaitHandle.DangerousGetHandle());
                }
                return this.eventHandle;
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private static unsafe void CompleteCallback(
          uint error,
          uint numBytes,
          System.Threading.NativeOverlapped* nativeOverlapped)
        {
            OverlappedContext thisHolder = ((OverlappedContext.RootedHolder)Overlapped.Unpack(nativeOverlapped).AsyncResult).ThisHolder;
            thisHolder.rootedHolder.ThisHolder = (OverlappedContext)null;
            thisHolder.bufferPtr = (byte*)null;
            thisHolder.bufferHolder[0] = (object)OverlappedContext.dummyBuffer;
            OverlappedIOCompleteCallback pendingCallback = thisHolder.pendingCallback;
            thisHolder.pendingCallback = (OverlappedIOCompleteCallback)null;
            pendingCallback(true, (int)error, checked((int)numBytes));
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private static unsafe void EventCallback(object state, bool timedOut)
        {
            OverlappedContext overlappedContext = state as OverlappedContext;
            if (timedOut)
            {
                if (overlappedContext == null || overlappedContext.rootedHolder == null)
                    DiagnosticUtility.FailFast("Can't prevent heap corruption.");
                overlappedContext.rootedHolder.ThisHolder = overlappedContext;
            }
            else
            {
                overlappedContext.registration = (RegisteredWaitHandle)null;
                overlappedContext.bufferPtr = (byte*)null;
                overlappedContext.bufferHolder[0] = (object)OverlappedContext.dummyBuffer;
                OverlappedIOCompleteCallback pendingCallback = overlappedContext.pendingCallback;
                overlappedContext.pendingCallback = (OverlappedIOCompleteCallback)null;
                pendingCallback(false, 0, 0);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private static unsafe void CleanupCallback(object state, bool timedOut)
        {
            OverlappedContext overlappedContext = state as OverlappedContext;
            if (timedOut)
                return;
            overlappedContext.pinnedTarget = (object)null;
            overlappedContext.rootedHolder.EventHolder.Close();
            Overlapped.Free(overlappedContext.nativeOverlapped);
        }

        private class RootedHolder : IAsyncResult
        {
            private OverlappedContext overlappedBuffer;
            private ManualResetEvent eventHolder;

            public OverlappedContext ThisHolder
            {
                get
                {
                    return this.overlappedBuffer;
                }
                set
                {
                    this.overlappedBuffer = value;
                }
            }

            public ManualResetEvent EventHolder
            {
                get
                {
                    return this.eventHolder;
                }
                set
                {
                    this.eventHolder = value;
                }
            }

            object IAsyncResult.AsyncState
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.AsyncState called.");
                }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.AsyncWaitHandle called.");
                }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.CompletedSynchronously called.");
                }
            }

            bool IAsyncResult.IsCompleted
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.IsCompleted called.");
                }
            }
        }
    }
}

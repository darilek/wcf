// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Channels.PipeConnection
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.ServiceModel.Activation;
using System.ServiceModel.Diagnostics;
using System.Threading;

namespace System.ServiceModel.Channels
{
    internal sealed class PipeConnection : IConnection
    {
        private object readLock = new object();
        private object writeLock = new object();
        private PipeHandle pipe;
        private PipeConnection.CloseState closeState;
        private bool aborted;
        private bool isBoundToCompletionPort;
        private bool autoBindToCompletionPort;
        private TraceEventType exceptionEventType;
        private static byte[] zeroBuffer;
        private bool inReadingState;
        private bool isReadOutstanding;
        private OverlappedContext readOverlapped;
        private byte[] asyncReadBuffer;
        private int readBufferSize;
        private ManualResetEvent atEOFEvent;
        private bool isAtEOF;
        private OverlappedIOCompleteCallback onAsyncReadComplete;
        private Exception asyncReadException;
        private WaitCallback asyncReadCallback;
        private object asyncReadCallbackState;
        private int asyncBytesRead;
        private bool inWritingState;
        private bool isWriteOutstanding;
        private OverlappedContext writeOverlapped;
        private Exception asyncWriteException;
        private WaitCallback asyncWriteCallback;
        private object asyncWriteCallbackState;
        private int asyncBytesToWrite;
        private bool isShutdownWritten;
        private int syncWriteSize;
        private byte[] pendingWriteBuffer;
        private BufferManager pendingWriteBufferManager;
        private OverlappedIOCompleteCallback onAsyncWriteComplete;
        private int writeBufferSize;
        private TimeSpan readTimeout;
        private IOThreadTimer readTimer;
        private static Action<object> onReadTimeout;
        private string timeoutErrorString;
        private PipeConnection.TransferOperation timeoutErrorTransferOperation;
        private TimeSpan writeTimeout;
        private IOThreadTimer writeTimer;
        private static Action<object> onWriteTimeout;

        public PipeConnection(
          PipeHandle pipe,
          int connectionBufferSize,
          bool isBoundToCompletionPort,
          bool autoBindToCompletionPort)
        {
            if (pipe == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(pipe));
            if (pipe.IsInvalid)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(pipe));
            this.closeState = PipeConnection.CloseState.Open;
            this.exceptionEventType = TraceEventType.Error;
            this.isBoundToCompletionPort = isBoundToCompletionPort;
            this.autoBindToCompletionPort = autoBindToCompletionPort;
            this.pipe = pipe;
            this.readBufferSize = connectionBufferSize;
            this.writeBufferSize = connectionBufferSize;
            this.readOverlapped = new OverlappedContext();
            this.asyncReadBuffer = Fx.AllocateByteArray(connectionBufferSize); //DiagnosticUtility.Utility.AllocateByteArray(connectionBufferSize);

            this.writeOverlapped = new OverlappedContext();
            this.atEOFEvent = new ManualResetEvent(false);
            this.onAsyncReadComplete = new OverlappedIOCompleteCallback(this.OnAsyncReadComplete);
            this.onAsyncWriteComplete = new OverlappedIOCompleteCallback(this.OnAsyncWriteComplete);
        }

        public int AsyncReadBufferSize
        {
            get
            {
                return this.readBufferSize;
            }
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                return this.asyncReadBuffer;
            }
        }

        private static byte[] ZeroBuffer
        {
            get
            {
                if (PipeConnection.zeroBuffer == null)
                    PipeConnection.zeroBuffer = new byte[1];
                return PipeConnection.zeroBuffer;
            }
        }

        public EventLevel ExceptionEventType
        {
            get;
            set;
        }

        /*public TraceEventType ExceptionEventType
        {
            get
            {
                return this.exceptionEventType;
            }
            set
            {
                this.exceptionEventType = value;
            }
        } */

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
                return (IPEndPoint)null;
            }
        }

        private IOThreadTimer ReadTimer
        {
            get
            {
                if (this.readTimer == null)
                {
                    if (PipeConnection.onReadTimeout == null)
                        PipeConnection.onReadTimeout = new Action<object>(PipeConnection.OnReadTimeout);
                    this.readTimer = new IOThreadTimer(PipeConnection.onReadTimeout, (object)this, false);
                }
                return this.readTimer;
            }
        }

        private IOThreadTimer WriteTimer
        {
            get
            {
                if (this.writeTimer == null)
                {
                    if (PipeConnection.onWriteTimeout == null)
                        PipeConnection.onWriteTimeout = new Action<object>(PipeConnection.OnWriteTimeout);
                    this.writeTimer = new IOThreadTimer(PipeConnection.onWriteTimeout, (object)this, false);
                }
                return this.writeTimer;
            }
        }

        private static void OnReadTimeout(object state)
        {
            PipeConnection pipeConnection = (PipeConnection)state;
            pipeConnection.Abort(SR.Format("PipeConnectionAbortedReadTimedOut", (object)pipeConnection.readTimeout), PipeConnection.TransferOperation.Read);
        }

        private static void OnWriteTimeout(object state)
        {
            PipeConnection pipeConnection = (PipeConnection)state;
            pipeConnection.Abort(SR.Format("PipeConnectionAbortedWriteTimedOut", (object)pipeConnection.writeTimeout), PipeConnection.TransferOperation.Write);
        }

        public void Abort()
        {
            this.Abort((string)null, PipeConnection.TransferOperation.Undefined);
        }

        private void Abort(
          string timeoutErrorString,
          PipeConnection.TransferOperation transferOperation)
        {
            this.CloseHandle(true, timeoutErrorString, transferOperation);
        }

        private Exception ConvertPipeException(
          PipeException pipeException,
          PipeConnection.TransferOperation transferOperation)
        {
            return this.ConvertPipeException(pipeException.Message, pipeException, transferOperation);
        }

        private Exception ConvertPipeException(
          string exceptionMessage,
          PipeException pipeException,
          PipeConnection.TransferOperation transferOperation)
        {
            return this.timeoutErrorString != null ? (transferOperation == this.timeoutErrorTransferOperation ? (Exception)new TimeoutException(this.timeoutErrorString, (Exception)pipeException) : (Exception)new CommunicationException(this.timeoutErrorString, (Exception)pipeException)) : (this.aborted ? (Exception)new CommunicationObjectAbortedException(exceptionMessage, (Exception)pipeException) : (Exception)new CommunicationException(exceptionMessage, (Exception)pipeException));
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe AsyncCompletionResult BeginRead(
          int offset,
          int size,
          TimeSpan timeout,
          WaitCallback callback,
          object state)
        {
            ConnectionUtilities.ValidateBufferBounds(this.AsyncReadBuffer, offset, size);
            lock (this.readLock)
            {
                try
                {
                    this.ValidateEnterReadingState(true);
                    if (this.isAtEOF)
                    {
                        this.asyncBytesRead = 0;
                        this.asyncReadException = (Exception)null;
                        return AsyncCompletionResult.Completed;
                    }
                    if (this.autoBindToCompletionPort && !this.isBoundToCompletionPort)
                    {
                        lock (this.writeLock)
                            this.EnsureBoundToCompletionPort();
                    }
                    if (this.isReadOutstanding)
                        throw Fx.AssertAndThrow("Read I/O already pending when BeginRead called.");
                    try
                    {
                        this.readTimeout = timeout;
                        if (this.readTimeout != TimeSpan.MaxValue)
                            this.ReadTimer.Set(this.readTimeout);
                        this.asyncReadCallback = callback;
                        this.asyncReadCallbackState = state;
                        this.isReadOutstanding = true;
                        this.readOverlapped.StartAsyncOperation(this.AsyncReadBuffer, this.onAsyncReadComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                        {
                            int lastWin32Error = Marshal.GetLastWin32Error();
                            switch (lastWin32Error)
                            {
                                case 234:
                                case 997:
                                    break;
                                default:
                                    this.isReadOutstanding = false;
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateReadException(lastWin32Error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isReadOutstanding)
                        {
                            this.readOverlapped.CancelAsyncOperation();
                            this.asyncReadCallback = (WaitCallback)null;
                            this.asyncReadCallbackState = (object)null;
                            this.ReadTimer.Cancel();
                        }
                    }
                    if (!this.isReadOutstanding)
                    {
                        int bytesRead;
                        Exception overlappedReadException = (Exception)PipeConnection.Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out bytesRead);
                        if (overlappedReadException != null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(overlappedReadException);
                        this.asyncBytesRead = bytesRead;
                        this.HandleReadComplete(this.asyncBytesRead);
                    }
                    else
                        this.EnterReadingState();
                    return this.isReadOutstanding ? AsyncCompletionResult.Queued : AsyncCompletionResult.Completed;
                }
                catch (PipeException ex)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Read), this.ExceptionEventType);
                }
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public unsafe AsyncCompletionResult BeginWrite(
          byte[] buffer,
          int offset,
          int size,
          bool immediate,
          TimeSpan timeout,
          WaitCallback callback,
          object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.FinishPendingWrite(timeout);
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            if (this.autoBindToCompletionPort && !this.isBoundToCompletionPort)
            {
                lock (this.readLock)
                {
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        this.EnsureBoundToCompletionPort();
                    }
                }
            }
            lock (this.writeLock)
            {
                try
                {
                    this.ValidateEnterWritingState(true);
                    if (this.isWriteOutstanding)
                        throw Fx.AssertAndThrow("Write I/O already pending when BeginWrite called.");
                    try
                    {
                        this.writeTimeout = timeout;
                        this.WriteTimer.Set(timeoutHelper.RemainingTime());
                        this.asyncBytesToWrite = size;
                        this.asyncWriteException = (Exception)null;
                        this.asyncWriteCallback = callback;
                        this.asyncWriteCallbackState = state;
                        this.isWriteOutstanding = true;
                        this.writeOverlapped.StartAsyncOperation(buffer, this.onAsyncWriteComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                        {
                            int lastWin32Error = Marshal.GetLastWin32Error();
                            if (lastWin32Error != 997)
                            {
                                this.isWriteOutstanding = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateWriteException(lastWin32Error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isWriteOutstanding)
                        {
                            this.writeOverlapped.CancelAsyncOperation();
                            this.ResetWriteState();
                            this.WriteTimer.Cancel();
                        }
                    }
                    if (!this.isWriteOutstanding)
                    {
                        int bytesWritten;
                        Exception exception = (Exception)PipeConnection.Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out bytesWritten);
                        if (exception == null && bytesWritten != size)
                            exception = (Exception)new PipeException(SR.Format("PipeWriteIncomplete"));
                        if (exception != null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    else
                        this.EnterWritingState();
                    return this.isWriteOutstanding ? AsyncCompletionResult.Queued : AsyncCompletionResult.Completed;
                }
                catch (PipeException ex)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Write), this.ExceptionEventType);
                }
            }
        }

        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            System.Runtime.TimeoutHelper timeoutHelper = new System.Runtime.TimeoutHelper(timeout);
            this.FinishPendingWrite(timeout);
            bool flag1 = false;
            try
            {
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                lock (this.readLock)
                {
                    lock (this.writeLock)
                    {
                        if (!this.isShutdownWritten && this.inWritingState)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeCantCloseWithPendingWrite")), this.ExceptionEventType);
                        if (this.closeState == PipeConnection.CloseState.Closing || this.closeState == PipeConnection.CloseState.HandleClosed)
                            return;
                        this.closeState = PipeConnection.CloseState.Closing;
                        flag1 = true;
                        if (!this.isAtEOF)
                        {
                            if (this.inReadingState)
                                flag2 = true;
                            else
                                flag3 = true;
                        }
                        if (!this.isShutdownWritten)
                        {
                            flag4 = true;
                            this.isShutdownWritten = true;
                        }
                    }
                }
                if (flag4)
                    this.StartWriteZero(timeoutHelper.RemainingTime());
                if (flag3)
                    this.StartReadZero();
                try
                {
                    this.WaitForWriteZero(timeoutHelper.RemainingTime(), true);
                }
                catch (TimeoutException ex)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new TimeoutException(SR.Format("PipeShutdownWriteError"), (Exception)ex), this.ExceptionEventType);
                }
                if (flag3)
                {
                    try
                    {
                        this.WaitForReadZero(timeoutHelper.RemainingTime(), true);
                    }
                    catch (TimeoutException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new TimeoutException(SR.Format("PipeShutdownReadError"), (Exception)ex), this.ExceptionEventType);
                    }
                }
                else if (flag2 && !System.Runtime.TimeoutHelper.WaitOne((WaitHandle)this.atEOFEvent, timeoutHelper.RemainingTime()))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new TimeoutException(SR.Format("PipeShutdownReadError")), this.ExceptionEventType);
                try
                {
                    this.StartWriteZero(timeoutHelper.RemainingTime());
                    this.StartReadZero();
                    this.WaitForWriteZero(timeoutHelper.RemainingTime(), false);
                    this.WaitForReadZero(timeoutHelper.RemainingTime(), false);
                }
                catch (PipeException ex)
                {
                    if (!this.IsBrokenPipeError(ex.ErrorCode))
                        throw;
                    else
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                }
                catch (CommunicationException ex)
                {
                    DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                }
                catch (TimeoutException ex)
                {
                    DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                }
            }
            catch (TimeoutException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new TimeoutException(SR.Format("PipeCloseFailed"), (Exception)ex), this.ExceptionEventType);
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(SR.Format("PipeCloseFailed"), ex, PipeConnection.TransferOperation.Undefined), this.ExceptionEventType);
            }
            finally
            {
                if (flag1)
                    this.CloseHandle(false, (string)null, PipeConnection.TransferOperation.Undefined);
            }
        }

        private void CloseHandle(
          bool abort,
          string timeoutErrorString,
          PipeConnection.TransferOperation transferOperation)
        {
            lock (this.readLock)
            {
                lock (this.writeLock)
                {
                    if (this.closeState == PipeConnection.CloseState.HandleClosed)
                        return;
                    this.timeoutErrorString = timeoutErrorString;
                    this.timeoutErrorTransferOperation = transferOperation;
                    this.aborted = abort;
                    this.closeState = PipeConnection.CloseState.HandleClosed;
                    this.pipe.Close();
                    this.readOverlapped.FreeOrDefer();
                    this.writeOverlapped.FreeOrDefer();
                    if (this.atEOFEvent != null)
                        this.atEOFEvent.Close();
                    try
                    {
                        this.FinishPendingWrite(TimeSpan.Zero);
                    }
                    catch (TimeoutException ex)
                    {
                       // if (System.ServiceModel.Diagnostics.Application.TD.CloseTimeoutIsEnabled())
                       //     System.ServiceModel.Diagnostics.Application.TD.CloseTimeout(ex.Message);
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                    }
                    catch (CommunicationException ex)
                    {
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                    }
                }
            }
            if (!abort)
                return;
          /*  TraceEventType traceEventType = TraceEventType.Warning;
            if (this.ExceptionEventType == TraceEventType.Information)
                traceEventType = this.ExceptionEventType;
            if (!DiagnosticUtility.ShouldTrace(traceEventType))
                return;
            TraceUtility.TraceEvent(traceEventType, 262173, SR.Format("TraceCodePipeConnectionAbort"), (object)this); */
          return;
        }

        private CommunicationException CreatePipeDuplicationFailedException(
          int win32Error)
        {
            Exception innerException = (Exception)new PipeException(SR.Format("PipeDuplicationFailed"), win32Error);
            return new CommunicationException(innerException.Message, innerException);
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            System.ServiceModel.Activation.SafeCloseHandle hTargetProcessHandle = ListenerUnsafeNativeMethods.OpenProcess(64, false, targetProcessId);
            if (hTargetProcessHandle.IsInvalid)
            {
                hTargetProcessHandle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
            }
            try
            {
                IntPtr currentProcess = ListenerUnsafeNativeMethods.GetCurrentProcess();
                if (currentProcess == IntPtr.Zero)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
                IntPtr lpTargetHandle;
                if (!UnsafeNativeMethods.DuplicateHandle(currentProcess, this.pipe, hTargetProcessHandle, out lpTargetHandle, 0, false, 2))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
                this.Abort();
                return (object)lpTargetHandle;
            }
            finally
            {
                hTargetProcessHandle.Close();
            }
        }

        public object GetCoreTransport()
        {
            return (object)this.pipe;
        }

        private void EnsureBoundToCompletionPort()
        {
            if (this.isBoundToCompletionPort)
                return;
            ThreadPool.BindHandle((SafeHandle)this.pipe);
            this.isBoundToCompletionPort = true;
        }

        public int EndRead()
        {
            if (this.asyncReadException != null)
            {
                Exception asyncReadException = this.asyncReadException;
                this.asyncReadException = (Exception)null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(asyncReadException, this.ExceptionEventType);
            }
            return this.asyncBytesRead;
        }

        public void EndWrite()
        {
            if (this.asyncWriteException != null)
            {
                Exception asyncWriteException = this.asyncWriteException;
                this.asyncWriteException = (Exception)null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(asyncWriteException, this.ExceptionEventType);
            }
        }

        private void EnterReadingState()
        {
            this.inReadingState = true;
        }

        private void EnterWritingState()
        {
            this.inWritingState = true;
        }

        private void ExitReadingState()
        {
            this.inReadingState = false;
        }

        private void ExitWritingState()
        {
            this.inWritingState = false;
        }

        private void ReadIOCompleted()
        {
            this.readOverlapped.FreeIfDeferred();
        }

        private void WriteIOCompleted()
        {
            this.writeOverlapped.FreeIfDeferred();
        }

        private void FinishPendingWrite(TimeSpan timeout)
        {
            if (this.pendingWriteBuffer == null)
                return;
            byte[] pendingWriteBuffer;
            BufferManager writeBufferManager;
            lock (this.writeLock)
            {
                if (this.pendingWriteBuffer == null)
                    return;
                pendingWriteBuffer = this.pendingWriteBuffer;
                this.pendingWriteBuffer = (byte[])null;
                writeBufferManager = this.pendingWriteBufferManager;
                this.pendingWriteBufferManager = (BufferManager)null;
            }
            try
            {
                bool flag = false;
                try
                {
                    this.WaitForSyncWrite(timeout, true);
                    flag = true;
                }
                finally
                {
                    lock (this.writeLock)
                    {
                        try
                        {
                            if (flag)
                                this.FinishSyncWrite(true);
                        }
                        finally
                        {
                            this.ExitWritingState();
                            if (!this.isWriteOutstanding)
                            {
                                writeBufferManager.ReturnBuffer(pendingWriteBuffer);
                                this.WriteIOCompleted();
                            }
                        }
                    }
                }
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Write), this.ExceptionEventType);
            }
        }

        private void HandleReadComplete(int bytesRead)
        {
            if (bytesRead != 0)
                return;
            this.isAtEOF = true;
            this.atEOFEvent.Set();
        }

        private bool IsBrokenPipeError(int error)
        {
            return error == 232 || error == 109;
        }

        private Exception CreatePipeClosedException(
          PipeConnection.TransferOperation transferOperation)
        {
            return this.ConvertPipeException(new PipeException(SR.Format("PipeClosed")), transferOperation);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void OnAsyncReadComplete(bool haveResult, int error, int numBytes)
        {
            WaitCallback asyncReadCallback;
            object readCallbackState;
            lock (this.readLock)
            {
                try
                {
                    try
                    {
                        if (this.readTimeout != TimeSpan.MaxValue && !this.ReadTimer.Cancel())
                            this.Abort(SR.Format("PipeConnectionAbortedReadTimedOut", (object)this.readTimeout), PipeConnection.TransferOperation.Read);
                        if (this.closeState == PipeConnection.CloseState.HandleClosed)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeClosedException(PipeConnection.TransferOperation.Read));
                        if (!haveResult)
                            error = UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.readOverlapped.NativeOverlapped, out numBytes, 0) != 0 ? 0 : Marshal.GetLastWin32Error();
                        if (error != 0 && error != 234)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateReadException(error));
                        this.asyncBytesRead = numBytes;
                        this.HandleReadComplete(this.asyncBytesRead);
                    }
                    catch (PipeException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Read));
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                    else
                        this.asyncReadException = ex;
                }
                finally
                {
                    this.isReadOutstanding = false;
                    this.ReadIOCompleted();
                    this.ExitReadingState();
                    asyncReadCallback = this.asyncReadCallback;
                    this.asyncReadCallback = (WaitCallback)null;
                    readCallbackState = this.asyncReadCallbackState;
                    this.asyncReadCallbackState = (object)null;
                }
            }
            asyncReadCallback(readCallbackState);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void OnAsyncWriteComplete(bool haveResult, int error, int numBytes)
        {
            Exception exception = (Exception)null;
            this.WriteTimer.Cancel();
            WaitCallback asyncWriteCallback;
            object writeCallbackState;
            lock (this.writeLock)
            {
                try
                {
                    try
                    {
                        if (this.closeState == PipeConnection.CloseState.HandleClosed)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeClosedException(PipeConnection.TransferOperation.Write));
                        if (!haveResult)
                            error = UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.writeOverlapped.NativeOverlapped, out numBytes, 0) != 0 ? 0 : Marshal.GetLastWin32Error();
                        if (error != 0)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateWriteException(error));
                        if (numBytes != this.asyncBytesToWrite)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new PipeException(SR.Format("PipeWriteIncomplete")));
                    }
                    catch (PipeException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Write), this.ExceptionEventType);
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                    else
                        exception = ex;
                }
                finally
                {
                    this.isWriteOutstanding = false;
                    this.WriteIOCompleted();
                    this.ExitWritingState();
                    this.asyncWriteException = exception;
                    asyncWriteCallback = this.asyncWriteCallback;
                    writeCallbackState = this.asyncWriteCallbackState;
                    this.ResetWriteState();
                }
            }
            if (asyncWriteCallback == null)
                return;
            asyncWriteCallback(writeCallbackState);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            try
            {
                lock (this.readLock)
                {
                    this.ValidateEnterReadingState(true);
                    if (this.isAtEOF)
                        return 0;
                    this.StartSyncRead(buffer, offset, size);
                    this.EnterReadingState();
                }
                int bytesRead = -1;
                bool flag = false;
                try
                {
                    this.WaitForSyncRead(timeout, true);
                    flag = true;
                }
                finally
                {
                    lock (this.readLock)
                    {
                        try
                        {
                            if (flag)
                            {
                                bytesRead = this.FinishSyncRead(true);
                                this.HandleReadComplete(bytesRead);
                            }
                        }
                        finally
                        {
                            this.ExitReadingState();
                            if (!this.isReadOutstanding)
                                this.ReadIOCompleted();
                        }
                    }
                }
                return bytesRead;
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Read), this.ExceptionEventType);
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            try
            {
                System.Runtime.TimeoutHelper timeoutHelper = new System.Runtime.TimeoutHelper(timeout);
                this.FinishPendingWrite(timeoutHelper.RemainingTime());
                lock (this.writeLock)
                {
                    this.ValidateEnterWritingState(true);
                    this.StartWriteZero(timeoutHelper.RemainingTime());
                    this.isShutdownWritten = true;
                }
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Undefined), this.ExceptionEventType);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private void StartReadZero()
        {
            lock (this.readLock)
            {
                this.ValidateEnterReadingState(false);
                this.StartSyncRead(PipeConnection.ZeroBuffer, 0, 1);
                this.EnterReadingState();
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private void StartWriteZero(TimeSpan timeout)
        {
            this.FinishPendingWrite(timeout);
            lock (this.writeLock)
            {
                this.ValidateEnterWritingState(false);
                this.StartSyncWrite(PipeConnection.ZeroBuffer, 0, 0);
                this.EnterWritingState();
            }
        }

        private void ResetWriteState()
        {
            this.asyncBytesToWrite = -1;
            this.asyncWriteCallback = (WaitCallback)null;
            this.asyncWriteCallbackState = (object)null;
        }

        public IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return (IAsyncResult)new CompletedAsyncResult<bool>(true, callback, state);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        private void WaitForReadZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool flag = false;
            try
            {
                this.WaitForSyncRead(timeout, traceExceptionsAsErrors);
                flag = true;
            }
            finally
            {
                lock (this.readLock)
                {
                    try
                    {
                        if (flag)
                        {
                            if (this.FinishSyncRead(traceExceptionsAsErrors) != 0)
                            {
                                Exception exception = this.ConvertPipeException(new PipeException(SR.Format("PipeSignalExpected")), PipeConnection.TransferOperation.Read);
                                EventLevel eventType = EventLevel.Informational;
                                if (traceExceptionsAsErrors)
                                    eventType = EventLevel.Error;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, eventType);
                            }
                        }
                    }
                    finally
                    {
                        this.ExitReadingState();
                        if (!this.isReadOutstanding)
                            this.ReadIOCompleted();
                    }
                }
            }
        }

        private void WaitForWriteZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool flag = false;
            try
            {
                this.WaitForSyncWrite(timeout, traceExceptionsAsErrors);
                flag = true;
            }
            finally
            {
                lock (this.writeLock)
                {
                    try
                    {
                        if (flag)
                            this.FinishSyncWrite(traceExceptionsAsErrors);
                    }
                    finally
                    {
                        this.ExitWritingState();
                        if (!this.isWriteOutstanding)
                            this.WriteIOCompleted();
                    }
                }
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            this.WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private void WriteHelper(
          byte[] buffer,
          int offset,
          int size,
          bool immediate,
          TimeSpan timeout,
          ref object holder)
        {
            try
            {
                this.FinishPendingWrite(timeout);
                ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
                int num = size;
                if (size > this.writeBufferSize)
                    size = this.writeBufferSize;
                while (num > 0)
                {
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        this.StartSyncWrite(buffer, offset, size, ref holder);
                        this.EnterWritingState();
                    }
                    bool flag = false;
                    try
                    {
                        this.WaitForSyncWrite(timeout, true, ref holder);
                        flag = true;
                    }
                    finally
                    {
                        lock (this.writeLock)
                        {
                            try
                            {
                                if (flag)
                                    this.FinishSyncWrite(true);
                            }
                            finally
                            {
                                this.ExitWritingState();
                                if (!this.isWriteOutstanding)
                                    this.WriteIOCompleted();
                            }
                        }
                    }
                    num -= size;
                    offset += size;
                    if (size > num)
                        size = num;
                }
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Write), this.ExceptionEventType);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public void Write(
          byte[] buffer,
          int offset,
          int size,
          bool immediate,
          TimeSpan timeout,
          BufferManager bufferManager)
        {
            bool flag1 = true;
            try
            {
                if (size > this.writeBufferSize)
                {
                    this.WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
                }
                else
                {
                    this.FinishPendingWrite(timeout);
                    ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        bool flag2 = false;
                        try
                        {
                            flag1 = false;
                            this.StartSyncWrite(buffer, offset, size);
                            flag2 = true;
                        }
                        finally
                        {
                            if (!this.isWriteOutstanding)
                                flag1 = true;
                            else if (flag2)
                            {
                                this.EnterWritingState();
                                this.pendingWriteBuffer = buffer;
                                this.pendingWriteBufferManager = bufferManager;
                            }
                        }
                    }
                }
            }
            catch (PipeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(ex, PipeConnection.TransferOperation.Write), this.ExceptionEventType);
            }
            finally
            {
                if (flag1)
                    bufferManager.ReturnBuffer(buffer);
            }
        }

        private void ValidateEnterReadingState(bool checkEOF)
        {
            if (checkEOF && this.closeState == PipeConnection.CloseState.Closing)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeAlreadyClosing")), this.ExceptionEventType);
            if (this.inReadingState)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeReadPending")), this.ExceptionEventType);
            if (this.closeState == PipeConnection.CloseState.HandleClosed)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeClosed")), this.ExceptionEventType);
        }

        private void ValidateEnterWritingState(bool checkShutdown)
        {
            if (checkShutdown)
            {
                if (this.isShutdownWritten)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeAlreadyShuttingDown")), this.ExceptionEventType);
                if (this.closeState == PipeConnection.CloseState.Closing)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeAlreadyClosing")), this.ExceptionEventType);
            }
            if (this.inWritingState)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeWritePending")), this.ExceptionEventType);
            if (this.closeState == PipeConnection.CloseState.HandleClosed)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper((Exception)new PipeException(SR.Format("PipeClosed")), this.ExceptionEventType);
        }

        private void StartSyncRead(byte[] buffer, int offset, int size)
        {
            this.StartSyncRead(buffer, offset, size, ref this.readOverlapped.Holder[0]);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void StartSyncRead(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isReadOutstanding)
                throw Fx.AssertAndThrow("StartSyncRead called when read I/O was already pending.");
            try
            {
                this.isReadOutstanding = true;
                this.readOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 997)
                        return;
                    this.isReadOutstanding = false;
                    if (lastWin32Error != 234)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateReadException(lastWin32Error));
                }
                else
                    this.isReadOutstanding = false;
            }
            finally
            {
                if (!this.isReadOutstanding)
                    this.readOverlapped.CancelSyncOperation(ref holder);
            }
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private void WaitForSyncRead(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            if (!this.isReadOutstanding)
                return;
            if (!this.readOverlapped.WaitForSyncOperation(timeout))
            {
                this.Abort(SR.Format("PipeConnectionAbortedReadTimedOut", (object)this.readTimeout), PipeConnection.TransferOperation.Read);
                Exception exception = (Exception)new TimeoutException(SR.Format("PipeReadTimedOut", (object)timeout));
                EventLevel eventType = EventLevel.Informational;
                if (traceExceptionsAsErrors)
                    eventType = EventLevel.Error;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, eventType);
            }
            this.isReadOutstanding = false;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe int FinishSyncRead(bool traceExceptionsAsErrors)
        {
            int bytesRead = -1;
            Exception exception = this.closeState != PipeConnection.CloseState.HandleClosed ? (Exception)PipeConnection.Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out bytesRead) : this.CreatePipeClosedException(PipeConnection.TransferOperation.Read);
            if (exception != null)
            {
                EventLevel eventType = EventLevel.Informational;
                if (traceExceptionsAsErrors)
                    eventType = EventLevel.Error;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, eventType);
            }
            return bytesRead;
        }

        private void StartSyncWrite(byte[] buffer, int offset, int size)
        {
            this.StartSyncWrite(buffer, offset, size, ref this.writeOverlapped.Holder[0]);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void StartSyncWrite(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isWriteOutstanding)
                throw Fx.AssertAndThrow("StartSyncWrite called when write I/O was already pending.");
            try
            {
                this.syncWriteSize = size;
                this.isWriteOutstanding = true;
                this.writeOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error != 997)
                    {
                        this.isWriteOutstanding = false;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)PipeConnection.Exceptions.CreateWriteException(lastWin32Error));
                    }
                }
                else
                    this.isWriteOutstanding = false;
            }
            finally
            {
                if (!this.isWriteOutstanding)
                    this.writeOverlapped.CancelSyncOperation(ref holder);
            }
        }

        private void WaitForSyncWrite(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            this.WaitForSyncWrite(timeout, traceExceptionsAsErrors, ref this.writeOverlapped.Holder[0]);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private void WaitForSyncWrite(
          TimeSpan timeout,
          bool traceExceptionsAsErrors,
          ref object holder)
        {
            if (!this.isWriteOutstanding)
                return;
            if (!this.writeOverlapped.WaitForSyncOperation(timeout, ref holder))
            {
                this.Abort(SR.Format("PipeConnectionAbortedWriteTimedOut", (object)this.writeTimeout), PipeConnection.TransferOperation.Write);
                Exception exception = (Exception)new TimeoutException(SR.Format("PipeWriteTimedOut", (object)timeout));
                EventLevel eventType = EventLevel.Informational;
                if (traceExceptionsAsErrors)
                    eventType = EventLevel.Error;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, eventType);
            }
            this.isWriteOutstanding = false;
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void FinishSyncWrite(bool traceExceptionsAsErrors)
        {
            Exception exception;
            if (this.closeState == PipeConnection.CloseState.HandleClosed)
            {
                exception = this.CreatePipeClosedException(PipeConnection.TransferOperation.Write);
            }
            else
            {
                int bytesWritten;
                exception = (Exception)PipeConnection.Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out bytesWritten);
                if (exception == null && bytesWritten != this.syncWriteSize)
                    exception = (Exception)new PipeException(SR.Format("PipeWriteIncomplete"));
            }
            if (exception != null)
            {
                EventLevel eventType = EventLevel.Informational;
                if (traceExceptionsAsErrors)
                    eventType = EventLevel.Error;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, eventType);
            }
        }

        public AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, Action<object> callback, object state)
        {
            //throw new NotImplementedException();
            return BeginWrite(buffer, offset, size, immediate, timeout, new WaitCallback(callback), state);
        }

        public AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout, Action<object> callback, object state)
        {
            return BeginRead(offset, size, timeout, new WaitCallback(callback), state);
        }

        private enum CloseState
        {
            Open,
            Closing,
            HandleClosed,
        }

        private enum TransferOperation
        {
            Write,
            Read,
            Undefined,
        }

        private static class Exceptions
        {
            private static PipeException CreateException(string resourceString, int error)
            {
                return new PipeException(SR.Format(resourceString, (object)PipeError.GetErrorString(error)), error);
            }

            public static PipeException CreateReadException(int error)
            {
                return PipeConnection.Exceptions.CreateException("PipeReadError", error);
            }

            public static PipeException CreateWriteException(int error)
            {
                return PipeConnection.Exceptions.CreateException("PipeWriteError", error);
            }

            [SecuritySafeCritical]
            [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            public static unsafe PipeException GetOverlappedWriteException(
              PipeHandle pipe,
              NativeOverlapped* nativeOverlapped,
              out int bytesWritten)
            {
                return UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesWritten, 0) == 0 ? PipeConnection.Exceptions.CreateWriteException(Marshal.GetLastWin32Error()) : (PipeException)null;
            }

            [SecuritySafeCritical]
            [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            public static unsafe PipeException GetOverlappedReadException(
              PipeHandle pipe,
              NativeOverlapped* nativeOverlapped,
              out int bytesRead)
            {
                if (UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesRead, 0) != 0)
                    return (PipeException)null;
                int lastWin32Error = Marshal.GetLastWin32Error();
                return lastWin32Error == 234 ? (PipeException)null : PipeConnection.Exceptions.CreateReadException(lastWin32Error);
            }
        }
    }
}

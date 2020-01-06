using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using System.ServiceModel.Diagnostics;
using System.Threading.Tasks;

namespace System.ServiceModel.Channels
{
    internal class PipeConnectionInitiator : IConnectionInitiator
    {
        private int bufferSize;
        private IPipeTransportFactorySettings pipeSettings;

        public PipeConnectionInitiator(int bufferSize, IPipeTransportFactorySettings pipeSettings)
        {
            this.bufferSize = bufferSize;
            this.pipeSettings = pipeSettings;
        }

        private Exception CreateConnectFailedException(
            Uri remoteUri,
            PipeException innerException)
        {
            return (Exception)new CommunicationException(SR.Format("PipeConnectFailed", (object)remoteUri.AbsoluteUri), (Exception)innerException);
        }

        public IConnection Connect(Uri remoteUri, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            string resolvedAddress;
            BackoffTimeoutHelper backoffHelper;
            this.PrepareConnect(remoteUri, timeoutHelper.RemainingTime(), out resolvedAddress, out backoffHelper);
            IConnection connection = (IConnection)null;
            while (connection == null)
            {
                connection = this.TryConnect(remoteUri, resolvedAddress, backoffHelper);
                if (connection == null)
                {
                    backoffHelper.WaitAndBackoff();
                    // TODO: resolve tracing ...
                     if (DiagnosticUtility.ShouldTraceInformation)
                         TraceUtility.TraceEvent(TraceEventType.Information, 262193, SR.Format("TraceCodeFailedPipeConnect", (object)timeoutHelper.RemainingTime(), (object)remoteUri));
                }
            }
            return connection;
        }

        public Task<IConnection> ConnectAsync(Uri uri, TimeSpan timeout)
        {
           // throw new NotImplementedException();
           var connection = Connect(uri, timeout);
           return Task.FromResult(connection);
        }

        internal static string GetPipeName(
            Uri uri,
            IPipeTransportFactorySettings transportFactorySettings)
        {
            AppContainerInfo appContainerInfo = PipeConnectionInitiator.GetAppContainerInfo(transportFactorySettings);
            string[] strArray = new string[3]
            {
                "+",
                uri.Host,
                "*"
            };
            bool[] flagArray = new bool[2] { true, false };
            string str1 = string.Empty;
            string str2 = (string)null;
            for (int index1 = 0; index1 < strArray.Length; ++index1)
            {
                for (int index2 = 0; index2 < flagArray.Length; ++index2)
                {
                    if (appContainerInfo == null || !flagArray[index2])
                    {
                        for (string path = PipeUri.GetPath(uri); path.Length > 0; path = PipeUri.GetParentPath(path))
                        {
                            string sharedMemoryName = PipeUri.BuildSharedMemoryName(strArray[index1], path, flagArray[index2], appContainerInfo);
                            try
                            {
                                PipeSharedMemory pipeSharedMemory = PipeSharedMemory.Open(sharedMemoryName, uri);
                                if (pipeSharedMemory != null)
                                {
                                    try
                                    {
                                        string pipeName = pipeSharedMemory.GetPipeName(appContainerInfo);
                                        if (pipeName != null)
                                        {
                                            if (!ServiceModelAppSettings.UseBestMatchNamedPipeUri)
                                                return pipeName;
                                            if (path.Length > str1.Length)
                                            {
                                                str1 = path;
                                                str2 = pipeName;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        pipeSharedMemory.Dispose();
                                    }
                                }
                            }
                            catch (AddressAccessDeniedException ex)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new EndpointNotFoundException(SR.Format("EndpointNotFound", (object)uri.AbsoluteUri), (Exception)ex));
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(str2))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new EndpointNotFoundException(SR.Format("EndpointNotFound", (object)uri.AbsoluteUri), (Exception)new PipeException(SR.Format("PipeEndpointNotFound", (object)uri.AbsoluteUri))));
            return str2;
        }

        public IAsyncResult BeginConnect(
            Uri uri,
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            return (IAsyncResult)new PipeConnectionInitiator.ConnectAsyncResult(this, uri, timeout, callback, state);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            return PipeConnectionInitiator.ConnectAsyncResult.End(result);
        }

        private void PrepareConnect(
            Uri remoteUri,
            TimeSpan timeout,
            out string resolvedAddress,
            out BackoffTimeoutHelper backoffHelper)
        {
            PipeUri.Validate(remoteUri);
            // if (DiagnosticUtility.ShouldTraceInformation)
            //     TraceUtility.TraceEvent(TraceEventType.Information, 262186, SR.Format("TraceCodeInitiatingNamedPipeConnection"), (TraceRecord)new StringTraceRecord("Uri", remoteUri.ToString()), (object)this, (Exception)null);
            resolvedAddress = PipeConnectionInitiator.GetPipeName(remoteUri, this.pipeSettings);
            TimeSpan timeout1 = !(timeout >= TimeSpan.FromMilliseconds(300.0)) ? Ticks.ToTimeSpan(Ticks.FromMilliseconds(150) / 2L + 1L) : TimeoutHelper.Add(timeout, TimeSpan.Zero - TimeSpan.FromMilliseconds(150.0));
            backoffHelper = new BackoffTimeoutHelper(timeout1, TimeSpan.FromMinutes(5.0));
        }

        private IConnection TryConnect(
            Uri remoteUri,
            string resolvedAddress,
            BackoffTimeoutHelper backoffHelper)
        {
            bool flag = backoffHelper.IsExpired();
            int dwFlagsAndAttributes = 1073741824 | 1048576;
            PipeHandle file = UnsafeNativeMethods.CreateFile(resolvedAddress, -1073741824, 0, IntPtr.Zero, 3, dwFlagsAndAttributes, IntPtr.Zero);
            int lastWin32Error1 = Marshal.GetLastWin32Error();
            if (file.IsInvalid)
            {
                file.SetHandleAsInvalid();
                if (lastWin32Error1 == 2 || lastWin32Error1 == 231)
                {
                    if (flag)
                    {
                        Exception innerException = (Exception)new PipeException(SR.Format("PipeConnectAddressFailed", (object)resolvedAddress, (object)PipeError.GetErrorString(lastWin32Error1)), lastWin32Error1);
                        string absoluteUri = remoteUri.AbsoluteUri;
                        TimeoutException timeoutException;
                        if (lastWin32Error1 == 231)
                            timeoutException = new TimeoutException(SR.Format("PipeConnectTimedOutServerTooBusy", (object)absoluteUri, (object)backoffHelper.OriginalTimeout), innerException);
                        else
                            timeoutException = new TimeoutException(SR.Format("PipeConnectTimedOut", (object)absoluteUri, (object)backoffHelper.OriginalTimeout), innerException);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)timeoutException);
                    }
                    return (IConnection)null;
                }
                PipeException innerException1 = new PipeException(SR.Format("PipeConnectAddressFailed", (object)resolvedAddress, (object)PipeError.GetErrorString(lastWin32Error1)), lastWin32Error1);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateConnectFailedException(remoteUri, innerException1));
            }
            int mode = 2;
            if (UnsafeNativeMethods.SetNamedPipeHandleState(file, ref mode, IntPtr.Zero, IntPtr.Zero) == 0)
            {
                int lastWin32Error2 = Marshal.GetLastWin32Error();
                file.Close();
                PipeException innerException = new PipeException(SR.Format("PipeModeChangeFailed", (object)PipeError.GetErrorString(lastWin32Error2)), lastWin32Error2);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateConnectFailedException(remoteUri, innerException));
            }
            return (IConnection)new PipeConnection(file, this.bufferSize, false, true);
        }

        private static AppContainerInfo GetAppContainerInfo(
            IPipeTransportFactorySettings transportFactorySettings)
        {
            if (AppContainerInfo.IsAppContainerSupported && transportFactorySettings != null && transportFactorySettings.PipeSettings != null)
            {
                ApplicationContainerSettings containerSettings = transportFactorySettings.PipeSettings.ApplicationContainerSettings;
                if (containerSettings != null && containerSettings.TargetingAppContainer)
                    return AppContainerInfo.CreateAppContainerInfo(containerSettings.PackageFullName, containerSettings.SessionId);
            }
            return (AppContainerInfo)null;
        }



        private class ConnectAsyncResult : AsyncResult
        {
            private PipeConnectionInitiator parent;
            private Uri remoteUri;
            private string resolvedAddress;
            private BackoffTimeoutHelper backoffHelper;
            private TimeoutHelper timeoutHelper;
            private IConnection connection;
            private static Action<object> waitCompleteCallback;

            public ConnectAsyncResult(
                PipeConnectionInitiator parent,
                Uri remoteUri,
                TimeSpan timeout,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.remoteUri = remoteUri;
                this.timeoutHelper = new TimeoutHelper(timeout);
                parent.PrepareConnect(remoteUri, this.timeoutHelper.RemainingTime(), out this.resolvedAddress, out this.backoffHelper);
                if (!this.ConnectAndWait())
                    return;
                this.Complete(true);
            }

            private bool ConnectAndWait()
            {
                this.connection = this.parent.TryConnect(this.remoteUri, this.resolvedAddress, this.backoffHelper);
                bool flag = this.connection != null;
                if (!flag)
                {
                    if (PipeConnectionInitiator.ConnectAsyncResult.waitCompleteCallback == null)
                        PipeConnectionInitiator.ConnectAsyncResult.waitCompleteCallback = new Action<object>(PipeConnectionInitiator.ConnectAsyncResult.OnWaitComplete);
                    this.backoffHelper.WaitAndBackoff(PipeConnectionInitiator.ConnectAsyncResult.waitCompleteCallback, (object)this);
                }
                return flag;
            }

            public static IConnection End(IAsyncResult result)
            {
                return AsyncResult.End<PipeConnectionInitiator.ConnectAsyncResult>(result).connection;
            }

            private static void OnWaitComplete(object state)
            {
                Exception exception = (Exception)null;
                PipeConnectionInitiator.ConnectAsyncResult connectAsyncResult = (PipeConnectionInitiator.ConnectAsyncResult)state;
                bool flag = true;
                try
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                        TraceUtility.TraceEvent(TraceEventType.Information, 262193, SR.Format("TraceCodeFailedPipeConnect", (object)connectAsyncResult.timeoutHelper.RemainingTime(), (object)connectAsyncResult.remoteUri));
                    flag = connectAsyncResult.ConnectAndWait();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                    else
                        exception = ex;
                }
                if (!flag)
                    return;
                connectAsyncResult.Complete(false, exception);
            }
        }
    }
}

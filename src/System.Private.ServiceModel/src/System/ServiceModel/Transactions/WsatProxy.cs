using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Transactions;
using Microsoft.Transactions.Wsat.Messaging;

namespace System.ServiceModel.Transactions
{

    internal class WsatProxy
    {
        private object proxyLock = new object();
        private WsatConfiguration wsatConfig;
        private ProtocolVersion protocolVersion;
        private CoordinationService coordinationService;
        private ActivationProxy activationProxy;
        private static byte[] fixedPropagationToken;

        public WsatProxy(WsatConfiguration wsatConfig, ProtocolVersion protocolVersion)
        {
            this.wsatConfig = wsatConfig;
            this.protocolVersion = protocolVersion;
        }

        public Transaction UnmarshalTransaction(WsatTransactionInfo info)
        {
            if (info.Context.ProtocolVersion != this.protocolVersion)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
            if (this.wsatConfig.OleTxUpgradeEnabled)
            {
                byte[] propagationToken = info.Context.PropagationToken;
                if (propagationToken != null)
                {
                    try
                    {
                        return OleTxTransactionInfo.UnmarshalPropagationToken(propagationToken);
                    }
                    catch (TransactionException ex)
                    {
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Warning);
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                        TraceUtility.TraceEvent(TraceEventType.Information, 917518, System.ServiceModel.SR.GetString("TraceCodeTxFailedToNegotiateOleTx", (object)info.Context.Identifier));
                }
            }
            CoordinationContext coordinationContext = info.Context;
            if (!this.wsatConfig.IsLocalRegistrationService(coordinationContext.RegistrationService, this.protocolVersion))
            {
                if (!this.wsatConfig.IsProtocolServiceEnabled(this.protocolVersion))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("WsatProtocolServiceDisabled", (object)this.protocolVersion)));
                if (!this.wsatConfig.InboundEnabled)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("InboundTransactionsDisabled")));
                if (this.wsatConfig.IsDisabledRegistrationService(coordinationContext.RegistrationService))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("SourceTransactionsDisabled")));
                coordinationContext = this.CreateCoordinationContext(info);
            }
            Guid localTransactionId = coordinationContext.LocalTransactionId;
            if (localTransactionId == Guid.Empty)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("InvalidCoordinationContextTransactionId")));
            return OleTxTransactionInfo.UnmarshalPropagationToken(WsatProxy.MarshalPropagationToken(ref localTransactionId, coordinationContext.IsolationLevel, coordinationContext.IsolationFlags, coordinationContext.Description));
        }

        private CoordinationContext CreateCoordinationContext(
            WsatTransactionInfo info)
        {
            CreateCoordinationContext cccMessage = new CreateCoordinationContext(this.protocolVersion);
            cccMessage.CurrentContext = info.Context;
            cccMessage.IssuedToken = info.IssuedToken;
            try
            {
                using (new OperationContextScope((OperationContext)null))
                    return this.Enlist(ref cccMessage).CoordinationContext;
            }
            catch (WsatFaultException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("UnmarshalTransactionFaulted", (object)ex.Message), (Exception)ex));
            }
            catch (WsatSendFailureException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerCommunicationException(System.ServiceModel.SR.GetString("TMCommunicationError"), (Exception)ex));
            }
        }

        private CreateCoordinationContextResponse Enlist(
            ref CreateCoordinationContext cccMessage)
        {
            int num = 0;
            while (true)
            {
                ActivationProxy activationProxy = this.GetActivationProxy();
                EndpointAddress suggestedAddress = activationProxy.To;
                EndpointAddress endpointAddress1 = this.wsatConfig.LocalActivationService(this.protocolVersion);
                EndpointAddress endpointAddress2 = this.wsatConfig.RemoteActivationService(this.protocolVersion);
                try
                {
                    return activationProxy.SendCreateCoordinationContext(ref cccMessage);
                }
                catch (WsatSendFailureException ex)
                {
                    DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Warning);
                    switch (ex.InnerException)
                    {
                        case TimeoutException _:
                        case QuotaExceededException _:
                        case FaultException _:
                            throw;
                        default:
                            if (num > 10)
                            {
                                throw;
                            }
                            else
                            {
                                if (num > 5)
                                {
                                    if (endpointAddress2 != (EndpointAddress)null)
                                    {
                                        if ((object)suggestedAddress == (object)endpointAddress1)
                                        {
                                            suggestedAddress = endpointAddress2;
                                            break;
                                        }
                                        break;
                                    }
                                    break;
                                }
                                break;
                            }
                    }
                }
                finally
                {
                    activationProxy.Release();
                }
                this.TryStartMsdtcService();
                this.RefreshActivationProxy(suggestedAddress);
                Thread.Sleep(0);
                ++num;
            }
        }

        private void TryStartMsdtcService()
        {
            try
            {
                TransactionInterop.GetWhereabouts();
            }
            catch (TransactionException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Warning);
            }
        }

        private ActivationProxy GetActivationProxy()
        {
            if (this.activationProxy == null)
                this.RefreshActivationProxy((EndpointAddress)null);
            lock (this.proxyLock)
            {
                ActivationProxy activationProxy = this.activationProxy;
                activationProxy.AddRef();
                return activationProxy;
            }
        }

        private void RefreshActivationProxy(EndpointAddress suggestedAddress)
        {
            EndpointAddress address = suggestedAddress;
            if (address == (EndpointAddress)null)
            {
                address = this.wsatConfig.LocalActivationService(this.protocolVersion);
                if (address == (EndpointAddress)null)
                    address = this.wsatConfig.RemoteActivationService(this.protocolVersion);
            }
            if (!(address != (EndpointAddress)null))
                DiagnosticUtility.FailFast("Must have valid activation service address");
            lock (this.proxyLock)
            {
                ActivationProxy activationProxy = this.CreateActivationProxy(address);
                if (this.activationProxy != null)
                    this.activationProxy.Release();
                this.activationProxy = activationProxy;
            }
        }

        private ActivationProxy CreateActivationProxy(EndpointAddress address)
        {
            CoordinationService coordinationService = this.GetCoordinationService();
            try
            {
                return coordinationService.CreateActivationProxy(address, false);
            }
            catch (CreateChannelFailureException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("WsatProxyCreationFailed"), (Exception)ex));
            }
        }

        private CoordinationService GetCoordinationService()
        {
            if (this.coordinationService == null)
            {
                lock (this.proxyLock)
                {
                    if (this.coordinationService == null)
                    {
                        try
                        {
                            this.coordinationService = new CoordinationService(new CoordinationServiceConfiguration()
                            {
                                Mode = CoordinationServiceMode.Formatter,
                                RemoteClientsEnabled = this.wsatConfig.RemoteActivationService(this.protocolVersion) != (EndpointAddress)null
                            }, this.protocolVersion);
                        }
                        catch (MessagingInitializationException ex)
                        {
                            DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(System.ServiceModel.SR.GetString("WsatMessagingInitializationFailed"), (Exception)ex));
                        }
                    }
                }
            }
            return this.coordinationService;
        }

        private static byte[] CreateFixedPropagationToken()
        {
            if (WsatProxy.fixedPropagationToken == null)
            {
                CommittableTransaction committableTransaction = new CommittableTransaction();
                byte[] propagationToken = TransactionInterop.GetTransmitterPropagationToken((Transaction)committableTransaction);
                try
                {
                    committableTransaction.Commit();
                }
                catch (TransactionException ex)
                {
                    DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Information);
                }
                Interlocked.CompareExchange<byte[]>(ref WsatProxy.fixedPropagationToken, propagationToken, (byte[])null);
            }
            byte[] numArray = new byte[WsatProxy.fixedPropagationToken.Length];
            Array.Copy((Array)WsatProxy.fixedPropagationToken, (Array)numArray, WsatProxy.fixedPropagationToken.Length);
            return numArray;
        }

        private static byte[] MarshalPropagationToken(
            ref Guid transactionId,
            IsolationLevel isoLevel,
            IsolationFlags isoFlags,
            string description)
        {
            byte[] propagationToken = WsatProxy.CreateFixedPropagationToken();
            byte[] byteArray = transactionId.ToByteArray();
            Array.Copy((Array)byteArray, 0, (Array)propagationToken, 8, byteArray.Length);
            byte[] bytes1 = BitConverter.GetBytes((int)WsatProxy.ConvertIsolationLevel(isoLevel));
            Array.Copy((Array)bytes1, 0, (Array)propagationToken, 24, bytes1.Length);
            byte[] bytes2 = BitConverter.GetBytes((int)isoFlags);
            Array.Copy((Array)bytes2, 0, (Array)propagationToken, 28, bytes2.Length);
            if (!string.IsNullOrEmpty(description))
            {
                byte[] bytes3 = Encoding.UTF8.GetBytes(description);
                int length = Math.Min(bytes3.Length, 39);
                Array.Copy((Array)bytes3, 0, (Array)propagationToken, 36, length);
                propagationToken[36 + length] = (byte)0;
            }
            return propagationToken;
        }

        private static WsatProxy.ProxyIsolationLevel ConvertIsolationLevel(
            IsolationLevel IsolationLevel)
        {
            WsatProxy.ProxyIsolationLevel proxyIsolationLevel;
            switch (IsolationLevel)
            {
                case IsolationLevel.Serializable:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.Serializable;
                    break;
                case IsolationLevel.RepeatableRead:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.RepeatableRead;
                    break;
                case IsolationLevel.ReadCommitted:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.CursorStability;
                    break;
                case IsolationLevel.ReadUncommitted:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.ReadUncommitted;
                    break;
                case IsolationLevel.Unspecified:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.Unspecified;
                    break;
                default:
                    proxyIsolationLevel = WsatProxy.ProxyIsolationLevel.Serializable;
                    break;
            }
            return proxyIsolationLevel;
        }

        private enum ProxyIsolationLevel
        {
            Unspecified = -1, // 0xFFFFFFFF
            Chaos = 16, // 0x00000010
            Browse = 256, // 0x00000100
            ReadUncommitted = 256, // 0x00000100
            CursorStability = 4096, // 0x00001000
            ReadCommitted = 4096, // 0x00001000
            RepeatableRead = 65536, // 0x00010000
            Isolated = 1048576, // 0x00100000
            Serializable = 1048576, // 0x00100000
        }
    }
}

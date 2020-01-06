using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Security;
using System.ServiceModel.Transactions;
using System.Transactions;
using System.Xml;

namespace System.ServiceModel.Channels
{
    internal abstract class TransactionChannel<TChannel> : LayeredChannel<TChannel>, ITransactionChannel
        where TChannel : class, IChannel
    {
        private ITransactionChannelManager factory;
        private TransactionFormatter formatter;

        protected TransactionChannel(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {
            this.factory = (ITransactionChannelManager)channelManager;
            if (this.factory.TransactionProtocol == TransactionProtocol.OleTransactions)
                this.formatter = TransactionFormatter.OleTxFormatter;
            else if (this.factory.TransactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
            {
                this.formatter = TransactionFormatter.WsatFormatter10;
            }
            else
            {
                if (this.factory.TransactionProtocol != TransactionProtocol.WSAtomicTransaction11)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(SR.Format("SFxBadTransactionProtocols")));
                this.formatter = TransactionFormatter.WsatFormatter11;
            }
        }

        internal TransactionFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
        }

        internal TransactionProtocol Protocol
        {
            get
            {
                return this.factory.TransactionProtocol;
            }
        }

        public override T GetProperty<T>()
        {
            return typeof(T) == typeof(FaultConverter) ? (T)(object)new TransactionChannelFaultConverter<TChannel>(this) : base.GetProperty<T>();
        }

        public T GetInnerProperty<T>() where T : class
        {
            return this.InnerChannel.GetProperty<T>();
        }

        private static bool Found(int index)
        {
            return index != -1;
        }

        private void FaultOnMessage(Message message, string reason, string codeString)
        {
            FaultCode senderFaultCode = FaultCode.CreateSenderFaultCode(codeString, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions");
            throw TraceUtility.ThrowHelperError((Exception)new FaultException(reason, senderFaultCode, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault"), message);
        }

        private ICollection<RequestSecurityTokenResponse> GetIssuedTokens(
            Message message)
        {
            return (ICollection<RequestSecurityTokenResponse>)IssuedTokensHeader.ExtractIssuances(message, this.factory.StandardsManager, message.Version.Envelope.UltimateDestinationActorValues, (XmlQualifiedName)null);
        }

        public void ReadIssuedTokens(Message message, MessageDirection direction)
        {
            TransactionFlowOption flowIssuedTokens = this.factory.FlowIssuedTokens;
            ICollection<RequestSecurityTokenResponse> issuedTokens = this.GetIssuedTokens(message);
            if (issuedTokens == null || issuedTokens.Count == 0)
                return;
            if (flowIssuedTokens == TransactionFlowOption.NotAllowed)
                this.FaultOnMessage(message, SR.Format("IssuedTokenFlowNotAllowed"), "IssuedTokenFlowNotAllowed");
            foreach (RequestSecurityTokenResponse securityTokenResponse in (IEnumerable<RequestSecurityTokenResponse>)issuedTokens)
                TransactionFlowProperty.Ensure(message).IssuedTokens.Add(securityTokenResponse);
        }

        private void ReadTransactionFromMessage(Message message, TransactionFlowOption txFlowOption)
        {
            TransactionInfo transactionInfo = (TransactionInfo)null;
            try
            {
                transactionInfo = this.formatter.ReadTransaction(message);
            }
            catch (TransactionException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                this.FaultOnMessage(message, SR.Format("SFxTransactionDeserializationFailed", (object)ex.Message), "TransactionHeaderMalformed");
            }
            if (transactionInfo != null)
            {
                TransactionMessageProperty.Set(transactionInfo, message);
            }
            else
            {
                if (txFlowOption != TransactionFlowOption.Mandatory)
                    return;
                this.FaultOnMessage(message, SR.Format("SFxTransactionFlowRequired"), "TransactionHeaderMissing");
            }
        }

        public virtual void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            this.ReadIssuedTokens(message, direction);
            TransactionFlowOption transaction = this.factory.GetTransaction(direction, message.Headers.Action);
            if (!TransactionFlowOptionHelper.AllowedOrRequired(transaction))
                return;
            this.ReadTransactionFromMessage(message, transaction);
        }

        public void WriteTransactionDataToMessage(Message message, MessageDirection direction)
        {
            TransactionFlowOption transaction = this.factory.GetTransaction(direction, message.Headers.Action);
            if (TransactionFlowOptionHelper.AllowedOrRequired(transaction))
                this.WriteTransactionToMessage(message, transaction);
            if (!TransactionFlowOptionHelper.AllowedOrRequired(this.factory.FlowIssuedTokens))
                return;
            this.WriteIssuedTokens(message, direction);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteTransactionToMessage(Message message, TransactionFlowOption txFlowOption)
        {
            Transaction transaction = TransactionFlowProperty.TryGetTransaction(message);
            if (transaction != (Transaction)null)
            {
                try
                {
                    this.formatter.WriteTransaction(transaction, message);
                }
                catch (TransactionException ex)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ProtocolException(ex.Message, (Exception)ex));
                }
            }
            else if (txFlowOption == TransactionFlowOption.Mandatory)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ProtocolException(SR.Format("SFxTransactionFlowRequired")));
        }

        public void WriteIssuedTokens(Message message, MessageDirection direction)
        {
            ICollection<RequestSecurityTokenResponse> issuedTokens = TransactionFlowProperty.TryGetIssuedTokens(message);
            if (issuedTokens == null)
                return;
            IssuedTokensHeader issuedTokensHeader = new IssuedTokensHeader((IEnumerable<RequestSecurityTokenResponse>)issuedTokens, this.factory.StandardsManager);
            message.Headers.Add((MessageHeader)issuedTokensHeader);
        }
    }
}

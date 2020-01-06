using System.Globalization;

namespace System.ServiceModel.Channels
{
    internal class TransactionChannelFaultConverter<TChannel> : FaultConverter where TChannel : class, IChannel
    {
        private TransactionChannel<TChannel> channel;

        internal TransactionChannelFaultConverter(TransactionChannel<TChannel> channel)
        {
            this.channel = channel;
        }

        protected override bool OnTryCreateException(
            Message message,
            MessageFault fault,
            out Exception exception)
        {
            if (message.Headers.Action == "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault")
            {
                exception = (Exception)new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                return true;
            }
            if (fault.IsMustUnderstandFault)
            {
                MessageHeader transactionHeader = this.channel.Formatter.EmptyTransactionHeader;
                if (MessageFault.WasHeaderNotUnderstood(message.Headers, transactionHeader.Name, transactionHeader.Namespace))
                {
                    exception = (Exception)new ProtocolException(SR.Format("SFxTransactionHeaderNotUnderstood", (object)transactionHeader.Name, (object)transactionHeader.Namespace, (object)this.channel.Protocol));
                    return true;
                }
            }
            FaultConverter innerProperty = this.channel.GetInnerProperty<FaultConverter>();
            if (innerProperty != null)
                return innerProperty.TryCreateException(message, fault, out exception);
            exception = (Exception)null;
            return false;
        }

        protected override bool OnTryCreateFaultMessage(Exception exception, out Message message)
        {
            FaultConverter innerProperty = this.channel.GetInnerProperty<FaultConverter>();
            if (innerProperty != null)
                return innerProperty.TryCreateFaultMessage(exception, out message);
            message = (Message)null;
            return false;
        }
    }
}

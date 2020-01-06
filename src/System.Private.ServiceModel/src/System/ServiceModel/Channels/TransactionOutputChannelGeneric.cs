using System.Runtime;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class TransactionOutputChannelGeneric<TChannel> : TransactionChannel<TChannel>, IOutputChannel, IChannel, ICommunicationObject
        where TChannel : class, IOutputChannel
    {
        public TransactionOutputChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return this.InnerChannel.Via;
            }
        }

        public IAsyncResult BeginSend(
            Message message,
            AsyncCallback callback,
            object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(
            Message message,
            TimeSpan timeout,
            AsyncCallback asyncCallback,
            object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, MessageDirection.Input);
            return this.InnerChannel.BeginSend(message, timeoutHelper.RemainingTime(), asyncCallback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, MessageDirection.Input);
            this.InnerChannel.Send(message, timeoutHelper.RemainingTime());
        }
    }
}

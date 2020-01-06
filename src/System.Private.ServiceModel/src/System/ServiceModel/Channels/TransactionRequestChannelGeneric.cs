using System.Runtime;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class TransactionRequestChannelGeneric<TChannel> : TransactionChannel<TChannel>, IRequestChannel, IChannel, ICommunicationObject
        where TChannel : class, IRequestChannel
    {
        public TransactionRequestChannelGeneric(
            ChannelManagerBase channelManager,
            TChannel innerChannel)
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

        public IAsyncResult BeginRequest(
            Message message,
            AsyncCallback callback,
            object state)
        {
            return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(
            Message message,
            TimeSpan timeout,
            AsyncCallback asyncCallback,
            object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, MessageDirection.Input);
            return this.InnerChannel.BeginRequest(message, timeoutHelper.RemainingTime(), asyncCallback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message message = this.InnerChannel.EndRequest(result);
            if (message != null)
                this.ReadIssuedTokens(message, MessageDirection.Output);
            return message;
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, MessageDirection.Input);
            Message message1 = this.InnerChannel.Request(message, timeoutHelper.RemainingTime());
            if (message1 != null)
                this.ReadIssuedTokens(message1, MessageDirection.Output);
            return message1;
        }
    }
}
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class TransactionReceiveChannelGeneric<TChannel> : TransactionChannel<TChannel>, IInputChannel, IChannel, ICommunicationObject
        where TChannel : class, IInputChannel
    {
        private MessageDirection receiveMessageDirection;

        public TransactionReceiveChannelGeneric(
            ChannelManagerBase channelManager,
            TChannel innerChannel,
            MessageDirection direction)
            : base(channelManager, innerChannel)
        {
            this.receiveMessageDirection = direction;
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.InnerChannel.LocalAddress;
            }
        }

        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceiveAsync((IInputChannel)this, timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            return InputChannel.HelpBeginReceive((IInputChannel)this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public IAsyncResult BeginTryReceive(
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            return this.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public virtual bool EndTryReceive(IAsyncResult asyncResult, out Message message)
        {
            if (!this.InnerChannel.EndTryReceive(asyncResult, out message))
                return false;
            if (message != null)
                this.ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            return true;
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (!this.InnerChannel.TryReceive(timeout, out message))
                return false;
            if (message != null)
                this.ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            return true;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.InnerChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            return this.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.InnerChannel.EndWaitForMessage(result);
        }
    }
}

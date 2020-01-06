using System.Runtime;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class TransactionDuplexChannelGeneric<TChannel> : TransactionReceiveChannelGeneric<TChannel>, IDuplexChannel, IInputChannel, IChannel, ICommunicationObject, IOutputChannel
        where TChannel : class, IDuplexChannel
    {
        private MessageDirection sendMessageDirection;

        public TransactionDuplexChannelGeneric(
            ChannelManagerBase channelManager,
            TChannel innerChannel,
            MessageDirection direction)
            : base(channelManager, innerChannel, direction)
        {
            if (direction == MessageDirection.Input)
                this.sendMessageDirection = MessageDirection.Output;
            else
                this.sendMessageDirection = MessageDirection.Input;
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

        public override void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            try
            {
                base.ReadTransactionDataFromMessage(message, direction);
            }
            catch (FaultException ex)
            {
                Message message1 = Message.CreateMessage(message.Version, ex.CreateMessageFault(), ex.Action);
                RequestReplyCorrelator.AddressReply(message1, message);
                RequestReplyCorrelator.PrepareReply(message1, message.Headers.MessageId);
                try
                {
                    this.Send(message1);
                }
                finally
                {
                    message1.Close();
                }
                throw;
            }
        }

        public IAsyncResult BeginSend(
            Message message,
            AsyncCallback callback,
            object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public virtual IAsyncResult BeginSend(
            Message message,
            TimeSpan timeout,
            AsyncCallback asyncCallback,
            object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, this.sendMessageDirection);
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

        public virtual void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WriteTransactionDataToMessage(message, this.sendMessageDirection);
            this.InnerChannel.Send(message, timeoutHelper.RemainingTime());
        }
    }
}

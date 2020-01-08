//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
   // using SR = System.ServiceModel.SR;

    sealed class TransactionChannelFactory<TChannel> : LayeredChannelFactory<TChannel>, ITransactionChannelManager
    {
        TransactionFlowOption flowIssuedTokens;
        SecurityStandardsManager standardsManager;
        Dictionary<DirectionalAction, TransactionFlowOption> dictionary;
        TransactionProtocol transactionProtocol;
        bool allowWildcardAction;

        public TransactionChannelFactory(
            TransactionProtocol transactionProtocol,
            BindingContext context,
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary,
            bool allowWildcardAction)
            : base(context.Binding, context.BuildInnerChannelFactory<TChannel>())
        {
            this.dictionary = dictionary;
            this.TransactionProtocol = transactionProtocol;
            this.allowWildcardAction = allowWildcardAction;
            this.standardsManager = SecurityStandardsHelper.CreateStandardsManager(this.TransactionProtocol);
        }

        public TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!TransactionProtocol.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.SFxBadTransactionProtocols)));
                this.transactionProtocol = value;
            }
        }

        public TransactionFlowOption FlowIssuedTokens
        {
            get { return this.flowIssuedTokens; }
            set { this.flowIssuedTokens = value; }
        }

        public SecurityStandardsManager StandardsManager
        {
            get { return this.standardsManager; }
            set
            {
                this.standardsManager = (value != null ? value : SecurityStandardsHelper.CreateStandardsManager(this.transactionProtocol));
            }
        }

        public IDictionary<DirectionalAction, TransactionFlowOption> Dictionary
        {
            get { return this.dictionary; }
        }

        public TransactionFlowOption GetTransaction(MessageDirection direction, string action)
        {
            TransactionFlowOption txOption;
            if (!dictionary.TryGetValue(new DirectionalAction(direction, action), out txOption))
            {
                //Fixinng this for clients that opted in for lesser validation before flowing out a transaction
                if (this.allowWildcardAction && dictionary.TryGetValue(new DirectionalAction(direction, MessageHeaders.WildcardAction), out txOption))
                {
                    return txOption;
                }
                else
                    return TransactionFlowOption.NotAllowed;
            }

            else
                return txOption;
        }

        protected override TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            TChannel innerChannel = ((IChannelFactory<TChannel>)InnerChannelFactory).CreateChannel(remoteAddress, via);
            return CreateTransactionChannel(innerChannel);
        }

        TChannel CreateTransactionChannel(TChannel innerChannel)
        {
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel)(object)new TransactionDuplexSessionChannel(this, (IDuplexSessionChannel)(object)innerChannel);
            }
            else if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel)(object)new TransactionRequestSessionChannel(this, (IRequestSessionChannel)(object)innerChannel);
            }
            else if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel)(object)new TransactionOutputSessionChannel(this, (IOutputSessionChannel)(object)innerChannel);
            }
            else if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel)(object)new TransactionOutputChannel(this, (IOutputChannel)(object)innerChannel);
            }
            else if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel)(object)new TransactionRequestChannel(this, (IRequestChannel)(object)innerChannel);
            }
            else if (typeof(TChannel) == typeof(IDuplexChannel))
            {
                return (TChannel)(object)new TransactionDuplexChannel(this, (IDuplexChannel)(object)innerChannel);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateChannelTypeNotSupportedException(typeof(TChannel)));
            }
        }

        //===========================================================
        //                Transaction Output Channel classes
        //===========================================================

        sealed class TransactionOutputChannel : TransactionOutputChannelGeneric<IOutputChannel>
        {
            public TransactionOutputChannel(ChannelManagerBase channelManager, IOutputChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        sealed class TransactionRequestChannel : TransactionRequestChannelGeneric<IRequestChannel>
        {
            public TransactionRequestChannel(ChannelManagerBase channelManager, IRequestChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

        }

        sealed class TransactionDuplexChannel : TransactionOutputDuplexChannelGeneric<IDuplexChannel>
        {
            public TransactionDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        sealed class TransactionOutputSessionChannel : TransactionOutputChannelGeneric<IOutputSessionChannel>, IOutputSessionChannel
        {
            public TransactionOutputSessionChannel(ChannelManagerBase channelManager, IOutputSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session { get { return InnerChannel.Session; } }

        }

        sealed class TransactionRequestSessionChannel : TransactionRequestChannelGeneric<IRequestSessionChannel>, IRequestSessionChannel
        {
            public TransactionRequestSessionChannel(ChannelManagerBase channelManager, IRequestSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session { get { return InnerChannel.Session; } }

        }

        sealed class TransactionDuplexSessionChannel : TransactionOutputDuplexChannelGeneric<IDuplexSessionChannel>, IDuplexSessionChannel
        {
            public TransactionDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IDuplexSession Session { get { return InnerChannel.Session; } }
        }
    }

    static class SecurityStandardsHelper
    {
        static SecurityStandardsManager SecurityStandardsManager2007 =
            CreateStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12);

        static SecurityStandardsManager CreateStandardsManager(MessageSecurityVersion securityVersion)
        {
            return new SecurityStandardsManager(
                securityVersion,
                new WSSecurityTokenSerializer(securityVersion.SecurityVersion, securityVersion.TrustVersion, securityVersion.SecureConversationVersion, false, null, null, null));
        }

        public static SecurityStandardsManager CreateStandardsManager(TransactionProtocol transactionProtocol)
        {
            if (transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004 ||
                transactionProtocol == TransactionProtocol.OleTransactions)
            {
                return SecurityStandardsManager.DefaultInstance;
            }
            else
            {
                return SecurityStandardsHelper.SecurityStandardsManager2007;
            }
        }
    }

    //==============================================================
    //                Transaction channel base generic classes
    //==============================================================

    class TransactionOutputChannelGeneric<TChannel> : TransactionChannel<TChannel>, IOutputChannel
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
                return InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return InnerChannel.Via;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, MessageDirection.Input);
            return InnerChannel.BeginSend(message, timeoutHelper.RemainingTime(), asyncCallback, state);

        }

        public void EndSend(IAsyncResult result)
        {
            InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, MessageDirection.Input);
            InnerChannel.Send(message, timeoutHelper.RemainingTime());
        }
    }



    class TransactionRequestChannelGeneric<TChannel> : TransactionChannel<TChannel>, IRequestChannel
        where TChannel : class, IRequestChannel
    {
        public TransactionRequestChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return InnerChannel.Via;
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, MessageDirection.Input);
            return InnerChannel.BeginRequest(message, timeoutHelper.RemainingTime(), asyncCallback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message reply = InnerChannel.EndRequest(result);
            if (reply != null)
                this.ReadIssuedTokens(reply, MessageDirection.Output);
            return reply;
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, MessageDirection.Input);
            Message reply = InnerChannel.Request(message, timeoutHelper.RemainingTime());
            if (reply != null)
                this.ReadIssuedTokens(reply, MessageDirection.Output);
            return reply;
        }
    }

    class TransactionOutputDuplexChannelGeneric<TChannel> : TransactionDuplexChannelGeneric<TChannel>
        where TChannel : class, IDuplexChannel
    {
        public TransactionOutputDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel, MessageDirection.Output)
        {
        }
    }

    class TransactionDuplexChannelGeneric<TChannel> : TransactionReceiveChannelGeneric<TChannel>, IDuplexChannel
     where TChannel : class, IDuplexChannel
    {
        MessageDirection sendMessageDirection;

        public TransactionDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction)
            : base(channelManager, innerChannel, direction)
        {
            if (direction == MessageDirection.Input)
            {
                this.sendMessageDirection = MessageDirection.Output;
            }
            else
            {
                this.sendMessageDirection = MessageDirection.Input;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return InnerChannel.Via;
            }
        }

        public override void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            try
            {
                base.ReadTransactionDataFromMessage(message, direction);
            }
            catch (FaultException fault)
            {
                Message reply = Message.CreateMessage(message.Version, fault.CreateMessageFault(), fault.Action);

                System.ServiceModel.Channels.RequestReplyCorrelator.AddressReply(reply, message);
                System.ServiceModel.Channels.RequestReplyCorrelator.PrepareReply(reply, message.Headers.MessageId);

                try
                {
                    this.Send(reply);
                }
                finally
                {
                    reply.Close();
                }

                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public virtual IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, sendMessageDirection);
            return InnerChannel.BeginSend(message, timeoutHelper.RemainingTime(), asyncCallback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public virtual void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, sendMessageDirection);
            InnerChannel.Send(message, timeoutHelper.RemainingTime());
        }
    }

    class TransactionReceiveChannelGeneric<TChannel> : TransactionChannel<TChannel>, IInputChannel
      where TChannel : class, IInputChannel
    {
        MessageDirection receiveMessageDirection;

        public TransactionReceiveChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction)
            : base(channelManager, innerChannel)
        {
            this.receiveMessageDirection = direction;
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return InnerChannel.LocalAddress;
            }
        }

        public Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceive(this, timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public virtual bool EndTryReceive(IAsyncResult asyncResult, out Message message)
        {
            if (!InnerChannel.EndTryReceive(asyncResult, out message))
            {
                return false;
            }

            if (message != null)
            {
                ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }

            return true;
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (!InnerChannel.TryReceive(timeout, out message))
            {
                return false;
            }

            if (message != null)
            {
                ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }

            return true;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return InnerChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return InnerChannel.EndWaitForMessage(result);
        }
    }

}

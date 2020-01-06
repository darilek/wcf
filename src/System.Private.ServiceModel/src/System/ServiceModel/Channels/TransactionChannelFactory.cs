using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
    internal sealed class TransactionChannelFactory<TChannel> : LayeredChannelFactory<TChannel>, ITransactionChannelManager
    {
        private TransactionFlowOption flowIssuedTokens;
        private SecurityStandardsManager standardsManager;
        private Dictionary<DirectionalAction, TransactionFlowOption> dictionary;
        private TransactionProtocol transactionProtocol;
        private bool allowWildcardAction;

        public TransactionChannelFactory(
            TransactionProtocol transactionProtocol,
            BindingContext context,
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary,
            bool allowWildcardAction)
            : base((IDefaultCommunicationTimeouts)context.Binding, (IChannelFactory)context.BuildInnerChannelFactory<TChannel>())
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(SR.Format("SFxBadTransactionProtocols")));
                this.transactionProtocol = value;
            }
        }

        public TransactionFlowOption FlowIssuedTokens
        {
            get
            {
                return this.flowIssuedTokens;
            }
            set
            {
                this.flowIssuedTokens = value;
            }
        }

        public SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                this.standardsManager = value != null ? value : SecurityStandardsHelper.CreateStandardsManager(this.transactionProtocol);
            }
        }

        public IDictionary<DirectionalAction, TransactionFlowOption> Dictionary
        {
            get
            {
                return (IDictionary<DirectionalAction, TransactionFlowOption>)this.dictionary;
            }
        }

        public TransactionFlowOption GetTransaction(
            MessageDirection direction,
            string action)
        {
            TransactionFlowOption transactionFlowOption;
            return !this.dictionary.TryGetValue(new DirectionalAction(direction, action), out transactionFlowOption) && (!this.allowWildcardAction || !this.dictionary.TryGetValue(new DirectionalAction(direction, "*"), out transactionFlowOption)) ? TransactionFlowOption.NotAllowed : transactionFlowOption;
        }

        protected override TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            return this.CreateTransactionChannel(((IChannelFactory<TChannel>)this.InnerChannelFactory).CreateChannel(remoteAddress, via));
        }

        private TChannel CreateTransactionChannel(TChannel innerChannel)
        {
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionDuplexSessionChannel((ChannelManagerBase)this, (IDuplexSessionChannel)(object)innerChannel);
            if (typeof(TChannel) == typeof(IRequestSessionChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionRequestSessionChannel((ChannelManagerBase)this, (IRequestSessionChannel)(object)innerChannel);
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionOutputSessionChannel((ChannelManagerBase)this, (IOutputSessionChannel)(object)innerChannel);
            if (typeof(TChannel) == typeof(IOutputChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionOutputChannel((ChannelManagerBase)this, (IOutputChannel)(object)innerChannel);
            if (typeof(TChannel) == typeof(IRequestChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionRequestChannel((ChannelManagerBase)this, (IRequestChannel)(object)innerChannel);
            if (typeof(TChannel) == typeof(IDuplexChannel))
                return (TChannel)new TransactionChannelFactory<TChannel>.TransactionDuplexChannel((ChannelManagerBase)this, (IDuplexChannel)(object)innerChannel);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateChannelTypeNotSupportedException(typeof(TChannel)));
        }

        private sealed class TransactionOutputChannel : TransactionOutputChannelGeneric<IOutputChannel>
        {
            public TransactionOutputChannel(
                ChannelManagerBase channelManager,
                IOutputChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionRequestChannel : TransactionRequestChannelGeneric<IRequestChannel>
        {
            public TransactionRequestChannel(
                ChannelManagerBase channelManager,
                IRequestChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionDuplexChannel : TransactionOutputDuplexChannelGeneric<IDuplexChannel>
        {
            public TransactionDuplexChannel(
                ChannelManagerBase channelManager,
                IDuplexChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        private sealed class TransactionOutputSessionChannel : TransactionOutputChannelGeneric<IOutputSessionChannel>, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public TransactionOutputSessionChannel(
                ChannelManagerBase channelManager,
                IOutputSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return this.InnerChannel.Session;
                }
            }
        }

        private sealed class TransactionRequestSessionChannel : TransactionRequestChannelGeneric<IRequestSessionChannel>, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public TransactionRequestSessionChannel(
                ChannelManagerBase channelManager,
                IRequestSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return this.InnerChannel.Session;
                }
            }
        }

        private sealed class TransactionDuplexSessionChannel : TransactionOutputDuplexChannelGeneric<IDuplexSessionChannel>, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IChannel, ICommunicationObject, IOutputChannel, ISessionChannel<IDuplexSession>
        {
            public TransactionDuplexSessionChannel(
                ChannelManagerBase channelManager,
                IDuplexSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IDuplexSession Session
            {
                get
                {
                    return this.InnerChannel.Session;
                }
            }
        }
    }
}
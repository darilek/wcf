using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal class TransactionOutputDuplexChannelGeneric<TChannel> : TransactionDuplexChannelGeneric<TChannel>
        where TChannel : class, IDuplexChannel
    {
        public TransactionOutputDuplexChannelGeneric(
            ChannelManagerBase channelManager,
            TChannel innerChannel)
            : base(channelManager, innerChannel, MessageDirection.Output)
        {
        }
    }
}

using System.ServiceModel.Channels;
using Microsoft.Transactions.Wsat.Messaging;

namespace System.ServiceModel.Transactions
{
    class WsatTransactionFormatter10 : WsatTransactionFormatter
    {
        static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version10);

        public WsatTransactionFormatter10() : base(ProtocolVersion.Version10) { }

        //=======================================================================================
        public override MessageHeader EmptyTransactionHeader
        {
            get { return emptyTransactionHeader; }
        }
    }
}
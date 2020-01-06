using System.ServiceModel.Channels;
using Microsoft.Transactions.Wsat.Messaging;

namespace System.ServiceModel.Transactions
{
    class WsatTransactionFormatter11 : WsatTransactionFormatter
    {
        static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version11);

        public WsatTransactionFormatter11() : base(ProtocolVersion.Version11) { }

        //=======================================================================================
        public override MessageHeader EmptyTransactionHeader
        {
            get { return emptyTransactionHeader; }
        }
    }
}
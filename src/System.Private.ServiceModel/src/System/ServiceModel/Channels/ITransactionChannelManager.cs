using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
    internal interface ITransactionChannelManager
    {
        TransactionProtocol TransactionProtocol { get; set; }

        TransactionFlowOption FlowIssuedTokens { get; set; }

        IDictionary<DirectionalAction, TransactionFlowOption> Dictionary { get; }

        TransactionFlowOption GetTransaction(
            MessageDirection direction,
            string action);

        SecurityStandardsManager StandardsManager { get; }
    }
}

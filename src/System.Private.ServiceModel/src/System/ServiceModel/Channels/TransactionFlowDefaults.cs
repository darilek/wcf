namespace System.ServiceModel.Channels
{
    internal static class TransactionFlowDefaults
    {
        internal static TransactionProtocol TransactionProtocol = TransactionProtocol.OleTransactions;
        internal const TransactionFlowOption IssuedTokens = TransactionFlowOption.NotAllowed;
        internal const bool Transactions = false;
        internal const string TransactionProtocolString = "OleTransactions";
    }
}

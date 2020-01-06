namespace System.ServiceModel
{
    internal class OleTransactionsProtocol : TransactionProtocol
    {
        private static TransactionProtocol instance = (TransactionProtocol)new OleTransactionsProtocol();

        internal static TransactionProtocol Instance
        {
            get
            {
                return OleTransactionsProtocol.instance;
            }
        }

        internal override string Name
        {
            get
            {
                return "OleTransactions";
            }
        }
    }
}
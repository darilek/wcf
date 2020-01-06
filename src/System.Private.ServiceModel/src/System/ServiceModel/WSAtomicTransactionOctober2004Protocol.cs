namespace System.ServiceModel
{
    internal class WSAtomicTransactionOctober2004Protocol : TransactionProtocol
    {
        private static TransactionProtocol instance = (TransactionProtocol)new WSAtomicTransactionOctober2004Protocol();

        internal static TransactionProtocol Instance
        {
            get
            {
                return WSAtomicTransactionOctober2004Protocol.instance;
            }
        }

        internal override string Name
        {
            get
            {
                return "WSAtomicTransactionOctober2004";
            }
        }
    }
}
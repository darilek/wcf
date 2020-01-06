namespace System.ServiceModel
{
    internal class WSAtomicTransaction11Protocol : TransactionProtocol
    {
        private static TransactionProtocol instance = (TransactionProtocol)new WSAtomicTransaction11Protocol();

        internal static TransactionProtocol Instance
        {
            get
            {
                return WSAtomicTransaction11Protocol.instance;
            }
        }

        internal override string Name
        {
            get
            {
                return "WSAtomicTransaction11";
            }
        }
    }
}
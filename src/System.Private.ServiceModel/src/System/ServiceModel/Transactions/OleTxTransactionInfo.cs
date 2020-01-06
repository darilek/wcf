using System.Transactions;

namespace System.ServiceModel.Transactions
{
    class OleTxTransactionInfo : TransactionInfo
    {
        readonly OleTxTransactionHeader header;

        public OleTxTransactionInfo(OleTxTransactionHeader header)
        {
            this.header = header;
        }

        public override Transaction UnmarshalTransaction()
        {
            Transaction tx = UnmarshalPropagationToken(header.PropagationToken);

            header.WsatExtendedInformation?.TryCache(tx);

            return tx;
        }

        public static Transaction UnmarshalPropagationToken(byte[] propToken)
        {
            try
            {
                return TransactionInterop.GetTransactionFromTransmitterPropagationToken(propToken);
            }
            catch (ArgumentException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException("InvalidPropagationToken", e));
            }
        }
    }
}

using System.Transactions;

namespace System.ServiceModel.Transactions
{
    internal abstract class TransactionInfo
    {
        public abstract Transaction UnmarshalTransaction();
    }
}

using System.ComponentModel;

namespace System.ServiceModel
{
    /// <summary>Specifies the transaction protocol used in flowing transactions.</summary>
    // TODO: Add converter
    //[TypeConverter(typeof(TransactionProtocolConverter))]
    public abstract class TransactionProtocol
    {
        /// <summary>Gets the default value for the transaction protocol.</summary>
        /// <returns>A valid <see cref="T:System.ServiceModel.TransactionProtocol" /> value that specifies the default transaction protocol to be used in flowing a transaction.</returns>
        public static TransactionProtocol Default
        {
            get
            {
                return TransactionProtocol.OleTransactions;
            }
        }

        /// <summary>Gets the OleTransactions transaction protocol value.</summary>
        /// <returns>An <see cref="P:System.ServiceModel.TransactionProtocol.OleTransactions" /> value.</returns>
        public static TransactionProtocol OleTransactions
        {
            get
            {
                return OleTransactionsProtocol.Instance;
            }
        }

        /// <summary>Gets the WSAtomicTransactionOctober2004 transaction protocol value.</summary>
        /// <returns>A <see cref="T:System.ServiceModel.TransactionProtocol" /> value.</returns>
        public static TransactionProtocol WSAtomicTransactionOctober2004
        {
            get
            {
                return WSAtomicTransactionOctober2004Protocol.Instance;
            }
        }

        /// <summary>Gets the WSAtomicTransaction11 transaction protocol value.</summary>
        /// <returns>A <see cref="T:System.ServiceModel.TransactionProtocol" /> value.</returns>
        public static TransactionProtocol WSAtomicTransaction11
        {
            get
            {
                return WSAtomicTransaction11Protocol.Instance;
            }
        }

        internal abstract string Name { get; }

        internal static bool IsDefined(TransactionProtocol transactionProtocol)
        {
            return transactionProtocol == TransactionProtocol.OleTransactions || transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004 || transactionProtocol == TransactionProtocol.WSAtomicTransaction11;
        }
    }
}

using System.ServiceModel.Transactions;
using System.Transactions;

namespace System.ServiceModel.Channels
{
    /// <summary>Allows a custom channel, which uses a proprietary transaction flow mechanism, to inject its transaction into the Windows Communication Foundation (WCF) framework. This class cannot be inherited.</summary>
    public sealed class TransactionMessageProperty
    {
        private TransactionInfo flowedTransactionInfo;
        private Transaction flowedTransaction;
        private const string PropertyName = "TransactionMessageProperty";

        private TransactionMessageProperty()
        {
        }

        /// <summary>Gets the transaction that will be used when executing the service method.</summary>
        /// <returns>A <see cref="T:System.Transactions.Transaction" /> instance that will be used when executing the service method.</returns>
        public Transaction Transaction
        {
            get
            {
                if (this.flowedTransaction == (Transaction)null)
                {
                    if (this.flowedTransactionInfo != null)
                    {
                        try
                        {
                            this.flowedTransaction = this.flowedTransactionInfo.UnmarshalTransaction();
                        }
                        catch (TransactionException ex)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)ex);
                        }
                    }
                }
                return this.flowedTransaction;
            }
        }

        internal static TransactionMessageProperty TryGet(Message message)
        {
            return message.Properties.ContainsKey(nameof(TransactionMessageProperty)) ? message.Properties[nameof(TransactionMessageProperty)] as TransactionMessageProperty : (TransactionMessageProperty)null;
        }

        internal static Transaction TryGetTransaction(Message message)
        {
            return !message.Properties.ContainsKey(nameof(TransactionMessageProperty)) ? (Transaction)null : ((TransactionMessageProperty)message.Properties[nameof(TransactionMessageProperty)]).Transaction;
        }

        private static TransactionMessageProperty GetPropertyAndThrowIfAlreadySet(
            Message message)
        {
            if (message.Properties.ContainsKey(nameof(TransactionMessageProperty)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new FaultException(SR.Format("SFxTryAddMultipleTransactionsOnMessage")));
            return new TransactionMessageProperty();
        }

        /// <summary>Sets the transaction that will be used when executing the service method.</summary>
        /// <param name="transaction">The transaction that will be used when executing the service method.</param>
        /// <param name="message">The incoming message that results in calling the service method.</param>
        /// <exception cref="T:System.ServiceModel.FaultException">The property has already been set on <paramref name="message" />.</exception>
        /// <exception cref="T:System.Transactions.TransactionException">
        /// <paramref name="transaction" /> needs to be unmarshaled, and that operation fails.</exception>
        public static void Set(Transaction transaction, Message message)
        {
            TransactionMessageProperty throwIfAlreadySet = TransactionMessageProperty.GetPropertyAndThrowIfAlreadySet(message);
            throwIfAlreadySet.flowedTransaction = transaction;
            message.Properties.Add(nameof(TransactionMessageProperty), (object)throwIfAlreadySet);
        }

        internal static void Set(TransactionInfo transactionInfo, Message message)
        {
            TransactionMessageProperty throwIfAlreadySet = TransactionMessageProperty.GetPropertyAndThrowIfAlreadySet(message);
            throwIfAlreadySet.flowedTransactionInfo = transactionInfo;
            message.Properties.Add(nameof(TransactionMessageProperty), (object)throwIfAlreadySet);
        }
    }
}

using System.ServiceModel.Transactions;
using System.Transactions;

namespace System.ServiceModel.Channels
{
    public sealed class TransactionMessageProperty
    {
        TransactionInfo flowedTransactionInfo;
        Transaction flowedTransaction;
        const string PropertyName = "TransactionMessageProperty";

        private TransactionMessageProperty()
        {
        }

        public Transaction Transaction
        {
            get
            {
                if (this.flowedTransaction == null && this.flowedTransactionInfo != null)
                {
                    try
                    {
                        this.flowedTransaction = this.flowedTransactionInfo.UnmarshalTransaction();
                    }
                    catch (TransactionException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                    }
                }
                return this.flowedTransaction;
            }
        }

        static internal TransactionMessageProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return message.Properties[PropertyName] as TransactionMessageProperty;
            else
                return null;
        }

        static internal Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey(PropertyName))
                return null;

            return ((TransactionMessageProperty)message.Properties[PropertyName]).Transaction;

        }

        static TransactionMessageProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new FaultException(SR.GetString(SR.SFxTryAddMultipleTransactionsOnMessage)));
            }

            return new TransactionMessageProperty();
        }

        static public void Set(Transaction transaction, Message message)
        {
            TransactionMessageProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransaction = transaction;
            message.Properties.Add(PropertyName, property);
        }

        static internal void Set(TransactionInfo transactionInfo, Message message)
        {
            TransactionMessageProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransactionInfo = transactionInfo;
            message.Properties.Add(PropertyName, property);
        }
    }
}

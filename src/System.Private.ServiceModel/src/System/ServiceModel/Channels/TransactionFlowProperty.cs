using System.Collections.Generic;
using System.ServiceModel.Security;
using System.Transactions;

namespace System.ServiceModel.Channels
{
    internal class TransactionFlowProperty
    {
        private Transaction flowedTransaction;
        private List<RequestSecurityTokenResponse> issuedTokens;
        private const string PropertyName = "TransactionFlowProperty";

        private TransactionFlowProperty()
        {
        }

        internal ICollection<RequestSecurityTokenResponse> IssuedTokens
        {
            get
            {
                if (this.issuedTokens == null)
                    this.issuedTokens = new List<RequestSecurityTokenResponse>();
                return (ICollection<RequestSecurityTokenResponse>)this.issuedTokens;
            }
        }

        internal Transaction Transaction
        {
            get
            {
                return this.flowedTransaction;
            }
        }

        internal static TransactionFlowProperty Ensure(Message message)
        {
            if (message.Properties.ContainsKey(nameof(TransactionFlowProperty)))
                return (TransactionFlowProperty)message.Properties[nameof(TransactionFlowProperty)];
            TransactionFlowProperty transactionFlowProperty = new TransactionFlowProperty();
            message.Properties.Add(nameof(TransactionFlowProperty), (object)transactionFlowProperty);
            return transactionFlowProperty;
        }

        internal static TransactionFlowProperty TryGet(Message message)
        {
            return message.Properties.ContainsKey(nameof(TransactionFlowProperty)) ? message.Properties[nameof(TransactionFlowProperty)] as TransactionFlowProperty : (TransactionFlowProperty)null;
        }

        internal static ICollection<RequestSecurityTokenResponse> TryGetIssuedTokens(
            Message message)
        {
            TransactionFlowProperty transactionFlowProperty = TransactionFlowProperty.TryGet(message);
            if (transactionFlowProperty == null)
                return (ICollection<RequestSecurityTokenResponse>)null;
            return transactionFlowProperty.issuedTokens == null || transactionFlowProperty.issuedTokens.Count == 0 ? (ICollection<RequestSecurityTokenResponse>)null : (ICollection<RequestSecurityTokenResponse>)transactionFlowProperty.issuedTokens;
        }

        internal static Transaction TryGetTransaction(Message message)
        {
            return !message.Properties.ContainsKey(nameof(TransactionFlowProperty)) ? (Transaction)null : ((TransactionFlowProperty)message.Properties[nameof(TransactionFlowProperty)]).Transaction;
        }

        private static TransactionFlowProperty GetPropertyAndThrowIfAlreadySet(
            Message message)
        {
            TransactionFlowProperty transactionFlowProperty = TransactionFlowProperty.TryGet(message);
            if (transactionFlowProperty != null)
            {
                if (transactionFlowProperty.flowedTransaction != (Transaction)null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new FaultException(SR.Format("SFxTryAddMultipleTransactionsOnMessage")));
            }
            else
                transactionFlowProperty = new TransactionFlowProperty();
            return transactionFlowProperty;
        }

        internal static void Set(Transaction transaction, Message message)
        {
            TransactionFlowProperty throwIfAlreadySet = TransactionFlowProperty.GetPropertyAndThrowIfAlreadySet(message);
            throwIfAlreadySet.flowedTransaction = transaction;
            message.Properties.Add(nameof(TransactionFlowProperty), (object)throwIfAlreadySet);
        }
    }
}

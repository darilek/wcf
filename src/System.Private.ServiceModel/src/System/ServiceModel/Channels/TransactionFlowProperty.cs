using System.Collections.Generic;
using System.ServiceModel.Security;
using System.Transactions;

namespace System.ServiceModel.Channels
{
    class TransactionFlowProperty
    {
        Transaction flowedTransaction;
        List<RequestSecurityTokenResponse> issuedTokens;
        const string PropertyName = "TransactionFlowProperty";

        private TransactionFlowProperty()
        {
        }

        internal ICollection<RequestSecurityTokenResponse> IssuedTokens
        {
            get
            {
                if (this.issuedTokens == null)
                {
                    this.issuedTokens = new List<RequestSecurityTokenResponse>();
                }

                return this.issuedTokens;
            }
        }

        internal Transaction Transaction
        {
            get { return this.flowedTransaction; }
        }

        internal static TransactionFlowProperty Ensure(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return (TransactionFlowProperty)message.Properties[PropertyName];

            TransactionFlowProperty property = new TransactionFlowProperty();
            message.Properties.Add(PropertyName, property);
            return property;
        }

        internal static TransactionFlowProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return message.Properties[PropertyName] as TransactionFlowProperty;
            else
                return null;
        }

        internal static ICollection<RequestSecurityTokenResponse> TryGetIssuedTokens(Message message)
        {
            TransactionFlowProperty property = TransactionFlowProperty.TryGet(message);
            if (property == null)
                return null;

            // use this when reading only, consistently return null if no tokens.
            if (property.issuedTokens == null || property.issuedTokens.Count == 0)
                return null;

            return property.issuedTokens;
        }

        internal static Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey(PropertyName))
                return null;

            return ((TransactionFlowProperty)message.Properties[PropertyName]).Transaction;

        }

        static TransactionFlowProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            TransactionFlowProperty property = TryGet(message);

            if (property != null)
            {
                if (property.flowedTransaction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(SR.GetString(SR.SFxTryAddMultipleTransactionsOnMessage)));
                }
            }
            else
            {
                property = new TransactionFlowProperty();
            }

            return property;
        }

        internal static void Set(Transaction transaction, Message message)
        {
            TransactionFlowProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransaction = transaction;
            message.Properties.Add(PropertyName, property);
        }
    }
}

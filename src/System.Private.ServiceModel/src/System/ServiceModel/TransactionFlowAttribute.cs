using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TransactionFlowAttribute : Attribute, IOperationBehavior
    {
        private TransactionFlowOption transactions;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.TransactionFlowAttribute" /> class.</summary>
        /// <param name="transactions">A <see cref="T:System.ServiceModel.TransactionFlowOption" />.</param>
        public TransactionFlowAttribute(TransactionFlowOption transactions)
        {
            TransactionFlowBindingElement.ValidateOption(transactions);
            this.transactions = transactions;
        }

        /// <summary>Gets a value that indicates whether the incoming transaction is supported.</summary>
        /// <returns>A <see cref="T:System.ServiceModel.TransactionFlowOption" /> that indicates whether the incoming transaction is supported.</returns>
        public TransactionFlowOption Transactions
        {
            get
            {
                return this.transactions;
            }
        }

        internal static void OverrideFlow(
            BindingParameterCollection parameters,
            string action,
            MessageDirection direction,
            TransactionFlowOption option)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = TransactionFlowAttribute.EnsureDictionary(parameters);
            DirectionalAction key = new DirectionalAction(direction, action);
            if (dictionary.ContainsKey(key))
                dictionary[key] = option;
            else
                dictionary.Add(key, option);
        }

        private static Dictionary<DirectionalAction, TransactionFlowOption> EnsureDictionary(
            BindingParameterCollection parameters)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = parameters.Find<Dictionary<DirectionalAction, TransactionFlowOption>>();
            if (dictionary == null)
            {
                dictionary = new Dictionary<DirectionalAction, TransactionFlowOption>();
                parameters.Add((object)dictionary);
            }
            return dictionary;
        }

        private void ApplyBehavior(
            OperationDescription description,
            BindingParameterCollection parameters)
        {
            TransactionFlowAttribute.EnsureDictionary(parameters)[new DirectionalAction(description.Messages[0].Direction, description.Messages[0].Action)] = this.transactions;
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(
            OperationDescription description,
            DispatchOperation dispatch)
        {
        }

        void IOperationBehavior.AddBindingParameters(
            OperationDescription description,
            BindingParameterCollection parameters)
        {
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(parameters));
            this.ApplyBehavior(description, parameters);
        }

        void IOperationBehavior.ApplyClientBehavior(
            OperationDescription description,
            ClientOperation proxy)
        {
        }
    }
}

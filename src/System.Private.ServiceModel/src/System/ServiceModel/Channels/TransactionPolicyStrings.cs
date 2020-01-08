using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;

namespace System.ServiceModel.Channels
{
    static class TransactionPolicyStrings
    {
        public const string OptionalLocal = MetadataStrings.WSPolicy.Attributes.Optional;
        public const string OptionalPrefix10 = MetadataStrings.WSPolicy.Prefix + "1";
        public const string OptionalPrefix11 = MetadataStrings.WSPolicy.Prefix;
        public const string OptionalNamespaceLegacy = "http://schemas.xmlsoap.org/ws/2002/12/policy";
        public const string WsatTransactionsPrefix = AtomicTransactionExternalStrings.Prefix;
        public const string WsatTransactionsNamespace10 = AtomicTransactionExternal10Strings.Namespace;
        public const string WsatTransactionsNamespace11 = AtomicTransactionExternal11Strings.Namespace;
        public const string WsatTransactionsLocal = "ATAssertion";
        public const string OleTxTransactionsPrefix = OleTxTransactionExternalStrings.Prefix;
        public const string OleTxTransactionsNamespace = OleTxTransactionExternalStrings.Namespace;
        public const string OleTxTransactionsLocal = "OleTxAssertion";
        public const string TrueValue = "true";
    }
}

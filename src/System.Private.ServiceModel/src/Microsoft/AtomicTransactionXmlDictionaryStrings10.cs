using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class AtomicTransactionXmlDictionaryStrings10 : AtomicTransactionXmlDictionaryStrings
    {
        private static AtomicTransactionXmlDictionaryStrings instance = (AtomicTransactionXmlDictionaryStrings)new AtomicTransactionXmlDictionaryStrings10();

        public static AtomicTransactionXmlDictionaryStrings Instance
        {
            get
            {
                return AtomicTransactionXmlDictionaryStrings10.instance;
            }
        }

        public override XmlDictionaryString Namespace
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString CompletionUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CompletionUri;
            }
        }

        public override XmlDictionaryString Durable2PCUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Durable2PCUri;
            }
        }

        public override XmlDictionaryString Volatile2PCUri
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.Volatile2PCUri;
            }
        }

        public override XmlDictionaryString CommitAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CommitAction;
            }
        }

        public override XmlDictionaryString RollbackAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.RollbackAction;
            }
        }

        public override XmlDictionaryString CommittedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.CommittedAction;
            }
        }

        public override XmlDictionaryString AbortedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.AbortedAction;
            }
        }

        public override XmlDictionaryString PrepareAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.PrepareAction;
            }
        }

        public override XmlDictionaryString PreparedAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.PreparedAction;
            }
        }

        public override XmlDictionaryString ReadOnlyAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.ReadOnlyAction;
            }
        }

        public override XmlDictionaryString ReplayAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.ReplayAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return XD.AtomicTransactionExternal10Dictionary.FaultAction;
            }
        }
    }
}
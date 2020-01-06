using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class AtomicTransactionXmlDictionaryStrings11 : AtomicTransactionXmlDictionaryStrings
    {
        private static AtomicTransactionXmlDictionaryStrings instance = (AtomicTransactionXmlDictionaryStrings)new AtomicTransactionXmlDictionaryStrings11();

        public static AtomicTransactionXmlDictionaryStrings Instance
        {
            get
            {
                return AtomicTransactionXmlDictionaryStrings11.instance;
            }
        }

        public override XmlDictionaryString Namespace
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString CompletionUri
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.CompletionUri;
            }
        }

        public override XmlDictionaryString Durable2PCUri
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.Durable2PCUri;
            }
        }

        public override XmlDictionaryString Volatile2PCUri
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.Volatile2PCUri;
            }
        }

        public override XmlDictionaryString CommitAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.CommitAction;
            }
        }

        public override XmlDictionaryString RollbackAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.RollbackAction;
            }
        }

        public override XmlDictionaryString CommittedAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.CommittedAction;
            }
        }

        public override XmlDictionaryString AbortedAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.AbortedAction;
            }
        }

        public override XmlDictionaryString PrepareAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.PrepareAction;
            }
        }

        public override XmlDictionaryString PreparedAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.PreparedAction;
            }
        }

        public override XmlDictionaryString ReadOnlyAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.ReadOnlyAction;
            }
        }

        public override XmlDictionaryString ReplayAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.ReplayAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.FaultAction;
            }
        }
    }
}
using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal abstract class AtomicTransactionXmlDictionaryStrings
    {
        public static AtomicTransactionXmlDictionaryStrings Version(
            ProtocolVersion protocolVersion)
        {
            //ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(AtomicTransactionXmlDictionaryStrings), "V");
            if (protocolVersion == ProtocolVersion.Version10)
                return AtomicTransactionXmlDictionaryStrings10.Instance;
            return protocolVersion == ProtocolVersion.Version11 ? AtomicTransactionXmlDictionaryStrings11.Instance : (AtomicTransactionXmlDictionaryStrings)null;
        }

        public abstract XmlDictionaryString Namespace { get; }

        public abstract XmlDictionaryString CompletionUri { get; }

        public abstract XmlDictionaryString Durable2PCUri { get; }

        public abstract XmlDictionaryString Volatile2PCUri { get; }

        public abstract XmlDictionaryString CommitAction { get; }

        public abstract XmlDictionaryString RollbackAction { get; }

        public abstract XmlDictionaryString CommittedAction { get; }

        public abstract XmlDictionaryString AbortedAction { get; }

        public abstract XmlDictionaryString PrepareAction { get; }

        public abstract XmlDictionaryString PreparedAction { get; }

        public abstract XmlDictionaryString ReadOnlyAction { get; }

        public abstract XmlDictionaryString ReplayAction { get; }

        public abstract XmlDictionaryString FaultAction { get; }

        public XmlDictionaryString Prefix
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prefix;
            }
        }

        public XmlDictionaryString Prepare
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prepare;
            }
        }

        public XmlDictionaryString Prepared
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Prepared;
            }
        }

        public XmlDictionaryString ReadOnly
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.ReadOnly;
            }
        }

        public XmlDictionaryString Commit
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Commit;
            }
        }

        public XmlDictionaryString Rollback
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Rollback;
            }
        }

        public XmlDictionaryString Committed
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Committed;
            }
        }

        public XmlDictionaryString Aborted
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Aborted;
            }
        }

        public XmlDictionaryString Replay
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.Replay;
            }
        }

        public XmlDictionaryString CompletionCoordinatorPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CompletionCoordinatorPortType;
            }
        }

        public XmlDictionaryString CompletionParticipantPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CompletionParticipantPortType;
            }
        }

        public XmlDictionaryString CoordinatorPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.CoordinatorPortType;
            }
        }

        public XmlDictionaryString ParticipantPortType
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.ParticipantPortType;
            }
        }

        public XmlDictionaryString InconsistentInternalState
        {
            get
            {
                return XD.AtomicTransactionExternalDictionary.InconsistentInternalState;
            }
        }

        public XmlDictionaryString UnknownTransaction
        {
            get
            {
                return DXD.AtomicTransactionExternal11Dictionary.UnknownTransaction;
            }
        }
    }
}
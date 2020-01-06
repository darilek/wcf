using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal abstract class CoordinationXmlDictionaryStrings
    {
        public static CoordinationXmlDictionaryStrings Version(
            ProtocolVersion protocolVersion)
        {
            // ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationXmlDictionaryStrings), "V");
            if (protocolVersion == ProtocolVersion.Version10)
                return CoordinationXmlDictionaryStrings10.Instance;
            return protocolVersion == ProtocolVersion.Version11 ? CoordinationXmlDictionaryStrings11.Instance : (CoordinationXmlDictionaryStrings)null;
        }

        public abstract XmlDictionaryString Namespace { get; }

        public abstract XmlDictionaryString CreateCoordinationContextAction { get; }

        public abstract XmlDictionaryString CreateCoordinationContextResponseAction { get; }

        public abstract XmlDictionaryString RegisterAction { get; }

        public abstract XmlDictionaryString RegisterResponseAction { get; }

        public abstract XmlDictionaryString FaultAction { get; }

        public XmlDictionaryString Prefix
        {
            get
            {
                return XD.CoordinationExternalDictionary.Prefix;
            }
        }

        public XmlDictionaryString CreateCoordinationContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CreateCoordinationContext;
            }
        }

        public XmlDictionaryString CreateCoordinationContextResponse
        {
            get
            {
                return XD.CoordinationExternalDictionary.CreateCoordinationContextResponse;
            }
        }

        public XmlDictionaryString CoordinationContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinationContext;
            }
        }

        public XmlDictionaryString CurrentContext
        {
            get
            {
                return XD.CoordinationExternalDictionary.CurrentContext;
            }
        }

        public XmlDictionaryString CoordinationType
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinationType;
            }
        }

        public XmlDictionaryString RegistrationService
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegistrationService;
            }
        }

        public XmlDictionaryString Register
        {
            get
            {
                return XD.CoordinationExternalDictionary.Register;
            }
        }

        public XmlDictionaryString RegisterResponse
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegisterResponse;
            }
        }

        public XmlDictionaryString Protocol
        {
            get
            {
                return XD.CoordinationExternalDictionary.Protocol;
            }
        }

        public XmlDictionaryString CoordinatorProtocolService
        {
            get
            {
                return XD.CoordinationExternalDictionary.CoordinatorProtocolService;
            }
        }

        public XmlDictionaryString ParticipantProtocolService
        {
            get
            {
                return XD.CoordinationExternalDictionary.ParticipantProtocolService;
            }
        }

        public XmlDictionaryString Expires
        {
            get
            {
                return XD.CoordinationExternalDictionary.Expires;
            }
        }

        public XmlDictionaryString Identifier
        {
            get
            {
                return XD.CoordinationExternalDictionary.Identifier;
            }
        }

        public XmlDictionaryString ActivationCoordinatorPortType
        {
            get
            {
                return XD.CoordinationExternalDictionary.ActivationCoordinatorPortType;
            }
        }

        public XmlDictionaryString RegistrationCoordinatorPortType
        {
            get
            {
                return XD.CoordinationExternalDictionary.RegistrationCoordinatorPortType;
            }
        }

        public XmlDictionaryString InvalidState
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidState;
            }
        }

        public XmlDictionaryString InvalidProtocol
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidProtocol;
            }
        }

        public XmlDictionaryString InvalidParameters
        {
            get
            {
                return XD.CoordinationExternalDictionary.InvalidParameters;
            }
        }

        public XmlDictionaryString NoActivity
        {
            get
            {
                return XD.CoordinationExternalDictionary.NoActivity;
            }
        }

        public XmlDictionaryString ContextRefused
        {
            get
            {
                return XD.CoordinationExternalDictionary.ContextRefused;
            }
        }

        public XmlDictionaryString AlreadyRegistered
        {
            get
            {
                return XD.CoordinationExternalDictionary.AlreadyRegistered;
            }
        }

        public XmlDictionaryString CannotCreateContext
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CannotCreateContext;
            }
        }

        public XmlDictionaryString CannotRegisterParticipant
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CannotRegisterParticipant;
            }
        }
    }
}
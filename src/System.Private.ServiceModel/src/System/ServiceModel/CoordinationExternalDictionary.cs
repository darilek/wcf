using System.Xml;

namespace System.ServiceModel
{
    internal class CoordinationExternalDictionary
    {
        public XmlDictionaryString Prefix;
        public XmlDictionaryString CreateCoordinationContext;
        public XmlDictionaryString CreateCoordinationContextResponse;
        public XmlDictionaryString CoordinationContext;
        public XmlDictionaryString CurrentContext;
        public XmlDictionaryString CoordinationType;
        public XmlDictionaryString RegistrationService;
        public XmlDictionaryString Register;
        public XmlDictionaryString RegisterResponse;
        public XmlDictionaryString Protocol;
        public XmlDictionaryString CoordinatorProtocolService;
        public XmlDictionaryString ParticipantProtocolService;
        public XmlDictionaryString Expires;
        public XmlDictionaryString Identifier;
        public XmlDictionaryString ActivationCoordinatorPortType;
        public XmlDictionaryString RegistrationCoordinatorPortType;
        public XmlDictionaryString InvalidState;
        public XmlDictionaryString InvalidProtocol;
        public XmlDictionaryString InvalidParameters;
        public XmlDictionaryString NoActivity;
        public XmlDictionaryString ContextRefused;
        public XmlDictionaryString AlreadyRegistered;

        public CoordinationExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Prefix = dictionary.CreateString("wscoor", 357);
            this.CreateCoordinationContext = dictionary.CreateString(nameof(CreateCoordinationContext), 358);
            this.CreateCoordinationContextResponse = dictionary.CreateString(nameof(CreateCoordinationContextResponse), 359);
            this.CoordinationContext = dictionary.CreateString(nameof(CoordinationContext), 360);
            this.CurrentContext = dictionary.CreateString(nameof(CurrentContext), 361);
            this.CoordinationType = dictionary.CreateString(nameof(CoordinationType), 362);
            this.RegistrationService = dictionary.CreateString(nameof(RegistrationService), 363);
            this.Register = dictionary.CreateString(nameof(Register), 364);
            this.RegisterResponse = dictionary.CreateString(nameof(RegisterResponse), 365);
            this.Protocol = dictionary.CreateString("ProtocolIdentifier", 366);
            this.CoordinatorProtocolService = dictionary.CreateString(nameof(CoordinatorProtocolService), 367);
            this.ParticipantProtocolService = dictionary.CreateString(nameof(ParticipantProtocolService), 368);
            this.Expires = dictionary.CreateString(nameof(Expires), 55);
            this.Identifier = dictionary.CreateString(nameof(Identifier), 15);
            this.ActivationCoordinatorPortType = dictionary.CreateString(nameof(ActivationCoordinatorPortType), 374);
            this.RegistrationCoordinatorPortType = dictionary.CreateString(nameof(RegistrationCoordinatorPortType), 375);
            this.InvalidState = dictionary.CreateString(nameof(InvalidState), 376);
            this.InvalidProtocol = dictionary.CreateString(nameof(InvalidProtocol), 377);
            this.InvalidParameters = dictionary.CreateString(nameof(InvalidParameters), 378);
            this.NoActivity = dictionary.CreateString(nameof(NoActivity), 379);
            this.ContextRefused = dictionary.CreateString(nameof(ContextRefused), 380);
            this.AlreadyRegistered = dictionary.CreateString(nameof(AlreadyRegistered), 381);
        }
    }
}

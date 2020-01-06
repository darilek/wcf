namespace Microsoft.Transactions.Wsat.Messaging
{
    internal abstract class CoordinationStrings
    {
        public static CoordinationStrings Version(ProtocolVersion protocolVersion)
        {
            //ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(CoordinationStrings), "V");
            if (protocolVersion == ProtocolVersion.Version10)
                return CoordinationStrings10.Instance;
            return protocolVersion == ProtocolVersion.Version11 ? CoordinationStrings11.Instance : (CoordinationStrings)null;
        }

        public abstract string Namespace { get; }

        public abstract string CreateCoordinationContextAction { get; }

        public abstract string CreateCoordinationContextResponseAction { get; }

        public abstract string RegisterAction { get; }

        public abstract string RegisterResponseAction { get; }

        public abstract string FaultAction { get; }

        public string Prefix
        {
            get
            {
                return "wscoor";
            }
        }

        public string CreateCoordinationContext
        {
            get
            {
                return nameof(CreateCoordinationContext);
            }
        }

        public string CreateCoordinationContextResponse
        {
            get
            {
                return nameof(CreateCoordinationContextResponse);
            }
        }

        public string CoordinationContext
        {
            get
            {
                return nameof(CoordinationContext);
            }
        }

        public string CurrentContext
        {
            get
            {
                return nameof(CurrentContext);
            }
        }

        public string CoordinationType
        {
            get
            {
                return nameof(CoordinationType);
            }
        }

        public string RegistrationService
        {
            get
            {
                return nameof(RegistrationService);
            }
        }

        public string Register
        {
            get
            {
                return nameof(Register);
            }
        }

        public string RegisterResponse
        {
            get
            {
                return nameof(RegisterResponse);
            }
        }

        public string Protocol
        {
            get
            {
                return "ProtocolIdentifier";
            }
        }

        public string CoordinatorProtocolService
        {
            get
            {
                return nameof(CoordinatorProtocolService);
            }
        }

        public string ParticipantProtocolService
        {
            get
            {
                return nameof(ParticipantProtocolService);
            }
        }

        public string Expires
        {
            get
            {
                return nameof(Expires);
            }
        }

        public string Identifier
        {
            get
            {
                return nameof(Identifier);
            }
        }

        public string ActivationCoordinatorPortType
        {
            get
            {
                return nameof(ActivationCoordinatorPortType);
            }
        }

        public string RegistrationCoordinatorPortType
        {
            get
            {
                return nameof(RegistrationCoordinatorPortType);
            }
        }

        public string InvalidState
        {
            get
            {
                return nameof(InvalidState);
            }
        }

        public string InvalidProtocol
        {
            get
            {
                return nameof(InvalidProtocol);
            }
        }

        public string InvalidParameters
        {
            get
            {
                return nameof(InvalidParameters);
            }
        }

        public string NoActivity
        {
            get
            {
                return nameof(NoActivity);
            }
        }

        public string ContextRefused
        {
            get
            {
                return nameof(ContextRefused);
            }
        }

        public string AlreadyRegistered
        {
            get
            {
                return nameof(AlreadyRegistered);
            }
        }

        public string CannotCreateContext
        {
            get
            {
                return nameof(CannotCreateContext);
            }
        }

        public string CannotRegisterParticipant
        {
            get
            {
                return nameof(CannotRegisterParticipant);
            }
        }
    }
}
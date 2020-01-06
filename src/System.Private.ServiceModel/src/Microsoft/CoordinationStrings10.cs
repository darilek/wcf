namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class CoordinationStrings10 : CoordinationStrings
    {
        private static CoordinationStrings instance = (CoordinationStrings)new CoordinationStrings10();

        public static CoordinationStrings Instance
        {
            get
            {
                return CoordinationStrings10.instance;
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor";
            }
        }

        public override string CreateCoordinationContextAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext";
            }
        }

        public override string CreateCoordinationContextResponseAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse";
            }
        }

        public override string RegisterAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register";
            }
        }

        public override string RegisterResponseAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault";
            }
        }
    }
}
namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class CoordinationStrings11 : CoordinationStrings
    {
        private static CoordinationStrings instance = (CoordinationStrings)new CoordinationStrings11();

        public static CoordinationStrings Instance
        {
            get
            {
                return CoordinationStrings11.instance;
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06";
            }
        }

        public override string CreateCoordinationContextAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContext";
            }
        }

        public override string CreateCoordinationContextResponseAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContextResponse";
            }
        }

        public override string RegisterAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/Register";
            }
        }

        public override string RegisterResponseAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/RegisterResponse";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault";
            }
        }
    }
}
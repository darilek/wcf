using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class CoordinationXmlDictionaryStrings10 : CoordinationXmlDictionaryStrings
    {
        private static CoordinationXmlDictionaryStrings instance = (CoordinationXmlDictionaryStrings)new CoordinationXmlDictionaryStrings10();

        public static CoordinationXmlDictionaryStrings Instance
        {
            get
            {
                return CoordinationXmlDictionaryStrings10.instance;
            }
        }

        public override XmlDictionaryString Namespace
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.CreateCoordinationContextAction;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextResponseAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.CreateCoordinationContextResponseAction;
            }
        }

        public override XmlDictionaryString RegisterAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.RegisterAction;
            }
        }

        public override XmlDictionaryString RegisterResponseAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.RegisterResponseAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return XD.CoordinationExternal10Dictionary.FaultAction;
            }
        }
    }
}
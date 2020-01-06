using System.ServiceModel;
using System.Xml;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class CoordinationXmlDictionaryStrings11 : CoordinationXmlDictionaryStrings
    {
        private static CoordinationXmlDictionaryStrings instance = (CoordinationXmlDictionaryStrings)new CoordinationXmlDictionaryStrings11();

        public static CoordinationXmlDictionaryStrings Instance
        {
            get
            {
                return CoordinationXmlDictionaryStrings11.instance;
            }
        }

        public override XmlDictionaryString Namespace
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.Namespace;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CreateCoordinationContextAction;
            }
        }

        public override XmlDictionaryString CreateCoordinationContextResponseAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.CreateCoordinationContextResponseAction;
            }
        }

        public override XmlDictionaryString RegisterAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.RegisterAction;
            }
        }

        public override XmlDictionaryString RegisterResponseAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.RegisterResponseAction;
            }
        }

        public override XmlDictionaryString FaultAction
        {
            get
            {
                return DXD.CoordinationExternal11Dictionary.FaultAction;
            }
        }
    }
}
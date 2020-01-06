using System.Xml;

namespace System.ServiceModel
{
    internal class CoordinationExternal10Dictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString CreateCoordinationContextAction;
        public XmlDictionaryString CreateCoordinationContextResponseAction;
        public XmlDictionaryString RegisterAction;
        public XmlDictionaryString RegisterResponseAction;
        public XmlDictionaryString FaultAction;

        public CoordinationExternal10Dictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor", 356);
            this.CreateCoordinationContextAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext", 369);
            this.CreateCoordinationContextResponseAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse", 370);
            this.RegisterAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register", 371);
            this.RegisterResponseAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse", 372);
            this.FaultAction = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", 373);
        }
    }
}

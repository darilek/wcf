using System.Xml;

namespace System.ServiceModel
{
    internal class OleTxTransactionExternalDictionary
    {
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString OleTxTransaction;
        public XmlDictionaryString PropagationToken;

        public OleTxTransactionExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2006/02/tx/oletx", 352);
            this.Prefix = dictionary.CreateString("oletx", 353);
            this.OleTxTransaction = dictionary.CreateString(nameof(OleTxTransaction), 354);
            this.PropagationToken = dictionary.CreateString(nameof(PropagationToken), 355);
        }
    }
}

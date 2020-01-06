using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Transactions;
using System.Xml;

namespace System.ServiceModel.Transactions
{
    internal class OleTxTransactionHeader : MessageHeader
    {
        private static readonly XmlDictionaryString CoordinationNamespace = XD.CoordinationExternal10Dictionary.Namespace;
        private const string OleTxHeaderElement = "OleTxTransaction";
        private const string OleTxNamespace = "http://schemas.microsoft.com/ws/2006/02/tx/oletx";
        private byte[] propagationToken;
        private WsatExtendedInformation wsatInfo;

        public OleTxTransactionHeader(byte[] propagationToken, WsatExtendedInformation wsatInfo)
        {
            this.propagationToken = propagationToken;
            this.wsatInfo = wsatInfo;
        }

        public override bool MustUnderstand
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "OleTxTransaction";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/02/tx/oletx";
            }
        }

        public byte[] PropagationToken
        {
            get
            {
                return this.propagationToken;
            }
        }

        public WsatExtendedInformation WsatExtendedInformation
        {
            get
            {
                return this.wsatInfo;
            }
        }

        protected override void OnWriteHeaderContents(
            XmlDictionaryWriter writer,
            MessageVersion messageVersion)
        {
            if (this.wsatInfo != null)
            {
                if (this.wsatInfo.Timeout != 0U)
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Expires, OleTxTransactionHeader.CoordinationNamespace, XmlConvert.ToString(this.wsatInfo.Timeout));
                if (!string.IsNullOrEmpty(this.wsatInfo.Identifier))
                    writer.WriteAttributeString(XD.CoordinationExternalDictionary.Identifier, OleTxTransactionHeader.CoordinationNamespace, this.wsatInfo.Identifier);
            }
            OleTxTransactionHeader.WritePropagationTokenElement(writer, this.propagationToken);
        }

        public static OleTxTransactionHeader ReadFrom(Message message)
        {
            int header1;
            try
            {
                header1 = message.Headers.FindHeader("OleTxTransaction", "http://schemas.microsoft.com/ws/2006/02/tx/oletx");
            }
            catch (MessageHeaderException ex)
            {
                DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(SR.Format("OleTxHeaderCorrupt"), (Exception)ex));
            }
            if (header1 < 0)
                return (OleTxTransactionHeader)null;
            XmlDictionaryReader readerAtHeader = message.Headers.GetReaderAtHeader(header1);
            OleTxTransactionHeader transactionHeader;
            using (readerAtHeader)
            {
                try
                {
                    transactionHeader = OleTxTransactionHeader.ReadFrom(readerAtHeader);
                }
                catch (XmlException ex)
                {
                    DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionException(SR.Format("OleTxHeaderCorrupt"), (Exception)ex));
                }
            }
            MessageHeaderInfo header2 = message.Headers[header1];
            if (!message.Headers.UnderstoodHeaders.Contains(header2))
                message.Headers.UnderstoodHeaders.Add(header2);
            return transactionHeader;
        }

        private static OleTxTransactionHeader ReadFrom(XmlDictionaryReader reader)
        {
            WsatExtendedInformation wsatInfo = (WsatExtendedInformation)null;
            if (reader.IsStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction, XD.OleTxTransactionExternalDictionary.Namespace))
            {
                string attribute1 = reader.GetAttribute(XD.CoordinationExternalDictionary.Identifier, OleTxTransactionHeader.CoordinationNamespace);
                if (!string.IsNullOrEmpty(attribute1) && !Uri.TryCreate(attribute1, UriKind.Absolute, out Uri _))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new XmlException(SR.Format("InvalidWsatExtendedInfo")));
                string attribute2 = reader.GetAttribute(XD.CoordinationExternalDictionary.Expires, OleTxTransactionHeader.CoordinationNamespace);
                uint timeout = 0;
                if (!string.IsNullOrEmpty(attribute2))
                {
                    try
                    {
                        timeout = XmlConvert.ToUInt32(attribute2);
                    }
                    catch (FormatException ex)
                    {
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new XmlException(SR.Format("InvalidWsatExtendedInfo"), (Exception)ex));
                    }
                    catch (OverflowException ex)
                    {
                        DiagnosticUtility.TraceHandledException((Exception)ex, TraceEventType.Error);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new XmlException(SR.Format("InvalidWsatExtendedInfo"), (Exception)ex));
                    }
                }
                if (!string.IsNullOrEmpty(attribute1) || timeout != 0U)
                    wsatInfo = new WsatExtendedInformation(attribute1, timeout);
            }
            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.OleTxTransaction, XD.OleTxTransactionExternalDictionary.Namespace);
            byte[] propagationToken = OleTxTransactionHeader.ReadPropagationTokenElement(reader);
            while (reader.IsStartElement())
                reader.Skip();
            reader.ReadEndElement();
            return new OleTxTransactionHeader(propagationToken, wsatInfo);
        }

        public static void WritePropagationTokenElement(
            XmlDictionaryWriter writer,
            byte[] propagationToken)
        {
            writer.WriteStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
            writer.WriteBase64(propagationToken, 0, propagationToken.Length);
            writer.WriteEndElement();
        }

        public static bool IsStartPropagationTokenElement(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
        }

        public static byte[] ReadPropagationTokenElement(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement(XD.OleTxTransactionExternalDictionary.PropagationToken, XD.OleTxTransactionExternalDictionary.Namespace);
            byte[] numArray = reader.ReadContentAsBase64();
            if (numArray.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new XmlException(SR.Format("InvalidPropagationToken")));
            reader.ReadEndElement();
            return numArray;
        }
    }
}

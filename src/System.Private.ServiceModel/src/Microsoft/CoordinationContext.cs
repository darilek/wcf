// Decompiled with JetBrains decompiler
// Type: Microsoft.Transactions.Wsat.Messaging.CoordinationContext
// Assembly: Microsoft.Transactions.Bridge, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 4750DD13-8A99-422B-B93B-0421573449B9
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Microsoft.Transactions.Bridge\v4.0_4.0.0.0__b03f5f7f11d50a3a\Microsoft.Transactions.Bridge.dll

//using Microsoft.Transactions.Wsat.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Transactions;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class CoordinationContext : IXmlSerializable
    {
        private IsolationLevel isoLevel = IsolationLevel.Unspecified;
        private Guid localTxId = Guid.Empty;
        private CoordinationStrings coordinationStrings;
        private CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings;
        private AtomicTransactionXmlDictionaryStrings atomicTransactionXmlDictionaryStrings;
        private ProtocolVersion protocolVersion;
        public const int MaxIdentifierLength = 256;
        private string contextId;
        private List<XmlNode> unknownIdentifierAttributes;
        private bool expiresPresent;
        private uint expiration;
        private List<XmlNode> unknownExpiresAttributes;
        private EndpointAddress registrationRef;
        private IsolationFlags isoFlags;
        private string description;
        private byte[] propToken;
        private List<XmlNode> unknownData;
        public const string UuidScheme = "urn:uuid:";

        public CoordinationContext(ProtocolVersion protocolVersion)
        {
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            this.atomicTransactionXmlDictionaryStrings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            this.protocolVersion = protocolVersion;
        }

        public string Identifier
        {
            get
            {
                return this.contextId;
            }
            set
            {
                this.contextId = value;
            }
        }

        public bool ExpiresPresent
        {
            get
            {
                return this.expiresPresent;
            }
        }

        public uint Expires
        {
            get
            {
                return this.expiration;
            }
            set
            {
                this.expiration = value;
                this.expiresPresent = true;
            }
        }

        public EndpointAddress RegistrationService
        {
            get
            {
                return this.registrationRef;
            }
            set
            {
                this.registrationRef = value;
            }
        }

        public IsolationLevel IsolationLevel
        {
            get
            {
                return this.isoLevel;
            }
            set
            {
                this.isoLevel = value;
            }
        }

        public IsolationFlags IsolationFlags
        {
            get
            {
                return this.isoFlags;
            }
            set
            {
                this.isoFlags = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public Guid LocalTransactionId
        {
            get
            {
                return this.localTxId;
            }
            set
            {
                this.localTxId = value;
            }
        }

        public byte[] PropagationToken
        {
            get
            {
                return this.propToken;
            }
            set
            {
                this.propToken = value;
            }
        }

        public ProtocolVersion ProtocolVersion
        {
            get
            {
                return this.protocolVersion;
            }
        }

        public void WriteTo(
          XmlDictionaryWriter writer,
          XmlDictionaryString localName,
          XmlDictionaryString ns)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, localName, ns);
            this.WriteContent(writer);
            writer.WriteEndElement();
        }

        public void WriteContent(XmlDictionaryWriter writer)
        {
            if (this.isoLevel != IsolationLevel.Unspecified || this.localTxId != Guid.Empty)
                writer.WriteXmlnsAttribute("mstx", XD.DotNetAtomicTransactionExternalDictionary.Namespace);
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.Identifier, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.unknownIdentifierAttributes != null)
            {
                foreach (XmlNode identifierAttribute in this.unknownIdentifierAttributes)
                    identifierAttribute.WriteTo((XmlWriter)writer);
            }
            writer.WriteString(this.contextId);
            writer.WriteEndElement();
            if (this.expiresPresent)
            {
                writer.WriteStartElement(this.coordinationXmlDictionaryStrings.Expires, this.coordinationXmlDictionaryStrings.Namespace);
                if (this.unknownExpiresAttributes != null)
                {
                    foreach (XmlNode expiresAttribute in this.unknownExpiresAttributes)
                        expiresAttribute.WriteTo((XmlWriter)writer);
                }
                writer.WriteValue(this.expiration.ToString((IFormatProvider)CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteStartElement(this.coordinationXmlDictionaryStrings.CoordinationType, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteString(this.atomicTransactionXmlDictionaryStrings.Namespace);
            writer.WriteEndElement();
            this.registrationRef.WriteTo(MessagingVersionHelper.AddressingVersion(this.protocolVersion), writer, this.coordinationXmlDictionaryStrings.RegistrationService, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.isoLevel != IsolationLevel.Unspecified)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int)this.isoLevel);
                writer.WriteEndElement();
            }
            if (this.isoFlags != (IsolationFlags)0)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationFlags, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int)this.isoFlags);
                writer.WriteEndElement();
            }
            if (!string.IsNullOrEmpty(this.description))
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.Description, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.description);
                writer.WriteEndElement();
            }
            if (this.localTxId != Guid.Empty)
            {
                writer.WriteStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.localTxId);
                writer.WriteEndElement();
            }
            if (this.propToken != null)
                OleTxTransactionHeader.WritePropagationTokenElement(writer, this.propToken);
            if (this.unknownData == null)
                return;
            int count = this.unknownData.Count;
            for (int index = 0; index < count; ++index)
                this.unknownData[index].WriteTo((XmlWriter)writer);
        }

        public static CoordinationContext ReadFrom(
          XmlDictionaryReader reader,
          XmlDictionaryString localName,
          XmlDictionaryString ns,
          ProtocolVersion protocolVersion)
        {
            CoordinationContext that = new CoordinationContext(protocolVersion);
            CoordinationContext.ReadFrom(that, reader, localName, ns, protocolVersion);
            return that;
        }

        private static void ReadFrom(
          CoordinationContext that,
          XmlDictionaryReader reader,
          XmlDictionaryString localName,
          XmlDictionaryString ns,
          ProtocolVersion protocolVersion)
        {
            try
            {
                CoordinationXmlDictionaryStrings dictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
                AtomicTransactionStrings transactionStrings = AtomicTransactionStrings.Version(protocolVersion);
                reader.ReadFullStartElement(localName, dictionaryStrings.Namespace);
                reader.MoveToStartElement(dictionaryStrings.Identifier, dictionaryStrings.Namespace);
                that.unknownIdentifierAttributes = CoordinationContext.ReadOtherAttributes(reader, dictionaryStrings.Namespace);
                that.contextId = reader.ReadElementContentAsString().Trim();
                if (that.contextId.Length == 0 || that.contextId.Length > 256)
                    throw new InvalidCoordinationContextException("InvalidCoordinationContext");
                if (!Uri.TryCreate(that.contextId, UriKind.Absolute, out Uri _))
                    throw new InvalidCoordinationContextException("InvalidCoordinationContext");
                if (reader.IsStartElement(dictionaryStrings.Expires, dictionaryStrings.Namespace))
                {
                    that.unknownExpiresAttributes = CoordinationContext.ReadOtherAttributes(reader, dictionaryStrings.Namespace);
                    string s = reader.ReadElementContentAsString();
                    try
                    {
                        that.expiration = XmlConvert.ToUInt32(s);
                    }
                    catch (FormatException ex)
                    {
                        throw new InvalidCoordinationContextException("InvalidCoordinationContext", ex);
                    }
                    catch (OverflowException ex)
                    {
                        throw new InvalidCoordinationContextException("InvalidCoordinationContext", ex);
                    }
                    catch (ArgumentNullException ex)
                    {
                        throw new InvalidCoordinationContextException("InvalidCoordinationContext", ex);
                    }
                    that.expiresPresent = true;
                }
                reader.MoveToStartElement(dictionaryStrings.CoordinationType, dictionaryStrings.Namespace);
                if (reader.ReadElementContentAsString().Trim() != transactionStrings.Namespace)
                    throw new InvalidCoordinationContextException("InvalidCoordinationContext");
                that.registrationRef = EndpointAddress.ReadFrom(MessagingVersionHelper.AddressingVersion(protocolVersion), reader, dictionaryStrings.RegistrationService, dictionaryStrings.Namespace);
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    that.isoLevel = (IsolationLevel)reader.ReadElementContentAsInt();
                    if (that.IsolationLevel < IsolationLevel.Serializable || that.IsolationLevel > IsolationLevel.Unspecified || that.IsolationLevel == IsolationLevel.Snapshot)
                        throw new InvalidCoordinationContextException("InvalidCoordinationContext");
                }
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationFlags, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                    that.isoFlags = (IsolationFlags)reader.ReadElementContentAsInt();
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.Description, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                    that.description = reader.ReadElementContentAsString().Trim();
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.LocalTransactionId, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                    that.localTxId = reader.ReadElementContentAsGuid();
                if (OleTxTransactionHeader.IsStartPropagationTokenElement(reader))
                    that.propToken = OleTxTransactionHeader.ReadPropagationTokenElement(reader);
                if (reader.IsStartElement())
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    that.unknownData = new List<XmlNode>(5);
                    while (reader.IsStartElement())
                    {
                        XmlNode xmlNode = xmlDocument.ReadNode((XmlReader)reader);
                        that.unknownData.Add(xmlNode);
                    }
                }
                reader.ReadEndElement();
            }
            catch (XmlException ex)
            {
                throw new InvalidCoordinationContextException("InvalidCoordinationContext", ex);
            }
        }

        private static List<XmlNode> ReadOtherAttributes(
          XmlDictionaryReader reader,
          XmlDictionaryString ns)
        {
            int attributeCount = reader.AttributeCount;
            if (attributeCount == 0)
                return (List<XmlNode>)null;
            XmlDocument xmlDocument = new XmlDocument();
            List<XmlNode> xmlNodeList = new List<XmlNode>(attributeCount);
            reader.MoveToFirstAttribute();
            do
            {
                XmlNode xmlNode = xmlDocument.ReadNode((XmlReader)reader);
                if (xmlNode == null || xmlNode.NamespaceURI == ns.Value)
                    // throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new InvalidCoordinationContextException(Microsoft.Transactions.SR.GetString("InvalidCoordinationContext")));
                    throw new InvalidCoordinationContextException("InvalidCoordinationContext");
                xmlNodeList.Add(xmlNode);
            }
            while (reader.MoveToNextAttribute());
            reader.MoveToElement();
            return xmlNodeList;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            //throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new NotSupportedException());
            throw new NotSupportedException();
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            CoordinationContext.ReadFrom(this, XmlDictionaryReader.CreateDictionaryReader(reader), this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace, this.protocolVersion);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            this.WriteTo(XmlDictionaryWriter.CreateDictionaryWriter(writer), this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace);
        }

        public static string CreateNativeIdentifier(Guid transactionId)
        {
            return "urn:uuid:" + transactionId.ToString("D");
        }

        public static bool IsNativeIdentifier(string identifier, Guid transactionId)
        {
            return string.Compare(identifier, CoordinationContext.CreateNativeIdentifier(transactionId), StringComparison.Ordinal) == 0;
        }
    }
}

namespace Microsoft.Transactions.Wsat.Messaging
{
}

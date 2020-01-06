using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Transactions.Wsat.Messaging;

namespace Microsoft.Transactions.Wsat.Protocol
{
    internal class ProtocolInformationReader
    {
        private int httpsPort;
        private string hostName;
        private string nodeName;
        private string basePath;
        private TimeSpan maxTimeout;
        private ProtocolInformationFlags flags;
        private bool isV10Enabled;
        private bool isV11Enabled;

        public ProtocolInformationReader(MemoryStream mem)
        {
            this.ReadProtocolInformation(mem);
        }

        public int HttpsPort
        {
            get
            {
                return this.httpsPort;
            }
        }

        public string HostName
        {
            get
            {
                return this.hostName;
            }
        }

        public string NodeName
        {
            get
            {
                return this.nodeName;
            }
        }

        public string BasePath
        {
            get
            {
                return this.basePath;
            }
        }

        public TimeSpan MaxTimeout
        {
            get
            {
                return this.maxTimeout;
            }
        }

        public bool IssuedTokensEnabled
        {
            get
            {
                return (uint)(this.flags & ProtocolInformationFlags.IssuedTokensEnabled) > 0U;
            }
        }

        public bool NetworkInboundAccess
        {
            get
            {
                return (uint)(this.flags & ProtocolInformationFlags.NetworkInboundAccess) > 0U;
            }
        }

        public bool NetworkOutboundAccess
        {
            get
            {
                return (uint)(this.flags & ProtocolInformationFlags.NetworkOutboundAccess) > 0U;
            }
        }

        public bool NetworkClientAccess
        {
            get
            {
                return (uint)(this.flags & ProtocolInformationFlags.NetworkClientAccess) > 0U;
            }
        }

        public bool IsClustered
        {
            get
            {
                return (uint)(this.flags & ProtocolInformationFlags.IsClustered) > 0U;
            }
        }

        public bool IsV10Enabled
        {
            get
            {
                return this.isV10Enabled;
            }
        }

        public bool IsV11Enabled
        {
            get
            {
                return this.isV11Enabled;
            }
        }

        private void ReadProtocolInformation(MemoryStream mem)
        {
            ProtocolInformationMajorVersion informationMajorVersion = (ProtocolInformationMajorVersion)mem.ReadByte();
            if (informationMajorVersion != ProtocolInformationMajorVersion.v1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoUnsupportedVersion", (object)informationMajorVersion)));
            ProtocolInformationMinorVersion informationMinorVersion = (ProtocolInformationMinorVersion)mem.ReadByte();
            this.flags = (ProtocolInformationFlags)mem.ReadByte();
            this.CheckFlags(this.flags);
            this.httpsPort = SerializationUtils.ReadInt(mem);
            if (this.httpsPort < 0 || this.httpsPort > (int)ushort.MaxValue)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidHttpsPort", (object)this.httpsPort)));
            this.maxTimeout = SerializationUtils.ReadTimeout(mem);
            if (this.maxTimeout < TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidMaxTimeout", (object)this.maxTimeout)));
            this.hostName = SerializationUtils.ReadString(mem);
            if (string.IsNullOrEmpty(this.hostName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidHostName")));
            this.basePath = SerializationUtils.ReadString(mem);
            if (string.IsNullOrEmpty(this.basePath))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidBasePath")));
            this.nodeName = SerializationUtils.ReadString(mem);
            if (string.IsNullOrEmpty(this.nodeName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidNodeName")));
            byte num = 2;
            if (informationMinorVersion >= (ProtocolInformationMinorVersion)num)
            {
                ProtocolVersion protocolVersion = (ProtocolVersion)SerializationUtils.ReadUShort(mem);
                if ((protocolVersion & ProtocolVersion.Version10) != (ProtocolVersion)0)
                    this.isV10Enabled = true;
                if ((protocolVersion & ProtocolVersion.Version11) == (ProtocolVersion)0)
                    return;
                this.isV11Enabled = true;
            }
            else
                this.isV10Enabled = true;
        }

        private void CheckFlags(ProtocolInformationFlags flags)
        {
            if ((flags | ProtocolInformationFlags.IssuedTokensEnabled | ProtocolInformationFlags.NetworkClientAccess | ProtocolInformationFlags.NetworkInboundAccess | ProtocolInformationFlags.NetworkOutboundAccess | ProtocolInformationFlags.IsClustered) != (ProtocolInformationFlags.IssuedTokensEnabled | ProtocolInformationFlags.NetworkClientAccess | ProtocolInformationFlags.NetworkInboundAccess | ProtocolInformationFlags.NetworkOutboundAccess | ProtocolInformationFlags.IsClustered))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidFlags", (object)flags)));
            if ((flags & (ProtocolInformationFlags.NetworkInboundAccess | ProtocolInformationFlags.NetworkOutboundAccess)) == (ProtocolInformationFlags)0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new SerializationException(Microsoft.Transactions.SR.GetString("ProtocolInfoInvalidFlags", (object)flags)));
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.NetNamedPipeSecurity
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.ComponentModel;
using System.Net.Security;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
    /// <summary>Provides access to the security settings for endpoints configured with the named pipe binding.</summary>
    public sealed class NetNamedPipeSecurity
    {
        private NamedPipeTransportSecurity transport = new NamedPipeTransportSecurity();
        internal const NetNamedPipeSecurityMode DefaultMode = NetNamedPipeSecurityMode.Transport;
        private NetNamedPipeSecurityMode mode;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.NetNamedPipeSecurity" /> class.</summary>
        public NetNamedPipeSecurity()
        {
            this.mode = NetNamedPipeSecurityMode.Transport;
        }

        private NetNamedPipeSecurity(
          NetNamedPipeSecurityMode mode,
          NamedPipeTransportSecurity transport)
        {
            this.mode = mode;
            this.transport = transport == null ? new NamedPipeTransportSecurity() : transport;
        }

        /// <summary>Gets or sets the security mode for the named pipe binding.</summary>
        /// <returns>The value of the <see cref="T:System.ServiceModel.NetNamedPipeSecurityMode" /> for the named pipe binding. The default value is <see cref="F:System.ServiceModel.NetNamedPipeSecurityMode.Transport" />.</returns>
        [DefaultValue(NetNamedPipeSecurityMode.Transport)]
        public NetNamedPipeSecurityMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (!NetNamedPipeSecurityModeHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value)));
                this.mode = value;
            }
        }

        /// <summary>Gets the transport security for the named pipe binding.</summary>
        /// <returns>The value of the <see cref="T:System.ServiceModel.NamedPipeTransportSecurity" /> for the named pipe binding.</returns>
        public NamedPipeTransportSecurity Transport
        {
            get
            {
                return this.transport;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                this.transport = value;
            }
        }

        internal WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            return this.mode == NetNamedPipeSecurityMode.Transport ? this.transport.CreateTransportProtectionAndAuthentication() : (WindowsStreamSecurityBindingElement)null;
        }

        internal static bool TryCreate(
          WindowsStreamSecurityBindingElement wssbe,
          NetNamedPipeSecurityMode mode,
          out NetNamedPipeSecurity security)
        {
            security = (NetNamedPipeSecurity)null;
            NamedPipeTransportSecurity transportSecurity = new NamedPipeTransportSecurity();
            if (mode == NetNamedPipeSecurityMode.Transport && !NamedPipeTransportSecurity.IsTransportProtectionAndAuthentication(wssbe, transportSecurity))
                return false;
            security = new NetNamedPipeSecurity(mode, transportSecurity);
            return true;
        }

        /// <summary>Returns a a value that indicates whether the <see cref="P:System.ServiceModel.NetNamedPipeSecurity.Transport" /> property has changed from its default value and should be serialized. This is used by WCF for XAML integration.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="P:System.ServiceModel.NetNamedPipeSecurity.Transport" /> property value should be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            return this.transport.ProtectionLevel != ProtectionLevel.EncryptAndSign;
        }
    }
}

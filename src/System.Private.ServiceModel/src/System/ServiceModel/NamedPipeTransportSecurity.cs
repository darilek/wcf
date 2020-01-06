// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.NamedPipeTransportSecurity
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.ComponentModel;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace System.ServiceModel
{
    /// <summary>Provides properties that control protection level for a named pipe.</summary>
    public sealed class NamedPipeTransportSecurity
    {
        internal const ProtectionLevel DefaultProtectionLevel = ProtectionLevel.EncryptAndSign;
        private ProtectionLevel protectionLevel;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.NamedPipeTransportSecurity" /> class.</summary>
        public NamedPipeTransportSecurity()
        {
            this.protectionLevel = ProtectionLevel.EncryptAndSign;
        }

        /// <summary>Specifies the protection for the named pipe.</summary>
        /// <returns>A <see cref="T:System.Net.Security.ProtectionLevel" />. The default is <see cref="F:System.Net.Security.ProtectionLevel.EncryptAndSign" />.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value is not a valid <see cref="T:System.Net.Security.ProtectionLevel" />.</exception>
        [DefaultValue(ProtectionLevel.EncryptAndSign)]
        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value)));
                this.protectionLevel = value;
            }
        }

        internal WindowsStreamSecurityBindingElement CreateTransportProtectionAndAuthentication()
        {
            return new WindowsStreamSecurityBindingElement()
            {
                ProtectionLevel = this.protectionLevel
            };
        }

        internal static bool IsTransportProtectionAndAuthentication(
          WindowsStreamSecurityBindingElement wssbe,
          NamedPipeTransportSecurity transportSecurity)
        {
            transportSecurity.protectionLevel = wssbe.ProtectionLevel;
            return true;
        }
    }
}

// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.NetNamedPipeBinding
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net.Security;
using System.Net.WebSockets;
//using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel
{
    /// <summary>Provides a secure and reliable binding that is optimized for on-machine communication.</summary>
    public class NetNamedPipeBinding : Binding, IBindingRuntimePreferences
    {
        private NetNamedPipeSecurity security = new NetNamedPipeSecurity();
        private TransactionFlowBindingElement context;
        private BinaryMessageEncodingBindingElement encoding;
        private NamedPipeTransportBindingElement namedPipe;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.NetNamedPipeBinding" /> class. </summary>
        public NetNamedPipeBinding()
        {
            this.Initialize();
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.NetNamedPipeBinding" /> class with a specified security mode.</summary>
        /// <param name="securityMode">The <see cref="T:System.ServiceModel.NetNamedPipeSecurityMode" /> value that specifies whether Windows security is used with named pipes.</param>
        public NetNamedPipeBinding(NetNamedPipeSecurityMode securityMode)
          : this()
        {
            this.security.Mode = securityMode;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.NetNamedPipeBinding" /> class with a specified configuration name.</summary>
        /// <param name="configurationName">The binding configuration name for the netNamedPipeBinding Element.</param>
        public NetNamedPipeBinding(string configurationName)
          : this()
        {
            //this.ApplyConfiguration(configurationName);
            throw new PlatformNotSupportedException();
        }

        private NetNamedPipeBinding(NetNamedPipeSecurity security)
          : this()
        {
            this.security = security;
        }

        /// <summary>Gets or sets a value that determines whether transactions should be flowed to the service.</summary>
        /// <returns>
        /// <see langword="true" /> if the client’s transactions should be flowed to the service; otherwise, <see langword="false" />. The default is <see langword="false" />.</returns>
        [DefaultValue(false)]
        public bool TransactionFlow
        {
            get
            {
                return this.context.Transactions;
            }
            set
            {
                this.context.Transactions = value;
            }
        }

        /// <summary>Gets or sets the transactions protocol used by the service to flow transactions.</summary>
        /// <returns>The <see cref="T:System.ServiceModel.TransactionProtocol" /> used by the service to flow transactions. The default protocol is <see cref="P:System.ServiceModel.TransactionProtocol.OleTransactions" />.</returns>
        public TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.context.TransactionProtocol;
            }
            set
            {
                this.context.TransactionProtocol = value;
            }
        }

        /// <summary>Gets or sets a value that indicates whether the service configured with the binding uses streamed (in one or both directions) or buffered modes of message transfer.</summary>
        /// <returns>The <see cref="T:System.ServiceModel.TransferMode" /> value that indicates whether the service configured with the binding uses streamed (in one or both directions) or buffered modes of message transfer. The default is <see cref="F:System.ServiceModel.TransferMode.Buffered" />.</returns>
        [DefaultValue(TransferMode.Buffered)]
        public TransferMode TransferMode
        {
            get
            {
                return this.namedPipe.TransferMode;
            }
            set
            {
                this.namedPipe.TransferMode = value;
            }
        }

        /// <summary>Gets or sets a value that indicates whether the hostname is used to reach the service when matching the URI.</summary>
        /// <returns>The <see cref="P:System.ServiceModel.Configuration.WSDualHttpBindingElement.HostNameComparisonMode" /> value that indicates whether the hostname is used to reach the service when matching the URI. The default value is <see cref="F:System.ServiceModel.HostNameComparisonMode.StrongWildcard" />, which ignores the hostname in the match.</returns>
        [DefaultValue(HostNameComparisonMode.StrongWildcard)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.namedPipe.HostNameComparisonMode;
            }
            set
            {
                this.namedPipe.HostNameComparisonMode = value;
            }
        }

        /// <summary>Gets or sets the maximum number of bytes that are used to buffer incoming messages in memory.</summary>
        /// <returns>The maximum number of bytes that are used to buffer incoming messages in memory. The default value is 524,288 bytes.</returns>
        [DefaultValue(524288)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.namedPipe.MaxBufferPoolSize;
            }
            set
            {
                this.namedPipe.MaxBufferPoolSize = value;
            }
        }

        /// <summary>Gets or sets the maximum number of bytes used to buffer incoming messages in memory.  </summary>
        /// <returns>The maximum number of bytes that are used to buffer incoming messages in memory. The default value is 65,536 bytes.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The value set is less than 0.</exception>
        [DefaultValue(65536)]
        public int MaxBufferSize
        {
            get
            {
                return this.namedPipe.MaxBufferSize;
            }
            set
            {
                this.namedPipe.MaxBufferSize = value;
            }
        }

        /// <summary>Gets or sets the maximum number of connections, both inbound and outbound, that are allowed to endpoints configured with the named pipe binding.</summary>
        /// <returns>The maximum number of named pipe connections that are allowed with this binding. The default value is 10.</returns>
        public int MaxConnections
        {
            get
            {
                return this.namedPipe.MaxPendingConnections;
            }
            set
            {
                this.namedPipe.MaxPendingConnections = value;
                this.namedPipe.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = value;
            }
        }

        internal bool IsMaxConnectionsSet
        {
            get
            {
                return this.namedPipe.IsMaxPendingConnectionsSet;
            }
        }

        /// <summary>Gets or sets the maximum size, in bytes, for a received message that is processed by the binding.</summary>
        /// <returns>The maximum size (in bytes) for a received message that is processed by the binding. The default value is 65,536 bytes.</returns>
        [DefaultValue(65536)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.namedPipe.MaxReceivedMessageSize;
            }
            set
            {
                this.namedPipe.MaxReceivedMessageSize = value;
            }
        }

        /// <summary>Gets or sets constraints on the complexity of SOAP messages that can be processed by endpoints configured with this binding.</summary>
        /// <returns>The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> that specifies the complexity constraints on SOAP messages exchanged. The default values for these constraints are provided in the following Remarks section.</returns>
        /// <exception cref="T:System.ArgumentNullException">The value set is <see langword="null" />.</exception>
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.encoding.ReaderQuotas;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                value.CopyTo(this.encoding.ReaderQuotas);
            }
        }

        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets the URI transport scheme for the channels and listeners that are configured with this binding.</summary>
        /// <returns>Returns "net.pipe".</returns>
        public override string Scheme
        {
            get
            {
                return this.namedPipe.Scheme;
            }
        }

        /// <summary>Gets the version of SOAP that is used for messages processed by this binding. </summary>
        /// <returns>
        /// <see cref="P:System.ServiceModel.EnvelopeVersion.Soap12" />.</returns>
        public EnvelopeVersion EnvelopeVersion
        {
            get
            {
                return EnvelopeVersion.Soap12;
            }
        }

        /// <summary>Gets an object that specifies the type of security used with services configured with this binding.</summary>
        /// <returns>The <see cref="T:System.ServiceModel.NetNamedPipeSecurity" /> that is used with this binding. The default value is <see cref="F:System.ServiceModel.NetNamedPipeSecurityMode.Transport" />. </returns>
        public NetNamedPipeSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(value));
                this.security = value;
            }
        }

        private static TransactionFlowBindingElement GetDefaultTransactionFlowBindingElement()
        {
            return new TransactionFlowBindingElement(false);
        }

        private void Initialize()
        {
            this.namedPipe = new NamedPipeTransportBindingElement();
            this.encoding = new BinaryMessageEncodingBindingElement();
            this.context = NetNamedPipeBinding.GetDefaultTransactionFlowBindingElement();
        }

        private void InitializeFrom(
          NamedPipeTransportBindingElement namedPipe,
          BinaryMessageEncodingBindingElement encoding,
          TransactionFlowBindingElement context)
        {
            this.Initialize();
            this.HostNameComparisonMode = namedPipe.HostNameComparisonMode;
            this.MaxBufferPoolSize = namedPipe.MaxBufferPoolSize;
            this.MaxBufferSize = namedPipe.MaxBufferSize;
            if (namedPipe.IsMaxPendingConnectionsSet)
                this.MaxConnections = namedPipe.MaxPendingConnections;
            this.MaxReceivedMessageSize = namedPipe.MaxReceivedMessageSize;
            this.TransferMode = namedPipe.TransferMode;
            this.ReaderQuotas = encoding.ReaderQuotas;
            this.TransactionFlow = context.Transactions;
            this.TransactionProtocol = context.TransactionProtocol;
        }

        private bool IsBindingElementsMatch(
          NamedPipeTransportBindingElement namedPipe,
          BinaryMessageEncodingBindingElement encoding,
          TransactionFlowBindingElement context)
        {
            return this.namedPipe.IsMatch((BindingElement)namedPipe) && this.encoding.IsMatch((BindingElement)encoding) && this.context.IsMatch((BindingElement)context);
        }

     /*   private void ApplyConfiguration(string configurationName)
        {
            NetNamedPipeBindingElement binding = NetNamedPipeBindingCollectionElement.GetBindingCollectionElement().Bindings[(object)configurationName];
            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ConfigurationErrorsException(SR.GetString("ConfigInvalidBindingConfigurationName", (object)configurationName, (object)"netNamedPipeBinding")));
            binding.ApplyConfiguration((Binding)this);
        }*/

        /// <summary>Creates a collection with the binding elements for the binding.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.ICollection`1" /> of type <see cref="T:System.ServiceModel.Channels.BindingElement" /> that makes up the binding.</returns>
        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elementCollection = new BindingElementCollection();
            elementCollection.Add((BindingElement)this.context);
            elementCollection.Add((BindingElement)this.encoding);
            WindowsStreamSecurityBindingElement transportSecurity = this.CreateTransportSecurity();
            if (transportSecurity != null)
                elementCollection.Add((BindingElement)transportSecurity);
            elementCollection.Add((BindingElement)this.namedPipe);
            return elementCollection.Clone();
        }

        internal static bool TryCreate(BindingElementCollection elements, out Binding binding)
        {
            binding = (Binding)null;
            if (elements.Count > 4)
                return false;
            TransactionFlowBindingElement context = (TransactionFlowBindingElement)null;
            BinaryMessageEncodingBindingElement encoding = (BinaryMessageEncodingBindingElement)null;
            WindowsStreamSecurityBindingElement wssbe = (WindowsStreamSecurityBindingElement)null;
            NamedPipeTransportBindingElement namedPipe = (NamedPipeTransportBindingElement)null;
            foreach (BindingElement element in (Collection<BindingElement>)elements)
            {
                switch (element)
                {
                    case TransactionFlowBindingElement _:
                        context = element as TransactionFlowBindingElement;
                        continue;
                    case BinaryMessageEncodingBindingElement _:
                        encoding = element as BinaryMessageEncodingBindingElement;
                        continue;
                    case WindowsStreamSecurityBindingElement _:
                        wssbe = element as WindowsStreamSecurityBindingElement;
                        continue;
                    case NamedPipeTransportBindingElement _:
                        namedPipe = element as NamedPipeTransportBindingElement;
                        continue;
                    default:
                        return false;
                }
            }
            if (namedPipe == null || encoding == null)
                return false;
            if (context == null)
                context = NetNamedPipeBinding.GetDefaultTransactionFlowBindingElement();
            NetNamedPipeSecurity security;
            if (!NetNamedPipeBinding.TryCreateSecurity(wssbe, out security))
                return false;
            NetNamedPipeBinding namedPipeBinding = new NetNamedPipeBinding(security);
            namedPipeBinding.InitializeFrom(namedPipe, encoding, context);
            if (!namedPipeBinding.IsBindingElementsMatch(namedPipe, encoding, context))
                return false;
            binding = (Binding)namedPipeBinding;
            return true;
        }

        private WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            return this.security.Mode == NetNamedPipeSecurityMode.Transport ? this.security.CreateTransportSecurity() : (WindowsStreamSecurityBindingElement)null;
        }

        private static bool TryCreateSecurity(
          WindowsStreamSecurityBindingElement wssbe,
          out NetNamedPipeSecurity security)
        {
            NetNamedPipeSecurityMode mode = wssbe == null ? NetNamedPipeSecurityMode.None : NetNamedPipeSecurityMode.Transport;
            return NetNamedPipeSecurity.TryCreate(wssbe, mode, out security);
        }

        /// <summary>Returns a value that indicates whether the <see cref="P:System.ServiceModel.NetNamedPipeBinding.ReaderQuotas" /> property has changed from its default value and should be serialized. This is used by WCF for XAML integration.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="P:System.ServiceModel.NetNamedPipeBinding.ReaderQuotas" /> property value should be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return !EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas);
        }

        /// <summary>Returns a value that indicates whether the <see cref="P:System.ServiceModel.NetNamedPipeBinding.Security" /> property has changed from its default value and should be serialized. This is used by WCF for XAML integration.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="P:System.ServiceModel.NetNamedPipeBinding.Security" /> property value should be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            return this.security.Mode != NetNamedPipeSecurityMode.Transport || this.security.Transport.ProtectionLevel != ProtectionLevel.EncryptAndSign;
        }

        /// <summary>Returns a value that indicates whether the <see cref="P:System.ServiceModel.NetNamedPipeBinding.TransactionProtocol" /> property has changed from its default value and should be serialized. This is used by WCF for XAML integration.</summary>
        /// <returns>
        /// <see langword="true" /> if the <see cref="P:System.ServiceModel.NetNamedPipeBinding.TransactionProtocol" /> property value should be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return this.TransactionProtocol != NetTcpDefaults.TransactionProtocol;
        }

        /// <summary>Returns a value that indicates whether the <see cref="P:System.ServiceModel.NetNamedPipeBinding.MaxConnections" /> property has changed from its default value and should be serialized.</summary>
        /// <returns>
        /// <see langword="True" /> if the <see cref="P:System.ServiceModel.NetNamedPipeBinding.MaxConnections" /> property value should be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMaxConnections()
        {
            return this.namedPipe.ShouldSerializeMaxPendingConnections();
        }
    }
}

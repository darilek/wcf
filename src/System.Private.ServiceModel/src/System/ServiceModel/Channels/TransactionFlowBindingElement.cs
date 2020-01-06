// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Channels.TransactionFlowBindingElement
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;

namespace System.ServiceModel.Channels
{
    /// <summary>Represents the configuration element that specifies transaction flow support for a binding. This class cannot be inherited.</summary>
    public sealed class TransactionFlowBindingElement : BindingElement //, IPolicyExportExtension
    {
        private bool transactions;
        private TransactionFlowOption issuedTokens;
        private TransactionProtocol transactionProtocol;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.Channels.TransactionFlowBindingElement" /> class.</summary>
        public TransactionFlowBindingElement()
          : this(true, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.Channels.TransactionFlowBindingElement" /> class with the specified protocol that is used to flow a transaction.</summary>
        /// <param name="transactionProtocol">A <see cref="T:System.ServiceModel.TransactionProtocol" /> value that contains the transaction protocol used in flowing a transaction.</param>
        public TransactionFlowBindingElement(TransactionProtocol transactionProtocol)
          : this(true, transactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(bool transactions)
          : this(transactions, TransactionFlowDefaults.TransactionProtocol)
        {
        }

        internal TransactionFlowBindingElement(
          bool transactions,
          TransactionProtocol transactionProtocol)
        {
            this.transactions = transactions;
            this.issuedTokens = transactions ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;
            if (!TransactionProtocol.IsDefined(transactionProtocol))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.Format("ConfigInvalidTransactionFlowProtocolValue", (object)transactionProtocol.ToString()));
            this.transactionProtocol = transactionProtocol;
        }

        private TransactionFlowBindingElement(TransactionFlowBindingElement elementToBeCloned)
          : base((BindingElement)elementToBeCloned)
        {
            this.transactions = elementToBeCloned.transactions;
            this.issuedTokens = elementToBeCloned.issuedTokens;
            if (!TransactionProtocol.IsDefined(elementToBeCloned.transactionProtocol))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.Format("ConfigInvalidTransactionFlowProtocolValue", (object)elementToBeCloned.transactionProtocol.ToString()));
            this.transactionProtocol = elementToBeCloned.transactionProtocol;
            this.AllowWildcardAction = elementToBeCloned.AllowWildcardAction;
        }

        internal bool Transactions
        {
            get
            {
                return this.transactions;
            }
            set
            {
                this.transactions = value;
                this.issuedTokens = value ? TransactionFlowOption.Allowed : TransactionFlowOption.NotAllowed;
            }
        }

        internal TransactionFlowOption IssuedTokens
        {
            get
            {
                return this.issuedTokens;
            }
            set
            {
                TransactionFlowBindingElement.ValidateOption(value);
                this.issuedTokens = value;
            }
        }

        /// <summary>Creates a duplicate of this element.</summary>
        /// <returns>A <see cref="T:System.ServiceModel.Channels.BindingElement" /> that is a duplicate of this element.</returns>
        public override BindingElement Clone()
        {
            return (BindingElement)new TransactionFlowBindingElement(this);
        }

        private bool IsFlowEnabled(
          Dictionary<DirectionalAction, TransactionFlowOption> dictionary)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
                return true;
            if (!this.transactions)
                return false;
            foreach (TransactionFlowOption transactionFlowOption in dictionary.Values)
            {
                if (transactionFlowOption != TransactionFlowOption.NotAllowed)
                    return true;
            }
            return false;
        }

        internal bool IsFlowEnabled(ContractDescription contract)
        {
            if (this.issuedTokens != TransactionFlowOption.NotAllowed)
                return true;
            if (!this.transactions)
                return false;
            foreach (OperationDescription operation in (Collection<OperationDescription>)contract.Operations)
            {
                TransactionFlowAttribute transactionFlowAttribute = operation.Behaviors.Find<TransactionFlowAttribute>();
                if (transactionFlowAttribute != null && transactionFlowAttribute.Transactions != TransactionFlowOption.NotAllowed)
                    return true;
            }
            return false;
        }

        /// <summary>Gets or sets the transaction protocol used in flowing a transaction.</summary>
        /// <returns>A <see cref="T:System.ServiceModel.TransactionProtocol" /> that specifies the transaction protocol to be used for transaction flow. The default is <see cref="P:System.ServiceModel.TransactionProtocol.OleTransactions" />.</returns>
        public TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!TransactionProtocol.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentOutOfRangeException(nameof(value)));
                this.transactionProtocol = value;
            }
        }

        /// <summary>Gets or sets whether wildcard action is set to be allowed on a transaction flow binding element to be cloned.</summary>
        /// <returns>
        /// <see langword="true" /> if wildcard action is allowed; otherwise, <see langword="false" />. </returns>
        [DefaultValue(false)]
        public bool AllowWildcardAction { get; set; }

        internal static void ValidateOption(TransactionFlowOption opt)
        {
            if (!TransactionFlowOptionHelper.IsDefined(opt))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(SR.Format("TransactionFlowBadOption")));
        }

        /// <summary>Returns whether the transaction protocol used for transaction flow can be serialized.</summary>
        /// <returns>
        /// <see langword="true" /> if the transaction protocol can be serialized; otherwise, <see langword="false" />.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransactionProtocol()
        {
            return this.TransactionProtocol != TransactionProtocol.Default;
        }

        /// <summary>Returns a value that indicates whether the specified binding context can build a channel factory for producing channels of a specified type.</summary>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that should be used to determine if a channel factory can be built.</param>
        /// <typeparam name="TChannel">The channel type.</typeparam>
        /// <returns>
        /// <see langword="true" /> if a channel factory for the specified type of channel can be built from <paramref name="context" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.</exception>
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentNullException(nameof(context)));
            return (typeof(TChannel) == typeof(IOutputChannel) || typeof(TChannel) == typeof(IDuplexChannel) || (typeof(TChannel) == typeof(IRequestChannel) || typeof(TChannel) == typeof(IOutputSessionChannel)) || (typeof(TChannel) == typeof(IRequestSessionChannel) || typeof(TChannel) == typeof(IDuplexSessionChannel))) && context.CanBuildInnerChannelFactory<TChannel>();
        }

        /// <summary>Initializes a channel factory for producing channels of a specified type from a binding context.</summary>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that should be used to build the channel factory.</param>
        /// <typeparam name="TChannel">The channel type.</typeparam>
        /// <returns>The <see cref="T:System.ServiceModel.Channels.IChannelFactory`1" /> of type <paramref name="TChannel" /> initialized from <paramref name="context" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">A channel factory for the specified channel type cannot be built.</exception>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(
          BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(context));
            if (!this.CanBuildChannelFactory<TChannel>(context))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(TChannel), SR.Format("ChannelTypeNotSupported", (object)typeof(TChannel)));
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = this.GetDictionary(context);
            if (!this.IsFlowEnabled(dictionary))
                return context.BuildInnerChannelFactory<TChannel>();
            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new InvalidOperationException(SR.Format("TransactionFlowRequiredIssuedTokens")));
            
            throw new PlatformNotSupportedException("Transactions are not supported");

          /*  return (IChannelFactory<TChannel>)new TransactionChannelFactory<TChannel>(this.transactionProtocol, context, dictionary, this.AllowWildcardAction)
            {
                FlowIssuedTokens = this.IssuedTokens
            };*/
        }

     /*   /// <summary>Initializes a channel listener for accepting channels of a specified type from the binding context.</summary>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that should be used to build the channel listener.</param>
        /// <typeparam name="TChannel">The channel type.</typeparam>
        /// <returns>The <see cref="T:System.ServiceModel.Channels.IChannelListener`1" /> of type <paramref name="TChannel" /> initialized from <paramref name="context" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException">A channel listener for the specified channel type cannot be built.</exception>
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(
          BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentNullException(nameof(context)));
            if (!context.CanBuildInnerChannelListener<TChannel>())
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(nameof(TChannel), SR.Format("ChannelTypeNotSupported", (object)typeof(TChannel)));
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = this.GetDictionary(context);
            if (!this.IsFlowEnabled(dictionary))
                return context.BuildInnerChannelListener<TChannel>();
            if (this.issuedTokens == TransactionFlowOption.NotAllowed)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new InvalidOperationException(SR.Format("TransactionFlowRequiredIssuedTokens")));
            IChannelListener<TChannel> innerListener = context.BuildInnerChannelListener<TChannel>();
            return (IChannelListener<TChannel>)new TransactionChannelListener<TChannel>(this.transactionProtocol, (IDefaultCommunicationTimeouts)context.Binding, dictionary, innerListener)
            {
                FlowIssuedTokens = this.IssuedTokens
            };
        }

        /// <summary>Returns a value that indicates whether the specified binding context can build a channel listener for accepting channels of a specified type.</summary>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that should be used to determine if a channel listener can be built.</param>
        /// <typeparam name="TChannel">The channel type.</typeparam>
        /// <returns>
        /// <see langword="true" /> if a channel listener for the specified type of channel can be built from <paramref name="context" />; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.</exception>
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (!context.CanBuildInnerChannelListener<TChannel>())
                return false;
            return typeof(TChannel) == typeof(IInputChannel) || typeof(TChannel) == typeof(IReplyChannel) || (typeof(TChannel) == typeof(IDuplexChannel) || typeof(TChannel) == typeof(IInputSessionChannel)) || typeof(TChannel) == typeof(IReplySessionChannel) || typeof(TChannel) == typeof(IDuplexSessionChannel);
        }*/

        private Dictionary<DirectionalAction, TransactionFlowOption> GetDictionary(
          BindingContext context)
        {
            return context.BindingParameters.Find<Dictionary<DirectionalAction, TransactionFlowOption>>() ?? new Dictionary<DirectionalAction, TransactionFlowOption>();
        }

        internal static MessagePartSpecification GetIssuedTokenHeaderSpecification(
          SecurityStandardsManager standardsManager)
        {
            if (!standardsManager.TrustDriver.IsIssuedTokensSupported)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new InvalidOperationException(SR.Format("TrustDriverVersionDoesNotSupportIssuedTokens")));
            return new MessagePartSpecification(new XmlQualifiedName[1]
            {
        new XmlQualifiedName(standardsManager.TrustDriver.IssuedTokensHeaderName, standardsManager.TrustDriver.IssuedTokensHeaderNamespace)
            });
        }

        /// <summary>Gets the typed object requested, if present, from the appropriate layer in the binding stack.</summary>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" />that should be used to get the requested property.</param>
        /// <typeparam name="T">The typed object for which the method is querying.</typeparam>
        /// <returns>The typed object <paramref name="T" /> requested if it is present; <see langword="null" /> if it is not. </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="context" /> is <see langword="null" />.</exception>
        public override T GetProperty<T>(BindingContext context)
        {
            if (!(typeof(T) == typeof(ChannelProtectionRequirements)))
                return context.GetInnerProperty<T>();
            ChannelProtectionRequirements protectionRequirements = this.GetProtectionRequirements();
            if (protectionRequirements == null)
                return (T)(object)context.GetInnerProperty<ChannelProtectionRequirements>();
            protectionRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
            return (T)(object)protectionRequirements;
        }

        private ChannelProtectionRequirements GetProtectionRequirements()
        {
            if (!this.Transactions && this.IssuedTokens == TransactionFlowOption.NotAllowed)
                return (ChannelProtectionRequirements)null;
            ChannelProtectionRequirements protectionRequirements = new ChannelProtectionRequirements();
            if (this.Transactions)
            {
                MessagePartSpecification parts = new MessagePartSpecification(new XmlQualifiedName[3]
                {
          new XmlQualifiedName("CoordinationContext", "http://schemas.xmlsoap.org/ws/2004/10/wscoor"),
          new XmlQualifiedName("CoordinationContext", "http://docs.oasis-open.org/ws-tx/wscoor/2006/06"),
          new XmlQualifiedName("OleTxTransaction", "http://schemas.microsoft.com/ws/2006/02/tx/oletx")
                });
                parts.MakeReadOnly();
                protectionRequirements.IncomingSignatureParts.AddParts(parts);
                protectionRequirements.OutgoingSignatureParts.AddParts(parts);
                protectionRequirements.IncomingEncryptionParts.AddParts(parts);
                protectionRequirements.OutgoingEncryptionParts.AddParts(parts);
            }
            if (this.IssuedTokens != TransactionFlowOption.NotAllowed)
            {
                MessagePartSpecification headerSpecification = TransactionFlowBindingElement.GetIssuedTokenHeaderSpecification(SecurityStandardsManager.DefaultInstance);
                headerSpecification.MakeReadOnly();
                protectionRequirements.IncomingSignatureParts.AddParts(headerSpecification);
                protectionRequirements.IncomingEncryptionParts.AddParts(headerSpecification);
                protectionRequirements.OutgoingSignatureParts.AddParts(headerSpecification);
                protectionRequirements.OutgoingEncryptionParts.AddParts(headerSpecification);
            }
            MessagePartSpecification parts1 = new MessagePartSpecification(true);
            parts1.MakeReadOnly();
            protectionRequirements.OutgoingSignatureParts.AddParts(parts1, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault");
            protectionRequirements.OutgoingEncryptionParts.AddParts(parts1, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/transactions/fault");
            return protectionRequirements;
        }

        private XmlElement GetAssertion(
          XmlDocument doc,
          TransactionFlowOption option,
          string prefix,
          string name,
          string ns,
          string policyNs)
        {
            if (doc == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(doc));
            XmlElement xmlElement = (XmlElement)null;
            switch (option)
            {
                case TransactionFlowOption.Allowed:
                    xmlElement = doc.CreateElement(prefix, name, ns);
                    XmlAttribute attribute1 = doc.CreateAttribute("wsp", "Optional", policyNs);
                    attribute1.Value = "true";
                    xmlElement.Attributes.Append(attribute1);
                    if (this.transactionProtocol == TransactionProtocol.OleTransactions || this.transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
                    {
                        XmlAttribute attribute2 = doc.CreateAttribute("wsp1", "Optional", "http://schemas.xmlsoap.org/ws/2002/12/policy");
                        attribute2.Value = "true";
                        xmlElement.Attributes.Append(attribute2);
                        break;
                    }
                    break;
                case TransactionFlowOption.Mandatory:
                    xmlElement = doc.CreateElement(prefix, name, ns);
                    break;
            }
            return xmlElement;
        }

      /*  void IPolicyExportExtension.ExportPolicy(
          MetadataExporter exporter,
          PolicyConversionContext context)
        {
            if (exporter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(exporter));
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(context));
            TransactionFlowBindingElement flowBindingElement = context.BindingElements.Find<TransactionFlowBindingElement>();
            if (flowBindingElement == null || !flowBindingElement.Transactions)
                return;
            XmlDocument doc = new XmlDocument();
            XmlElement xmlElement = (XmlElement)null;
            foreach (OperationDescription operation in (Collection<OperationDescription>)context.Contract.Operations)
            {
                TransactionFlowAttribute transactionFlowAttribute = operation.Behaviors.Find<TransactionFlowAttribute>();
                TransactionFlowOption option = transactionFlowAttribute == null ? TransactionFlowOption.NotAllowed : transactionFlowAttribute.Transactions;
                if (flowBindingElement.TransactionProtocol == TransactionProtocol.OleTransactions)
                    xmlElement = this.GetAssertion(doc, option, "oletx", "OleTxAssertion", "http://schemas.microsoft.com/ws/2006/02/tx/oletx", exporter.PolicyVersion.Namespace);
                else if (flowBindingElement.TransactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004)
                    xmlElement = this.GetAssertion(doc, option, "wsat", "ATAssertion", "http://schemas.xmlsoap.org/ws/2004/10/wsat", exporter.PolicyVersion.Namespace);
                else if (flowBindingElement.TransactionProtocol == TransactionProtocol.WSAtomicTransaction11)
                    xmlElement = this.GetAssertion(doc, option, "wsat", "ATAssertion", "http://docs.oasis-open.org/ws-tx/wsat/2006/06", exporter.PolicyVersion.Namespace);
                if (xmlElement != null)
                    context.GetOperationBindingAssertions(operation).Add(xmlElement);
            }
        }*/

        internal override bool IsMatch(BindingElement b)
        {
            return b != null && b is TransactionFlowBindingElement flowBindingElement && (this.transactions == flowBindingElement.transactions && this.issuedTokens == flowBindingElement.issuedTokens) && this.transactionProtocol == flowBindingElement.transactionProtocol;
        }
    }
}

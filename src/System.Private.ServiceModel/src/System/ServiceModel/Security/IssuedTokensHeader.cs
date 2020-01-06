// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Security.IssuedTokensHeader
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Security
{
    internal sealed class IssuedTokensHeader : MessageHeader
    {
        private ReadOnlyCollection<RequestSecurityTokenResponse> tokenIssuances;
        private SecurityStandardsManager standardsManager;
        private string actor;
        private bool mustUnderstand;
        private bool relay;
        private bool isRefParam;

        public IssuedTokensHeader(
          RequestSecurityTokenResponse tokenIssuance,
          MessageSecurityVersion version,
          SecurityTokenSerializer tokenSerializer)
          : this(tokenIssuance, new SecurityStandardsManager(version, tokenSerializer))
        {
        }

        public IssuedTokensHeader(
          RequestSecurityTokenResponse tokenIssuance,
          SecurityStandardsManager standardsManager)
        {
            if (tokenIssuance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(tokenIssuance));
            this.Initialize(new Collection<RequestSecurityTokenResponse>()
      {
        tokenIssuance
      }, standardsManager);
        }

        public IssuedTokensHeader(
          IEnumerable<RequestSecurityTokenResponse> tokenIssuances,
          SecurityStandardsManager standardsManager)
        {
            if (tokenIssuances == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(tokenIssuances));
            int num = 0;
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse>();
            foreach (RequestSecurityTokenResponse tokenIssuance in tokenIssuances)
            {
                if (tokenIssuance == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "tokenIssuances[{0}]", new object[1]
                    {
            (object) num
                    }));
                coll.Add(tokenIssuance);
                ++num;
            }
            this.Initialize(coll, standardsManager);
        }

        private void Initialize(
          Collection<RequestSecurityTokenResponse> coll,
          SecurityStandardsManager standardsManager)
        {
            if (standardsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentNullException(nameof(standardsManager)));
            this.standardsManager = standardsManager;
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>((IList<RequestSecurityTokenResponse>)coll);
            this.actor = base.Actor;
            this.mustUnderstand = base.MustUnderstand;
            this.relay = base.Relay;
        }

        public IssuedTokensHeader(
          XmlReader xmlReader,
          MessageVersion version,
          SecurityStandardsManager standardsManager)
        {
            if (xmlReader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(xmlReader));
            if (standardsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentNullException(nameof(standardsManager)));
            this.standardsManager = standardsManager;
            XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            MessageHeader.GetHeaderAttributes(dictionaryReader, version, out this.actor, out this.mustUnderstand, out this.relay, out this.isRefParam);
            dictionaryReader.ReadStartElement(this.Name, this.Namespace);
            Collection<RequestSecurityTokenResponse> collection = new Collection<RequestSecurityTokenResponse>();
            if (this.standardsManager.TrustDriver.IsAtRequestSecurityTokenResponseCollection((XmlReader)dictionaryReader))
            {
                foreach (RequestSecurityTokenResponse rstr in this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection((XmlReader)dictionaryReader).RstrCollection)
                    collection.Add(rstr);
            }
            else
            {
                RequestSecurityTokenResponse securityTokenResponse = this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponse((XmlReader)dictionaryReader);
                collection.Add(securityTokenResponse);
            }
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>((IList<RequestSecurityTokenResponse>)collection);
            dictionaryReader.ReadEndElement();
        }

        public ReadOnlyCollection<RequestSecurityTokenResponse> TokenIssuances
        {
            get
            {
                return this.tokenIssuances;
            }
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return this.isRefParam;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.relay;
            }
        }

        public override string Name
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderName;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderNamespace;
            }
        }

        protected override void OnWriteHeaderContents(
          XmlDictionaryWriter writer,
          MessageVersion messageVersion)
        {
            if (this.tokenIssuances.Count == 1)
                this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponse(this.tokenIssuances[0], (XmlWriter)writer);
            else
                new RequestSecurityTokenResponseCollection((IEnumerable<RequestSecurityTokenResponse>)this.tokenIssuances, this.standardsManager).WriteTo((XmlWriter)writer);
        }

        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(
          Message message,
          MessageSecurityVersion version,
          WSSecurityTokenSerializer tokenSerializer,
          string[] actors,
          XmlQualifiedName expectedAppliesToQName)
        {
            return IssuedTokensHeader.ExtractIssuances(message, new SecurityStandardsManager(version, (SecurityTokenSerializer)tokenSerializer), actors, expectedAppliesToQName);
        }

        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(
          Message message,
          SecurityStandardsManager standardsManager,
          string[] actors,
          XmlQualifiedName expectedAppliesToQName)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(nameof(message));
            if (standardsManager == null)
                standardsManager = SecurityStandardsManager.DefaultInstance;
            if (actors == null)
                throw TraceUtility.ThrowHelperArgumentNull(nameof(actors), message);
            Collection<RequestSecurityTokenResponse> collection = new Collection<RequestSecurityTokenResponse>();
            for (int headerIndex = 0; headerIndex < message.Headers.Count; ++headerIndex)
            {
                if (message.Headers[headerIndex].Name == standardsManager.TrustDriver.IssuedTokensHeaderName && message.Headers[headerIndex].Namespace == standardsManager.TrustDriver.IssuedTokensHeaderNamespace)
                {
                    bool flag1 = false;
                    for (int index = 0; index < actors.Length; ++index)
                    {
                        if (actors[index] == message.Headers[headerIndex].Actor)
                        {
                            flag1 = true;
                            break;
                        }
                    }
                    if (flag1)
                    {
                        IssuedTokensHeader issuedTokensHeader = new IssuedTokensHeader((XmlReader)message.Headers.GetReaderAtHeader(headerIndex), message.Version, standardsManager);
                        for (int index = 0; index < issuedTokensHeader.TokenIssuances.Count; ++index)
                        {
                            bool flag2;
                            if (expectedAppliesToQName != (XmlQualifiedName)null)
                            {
                                string localName;
                                string namespaceUri;
                                issuedTokensHeader.TokenIssuances[index].GetAppliesToQName(out localName, out namespaceUri);
                                flag2 = localName == expectedAppliesToQName.Name && namespaceUri == expectedAppliesToQName.Namespace;
                            }
                            else
                                flag2 = true;
                            if (flag2)
                                collection.Add(issuedTokensHeader.TokenIssuances[index]);
                        }
                    }
                }
            }
            return collection;
        }
    }
}

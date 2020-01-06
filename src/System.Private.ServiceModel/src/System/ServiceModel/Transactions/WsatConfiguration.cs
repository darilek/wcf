using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.ServiceModel.Channels;
using System.Transactions;
using Microsoft.Transactions.Wsat.Messaging;
using Microsoft.Transactions.Wsat.Protocol;

namespace System.ServiceModel.Transactions
{
    internal class WsatConfiguration
    {
        private static readonly string DisabledRegistrationPath = "WsatService" + "/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10) + "Disabled/";
        private const string WsatKey = "Software\\Microsoft\\WSAT\\3.0";
        private const string OleTxUpgradeEnabledValue = "OleTxUpgradeEnabled";
        private const bool OleTxUpgradeEnabledDefault = true;
        private bool oleTxUpgradeEnabled;
        private EndpointAddress localActivationService10;
        private EndpointAddress localActivationService11;
        private EndpointAddress remoteActivationService10;
        private EndpointAddress remoteActivationService11;
        private Uri registrationServiceAddress10;
        private Uri registrationServiceAddress11;
        private bool protocolService10Enabled;
        private bool protocolService11Enabled;
        private bool inboundEnabled;
        private bool issuedTokensEnabled;
        private TimeSpan maxTimeout;

        public WsatConfiguration()
        {
            WhereaboutsReader whereabouts = this.GetWhereabouts();
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if (protocolInformation != null)
            {
                this.protocolService10Enabled = protocolInformation.IsV10Enabled;
                this.protocolService11Enabled = protocolInformation.IsV11Enabled;
            }
            this.Initialize(whereabouts);
            this.oleTxUpgradeEnabled = WsatConfiguration.ReadFlag("Software\\Microsoft\\WSAT\\3.0", nameof(OleTxUpgradeEnabled), true);
        }

        private void Initialize(WhereaboutsReader whereabouts)
        {
            try
            {
                this.InitializeForUnmarshal(whereabouts);
                this.InitializeForMarshal(whereabouts);
            }
            catch (UriFormatException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatUriCreationFailed"), (Exception)ex));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatUriCreationFailed"), (Exception)ex));
            }
        }

        public bool OleTxUpgradeEnabled
        {
            get
            {
                return this.oleTxUpgradeEnabled;
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
                return this.issuedTokensEnabled;
            }
        }

        public bool InboundEnabled
        {
            get
            {
                return this.inboundEnabled;
            }
        }

        public bool IsProtocolServiceEnabled(ProtocolVersion protocolVersion)
        {
            if (protocolVersion == ProtocolVersion.Version10)
                return this.protocolService10Enabled;
            if (protocolVersion == ProtocolVersion.Version11)
                return this.protocolService11Enabled;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public EndpointAddress LocalActivationService(ProtocolVersion protocolVersion)
        {
            if (protocolVersion == ProtocolVersion.Version10)
                return this.localActivationService10;
            if (protocolVersion == ProtocolVersion.Version11)
                return this.localActivationService11;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public EndpointAddress RemoteActivationService(ProtocolVersion protocolVersion)
        {
            if (protocolVersion == ProtocolVersion.Version10)
                return this.remoteActivationService10;
            if (protocolVersion == ProtocolVersion.Version11)
                return this.remoteActivationService11;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public EndpointAddress CreateRegistrationService(
            AddressHeader refParam,
            ProtocolVersion protocolVersion)
        {
            if (protocolVersion != ProtocolVersion.Version10)
            {
                if (protocolVersion != ProtocolVersion.Version11)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
                return new EndpointAddress(this.registrationServiceAddress11, new AddressHeader[1]
                {
                    refParam
                });
            }
            return new EndpointAddress(this.registrationServiceAddress10, new AddressHeader[1]
            {
                refParam
            });
        }

        public bool IsLocalRegistrationService(
            EndpointAddress endpoint,
            ProtocolVersion protocolVersion)
        {
            if (endpoint.Uri == (Uri)null)
                return false;
            if (protocolVersion == ProtocolVersion.Version10)
                return endpoint.Uri == this.registrationServiceAddress10;
            if (protocolVersion == ProtocolVersion.Version11)
                return endpoint.Uri == this.registrationServiceAddress11;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new ArgumentException(System.ServiceModel.SR.GetString("InvalidWsatProtocolVersion")));
        }

        public bool IsDisabledRegistrationService(EndpointAddress endpoint)
        {
            return endpoint.Uri.AbsolutePath == WsatConfiguration.DisabledRegistrationPath;
        }

        private WhereaboutsReader GetWhereabouts()
        {
            try
            {
                return new WhereaboutsReader(TransactionInterop.GetWhereabouts());
            }
            catch (SerializationException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WhereaboutsReadFailed"), (Exception)ex));
            }
        }

        private void InitializeForUnmarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if (protocolInformation == null || !protocolInformation.NetworkInboundAccess)
                return;
            this.inboundEnabled = true;
            bool flag = string.Compare(Environment.MachineName, protocolInformation.NodeName, StringComparison.OrdinalIgnoreCase) == 0;
            string suffix1 = BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version10);
            string suffix2 = BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version11);
            if (protocolInformation.IsClustered || protocolInformation.NetworkClientAccess && !flag)
            {
                string spnIdentity = !protocolInformation.IsClustered ? "host/" + protocolInformation.HostName : (string)null;
                if (protocolInformation.IsV10Enabled)
                    this.remoteActivationService10 = this.CreateActivationEndpointAddress(protocolInformation, suffix1, spnIdentity, true);
                if (protocolInformation.IsV11Enabled)
                    this.remoteActivationService11 = this.CreateActivationEndpointAddress(protocolInformation, suffix2, spnIdentity, true);
            }
            if (!flag)
                return;
            string spnIdentity1 = "host/" + protocolInformation.NodeName;
            if (protocolInformation.IsV10Enabled)
                this.localActivationService10 = this.CreateActivationEndpointAddress(protocolInformation, suffix1, spnIdentity1, false);
            if (!protocolInformation.IsV11Enabled)
                return;
            this.localActivationService11 = this.CreateActivationEndpointAddress(protocolInformation, suffix2, spnIdentity1, false);
        }

        private EndpointAddress CreateActivationEndpointAddress(
            ProtocolInformationReader protocol,
            string suffix,
            string spnIdentity,
            bool isRemote)
        {
            string scheme;
            string host;
            int port;
            string pathValue;
            if (isRemote)
            {
                scheme = Uri.UriSchemeHttps;
                host = protocol.HostName;
                port = protocol.HttpsPort;
                pathValue = protocol.BasePath + "/" + suffix + "Remote/";
            }
            else
            {
                scheme = Uri.UriSchemeNetPipe;
                host = "localhost";
                port = -1;
                pathValue = protocol.HostName + "/" + protocol.BasePath + "/" + suffix;
            }
            UriBuilder uriBuilder = new UriBuilder(scheme, host, port, pathValue);
            if (spnIdentity == null)
                return new EndpointAddress(uriBuilder.Uri, new AddressHeader[0]);
            EndpointIdentity spnIdentity1 = EndpointIdentity.CreateSpnIdentity(spnIdentity);
            return new EndpointAddress(uriBuilder.Uri, spnIdentity1, new AddressHeader[0]);
        }

        private void InitializeForMarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocolInformation = whereabouts.ProtocolInformation;
            if (protocolInformation != null && protocolInformation.NetworkOutboundAccess)
            {
                if (protocolInformation.IsV10Enabled)
                    this.registrationServiceAddress10 = new UriBuilder(Uri.UriSchemeHttps, protocolInformation.HostName, protocolInformation.HttpsPort, protocolInformation.BasePath + "/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10)).Uri;
                if (protocolInformation.IsV11Enabled)
                    this.registrationServiceAddress11 = new UriBuilder(Uri.UriSchemeHttps, protocolInformation.HostName, protocolInformation.HttpsPort, protocolInformation.BasePath + "/" + BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version11)).Uri;
                this.issuedTokensEnabled = protocolInformation.IssuedTokensEnabled;
                this.maxTimeout = protocolInformation.MaxTimeout;
            }
            else
            {
                UriBuilder uriBuilder = new UriBuilder(Uri.UriSchemeHttps, whereabouts.HostName, 443, WsatConfiguration.DisabledRegistrationPath);
                this.registrationServiceAddress10 = uriBuilder.Uri;
                this.registrationServiceAddress11 = uriBuilder.Uri;
                this.issuedTokensEnabled = false;
                this.maxTimeout = TimeSpan.FromMinutes(5.0);
            }
        }

        private static object ReadValue(string key, string value)
        {
            try
            {
                using (RegistryHandle nativeHklmSubkey = RegistryHandle.GetNativeHKLMSubkey(key, false))
                    return nativeHklmSubkey?.GetValue(value);
            }
            catch (SecurityException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatRegistryValueReadError", (object)value), (Exception)ex));
            }
            catch (IOException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new TransactionManagerConfigurationException(System.ServiceModel.SR.GetString("WsatRegistryValueReadError", (object)value), (Exception)ex));
            }
        }

        private static int ReadInt(string key, string value, int defaultValue)
        {
            object obj = WsatConfiguration.ReadValue(key, value);
            return obj == null || !(obj is int num) ? defaultValue : num;
        }

        private static bool ReadFlag(string key, string value, bool defaultValue)
        {
            return (uint)WsatConfiguration.ReadInt(key, value, defaultValue ? 1 : 0) > 0U;
        }
    }
}
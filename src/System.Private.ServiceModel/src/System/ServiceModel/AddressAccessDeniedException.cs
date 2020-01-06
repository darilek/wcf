using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.ServiceModel
{
    [Serializable]
    public class AddressAccessDeniedException : CommunicationException
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAccessDeniedException" /> class.  </summary>
        public AddressAccessDeniedException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAccessDeniedException" /> class with a specified error message. </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AddressAccessDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAccessDeniedException" /> class with a specified error message and a reference to the inner exception that is the cause of the exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The <see cref="T:System.Exception" /> that caused the current exception to be thrown. </param>
        public AddressAccessDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAccessDeniedException" /> class with serialization information and streaming context specified.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains all the data required to serialize the exception.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the source and destination of the stream.</param>
        protected AddressAccessDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Obsolete("Do not use it")]
    internal static class ServiceModelAppSettings
    {
        private static volatile bool settingsInitalized = false;
        private static object appSettingsLock = new object();
        internal const string HttpTransportPerFactoryConnectionPoolString = "wcf:httpTransportBinding:useUniqueConnectionPoolPerFactory";
        internal const string EnsureUniquePerformanceCounterInstanceNamesString = "wcf:ensureUniquePerformanceCounterInstanceNames";
        internal const string UseConfiguredTransportSecurityHeaderLayoutString = "wcf:useConfiguredTransportSecurityHeaderLayout";
        internal const string UseBestMatchNamedPipeUriString = "wcf:useBestMatchNamedPipeUri";
        internal const string DisableOperationContextAsyncFlowString = "wcf:disableOperationContextAsyncFlow";
        internal const string UseLegacyCertificateUsagePolicyString = "wcf:useLegacyCertificateUsagePolicy";
        internal const string DeferSslStreamServerCertificateCleanupString = "wcf:deferSslStreamServerCertificateCleanup";
        private const bool DefaultHttpTransportPerFactoryConnectionPool = false;
        private const bool DefaultEnsureUniquePerformanceCounterInstanceNames = false;
        private const bool DefaultUseConfiguredTransportSecurityHeaderLayout = false;
        private const bool DefaultUseBestMatchNamedPipeUri = false;
        private const bool DefaultUseLegacyCertificateUsagePolicy = false;
        private const bool DefaultDisableOperationContextAsyncFlow = true;
        private const bool DefaultDeferSslStreamServerCertificateCleanup = false;
        private static bool useLegacyCertificateUsagePolicy;
        private static bool httpTransportPerFactoryConnectionPool;
        private static bool ensureUniquePerformanceCounterInstanceNames;
        private static bool useConfiguredTransportSecurityHeaderLayout;
        private static bool useBestMatchNamedPipeUri;
        private static bool disableOperationContextAsyncFlow;
        private static bool deferSslStreamServerCertificateCleanup;

        internal static bool UseLegacyCertificateUsagePolicy
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.useLegacyCertificateUsagePolicy;
            }
        }

        internal static bool HttpTransportPerFactoryConnectionPool
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.httpTransportPerFactoryConnectionPool;
            }
        }

        internal static bool EnsureUniquePerformanceCounterInstanceNames
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.ensureUniquePerformanceCounterInstanceNames;
            }
        }

        internal static bool DisableOperationContextAsyncFlow
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.disableOperationContextAsyncFlow;
            }
        }

        internal static bool UseConfiguredTransportSecurityHeaderLayout
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.useConfiguredTransportSecurityHeaderLayout;
            }
        }

        internal static bool UseBestMatchNamedPipeUri
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.useBestMatchNamedPipeUri;
            }
        }

        internal static bool DeferSslStreamServerCertificateCleanup
        {
            get
            {
                ServiceModelAppSettings.EnsureSettingsLoaded();
                return ServiceModelAppSettings.deferSslStreamServerCertificateCleanup;
            }
        }

        private static void EnsureSettingsLoaded()
        {
            if (ServiceModelAppSettings.settingsInitalized)
                return;
            lock (ServiceModelAppSettings.appSettingsLock)
            {
                if (ServiceModelAppSettings.settingsInitalized)
                    return;
                NameValueCollection nameValueCollection = (NameValueCollection)null;
                try
                {
                    // TODO: consider implement configuration read
                   // nameValueCollection = ConfigurationManager.AppSettings;
                }
               // catch (ConfigurationErrorsException ex)
               // {
               // }
                finally
                {
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:useLegacyCertificateUsagePolicy"], out ServiceModelAppSettings.useLegacyCertificateUsagePolicy))
                        ServiceModelAppSettings.useLegacyCertificateUsagePolicy = false;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:httpTransportBinding:useUniqueConnectionPoolPerFactory"], out ServiceModelAppSettings.httpTransportPerFactoryConnectionPool))
                        ServiceModelAppSettings.httpTransportPerFactoryConnectionPool = false;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:ensureUniquePerformanceCounterInstanceNames"], out ServiceModelAppSettings.ensureUniquePerformanceCounterInstanceNames))
                        ServiceModelAppSettings.ensureUniquePerformanceCounterInstanceNames = false;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:disableOperationContextAsyncFlow"], out ServiceModelAppSettings.disableOperationContextAsyncFlow))
                        ServiceModelAppSettings.disableOperationContextAsyncFlow = true;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:useConfiguredTransportSecurityHeaderLayout"], out ServiceModelAppSettings.useConfiguredTransportSecurityHeaderLayout))
                        ServiceModelAppSettings.useConfiguredTransportSecurityHeaderLayout = false;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:useBestMatchNamedPipeUri"], out ServiceModelAppSettings.useBestMatchNamedPipeUri))
                        ServiceModelAppSettings.useBestMatchNamedPipeUri = false;
                    if (nameValueCollection == null || !bool.TryParse(nameValueCollection["wcf:deferSslStreamServerCertificateCleanup"], out ServiceModelAppSettings.deferSslStreamServerCertificateCleanup))
                        ServiceModelAppSettings.deferSslStreamServerCertificateCleanup = false;
                    ServiceModelAppSettings.settingsInitalized = true;
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
    internal static class SecurityStandardsHelper
    {
        private static SecurityStandardsManager SecurityStandardsManager2007 = SecurityStandardsHelper.CreateStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12);

        private static SecurityStandardsManager CreateStandardsManager(
            MessageSecurityVersion securityVersion)
        {
            return new SecurityStandardsManager(securityVersion, (SecurityTokenSerializer)new WSSecurityTokenSerializer(securityVersion.SecurityVersion, securityVersion.TrustVersion, securityVersion.SecureConversationVersion, false, (SamlSerializer)null, (SecurityStateEncoder)null, (IEnumerable<System.Type>)null));
        }

        public static SecurityStandardsManager CreateStandardsManager(
            TransactionProtocol transactionProtocol)
        {
            return transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004 || transactionProtocol == TransactionProtocol.OleTransactions ? SecurityStandardsManager.DefaultInstance : SecurityStandardsHelper.SecurityStandardsManager2007;
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceModel.Activation;

namespace System.ServiceModel.Channels
{
    internal static class SecurityDescriptorHelper
    {
        private static byte[] worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork = SecurityDescriptorHelper.FromSecurityIdentifiersFull((List<SecurityIdentifier>)null, -1073741824);
        private static byte[] worldCreatorOwnerWithReadDescriptorDenyNetwork = SecurityDescriptorHelper.FromSecurityIdentifiersFull((List<SecurityIdentifier>)null, int.MinValue);

        internal static byte[] FromSecurityIdentifiers(
            List<SecurityIdentifier> allowedSids,
            int accessRights)
        {
            if (allowedSids == null)
            {
                if (accessRights == -1073741824)
                    return SecurityDescriptorHelper.worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork;
                if (accessRights == int.MinValue)
                    return SecurityDescriptorHelper.worldCreatorOwnerWithReadDescriptorDenyNetwork;
            }
            return SecurityDescriptorHelper.FromSecurityIdentifiersFull(allowedSids, accessRights);
        }

        private static byte[] FromSecurityIdentifiersFull(
            List<SecurityIdentifier> allowedSids,
            int accessRights)
        {
            DiscretionaryAcl discretionaryAcl = new DiscretionaryAcl(false, false, allowedSids == null ? 3 : 2 + allowedSids.Count);
            discretionaryAcl.AddAccess(AccessControlType.Deny, new SecurityIdentifier(WellKnownSidType.NetworkSid, (SecurityIdentifier)null), 268435456, InheritanceFlags.None, PropagationFlags.None);
            int clientAccessRights = SecurityDescriptorHelper.GenerateClientAccessRights(accessRights);
            if (allowedSids == null)
            {
                discretionaryAcl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier)null), clientAccessRights, InheritanceFlags.None, PropagationFlags.None);
            }
            else
            {
                for (int index = 0; index < allowedSids.Count; ++index)
                {
                    SecurityIdentifier allowedSid = allowedSids[index];
                    discretionaryAcl.AddAccess(AccessControlType.Allow, allowedSid, clientAccessRights, InheritanceFlags.None, PropagationFlags.None);
                }
            }
            discretionaryAcl.AddAccess(AccessControlType.Allow, SecurityDescriptorHelper.GetProcessLogonSid(), accessRights, InheritanceFlags.None, PropagationFlags.None);
            if (AppContainerInfo.IsRunningInAppContainer)
                discretionaryAcl.AddAccess(AccessControlType.Allow, AppContainerInfo.GetCurrentAppContainerSid(), accessRights, InheritanceFlags.None, PropagationFlags.None);
            CommonSecurityDescriptor securityDescriptor = new CommonSecurityDescriptor(false, false, ControlFlags.None, (SecurityIdentifier)null, (SecurityIdentifier)null, (SystemAcl)null, discretionaryAcl);
            byte[] binaryForm = new byte[securityDescriptor.BinaryLength];
            securityDescriptor.GetBinaryForm(binaryForm, 0);
            return binaryForm;
        }

        private static int GenerateClientAccessRights(int accessRights)
        {
            int num = accessRights;
            if ((num & 1073741824) != 0)
                num = num & -1073741825 | 274;
            return num & -5;
        }

        private static SecurityIdentifier GetProcessLogonSid()
        {
            return Utility.GetLogonSidForPid(Process.GetCurrentProcess().Id);
        }
    }
}

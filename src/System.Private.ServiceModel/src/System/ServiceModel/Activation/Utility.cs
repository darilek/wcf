using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace System.ServiceModel.Activation
{
    internal static class Utility
    {
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        internal static unsafe SecurityIdentifier GetLogonSidForPid(int pid)
        {
            SafeCloseHandle process = Utility.OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle processToken = Utility.GetProcessToken(process, 8);
                try
                {
                    byte[] tokenInformation = new byte[Utility.GetTokenInformationLength(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups)];
                    fixed (byte* numPtr = tokenInformation)
                    {
                        Utility.GetTokenInformation(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups, tokenInformation);
                        ListenerUnsafeNativeMethods.TOKEN_GROUPS* tokenGroupsPtr = (ListenerUnsafeNativeMethods.TOKEN_GROUPS*)numPtr;
                        ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sidAndAttributesPtr = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*)&tokenGroupsPtr->Groups;
                        for (int index = 0; index < tokenGroupsPtr->GroupCount; ++index)
                        {
                            if ((sidAndAttributesPtr[index].Attributes & ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID) == ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID)
                                return new SecurityIdentifier(sidAndAttributesPtr[index].Sid);
                        }
                    }
                    return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, (SecurityIdentifier)null);
                }
                finally
                {
                    processToken.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        private static int GetTokenInformationLength(
            SafeCloseHandle token,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic)
        {
            int returnLength;
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, (byte[])null, 0, out returnLength))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 122)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new Win32Exception(lastWin32Error));
            }
            return returnLength;
        }

        private static void GetTokenInformation(
            SafeCloseHandle token,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic,
            byte[] tokenInformation)
        {
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, tokenInformation, tokenInformation.Length, out int _))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
        }

        private static SafeCloseHandle OpenProcessForQuery(int pid)
        {
            SafeCloseHandle safeCloseHandle = ListenerUnsafeNativeMethods.OpenProcess(1024, false, pid);
            if (safeCloseHandle.IsInvalid)
            {
                Exception exception = (Exception)new Win32Exception();
                safeCloseHandle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return safeCloseHandle;
        }

        private static SafeCloseHandle GetProcessToken(
            SafeCloseHandle process,
            int requiredAccess)
        {
            SafeCloseHandle tokenHandle;
            bool flag = ListenerUnsafeNativeMethods.OpenProcessToken(process, requiredAccess, out tokenHandle);
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (!flag)
            {
                tokenHandle?.SetHandleAsInvalid();
                //System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle((SafeHandle)tokenHandle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new Win32Exception(lastWin32Error));
            }
            return tokenHandle;
        }
    }
}

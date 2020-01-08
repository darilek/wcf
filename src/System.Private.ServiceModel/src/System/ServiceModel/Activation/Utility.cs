using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.Text;

namespace System.ServiceModel.Activation
{
    internal static unsafe class Utility
    {
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal static SecurityIdentifier GetLogonSidForPid(int pid)
        {
            SafeCloseHandle process = OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle token = GetProcessToken(process, ListenerUnsafeNativeMethods.TOKEN_QUERY);
                try
                {
                    int length = GetTokenInformationLength(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups);
                    byte[] tokenInformation = new byte[length];
                    fixed (byte* pTokenInformation = tokenInformation)
                    {
                        GetTokenInformation(token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups, tokenInformation);

                        ListenerUnsafeNativeMethods.TOKEN_GROUPS* ptg = (ListenerUnsafeNativeMethods.TOKEN_GROUPS*)pTokenInformation;
                        ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sids = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*)(&(ptg->Groups));
                        for (int i = 0; i < ptg->GroupCount; i++)
                        {
                            if ((sids[i].Attributes & ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID) == ListenerUnsafeNativeMethods.SidAttribute.SE_GROUP_LOGON_ID)
                            {
                                return new SecurityIdentifier(sids[i].Sid);
                            }
                        }
                    }
                    return new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
                }
                finally
                {
                    token.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        static void GetTokenInformation(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic, byte[] tokenInformation)
        {
            int lengthNeeded;
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, tokenInformation, tokenInformation.Length, out lengthNeeded))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        static int GetTokenInformationLength(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic)
        {
            int lengthNeeded;
            bool success = ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, null, 0, out lengthNeeded);
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != ListenerUnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }

            return lengthNeeded;
        }

        static SafeCloseHandle OpenProcessForQuery(int pid)
        {
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            SafeCloseHandle process = ListenerUnsafeNativeMethods.OpenProcess(ListenerUnsafeNativeMethods.PROCESS_QUERY_INFORMATION, false, pid);
            if (process.IsInvalid)
            {
                Exception exception = new Win32Exception();
                process.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return process;
        }

        static SafeCloseHandle GetProcessToken(SafeCloseHandle process, int requiredAccess)
        {
            SafeCloseHandle processToken;
            bool success = ListenerUnsafeNativeMethods.OpenProcessToken(process, requiredAccess, out processToken);
            int error = Marshal.GetLastWin32Error();
            if (!success)
            {
                CloseInvalidOutSafeHandle(processToken);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }

            return processToken;
        }

        // Call this when a p/invoke with an 'out SafeHandle' parameter returns an error.  This will safely clean up the handle.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.TransparentMethodsMustNotReferenceCriticalCode)] // we got APTCA approval with no requirement to fix this transparency warning
        internal static void CloseInvalidOutSafeHandle(SafeHandle handle)
        {
            // Workaround for 64-bit CLR bug VSWhidbey 546830 - sometimes invalid SafeHandles come back null.
            if (handle != null)
            {
#pragma warning disable 618
                Fx.Assert(handle.IsInvalid, "CloseInvalidOutSafeHandle called with a valid handle!");
#pragma warning restore 618

                // Calls SuppressFinalize.
                handle.SetHandleAsInvalid();
            }
        }
    }
}

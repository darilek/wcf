using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.ServiceModel.Activation
{
    [SuppressUnmanagedCodeSecurity]
    internal static class ListenerUnsafeNativeMethods
    {
        private const string ADVAPI32 = "advapi32.dll";
        private const string KERNEL32 = "kernel32.dll";
        internal const int OWNER_SECURITY_INFORMATION = 1;
        internal const int DACL_SECURITY_INFORMATION = 4;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        internal const int PROCESS_QUERY_INFORMATION = 1024;
        internal const int PROCESS_DUP_HANDLE = 64;
        internal const int READ_CONTROL = 131072;
        internal const int TOKEN_QUERY = 8;
        internal const int WRITE_DAC = 262144;
        internal const int TOKEN_ADJUST_PRIVILEGES = 32;
        internal const int SC_MANAGER_CONNECT = 1;
        internal const int SC_STATUS_PROCESS_INFO = 0;
        internal const int SERVICE_QUERY_CONFIG = 1;
        internal const int SERVICE_QUERY_STATUS = 4;
        internal const int SERVICE_RUNNING = 4;
        internal const int SERVICE_START = 16;
        internal const int SERVICE_START_PENDING = 2;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        internal static extern void DebugBreak();

      /*  [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern unsafe bool AdjustTokenPrivileges(
            SafeCloseHandle tokenHandle,
            bool disableAllPrivileges,
            ListenerUnsafeNativeMethods.TOKEN_PRIVILEGES* newState,
            int bufferLength,
            IntPtr previousState,
            IntPtr returnLength);*/

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool LookupAccountName(
            string systemName,
            string accountName,
            byte[] sid,
            ref uint cbSid,
            StringBuilder referencedDomainName,
            ref uint cchReferencedDomainName,
            out short peUse);

    /*    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern unsafe bool LookupPrivilegeValue(
            IntPtr lpSystemName,
            string lpName,
            LUID* lpLuid);*/

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetKernelObjectSecurity(
            SafeCloseHandle handle,
            int securityInformation,
            [Out] byte[] pSecurityDescriptor,
            int nLength,
            out int lpnLengthNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetTokenInformation(
            SafeCloseHandle tokenHandle,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass,
            [Out] byte[] pTokenInformation,
            int tokenInformationLength,
            out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeCloseHandle OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(
            SafeCloseHandle processHandle,
            int desiredAccess,
            out SafeCloseHandle tokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeServiceHandle OpenSCManager(
            string lpMachineName,
            string lpDatabaseName,
            int dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeServiceHandle OpenService(
            SafeServiceHandle hSCManager,
            string lpServiceName,
            int dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool QueryServiceConfig(
            SafeServiceHandle hService,
            [Out] byte[] pServiceConfig,
            int cbBufSize,
            out int pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool QueryServiceStatus(
            SafeServiceHandle hService,
            out ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS status);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool QueryServiceStatusEx(
            SafeServiceHandle hService,
            int InfoLevel,
            [Out] byte[] pBuffer,
            int cbBufSize,
            out int pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SetKernelObjectSecurity(
            SafeCloseHandle handle,
            int securityInformation,
            [In] byte[] pSecurityDescriptor);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool StartService(
            SafeServiceHandle hSCManager,
            int dwNumServiceArgs,
            string[] lpServiceArgVectors);

        [Flags]
        internal enum SidAttribute : uint
        {
            SE_GROUP_MANDATORY = 1,
            SE_GROUP_ENABLED_BY_DEFAULT = 2,
            SE_GROUP_ENABLED = 4,
            SE_GROUP_OWNER = 8,
            SE_GROUP_USE_FOR_DENY_ONLY = 16, // 0x00000010
            SE_GROUP_RESOURCE = 536870912, // 0x20000000
            SE_GROUP_LOGON_ID = 3221225472, // 0xC0000000
        }

        internal enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups = 2,
            TokenPrivileges = 3,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenDefaultDacl = 6,
            TokenSource = 7,
            TokenType = 8,
            TokenImpersonationLevel = 9,
            TokenStatistics = 10, // 0x0000000A
            TokenRestrictedSids = 11, // 0x0000000B
            TokenSessionId = 12, // 0x0000000C
            TokenGroupsAndPrivileges = 13, // 0x0000000D
            TokenSessionReference = 14, // 0x0000000E
            TokenSandBoxInert = 15, // 0x0000000F
            TokenAuditPolicy = 16, // 0x00000010
            TokenOrigin = 17, // 0x00000011
            TokenElevationType = 18, // 0x00000012
            TokenLinkedToken = 19, // 0x00000013
            TokenElevation = 20, // 0x00000014
            TokenHasRestrictions = 21, // 0x00000015
            TokenAccessInformation = 22, // 0x00000016
            TokenVirtualizationAllowed = 23, // 0x00000017
            TokenVirtualizationEnabled = 24, // 0x00000018
            TokenIntegrityLevel = 25, // 0x00000019
            TokenUIAccess = 26, // 0x0000001A
            TokenMandatoryPolicy = 27, // 0x0000001B
            TokenLogonSid = 28, // 0x0000001C
            TokenIsAppContainer = 29, // 0x0000001D
            TokenCapabilities = 30, // 0x0000001E
            TokenAppContainerSid = 31, // 0x0000001F
            TokenAppContainerNumber = 32, // 0x00000020
            TokenUserClaimAttributes = 33, // 0x00000021
            TokenDeviceClaimAttributes = 34, // 0x00000022
            TokenRestrictedUserClaimAttributes = 35, // 0x00000023
            TokenRestrictedDeviceClaimAttributes = 36, // 0x00000024
            TokenDeviceGroups = 37, // 0x00000025
            TokenRestrictedDeviceGroups = 38, // 0x00000026
            MaxTokenInfoClass = 39, // 0x00000027
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct QUERY_SERVICE_CONFIG
        {
            internal int dwServiceType;
            internal int dwStartType;
            internal int dwErrorControl;
            internal string lpBinaryPathName;
            internal string lpLoadOrderGroup;
            internal int dwTagId;
            internal string lpDependencies;
            internal string lpServiceStartName;
            internal string lpDisplayName;
        }

        internal struct SERVICE_STATUS_PROCESS
        {
            internal int dwServiceType;
            internal int dwCurrentState;
            internal int dwControlsAccepted;
            internal int dwWin32ExitCode;
            internal int dwServiceSpecificExitCode;
            internal int dwCheckPoint;
            internal int dwWaitHint;
            internal int dwProcessId;
            internal int dwServiceFlags;
        }

        internal struct SID_AND_ATTRIBUTES
        {
            internal IntPtr Sid;
            internal ListenerUnsafeNativeMethods.SidAttribute Attributes;
        }

        internal struct TOKEN_GROUPS
        {
            internal int GroupCount;
            internal IntPtr Groups;
        }

        internal struct TOKEN_USER
        {
            internal IntPtr User;
        }

     /*   internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            internal LUID_AND_ATTRIBUTES Privileges;
        }*/

        [Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
        [ComConversionLoss]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        internal interface ICorRuntimeHost
        {
            void Void0();

            void Void1();

            void Void2();

            void Void3();

            void Void4();

            void Void5();

            void Void6();

            void Void7();

            void Void8();

            void Void9();

            void GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


#define WSARECV


using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Channels
{

    internal static class UnsafeNativeMethods
    {
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_OUTOFMEMORY = 14;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_NETNAME_DELETED = 64;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_BROKEN_PIPE = 109;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_PIPE_BUSY = 231;
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_MORE_DATA = 234;
        public const int WAIT_TIMEOUT = 258;
        public const int ERROR_PIPE_CONNECTED = 535;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        public const int ERROR_SERVICE_DISABLED = 1058;
        public const int ERROR_NO_TRACKING_SERVICE = 1172;
        public const int ERROR_ALLOTTED_SPACE_EXCEEDED = 1344;
        public const int ERROR_NO_SYSTEM_RESOURCES = 1450;

        // When querying for the token length
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        public const int STATUS_PENDING = 0x103;

        // socket errors
        public const int WSAACCESS = 10013;
        public const int WSAEMFILE = 10024;
        public const int WSAEMSGSIZE = 10040;
        public const int WSAEADDRINUSE = 10048;
        public const int WSAEADDRNOTAVAIL = 10049;
        public const int WSAENETDOWN = 10050;
        public const int WSAENETUNREACH = 10051;
        public const int WSAENETRESET = 10052;
        public const int WSAECONNABORTED = 10053;
        public const int WSAECONNRESET = 10054;
        public const int WSAENOBUFS = 10055;
        public const int WSAESHUTDOWN = 10058;
        public const int WSAETIMEDOUT = 10060;
        public const int WSAECONNREFUSED = 10061;
        public const int WSAEHOSTDOWN = 10064;
        public const int WSAEHOSTUNREACH = 10065;

        private struct TokenAppContainerInfo
        {
            public IntPtr psid;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileMappingHandle OpenFileMapping(
            int access,
            bool inheritHandle,
            string name);


        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeViewOfFileHandle MapViewOfFile(
            SafeFileMappingHandle handle,
            int dwDesiredAccess,
            int dwFileOffsetHigh,
            int dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap);


        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            PipeHandle hSourceHandle,
            SafeCloseHandle hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int WriteFile(
            IntPtr handle,
            byte* bytes,
            int numBytesToWrite,
            IntPtr numBytesWritten_mustBeZero,
            NativeOverlapped* lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int GetOverlappedResult(
            PipeHandle handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int GetOverlappedResult(
            IntPtr handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern unsafe int ReadFile(
            IntPtr handle,
            byte* bytes,
            int numBytesToRead,
            IntPtr numBytesRead_mustBeZero,
            NativeOverlapped* overlapped);


        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        internal static unsafe bool HasOverlappedIoCompleted(NativeOverlapped* overlapped)
        {
            return overlapped->InternalLow != (IntPtr)259;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength = Marshal.SizeOf(typeof(UnsafeNativeMethods.SECURITY_ATTRIBUTES));
            internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
            internal bool bInheritHandle;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int SetNamedPipeHandleState(
            PipeHandle handle,
            ref int mode,
            IntPtr collectionCount,
            IntPtr collectionDataTimeout);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern PipeHandle CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSECURITY_ATTRIBUTES,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileMappingHandle CreateFileMapping(
            IntPtr fileHandle,
            UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes,
            int protect,
            int sizeHigh,
            int sizeLow,
            string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetNamedPipeClientProcessId(PipeHandle handle, out int id);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("userenv.dll", SetLastError = true)]
        internal static extern int DeriveAppContainerSidFromAppContainerName(
            [MarshalAs(UnmanagedType.LPWStr), In] string appContainerName,
            out IntPtr appContainerSid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern IntPtr FreeSid(IntPtr pSid);

        [DllImport("kernel32.dll")]
        internal static extern int PackageFamilyNameFromFullName(
            [MarshalAs(UnmanagedType.LPWStr), In] string packageFullName,
            ref uint packageFamilyNameLength,
            [MarshalAs(UnmanagedType.LPWStr), In, Out] StringBuilder packageFamilyName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetAppContainerNamedObjectPath(
            IntPtr token,
            IntPtr appContainerSid,
            uint objectPathLength,
            [MarshalAs(UnmanagedType.LPWStr), In, Out] StringBuilder objectPath,
            ref uint returnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            TokenAccessLevels DesiredAccess,
            out SafeCloseHandle TokenHandle);



        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        internal static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        internal static extern int CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(
            SafeCloseHandle tokenHandle,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass,
            byte[] tokenInformation,
            uint tokenInformationLength,
            out uint returnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(
            SafeCloseHandle tokenHandle,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass,
            out uint tokenInformation,
            uint tokenInformationLength,
            out uint returnLength);

        [SecurityCritical]
        [DllImport("kernel32.dll")]
        public static extern uint GetSystemTimeAdjustment(
            out int adjustment,
            out uint increment,
            out uint adjustmentDisabled);

        [SecurityCritical]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        public static extern SafeWaitHandle CreateWaitableTimer(
            IntPtr mustBeZero,
            bool manualReset,
            string timerName);

        [SecurityCritical]
        [DllImport("kernel32.dll")]
        public static extern bool SetWaitableTimer(
            SafeWaitHandle handle,
            ref long dueTime,
            int period,
            IntPtr mustBeZero,
            IntPtr mustBeZeroAlso,
            bool resume);

        internal static unsafe SecurityIdentifier GetAppContainerSid(
  SafeCloseHandle tokenHandle)
        {
            uint returnLength = UnsafeNativeMethods.GetTokenInformationLength(tokenHandle, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenAppContainerSid);
            byte[] tokenInformation = new byte[(int)returnLength];
            fixed (byte* numPtr = tokenInformation)
            {
                if (!UnsafeNativeMethods.GetTokenInformation(tokenHandle, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenAppContainerSid, tokenInformation, returnLength, out returnLength))
                    throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                return new SecurityIdentifier(((UnsafeNativeMethods.TokenAppContainerInfo*)numPtr)->psid);
            }
        }

        private static uint GetTokenInformationLength(
          SafeCloseHandle token,
          ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass)
        {
            uint returnLength;
            if (!UnsafeNativeMethods.GetTokenInformation(token, tokenInformationClass, (byte[])null, 0U, out returnLength))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 122)
                    throw FxTrace.Exception.AsError((Exception)new Win32Exception(lastWin32Error));
            }
            return returnLength;
        }

        internal static int GetSessionId(SafeCloseHandle tokenHandle)
        {
            uint tokenInformation;
            if (!UnsafeNativeMethods.GetTokenInformation(tokenHandle, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenSessionId, out tokenInformation, 4U, out uint _))
                throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return (int)tokenInformation;
        }

        internal static bool RunningInAppContainer(SafeCloseHandle tokenHandle)
        {
            uint tokenInformation;
            if (!UnsafeNativeMethods.GetTokenInformation(tokenHandle, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenIsAppContainer, out tokenInformation, 4U, out uint _))
                throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return tokenInformation == 1U;
        }
    }
}

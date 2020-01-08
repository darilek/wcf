// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


#define WSARECV


using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Channels
{
    using SafeCloseHandle = System.ServiceModel.Activation.SafeCloseHandle;
    internal static class UnsafeNativeMethods
    {

        public const string KERNEL32 = "kernel32.dll";
        public const string ADVAPI32 = "advapi32.dll";
        public const string SECUR32 = "secur32.dll";
        public const string USERENV = "userenv.dll";

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

        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;

        public const int SECURITY_ANONYMOUS = 0x00000000;
        public const int SECURITY_QOS_PRESENT = 0x00100000;
        public const int SECURITY_IDENTIFICATION = 0x00010000;

        public const int GENERIC_ALL = 0x10000000;
        public const int GENERIC_READ = unchecked((int)0x80000000);
        public const int GENERIC_WRITE = 0x40000000;
        public const int FILE_CREATE_PIPE_INSTANCE = 0x00000004;
        public const int FILE_WRITE_ATTRIBUTES = 0x00000100;
        public const int FILE_WRITE_DATA = 0x00000002;
        public const int FILE_WRITE_EA = 0x00000010;

        public const int OPEN_EXISTING = 3;

        public const int FILE_MAP_WRITE = 2;
        public const int FILE_MAP_READ = 4;


        // VirtualAlloc constants
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const int PAGE_READWRITE = 4;

        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        public const int DUPLICATE_CLOSE_SOURCE = 0x00000001;
        public const int DUPLICATE_SAME_ACCESS = 0x00000002;

        public const int PIPE_ACCESS_DUPLEX = 3;
        public const int PIPE_UNLIMITED_INSTANCES = 255;
        public const int PIPE_TYPE_BYTE = 0;
        public const int PIPE_TYPE_MESSAGE = 4;
        public const int PIPE_READMODE_BYTE = 0;
        public const int PIPE_READMODE_MESSAGE = 2;

        public const uint MAX_PATH = 260;

        // Note: The CLR's Watson bucketization code looks at the caller of the FCALL method
        // to assign blame for crashes.  Don't mess with this, such as by making it call 
        // another managed helper method, unless you consult with some CLR Watson experts.
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.Process)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void FailFast(string message);

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
            internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
            internal bool bInheritHandle = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct TokenAppContainerInfo
        {
            public IntPtr psid;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal unsafe static bool HasOverlappedIoCompleted(
            NativeOverlapped* overlapped)
        {
            return overlapped->InternalLow != (IntPtr)STATUS_PENDING;
        }

        internal static bool RunningInAppContainer(SafeCloseHandle tokenHandle)
        {
            uint runningInAppContainer;
            uint returnLength;
            if (!UnsafeNativeMethods.GetTokenInformation(
                tokenHandle,
                ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenIsAppContainer,
                out runningInAppContainer,
                sizeof(uint),
                out returnLength))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
            }

            return runningInAppContainer == 1;
        }

        static uint GetTokenInformationLength(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass)
        {
            uint lengthNeeded;
            bool success;
            if (!(success = GetTokenInformation(
                token,
                tokenInformationClass,
                null,
                0,
                out lengthNeeded)))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != UnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw FxTrace.Exception.AsError(new Win32Exception(error));
                }
            }

            Fx.Assert(!success, "Retreving the length should always fail.");

            return lengthNeeded;
        }

        internal static unsafe SecurityIdentifier GetAppContainerSid(SafeCloseHandle tokenHandle)
        {
            // Get length of buffer needed for sid.
            uint returnLength = UnsafeNativeMethods.GetTokenInformationLength(
                tokenHandle,
                ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenAppContainerSid);

            byte[] tokenInformation = new byte[returnLength];
            fixed (byte* pTokenInformation = tokenInformation)
            {
                if (!UnsafeNativeMethods.GetTokenInformation(
                    tokenHandle,
                    ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenAppContainerSid,
                    tokenInformation,
                    returnLength,
                    out returnLength))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
                }

                TokenAppContainerInfo* ptg = (TokenAppContainerInfo*)pTokenInformation;
                return new SecurityIdentifier(ptg->psid);
            }
        }


        internal static int GetSessionId(SafeCloseHandle tokenHandle)
        {
            uint sessionId;
            uint returnLength;

            if (!UnsafeNativeMethods.GetTokenInformation(
                tokenHandle,
                ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenSessionId,
                out sessionId,
                sizeof(uint),
                out returnLength))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
            }

            return (int)sessionId;
        }

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int ReadFile
        (
            IntPtr handle,
            byte* bytes,
            int numBytesToRead,
            IntPtr numBytesRead_mustBeZero,
            NativeOverlapped* overlapped
        );

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern int WriteFile
        (
            IntPtr handle,
            byte* bytes,
            int numBytesToWrite,
            IntPtr numBytesWritten_mustBeZero,
            NativeOverlapped* lpOverlapped
        );

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool OpenProcessToken
        (
            IntPtr ProcessHandle,
            TokenAccessLevels DesiredAccess,
            out SafeCloseHandle TokenHandle
        );

        // Token marshalled as uint
        [DllImport(ADVAPI32, SetLastError = true)]
        static extern bool GetTokenInformation
        (
            SafeCloseHandle tokenHandle,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass,
            out uint tokenInformation,
            uint tokenInformationLength,
            out uint returnLength
        );

        // Token marshalled as byte[]
        [DllImport(ADVAPI32, SetLastError = true)]
        static extern unsafe bool GetTokenInformation
        (
            SafeCloseHandle tokenHandle,
            ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tokenInformationClass,
            byte[] tokenInformation,
            uint tokenInformationLength,
            out uint returnLength
        );

        [DllImport(USERENV, SetLastError = true)]
        internal static extern int DeriveAppContainerSidFromAppContainerName
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string appContainerName,
            out IntPtr appContainerSid
        );

        [DllImport(ADVAPI32, SetLastError = true)]
        internal static extern IntPtr FreeSid
        (
            IntPtr pSid
        );

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool GetAppContainerNamedObjectPath
        (
            IntPtr token,
            IntPtr appContainerSid,
            uint objectPathLength,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder objectPath,
            ref uint returnLength
        );

        // If the function succeeds, the return value is ERROR_SUCCESS and 'packageFamilyNameLength' contains the size of the data copied 
        // to 'packageFamilyName' (in WCHARs, including the null-terminator). If the function fails, the return value is a Win32 error code.
        [DllImport(KERNEL32)]
        internal static extern int PackageFamilyNameFromFullName
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
            ref uint packageFamilyNameLength,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder packageFamilyName
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern PipeHandle CreateFile
        (
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSECURITY_ATTRIBUTES,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeViewOfFileHandle MapViewOfFile
        (
            SafeFileMappingHandle handle,
            int dwDesiredAccess,
            int dwFileOffsetHigh,
            int dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap
        );


        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileMappingHandle OpenFileMapping
        (
            int access,
            bool inheritHandle,
            string name
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool GetNamedPipeClientProcessId(PipeHandle handle, out int id);

        [DllImport(KERNEL32, CharSet = CharSet.None, ExactSpelling = false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern int CloseHandle(IntPtr handle);

        [DllImport(KERNEL32, ExactSpelling = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern unsafe int GetOverlappedResult
        (
            PipeHandle handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait
        );

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern unsafe int GetOverlappedResult
        (
            IntPtr handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait
        );

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            PipeHandle hSourceHandle,
            SafeCloseHandle hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern PipeHandle CreateNamedPipe
        (
            string name,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufSize,
            int inBufSize,
            int timeout,
            SECURITY_ATTRIBUTES securityAttributes
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int DisconnectNamedPipe
        (
            PipeHandle handle
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern unsafe int ConnectNamedPipe
        (
            PipeHandle handle,
            NativeOverlapped* lpOverlapped
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeFileMappingHandle CreateFileMapping(
            IntPtr fileHandle,
            SECURITY_ATTRIBUTES securityAttributes,
            int protect,
            int sizeHigh,
            int sizeLow,
            string name
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int FormatMessage
        (
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int FormatMessage
        (
            int dwFlags,
            SafeLibraryHandle lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SetNamedPipeHandleState
        (
            PipeHandle handle,
            ref int mode,
            IntPtr collectionCount,
            IntPtr collectionDataTimeout
        );
    }

    [SuppressUnmanagedCodeSecurity]
    internal class PipeHandle : SafeHandleMinusOneIsInvalid
    {
        internal PipeHandle() : base(true)
        {
        }

        internal PipeHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        internal int GetClientPid()
        {
            int num;
            if (!UnsafeNativeMethods.GetNamedPipeClientProcessId(this, out num))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
            return num;
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.CloseHandle(this.handle) != 0;
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute()]
    internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.CloseHandle(handle) != 0;
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        bool doNotfreeLibraryOnRelease;

        internal SafeLibraryHandle()
            : base(true)
        {
            doNotfreeLibraryOnRelease = false;
        }

        public void DoNotFreeLibraryOnRelease()
        {
            this.doNotfreeLibraryOnRelease = true;
        }

        [DllImport(UnsafeNativeMethods.KERNEL32, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool FreeLibrary(IntPtr hModule);

        protected override bool ReleaseHandle()
        {
            if (doNotfreeLibraryOnRelease)
            {
                handle = IntPtr.Zero;
                return true;
            }

            return FreeLibrary(handle);
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeViewOfFileHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (UnsafeNativeMethods.UnmapViewOfFile(handle) != 0)
            {
                handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }
}

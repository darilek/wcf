using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.ServiceModel.Activation;
using System.Text;

namespace System.ServiceModel.Channels
{
    internal class AppContainerInfo
    {
        private static object thisLock = new object();
        private static object isRunningInAppContainerLock = new object();
        private static bool isAppContainerSupported = OsEnvironmentHelper.IsAtLeast(OSVersion.Win8);
        private static bool isRunningInAppContainer;
        private static volatile bool isRunningInAppContainerSet;
        private static int? currentSessionId;
        private static volatile SecurityIdentifier currentAppContainerSid;

        static AppContainerInfo()
        {
            if (AppContainerInfo.isAppContainerSupported)
                return;
            AppContainerInfo.isRunningInAppContainerSet = true;
        }

        private AppContainerInfo(int sessionId, string namedObjectPath)
        {
            this.SessionId = sessionId;
            this.NamedObjectPath = namedObjectPath;
        }

        internal static bool IsAppContainerSupported
        {
            get
            {
                return AppContainerInfo.isAppContainerSupported;
            }
        }

        internal static bool IsRunningInAppContainer
        {
            get
            {
                if (!AppContainerInfo.isRunningInAppContainerSet)
                {
                    lock (AppContainerInfo.isRunningInAppContainerLock)
                    {
                        if (!AppContainerInfo.isRunningInAppContainerSet)
                        {
                            AppContainerInfo.isRunningInAppContainer = AppContainerInfo.RunningInAppContainer();
                            AppContainerInfo.isRunningInAppContainerSet = true;
                        }
                    }
                }
                return AppContainerInfo.isRunningInAppContainer;
            }
        }

        internal int SessionId { get; private set; }

        internal string NamedObjectPath { get; private set; }

        internal static AppContainerInfo CreateAppContainerInfo(
            string fullName,
            int sessionId)
        {
            int sessionId1 = sessionId;
            if (sessionId1 == -1)
            {
                lock (AppContainerInfo.thisLock)
                {
                    if (!AppContainerInfo.currentSessionId.HasValue)
                        AppContainerInfo.currentSessionId = new int?(AppContainerInfo.GetCurrentSessionId());
                }
                sessionId1 = AppContainerInfo.currentSessionId.Value;
            }
            string containerNamedObjectPath = AppContainerInfo.GetAppContainerNamedObjectPath(fullName);
            return new AppContainerInfo(sessionId1, containerNamedObjectPath);
        }

        [SecuritySafeCritical]
        internal static SecurityIdentifier GetCurrentAppContainerSid()
        {
            if (AppContainerInfo.currentAppContainerSid == (SecurityIdentifier)null)
            {
                lock (AppContainerInfo.thisLock)
                {
                    if (AppContainerInfo.currentAppContainerSid == (SecurityIdentifier)null)
                    {
                        SafeCloseHandle tokenHandle = (SafeCloseHandle)null;
                        try
                        {
                            tokenHandle = AppContainerInfo.GetCurrentProcessToken();
                            AppContainerInfo.currentAppContainerSid = UnsafeNativeMethods.GetAppContainerSid(tokenHandle);
                        }
                        finally
                        {
                            tokenHandle?.Dispose();
                        }
                    }
                }
            }
            return AppContainerInfo.currentAppContainerSid;
        }

        [SecuritySafeCritical]
        private static bool RunningInAppContainer()
        {
            SafeCloseHandle tokenHandle = (SafeCloseHandle)null;
            try
            {
                tokenHandle = AppContainerInfo.GetCurrentProcessToken();
                return UnsafeNativeMethods.RunningInAppContainer(tokenHandle);
            }
            finally
            {
                tokenHandle?.Dispose();
            }
        }

        [SecuritySafeCritical]
        private static string GetAppContainerNamedObjectPath(string name)
        {
            IntPtr appContainerSid = IntPtr.Zero;
            uint packageFamilyNameLength = 260;
            StringBuilder packageFamilyName = new StringBuilder(260);
            int error = UnsafeNativeMethods.PackageFamilyNameFromFullName(name, ref packageFamilyNameLength, packageFamilyName);
            if (error != 0)
                throw FxTrace.Exception.AsError((Exception)new Win32Exception(error, SR.Format("PackageFullNameInvalid", (object)name)));
            string appContainerName = packageFamilyName.ToString();
            try
            {
                if (UnsafeNativeMethods.DeriveAppContainerSidFromAppContainerName(appContainerName, out appContainerSid) != 0)
                    throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                StringBuilder objectPath = new StringBuilder(260);
                uint returnLength = 0;
                if (!UnsafeNativeMethods.GetAppContainerNamedObjectPath(IntPtr.Zero, appContainerSid, 260U, objectPath, ref returnLength))
                    throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
                return objectPath.ToString();
            }
            finally
            {
                if (appContainerSid != IntPtr.Zero)
                    UnsafeNativeMethods.FreeSid(appContainerSid);
            }
        }

        [SecuritySafeCritical]
        private static int GetCurrentSessionId()
        {
            SafeCloseHandle tokenHandle = (SafeCloseHandle)null;
            try
            {
                tokenHandle = AppContainerInfo.GetCurrentProcessToken();
                return UnsafeNativeMethods.GetSessionId(tokenHandle);
            }
            finally
            {
                tokenHandle?.Dispose();
            }
        }

        [SecurityCritical]
        private static SafeCloseHandle GetCurrentProcessToken()
        {
            SafeCloseHandle TokenHandle = (SafeCloseHandle)null;
            if (!UnsafeNativeMethods.OpenProcessToken(UnsafeNativeMethods.GetCurrentProcess(), TokenAccessLevels.Query, out TokenHandle))
                throw FxTrace.Exception.AsError((Exception)new Win32Exception(Marshal.GetLastWin32Error()));
            return TokenHandle;
        }
    }
}

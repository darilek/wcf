using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.ServiceModel.Channels
{
    internal class PipeSharedMemory : IDisposable
    {
        internal const string PipePrefix = "\\\\.\\pipe\\";
        internal const string PipeLocalPrefix = "\\\\.\\pipe\\Local\\";
        private SafeFileMappingHandle fileMapping;
        private string pipeName;
        private string pipeNameGuidPart;
        private Uri pipeUri;

        private PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri)
            : this(fileMapping, pipeUri, (string)null)
        {
        }

        private PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri, string pipeName)
        {
            this.pipeName = pipeName;
            this.fileMapping = fileMapping;
            this.pipeUri = pipeUri;
        }

        public static PipeSharedMemory Create(
            List<SecurityIdentifier> allowedSids,
            Uri pipeUri,
            string sharedMemoryName)
        {
            PipeSharedMemory result;
            if (PipeSharedMemory.TryCreate(allowedSids, pipeUri, sharedMemoryName, out result))
                return result;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameInUseException(5, pipeUri));
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        public static unsafe bool TryCreate(
            List<SecurityIdentifier> allowedSids,
            Uri pipeUri,
            string sharedMemoryName,
            out PipeSharedMemory result)
        {
            Guid pipeGuid = Guid.NewGuid();
            string pipeName = PipeSharedMemory.BuildPipeName(pipeGuid.ToString());
            byte[] numArray;
            try
            {
                numArray = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, int.MinValue);
            }
            catch (Win32Exception ex)
            {
                Exception innerException = (Exception)new PipeException(ex.Message, (Exception)ex);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new CommunicationException(innerException.Message, innerException));
            }
            result = (PipeSharedMemory)null;
            SafeFileMappingHandle fileMapping;
            int lastWin32Error;
            fixed (byte* numPtr = numArray)
            {
                fileMapping = UnsafeNativeMethods.CreateFileMapping((IntPtr)(-1), new UnsafeNativeMethods.SECURITY_ATTRIBUTES()
                {
                    lpSecurityDescriptor = (IntPtr)(void*)numPtr
                }, 4, 0, sizeof(PipeSharedMemory.SharedMemoryContents), sharedMemoryName);
                lastWin32Error = Marshal.GetLastWin32Error();
            }
            if (fileMapping.IsInvalid)
            {
                fileMapping.SetHandleAsInvalid();
                if (lastWin32Error == 5)
                    return false;
                Exception innerException = (Exception)new PipeException(SR.Format("PipeNameCantBeReserved", (object)pipeUri.AbsoluteUri, (object)PipeError.GetErrorString(lastWin32Error)), lastWin32Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new AddressAccessDeniedException(innerException.Message, innerException));
            }
            if (lastWin32Error == 183)
            {
                fileMapping.Close();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameInUseException(lastWin32Error, pipeUri));
            }
            PipeSharedMemory pipeSharedMemory = new PipeSharedMemory(fileMapping, pipeUri, pipeName);
            bool flag = true;
            try
            {
                pipeSharedMemory.InitializeContents(pipeGuid);
                flag = false;
                result = pipeSharedMemory;
               // if (TD.PipeSharedMemoryCreatedIsEnabled())
               //     TD.PipeSharedMemoryCreated(sharedMemoryName);
                return true;
            }
            finally
            {
                if (flag)
                    pipeSharedMemory.Dispose();
            }
        }

        public static PipeSharedMemory Open(string sharedMemoryName, Uri pipeUri)
        {
            SafeFileMappingHandle fileMapping1 = UnsafeNativeMethods.OpenFileMapping(4, false, sharedMemoryName);
            if (!fileMapping1.IsInvalid)
                return new PipeSharedMemory(fileMapping1, pipeUri);
            int lastWin32Error1 = Marshal.GetLastWin32Error();
            fileMapping1.SetHandleAsInvalid();
            if (lastWin32Error1 != 2)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameCannotBeAccessedException(lastWin32Error1, pipeUri));
            SafeFileMappingHandle fileMapping2 = UnsafeNativeMethods.OpenFileMapping(4, false, "Global\\" + sharedMemoryName);
            if (!fileMapping2.IsInvalid)
                return new PipeSharedMemory(fileMapping2, pipeUri);
            int lastWin32Error2 = Marshal.GetLastWin32Error();
            fileMapping2.SetHandleAsInvalid();
            if (lastWin32Error2 == 2)
                return (PipeSharedMemory)null;
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameCannotBeAccessedException(lastWin32Error2, pipeUri));
        }

        public void Dispose()
        {
            if (this.fileMapping == null)
                return;
            this.fileMapping.Close();
            this.fileMapping = (SafeFileMappingHandle)null;
        }

        public unsafe string PipeName
        {
            [SecuritySafeCritical, PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                if (this.pipeName == null)
                {
                    SafeViewOfFileHandle view = this.GetView(false);
                    try
                    {
                        PipeSharedMemory.SharedMemoryContents* handle = (PipeSharedMemory.SharedMemoryContents*)(void*)view.DangerousGetHandle();
                        if (handle->isInitialized)
                        {
                            Thread.MemoryBarrier();
                            this.pipeNameGuidPart = handle->pipeGuid.ToString();
                            this.pipeName = PipeSharedMemory.BuildPipeName(this.pipeNameGuidPart);
                        }
                    }
                    finally
                    {
                        view.Close();
                    }
                }
                return this.pipeName;
            }
        }

        internal string GetPipeName(AppContainerInfo appInfo)
        {
            if (appInfo == null)
                return this.PipeName;
            if (this.PipeName == null)
                return (string)null;
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "\\\\.\\pipe\\Sessions\\{0}\\{1}\\{2}", new object[3]
            {
                (object) appInfo.SessionId,
                (object) appInfo.NamedObjectPath,
                (object) this.pipeNameGuidPart
            });
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        private unsafe void InitializeContents(Guid pipeGuid)
        {
            SafeViewOfFileHandle view = this.GetView(true);
            try
            {
                PipeSharedMemory.SharedMemoryContents* handle = (PipeSharedMemory.SharedMemoryContents*)(void*)view.DangerousGetHandle();
                handle->pipeGuid = pipeGuid;
                Thread.MemoryBarrier();
                handle->isInitialized = true;
            }
            finally
            {
                view.Close();
            }
        }

        public static Exception CreatePipeNameInUseException(int error, Uri pipeUri)
        {
            Exception innerException = (Exception)new PipeException(SR.Format("PipeNameInUse", (object)pipeUri.AbsoluteUri), error);
            return (Exception)new AddressAlreadyInUseException(innerException.Message, innerException);
        }

        private static Exception CreatePipeNameCannotBeAccessedException(
            int error,
            Uri pipeUri)
        {
            Exception innerException = (Exception)new PipeException(SR.Format("PipeNameCanNotBeAccessed", (object)PipeError.GetErrorString(error)), error);
            return (Exception)new AddressAccessDeniedException(SR.Format("PipeNameCanNotBeAccessed2", (object)pipeUri.AbsoluteUri), innerException);
        }

        private unsafe SafeViewOfFileHandle GetView(bool writable)
        {
            SafeViewOfFileHandle viewOfFileHandle = UnsafeNativeMethods.MapViewOfFile(this.fileMapping, writable ? 2 : 4, 0, 0, (IntPtr)sizeof(SharedMemoryContents));
            if (viewOfFileHandle.IsInvalid)
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                viewOfFileHandle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(PipeSharedMemory.CreatePipeNameCannotBeAccessedException(lastWin32Error, this.pipeUri));
            }
            return viewOfFileHandle;
        }

        private static string BuildPipeName(string pipeGuid)
        {
            return (AppContainerInfo.IsRunningInAppContainer ? "\\\\.\\pipe\\Local\\" : "\\\\.\\pipe\\") + pipeGuid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SharedMemoryContents
        {
            public bool isInitialized;
            public Guid pipeGuid;
        }
    }
}

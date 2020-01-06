using System.ComponentModel;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Channels
{
    [SuppressUnmanagedCodeSecurity]
    internal class PipeHandle : SafeHandleMinusOneIsInvalid
    {
        internal PipeHandle()
            : base(true)
        {
        }

        internal PipeHandle(IntPtr handle)
            : base(true)
        {
            this.SetHandle(handle);
        }

        internal int GetClientPid()
        {
            int id;
            if (!UnsafeNativeMethods.GetNamedPipeClientProcessId(this, out id))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception)new Win32Exception());
            return id;
        }

        protected override bool ReleaseHandle()
        {
            return (uint)UnsafeNativeMethods.CloseHandle(this.handle) > 0U;
        }
    }
}

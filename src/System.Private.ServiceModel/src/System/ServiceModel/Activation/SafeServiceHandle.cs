using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Activation
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeServiceHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return ListenerUnsafeNativeMethods.CloseServiceHandle(this.handle);
        }
    }
}

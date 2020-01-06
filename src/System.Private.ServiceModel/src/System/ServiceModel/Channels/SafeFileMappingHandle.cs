using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Channels
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return (uint)UnsafeNativeMethods.CloseHandle(this.handle) > 0U;
        }
    }
}

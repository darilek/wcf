using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.ServiceModel.Channels
{
    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeViewOfFileHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (UnsafeNativeMethods.UnmapViewOfFile(this.handle) == 0)
                return false;
            this.handle = IntPtr.Zero;
            return true;
        }
    }
}

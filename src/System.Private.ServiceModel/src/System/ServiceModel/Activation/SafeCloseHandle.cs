// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Activation.SafeCloseHandle
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace System.ServiceModel.Activation
{
    internal sealed class SafeCloseHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string KERNEL32 = "kernel32.dll";

        private SafeCloseHandle()
            : base(true)
        {
        }

        internal SafeCloseHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            this.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return SafeCloseHandle.CloseHandle(this.handle);
        }

        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}

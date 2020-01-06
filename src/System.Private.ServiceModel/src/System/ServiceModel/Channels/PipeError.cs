using System.Globalization;
using System.Text;

namespace System.ServiceModel.Channels
{
    internal static class PipeError
    {
        public static string GetErrorString(int error)
        {
            StringBuilder lpBuffer = new StringBuilder(512);
            return UnsafeNativeMethods.FormatMessage(12800, IntPtr.Zero, error, CultureInfo.CurrentCulture.LCID, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0 
                ? SR.Format("PipeKnownWin32Error", (object)lpBuffer.Replace("\n", "").Replace("\r", "").ToString(), (object)error.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)Convert.ToString(error, 16)) 
                : SR.Format("PipeUnknownWin32Error", (object)error.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)Convert.ToString(error, 16));
        }
    }
}

namespace System.ServiceModel
{
    internal static class NetNamedPipeSecurityModeHelper
    {
        internal static bool IsDefined(NetNamedPipeSecurityMode value)
        {
            return value == NetNamedPipeSecurityMode.Transport || value == NetNamedPipeSecurityMode.None;
        }
    }
}

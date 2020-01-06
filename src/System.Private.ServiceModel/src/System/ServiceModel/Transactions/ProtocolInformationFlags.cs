namespace Microsoft.Transactions.Wsat.Protocol
{
    internal enum ProtocolInformationFlags : byte
    {
        IssuedTokensEnabled = 1,
        NetworkClientAccess = 2,
        NetworkInboundAccess = 4,
        NetworkOutboundAccess = 8,
        IsClustered = 16, // 0x10
    }
}
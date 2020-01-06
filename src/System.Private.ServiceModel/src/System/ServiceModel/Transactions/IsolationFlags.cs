namespace System.ServiceModel.Transactions
{
    [Flags]
    internal enum IsolationFlags
    {
        RetainCommitDC = 1,
        RetainCommit = 2,
        RetainCommitNo = RetainCommit | RetainCommitDC, // 0x00000003
        RetainAbortDC = 4,
        RetainAbort = 8,
        RetainAbortNo = RetainAbort | RetainAbortDC, // 0x0000000C
        RetainDoNotCare = RetainAbortDC | RetainCommitDC, // 0x00000005
        RetainBoth = RetainAbort | RetainCommit, // 0x0000000A
        RetainNone = RetainBoth | RetainDoNotCare, // 0x0000000F
        Optimistic = 16, // 0x00000010
        ReadOnly = 32, // 0x00000020
    }
}

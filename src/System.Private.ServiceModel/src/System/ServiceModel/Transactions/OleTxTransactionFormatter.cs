using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Transactions;

namespace System.ServiceModel.Transactions
{
    internal class OleTxTransactionFormatter : TransactionFormatter
    {
        private static OleTxTransactionHeader emptyTransactionHeader = new OleTxTransactionHeader((byte[])null, (WsatExtendedInformation)null);

        public override MessageHeader EmptyTransactionHeader
        {
            get
            {
                return (MessageHeader)OleTxTransactionFormatter.emptyTransactionHeader;
            }
        }

        public override void WriteTransaction(Transaction transaction, Message message)
        {
            byte[] propagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
            WsatExtendedInformation wsatInfo;
            if (!TransactionCache<Transaction, WsatExtendedInformation>.Find(transaction, out wsatInfo))
            {
                uint timeoutFromTransaction = OleTxTransactionFormatter.GetTimeoutFromTransaction(transaction);
                wsatInfo = timeoutFromTransaction != 0U ? new WsatExtendedInformation((string)null, timeoutFromTransaction) : (WsatExtendedInformation)null;
            }
            OleTxTransactionHeader transactionHeader = new OleTxTransactionHeader(propagationToken, wsatInfo);
            message.Headers.Add((MessageHeader)transactionHeader);
        }

        public override TransactionInfo ReadTransaction(Message message)
        {
            OleTxTransactionHeader header = OleTxTransactionHeader.ReadFrom(message);
            return header == null ? (TransactionInfo)null : (TransactionInfo)new OleTxTransactionInfo(header);
        }

        public static uint GetTimeoutFromTransaction(Transaction transaction)
        {
            OleTxTransactionFormatter.XACTOPT pOptions;
            ((OleTxTransactionFormatter.ITransactionOptions)TransactionInterop.GetDtcTransaction(transaction)).GetOptions(out pOptions);
            return pOptions.ulTimeout;
        }

        public static void GetTransactionAttributes(
            Transaction transaction,
            out uint timeout,
            out IsolationFlags isoFlags,
            out string description)
        {
            IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(transaction);
            OleTxTransactionFormatter.ITransactionOptions transactionOptions = (OleTxTransactionFormatter.ITransactionOptions)dtcTransaction;
            OleTxTransactionFormatter.ISaneDtcTransaction saneDtcTransaction = (OleTxTransactionFormatter.ISaneDtcTransaction)dtcTransaction;
            OleTxTransactionFormatter.XACTOPT pOptions;
            transactionOptions.GetOptions(out pOptions);
            timeout = pOptions.ulTimeout;
            description = pOptions.szDescription;
            OleTxTransactionFormatter.XACTTRANSINFO transactionInformation;
            saneDtcTransaction.GetTransactionInfo(out transactionInformation);
            isoFlags = transactionInformation.isoFlags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct XACTOPT
        {
            public uint ulTimeout;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct XACTTRANSINFO
        {
            public Guid uow;
            public IsolationLevel isoLevel;
            public IsolationFlags isoFlags;
            public uint grfTCSupported;
            public uint grfRMSupported;
            public uint grfTCSupportedRetaining;
            public uint grfRMSupportedRetaining;
        }

        [Guid("3A6AD9E0-23B9-11cf-AD60-00AA00A74CCD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface ITransactionOptions
        {
            void SetOptions([In] ref OleTxTransactionFormatter.XACTOPT pOptions);

            void GetOptions(out OleTxTransactionFormatter.XACTOPT pOptions);
        }

        [Guid("0fb15084-af41-11ce-bd2b-204c4f4f5020")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComImport]
        private interface ISaneDtcTransaction
        {
            void Abort(IntPtr reason, int retaining, int async);

            void Commit(int retaining, int commitType, int reserved);

            void GetTransactionInfo(
                out OleTxTransactionFormatter.XACTTRANSINFO transactionInformation);
        }
    }
}

using System.Transactions;

namespace System.ServiceModel.Transactions
{
    internal class WsatExtendedInformation
    {
        private string identifier;
        private uint timeout;
        public const string UuidScheme = "urn:uuid:";

        public WsatExtendedInformation(string identifier, uint timeout)
        {
            this.identifier = identifier;
            this.timeout = timeout;
        }

        public string Identifier
        {
            get
            {
                return this.identifier;
            }
        }

        public uint Timeout
        {
            get
            {
                return this.timeout;
            }
        }

        public void TryCache(Transaction tx)
        {
            string identifier = WsatExtendedInformation.IsNativeIdentifier(this.identifier, tx.TransactionInformation.DistributedIdentifier) ? (string)null : this.identifier;
            if (string.IsNullOrEmpty(identifier) && this.timeout == 0U)
                return;
            WsatExtendedInformationCache.Cache(tx, new WsatExtendedInformation(identifier, this.timeout));
        }

        public static string CreateNativeIdentifier(Guid transactionId)
        {
            return "urn:uuid:" + transactionId.ToString("D");
        }

        public static bool IsNativeIdentifier(string identifier, Guid transactionId)
        {
            return string.Compare(identifier, WsatExtendedInformation.CreateNativeIdentifier(transactionId), StringComparison.Ordinal) == 0;
        }
    }
}

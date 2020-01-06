// Decompiled with JetBrains decompiler
// Type: System.ServiceModel.Transactions.TransactionFormatter
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.ServiceModel.Channels;
using System.Transactions;

namespace System.ServiceModel.Transactions
{
    internal abstract class TransactionFormatter
    {
        private static TransactionFormatter oleTxFormatter = (TransactionFormatter)new OleTxTransactionFormatter();
        private static object syncRoot = new object();
        private static volatile TransactionFormatter wsatFormatter10;
        private static volatile TransactionFormatter wsatFormatter11;

        public static TransactionFormatter OleTxFormatter
        {
            get
            {
                return TransactionFormatter.oleTxFormatter;
            }
        }

        public static TransactionFormatter WsatFormatter10
        {
            get
            {
                if (TransactionFormatter.wsatFormatter10 == null)
                {
                    lock (TransactionFormatter.syncRoot)
                    {
                        if (TransactionFormatter.wsatFormatter10 == null)
                            TransactionFormatter.wsatFormatter10 = (TransactionFormatter)new WsatTransactionFormatter10();
                    }
                }
                return TransactionFormatter.wsatFormatter10;
            }
        }

        public static TransactionFormatter WsatFormatter11
        {
            get
            {
                if (TransactionFormatter.wsatFormatter11 == null)
                {
                    lock (TransactionFormatter.syncRoot)
                    {
                        if (TransactionFormatter.wsatFormatter11 == null)
                            TransactionFormatter.wsatFormatter11 = (TransactionFormatter)new WsatTransactionFormatter11();
                    }
                }
                return TransactionFormatter.wsatFormatter11;
            }
        }

        public abstract MessageHeader EmptyTransactionHeader { get; }

        public abstract void WriteTransaction(Transaction transaction, Message message);

        public abstract TransactionInfo ReadTransaction(Message message);
    }
}


using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
    internal interface ITransactionChannel
    {
        void WriteTransactionDataToMessage(Message message, MessageDirection direction);

        void ReadTransactionDataFromMessage(Message message, MessageDirection direction);

        void ReadIssuedTokens(Message message, MessageDirection direction);

        void WriteIssuedTokens(Message message, MessageDirection direction);
    }
}

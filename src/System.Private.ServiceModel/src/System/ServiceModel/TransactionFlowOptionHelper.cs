namespace System.ServiceModel
{
    internal static class TransactionFlowOptionHelper
    {
        public static bool IsDefined(TransactionFlowOption option)
        {
            return option == TransactionFlowOption.NotAllowed || option == TransactionFlowOption.Allowed || option == TransactionFlowOption.Mandatory;
        }

        internal static bool AllowedOrRequired(TransactionFlowOption option)
        {
            return option == TransactionFlowOption.Allowed || option == TransactionFlowOption.Mandatory;
        }
    }
}

using System;
using System.ServiceModel;

namespace Microsoft.Transactions.Wsat.Messaging
{
    internal class InvalidCoordinationContextException : CommunicationException
    {
        public InvalidCoordinationContextException(string message) : base(message)
        {
        }

        public InvalidCoordinationContextException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
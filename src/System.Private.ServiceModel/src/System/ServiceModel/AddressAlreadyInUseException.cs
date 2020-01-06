using System.Runtime.Serialization;

namespace System.ServiceModel
{
    /// <summary>The exception that is thrown when an address is unavailable because it is already in use.</summary>
    [Serializable]
    public class AddressAlreadyInUseException : CommunicationException
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAlreadyInUseException" /> class.  </summary>
        public AddressAlreadyInUseException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAlreadyInUseException" /> class with a specified error message.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AddressAlreadyInUseException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAlreadyInUseException" /> class with a specified error message and a reference to the inner exception that is the cause of the exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The <see cref="T:System.Exception" /> that caused the current exception to be thrown. </param>
        public AddressAlreadyInUseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceModel.AddressAlreadyInUseException" /> class with serialization information and streaming context specified.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains all the data required to serialize the exception.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the source and destination of the stream.</param>
        protected AddressAlreadyInUseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

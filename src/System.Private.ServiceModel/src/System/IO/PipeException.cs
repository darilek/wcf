// Decompiled with JetBrains decompiler
// Type: System.IO.PipeException
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: DFA5A02E-DC20-4F5C-BC91-9F625E2A95D3
// Assembly location: C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel\v4.0_4.0.0.0__b77a5c561934e089\System.ServiceModel.dll

using System.Runtime.Serialization;

namespace System.IO
{
    /// <summary>Thrown when an error occurs within a named pipe.</summary>
    [Serializable]
    public class PipeException : IOException
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.IO.PipeException" /> class. </summary>
        public PipeException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.IO.PipeException" /> class with a specified error message. </summary>
        /// <param name="message">A string that contains the error message that explains the reason for the exception.</param>
        public PipeException(string message)
          : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.IO.PipeException" /> class with a specified error message and error code. </summary>
        /// <param name="message">A string that contains the error message that explains the reason for the exception.</param>
        /// <param name="errorCode">An integer that contains the error code.</param>
        public PipeException(string message, int errorCode)
          : base(message, errorCode)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.IO.PipeException" /> class with the specified error message and the inner exception.  </summary>
        /// <param name="message">A string that contains the error message that explains the reason for the exception.</param>
        /// <param name="inner">The <see cref="T:System.Exception" /> that caused the current exception to be thrown. </param>
        public PipeException(string message, Exception inner)
          : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.IO.PipeException" /> class with the specified serialization information and streaming context.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that contains all the data required to serialize the exception.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that specifies the source and destination of the stream.</param>
        protected PipeException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        /// <summary>Gets the error code to be returned with the exception. </summary>
        /// <returns>An integer with the error code to be returned with the exception. </returns>
        public virtual int ErrorCode
        {
            get
            {
                return this.HResult;
            }
        }
    }
}

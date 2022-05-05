using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MiScaleBodyComposition.Exceptions
{
    public class DataLengthException : Exception, IMiScaleBodyCompositionException
    {
        public DataLengthException()
        {

        }

        public DataLengthException(string message) : base(message)
        {
        }

        public DataLengthException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataLengthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

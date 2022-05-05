using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MiScaleBodyComposition.Exceptions
{
    public class NoImpedanceException : Exception
    {
        public NoImpedanceException()
        {
        }

        public NoImpedanceException(string message) : base(message)
        {
        }

        public NoImpedanceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoImpedanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MiScaleBodyComposition.Exceptions
{
    public class NotStabilizedException : Exception, IMiScaleBodyCompositionException
    {
        public NotStabilizedException()
        {
        }

        public NotStabilizedException(string message) : base(message)
        {
        }

        public NotStabilizedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotStabilizedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

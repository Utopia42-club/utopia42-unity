using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Source.UtopiaException
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException()
        {
        }

        protected IllegalStateException([NotNull] SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        public IllegalStateException(string message) : base(message)
        {
        }

        public IllegalStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
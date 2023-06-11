using System.Runtime.Serialization;

namespace SmatHome.Connector
{
    [Serializable]
    internal class SignalRException : Exception
    {
        public SignalRException()
        {
        }

        public SignalRException(string? message) : base(message)
        {
        }

        public SignalRException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SignalRException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
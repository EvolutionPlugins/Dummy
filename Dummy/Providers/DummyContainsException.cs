using System;
using System.Runtime.Serialization;

namespace EvolutionPlugins.Dummy.Providers
{
    [Serializable]
    public class DummyContainsException : Exception
    {
        public ulong Id { get; }

        public DummyContainsException(ulong id)
        {
            Id = id;
        }

        public DummyContainsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DummyContainsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DummyContainsException()
        {
        }

        public DummyContainsException(string message) : base(message)
        {
        }
    }
}
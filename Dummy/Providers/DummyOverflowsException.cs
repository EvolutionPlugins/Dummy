using System;
using System.Runtime.Serialization;

namespace EvolutionPlugins.Dummy.Providers
{
    [Serializable]
    public class DummyOverflowsException : Exception
    {
        public byte DummiesCount;
        public byte MaxDummies;

        public DummyOverflowsException()
        {
        }

        public DummyOverflowsException(string message) : base(message)
        {
        }

        public DummyOverflowsException(byte count, byte amountDummiesConfig)
        {
            DummiesCount = count;
            MaxDummies = amountDummiesConfig;
        }

        public DummyOverflowsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DummyOverflowsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.Builders;

namespace Dafda.Tests.Producing
{
    internal static class A
    {
        public static ProducerBuilder Producer => new ProducerBuilder();
        public static OutgoingMessageRegistryBuilder OutgoingMessageRegistry => new OutgoingMessageRegistryBuilder();
        public static ProducerOptions ValidProducerConfiguration
        {
            get
            {
                var options = new ProducerOptions(new OutgoingMessageRegistry());
                options.WithBootstrapServers("dummy");
                return options;
            }
        }
    }
}
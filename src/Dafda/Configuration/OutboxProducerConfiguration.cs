using System;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class OutboxProducerConfiguration
    {
        public OutboxProducerConfiguration(Func<IServiceProvider, KafkaProducer> kafkaProducerFactory, Pipeline pipeline)
        {
            KafkaProducerFactory = kafkaProducerFactory;
            Pipeline = pipeline;
        }

        public Func<IServiceProvider, KafkaProducer> KafkaProducerFactory { get; }

        public Pipeline Pipeline { get; }
    }
}
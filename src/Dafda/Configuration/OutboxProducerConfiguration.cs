using System;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class OutboxProducerConfiguration
    {
        public OutboxProducerConfiguration(MiddlewareBuilder<OutgoingRawMessageContext> middlewareBuilder, Func<IServiceProvider, KafkaProducer> kafkaProducerFactory)
        {
            MiddlewareBuilder = middlewareBuilder;
            KafkaProducerFactory = kafkaProducerFactory;
        }

        public MiddlewareBuilder<OutgoingRawMessageContext> MiddlewareBuilder { get; }
        public Func<IServiceProvider, KafkaProducer> KafkaProducerFactory { get; }
    }
}
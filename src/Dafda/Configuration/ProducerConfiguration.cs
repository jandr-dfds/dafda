using System;
using System.Collections.Generic;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class ProducerConfiguration
    {
        public ProducerConfiguration(IDictionary<string, string> configuration,
            MessageIdGenerator messageIdGenerator,
            Func<IServiceProvider, KafkaProducer> kafkaProducerFactory,
            OutgoingMessageRegistry outgoingMessageRegistry,
            MiddlewareBuilder<OutgoingMessageContext> middlewareBuilder)
        {
            KafkaConfiguration = configuration;
            MessageIdGenerator = messageIdGenerator;
            KafkaProducerFactory = kafkaProducerFactory;
            OutgoingMessageRegistry = outgoingMessageRegistry;
            MiddlewareBuilder = middlewareBuilder;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageIdGenerator MessageIdGenerator { get; }
        public Func<IServiceProvider, KafkaProducer> KafkaProducerFactory { get; }
        public OutgoingMessageRegistry OutgoingMessageRegistry { get; }
        public MiddlewareBuilder<OutgoingMessageContext> MiddlewareBuilder { get; }
    }
}
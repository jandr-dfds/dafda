using System;
using System.Collections.Generic;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Configuration
{
    internal class ProducerConfiguration
    {
        public ProducerConfiguration(
            IDictionary<string, string> configuration,
            MessageIdGenerator messageIdGenerator,
            Func<IServiceProvider, KafkaProducer> kafkaProducerFactory, 
            Pipeline pipeline)
        {
            KafkaConfiguration = configuration;
            MessageIdGenerator = messageIdGenerator;
            KafkaProducerFactory = kafkaProducerFactory;
            Pipeline = pipeline;
        }

        public IDictionary<string, string> KafkaConfiguration { get; }
        public MessageIdGenerator MessageIdGenerator { get; }
        public Func<IServiceProvider, KafkaProducer> KafkaProducerFactory { get; }
        public Pipeline Pipeline { get; }
    }
}
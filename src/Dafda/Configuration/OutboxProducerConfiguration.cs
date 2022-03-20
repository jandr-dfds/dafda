using System;
using Dafda.Producing;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    internal class OutboxProducerConfiguration
    {
        public OutboxProducerConfiguration(Func<ILoggerFactory, KafkaProducer> kafkaProducerFactory)
        {
            KafkaProducerFactory = kafkaProducerFactory;
        }

        public Func<ILoggerFactory, KafkaProducer> KafkaProducerFactory { get; }
    }
}
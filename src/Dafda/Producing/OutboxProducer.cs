using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Outbox;

namespace Dafda.Producing
{
    /// <summary>
    /// A specific producer for the Dafda outbox
    /// </summary>
    internal sealed class OutboxProducer
    {
        private readonly Pipeline _pipeline;
        private readonly IServiceProvider _serviceProvider;
        private readonly KafkaProducer _kafkaProducer;

        internal OutboxProducer(Pipeline pipeline, IServiceProvider serviceProvider, KafkaProducer kafkaProducer)
        {
            _pipeline = pipeline;
            _serviceProvider = serviceProvider;
            _kafkaProducer = kafkaProducer;
        }

        /// <summary>
        /// Produce the <see cref="OutboxEntry"/> on Kafka using the data contained
        /// </summary>
        /// <param name="entry">The outbox message</param>
        public async Task Produce(OutboxEntry entry)
        {
            var rootMiddlewareContext = new RootProducerMiddlewareContext(_serviceProvider, _kafkaProducer);
            await _pipeline.Invoke(new OutgoingRawMessageContext(new OutgoingRawMessage(entry.Topic, entry.Key, entry.Payload), rootMiddlewareContext));
        }
    }
}
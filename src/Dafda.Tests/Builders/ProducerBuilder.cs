using System.Linq;
using Dafda.Middleware;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Builders
{
    internal class ProducerBuilder
    {
        private KafkaProducer _kafkaProducer;
        private OutgoingMessageRegistry _outgoingMessageRegistry = new();
        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;
        private IPayloadSerializer _payloadSerializer = new DefaultPayloadSerializer();

        public ProducerBuilder With(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            return this;
        }

        public ProducerBuilder With(OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public ProducerBuilder With(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
            return this;
        }

        public ProducerBuilder With(IPayloadSerializer payloadSerializer)
        {
            _payloadSerializer = payloadSerializer;
            return this;
        }

        public Producer Build()
        {
            var serviceCollection = new ServiceCollection();
            var middlewareBuilder = new MiddlewareBuilder<OutgoingMessageContext>();
            middlewareBuilder
                .Register(new PayloadDescriptionMiddleware(_outgoingMessageRegistry, _messageIdGenerator))
                .Register(new SerializationMiddleware(new TopicPayloadSerializerRegistry(() => _payloadSerializer)))
                .Register(new DispatchMiddleware());

            var provider = serviceCollection.BuildServiceProvider();
            var middlewares = middlewareBuilder
                .Build();

            var pipeline = new Pipeline(middlewares);

            return new Producer(pipeline, provider, _kafkaProducer);
        }

        public static implicit operator Producer(ProducerBuilder builder)
        {
            return builder.Build();
        }
    }
}
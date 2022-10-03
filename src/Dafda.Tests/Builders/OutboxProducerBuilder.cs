using System.Linq;
using Dafda.Middleware;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Builders
{
    internal class OutboxProducerBuilder
    {
        private KafkaProducer _kafkaProducer;

        public OutboxProducerBuilder With(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            return this;
        }

        public OutboxProducer Build()
        {
            var serviceCollection = new ServiceCollection();
            var middlewareBuilder = new MiddlewareBuilder<OutgoingRawMessageContext>();
            middlewareBuilder.Register(new DispatchMiddleware());

            var provider = serviceCollection.BuildServiceProvider();
            var middlewares = middlewareBuilder
                .Build();

            var pipeline = new Pipeline(middlewares);

            return new OutboxProducer(pipeline, provider, _kafkaProducer);
        }

        public static implicit operator OutboxProducer(OutboxProducerBuilder builder)
        {
            return builder.Build();
        }
    }
}
using System;
using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration
    {
        public ConsumerConfiguration(
            Configuration configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory,
            MiddlewareBuilder<IncomingRawMessageContext> middlewareBuilder,
            ConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            MiddlewareBuilder = middlewareBuilder;
            ConsumerErrorHandler = consumerErrorHandler;
        }

        public Configuration KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; }
        public MiddlewareBuilder<IncomingRawMessageContext> MiddlewareBuilder { get; }
        public ConsumerErrorHandler ConsumerErrorHandler { get; }

        public string GroupId => KafkaConfiguration.GroupId;
        public bool EnableAutoCommit => KafkaConfiguration.EnableAutoCommit;
    }
}
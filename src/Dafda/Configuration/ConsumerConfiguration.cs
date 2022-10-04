using System;
using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration
    {
        public ConsumerConfiguration(Configuration configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            Func<IServiceProvider, ConsumerScope> consumerScopeFactory,
            Pipeline pipeline,
            ConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            ConsumerErrorHandler = consumerErrorHandler;
            Pipeline = pipeline;
        }

        public Configuration KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public Func<IServiceProvider, ConsumerScope> ConsumerScopeFactory { get; }
        public Pipeline Pipeline { get; }
        public ConsumerErrorHandler ConsumerErrorHandler { get; }
        public string GroupId => KafkaConfiguration.GroupId;
        public bool EnableAutoCommit => KafkaConfiguration.EnableAutoCommit;
    }
}
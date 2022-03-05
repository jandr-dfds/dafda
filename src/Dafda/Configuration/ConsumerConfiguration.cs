using System;
using Dafda.Consuming;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration
    {
        public ConsumerConfiguration(
            Configuration configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory,
            Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory,
            MessageFilter messageFilter,
            ConsumerErrorHandler consumerErrorHandler)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            IncomingMessageFactory = incomingMessageFactory;
            MessageFilter = messageFilter;
            ConsumerErrorHandler = consumerErrorHandler;
        }

        public Configuration KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; }
        public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; }
        public MessageFilter MessageFilter { get; }
        public ConsumerErrorHandler ConsumerErrorHandler { get; }

        public string GroupId => KafkaConfiguration.GroupId;
        public bool EnableAutoCommit => KafkaConfiguration.EnableAutoCommit;
    }
}
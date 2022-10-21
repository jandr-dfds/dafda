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
            EvaluateError evaluateError)
        {
            KafkaConfiguration = configuration;
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            EvaluateError = evaluateError;
            Pipeline = pipeline;
        }

        public Configuration KafkaConfiguration { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public Func<IServiceProvider, ConsumerScope> ConsumerScopeFactory { get; }
        public Pipeline Pipeline { get; }
        public EvaluateError EvaluateError { get; }
        public string GroupId => KafkaConfiguration.GroupId;
        public bool EnableAutoCommit => KafkaConfiguration.EnableAutoCommit;
    }
}
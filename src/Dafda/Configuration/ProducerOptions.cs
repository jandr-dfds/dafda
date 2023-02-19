using System;
using System.Collections.Generic;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary>
    /// Facilitates Dafda configuration in .NET applications using the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
    /// </summary>
    public sealed class ProducerOptions
    {
        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly NamingConventions _namingConventions = new();
        private readonly OutgoingMessageRegistry _outgoingMessageRegistry = new();
        private readonly TopicPayloadSerializerRegistry _topicPayloadSerializerRegistry = new(() => new DefaultPayloadSerializer());

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;
        private Func<ILoggerFactory, KafkaProducer> _kafkaProducerFactory;

        internal ProducerOptions()
        {
        }

        /// <summary>
        /// Specify a custom implementation of the <see cref="ConfigurationSource"/> to use. 
        /// </summary>
        /// <param name="configurationSource">The <see cref="ConfigurationSource"/> to use.</param>
        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
        }

        /// <summary>
        /// Use <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> as the configuration source.
        /// </summary>
        /// <param name="configuration">The configuration instance.</param>
        public void WithConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            WithConfigurationSource(new DefaultConfigurationSource(configuration));
        }

        /// <summary>
        /// Add a custom naming convention for converting configuration keys when
        /// looking up keys in the <see cref="ConfigurationSource"/>.
        /// </summary>
        /// <param name="converter">Use this to transform keys.</param>
        public void WithNamingConvention(Func<string, string> converter)
        {
            _namingConventions.Add(NamingConvention.UseCustom(converter));
        }

        internal void WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
        }

        /// <summary>
        /// Add default environment style naming convention. The configuration will attempt to
        /// fetch keys from <see cref="ConfigurationSource"/>, using the following scheme:
        /// <list type="bullet">
        ///     <item><description>keys will be converted to uppercase.</description></item>
        ///     <item><description>any one or more of <c>SPACE</c>, <c>TAB</c>, <c>.</c>, and <c>-</c> will be converted to a single <c>_</c>.</description></item>
        ///     <item><description>the prefix will be prefixed (in uppercase) along with a <c>_</c>.</description></item>
        /// </list>
        /// 
        /// When configuring a consumer the <c>WithEnvironmentStyle("app")</c>, Dafda will attempt to find the
        /// key <c>APP_GROUP_ID</c> in the <see cref="ConfigurationSource"/>.
        /// </summary>
        /// <param name="prefix">The prefix to use before keys.</param>
        /// <param name="additionalPrefixes">Additional prefixes to use before keys.</param>
        public void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }
        }

        /// <summary>
        /// Add a configuration key/value directly to the underlying Kafka consumer.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The configuration value.</param>
        public void WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
        }

        /// <summary>
        /// A shorthand to set the <c>bootstrap.servers</c> Kafka configuration value.
        /// </summary>
        /// <param name="bootstrapServers">A list of bootstrap servers.</param>
        public void WithBootstrapServers(string bootstrapServers)
        {
            WithConfiguration(ConfigurationKeys.BootstrapServers, bootstrapServers);
        }

        internal void WithKafkaProducerFactory(Func<ILoggerFactory, KafkaProducer> inlineFactory)
        {
            _kafkaProducerFactory = inlineFactory;
        }

        /// <summary>
        /// Override the default Dafda implementation of <see cref="MessageIdGenerator"/>.
        /// </summary>
        /// <param name="messageIdGenerator">A custom implementation of <see cref="MessageIdGenerator"/>.</param>
        public void WithMessageIdGenerator(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
        }

        /// <summary>
        /// Register outbox messages.
        /// </summary>
        /// <typeparam name="T">The type of message going to the <see cref="Producer.Produce(object)"/> call.</typeparam>
        /// <param name="topic">The target topic.</param>
        /// <param name="type">The event type to use in the Dafda message envelope.</param>
        /// <param name="keySelector">The key selector takes an instance of <typeparamref name="T"/>,
        /// and returns a string of the Kafka partition key.</param>
        public void Register<T>(string topic, string type, Func<T, string> keySelector) where T : class
        {
            _outgoingMessageRegistry.Register(topic, type, keySelector);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation
        /// </summary>
        /// <param name="payloadSerializer">A custom implementation of <see cref="DefaultPayloadSerializer"/></param>
        public void WithDefaultPayloadSerializer(IPayloadSerializer payloadSerializer)
        {
            WithDefaultPayloadSerializer(() => payloadSerializer);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation
        /// </summary>
        /// <param name="payloadSerializerFactory">A factory method that returns a custom implementation
        /// of <see cref="DefaultPayloadSerializer"/>
        /// </param>
        public void WithDefaultPayloadSerializer(Func<IPayloadSerializer> payloadSerializerFactory)
        {
            _topicPayloadSerializerRegistry.SetDefaultPayloadSerializer(payloadSerializerFactory);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation for
        /// the specified <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">Name of the topic</param>
        /// <param name="payloadSerializer">A custom implementation of <see cref="DefaultPayloadSerializer"/></param>
        public void WithPayloadSerializer(string topic, IPayloadSerializer payloadSerializer)
        {
            WithPayloadSerializer(topic, () => payloadSerializer);
        }

        /// <summary>
        /// Override the <see cref="DefaultPayloadSerializer"/> with a custom implementation for
        /// the specified <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">Name of the topic</param>
        /// <param name="payloadSerializerFactory">A factory method that returns a custom implementation
        /// of <see cref="DefaultPayloadSerializer"/>
        /// </param>
        public void WithPayloadSerializer(string topic, Func<IPayloadSerializer> payloadSerializerFactory)
        {
            _topicPayloadSerializerRegistry.Register(topic, payloadSerializerFactory);
        }

        private class DefaultConfigurationSource : ConfigurationSource
        {
            private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

            public DefaultConfigurationSource(Microsoft.Extensions.Configuration.IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public override string GetByKey(string key)
            {
                return _configuration[key];
            }
        }

        internal ProducerConfiguration Build()
        {
            var configurations = ConfigurationBuilder
                .ForProducer
                .WithNamingConventions(_namingConventions)
                .WithConfigurationSource(_configurationSource)
                .WithConfigurations(_configurations)
                .Build();

            _kafkaProducerFactory ??= loggerFactory => new KafkaProducer(loggerFactory, configurations, _topicPayloadSerializerRegistry);

            return new ProducerConfiguration(
                configurations,
                _messageIdGenerator,
                _kafkaProducerFactory,
                _outgoingMessageRegistry
            );
        }
    }
}